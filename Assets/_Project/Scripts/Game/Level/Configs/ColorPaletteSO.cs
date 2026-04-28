using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Level.Data;
using NaughtyAttributes;

namespace Game.Level.Configs
{
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "Game/Level/Color Palette")]
    public sealed class ColorPaletteSO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public ColorId Id;
            public Color Color;
        }

        [field: SerializeField] public Entry[] Entries { get; private set; }

        public Color GetColor(ColorId id)
        {
            for (int i = 0; i < Entries.Length; i++)
                if (Entries[i].Id == id) return Entries[i].Color;
            return Color.magenta;
        }

        public ColorId FindClosest(Color sample)
        {
            float bestDistSq = float.MaxValue;
            ColorId best = ColorId.None;
            for (int i = 0; i < Entries.Length; i++)
            {
                var e = Entries[i];
                if (e.Id == ColorId.None) continue;
                float d = SqrDistance(sample, e.Color);
                if (d < bestDistSq) { bestDistSq = d; best = e.Id; }
            }
            return best;
        }

        private static float SqrDistance(Color a, Color b)
        {
            float dr = a.r - b.r, dg = a.g - b.g, db = a.b - b.b;
            return dr * dr + dg * dg + db * db;
        }
        
#if UNITY_EDITOR
        [Button("Fill All ColorIds With Defaults")]
        private void FillDefaults()
        {
            var existing = new Dictionary<ColorId, Color>();
            if (Entries != null)
                for (int i = 0; i < Entries.Length; i++)
                    existing[Entries[i].Id] = Entries[i].Color;

            var values = (ColorId[])Enum.GetValues(typeof(ColorId));
            var list = new List<Entry>(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                var id = values[i];
                if (id == ColorId.None) continue;
                var color = existing.TryGetValue(id, out var c) ? c : DefaultColorFor(id);
                list.Add(new Entry { Id = id, Color = color });
            }

            Entries = list.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [Button("Reset All To Defaults")]
        private void ResetDefaults()
        {
            var values = (ColorId[])Enum.GetValues(typeof(ColorId));
            var list = new List<Entry>(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                var id = values[i];
                if (id == ColorId.None) continue;
                list.Add(new Entry { Id = id, Color = DefaultColorFor(id) });
            }
            Entries = list.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private static Color DefaultColorFor(ColorId id) => id switch
        {
            ColorId.Red => new Color(0.86f, 0.12f, 0.12f),
            ColorId.Orange => new Color(1.00f, 0.55f, 0.10f),
            ColorId.Yellow => new Color(1.00f, 0.85f, 0.10f),
            ColorId.Green => new Color(0.25f, 0.75f, 0.30f),
            ColorId.Blue => new Color(0.20f, 0.45f, 0.90f),
            ColorId.Purple => new Color(0.55f, 0.25f, 0.75f),
            ColorId.Pink => new Color(0.95f, 0.55f, 0.75f),
            ColorId.White => new Color(0.95f, 0.95f, 0.95f),
            ColorId.Black => new Color(0.10f, 0.10f, 0.10f),
            _ => Color.magenta
        };
#endif
    }
}
