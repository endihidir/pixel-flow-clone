using Game.Lane.Item;
using UnityEngine;

namespace Game.Views
{
    public interface IUnitSlotView
    {
        int SlotCount { get; }
        void PlaceUnit(int slotIndex, BaseLaneUnitObject unit);
        bool TryGetSlotIndexAtScreenPoint(Vector2 screenPoint, out int slotIndex);
    }
}