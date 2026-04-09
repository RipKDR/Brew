using System;
using System.Collections.Generic;

namespace Brew.Data
{
    /// <summary>
    /// Serializable player progress data. Persisted locally as JSON.
    /// Pure C# for the data structure; LocalSaveManager handles Unity file I/O.
    /// </summary>
    [Serializable]
    public class PlayerProgress
    {
        public int SaveVersion;
        public int CurrentLevel;
        public int TotalLevelsCompleted;
        public List<LevelStarEntry> LevelStars;

        public PlayerProgress()
        {
            SaveVersion = 1;
            CurrentLevel = 1;
            TotalLevelsCompleted = 0;
            LevelStars = new List<LevelStarEntry>();
        }

        public int GetStars(int levelId)
        {
            for (int i = 0; i < LevelStars.Count; i++)
                if (LevelStars[i].LevelId == levelId)
                    return LevelStars[i].Stars;
            return 0;
        }

        public void SetStars(int levelId, int stars)
        {
            for (int i = 0; i < LevelStars.Count; i++)
            {
                if (LevelStars[i].LevelId == levelId)
                {
                    if (stars > LevelStars[i].Stars)
                        LevelStars[i] = new LevelStarEntry(levelId, stars);
                    return;
                }
            }
            LevelStars.Add(new LevelStarEntry(levelId, stars));
        }

        /// <summary>
        /// Records a level completion: updates stars (best only), advances current level,
        /// and increments total count.
        /// </summary>
        public void RecordLevelComplete(int levelId, int stars)
        {
            SetStars(levelId, stars);

            if (levelId >= CurrentLevel)
                CurrentLevel = levelId + 1;

            TotalLevelsCompleted++;
        }

        public bool IsLevelUnlocked(int levelId) => levelId <= CurrentLevel;
    }

    [Serializable]
    public struct LevelStarEntry
    {
        public int LevelId;
        public int Stars;

        public LevelStarEntry(int levelId, int stars)
        {
            LevelId = levelId;
            Stars = stars;
        }
    }
}
