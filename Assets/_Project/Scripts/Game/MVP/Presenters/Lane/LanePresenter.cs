using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Handlers;
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
        private readonly ILaneUnitShootHandler _laneUnitShootHandler;
        private readonly HashSet<int> _lockedLaneIndexes = new();

        public LanePresenter(ILaneModel model, ILaneView view, IInputService inputService, ILaneUnitShootHandler laneUnitShootHandler)
        {
            _model = model;
            _view = view;
            _inputService = inputService;
            _laneUnitShootHandler = laneUnitShootHandler;

            _view.OnViewInitialized += OnViewInitialized;
            _model.OnUnitAdvanced += OnUnitAdvanced;
            _inputService.OnTap += OnTap;
        }

        private void OnViewInitialized()
        {
            for (int i = 0; i < _model.LaneCount; i++)
            {
                var unitCount = _model.GetUnitCount(i);

                for (int j = 0; j < unitCount; j++)
                {
                    var unit = _model.GetUnitAt(i, j);
                    _view.PlaceUnit(i, j, unit);
                }
            }
        }

        private void OnTap(Vector2 screenPoint)
        {
            if (!_view.TryGetLaneIndexAtScreenPoint(screenPoint, out var laneIndex)) return;
            if (_lockedLaneIndexes.Contains(laneIndex)) return;

            var unitCount = _model.GetUnitCount(laneIndex);
            if (!_view.TryGetLaneUnitSlotAtScreenPoint(screenPoint, laneIndex, unitCount, out var slotIndex)) return;
            if (slotIndex != 0) return;

            var unit = _model.GetUnitAt(laneIndex, slotIndex);
            if (!unit) return;

            _laneUnitShootHandler.OnFrontUnitTapped(unit);
        }

        private void OnUnitAdvanced(int laneIndex, BaseLaneUnitObject newFront) => AnimateLaneAdvanceAsync(laneIndex).Forget();

        private async UniTask AnimateLaneAdvanceAsync(int laneIndex)
        {
            _lockedLaneIndexes.Add(laneIndex);

            var unitCount = _model.GetUnitCount(laneIndex);

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