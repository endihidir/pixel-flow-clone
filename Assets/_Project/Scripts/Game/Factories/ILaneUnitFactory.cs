using Game.Lane.Item;
using UnityEngine;

namespace Game.Factories
{
    public interface ILaneUnitFactory
    {
        T GetLaneUnit<T>() where T : BaseLaneUnitObject;
        void ReleaseLaneUnit(BaseLaneUnitObject grid);
        void ReleaseLaneUnit(Transform item);
        void ReleaseLaneUnitsByType<T>() where T : BaseLaneUnitObject;
        void RemoveLaneUnitPoolByType<T>() where T : BaseLaneUnitObject;
    }
}