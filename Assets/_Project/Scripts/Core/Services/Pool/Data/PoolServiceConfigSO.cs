using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(fileName = "PoolServiceConfig", menuName = "Game/Services/PoolServiceConfig")]
    public sealed class PoolServiceConfigSO : ScriptableObject
    {
        [field: SerializeField] public PooledAssetConfigSO[] PooledAssets { get; private set; }
    }
}