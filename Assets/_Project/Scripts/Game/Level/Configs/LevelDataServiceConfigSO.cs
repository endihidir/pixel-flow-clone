using UnityEngine;

namespace Game.Level.Configs
{
    [CreateAssetMenu(fileName = "LevelDataServiceConfig", menuName = "Game/Level/Level Data Service Config")]
    public sealed class LevelDataServiceConfigSO : ScriptableObject
    {
        [field: SerializeField] public string ResourcesFolder { get; private set; } = "Levels";
        [field: SerializeField] public string FileNameFormat { get; private set; } = "level_{0}";
        [field: SerializeField] public int TraySize { get; private set; } = 5;
    }
}
