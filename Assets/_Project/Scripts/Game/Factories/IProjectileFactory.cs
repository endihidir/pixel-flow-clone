using Game.Lane.Item;
using UnityEngine;

namespace Game.Factories
{
    public interface IProjectileFactory
    {
        T GetProjectile<T>() where T : BaseProjectileObject;
        void ReleaseProjectile(BaseProjectileObject grid);
        void ReleaseProjectile(Transform item);
        void ReleaseProjectileByType<T>() where T : BaseProjectileObject;
        void RemoveProjectilePoolByType<T>() where T : BaseProjectileObject;
    }
}