using Game.Level.Configs;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "GameplayConfigContainer", menuName = "Game/Containers/GameplayConfigContainer")]
    public class GameplayConfigContainerSO : ScriptableObject
    {
        [field: SerializeField] public ColorPaletteSO ColorPalette { get; private set; }
    }
}
