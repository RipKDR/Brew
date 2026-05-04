using Brew.Data;
using Brew.Data.LiveOps;
using Brew.Data.Services;
using UnityEditor;
using UnityEngine;

namespace Brew.Editor
{
    public static class ConfigAssetGenerator
    {
        [MenuItem("Brew/Generate Config Assets")]
        public static void GenerateAllConfigAssets()
        {
            EnsureConfigFolderExists();

            GenerateEconomyConfig();
            GeneratePotionShelfConfig();
            GenerateWorkshopConfig();
            GenerateDailyBrewConfig();
            GenerateWinStreakConfig();
            GenerateWeeklyEventConfig();
            GenerateNotificationConfig();
            GenerateScreenShakeConfig();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Brew] All config assets generated successfully.");
        }

        private const string ConfigFolder = "Assets/_Project/ScriptableObjects/Config";

        private static void EnsureConfigFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(ConfigFolder))
            {
                var parentFolder = ConfigFolder.Substring(0, ConfigFolder.LastIndexOf('/'));
                var folderName = ConfigFolder.Substring(ConfigFolder.LastIndexOf('/') + 1);
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
        }

        private static void GenerateEconomyConfig()
        {
            var path = $"{ConfigFolder}/EconomyConfig.asset";
            var config = ScriptableObject.CreateInstance<EconomyConfigSO>();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[Brew] Created {path}");
        }

        private static void GeneratePotionShelfConfig()
        {
            var path = $"{ConfigFolder}/PotionShelfConfig.asset";
            var config = ScriptableObject.CreateInstance<PotionShelfConfigSO>();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[Brew] Created {path}");
        }

        private static void GenerateWorkshopConfig()
        {
            var path = $"{ConfigFolder}/WorkshopConfig.asset";
            var config = ScriptableObject.CreateInstance<WorkshopConfigSO>();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[Brew] Created {path}");
        }

        private static void GenerateDailyBrewConfig()
        {
            var path = $"{ConfigFolder}/DailyBrewConfig.asset";
            var config = ScriptableObject.CreateInstance<DailyBrewConfigSO>();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[Brew] Created {path}");
        }

        private static void GenerateWinStreakConfig()
        {
            var path = $"{ConfigFolder}/WinStreakConfig.asset";
            var config = ScriptableObject.CreateInstance<WinStreakConfigSO>();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[Brew] Created {path}");
        }

        private static void GenerateWeeklyEventConfig()
        {
            var path = $"{ConfigFolder}/WeeklyEventConfig.asset";
            var config = ScriptableObject.CreateInstance<WeeklyEventConfigSO>();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[Brew] Created {path}");
        }

        private static void GenerateNotificationConfig()
        {
            var path = $"{ConfigFolder}/NotificationConfig.asset";
            var config = ScriptableObject.CreateInstance<NotificationConfigSO>();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[Brew] Created {path}");
        }

        private static void GenerateScreenShakeConfig()
        {
            var path = $"{ConfigFolder}/ScreenShakeConfig.asset";
            var config = ScriptableObject.CreateInstance<ScreenShakeConfigSO>();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[Brew] Created {path}");
        }
    }
}
