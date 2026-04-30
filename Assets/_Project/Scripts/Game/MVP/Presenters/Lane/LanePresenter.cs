using System;
using Game.Lane.Item;
using Game.Models;
using Game.Services;
using Game.Views;
using UnityEngine;

namespace Game.Presenters
{
    public sealed class LanePresenter : IDisposable
    {
        private readonly ILaneModel _model;
        private readonly ILaneView _view;
        private readonly IInputService _inputService;
        private readonly Camera _camera;

        public event Action<BaseLaneUnitObject> OnFrontUnitTapped;

        public LanePresenter(ILaneModel model, ILaneView view, IInputService inputService, Camera camera)
        {
            _model = model;
            _view = view;
            _inputService = inputService;
            _camera = camera;

            _view.OnViewInitialized += OnViewInitialized;
            _inputService.OnTap += OnTap;
        }

        private void OnViewInitialized()
        {
            for (int i = 0; i < _model.LaneCount; i++)
            {
                int unitCount = _model.GetUnitCount(i);
                
                for (int ui = 0; ui < unitCount; ui++)
                {
                    var unit = _model.GetUnitAt(i, ui);
                    _view.PlaceUnit(i, ui, unit);
                }
            }
        }

        private void OnTap(Vector2 screenPoint)
        {
            if (!_view.TryGetLaneIndexAtScreenPoint(screenPoint, _camera, out int laneIndex)) return;

            var front = _model.GetFrontUnit(laneIndex);
            if (front == null) return;

            OnFrontUnitTapped?.Invoke(front);
        }

        public void Dispose()
        {
            _view.OnViewInitialized -= OnViewInitialized;
            _inputService.OnTap -= OnTap;
        }
    }
}