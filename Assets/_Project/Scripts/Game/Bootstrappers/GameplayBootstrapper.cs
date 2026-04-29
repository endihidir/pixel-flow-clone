using Core.Pool.Services;
using Game.Configs;
using Game.Grid.Handlers;
using Game.Level.Services;
using Game.Models;
using Game.Presenters;
using Game.Services;
using Game.View.Factories;
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
            IPixelCellFactory pixelCellFactory = new PixelCellFactory(poolService);
            IPixelGridModel pixelGridModel = new PixelGridModel();
            ILaneModel laneModel = new LaneModel();
            IUnitSlotModel unitSlotModel = new UnitSlotModel();
            IPixelCellFactoryHandler pixelCellFactoryHandler = new PixelCellFactoryHandler(pixelCellFactory, gameplayConfig.ColorPalette);

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