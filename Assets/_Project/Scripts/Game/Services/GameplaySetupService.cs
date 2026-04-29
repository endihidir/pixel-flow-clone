using Game.Grid.Handlers;
using Game.Level.Services;
using Game.Models;
using Game.Views;

namespace Game.Services
{
    public sealed class GameplaySetupService : IGameplaySetupService
    {
        private readonly IPixelCellFactoryHandler _pixelCellFactoryHandler;
        private readonly IPixelGridModel _pixelGridModel;
        private readonly IPixelGridView _pixelGridView;
        private readonly ILaneModel _laneModel;
        private readonly IUnitSlotModel _unitSlotModel;
        private readonly ILevelDefinitionProvider _levelDefinitionProvider;

        public GameplaySetupService(ILevelDefinitionProvider levelDefinitionProvider, IPixelCellFactoryHandler pixelCellFactoryHandler, IPixelGridModel pixelGridModel, IPixelGridView pixelGridView, ILaneModel laneModel, 
            IUnitSlotModel unitSlotModel)
        {
            _levelDefinitionProvider = levelDefinitionProvider;
            
            _pixelCellFactoryHandler = pixelCellFactoryHandler;
            _pixelGridModel = pixelGridModel;
            _pixelGridView = pixelGridView;
            
            _laneModel = laneModel;
            _unitSlotModel = unitSlotModel;
        }

        public void SetupGameplay()
        {
            var pixels = _levelDefinitionProvider.GetPixels();
            var width = _levelDefinitionProvider.GetGridWidth();
            var height = _levelDefinitionProvider.GetGridHeight();
            _pixelCellFactoryHandler.PopulatePixelCells(pixels, width, height, out var pixelCells);
            _pixelGridModel.Initialize(pixelCells, width, height, out _);
            _pixelGridView.Initialize(width, height);
            
            _laneModel.Initialize(_levelDefinitionProvider.GetLanes());
            _unitSlotModel.Initialize();
        }

        public void ResetGameplay()
        {
            
        }

        public void ReleaseFactories()
        {
            
        }
    }
}