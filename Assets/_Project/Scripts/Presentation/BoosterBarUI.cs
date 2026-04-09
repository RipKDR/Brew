using System;
using Brew.Core;
using UnityEngine;

namespace Brew.Presentation
{
    /// <summary>
    /// Manages three booster slots: Shake and ExtraMoves fire immediately on tap;
    /// Catalyst arms until cancelled or the board applies it.
    /// </summary>
    public class BoosterBarUI : MonoBehaviour
    {
        [SerializeField] private BoosterSlotUI _shakeSlot;
        [SerializeField] private BoosterSlotUI _catalystSlot;
        [SerializeField] private BoosterSlotUI _extraMovesSlot;

        private BoosterManager _boosterManager;
        private BoardModel _board;
        private TokenSpawner _spawner;
        private ClusterDetector _detector;
        private MoveTracker _moveTracker;
        private int _minClusterSize = 3;

        private BoosterType? _armedBooster;

        public bool IsBoosterArmed => _armedBooster.HasValue;
        public BoosterType? ArmedBooster => _armedBooster;

        public event Action<BoosterType> OnBoosterArmed;
        public event Action OnBoosterCancelled;

        /// <summary>
        /// Wires the bar to gameplay systems. Shake needs board/spawner/detector; ExtraMoves needs <paramref name="moveTracker"/>.
        /// </summary>
        public void Initialize(
            BoosterManager manager,
            BoardModel board,
            TokenSpawner spawner,
            ClusterDetector detector,
            MoveTracker moveTracker,
            int minClusterSize = 3)
        {
            _boosterManager = manager ?? throw new ArgumentNullException(nameof(manager));
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
            _detector = detector ?? throw new ArgumentNullException(nameof(detector));
            _moveTracker = moveTracker ?? throw new ArgumentNullException(nameof(moveTracker));
            _minClusterSize = minClusterSize;

            _armedBooster = null;

            SetupSlot(_shakeSlot, BoosterType.Shake);
            SetupSlot(_catalystSlot, BoosterType.Catalyst);
            SetupSlot(_extraMovesSlot, BoosterType.ExtraMoves);

            _boosterManager.OnChargesChanged += HandleChargesChanged;
            RefreshAll();
        }

        private void SetupSlot(BoosterSlotUI slot, BoosterType type)
        {
            if (slot == null) return;
            slot.Setup(type, _boosterManager.GetCharges(type), OnSlotClicked);
        }

        private void OnSlotClicked(BoosterType type)
        {
            if (type == BoosterType.Shake)
            {
                if (_armedBooster == BoosterType.Catalyst)
                    OnBoosterCancelled?.Invoke();

                ClearArmedHighlights();
                _armedBooster = null;

                OnBoosterArmed?.Invoke(BoosterType.Shake);
                _boosterManager.ActivateShake(_board, _spawner, _detector, _minClusterSize);
                return;
            }

            if (type == BoosterType.ExtraMoves)
            {
                if (_armedBooster == BoosterType.Catalyst)
                    OnBoosterCancelled?.Invoke();

                ClearArmedHighlights();
                _armedBooster = null;

                if (_boosterManager.ActivateExtraMoves(_moveTracker, 5))
                    OnBoosterArmed?.Invoke(BoosterType.ExtraMoves);

                return;
            }

            if (_armedBooster.HasValue && _armedBooster.Value == type)
            {
                DisarmAll();
                return;
            }

            if (_armedBooster.HasValue)
            {
                OnBoosterCancelled?.Invoke();
                ClearArmedHighlights();
            }

            _armedBooster = type;
            _shakeSlot?.SetArmed(type == BoosterType.Shake);
            _catalystSlot?.SetArmed(type == BoosterType.Catalyst);
            _extraMovesSlot?.SetArmed(type == BoosterType.ExtraMoves);
            OnBoosterArmed?.Invoke(type);
        }

        public void DisarmAll()
        {
            if (!_armedBooster.HasValue)
            {
                ClearArmedHighlights();
                return;
            }

            _armedBooster = null;
            ClearArmedHighlights();
            OnBoosterCancelled?.Invoke();
        }

        public void RefreshAll()
        {
            _shakeSlot?.UpdateCharges(_boosterManager.GetCharges(BoosterType.Shake));
            _catalystSlot?.UpdateCharges(_boosterManager.GetCharges(BoosterType.Catalyst));
            _extraMovesSlot?.UpdateCharges(_boosterManager.GetCharges(BoosterType.ExtraMoves));
        }

        private void ClearArmedHighlights()
        {
            _shakeSlot?.SetArmed(false);
            _catalystSlot?.SetArmed(false);
            _extraMovesSlot?.SetArmed(false);
        }

        private void HandleChargesChanged(BoosterType type, int newCharges)
        {
            switch (type)
            {
                case BoosterType.Shake:
                    _shakeSlot?.UpdateCharges(newCharges);
                    break;
                case BoosterType.Catalyst:
                    _catalystSlot?.UpdateCharges(newCharges);
                    break;
                case BoosterType.ExtraMoves:
                    _extraMovesSlot?.UpdateCharges(newCharges);
                    break;
            }
        }

        private void OnDestroy()
        {
            if (_boosterManager != null)
                _boosterManager.OnChargesChanged -= HandleChargesChanged;
        }
    }
}
