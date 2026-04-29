using UnityEngine;

namespace Game.Level.Data
{
    public sealed class LevelDefinition
    {
        public int LevelNumber { get; }
        public Vector2Int GridSize { get; }
        public int Width => GridSize.x;
        public int Height => GridSize.y;
        public ColorId[] Pixels { get; }
        public LaneDefinition[] Lanes { get; }

        public LevelDefinition(int levelNumber, int width, int height, ColorId[] pixels, LaneDefinition[] lanes)
        {
            LevelNumber = levelNumber;
            GridSize = new Vector2Int(width, height);
            Pixels = pixels;
            Lanes = lanes;
        }

        public override string ToString() => $"LevelNumber:{LevelNumber}, GridSize:{GridSize}, Lanes:{Lanes?.Length ?? 0}";
    }
}