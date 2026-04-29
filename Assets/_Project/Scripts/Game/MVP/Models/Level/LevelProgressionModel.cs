using System;
using Core.SaveSystem;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Models
{
    public sealed class LevelProgressionModel : ILevelProgressionModel
    {
        private const string SaveKey = "level_progression";

        private readonly IJsonSaveService _saveService;

        private LevelProgressionData _levelProgressionData;
        public int MaxLevelCount { get; private set; }
        public int CurrentLevelIndex => _levelProgressionData.currentLevelIndex;
        public int LevelCompletionCount => _levelProgressionData.levelCompletionCount;
        private bool ResetIndexOnLimit => true; //TODO: Get this form config
        public event Action OnLevelChanged;
        public LevelProgressionModel(IJsonSaveService saveService) => _saveService = saveService;

        public void Initialize(int maxLevelCount)
        {
            MaxLevelCount = maxLevelCount;
            
            var defaultState = new LevelProgressionData
            {
                currentLevelIndex = 0,
                levelCompletionCount = 0
            };
            
            _levelProgressionData = _saveService.LoadFromPrefs(SaveKey, defaultState);
            _levelProgressionData.currentLevelIndex = Mathf.Clamp(_levelProgressionData.currentLevelIndex, 0, MaxLevelCount - 1);
        }

        public void SetLevel(int levelIndex)
        {
            if (MaxLevelCount <= 0) return;

            var index = Mathf.Clamp(levelIndex, 0, MaxLevelCount - 1);
            
            if (index == _levelProgressionData.currentLevelIndex) return;

            _levelProgressionData.currentLevelIndex = index;
            
            RaiseChanged();
        }

        public void AdvanceLevel()
        {
            if (MaxLevelCount <= 0) return;

            var next = _levelProgressionData.currentLevelIndex + 1;
            
            next = next >= MaxLevelCount ? (ResetIndexOnLimit ? 0 : MaxLevelCount - 1) : next;
            
            _levelProgressionData.currentLevelIndex = next;
            
            _levelProgressionData.levelCompletionCount++;
            
            RaiseChanged();
        }

        public void ResetProgress()
        {
            _levelProgressionData.currentLevelIndex = 0;
            _levelProgressionData.levelCompletionCount = 0;
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            _saveService.SaveToPrefs(SaveKey, _levelProgressionData);
            OnLevelChanged?.Invoke();
        }
        
        public void OverrideSaveData(int levelIndex)
        {
            var data = new LevelProgressionData
            {
                currentLevelIndex = levelIndex,
                levelCompletionCount = levelIndex
            };
            
            _saveService.SaveToPrefs(SaveKey, data);
        }
    }
}