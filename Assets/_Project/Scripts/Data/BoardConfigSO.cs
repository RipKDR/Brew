using Brew.Core;
using UnityEngine;

namespace Brew.Data
{
    /// <summary>
    /// Board dimensions, layout, and gameplay configuration.
    /// Provides design-time defaults; runtime values may be overridden by Remote Config.
    /// </summary>
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Brew/Config/Board Config", order = 1)]
    public class BoardConfigSO : ScriptableObject
    {
        [Header("Board Dimensions")]
        [SerializeField] private int _width = 7;
        [SerializeField] private int _height = 9;

        [Header("Visual Layout")]
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private float _cellSpacing = 0.1f;

        [Header("Gameplay")]
        [SerializeField] private int _minClusterSize = 3;
        [SerializeField] private int _brewThreshold = 6;
        [SerializeField] private int _maxCascadeWaves = 20;
        [SerializeField, Tooltip("Consecutive deadlock reshuffles before regenerating tokens. Orbs and stones stay.")]
        private int _maxReshuffles = 10;

        [Header("Available Ingredients")]
        [SerializeField] private IngredientColor[] _availableColors =
        {
            IngredientColor.Ember,
            IngredientColor.Frost,
            IngredientColor.Vine,
            IngredientColor.Sun,
            IngredientColor.Shadow
        };

        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        public float CellSpacing => _cellSpacing;
        public int MinClusterSize => _minClusterSize;
        public int BrewThreshold => _brewThreshold;
        public int MaxCascadeWaves => _maxCascadeWaves;
        /// <summary>
        /// Consecutive deadlock reshuffles before token regeneration (core-mechanic.md §11.4).
        /// Uninitialized assets deserialize as 0; treat that as the design default of 10.
        /// </summary>
        public int MaxReshuffles => _maxReshuffles > 0 ? _maxReshuffles : 10;
        public IngredientColor[] AvailableColors => _availableColors;

        private void OnValidate()
        {
            _width = Mathf.Clamp(_width, 5, 9);
            _height = Mathf.Clamp(_height, 5, 11);
            _cellSize = Mathf.Max(0.01f, _cellSize);
            _cellSpacing = Mathf.Max(0f, _cellSpacing);
            _minClusterSize = Mathf.Max(2, _minClusterSize);
            _brewThreshold = Mathf.Max(2, _brewThreshold);
            _maxCascadeWaves = Mathf.Clamp(_maxCascadeWaves, 1, 50);
            _maxReshuffles = Mathf.Clamp(_maxReshuffles, 1, 50);
        }
    }
}
