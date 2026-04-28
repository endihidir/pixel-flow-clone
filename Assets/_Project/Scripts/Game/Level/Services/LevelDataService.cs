using System.Collections.Generic;
using UnityEngine;
using Game.Level.Data;
using Game.Level.Configs;

namespace Game.Level.Services
{
    public sealed class LevelDataService : ILevelDataService
    {
        private readonly LevelDataServiceConfigSO _config;
        private LevelDefinition[] _levels;

        public bool IsInitialized { get; private set; }
        public int LevelCount => _levels?.Length ?? 0;

        public LevelDataService(LevelDataServiceConfigSO config)
        {
            _config = config;
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            // Resources.LoadAll<TextAsset> from the configured folder.
            var assets = Resources.LoadAll<TextAsset>(_config.ResourcesFolder);
            var list = new List<LevelDefinition>(assets.Length);

            // Sort by name so level_1, level_2, ... are in order.
            System.Array.Sort(assets, (a, b) => string.CompareOrdinal(a.name, b.name));

            for (int i = 0; i < assets.Length; i++)
            {
                try
                {
                    var def = JsonUtility.FromJson<LevelDefinition>(assets[i].text);
                    if (def != null) list.Add(def);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[LevelDataService] Failed to parse {assets[i].name}: {e.Message}");
                }
            }

            _levels = list.ToArray();
            IsInitialized = true;
        }

        public LevelDefinition GetLevel(int index)
        {
            if (!IsInitialized)
                throw new System.InvalidOperationException("LevelDataService not initialized.");
            if (index < 0 || index >= _levels.Length)
                throw new System.ArgumentOutOfRangeException(nameof(index));
            return _levels[index];
        }
    }
}
