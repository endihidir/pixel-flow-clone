using UnityEngine;

namespace Core.Pool.Services
{
    [DisallowMultipleComponent]
    public class PooledObject : MonoBehaviour, IPooledObject
    {
        public int PoolKey { get; set; }
        public virtual bool IsActive => gameObject.activeInHierarchy;

        public virtual void Activate()
        {
            gameObject.SetActive(true);
            OnActivate();
        }

        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
            OnDeactivate();
        }
        
        protected virtual void OnActivate() { }
        protected virtual void OnDeactivate() { }
    }
}