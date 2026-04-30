using System;
using Game.Lane.Item;
using Game.Models;
using Game.Services;
using Game.Views;
using UnityEngine;

namespace Game.Presenters
{
    public sealed class UnitSlotPresenter : IDisposable
    {
        private readonly IUnitSlotModel _model;
        private readonly IUnitSlotView _view;
        private readonly IInputService _inputService;
        private readonly Camera _camera;

        public event Action<int, BaseLaneUnitObject> OnSlotUnitTapped;

        public UnitSlotPresenter(IUnitSlotModel model, IUnitSlotView view, IInputService inputService, Camera camera)
        {
            _model = model;
            _view = view;
            _inputService = inputService;
            _camera = camera;

            _model.OnUnitAdded += OnUnitAdded;
            _inputService.OnTap += OnTap;
        }

        private void OnUnitAdded(int slotIndex, BaseLaneUnitObject unit)
        {
            _view.PlaceUnit(slotIndex, unit);
        }

        private void OnTap(Vector2 screenPoint)
        {
            if (!_view.TryGetSlotIndexAtScreenPoint(screenPoint, _camera, out var slotIndex)) return;
            if (_model.IsEmpty(slotIndex)) return;

            var unit = _model.GetUnitAt(slotIndex);
            OnSlotUnitTapped?.Invoke(slotIndex, unit);
        }

        public void Dispose()
        {
            _model.OnUnitAdded -= OnUnitAdded;
            _inputService.OnTap -= OnTap;
        }
    }
}