using System.Collections;
using Brew.Core;
using Brew.Data;
using UnityEngine;

namespace Brew.Presentation
{
    /// <summary>
    /// Drives adaptive music from moves remaining: tension stems, heartbeat layer, and recovery ramps.
    /// </summary>
    public class AdaptiveAudioController : MonoBehaviour
    {
        public enum MoveAudioState
        {
            Normal = 0,
            LowMoves = 1,
            Critical = 2
        }

        const int LowMovesThreshold = 5;
        const float EscalateDuration = 2f;
        const float RecoverDuration = 4f;

        [SerializeField] private AudioManager _audioManager;

        private MoveTracker _moveTracker;
        private MoveAudioState _state = MoveAudioState.Normal;
        private Coroutine _transitionRoutine;

        private AudioManager Manager =>
            _audioManager != null ? _audioManager : AudioManager.Instance;

        public void Initialize(MoveTracker moveTracker)
        {
            if (_moveTracker != null)
                _moveTracker.OnMovesChanged -= OnMovesChanged;

            _moveTracker = moveTracker;
            _moveTracker.OnMovesChanged += OnMovesChanged;

            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
                _transitionRoutine = null;
            }

            _state = StateForMoves(_moveTracker.MovesRemaining);
            SnapToState(_state);
        }

        public void DuckForBrew()
        {
            Manager?.DuckMusic(-12f, 0.2f, 0.8f);
        }

        private void OnDestroy()
        {
            if (_moveTracker != null)
                _moveTracker.OnMovesChanged -= OnMovesChanged;
        }

        private static MoveAudioState StateForMoves(int movesRemaining)
        {
            if (movesRemaining <= 1)
                return MoveAudioState.Critical;
            if (movesRemaining <= LowMovesThreshold)
                return MoveAudioState.LowMoves;
            return MoveAudioState.Normal;
        }

        private void OnMovesChanged(int movesRemaining)
        {
            MoveAudioState next = StateForMoves(movesRemaining);
            if (next == _state)
                return;

            if (_transitionRoutine != null)
                StopCoroutine(_transitionRoutine);

            bool recovering = next < _state;
            float duration = recovering ? RecoverDuration : EscalateDuration;
            _state = next;
            _transitionRoutine = StartCoroutine(TransitionToStateCoroutine(next, duration));
        }

        private void SnapToState(MoveAudioState state)
        {
            AudioManager mgr = Manager;
            AudioConfigSO cfg = mgr != null ? mgr.Config : null;
            if (mgr == null || cfg == null)
                return;

            ApplyClipsForState(state, mgr, cfg);
            float[] targets = GetStemLevelTargets(state);
            for (int i = 0; i < AudioManager.MusicStemCount; i++)
                mgr.SetStemVolume(i, targets[i]);
        }

        private IEnumerator TransitionToStateCoroutine(MoveAudioState to, float duration)
        {
            AudioManager mgr = Manager;
            AudioConfigSO cfg = mgr != null ? mgr.Config : null;
            if (mgr == null || cfg == null)
            {
                _transitionRoutine = null;
                yield break;
            }

            if (to == MoveAudioState.Normal)
                ApplyClipsForState(MoveAudioState.Normal, mgr, cfg);
            else
                ApplyClipsForState(MoveAudioState.LowMoves, mgr, cfg);

            float[] startLevels = CaptureNormalizedStemLevels(mgr, cfg);
            float[] targetLevels = GetStemLevelTargets(to);

            duration = Mathf.Max(0.01f, duration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                for (int i = 0; i < AudioManager.MusicStemCount; i++)
                    mgr.SetStemVolume(i, Mathf.Lerp(startLevels[i], targetLevels[i], t));
                yield return null;
            }

            for (int i = 0; i < AudioManager.MusicStemCount; i++)
                mgr.SetStemVolume(i, targetLevels[i]);

            _transitionRoutine = null;
        }

        private static void ApplyClipsForState(MoveAudioState state, AudioManager mgr, AudioConfigSO cfg)
        {
            if (state == MoveAudioState.Normal)
            {
                mgr.SetStemClip(2, cfg.PercussionClip);
                mgr.SetStemClip(3, cfg.FxChimesClip);
            }
            else
            {
                AudioClip perc = cfg.PercussionTenseClip != null ? cfg.PercussionTenseClip : cfg.PercussionClip;
                AudioClip layer = cfg.HeartbeatClip != null ? cfg.HeartbeatClip : cfg.FxChimesClip;
                mgr.SetStemClip(2, perc);
                mgr.SetStemClip(3, layer);
            }
        }

        private static float[] GetStemLevelTargets(MoveAudioState state)
        {
            return state switch
            {
                MoveAudioState.Normal => new[] { 1f, 1f, 1f, 1f },
                MoveAudioState.LowMoves => new[] { 0.5f, 1f, 1f, 1f },
                MoveAudioState.Critical => new[] { 0f, 1.2f, 0f, 1f },
                _ => new[] { 1f, 1f, 1f, 1f }
            };
        }

        private static float[] CaptureNormalizedStemLevels(AudioManager mgr, AudioConfigSO cfg)
        {
            float g = cfg.MusicBaseVolume * AudioManager.MusicVolume;
            if (AudioManager.IsMuted)
                g = 0f;

            float[] levels = new float[4];
            if (g < 1e-5f)
            {
                for (int i = 0; i < AudioManager.MusicStemCount; i++)
                    levels[i] = 0f;
                return levels;
            }

            for (int i = 0; i < AudioManager.MusicStemCount; i++)
            {
                AudioSource src = mgr.GetMusicSource(i);
                levels[i] = src != null ? Mathf.Clamp(src.volume / g, 0f, 4f) : 0f;
            }

            return levels;
        }
    }
}
