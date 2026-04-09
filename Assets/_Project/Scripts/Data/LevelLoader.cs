using System;
using System.Collections.Generic;
using Brew.Core;
using UnityEngine;

namespace Brew.Data
{
    /// <summary>
    /// Parses level JSON files into LevelConfig instances.
    /// Handles color name mapping, schema validation, and event theme overlay.
    /// </summary>
    public static class LevelLoader
    {
        private const string ThemedPlaceholder = "themed";

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
        /// Parses a campaign level JSON string into a LevelConfig.
        /// Throws if "themed" appears in the ingredient pool (use
        /// <see cref="LoadEventLevel"/> for event templates).
        /// </summary>
        public static LevelConfig LoadFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("Level JSON is null or empty.", nameof(json));

            var raw = JsonUtility.FromJson<RawLevelData>(json);
            return ConvertToLevelConfig(raw, resolveThemed: false, themedIngredientId: null);
        }

        /// <summary>
        /// Parses an event level JSON, resolving "themed" ingredient slots to the
        /// given ingredient ID from Remote Config. Falls back to "shadow" if
        /// <paramref name="themedIngredientId"/> is null or empty.
        /// </summary>
        public static LevelConfig LoadEventLevel(string json, string themedIngredientId)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("Level JSON is null or empty.", nameof(json));

            var raw = JsonUtility.FromJson<RawLevelData>(json);
            return ConvertToLevelConfig(raw, resolveThemed: true, themedIngredientId);
        }

        private static int _cachedMaxLevelId = -1;

        /// <summary>
        /// Returns the number of level files in Resources/Levels/.
        /// Cached after first call to avoid repeated resource loading.
        /// Falls back to 40 if no resources found.
        /// </summary>
        public static int GetMaxLevelId()
        {
            if (_cachedMaxLevelId > 0)
                return _cachedMaxLevelId;

            var all = Resources.LoadAll<TextAsset>("Levels");
            _cachedMaxLevelId = (all != null && all.Length > 0) ? all.Length : 40;
            return _cachedMaxLevelId;
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

        private static bool IsThemed(string name) =>
            name != null && name.Trim().Equals(ThemedPlaceholder, StringComparison.OrdinalIgnoreCase);

        private static string ResolveThemed(string name, string themedIngredientId)
        {
            if (!IsThemed(name))
                return name;
            return string.IsNullOrWhiteSpace(themedIngredientId) ? "shadow" : themedIngredientId;
        }

        private static LevelConfig ConvertToLevelConfig(
            RawLevelData raw, bool resolveThemed, string themedIngredientId)
        {
            var ingredientPool = new List<IngredientColor>();
            if (raw.ingredient_pool != null)
            {
                foreach (string name in raw.ingredient_pool)
                {
                    string resolved = resolveThemed ? ResolveThemed(name, themedIngredientId) : name;
                    ingredientPool.Add(ParseColor(resolved));
                }
            }

            var recipeTargets = new List<RecipeTarget>();
            if (raw.recipe_targets != null)
            {
                foreach (var rt in raw.recipe_targets)
                {
                    string resolved = resolveThemed ? ResolveThemed(rt.ingredient, themedIngredientId) : rt.ingredient;
                    recipeTargets.Add(new RecipeTarget(ParseColor(resolved), rt.count));
                }
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
            public int[] grid_mask;
            public RawBlockerPlacement[] blocker_placements;
            public RawTutorialStep[] tutorial_steps;
        }

        [Serializable]
        private class RawRecipeTarget
        {
            public string ingredient;
            public int count;
        }

        [Serializable]
        private class RawBlockerPlacement
        {
            public string type;
            public int row;
            public int col;
        }

        [Serializable]
        private class RawTutorialStep
        {
            public string instruction;
            public int highlight_row;
            public int highlight_col;
        }
    }
}
