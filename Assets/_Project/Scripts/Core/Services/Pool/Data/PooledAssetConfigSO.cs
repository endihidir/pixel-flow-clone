using System.Linq;
using Core.Pool.Services;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(fileName = "PoolAsset", menuName = "Game/Assets/PooledAssetConfig")]
    public sealed class PooledAssetConfigSO : ScriptableObject
    {
        [field: SerializeField] public bool IsLazy { get; private set; } = true;
        [field: SerializeField] public int PoolSize {get; private set;}
        
        [field: SerializeField, Required, ValidateInput(nameof(HasDerivedPooledObject), "It must have a component derived from PooledObject component")]
        public GameObject PoolObject { get; private set; }
        
        private bool HasDerivedPooledObject(GameObject go)
        {
            if (!go) return true;

            var components = go.GetComponents<PooledObject>();

            return components.Any(comp => comp.GetType() != typeof(PooledObject));
        }
    }
}