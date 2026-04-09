using System.Collections.Generic;
using UnityEngine;

namespace Brew.Core
{
    /// <summary>
    /// Assigns token ingredients to the board while avoiding immediate 3+ clusters at spawn.
    /// </summary>
    public class TokenSpawner : MonoBehaviour
    {
        [SerializeField] private BoardManager _boardManager;

        private static readonly BoardManager.IngredientType[] IngredientPool =
        {
            BoardManager.IngredientType.Ember,
            BoardManager.IngredientType.Frost,
            BoardManager.IngredientType.Vine,
            BoardManager.IngredientType.Sun,
            BoardManager.IngredientType.Shadow
        };

        public void PopulateInitialBoard()
        {
            if (_boardManager == null || !_boardManager.IsReady)
            {
                Debug.LogError("TokenSpawner requires a ready BoardManager.");
                return;
            }

            for (var y = 0; y < _boardManager.Height; y++)
            {
                for (var x = 0; x < _boardManager.Width; x++)
                {
                    var coordinate = new Vector2Int(x, y);
                    var ingredient = ChooseIngredientWithoutPremadeCluster(coordinate);
                    _boardManager.TrySetIngredient(coordinate, ingredient);
                }
            }
        }

        private BoardManager.IngredientType ChooseIngredientWithoutPremadeCluster(Vector2Int coordinate)
        {
            var candidates = new List<BoardManager.IngredientType>(IngredientPool);

            // Prevent immediate horizontal 3-in-a-row at spawn.
            if (coordinate.x >= 2 &&
                _boardManager.TryGetCell(new Vector2Int(coordinate.x - 1, coordinate.y), out var leftA) &&
                _boardManager.TryGetCell(new Vector2Int(coordinate.x - 2, coordinate.y), out var leftB) &&
                leftA.Ingredient == leftB.Ingredient)
            {
                candidates.Remove(leftA.Ingredient);
            }

            // Prevent immediate vertical 3-in-a-row at spawn.
            if (coordinate.y >= 2 &&
                _boardManager.TryGetCell(new Vector2Int(coordinate.x, coordinate.y - 1), out var downA) &&
                _boardManager.TryGetCell(new Vector2Int(coordinate.x, coordinate.y - 2), out var downB) &&
                downA.Ingredient == downB.Ingredient)
            {
                candidates.Remove(downA.Ingredient);
            }

            if (candidates.Count == 0)
            {
                return IngredientPool[Random.Range(0, IngredientPool.Length)];
            }

            return candidates[Random.Range(0, candidates.Count)];
        }
    }
}
