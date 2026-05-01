using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
        private readonly HashSet<int> _lockedLaneIndexes = new();

        public event Action<BaseLaneUnitObject> OnFrontUnitTapped;

        public LanePresenter(ILaneModel model, ILaneView view, IInputService inputService, Camera camera)
        {
            _model = model;
            _view = view;
            _inputService = inputService;
            _camera = camera;

            _view.OnViewInitialized += OnViewInitialized;
            _model.OnUnitAdvanced += OnUnitAdvanced;
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
            if (_lockedLaneIndexes.Contains(laneIndex)) return;

            int unitCount = _model.GetUnitCount(laneIndex);
            if (!_view.TryGetLaneUnitSlotAtScreenPoint(screenPoint, _camera, laneIndex, unitCount, out int slotIndex)) return;
            if (slotIndex != 0) return;

            var front = _model.GetUnitAt(laneIndex, slotIndex);
            if (!front) return;

            OnFrontUnitTapped?.Invoke(front);
        }

        private void OnUnitAdvanced(int laneIndex, BaseLaneUnitObject newFront) => AnimateLaneAdvanceAsync(laneIndex).Forget();

        private async UniTask AnimateLaneAdvanceAsync(int laneIndex)
        {
            _lockedLaneIndexes.Add(laneIndex);

            int unitCount = _model.GetUnitCount(laneIndex);

            var tasks = new UniTask[unitCount];

            for (int i = 0; i < unitCount; i++)
            {
                var unit = _model.GetUnitAt(laneIndex, i);
                
                tasks[i] = _view.AnimateUnitToSlot(i, i, unit);
            }

            await UniTask.WhenAll(tasks);
            
            _lockedLaneIndexes.Remove(laneIndex);
        }

        public void Dispose()
        {
            _view.OnViewInitialized -= OnViewInitialized;
            _model.OnUnitAdvanced -= OnUnitAdvanced;
            _inputService.OnTap -= OnTap;

            _lockedLaneIndexes.Clear();
        }
    }
}