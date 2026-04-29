using System;

namespace Game.Level.Models
{
    public interface ILevelProgressionModel
    {
        event Action OnLevelChanged;
        int CurrentLevelIndex { get; }
        int LevelCompletionCount { get; }
        void Initialize(int maxLevelCount);
        void SetLevel(int levelIndex);
        void AdvanceLevel();
        void ResetProgress();
    }
}