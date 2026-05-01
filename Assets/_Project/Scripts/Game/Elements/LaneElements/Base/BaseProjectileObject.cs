using Core.Pool.Services;
using Game.Modules;
using UnityEngine;

namespace Game.Lane.Item
{
    public abstract class BaseProjectileObject : PooledObject
    {
        [field: SerializeField] public ProjectileAnimationModule MoveAnimation { get; private set; }

        protected override void OnDeactivate() => MoveAnimation?.Dispose();
    }
}