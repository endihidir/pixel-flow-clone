using Game.Level.Data;
using Game.Level.Models;
using UnityEngine;

namespace Game.Level.Services
{
    public sealed class LevelDefinitionProvider : ILevelDefinitionProvider
    {
        private readonly ILevelDataService _levelDataService;
        private readonly ILevelProgressionModel _progressionModel;
        
        private int CurrentLevelIndex => !_levelDataService.UseTestLevel ? _progressionModel.CurrentLevelIndex : 
                                                                                   _levelDataService.TestLevelIndex;

        public LevelDefinitionProvider(ILevelDataService levelDataService, ILevelProgressionModel progressionModel)
        {
            _levelDataService = levelDataService;
            _progressionModel = progressionModel;
        }
        
        public int GetUnitSlotSize() => _levelDataService.GetUnitSlotSize();
        public int GetGridWidth() => _levelDataService.GetLevelDefinition(CurrentLevelIndex).Width;
        public int GetGridHeight() => _levelDataService.GetLevelDefinition(CurrentLevelIndex).Height;
        public Vector2Int GetGridSize() => _levelDataService.GetLevelDefinition(CurrentLevelIndex).GridSize;
        public LaneDefinition[] GetLanes() => _levelDataService.GetLevelDefinition(CurrentLevelIndex).Lanes;
        public ColorId[] GetPixels() => _levelDataService.GetLevelDefinition(CurrentLevelIndex).Pixels;
        public int GetLevelNumber(bool useLevelCompletionCount = false) => useLevelCompletionCount 
                                                                  ? _progressionModel.LevelCompletionCount + 1
                                                                  : _levelDataService.GetLevelDefinition(CurrentLevelIndex).LevelNumber;
    }
}