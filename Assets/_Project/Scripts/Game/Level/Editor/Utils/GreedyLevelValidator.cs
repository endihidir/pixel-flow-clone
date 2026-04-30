using System;
using System.Collections.Generic;
using System.Text;
using Game.Level.Data;

namespace Game.Level.EditorTools
{
    public static class GreedyLevelValidator
    {
        public static bool IsSolvable(string[] pixels, LaneJson[] lanes, int unitSlotSize)
        {
            return IsSolvable(pixels, lanes, unitSlotSize, out _);
        }

        public static bool IsSolvable(string[] pixels, LaneJson[] lanes, int unitSlotSize, out string reason)
        {
            reason = null;

            if (!HasExactAmmoPixelBalance(pixels, lanes, out string balanceError))
            {
                reason = balanceError;
                return false;
            }

            var counts = new Dictionary<ColorId, int>();

            for (int i = 0; i < pixels.Length; i++)
            {
                ColorId id = ParseColor(pixels[i]);

                if (id == ColorId.None)
                    continue;

                if (counts.TryGetValue(id, out int count))
                    counts[id] = count + 1;
                else
                    counts[id] = 1;
            }

            int[] laneIndexes = new int[lanes.Length];
            var unitSlots = new List<LaneUnitJson>(unitSlotSize + 1);

            int boardTotal = 0;

            foreach (int count in counts.Values)
                boardTotal += count;

            int safety = 0;
            int safetyMax = SumLaneUnits(lanes) + unitSlotSize + 10;

            while (boardTotal > 0)
            {
                if (++safety > safetyMax * 4)
                {
                    reason = "Greedy validation safety limit reached.";
                    return false;
                }

                if (TryFireFromUnitSlot(unitSlots, counts, ref boardTotal))
                    continue;

                int bestLane = -1;
                int bestScore = -1;

                for (int laneIndex = 0; laneIndex < lanes.Length; laneIndex++)
                {
                    if (laneIndexes[laneIndex] >= lanes[laneIndex].laneUnits.Length)
                        continue;

                    LaneUnitJson laneUnit = lanes[laneIndex].laneUnits[laneIndexes[laneIndex]];
                    ColorId color = ParseColor(laneUnit.color);

                    int availablePixelCount = counts.GetValueOrDefault(color, 0);
                    int score = Math.Min(laneUnit.ammo, availablePixelCount);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestLane = laneIndex;
                    }
                }

                if (bestLane == -1)
                {
                    while (TryFireFromUnitSlot(unitSlots, counts, ref boardTotal)) { }

                    if (boardTotal == 0)
                        return true;

                    reason = "No lane unit left but board still has remaining pixels.";
                    return false;
                }

                if (bestScore == 0)
                {
                    bestLane = -1;

                    for (int laneIndex = 0; laneIndex < lanes.Length; laneIndex++)
                    {
                        if (laneIndexes[laneIndex] < lanes[laneIndex].laneUnits.Length)
                        {
                            bestLane = laneIndex;
                            break;
                        }
                    }

                    if (bestLane == -1)
                    {
                        if (boardTotal == 0)
                            return true;

                        reason = "No usable lane unit found.";
                        return false;
                    }
                }

                LaneUnitJson firedUnit = lanes[bestLane].laneUnits[laneIndexes[bestLane]];
                laneIndexes[bestLane]++;

                ColorId firedColor = ParseColor(firedUnit.color);
                int destroyed = ConsumeColor(counts, firedColor, firedUnit.ammo);
                boardTotal -= destroyed;

                int leftover = firedUnit.ammo - destroyed;

                if (leftover > 0)
                {
                    if (unitSlots.Count >= unitSlotSize)
                    {
                        reason = $"Unit slot is full. Color: {firedColor}, Leftover ammo: {leftover}";
                        return false;
                    }

                    unitSlots.Add(new LaneUnitJson { color = firedUnit.color, ammo = leftover });
                }

                while (TryFireFromUnitSlot(unitSlots, counts, ref boardTotal)) { }
            }

