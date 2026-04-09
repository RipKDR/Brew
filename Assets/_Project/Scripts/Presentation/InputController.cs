using System;
using Brew.Core;
using UnityEngine;

namespace Brew.Presentation
{
    /// <summary>
    /// Detects tap input and converts screen position to grid coordinates.
    /// Only forwards taps when the board state machine accepts input.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        [SerializeField] private Camera _inputCamera;

        private BoardStateMachine _stateMachine;
        private float _cellSize;
        private float _cellSpacing;
        private Vector3 _boardOrigin;
        private int _boardWidth;
        private int _boardHeight;

        public event Action<GridCoord> OnCellTapped;

        public void Initialize(
            BoardStateMachine stateMachine,
            int boardWidth,
            int boardHeight,
            float cellSize,
            float cellSpacing,
            Vector3 boardOrigin)
        {
            _stateMachine = stateMachine;
            _boardWidth = boardWidth;
            _boardHeight = boardHeight;
            _cellSize = cellSize;
            _cellSpacing = cellSpacing;
            _boardOrigin = boardOrigin;
        }

        private void Update()
        {
            if (_stateMachine == null || !_stateMachine.AcceptsInput)
                return;

            if (!Input.GetMouseButtonDown(0))
                return;

            if (_inputCamera == null)
                _inputCamera = Camera.main;

            if (_inputCamera == null)
                return;

            var worldPos = _inputCamera.ScreenToWorldPoint(Input.mousePosition);
            if (TryWorldToGrid(worldPos, out var gridCoord))
            {
                OnCellTapped?.Invoke(gridCoord);
            }
        }

        private bool TryWorldToGrid(Vector3 worldPos, out GridCoord coord)
        {
            float stride = _cellSize + _cellSpacing;
            float relX = worldPos.x - _boardOrigin.x;
            float relY = _boardOrigin.y - worldPos.y;

            int col = Mathf.RoundToInt(relX / stride);
            int row = Mathf.RoundToInt(relY / stride);

            coord = new GridCoord(col, row);
            return col >= 0 && col < _boardWidth && row >= 0 && row < _boardHeight;
        }
    }
}
