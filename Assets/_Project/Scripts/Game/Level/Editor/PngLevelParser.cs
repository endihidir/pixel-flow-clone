using System.Collections.Generic;
using UnityEngine;
using Game.Level.Data;
using Game.Level.Configs;

namespace Game.Level.EditorTools
{
    /// <summary>
    /// Parses a Texture2D into a flat ColorId grid.
    /// Pipeline: bounding box -> aspect-aware target size -> downsample per cell -> palette snap.
    /// </summary>
    public static class PngLevelParser
    {
        public struct Result
        {
            public int Width;
            public int Height;
            public ColorId[] Cubes;
        }

        public static Result Parse(Texture2D tex, LevelEditorConfigSO config)
        {
            if (tex == null) throw new System.ArgumentNullException(nameof(tex));
            if (config == null) throw new System.ArgumentNullException(nameof(config));
            if (config.Palette == null) throw new System.InvalidOperationException("Palette missing on LevelImportConfigSO.");

            var pixels = tex.GetPixels();
            int srcW = tex.width;
            int srcH = tex.height;

            // 1) Bounding box of opaque pixels.
            if (!TryFindOpaqueBounds(pixels, srcW, srcH, config.AlphaThreshold,
                    out int xMin, out int yMin, out int xMax, out int yMax))
            {
                // Empty image -> empty level.
                return new Result { Width = 0, Height = 0, Cubes = new ColorId[0] };
            }

            int boundsW = xMax - xMin + 1;
            int boundsH = yMax - yMin + 1;

            // 2) Aspect-aware target size, longest side = MaxGridSize.
            int targetW, targetH;
            if (boundsW >= boundsH)
            {
                targetW = config.MaxGridSize;
                targetH = Mathf.Max(1, Mathf.RoundToInt(boundsH * (float)config.MaxGridSize / boundsW));
            }
            else
            {
                targetH = config.MaxGridSize;
                targetW = Mathf.Max(1, Mathf.RoundToInt(boundsW * (float)config.MaxGridSize / boundsH));
            }

            // 3) Per-cell sampling.
            var cubes = new ColorId[targetW * targetH];
            float cellW = boundsW / (float)targetW;
            float cellH = boundsH / (float)targetH;

            for (int cy = 0; cy < targetH; cy++)
            {
                int py0 = yMin + Mathf.FloorToInt(cy * cellH);
                int py1 = yMin + Mathf.Max(py0 + 1, Mathf.FloorToInt((cy + 1) * cellH));
                if (py1 > yMax + 1) py1 = yMax + 1;

                for (int cx = 0; cx < targetW; cx++)
                {
                    int px0 = xMin + Mathf.FloorToInt(cx * cellW);
                    int px1 = xMin + Mathf.Max(px0 + 1, Mathf.FloorToInt((cx + 1) * cellW));
                    if (px1 > xMax + 1) px1 = xMax + 1;

                    int totalPixels = (px1 - px0) * (py1 - py0);
                    if (totalPixels <= 0)
                    {
                        cubes[cy * targetW + cx] = ColorId.None;
                        continue;
                    }

                    // Count opaque pixels and tally ColorId votes.
                    int opaqueCount = 0;
                    var votes = new Dictionary<ColorId, int>(8);

                    for (int py = py0; py < py1; py++)
                    {
                        for (int px = px0; px < px1; px++)
                        {
                            var c = pixels[py * srcW + px];
                            if (c.a < config.AlphaThreshold) continue;
                            opaqueCount++;
                            var id = config.Palette.FindClosest(c);
                            if (votes.TryGetValue(id, out int n)) votes[id] = n + 1;
                            else votes[id] = 1;
                        }
                    }

                    float opaqueRatio = opaqueCount / (float)totalPixels;
                    if (opaqueRatio < config.CellOpaqueRatio || votes.Count == 0)
                    {
                        cubes[cy * targetW + cx] = ColorId.None;
                        continue;
                    }

                    // Mode (most-voted ColorId).
                    ColorId best = ColorId.None;
                    int bestVotes = -1;
                    foreach (var kv in votes)
                    {
                        if (kv.Value > bestVotes) { bestVotes = kv.Value; best = kv.Key; }
                    }
                    cubes[cy * targetW + cx] = best;
                }
            }

            // Note: Texture2D origin is bottom-left. Our flat grid index is y * Width + x,
            // so cy=0 corresponds to the bottom row. Flip if you want top-left semantics:
            FlipVertical(cubes, targetW, targetH);

            return new Result { Width = targetW, Height = targetH, Cubes = cubes };
        }

        private static bool TryFindOpaqueBounds(Color[] pixels, int w, int h, float alphaThreshold,
            out int xMin, out int yMin, out int xMax, out int yMax)
        {
            xMin = int.MaxValue; yMin = int.MaxValue;
            xMax = int.MinValue; yMax = int.MinValue;
            bool any = false;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (pixels[y * w + x].a < alphaThreshold) continue;
                    if (x < xMin) xMin = x;
                    if (y < yMin) yMin = y;
                    if (x > xMax) xMax = x;
                    if (y > yMax) yMax = y;
                    any = true;
                }
            }
            return any;
        }

        private static void FlipVertical(ColorId[] cubes, int w, int h)
        {
            for (int y = 0; y < h / 2; y++)
            {
                int yMirror = h - 1 - y;
                for (int x = 0; x < w; x++)
                {
                    int a = y * w + x;
                    int b = yMirror * w + x;
                    var tmp = cubes[a];
                    cubes[a] = cubes[b];
                    cubes[b] = tmp;
                }
            }
        }
    }
}
