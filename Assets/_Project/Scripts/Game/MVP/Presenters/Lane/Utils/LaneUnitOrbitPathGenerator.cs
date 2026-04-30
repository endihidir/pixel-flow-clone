using System.Collections.Generic;
using Game.Data;
using Game.Views;
using UnityEngine;

namespace Game.Utils
{
    public static class LaneUnitOrbitPathGenerator
    {
        public static LaneUnitOrbitPath Generate(IPixelGridView pixelGridView)
        {
            var pointA = pixelGridView.AreaPointAPosition;
            var pointB = pixelGridView.AreaPointBPosition;
            float offset = pixelGridView.OrbitOffset;
            float cellSize = pixelGridView.CellSize;
            int width = pixelGridView.Width;
            int height = pixelGridView.Height;
            float launchOffset = pixelGridView.LaunchOffsetFromLeft;
            float cornerRadius = pixelGridView.CornerRadius;
            float cornerOutwardOffset = pixelGridView.CornerOutwardOffset;
            int cornerSegments = pixelGridView.CornerSegments;

            float minX = Mathf.Min(pointA.x, pointB.x) - offset;
            float maxX = Mathf.Max(pointA.x, pointB.x) + offset;
            float minZ = Mathf.Min(pointA.z, pointB.z) - offset;
            float maxZ = Mathf.Max(pointA.z, pointB.z) + offset;
            float y = pointA.y;

            int totalNodes = 2 * width + 2 * height;
            var nodes = new OrbitNode[totalNodes];
            int idx = 0;

            for (int i = 0; i < width; i++)
            {
                var coord = new Vector2Int(i, height - 1);
                var cellWorldPos = pixelGridView.GetWorldPosition(coord);
                nodes[idx++] = new OrbitNode { Position = new Vector3(cellWorldPos.x, y, minZ), Edge = OrbitEdge.Bottom, LineIndex = i };
            }

            for (int i = 0; i < height; i++)
            {
                var coord = new Vector2Int(width - 1, height - 1 - i);
                var cellWorldPos = pixelGridView.GetWorldPosition(coord);
                nodes[idx++] = new OrbitNode { Position = new Vector3(maxX, y, cellWorldPos.z), Edge = OrbitEdge.Right, LineIndex = height - 1 - i };
            }

            for (int i = 0; i < width; i++)
            {
                var coord = new Vector2Int(width - 1 - i, 0);
                var cellWorldPos = pixelGridView.GetWorldPosition(coord);
                nodes[idx++] = new OrbitNode { Position = new Vector3(cellWorldPos.x, y, maxZ), Edge = OrbitEdge.Top, LineIndex = width - 1 - i };
            }

            for (int i = 0; i < height; i++)
            {
                var coord = new Vector2Int(0, i);
                var cellWorldPos = pixelGridView.GetWorldPosition(coord);
                nodes[idx++] = new OrbitNode { Position = new Vector3(minX, y, cellWorldPos.z), Edge = OrbitEdge.Left, LineIndex = i };
            }

            int launchIndex = Mathf.Clamp(Mathf.RoundToInt(launchOffset / cellSize), 0, width - 1);

            var waypoints = BuildWaypointsWithRoundedCorners(nodes, cornerRadius, cornerOutwardOffset, cornerSegments);

            return new LaneUnitOrbitPath
            {
                Waypoints = waypoints,
                Nodes = nodes,
                LaunchNodeIndex = launchIndex
            };
        }

        private static Vector3[] BuildWaypointsWithRoundedCorners(OrbitNode[] nodes, float radius, float outwardOffset, int segments)
        {
            var center = ComputePathCenter(nodes);
            var result = new List<Vector3>(nodes.Length + 4 * segments);

            for (int i = 0; i < nodes.Length; i++)
            {
                var current = nodes[i];
                var next = nodes[(i + 1) % nodes.Length];

                result.Add(current.Position);

                if (current.Edge != next.Edge)
                {
                    var corner = ComputeCornerPoint(current, next);

                    if (segments <= 1 && radius == 0f && outwardOffset == 0f)
                    {
                        result.Add(corner);
                    }
                    else
                    {
                        var outwardDir = (corner - center).normalized;
                        var midpoint = (current.Position + next.Position) * 0.5f;
                        var controlPoint = Vector3.Lerp(midpoint, corner, radius) + outwardDir * outwardOffset;

                        for (int s = 1; s < segments; s++)
                        {
                            float t = (float)s / segments;
                            var p = QuadraticBezier(current.Position, controlPoint, next.Position, t);
                            result.Add(p);
                        }
                    }
                }
            }

            return result.ToArray();
        }

        private static Vector3 ComputePathCenter(OrbitNode[] nodes)
        {
            var sum = Vector3.zero;
            for (int i = 0; i < nodes.Length; i++) sum += nodes[i].Position;
            return sum / nodes.Length;
        }

        private static Vector3 ComputeCornerPoint(OrbitNode a, OrbitNode b)
        {
            float x, z;

            if (a.Edge == OrbitEdge.Bottom || a.Edge == OrbitEdge.Top)
            {
                x = b.Position.x;
                z = a.Position.z;
            }
            else
            {
                x = a.Position.x;
                z = b.Position.z;
            }

            return new Vector3(x, a.Position.y, z);
        }

        private static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }
    }
}