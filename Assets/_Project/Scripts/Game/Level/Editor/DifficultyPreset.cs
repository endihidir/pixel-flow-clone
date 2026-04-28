using System;
using UnityEngine;

namespace Game.Level.EditorTools
{
    [Serializable]
    public struct DifficultyPreset
    {
        [Tooltip("Display name of this difficulty preset (e.g. Easy, Medium, Hard).")]
        public string Name;

        [Tooltip("Target average ammo per pig. Determines pig count: pigCount = ceil(cubeCount / AvgAmmo). " +
                 "Higher = fewer pigs with more ammo (easier). Lower = more pigs with less ammo (harder).")]
        public int AvgAmmo;

        [Tooltip("Maximum +/- ammo deviation applied during distribution. Total ammo per color is preserved. " +
                 "Higher = more varied pig ammo values, more strategic depth.")]
        public int AmmoVariance;
    }
}
