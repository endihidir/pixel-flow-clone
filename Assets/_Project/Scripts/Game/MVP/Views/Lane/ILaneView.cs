using System;
using Game.Lane.Item;
using UnityEngine;

namespace Game.Views
{
    public interface ILaneView
    {
        event Action OnViewInitialized;
        void Initialize(int laneCount);
        Transform GetLaneRoot(int laneIndex);
        Vector3 GetUnitLocalPosition(int slotIndex);
        bool TryGetLaneIndexAtScreenPoint(Vector2 screenPoint, Camera cam, out int laneIndex);
        void PlaceUnit(int laneIndex, int slotIndex, BaseLaneUnitObject unit);
    }
}