using System.IO;
using UnityEngine;
using Game.Level.Data;

namespace Game.Level.EditorTools
{
    /// <summary>
    /// Orchestrates the full level import pipeline:
    /// PNG -> parse -> generate pigs -> greedy-validate -> retry on fail -> save JSON.
    /// </summary>
    public static class LevelImporter
    {
        private const int LaneMin = 3;
        private const int LaneMax = 5;

        public struct ImportResult
        {
            public bool Success;
            public string OutputPath;
            public int Attempts;
            public LevelDefinition Level;
            public string Error;
        }

        public static ImportResult Import(Texture2D pngAsset, int laneCount, int difficultyIndex, LevelEditorConfigSO config, string outputFileName)
        {
            if (config == null)
                return new ImportResult { Success = false, Error = "Config is null." };
            if (config.Palette == null)
                return new ImportResult { Success = false, Error = "Palette missing on config." };
            if (config.DifficultyPresets == null || config.DifficultyPresets.Length == 0)
                return new ImportResult { Success = false, Error = "No difficulty presets configured." };
            if (difficultyIndex < 0 || difficultyIndex >= config.DifficultyPresets.Length)
                return new ImportResult { Success = false, Error = "Difficulty index out of range." };

            int clampedLanes = Mathf.Clamp(laneCount, LaneMin, LaneMax);
            var preset = config.DifficultyPresets[difficultyIndex];

            // Parse PNG once.
            var parsed = PngLevelParser.Parse(pngAsset, config);
            if (parsed.Cubes.Length == 0)
                return new ImportResult { Success = false, Error = "PNG has no opaque pixels." };

            // Generate + validate, retry on fail.
            int seedBase = pngAsset.name.GetHashCode() ^ difficultyIndex ^ clampedLanes;
            LaneData[] lanes = null;
            int attempts = 0;
            for (int i = 0; i < config.MaxValidationRetries; i++)
            {
                attempts++;
                var rng = new System.Random(seedBase + i);
                lanes = PigGenerator.Generate(parsed.Cubes, clampedLanes, preset, config, rng);
                if (GreedyLevelOracle.IsSolvable(parsed.Cubes, lanes, config.LevelDataServiceConfig.TraySize)) break;
                lanes = null;
            }

            if (lanes == null)
            {
                return new ImportResult
                {
                    Success = false,
                    Attempts = attempts,
                    Error = $"Could not generate solvable level after {attempts} attempts."
                };
            }

            var level = new LevelDefinition
            {
                Width = parsed.Width,
                Height = parsed.Height,
                Cubes = parsed.Cubes,
                Lanes = lanes
            };

            // Save JSON.
            if (!Directory.Exists(config.OutputFolder))
                Directory.CreateDirectory(config.OutputFolder);

            string path = Path.Combine(config.OutputFolder, outputFileName + ".json");
            File.WriteAllText(path, JsonUtility.ToJson(level, prettyPrint: false));

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            return new ImportResult
            {
                Success = true,
                Attempts = attempts,
                OutputPath = path,
                Level = level
            };
        }
    }
}