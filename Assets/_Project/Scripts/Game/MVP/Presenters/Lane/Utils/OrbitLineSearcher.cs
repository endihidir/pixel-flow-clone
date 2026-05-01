using Game.Data;
using Game.Level.Data;
using Game.Models;
using UnityEngine;

namespace Game.Utils
{
    public static class OrbitLineSearcher
    {
        public static bool TryFindMatchingPixelOnLine(IPixelGridModel pixelGridModel, OrbitNode node, ColorId colorId, out Vector2Int coord)
        {
            coord = default;
            int width = pixelGridModel.Width;
            int height = pixelGridModel.Height;

            switch (node.Edge)
            {
                case OrbitEdge.Bottom:
                    for (int y = height - 1; y >= 0; y--)
                        if (TryCheckFirstOccupiedCell(pixelGridModel, new Vector2Int(node.LineIndex, y), colorId, out coord, out bool foundOccupied))
                            return true;
                        else if (foundOccupied)
                            return false;

                    break;

                case OrbitEdge.Top:
                    for (int y = 0; y < height; y++)
                        if (TryCheckFirstOccupiedCell(pixelGridModel, new Vector2Int(node.LineIndex, y), colorId, out coord, out bool foundOccupied))
                            return true;
                        else if (foundOccupied)
                            return false;

                    break;

                case OrbitEdge.Left:
                    for (int x = 0; x < width; x++)
                        if (TryCheckFirstOccupiedCell(pixelGridModel, new Vector2Int(x, node.LineIndex), colorId, out coord, out bool foundOccupied))
                            return true;
                        else if (foundOccupied)
                            return false;

                    break;

                case OrbitEdge.Right:
                    for (int x = width - 1; x >= 0; x--)
                        if (TryCheckFirstOccupiedCell(pixelGridModel, new Vector2Int(x, node.LineIndex), colorId, out coord, out bool foundOccupied))
                            return true;
                        else if (foundOccupied)
                            return false;

                    break;
            }

            return false;
        }

        private static bool TryCheckFirstOccupiedCell(IPixelGridModel pixelGridModel, Vector2Int testCoord, ColorId colorId, out Vector2Int coord, out bool foundOccupied)
        {
            coord = default;
            foundOccupied = false;

            var pixel = pixelGridModel.GetGridObject(testCoord);

            if (pixel == null)
                return false;

            foundOccupied = true;

            if (pixel.ColorId != colorId)
                return false;

            coord = testCoord;
            return true;
        }
    }
}