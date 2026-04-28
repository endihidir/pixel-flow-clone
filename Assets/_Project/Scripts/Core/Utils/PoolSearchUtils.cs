using System.Collections.Generic;
using System.Linq;
using Core.Pool.Services;
using UnityEngine;

namespace Core.Utils
{
    public static class PoolSearchUtils
    {
        public static IEnumerable<T> FindPooledObjectsOfType<T>(bool includeInactive = false) where T : IPooledObject
        {
            return Object.FindObjectsOfType<MonoBehaviour>(includeInactive)
                         .OfType<T>()
                         .Where(p => includeInactive || p.IsActive);
        }
    }
}