using System;
using System.Collections.Generic;
using Game.Level.Data;

namespace Game.Level.EditorTools
{
    public static class GreedyLevelValidator
    {
        public static bool IsSolvable(string[] pixels, LaneJson[] lanes, int unitSlotSize)
        {
            var counts = new Dictionary<ColorId, int>();
            for (int i = 0; i < pixels.Length; i++)
            {
                var id = ParseColor(pixels[i]);
                if (id == ColorId.None) continue;
                if (counts.TryGetValue(id, out int n)) counts[id] = n + 1;
                else counts[id] = 1;
            }

            int[] laneIdx = new int[lanes.Length];
            var unitSlots = new List<LaneUnitJson>(unitSlotSize + 1);

            int boardTotal = 0;
            foreach (var c in counts.Values) boardTotal += c;

            int safety = 0;
            int safetyMax = SumLaneUnits(lanes) + unitSlotSize + 10;

            while (boardTotal > 0)
            {
                if (++safety > safetyMax * 4) return false;
                if (TryFireFromUnitSlot(unitSlots, counts, ref boardTotal)) continue;

                int bestLane = -1;
                int bestScore = -1;
                for (int li = 0; li < lanes.Length; li++)
                {
                    if (laneIdx[li] >= lanes[li].laneUnits.Length) continue;
                    var laneUnit = lanes[li].laneUnits[laneIdx[li]];
                    var color = ParseColor(laneUnit.color);
                    int avail = counts.GetValueOrDefault(color, 0);
                    int score = Math.Min(laneUnit.ammo, avail);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestLane = li;
                    }
                }

                if (bestLane == -1)
                {
                    while (TryFireFromUnitSlot(unitSlots, counts, ref boardTotal)) { }
                    return boardTotal == 0;
                }

                if (bestScore == 0)
                {
                    bestLane = -1;
                    for (int li = 0; li < lanes.Length; li++)
                    {
                        if (laneIdx[li] < lanes[li].laneUnits.Length) { bestLane = li; break; }
                    }
                    if (bestLane == -1) return boardTotal == 0;
                }

                var fired = lanes[bestLane].laneUnits[laneIdx[bestLane]];
                laneIdx[bestLane]++;
                var firedColor = ParseColor(fired.color);
                int destroyed = ConsumeColor(counts, firedColor, fired.ammo);
                boardTotal -= destroyed;
                int leftover = fired.ammo - destroyed;

                if (leftover > 0)
                {
                    if (unitSlots.Count >= unitSlotSize) return false;
                    unitSlots.Add(new LaneUnitJson { color = fired.color, ammo = leftover });
                }

                while (TryFireFromUnitSlot(unitSlots, counts, ref boardTotal)) { }
            }

            return true;
        }

        private static bool TryFireFromUnitSlot(List<LaneUnitJson> unitSlots, Dictionary<ColorId, int> counts, ref int boardTotal)
        {
            int bestIdx = -1;
            int bestScore = 0;
            for (int i = 0; i < unitSlots.Count; i++)
            {
                var p = unitSlots[i];
                var color = ParseColor(p.color);
                int avail = counts.GetValueOrDefault(color, 0);
                int score = Math.Min(p.ammo, avail);
                if (score > bestScore) { bestScore = score; bestIdx = i; }
            }
            if (bestIdx < 0) return false;

            var laneUnit = unitSlots[bestIdx];
            unitSlots.RemoveAt(bestIdx);
            var laneUnitColor = ParseColor(laneUnit.color);
            int destroyed = ConsumeColor(counts, laneUnitColor, laneUnit.ammo);
            boardTotal -= destroyed;
            int leftover = laneUnit.ammo - destroyed;
            if (leftover > 0) unitSlots.Add(new LaneUnitJson { color = laneUnit.color, ammo = leftover });
            return true;
        }

        private static int ConsumeColor(Dictionary<ColorId, int> counts, ColorId color, int ammo)
        {
            if (!counts.TryGetValue(color, out int n) || n <= 0) return 0;
            int destroyed = Math.Min(ammo, n);
            counts[color] = n - destroyed;
            return destroyed;
        }

        private static int SumLaneUnits(LaneJson[] lanes)
        {
            int s = 0;
            for (int i = 0; i < lanes.Length; i++) s += lanes[i].laneUnits?.Length ?? 0;
            return s;
        }

        private static ColorId ParseColor(string value)
        {
            return Enum.TryParse(value, out ColorId id) ? id : ColorId.None;
        }
    }
}