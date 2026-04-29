using Core.Pool.Services;
using Game.Configs;
using Game.Factory.Handlers;
using Game.Level.Services;
using Game.Models;
using Game.Presenters;
using Game.Services;
using Game.Factories;
using Game.Views;
using UnityEngine;

namespace Game.Bootstrappers
{
    public class GameplayBootstrapper : MonoBehaviour
    {
        [field: SerializeField] private PixelGridView PixelGridView { get; set; }

        private GameplaySetupService _gameplaySetupService;
        private PixelGridPresenter _pixelGridPresenter;

        public void Initialize(IObjectPoolService poolService, ILevelDefinitionProvider levelDefinitionProvider, GameplayConfigContainerSO gameplayConfig)
        {
            
            IPixelGridModel pixelGridModel = new PixelGridModel();
            ILaneModel laneModel = new LaneModel();
            IUnitSlotModel unitSlotModel = new UnitSlotModel();
            
            IPixelCellFactory pixelCellFactory = new PixelCellFactory(poolService);
            ILaneUnitFactory laneUnitFactory = new LaneUnitFactory(poolService);
            IPixelCellFactoryHandler pixelCellFactoryHandler = new PixelCellFactoryHandler(pixelCellFactory, gameplayConfig.ColorPalette);
            ILaneUnitFactoryHandler laneUnitFactoryHandler = new LaneUnitFactoryHandler(laneUnitFactory,  gameplayConfig.ColorPalette);

            _pixelGridPresenter = new PixelGridPresenter(pixelGridModel, PixelGridView);
            _gameplaySetupService = new GameplaySetupService(levelDefinitionProvider, pixelCellFactoryHandler, pixelGridModel, PixelGridView, laneModel, unitSlotModel);
            _gameplaySetupService.SetupGameplay();
        }

        private void OnDestroy()
        {
            _pixelGridPresenter?.Dispose();
        }
    }
}