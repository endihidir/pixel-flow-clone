using Game.Level.Data;

namespace Game.Level.Services
{
    public interface ILevelDataService
    {
        int LevelCount { get; }
        bool IsInitialized { get; }
        bool UseTestLevel { get; }
        int TestLevelIndex { get; }
        void Initialize();
        LevelDefinition GetLevelDefinition(int index);
    }
}