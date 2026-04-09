using UnityEngine;

namespace Brew.Data
{
    /// <summary>
    /// Board dimensions and spacing configuration for level initialization.
    /// </summary>
    [CreateAssetMenu(
        fileName = "BoardConfig",
        menuName = "Brew/Config/Board Config",
        order = 1
    )]
    public class BoardConfigSO : ScriptableObject
    {
        [Header("Board Dimensions")]
        [SerializeField] private int _width = 7;
        [SerializeField] private int _height = 9;

        [Header("Visual Layout")]
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private float _cellSpacing = 0.1f;

        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        public float CellSpacing => _cellSpacing;

        private void OnValidate()
        {
            _width = Mathf.Clamp(_width, 5, 11);
            _height = Mathf.Clamp(_height, 5, 11);
            _cellSize = Mathf.Max(0.01f, _cellSize);
            _cellSpacing = Mathf.Max(0f, _cellSpacing);
        }
    }
}
