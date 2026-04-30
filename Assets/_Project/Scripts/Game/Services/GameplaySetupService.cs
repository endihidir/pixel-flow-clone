using Game.Factory.Handlers;
using Game.Level.Services;
using Game.Models;
using Game.Views;

namespace Game.Services
{
    public sealed class GameplaySetupService : IGameplaySetupService
    {
        private readonly ILevelDefinitionProvider _levelDefinitionProvider;
        private readonly IPixelCellFactoryHandler _pixelCellFactoryHandler;
        private readonly ILaneUnitFactoryHandler _laneUnitFactoryHandler;
        private readonly IPixelGridModel _pixelGridModel;
        private readonly IPixelGridView _pixelGridView;
        private readonly ILaneModel _laneModel;
        private readonly ILaneView _laneView;
        private readonly IUnitSlotModel _unitSlotModel;

        public GameplaySetupService(ILevelDefinitionProvider levelDefinitionProvider, IPixelCellFactoryHandler pixelCellFactoryHandler,
            ILaneUnitFactoryHandler laneUnitFactoryHandler, IPixelGridModel pixelGridModel, IPixelGridView pixelGridView, ILaneModel laneModel,
            ILaneView laneView, IUnitSlotModel unitSlotModel)
        {
            _levelDefinitionProvider = levelDefinitionProvider;
            _pixelCellFactoryHandler = pixelCellFactoryHandler;
            _laneUnitFactoryHandler = laneUnitFactoryHandler;
            _pixelGridModel = pixelGridModel;
            _pixelGridView = pixelGridView;
            _laneModel = laneModel;
            _laneView = laneView;
            _unitSlotModel = unitSlotModel;
        }

        public void SetupGameplay()
        {
            SetupPixelGrid();
            SetupLanes();
            _unitSlotModel.Initialize(_levelDefinitionProvider.GetUnitSlotSize());
        }

        private void SetupPixelGrid()
        {
            var pixels = _levelDefinitionProvider.GetPixels();
            var width = _levelDefinitionProvider.GetGridWidth();
            var height = _levelDefinitionProvider.GetGridHeight();
            _pixelCellFactoryHandler.PopulatePixelCells(pixels, width, height, out var pixelCells);
            _pixelGridModel.Initialize(pixelCells, width, height, out _);
            _pixelGridView.Initialize(width, height);
        }

        private void SetupLanes()
        {
            var lanes = _levelDefinitionProvider.GetLanes();
            _laneUnitFactoryHandler.PopulateLaneUnits(lanes, out var laneUnits);
            _laneModel.Initialize(lanes, laneUnits);
            _laneView.Initialize(lanes.Length);
        }

        public void ResetGameplay() { }
        public void ReleaseFactories() { }
    }
}