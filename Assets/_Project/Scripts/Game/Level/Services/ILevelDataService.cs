using Game.Level.Data;

namespace Game.Level.Services
{
    public interface ILevelDataService
    {
        int LevelCount { get; }
        bool IsInitialized { get; }
        bool UseTestLevel { get; }
        int TestLevelIndex { get; }
        int GetUnitSlotSize();
        void Initialize();
        LevelDefinition GetLevelDefinition(int index);
    }
}