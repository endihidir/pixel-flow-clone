using UnityEngine;

namespace Core.Pool.Services
{
    public interface IObjectPoolService
    {
        IObjectPoolService Initialize();
        T GetObject<T>(T prefab, bool activate = true, int poolCount = 1, bool showLogs = false) where T : Component;
        T GetObject<T>(bool activate = true, bool showLogs = false) where T : Component, IPooledObject;
        
        void ReturnObject<T>(T objectRef, bool deactivate = true, bool showLogs = false) where T : Component;
        void ReturnObjectsByType<T>(bool deactivate = true) where T : Component, IPooledObject;
        void ReturnAll(bool deactivate = true);
        
        void RemovePoolsByType<T>() where T : IPooledObject;
        void RemovePoolsByPrefab<T>(T prefab) where T : Component, IPooledObject;
        void RemoveAllPools();
    }
}