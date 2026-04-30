using Core.Pool.Services;
using Game.Configs;
using Game.Factory.Handlers;
using Game.Factories;
using Game.Level.Services;
using Game.Models;
using Game.Presenters;
using Game.Services;
using Game.Views;
using UnityEngine;

namespace Game.Bootstrappers
{
    public class GameplayBootstrapper : MonoBehaviour
    {
        [field: SerializeField] private Camera GameplayCamera { get; set; }
        [field: SerializeField] private PixelGridView PixelGridView { get; set; }
        [field: SerializeField] private LaneView LaneView { get; set; }

        private GameplaySetupService _gameplaySetupService;
        private PixelGridPresenter _pixelGridPresenter;
        private LanePresenter _lanePresenter;
        private IInputService _inputService;

        public void Initialize(IObjectPoolService poolService, ILevelDefinitionProvider levelDefinitionProvider, GameplayConfigContainerSO gameplayConfig)
        {
            _inputService = new InputService();

            IPixelGridModel pixelGridModel = new PixelGridModel();
            ILaneModel laneModel = new LaneModel();
            IUnitSlotModel unitSlotModel = new UnitSlotModel();

            IPixelCellFactory pixelCellFactory = new PixelCellFactory(poolService);
            ILaneUnitFactory laneUnitFactory = new LaneUnitFactory(poolService);
            IPixelCellFactoryHandler pixelCellFactoryHandler = new PixelCellFactoryHandler(pixelCellFactory, gameplayConfig.ColorPalette);
            ILaneUnitFactoryHandler laneUnitFactoryHandler = new LaneUnitFactoryHandler(laneUnitFactory, gameplayConfig.ColorPalette);

            _pixelGridPresenter = new PixelGridPresenter(pixelGridModel, PixelGridView);
            _lanePresenter = new LanePresenter(laneModel, LaneView, _inputService, GameplayCamera);

            _gameplaySetupService = new GameplaySetupService(levelDefinitionProvider, pixelCellFactoryHandler, laneUnitFactoryHandler,
                                                                pixelGridModel, PixelGridView,
                                                                laneModel, LaneView,
                                                                unitSlotModel);

            _gameplaySetupService.SetupGameplay();

            _inputService.Enable();
        }

        private void Update() => _inputService?.Update();

        private void OnDestroy()
        {
            _pixelGridPresenter?.Dispose();
            _lanePresenter?.Dispose();
        }
    }
}