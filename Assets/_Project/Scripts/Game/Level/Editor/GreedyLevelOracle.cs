using System.Collections.Generic;
using Game.Level.Data;

namespace Game.Level.EditorTools
{
    /// <summary>
    /// Validates a generated level by simulating play with a greedy strategy:
    /// at each step, pick the lane whose front pig has the most matching cubes on the board.
    /// Pigs with leftover ammo go to tray; tray full -> FAIL; deadlock -> FAIL.
    /// </summary>
    public static class GreedyLevelOracle
    {
        public static bool IsSolvable(ColorId[] cubes, LaneData[] lanes, int traySize)
        {
            // Per-color cube counts.
            var counts = new Dictionary<ColorId, int>();
            for (int i = 0; i < cubes.Length; i++)
            {
                var id = cubes[i];
                if (id == ColorId.None) continue;
                if (counts.TryGetValue(id, out int n)) counts[id] = n + 1;
                else counts[id] = 1;
            }

            // Lane fronts as queues (index pointer).
            int[] laneIdx = new int[lanes.Length];

            // Tray: list of (color, leftoverAmmo).
            var tray = new List<PigData>(traySize + 1);

            int boardTotal = 0;
            foreach (var c in counts.Values) boardTotal += c;

            int safety = 0;
            int safetyMax = SumPigs(lanes) + traySize + 10;

            while (boardTotal > 0)
            {
                if (++safety > safetyMax * 4) return false;

                // 1) Try tray first if any tray pig can fire (avoid filling further).
                if (TryFireFromTray(tray, counts, ref boardTotal)) continue;

                // 2) Pick lane greedily.
                int bestLane = -1;
                int bestScore = -1;
                for (int li = 0; li < lanes.Length; li++)
                {
                    if (laneIdx[li] >= lanes[li].Pigs.Length) continue;
                    var pig = lanes[li].Pigs[laneIdx[li]];
                    int avail = counts.TryGetValue(pig.Color, out int n) ? n : 0;
                    int score = System.Math.Min(pig.Ammo, avail);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestLane = li;
                    }
                }

                if (bestLane == -1)
                {
                    // No more pigs in any lane.
                    // Final tray sweep.
                    while (TryFireFromTray(tray, counts, ref boardTotal)) { }
                    return boardTotal == 0;
                }

                if (bestScore == 0)
                {
                    // No lane front matches the board. We are forced to fire one anyway.
                    // Pick the first non-empty lane; pig will go fully to tray.
                    bestLane = -1;
                    for (int li = 0; li < lanes.Length; li++)
                    {
                        if (laneIdx[li] < lanes[li].Pigs.Length) { bestLane = li; break; }
                    }
                    if (bestLane == -1) return boardTotal == 0;
                }

                // Fire the chosen lane front.
                var fired = lanes[bestLane].Pigs[laneIdx[bestLane]];
                laneIdx[bestLane]++;
                int destroyed = ConsumeColor(counts, fired.Color, fired.Ammo);
                boardTotal -= destroyed;
                int leftover = fired.Ammo - destroyed;

                if (leftover > 0)
                {
                    if (tray.Count >= traySize) return false; // tray overflow
                    tray.Add(new PigData { Color = fired.Color, Ammo = leftover });
                }

                // After each fire, try to drain tray.
                while (TryFireFromTray(tray, counts, ref boardTotal)) { }
            }

            return true;
        }

        private static bool TryFireFromTray(List<PigData> tray, Dictionary<ColorId, int> counts, ref int boardTotal)
        {
            int bestIdx = -1;
            int bestScore = 0;
            for (int i = 0; i < tray.Count; i++)
            {
                var p = tray[i];
                int avail = counts.TryGetValue(p.Color, out int n) ? n : 0;
                int score = System.Math.Min(p.Ammo, avail);
                if (score > bestScore) { bestScore = score; bestIdx = i; }
            }
            if (bestIdx < 0) return false;

            var pig = tray[bestIdx];
            tray.RemoveAt(bestIdx);
            int destroyed = ConsumeColor(counts, pig.Color, pig.Ammo);
            boardTotal -= destroyed;
            int leftover = pig.Ammo - destroyed;
            if (leftover > 0) tray.Add(new PigData { Color = pig.Color, Ammo = leftover });
            return true;
        }

        private static int ConsumeColor(Dictionary<ColorId, int> counts, ColorId color, int ammo)
        {
            if (!counts.TryGetValue(color, out int n) || n <= 0) return 0;
            int destroyed = System.Math.Min(ammo, n);
            counts[color] = n - destroyed;
            return destroyed;
        }

        private static int SumPigs(LaneData[] lanes)
        {
            int s = 0;
            for (int i = 0; i < lanes.Length; i++) s += lanes[i].Pigs.Length;
            return s;
        }
    }
}
