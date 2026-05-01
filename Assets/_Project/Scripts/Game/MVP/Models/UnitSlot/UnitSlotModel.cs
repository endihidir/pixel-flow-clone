using System;
using Game.Lane.Item;

namespace Game.Models
{
    public sealed class UnitSlotModel : IUnitSlotModel
    {
        private BaseLaneUnitObject[] _slots;

        public int SlotCount => _slots?.Length ?? 0;

        public event Action<int, BaseLaneUnitObject> OnUnitAdded;
        public event Action<int, BaseLaneUnitObject> OnUnitRemoved;
        public event Action<int, int, BaseLaneUnitObject> OnUnitShifted;
        public event Action OnSlotFull;

        public void Initialize(int slotCount)
        {
            _slots = new BaseLaneUnitObject[slotCount];
        }

        public bool TryAddUnit(BaseLaneUnitObject unit, out int slotIndex)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != null) continue;

                _slots[i] = unit;
                slotIndex = i;
                OnUnitAdded?.Invoke(i, unit);
                return true;
            }

            OnSlotFull?.Invoke();
            slotIndex = -1;
            return false;
        }

        public bool TryRemoveUnitAndShiftLeft(int slotIndex, out BaseLaneUnitObject unit)
        {
            unit = null;

            if (slotIndex < 0 || slotIndex >= _slots.Length) return false;

            unit = _slots[slotIndex];

            if (unit == null) return false;

            _slots[slotIndex] = null;
            OnUnitRemoved?.Invoke(slotIndex, unit);

            for (int i = slotIndex + 1; i < _slots.Length; i++)
            {
                var shiftedUnit = _slots[i];

                if (shiftedUnit == null) continue;

                _slots[i - 1] = shiftedUnit;
                _slots[i] = null;

                OnUnitShifted?.Invoke(i, i - 1, shiftedUnit);
            }

            return true;
        }

        public void RemoveUnit(int slotIndex)
        {
            TryRemoveUnitAndShiftLeft(slotIndex, out _);
        }

        public BaseLaneUnitObject GetUnitAt(int slotIndex)
        {
            return _slots[slotIndex];
        }

        public bool IsEmpty(int slotIndex)
        {
            return _slots[slotIndex] == null;
        }

        public bool IsFull()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == null) return false;
            }

            return true;
        }

        public bool TryGetSlotIndexOf(BaseLaneUnitObject unit, out int slotIndex)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != unit) continue;

                slotIndex = i;
                return true;
            }

            slotIndex = -1;
            return false;
        }
    }
}