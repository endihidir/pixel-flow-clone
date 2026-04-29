using System.Collections.Generic;
using UnityEngine;
using Game.Level.Data;

namespace Game.Level.EditorTools
{
    public static class LaneGenerator
    {
        public static LaneJson[] Generate(ColorId[] cubes, int laneCount, DifficultyPreset preset, LevelEditorConfigSO config, System.Random rng)
        {
            if (laneCount < 1) laneCount = 1;
            if (rng == null) rng = new System.Random();

            var colorCounts = new Dictionary<ColorId, int>();
            for (int i = 0; i < cubes.Length; i++)
            {
                var id = cubes[i];
                if (id == ColorId.None) continue;
                if (colorCounts.TryGetValue(id, out int n)) colorCounts[id] = n + 1;
                else colorCounts[id] = 1;
            }

            var allLaneUnits = new List<LaneUnitJson>();
            foreach (var kv in colorCounts)
            {
                ColorId color = kv.Key;
                int total = kv.Value;
                int laneUnitCount = Mathf.Max(1, Mathf.CeilToInt(total / (float)preset.AvgAmmo));

                int baseAmmo = total / laneUnitCount;
                int remainder = total - baseAmmo * laneUnitCount;
                var ammos = new int[laneUnitCount];
                for (int i = 0; i < laneUnitCount; i++)
                    ammos[i] = baseAmmo + (i < remainder ? 1 : 0);

                ApplyVariance(ammos, preset.AmmoVariance, config.MinAmmoPerLaneUnit, config.MaxAmmoPerLaneUnit, rng);

                for (int i = 0; i < laneUnitCount; i++)
                    allLaneUnits.Add(new LaneUnitJson { color = color.ToString(), ammo = ammos[i] });
            }

            Shuffle(allLaneUnits, rng);

            var lanes = new List<LaneUnitJson>[laneCount];
            for (int i = 0; i < laneCount; i++) lanes[i] = new List<LaneUnitJson>();
            for (int i = 0; i < allLaneUnits.Count; i++)
                lanes[i % laneCount].Add(allLaneUnits[i]);

            var result = new LaneJson[laneCount];
            for (int i = 0; i < laneCount; i++)
                result[i] = new LaneJson { laneUnits = lanes[i].ToArray() };

            return result;
        }

        private static void ApplyVariance(int[] ammos, int variance, int min, int max, System.Random rng)
        {
            if (variance <= 0) return;
            int n = ammos.Length;
            if (n < 2) return;

            int passes = n;
            for (int p = 0; p < passes; p++)
            {
                int i = rng.Next(n);
                int j = rng.Next(n);
                if (i == j) continue;

                int delta = rng.Next(1, variance + 1);
                int newI = ammos[i] + delta;
                int newJ = ammos[j] - delta;
                if (newI > max || newJ < min) continue;
                ammos[i] = newI;
                ammos[j] = newJ;
            }
        }

        private static void Shuffle<T>(List<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}