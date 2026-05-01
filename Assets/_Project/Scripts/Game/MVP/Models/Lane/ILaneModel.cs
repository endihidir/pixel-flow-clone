using System;
using Game.Lane.Item;
using Game.Level.Data;

namespace Game.Models
{
    public interface ILaneModel
    {
        int LaneCount { get; }
        event Action<int, BaseLaneUnitObject> OnUnitAdvanced;
        void Initialize(LaneDefinition[] lanes, BaseLaneUnitObject[][] laneUnits);
        BaseLaneUnitObject GetFrontUnit(int laneIndex);
        bool TryGetLaneIndexOf(BaseLaneUnitObject unit, out int laneIndex);
        void RemoveFrontUnit(int laneIndex);
        bool TryGetLaneSlotIndexOf(BaseLaneUnitObject unit, out int laneIndex, out int slotIndex);
        void RemoveUnitAt(int laneIndex, int slotIndex);
        int GetUnitCount(int laneIndex);
        BaseLaneUnitObject GetUnitAt(int laneIndex, int slot);
    }
}