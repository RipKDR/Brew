using Brew.Core;
using UnityEngine;

namespace Brew.Presentation
{
    public class TokenView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private GridCoord _gridCoord;
        private CellContent _content;
        private TokenAnimator _animator;
        private float _cellSize = 1f;

        public GridCoord GridCoord => _gridCoord;
        public CellContent Content => _content;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public TokenAnimator Animator => _animator;

        private void Awake()
        {
            _animator = GetComponent<TokenAnimator>();
        }

        public void Initialize(GridCoord coord, CellContent content, Color displayColor, float cellSize)
        {
            _gridCoord = coord;
            _content = content;
            _cellSize = cellSize;

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            _spriteRenderer.color = displayColor;

            float scale = content.IsOrb ? cellSize * 1.3f : content.IsStone ? cellSize * 1.0f : cellSize * 0.85f;
            transform.localScale = new Vector3(scale, scale, 1f);

            if (_animator != null)
                _animator.PlayIdleShimmer(coord.Col * 0.3f + coord.Row * 0.7f);
        }

        public void UpdateVisual(CellContent content, Color displayColor)
        {
            _content = content;

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null) return;

            _spriteRenderer.color = displayColor;

            float scale = content.IsOrb ? _cellSize * 1.3f : content.IsStone ? _cellSize * 1.0f : _cellSize * 0.85f;
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
