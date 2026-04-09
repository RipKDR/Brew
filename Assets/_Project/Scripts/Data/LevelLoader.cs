using System;
using System.Collections.Generic;
using Brew.Core;
using UnityEngine;

namespace Brew.Data
{
    /// <summary>
    /// Parses level JSON files into LevelConfig instances.
    /// Handles color name mapping and schema validation.
    /// </summary>
    public static class LevelLoader
    {
        private static readonly Dictionary<string, IngredientColor> ColorMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "ember", IngredientColor.Ember },
            { "frost", IngredientColor.Frost },
            { "vine",  IngredientColor.Vine },
            { "sun",   IngredientColor.Sun },
            { "shadow", IngredientColor.Shadow },
            { "brine", IngredientColor.Sun },
            { "glow",  IngredientColor.Shadow },
        };

        /// <summary>
        /// Parses a level JSON string into a LevelConfig.
        /// </summary>
        public static LevelConfig LoadFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("Level JSON is null or empty.", nameof(json));

            var raw = JsonUtility.FromJson<RawLevelData>(json);
            return ConvertToLevelConfig(raw);
        }

        /// <summary>
        /// Loads a level from Unity Resources at path "Levels/level_NNN".
        /// The .json extension must be stripped; file must be a TextAsset.
        /// </summary>
        public static LevelConfig LoadFromResources(int levelId)
        {
            string path = $"Levels/level_{levelId:D3}";
            var textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
                throw new InvalidOperationException($"Level resource not found: {path}");
            return LoadFromJson(textAsset.text);
        }

        /// <summary>
        /// Maps a JSON ingredient name string to the IngredientColor enum.
        /// </summary>
        public static IngredientColor ParseColor(string name)
        {
            if (name != null && ColorMap.TryGetValue(name.Trim(), out var color))
                return color;
            throw new ArgumentException($"Unknown ingredient color: '{name}'. " +
                $"Valid names: ember, frost, vine, sun, shadow, brine, glow.");
        }

        private static LevelConfig ConvertToLevelConfig(RawLevelData raw)
        {
            var ingredientPool = new List<IngredientColor>();
            if (raw.ingredient_pool != null)
            {
                foreach (string name in raw.ingredient_pool)
                    ingredientPool.Add(ParseColor(name));
            }

            var recipeTargets = new List<RecipeTarget>();
            if (raw.recipe_targets != null)
            {
                foreach (var rt in raw.recipe_targets)
                    recipeTargets.Add(new RecipeTarget(ParseColor(rt.ingredient), rt.count));
            }

            int[] starThresholds;
            if (raw.star_thresholds != null && raw.star_thresholds.Length == 3)
                starThresholds = raw.star_thresholds;
            else
                starThresholds = new[] { 1000, 3000, 6000 };

            return new LevelConfig(
                raw.level_id,
                raw.grid_width,
                raw.grid_height,
                ingredientPool,
                recipeTargets,
                raw.move_limit,
                starThresholds,
                raw.is_tutorial);
        }

        [Serializable]
        private class RawLevelData
        {
            public int level_id;
            public int grid_width;
            public int grid_height;
            public string[] ingredient_pool;
            public RawRecipeTarget[] recipe_targets;
            public int move_limit;
            public int[] star_thresholds;
            public bool is_tutorial;
        }

        [Serializable]
        private class RawRecipeTarget
        {
            public string ingredient;
            public int count;
        }
    }
}
