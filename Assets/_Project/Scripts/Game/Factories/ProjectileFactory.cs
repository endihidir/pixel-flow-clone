using Core.Pool.Services;
using Game.Lane.Item;
using UnityEngine;

namespace Game.Factories
{
    public sealed class ProjectileFactory : IProjectileFactory
    {
        private readonly IObjectPoolService _objectPoolService;

        public ProjectileFactory(IObjectPoolService objectPoolService) => _objectPoolService = objectPoolService;

        public T GetProjectile<T>() where T : BaseProjectileObject => _objectPoolService.GetObject<T>();
        public void ReleaseProjectile(BaseProjectileObject grid) => _objectPoolService.ReturnObject(grid);
        public void ReleaseProjectile(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseProjectileByType<T>() where T : BaseProjectileObject => _objectPoolService.ReturnObjectsByType<T>();
        public void RemoveProjectilePoolByType<T>() where T : BaseProjectileObject => _objectPoolService.RemovePoolsByType<T>();
    }
}