            return true;
        }

        private static bool HasExactAmmoPixelBalance(string[] pixels, LaneJson[] lanes, out string error)
        {
            error = null;

            var pixelCounts = new Dictionary<ColorId, int>();
            var ammoCounts = new Dictionary<ColorId, int>();

            for (int i = 0; i < pixels.Length; i++)
            {
                ColorId color = ParseColor(pixels[i]);

                if (color == ColorId.None)
                    continue;

                if (pixelCounts.TryGetValue(color, out int count))
                    pixelCounts[color] = count + 1;
                else
                    pixelCounts[color] = 1;
            }

            for (int i = 0; i < lanes.Length; i++)
            {
                LaneUnitJson[] laneUnits = lanes[i].laneUnits;

                if (laneUnits == null)
                    continue;

                for (int j = 0; j < laneUnits.Length; j++)
                {
                    ColorId color = ParseColor(laneUnits[j].color);

                    if (color == ColorId.None)
                        continue;

                    int ammo = laneUnits[j].ammo;

                    if (ammoCounts.TryGetValue(color, out int count))
                        ammoCounts[color] = count + ammo;
                    else
                        ammoCounts[color] = ammo;
                }
            }

            StringBuilder builder = null;

            foreach (var pair in pixelCounts)
            {
                ammoCounts.TryGetValue(pair.Key, out int ammoCount);

                if (ammoCount == pair.Value)
                    continue;

                builder ??= new StringBuilder();

                int difference = ammoCount - pair.Value;

                if (difference > 0)
                    builder.AppendLine($"{pair.Key}: Ammo is {difference} higher than pixel count. Pixel: {pair.Value}, Ammo: {ammoCount}");
                else
                    builder.AppendLine($"{pair.Key}: Ammo is {-difference} lower than pixel count. Pixel: {pair.Value}, Ammo: {ammoCount}");
            }

            foreach (var pair in ammoCounts)
            {
                if (pixelCounts.ContainsKey(pair.Key))
                    continue;

                builder ??= new StringBuilder();
                builder.AppendLine($"{pair.Key}: Ammo exists but no pixels found for this color. Ammo: {pair.Value}");
            }

            if (builder == null)
                return true;

            error = builder.ToString();
            return false;
        }

        private static bool TryFireFromUnitSlot(List<LaneUnitJson> unitSlots, Dictionary<ColorId, int> counts, ref int boardTotal)
        {
            int bestIndex = -1;
            int bestScore = 0;

            for (int i = 0; i < unitSlots.Count; i++)
            {
                LaneUnitJson laneUnit = unitSlots[i];
                ColorId color = ParseColor(laneUnit.color);

                int availablePixelCount = counts.GetValueOrDefault(color, 0);
                int score = Math.Min(laneUnit.ammo, availablePixelCount);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0)
                return false;

            LaneUnitJson selectedUnit = unitSlots[bestIndex];
            unitSlots.RemoveAt(bestIndex);

            ColorId selectedColor = ParseColor(selectedUnit.color);
            int destroyed = ConsumeColor(counts, selectedColor, selectedUnit.ammo);
            boardTotal -= destroyed;

            int leftover = selectedUnit.ammo - destroyed;

            if (leftover > 0)
                unitSlots.Add(new LaneUnitJson { color = selectedUnit.color, ammo = leftover });

            return true;
        }

        private static int ConsumeColor(Dictionary<ColorId, int> counts, ColorId color, int ammo)
        {
            if (!counts.TryGetValue(color, out int count) || count <= 0)
                return 0;

            int destroyed = Math.Min(ammo, count);
            counts[color] = count - destroyed;

            return destroyed;
        }

        private static int SumLaneUnits(LaneJson[] lanes)
        {
            int sum = 0;

            for (int i = 0; i < lanes.Length; i++)
                sum += lanes[i].laneUnits?.Length ?? 0;

            return sum;
        }

        private static ColorId ParseColor(string value)
        {
            return Enum.TryParse(value, out ColorId id) ? id : ColorId.None;
        }
    }
}