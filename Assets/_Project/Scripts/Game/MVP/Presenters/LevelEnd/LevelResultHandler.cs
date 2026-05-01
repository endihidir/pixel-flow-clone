using System;
using Game.Grid.Item;
using Game.Models;

namespace Game.Handlers
{
    public sealed class LevelResultHandler : ILevelResultHandler
    {
        private readonly IPixelGridModel _pixelGridModel;
        private readonly IUnitSlotModel _unitSlotModel;

        private bool _isResolved;

        public event Action OnLevelSuccess;
        public event Action OnLevelFail;

        public LevelResultHandler(IPixelGridModel pixelGridModel, IUnitSlotModel unitSlotModel)
        {
            Reset();
            _pixelGridModel = pixelGridModel;
            _unitSlotModel = unitSlotModel;

            _pixelGridModel.OnUpdateCellData += OnGridCellUpdated;
            _unitSlotModel.OnSlotsFull += OnSlotFull;
        }

        public void Reset() => _isResolved = false;

        public void Dispose()
        {
            _pixelGridModel.OnUpdateCellData -= OnGridCellUpdated;
            _unitSlotModel.OnSlotsFull -= OnSlotFull;
        }

        private void OnGridCellUpdated(PixelCellObject _)
        {
            if (_isResolved) return;
            if (!_pixelGridModel.IsAllNull()) return;

            _isResolved = true;
            OnLevelSuccess?.Invoke();
        }

        private void OnSlotFull()
        {
            if (_isResolved) return;
            _isResolved = true;
            OnLevelFail?.Invoke();
        }
    }
}