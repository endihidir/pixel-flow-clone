using UnityEngine;

namespace Game.Utils
{
    public static class GridIndexUtil
    {
        public static int FromCoord(Vector2Int coordinate, int width) => coordinate.y * width + coordinate.x;
        public static Vector2Int ToCoord(int index, int width) => new (index % width, index / width);
    }
}