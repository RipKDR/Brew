using Brew.Core;
using UnityEngine;

namespace Brew.Presentation
{
    /// <summary>
    /// Visual representation of a single token or orb on the board.
    /// Managed by BoardPresenter via object pool.
    /// </summary>
    public class TokenView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private GridCoord _gridCoord;
        private CellContent _content;

        public GridCoord GridCoord => _gridCoord;
        public CellContent Content => _content;

        public void Initialize(GridCoord coord, CellContent content, Color displayColor, float cellSize)
        {
            _gridCoord = coord;
            _content = content;

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            _spriteRenderer.color = displayColor;

            float scale = content.IsOrb ? cellSize * 1.3f : cellSize * 0.85f;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        public void UpdateVisual(CellContent content, Color displayColor)
        {
            _content = content;
            _spriteRenderer.color = displayColor;

            float scale = content.IsOrb ? 1.3f : 0.85f;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        public void SetGridCoord(GridCoord coord)
        {
            _gridCoord = coord;
        }

        public void SetSortingOrder(int order)
        {
            if (_spriteRenderer != null)
                _spriteRenderer.sortingOrder = order;
        }
    }
}
