using Core.Pool.Services;
using Game.Configs;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Bootstrappers
{
    public class GameBootstrapper : MonoBehaviour
    {
        [field: SerializeField] private GameConfigContainerSO GameConfigContainer { get; set; }
        
        
        private void Awake()
        {
            Application.targetFrameRate = GameConfigContainer.TargetFrameRate;
            Input.multiTouchEnabled = GameConfigContainer.IsMultitouchEnabled;
            IObjectPoolService poolService = new ObjectPoolService(GameConfigContainer.PoolServiceConfig).Initialize();
        }
    }
}