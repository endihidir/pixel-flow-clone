using System;
using Cysharp.Threading.Tasks;
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
        bool TryGetLaneUnitSlotAtScreenPoint(Vector2 screenPoint, Camera cam, int laneIndex, int unitCount, out int slotIndex);
        void PlaceUnit(int laneIndex, int slotIndex, BaseLaneUnitObject unit);
        UniTask AnimateUnitToSlot(int slotIndex, int moveOrder, BaseLaneUnitObject unit);
    }
}