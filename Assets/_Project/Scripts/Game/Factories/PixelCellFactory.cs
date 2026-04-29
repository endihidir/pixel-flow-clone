using Core.Pool.Services;
using Game.Grid.Item;
using UnityEngine;

namespace Game.Factories
{
    public sealed class PixelCellFactory : IPixelCellFactory
    {
        private readonly IObjectPoolService _objectPoolService;

        public PixelCellFactory(IObjectPoolService objectPoolService) => _objectPoolService = objectPoolService;

        public T GetPixelCell<T>() where T : BasePixelCellObject => _objectPoolService.GetObject<T>();
        public void ReleasePixelCell(BasePixelCellObject grid) => _objectPoolService.ReturnObject(grid);
        public void ReleasePixelCell(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleasePixelCellsByType<T>() where T : BasePixelCellObject => _objectPoolService.ReturnObjectsByType<T>();
        public void RemovePixelCellPoolByType<T>() where T : BasePixelCellObject => _objectPoolService.RemovePoolsByType<T>();
    }
}