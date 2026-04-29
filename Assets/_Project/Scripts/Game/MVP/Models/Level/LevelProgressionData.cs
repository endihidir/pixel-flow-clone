using System;

namespace Game.Level.Data
{
    [Serializable]
    public struct LevelProgressionData
    {
        public int currentLevelIndex;
        public int levelCompletionCount;
    }
}