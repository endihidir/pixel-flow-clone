using Core.Configs;
using Game.Level.Configs;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "GameConfigContainer", menuName = "Game/Containers/GameConfigContainer")]
    public class GameConfigContainerSO : ScriptableObject
    {
        [field: SerializeField] public int TargetFrameRate { get; private set; } = 60;
        [field: SerializeField] public bool IsMultitouchEnabled { get; private set; } = true;
        [field: SerializeField] public PoolServiceConfigSO PoolServiceConfig { get; private set; }
        [field: SerializeField] public LevelDataServiceConfigSO LevelDataServiceConfig { get; private set; }
        [field: SerializeField] public GameplayConfigContainerSO GameplayConfigContainer { get; private set; }
    }
}