using System.IO;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.EditorTools
{
    public static class LevelImportPipeline
    {
        private const int LaneMin = 3;
        private const int LaneMax = 5;

        public struct ImportResult
        {
            public bool Success;
            public string OutputPath;
            public int Attempts;
            public LevelJson LevelJson;
            public string Error;
        }

        public static ImportResult Import(Texture2D pngAsset, int laneCount, int difficultyIndex, LevelEditorConfigSO config, string outputFileName, int levelNumber)
        {
            if (config == null) return new ImportResult { Success = false, Error = "Config is null." };
            if (config.Palette == null) return new ImportResult { Success = false, Error = "Palette missing on config." };
            if (config.DifficultyPresets == null || config.DifficultyPresets.Length == 0) return new ImportResult { Success = false, Error = "No difficulty presets configured." };
            if (difficultyIndex < 0 || difficultyIndex >= config.DifficultyPresets.Length) return new ImportResult { Success = false, Error = "Difficulty index out of range." };

            int clampedLanes = Mathf.Clamp(laneCount, LaneMin, LaneMax);
            var preset = config.DifficultyPresets[difficultyIndex];

            var parsed = PngLevelParser.Parse(pngAsset, config);
            if (parsed.Cubes.Length == 0) return new ImportResult { Success = false, Error = "PNG has no opaque pixels." };

            var pixels = ToPixelJson(parsed.Cubes);
            int seedBase = pngAsset.name.GetHashCode() ^ difficultyIndex ^ clampedLanes;
            LaneJson[] lanes = null;
            int attempts = 0;
            for (int i = 0; i < config.MaxValidationRetries; i++)
            {
                attempts++;
                var rng = new System.Random(seedBase + i);
                lanes = LaneGenerator.Generate(parsed.Cubes, clampedLanes, preset, config, rng);
                if (GreedyLevelValidator.IsSolvable(pixels, lanes, config.LevelDataServiceConfig.TraySize)) break;
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

            var levelJson = new LevelJson
            {
                level_number = levelNumber,
                grid_width = parsed.Width,
                grid_height = parsed.Height,
                pixels = pixels,
                lanes = lanes
            };

            if (!Directory.Exists(config.OutputFolder)) Directory.CreateDirectory(config.OutputFolder);

            string path = Path.Combine(config.OutputFolder, outputFileName + ".json");
            File.WriteAllText(path, LevelJsonConverter.ToJson(levelJson, prettyPrint: false));

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            return new ImportResult
            {
                Success = true,
                Attempts = attempts,
                OutputPath = path,
                LevelJson = levelJson
            };
        }

        private static string[] ToPixelJson(ColorId[] pixels)
        {
            var result = new string[pixels?.Length ?? 0];
            for (int i = 0; i < result.Length; i++) result[i] = pixels[i].ToString();
            return result;
        }
    }
}