using System;

namespace Game.Level.Data
{
    [Serializable]
    public sealed class LevelJson
    {
        public int level_number;
        public int grid_width;
        public int grid_height;
        public string[] pixels;
        public LaneJson[] lanes;
    }
}