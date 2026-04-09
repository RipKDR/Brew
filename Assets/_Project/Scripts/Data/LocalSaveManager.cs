using System;
using System.IO;
using UnityEngine;

namespace Brew.Data
{
    /// <summary>
    /// Persists PlayerProgress as JSON to Application.persistentDataPath.
    /// Uses write-to-temp-then-rename for corruption safety.
    /// </summary>
    public static class LocalSaveManager
    {
        private const string FileName = "brew_save.json";
        private const string TempFileName = "brew_save.tmp";
        private const int CurrentSaveVersion = 1;

        private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);
        private static string TempPath => Path.Combine(Application.persistentDataPath, TempFileName);

        public static void Save(PlayerProgress progress)
        {
            if (progress == null)
                throw new ArgumentNullException(nameof(progress));

            progress.SaveVersion = CurrentSaveVersion;
            string json = JsonUtility.ToJson(progress, prettyPrint: true);

            try
            {
                File.WriteAllText(TempPath, json);
                if (File.Exists(SavePath))
                    File.Delete(SavePath);
                File.Move(TempPath, SavePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalSaveManager] Failed to save: {e.Message}");
            }
        }

        public static PlayerProgress Load()
        {
            try
            {
                if (!File.Exists(SavePath))
                    return new PlayerProgress();

                string json = File.ReadAllText(SavePath);
                if (string.IsNullOrWhiteSpace(json))
                    return new PlayerProgress();

                var progress = JsonUtility.FromJson<PlayerProgress>(json);
                if (progress == null)
                    return new PlayerProgress();

                if (progress.LevelStars == null)
                    progress.LevelStars = new System.Collections.Generic.List<LevelStarEntry>();

                return progress;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LocalSaveManager] Failed to load save, returning defaults: {e.Message}");
                return new PlayerProgress();
            }
        }

        public static bool SaveExists() => File.Exists(SavePath);

        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
                if (File.Exists(TempPath)) File.Delete(TempPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalSaveManager] Failed to delete save: {e.Message}");
            }
        }
    }
}
