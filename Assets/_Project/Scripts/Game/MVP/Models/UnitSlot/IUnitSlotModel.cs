using System;
using Game.Lane.Item;

namespace Game.Models
{
    public interface IUnitSlotModel
    {
        int SlotCount { get; }

        event Action<int, BaseLaneUnitObject> OnUnitAdded;
        event Action<int, BaseLaneUnitObject> OnUnitRemoved;
        event Action<int, int, BaseLaneUnitObject> OnUnitShifted;
        event Action OnSlotFull;

        void Initialize(int slotCount);
        bool TryAddUnit(BaseLaneUnitObject unit, out int slotIndex);
        bool TryRemoveUnitAndShiftLeft(int slotIndex, out BaseLaneUnitObject unit);
        void RemoveUnit(int slotIndex);
        BaseLaneUnitObject GetUnitAt(int slotIndex);
        bool IsEmpty(int slotIndex);
        bool IsFull();
        bool TryGetSlotIndexOf(BaseLaneUnitObject unit, out int slotIndex);
    }
}