using System;
using Cysharp.Threading.Tasks;
using Game.Handlers;
using Game.Level.Models;
using Game.Services;
using Game.Views;

namespace Game.Presenters
{
    public sealed class LevelEndPresenter : IDisposable
    {
        private readonly ILevelResultHandler _levelResultHandler;
        private readonly ILevelEndView _view;
        private readonly IGameplaySetupService _gameplaySetupService;
        private readonly ILevelProgressionModel _levelProgressionModel;
        private readonly IInputService _inputService;

        public LevelEndPresenter(ILevelResultHandler levelResultHandler, ILevelEndView view, IGameplaySetupService gameplaySetupService, 
            ILevelProgressionModel levelProgressionModel, IInputService inputService)
        {
            _levelResultHandler = levelResultHandler;
            _view = view;
            _gameplaySetupService = gameplaySetupService;
            _levelProgressionModel = levelProgressionModel;
            _inputService = inputService;

            _view.HideAll();

            _levelResultHandler.OnLevelSuccess += OnLevelSuccess;
            _levelResultHandler.OnLevelFail += OnLevelFail;
            _view.OnNextButtonClicked += OnWinButtonClicked;
            _view.OnTryAgainButtonClicked += OnFailButtonClicked;
        }

        public void Dispose()
        {
            _levelResultHandler.OnLevelSuccess -= OnLevelSuccess;
            _levelResultHandler.OnLevelFail -= OnLevelFail;
            _view.OnNextButtonClicked -= OnWinButtonClicked;
            _view.OnTryAgainButtonClicked -= OnFailButtonClicked;
        }

        private void OnLevelSuccess() => ShowResultAsync(.5f, ApplySuccess).Forget();
        private void OnLevelFail() => ShowResultAsync(.5f, ApplyFail).Forget();
        
        private void ApplySuccess()
        {
            _levelProgressionModel.AdvanceLevel();
            _view.ShowWin();
        }

        private void ApplyFail() => _view.ShowFail();

        private async UniTask ShowResultAsync(float delay, Action result)
        {
            _inputService.Disable();
            await UniTask.WaitForSeconds(delay);
            result?.Invoke();
        }
        
        private void OnWinButtonClicked() => HandleResetRequested();
        private void OnFailButtonClicked() => HandleResetRequested();

        private void HandleResetRequested()
        {
            _levelResultHandler.Reset();
            _view.HideAll();
            _gameplaySetupService.ResetGameplay();
        }
    }
}