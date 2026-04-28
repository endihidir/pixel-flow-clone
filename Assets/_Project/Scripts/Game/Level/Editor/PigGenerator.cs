using System.Collections.Generic;
using UnityEngine;
using Game.Level.Data;

namespace Game.Level.EditorTools
{
    /// <summary>
    /// Generates LaneData[] from a parsed cube grid and a difficulty preset.
    /// All ammo is shuffled across lanes; difficulty controls AvgAmmo and Variance only.
    /// </summary>
    public static class PigGenerator
    {
        public static LaneData[] Generate(
            ColorId[] cubes,
            int laneCount,
            DifficultyPreset preset,
            LevelEditorConfigSO config,
            System.Random rng)
        {
            if (laneCount < 1) laneCount = 1;
            if (rng == null) rng = new System.Random();

            // 1) Count cubes per color.
            var colorCounts = new Dictionary<ColorId, int>();
            for (int i = 0; i < cubes.Length; i++)
            {
                var id = cubes[i];
                if (id == ColorId.None) continue;
                if (colorCounts.TryGetValue(id, out int n)) colorCounts[id] = n + 1;
                else colorCounts[id] = 1;
            }

            // 2) For each color: pig count = ceil(count / avgAmmo), distribute ammo with variance.
            var allPigs = new List<PigData>();
            foreach (var kv in colorCounts)
            {
                ColorId color = kv.Key;
                int total = kv.Value;
                int pigCount = Mathf.Max(1, Mathf.CeilToInt(total / (float)preset.AvgAmmo));

                // Even base distribution + remainder spread.
                int baseAmmo = total / pigCount;
                int remainder = total - baseAmmo * pigCount;
                var ammos = new int[pigCount];
                for (int i = 0; i < pigCount; i++)
                    ammos[i] = baseAmmo + (i < remainder ? 1 : 0);

                // Apply variance: add +-N to each, redistribute to keep total constant,
                // clamp to [MinAmmoPerPig, MaxAmmoPerPig].
                ApplyVariance(ammos, preset.AmmoVariance, config.MinAmmoPerPig, config.MaxAmmoPerPig, rng);

                for (int i = 0; i < pigCount; i++)
                    allPigs.Add(new PigData { Color = color, Ammo = ammos[i] });
            }

            // 3) Shuffle and distribute across lanes round-robin.
            Shuffle(allPigs, rng);

            var lanes = new List<PigData>[laneCount];
            for (int i = 0; i < laneCount; i++) lanes[i] = new List<PigData>();
            for (int i = 0; i < allPigs.Count; i++)
                lanes[i % laneCount].Add(allPigs[i]);

            var result = new LaneData[laneCount];
            for (int i = 0; i < laneCount; i++)
                result[i] = new LaneData { Pigs = lanes[i].ToArray() };

            return result;
        }

        private static void ApplyVariance(int[] ammos, int variance, int min, int max, System.Random rng)
        {
            if (variance <= 0) return;
            int n = ammos.Length;
            if (n < 2) return;

            // For each pair (i, j), shift a random delta from j to i.
            int passes = n; // a few passes to spread variance
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
