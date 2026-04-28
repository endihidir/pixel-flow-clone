using UnityEngine;
using System.Linq;

namespace Core.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// This method is used to hide the GameObject in the Hierarchy view.
        /// </summary>
        public static void HideInHierarchy(this GameObject gameObject)
        {
            gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
        
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var component)) component = gameObject.AddComponent<T>();

            return component;
        }
        
        public static T GetOrAddComponent<T>(this Component obj) where T : Component
        {
            if (!obj.TryGetComponent<T>(out var component)) component = obj.gameObject.AddComponent<T>();

            return component;
        }
        
        public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;
        
        /// <summary>
        /// Returns the hierarchical path in the Unity scene hierarchy for this GameObject.
        /// </summary>
        public static string Path(this GameObject gameObject)
        {
            return "/" + string.Join("/", gameObject.GetComponentsInParent<Transform>().Select(t => t.name).Reverse().ToArray());
        }

        /// <summary>
        /// Returns the full hierarchical path in the Unity scene hierarchy for this GameObject.
        /// </summary>

        public static string PathFull(this GameObject gameObject)
        {
            return gameObject.Path() + "/" + gameObject.name;
        }
    }
}