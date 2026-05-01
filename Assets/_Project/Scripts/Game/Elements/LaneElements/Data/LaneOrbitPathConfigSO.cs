using UnityEngine;

namespace Game.Lane.Configs
{
    [CreateAssetMenu(fileName = "LaneOrbitPathConfig", menuName = "Game/Lane/Lane Orbit Path Config")]
    public class LaneOrbitPathConfigSO : ScriptableObject
    {
        [field: SerializeField] public Vector3 OrbitOffset { get; private set; } = new (3f, 0f, 4f);
        [field: SerializeField] public float LaunchOffsetFromLeft { get; private set; } = 0f;
        
        [field: SerializeField, Range(0f, 2f)] public float CornerRadius { get; private set; } = 0.5f;
        [field: SerializeField, Range(0f, 2f)] public float CornerOutwardOffset { get; private set; } = 2f;
        [field: SerializeField, Range(1, 8)] public int CornerSegments { get; private set; } = 8;
    }
}