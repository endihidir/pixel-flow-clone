using UnityEngine;

namespace Game.Lane.Configs
{
    [CreateAssetMenu(fileName = "LaneViewConfig", menuName = "Game/Lane/Lane View Config")]
    public class LaneViewConfigSO : ScriptableObject
    {
        [Tooltip("Distance between lane roots on the horizontal axis.")]
        [field: SerializeField] public float LaneSpacing { get; private set; } = 5f;

        [Tooltip("Distance between units inside the same lane.")]
        [field: SerializeField] public float UnitOffset { get; private set; } = 5f;

        [Tooltip("Half width of the lane tap area on the local X axis.")]
        [field: SerializeField] public float TapPlaneHalfWidth { get; private set; } = 3f;

        [Tooltip("Half width of each unit tap area on the local X axis.")]
        [field: SerializeField] public float UnitTapHalfWidth { get; private set; } = 2f;

        [Tooltip("Half depth of each unit tap area on the local Z axis.")]
        [field: SerializeField] public float UnitTapHalfDepth { get; private set; } = 2f;

        [Tooltip("Duration of the local move animation when lane units advance forward.")]
        [field: SerializeField] public float AdvanceMoveDuration { get; private set; } = 0.18f;

        [Tooltip("Delay between each unit advance step animation.")]
        [field: SerializeField] public float AdvanceStepDelay { get; private set; } = 0.035f;
    }
}