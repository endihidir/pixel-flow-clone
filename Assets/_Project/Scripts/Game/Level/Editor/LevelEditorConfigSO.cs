using UnityEngine;
using Game.Level.Configs;

namespace Game.Level.EditorTools
{
    [CreateAssetMenu(fileName = "LevelEditorConfig", menuName = "Game/Level/Level Editor Config")]
    public sealed class LevelEditorConfigSO : ScriptableObject
    {
        [field: SerializeField] public LevelDataServiceConfigSO LevelDataServiceConfig { get; private set; }
        
        [field: Header("Grid")]
        [field: SerializeField] public int MaxGridSize { get; private set; } = 35;

        [field: Header("Parsing")]
        [field: SerializeField, Range(0f, 1f)] public float AlphaThreshold { get; private set; } = 0.5f;
        [field: SerializeField, Range(0f, 1f)] public float CellOpaqueRatio { get; private set; } = 0.5f;
        [field: SerializeField] public ColorPaletteSO Palette { get; private set; }

        [field: Header("Pig Generation")]
        [field: SerializeField] public int MaxAmmoPerPig { get; private set; } = 40;
        [field: SerializeField] public int MinAmmoPerPig { get; private set; } = 3;
        [field: SerializeField] public int DefaultLaneCount { get; private set; } = 3;
        [field: SerializeField] public DifficultyPreset[] DifficultyPresets { get; private set; }

        [field: Header("Validation")]
        [field: SerializeField] public int MaxValidationRetries { get; private set; } = 50;

        [field: Header("Output")]
        [field: SerializeField] public string OutputFolder { get; private set; } = "Assets/_Project/Resources/Levels";
    }
}
