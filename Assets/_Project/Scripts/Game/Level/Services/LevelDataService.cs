using System.Collections.Generic;
using Core.Utils;
using Game.Level.Configs;
using Game.Level.Data;
using Game.Level.Utils;
using UnityEngine;

namespace Game.Level.Services
{
    public sealed class LevelDataService : ILevelDataService
    {
        private readonly LevelDataServiceConfigSO _config;
        private LevelDefinition[] _levels;

        public bool IsInitialized { get; private set; }
        public int LevelCount => _levels?.Length ?? 0;
        
        public bool UseTestLevel => _config.UseTestLevel;
        public int TestLevelIndex => Mathf.Min(_config.TestLevelIndex, LevelCount - 1);

        public LevelDataService(LevelDataServiceConfigSO config)
        {
            _config = config;
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            var assets = Resources.LoadAll<TextAsset>(_config.ResourcesFolder);
            var list = new List<LevelDefinition>(assets.Length);

            foreach (var asset in assets)
            {
                try
                {
                    var definition = LevelJsonRuntimeUtils.ParseToLevelDefinition(asset);
                    if (definition != null) list.Add(definition);
                }
                catch (System.Exception e)
                {
                    EditorLogger.LogError($"[LevelDataService] Failed to parse {asset.name}: {e.Message}");
                }
            }

            list.Sort(static (a, b) => a.LevelNumber.CompareTo(b.LevelNumber));

            CheckDuplicates(list);

            _levels = list.ToArray();
            IsInitialized = true;
        }

        public LevelDefinition GetLevelDefinition(int index) => _levels[index];

        private static void CheckDuplicates(List<LevelDefinition> sortedList)
        {
            for (int i = 1; i < sortedList.Count; i++)
            {
                if (sortedList[i].LevelNumber == sortedList[i - 1].LevelNumber)
                    EditorLogger.LogError($"[LevelDataService] Duplicate LevelNumber: {sortedList[i].LevelNumber}");
            }
        }
    }
}