using System;

namespace Game.Level.Data
{
    [Serializable]
    public sealed class LevelDefinition
    {
        public int Width;
        public int Height;
        public ColorId[] Cubes;   // flat: index = y * Width + x, ColorId.None = bos hucre
        public LaneData[] Lanes;
    }
}
