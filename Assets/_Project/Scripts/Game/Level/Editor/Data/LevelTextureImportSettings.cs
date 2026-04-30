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

        public LevelTextureImportSettings(int maxGridSize, float alphaThreshold, float cellOpaqueRatio) : this(maxGridSize, alphaThreshold, cellOpaqueRatio, 0, 1)
        {
        }

        public LevelTextureImportSettings(int maxGridSize, float alphaThreshold, float cellOpaqueRatio, int difficultyIndex, int levelNumber)
        {
            MaxGridSize = maxGridSize;
            AlphaThreshold = alphaThreshold;
            CellOpaqueRatio = cellOpaqueRatio;
            DifficultyIndex = difficultyIndex;
            LevelNumber = levelNumber;
        }

        public static LevelTextureImportSettings FromConfig(LevelEditorConfigSO config)
        {
            if (config == null)
                return new LevelTextureImportSettings(35, 0.5f, 0.5f);

            return new LevelTextureImportSettings(config.MaxGridSize, config.AlphaThreshold, config.CellOpaqueRatio);
        }
    }
}