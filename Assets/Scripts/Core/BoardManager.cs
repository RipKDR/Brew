using System;
using Brew.Data;
using UnityEngine;

namespace Brew.Core
{
    /// <summary>
    /// Generates and owns the board grid for gameplay systems.
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Serializable]
        public enum IngredientType
        {
            Ember = 0,
            Frost = 1,
            Vine = 2,
            Sun = 3,
            Shadow = 4
        }

        [Serializable]
        public struct CellData
        {
            public int X;
            public int Y;
            public IngredientType Ingredient;
        }

        [SerializeField] private BoardConfigSO _boardConfig;

        private CellData[,] _cells;

        public int Width => _boardConfig != null ? _boardConfig.Width : 0;
        public int Height => _boardConfig != null ? _boardConfig.Height : 0;
        public bool IsReady => _cells != null;

        public event Action OnBoardGenerated;

        public void GenerateBoard()
        {
            if (_boardConfig == null)
            {
                Debug.LogError("BoardConfigSO is missing on BoardManager.");
                return;
            }

            _cells = new CellData[_boardConfig.Width, _boardConfig.Height];

            for (var x = 0; x < _boardConfig.Width; x++)
            {
                for (var y = 0; y < _boardConfig.Height; y++)
                {
                    _cells[x, y] = new CellData
                    {
                        X = x,
                        Y = y,
                        Ingredient = IngredientType.Ember
                    };
                }
            }

            OnBoardGenerated?.Invoke();
        }

        public bool TryGetCell(Vector2Int coordinate, out CellData cell)
        {
            if (!IsReady || coordinate.x < 0 || coordinate.x >= Width || coordinate.y < 0 || coordinate.y >= Height)
            {
                cell = default;
                return false;
            }

            cell = _cells[coordinate.x, coordinate.y];
            return true;
        }

        public bool TrySetIngredient(Vector2Int coordinate, IngredientType ingredient)
        {
            if (!IsReady || coordinate.x < 0 || coordinate.x >= Width || coordinate.y < 0 || coordinate.y >= Height)
            {
                return false;
            }

            var existing = _cells[coordinate.x, coordinate.y];
            existing.Ingredient = ingredient;
            _cells[coordinate.x, coordinate.y] = existing;
            return true;
        }
    }
}
