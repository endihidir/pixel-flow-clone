using UnityEngine;

namespace Game.Lane.Configs
{
    [CreateAssetMenu(fileName = "LaneViewConfig", menuName = "Game/Lane/Lane View Config")]
    public class LaneViewConfigSO : ScriptableObject
    {
        [field: SerializeField] public float LaneSpacing { get; private set; } = 5f;
        [field: SerializeField] public float UnitOffset { get; private set; } = 5f;
        [field: SerializeField] public float TapPlaneHalfWidth { get; private set; } = 3f;
    }
}