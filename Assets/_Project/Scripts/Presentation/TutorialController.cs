using System;
using System.Collections;
using System.Collections.Generic;
using Brew.Core;
using UnityEngine;

namespace Brew.Presentation
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private TutorialOverlay _overlay;
        [SerializeField] private BoardPresenter _boardPresenter;

        private TutorialLevelDefinition _definition;
        private int _currentStepIndex;
        private bool _waitingForTap;
        private HashSet<GridCoord> _validTapCells;
        private Coroutine _hintCoroutine;
        private float _lastInputTime;

        private const float HintShimmerDelay = 5f;
        private const float HintPulseDelay = 10f;
        private const float HintFingerDelay = 15f;

        public bool IsActive { get; private set; }
        public bool IsTutorialLevel => _definition != null;

        public event Action<GridCoord[]> OnInputRestricted;
        public event Action OnInputUnrestricted;

        public void StartTutorial(int levelId)
        {
            _definition = TutorialLevelData.GetLevel(levelId);
            if (_definition == null)
            {
                IsActive = false;
                return;
            }

            IsActive = true;
            _currentStepIndex = 0;
            _waitingForTap = false;
            _validTapCells = null;

            StartCoroutine(RunTutorialSequence());
        }

        public void StopTutorial()
        {
            IsActive = false;
            _definition = null;
            StopAllCoroutines();

            if (_overlay != null)
                _overlay.HideAll();

            OnInputUnrestricted?.Invoke();
        }

        public bool IsValidTutorialTap(GridCoord coord)
        {
            if (!IsActive || !_waitingForTap) return true;
            if (_validTapCells == null || _validTapCells.Count == 0) return true;
            return _validTapCells.Contains(coord);
        }

        public void NotifyTapPerformed()
        {
            _lastInputTime = Time.time;

            if (_waitingForTap)
            {
                _waitingForTap = false;
            }
        }

        private IEnumerator RunTutorialSequence()
        {
            yield return new WaitForSeconds(0.5f);

            while (_currentStepIndex < _definition.Steps.Count)
            {
                var step = _definition.Steps[_currentStepIndex];

                switch (step.Type)
                {
                    case TutorialStepType.ShowText:
                        yield return RunShowTextStep(step);
                        break;

                    case TutorialStepType.HighlightAndWaitForTap:
                        yield return RunHighlightStep(step);
                        break;

                    case TutorialStepType.FreePlay:
                        yield return RunFreePlayStep(step);
                        break;

                    case TutorialStepType.LevelComplete:
                        yield return RunLevelCompleteStep(step);
                        break;
                }

                _currentStepIndex++;
            }

            StopTutorial();
        }

        private IEnumerator RunShowTextStep(TutorialStep step)
        {
            if (_overlay != null)
            {
                _overlay.ShowDim();
                _overlay.ShowText(step.Text);
            }

            OnInputRestricted?.Invoke(Array.Empty<GridCoord>());

            if (step.AutoDismissTime > 0)
            {
                float waited = 0f;
                while (waited < step.AutoDismissTime)
                {
                    if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
                        break;
                    waited += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                yield return WaitForAnyTap();
            }

            if (_overlay != null)
            {
                _overlay.HideText();
                _overlay.HideDim();
            }

            if (step.CelebrationOnComplete && step.CelebrationText != null)
            {
                if (_overlay != null)
                    _overlay.ShowCelebrationText(step.CelebrationText);
                yield return new WaitForSeconds(1f);
            }

            OnInputUnrestricted?.Invoke();
        }

        private IEnumerator RunHighlightStep(TutorialStep step)
        {
            if (_overlay != null)
            {
                _overlay.ShowDim();
                _overlay.HighlightCells(step.HighlightedCells);
                _overlay.ShowText(step.Text);

                if (step.FingerIndicatorCell.HasValue)
                    _overlay.ShowFingerIndicator(step.FingerIndicatorCell.Value);
            }

            _validTapCells = new HashSet<GridCoord>();
            foreach (var cell in step.HighlightedCells)
                _validTapCells.Add(cell);

            OnInputRestricted?.Invoke(step.HighlightedCells);

            _waitingForTap = true;
            while (_waitingForTap)
                yield return null;

            if (_overlay != null)
            {
                _overlay.ClearHighlights();
                _overlay.HideFingerIndicator();
                _overlay.HideText();
                _overlay.HideDim();
            }

            _validTapCells = null;
            OnInputUnrestricted?.Invoke();

            if (step.CelebrationOnComplete && step.CelebrationText != null)
            {
                if (_overlay != null)
                    _overlay.ShowCelebrationText(step.CelebrationText);
                yield return new WaitForSeconds(1f);
            }

            yield return new WaitForSeconds(0.3f);
        }

        private bool _freePlayLevelComplete;

        private IEnumerator RunFreePlayStep(TutorialStep step)
        {
            if (step.Text != null && _overlay != null)
            {
                _overlay.ShowText(step.Text);
                yield return new WaitForSeconds(2f);
                _overlay.HideText();
            }

            OnInputUnrestricted?.Invoke();
            _lastInputTime = Time.time;
            _freePlayLevelComplete = false;

            if (_boardPresenter != null)
                _boardPresenter.OnLevelOutcome += OnFreePlayLevelOutcome;

            _hintCoroutine = StartCoroutine(RunHintSystem());

            while (IsActive && !_freePlayLevelComplete)
                yield return null;

            if (_boardPresenter != null)
                _boardPresenter.OnLevelOutcome -= OnFreePlayLevelOutcome;

            if (_hintCoroutine != null)
            {
                StopCoroutine(_hintCoroutine);
                _hintCoroutine = null;
            }
        }

        private void OnFreePlayLevelOutcome(LevelOutcome outcome)
        {
            _freePlayLevelComplete = true;
        }

        private IEnumerator RunLevelCompleteStep(TutorialStep step)
        {
            if (_overlay != null && step.Text != null)
                _overlay.ShowCelebrationText(step.Text);

            yield return new WaitForSeconds(2f);

            if (_overlay != null)
                _overlay.HideAll();
        }

        private IEnumerator RunHintSystem()
        {
            while (true)
            {
                float idleTime = Time.time - _lastInputTime;

                if (idleTime > HintFingerDelay)
                {
                    // Tier 3: finger indicator on largest cluster would go here
                    // (requires access to ClusterDetector through BoardPresenter)
                }
                else if (idleTime > HintPulseDelay)
                {
                    // Tier 2: pulsing glow
                }
                else if (idleTime > HintShimmerDelay)
                {
                    // Tier 1: faint shimmer
                }

                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator WaitForAnyTap()
        {
            while (!Input.GetMouseButtonDown(0) && Input.touchCount == 0)
                yield return null;
        }
    }
}
