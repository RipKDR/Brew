using Brew.Data;
using Brew.Data.LiveOps;
using Brew.Data.Services;
using UnityEditor;
using UnityEngine;

namespace Brew.Editor
{
    /// <summary>
    /// Generates missing config ScriptableObject assets under Assets/_Project/ScriptableObjects/Config.
    /// Idempotent: existing assets are left unchanged.
    /// Batchmode: Unity -batchmode -quit -projectPath . -executeMethod Brew.Editor.ConfigAssetGenerator.GenerateAllConfigAssets
    /// </summary>
    public static class ConfigAssetGenerator
    {
        private const string ConfigFolder = "Assets/_Project/ScriptableObjects/Config";

        [MenuItem("Brew/Generate Config Assets")]
        public static void GenerateAllConfigAssets()
        {
            EnsureConfigFolderExists();

            CreateIfMissing<EconomyConfigSO>("EconomyConfig.asset");
            CreateIfMissing<PotionShelfConfigSO>("PotionShelfConfig.asset");
            CreateIfMissing<WorkshopConfigSO>("WorkshopConfig.asset");
            CreateIfMissing<DailyBrewConfigSO>("DailyBrewConfig.asset");
            CreateIfMissing<WinStreakConfigSO>("WinStreakConfig.asset");
            CreateIfMissing<WeeklyEventConfigSO>("WeeklyEventConfig.asset");
            CreateIfMissing<NotificationConfigSO>("NotificationConfig.asset");
            CreateIfMissing<ScreenShakeConfigSO>("ScreenShakeConfig.asset");
            CreateIfMissing<AudioConfigSO>("AudioConfig.asset");
            CreateIfMissing<BoardConfigSO>("BoardConfig.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Brew] Config asset generation complete (missing assets created; existing left unchanged).");
        }

        private static void EnsureConfigFolderExists()
        {
            if (AssetDatabase.IsValidFolder(ConfigFolder))
                return;

            var parentFolder = ConfigFolder.Substring(0, ConfigFolder.LastIndexOf('/'));
            var folderName = ConfigFolder.Substring(ConfigFolder.LastIndexOf('/') + 1);

            if (!AssetDatabase.IsValidFolder(parentFolder))
            {
                var root = parentFolder.Substring(0, parentFolder.LastIndexOf('/'));
                var mid = parentFolder.Substring(parentFolder.LastIndexOf('/') + 1);
                if (!AssetDatabase.IsValidFolder(parentFolder))
                    AssetDatabase.CreateFolder(root, mid);
            }

            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        private static void CreateIfMissing<T>(string fileName) where T : ScriptableObject
        {
            var path = $"{ConfigFolder}/{fileName}";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                Debug.Log($"[Brew] Skip existing {path}");
                return;
            }

            var config = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[Brew] Created {path}");
        }
    }
}
