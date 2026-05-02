using System;
using Cysharp.Threading.Tasks;
using Game.Handlers;
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
        private readonly ILaneUnitShootHandler _laneUnitShootHandler;

        public UnitSlotPresenter(IUnitSlotModel model, IUnitSlotView view, IInputService inputService, ILaneUnitShootHandler laneUnitShootHandler)
        {
            _model = model;
            _view = view;
            _inputService = inputService;
            _laneUnitShootHandler = laneUnitShootHandler;

            _model.OnUnitAdded += OnUnitAdded;
            _model.OnUnitShifted += OnUnitShifted;
            _inputService.OnTap += OnTap;
        }

        private void OnUnitAdded(int slotIndex, BaseLaneUnitObject unit) => JumpAsync(slotIndex, unit).Forget();

        private void OnUnitShifted(int fromSlotIndex, int toSlotIndex, BaseLaneUnitObject unit) => JumpAsync(toSlotIndex, unit).Forget();

        private async UniTask JumpAsync(int slotIndex, BaseLaneUnitObject unit) => await _view.JumpUnitToSlot(slotIndex, unit);

        private void OnTap(Vector2 screenPoint)
        {
            if (IsAnyUnitJumping()) return;
            if (!_view.TryGetSlotIndexAtScreenPoint(screenPoint, out var slotIndex)) return;
            if (_model.IsEmpty(slotIndex)) return;

            var unit = _model.GetUnitAt(slotIndex);
            _laneUnitShootHandler.OnSlotUnitTapped(slotIndex, unit);
        }
        
        private bool IsAnyUnitJumping()
        {
            for (int i = 0; i < _model.SlotCount; i++)
            {
                if (_model.IsEmpty(i)) continue;
                if (_model.GetUnitAt(i).Animation.IsJumping) return true;
            }
            return false;
        }

        public void Dispose()
        {
            _model.OnUnitAdded -= OnUnitAdded;
            _model.OnUnitShifted -= OnUnitShifted;
            _inputService.OnTap -= OnTap;
        }
    }
}