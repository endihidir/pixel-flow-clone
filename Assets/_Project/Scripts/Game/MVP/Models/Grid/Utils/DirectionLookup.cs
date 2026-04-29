using UnityEngine;

namespace Game.Utils
{
    public static class DirectionLookup
    {
        public static readonly Vector2Int[] HorizontalDirections =
        {
            new (1, 0),
            new (-1, 0),
        };
        
        public static readonly Vector2Int[] VerticalDirections =
        {
            new (0, 1),
            new (0, -1)
        };
        
        public static readonly Vector2Int[] LinearDirections =
        {
            new (0, 1),
            new (1, 0),
            new (-1, 0),
            new (0, -1)
        };
        
        public static readonly Vector2Int[] DiagonalDirections =
        {
            new (1, 1),
            new (-1, 1),
            new (1, -1),
            new (-1, -1)
        };

        public static readonly Vector2Int[] AllDirections =
        {
            new (0, 1),
            new (1, 0),
            new (-1, 0),
            new (0, -1),
            new (1, 1),
            new (-1, 1),
            new (1, -1),
            new (-1, -1)
        };
    }
}