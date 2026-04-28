using System;
using System.Collections.Generic;
using System.Linq;
using Core.Configs;
using Core.Extensions;
using Core.Utils;
using UnityEngine;

namespace Core.Pool.Services
{
    public sealed class ObjectPoolService : IObjectPoolService, IDisposable
    {
        private const string ROOT_NAME = "PooledObjectsHolder";
        
        private readonly PoolServiceConfigSO _poolServiceConfigSo;
        
        private readonly IDictionary<int, ObjectPool> _idPools = new Dictionary<int, ObjectPool>();
        
        private readonly IDictionary<Type, ObjectPool> _typePools = new Dictionary<Type, ObjectPool>();
        
        private Transform _pooledObjectsRoot;
        public ObjectPoolService(PoolServiceConfigSO poolServiceConfigSo) => _poolServiceConfigSo = poolServiceConfigSo;

        public IObjectPoolService Initialize()
        {
            var root = GameObject.Find(ROOT_NAME) ?? new GameObject(ROOT_NAME);
            
            _pooledObjectsRoot = root.transform;
            
            CreateNonLazyPools();
            
            return this;
        }

        private void CreateNonLazyPools()
        {
            foreach (var pooledAssetConfig in _poolServiceConfigSo.PooledAssets)
            {
                if (pooledAssetConfig.IsLazy) continue;
                
                var objectPool = new ObjectPool(pooledAssetConfig.PoolObject, pooledAssetConfig.PoolSize, _pooledObjectsRoot);
                
                var key = pooledAssetConfig.PoolObject.GetComponent<IPooledObject>().GetType();
                
                _typePools.Add(key, objectPool);
            }
        }

        public T GetObject<T>(T prefab, bool activate = true, int poolCount = 1, bool showLogs = false) where T : Component
        {
            var key = prefab.gameObject.GetInstanceID();
            
            if (!_idPools.TryGetValue(key, out var objectPool))
            {
                objectPool = CreateNewPool(prefab.gameObject, poolCount);
                
                _idPools.Add(key, objectPool);
            }
            
            var pooledObject = objectPool.GetObject<T>(activate, showLogs);

            return pooledObject;
        }

        public T GetObject<T>(bool activate = true, bool showLogs = false) where T : Component, IPooledObject
        {
            var key = typeof(T);

            if (!_typePools.TryGetValue(key, out var objectPool))
            {
                objectPool = CreateNewPool<T>();
                
                _typePools.Add(key, objectPool);
            }
            
            var pooledObject = objectPool.GetObject<T>(activate, showLogs);

            return pooledObject;
        }

        public void ReturnObject<T>(T objectRef, bool deactivate = true, bool showLogs = false) where T : Component
        {
            if (!objectRef)
            {
                if(showLogs)
                    EditorLogger.LogError($"[{GetType().Name}] Return failed: null/destroyed object"); 
                
                return;
            }

            if (!objectRef.TryGetComponent<IPooledObject>(out var pooledObject))
            {
                if(showLogs)
                    EditorLogger.LogError($"[{GetType().Name}] Return failed: object is not IPooledObject.");
                return;
            }

            if (pooledObject == null) return;

            var idKey = pooledObject.PoolKey;
            var typeKey = pooledObject.GetType();
            
            if (!_idPools.TryGetValue(idKey, out var objectPool))
            {
                if (!_typePools.TryGetValue(typeKey, out objectPool))
                {
                    if(showLogs)
                        EditorLogger.LogError($"[{GetType().Name}] Return failed: no pool for '{objectRef.name}' ({objectRef.GetType().Name}).");
                    
                    return;
                }
            }
            
            objectPool.ReturnObject(pooledObject, deactivate);
        }
        
        public void ReturnObjectsByType<T>(bool deactivate = true) where T : Component, IPooledObject
        {
            var pooledObjects = PoolSearchUtils.FindPooledObjectsOfType<T>();
            
            foreach (var pooledObject in pooledObjects)
            {
                ReturnObject(pooledObject, deactivate);
            }
        }
        
        public void ReturnAll(bool deactivate = true)
        {
            var pooledObjects = PoolSearchUtils.FindPooledObjectsOfType<IPooledObject>();
            
            foreach (var pooledObject in pooledObjects)
            {
                if (pooledObject is Component component)
                {
                    ReturnObject(component, deactivate);
                }
            }
        }
        
        public void RemovePoolsByType<T>() where T : IPooledObject
        {
            var baseType = typeof(T);

            var keysToRemove = new List<Type>();

            foreach (var kvp in _typePools)
            {
                if (!baseType.IsAssignableFrom(kvp.Key)) continue;
                
                kvp.Value.Dispose();
                
                keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
            {
                _typePools.Remove(key);
            }
        }

        public void RemovePoolsByPrefab<T>(T prefab) where T : Component, IPooledObject
        {
            var id = prefab.gameObject.GetInstanceID();

            var keysToRemove = new List<int>();

            foreach (var kvp in _idPools)
            {
                if (id != kvp.Key) continue;
                
                kvp.Value.Dispose();
                
                keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
            {
                _idPools.Remove(key);
            }
        }
        
        public void RemoveAllPools()
        {
            foreach (var kvp in _typePools)
            {
                kvp.Value?.Dispose();
            }

            _typePools.Clear();

            foreach (var kvp in _idPools)
            {
                kvp.Value?.Dispose();
            }

            _idPools.Clear();
        }
        
        private ObjectPool CreateNewPool(GameObject prefab, int startPoolCount)
        {
            prefab.GetOrAddComponent<PooledObject>();
            return new ObjectPool(prefab, startPoolCount, _pooledObjectsRoot);
        }

        private ObjectPool CreateNewPool<T>() where T : Component, IPooledObject
        {
            var poolData = _poolServiceConfigSo.PooledAssets;
            var pooledAssetConfig = poolData.FirstOrDefault(x => x.PoolObject.GetComponent<T>());

            if (!pooledAssetConfig)
            {
                EditorLogger.LogError("Required component not found!");
                return null;
            }
            
            var startPoolCount = pooledAssetConfig.PoolSize;
            return new ObjectPool(pooledAssetConfig.PoolObject, startPoolCount, _pooledObjectsRoot);
        }
        
        public void Dispose() => RemoveAllPools();
    }
}