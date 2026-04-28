using System.Collections.Generic;
using Core.Extensions;
using Core.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Pool.Services
{
    public sealed class ObjectPool
    {
        private readonly GameObject _prefabObj;
        private readonly int _poolCount;
        private readonly Transform _rootTransform;
        private GameObject _poolParent;
        private Queue<IPooledObject> Pool { get; } = new();

        public ObjectPool(GameObject prefabObj, int poolCount, Transform rootTransform)
        {
            _prefabObj = prefabObj;
            _poolCount = poolCount;
            _rootTransform = rootTransform;
            CreatePoolParent();
            CreatePool();
        }

        private void CreatePool()
        {
            for (int i = 0; i < _poolCount; i++) 
                CreateNewObject(true);
        }

        public T GetObject<T>(bool activate = true, bool showLogs = false) where T : Component
        {
            if (HasAnyPooledMissing()) ClearPool();
            
            if (!_poolParent) CreatePoolParent();
            
            IPooledObject pooledObject = null;
            
            T component = null;
            
            while (Pool.TryDequeue(out var candidate))
            {
                if (candidate is Component c && !c) continue;
                pooledObject = candidate;
                break;
            }
            
            pooledObject ??= GetNewPooledObject();
            
            if (activate) 
                pooledObject?.Activate();
            
            switch (pooledObject)
            {
                case T tComp:
                    component = tComp;
                    break;
                case Component comp:
                    component = comp.GetComponent<T>();
                    if(showLogs)
                        EditorLogger.LogWarning($"[{GetType().Name}] Expected component '{typeof(T).Name}' not found on pooled object '{comp.gameObject.name}'.", component);
                    break;
                default:
                    EditorLogger.LogError($"[{GetType().Name}] IPooledObject is not a Component! Object: {pooledObject}");
                    break;
            }
            
            return component;
        }

        public void ReturnObject<T>(T pooledObject, bool deactivate = true) where T : IPooledObject
        {
            if (pooledObject.IsActive && deactivate)
            {
                pooledObject.Deactivate();
            }
            
            ReturnToPool(pooledObject);
        }

        private IPooledObject GetNewPooledObject()
        {
            CreateNewObject(false);
            return Pool.Dequeue();
        }

        private void CreateNewObject(bool onInitialize)
        {
            var objClone = Object.Instantiate(_prefabObj, _poolParent.transform);
            var pooledObject = objClone.GetOrAddComponent<PooledObject>();
            pooledObject.PoolKey = _prefabObj.GetInstanceID();
            if (onInitialize) pooledObject.Deactivate();
            Pool.Enqueue(pooledObject);
        }

        private void ReturnToPool(IPooledObject pooledObject)
        {
            if(!_poolParent) CreatePoolParent();

            if (pooledObject is not Component pooledObj)
            {
                EditorLogger.LogError($"{pooledObject.GetType().Name} is not Component!");
                return;
            }

            if (pooledObj && _poolParent)
            {
                pooledObj.transform.SetParent(_poolParent.transform, false);
                pooledObj.transform.localPosition = Vector3.zero;
            }
            
            Pool.Enqueue(pooledObject);
        }
        
        private bool HasAnyPooledMissing()
        {
            foreach (var po in Pool)
            {
                if (po == null) return true;
                if (po is Component c && !c) return true;
            }
            return false;
        }

        private void CreatePoolParent()
        {
            _poolParent = new GameObject("Pool_" + _prefabObj.name);
            _poolParent.transform.SetParent(_rootTransform);
        }
        
        private void ClearPool()
        {
            foreach (var pooledObject in Pool)
            {
                if (pooledObject is not Component pooledObj) continue;
                if (!pooledObj) continue;
                Object.Destroy(pooledObj.gameObject);
            }

            Pool?.Clear();
            
            if (!_poolParent) return;
            Object.Destroy(_poolParent);
        }

        public void Dispose() => ClearPool();
    }
}