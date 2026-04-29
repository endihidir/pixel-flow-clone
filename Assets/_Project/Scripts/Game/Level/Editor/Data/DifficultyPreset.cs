using System;
using UnityEngine;

namespace Game.Level.EditorTools
{
    [Serializable]
    public struct DifficultyPreset
    {
        [Tooltip("Display name of this difficulty preset (e.g. Easy, Medium, Hard).")]
        public string Name;

        [Tooltip("Target average ammo per laneUnit. Determines laneUnit count: laneUnitCount = ceil(cubeCount / AvgAmmo). " +
                 "Higher = fewer laneUnits with more ammo (easier). Lower = more laneUnits with less ammo (harder).")]
        public int AvgAmmo;

        [Tooltip("Maximum +/- ammo deviation applied during distribution. Total ammo per color is preserved. " +
                 "Higher = more varied laneUnit ammo values, more strategic depth.")]
        public int AmmoVariance;
    }
}
