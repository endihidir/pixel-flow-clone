using System;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Utils
{
    public static class LevelJsonRuntimeUtils
    {
        public static LevelDefinition ParseToLevelDefinition(TextAsset jsonFile)
        {
            var levelJson = JsonUtility.FromJson<LevelJson>(jsonFile.text);
            return ConvertToLevelDefinition(levelJson);
        }

        public static LevelDefinition ConvertToLevelDefinition(LevelJson levelJson)
        {
            var pixels = new ColorId[levelJson.pixels?.Length ?? 0];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Enum.TryParse(levelJson.pixels[i], out ColorId id) ? id : ColorId.None;
            }

            var laneCount = levelJson.lanes?.Length ?? 0;
            var lanes = new LaneDefinition[laneCount];
            for (int i = 0; i < laneCount; i++)
            {
                var laneJson = levelJson.lanes[i];
                var laneUnitCount = laneJson?.laneUnits?.Length ?? 0;
                var laneUnits = new LaneUnitDefinition[laneUnitCount];

                for (int j = 0; j < laneUnitCount; j++)
                {
                    var laneUnitJson = laneJson.laneUnits[j];
                    laneUnits[j] = new LaneUnitDefinition(
                        Enum.TryParse(laneUnitJson.color, out ColorId c) ? c : ColorId.None,
                        laneUnitJson.ammo);
                }

                lanes[i] = new LaneDefinition(laneUnits);
            }

            return new LevelDefinition(levelJson.level_number, levelJson.grid_width, levelJson.grid_height, pixels, lanes);
        }
    }
}