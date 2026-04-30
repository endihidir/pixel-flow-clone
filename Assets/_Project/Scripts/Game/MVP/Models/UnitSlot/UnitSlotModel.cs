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
            slotIndex = -1;
            return false;
        }

        public void RemoveUnit(int slotIndex)
        {
            var unit = _slots[slotIndex];
            if (unit == null) return;
            _slots[slotIndex] = null;
            OnUnitRemoved?.Invoke(slotIndex, unit);
        }

        public BaseLaneUnitObject GetUnitAt(int slotIndex) => _slots[slotIndex];
        public bool IsEmpty(int slotIndex) => _slots[slotIndex] == null;

        public bool IsFull()
        {
            for (int i = 0; i < _slots.Length; i++)
                if (_slots[i] == null) return false;
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