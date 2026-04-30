using System;

namespace Game.Level.EditorTools
{
    [Serializable]
    public struct LevelTextureImportSettings
    {
        public int MaxGridSize;
        public float AlphaThreshold;
        public float CellOpaqueRatio;
        public int DifficultyIndex;
        public int LevelNumber;
        public int LaneCount;

        public LevelTextureImportSettings(int maxGridSize, float alphaThreshold, float cellOpaqueRatio, int difficultyIndex, int levelNumber)
            : this(maxGridSize, alphaThreshold, cellOpaqueRatio, difficultyIndex, levelNumber, 2) { }

        public LevelTextureImportSettings(int maxGridSize, float alphaThreshold, float cellOpaqueRatio, int difficultyIndex = 0, int levelNumber = 1, int laneCount = 2)
        {
            MaxGridSize = maxGridSize;
            AlphaThreshold = alphaThreshold;
            CellOpaqueRatio = cellOpaqueRatio;
            DifficultyIndex = difficultyIndex;
            LevelNumber = levelNumber;
            LaneCount = laneCount;
        }

        public static LevelTextureImportSettings FromConfig(LevelEditorConfigSO config)
        {
            if (!config)
                return new LevelTextureImportSettings(35, 0.5f, 0.5f, 0, 1, 2);

            return new LevelTextureImportSettings(config.MaxGridSize, config.AlphaThreshold, config.CellOpaqueRatio, 0, 1, config.DefaultLaneCount);
        }
    }
}