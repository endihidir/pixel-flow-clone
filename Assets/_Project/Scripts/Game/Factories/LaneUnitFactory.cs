using Core.Pool.Services;
using Game.Lane.Item;
using UnityEngine;

namespace Game.Factories
{
    public sealed class LaneUnitFactory : ILaneUnitFactory
    {
        private readonly IObjectPoolService _objectPoolService;

        public LaneUnitFactory(IObjectPoolService objectPoolService) => _objectPoolService = objectPoolService;

        public T GetLaneUnit<T>() where T : BaseLaneUnitObject => _objectPoolService.GetObject<T>();
        public void ReleaseLaneUnit(BaseLaneUnitObject grid) => _objectPoolService.ReturnObject(grid);
        public void ReleaseLaneUnit(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseLaneUnitsByType<T>() where T : BaseLaneUnitObject => _objectPoolService.ReturnObjectsByType<T>();
        public void RemoveLaneUnitPoolByType<T>() where T : BaseLaneUnitObject => _objectPoolService.RemovePoolsByType<T>();
    }
}