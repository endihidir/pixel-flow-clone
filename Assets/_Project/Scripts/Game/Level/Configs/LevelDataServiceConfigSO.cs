using NaughtyAttributes;
using UnityEngine;

namespace Game.Level.Configs
{
    [CreateAssetMenu(fileName = "LevelDataServiceConfig", menuName = "Game/Level/Level Data Service Config")]
    public sealed class LevelDataServiceConfigSO : ScriptableObject
    {
        [field: SerializeField] public string ResourcesFolder { get; private set; } = "Levels";
        [field: SerializeField] public string FileNameFormat { get; private set; } = "level_{0}";
        [field: SerializeField] public int UnitSlotSize { get; private set; } = 5;
        
        [field: SerializeField, Header("GAMEPLAY TESTING")] public bool UseTestLevel { get; private set; } = false;
        [field: SerializeField, ShowIf(nameof(UseTestLevel))] public int TestLevelIndex { get; private set; }
        [field: SerializeField] public int TargetLevelIndex { get; private set; } = 0;
    }
}
