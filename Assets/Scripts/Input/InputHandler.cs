using System;
using System.Collections.Generic;
using Brew.Core;
using UnityEngine;

namespace Brew.Input
{
    /// <summary>
    /// Detects taps and resolves connected same-type clusters via flood fill.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private Camera _inputCamera;
        [SerializeField] private BoardManager _boardManager;
        [SerializeField] private int _minClusterSize = 3;

        public event Action<IReadOnlyList<Vector2Int>> OnValidClusterTapped;

        public IReadOnlyList<Vector2Int> FindCluster(Vector2Int start)
        {
            var cluster = new List<Vector2Int>();
            if (_boardManager == null || !_boardManager.IsReady)
            {
                return cluster;
            }

            if (!_boardManager.TryGetCell(start, out var startCell))
            {
                return cluster;
            }

            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!_boardManager.TryGetCell(current, out var currentCell))
                {
                    continue;
                }

                if (currentCell.Ingredient != startCell.Ingredient)
                {
                    continue;
                }

                cluster.Add(current);
                EnqueueNeighbor(current + Vector2Int.up, startCell.Ingredient, visited, queue);
                EnqueueNeighbor(current + Vector2Int.down, startCell.Ingredient, visited, queue);
                EnqueueNeighbor(current + Vector2Int.left, startCell.Ingredient, visited, queue);
                EnqueueNeighbor(current + Vector2Int.right, startCell.Ingredient, visited, queue);
            }

            return cluster;
        }

        private void Update()
        {
            if (!UnityEngine.Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (_inputCamera == null)
            {
                _inputCamera = Camera.main;
            }

            if (_inputCamera == null)
            {
                return;
            }

            var worldPosition = _inputCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            var coordinate = new Vector2Int(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y));
            var cluster = FindCluster(coordinate);

            if (cluster.Count >= _minClusterSize)
            {
                OnValidClusterTapped?.Invoke(cluster);
            }
        }

        private void EnqueueNeighbor(
            Vector2Int coordinate,
            BoardManager.IngredientType expectedType,
            ISet<Vector2Int> visited,
            Queue<Vector2Int> queue)
        {
            if (visited.Contains(coordinate))
            {
                return;
            }

            if (!_boardManager.TryGetCell(coordinate, out var cell))
            {
                return;
            }

            if (cell.Ingredient != expectedType)
            {
                return;
            }

            visited.Add(coordinate);
            queue.Enqueue(coordinate);
        }
    }
}
