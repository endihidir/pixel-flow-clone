using Game.Level.Data;

namespace Game.Level.Services
{
    public interface ILevelDataService
    {
        int LevelCount { get; }
        bool IsInitialized { get; }
        void Initialize();
        LevelDefinition GetLevel(int index);
    }
}