using Core.Pool.Services;
using Core.SaveSystem;
using Game.Configs;
using Game.Level.Models;
using Game.Level.Services;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Bootstrappers
{
    public class GameBootstrapper : MonoBehaviour
    {
        [field: SerializeField] private GameConfigContainerSO GameConfigContainer { get; set; }
        [field: SerializeField] private GameplayBootstrapper GameplayBootstrapper { get; set; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GameplayBootstrapper != null) return;

            GameplayBootstrapper = GetComponentInChildren<GameplayBootstrapper>();

            if (GameplayBootstrapper) EditorUtility.SetDirty(this);
        }
#endif

        private void Awake()
        {
            Application.targetFrameRate = GameConfigContainer.TargetFrameRate;
            Input.multiTouchEnabled = GameConfigContainer.IsMultitouchEnabled;

            IObjectPoolService poolService = new ObjectPoolService(GameConfigContainer.PoolServiceConfig).Initialize();
            IJsonSaveService jsonSaveService = new JsonSaveService();
            
            PopulateLevelData(jsonSaveService, out var levelDefinitionProvider);
            GameplayBootstrapper?.Initialize(poolService, levelDefinitionProvider, GameConfigContainer.GameplayConfigContainer);
        }

        private void PopulateLevelData(IJsonSaveService jsonSaveService, out ILevelDefinitionProvider levelDefinitionProvider)
        {
            ILevelDataService levelDataService = new LevelDataService(GameConfigContainer.LevelDataServiceConfig);
            levelDataService.Initialize();
            ILevelProgressionModel levelProgressionModel = new LevelProgressionModel(jsonSaveService);
            levelProgressionModel.Initialize(levelDataService.LevelCount);
            levelDefinitionProvider = new LevelDefinitionProvider(levelDataService, levelProgressionModel);
        }
    }
}