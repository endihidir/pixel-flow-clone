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
            float offsetX = pixelGridView.LaneOrbitPathConfig.OrbitOffset.x;
            float offsetY = pixelGridView.LaneOrbitPathConfig.OrbitOffset.y;
            float offsetZ = pixelGridView.LaneOrbitPathConfig.OrbitOffset.z;
            float cellSize = pixelGridView.CellSize;
            int width = pixelGridView.Width;
            int height = pixelGridView.Height;
            float launchOffset = pixelGridView.LaneOrbitPathConfig.LaunchOffsetFromLeft;
            float cornerRadius = pixelGridView.LaneOrbitPathConfig.CornerRadius;
            float cornerOutwardOffset = pixelGridView.LaneOrbitPathConfig.CornerOutwardOffset;
            int cornerSegments = pixelGridView.LaneOrbitPathConfig.CornerSegments;

            float minX = Mathf.Min(pointA.x, pointB.x) - offsetX;
            float maxX = Mathf.Max(pointA.x, pointB.x) + offsetX;
            float minZ = Mathf.Min(pointA.z, pointB.z) - offsetZ;
            float maxZ = Mathf.Max(pointA.z, pointB.z) + offsetZ;
            float y = offsetY;

            int triggerCount = 2 * width + 2 * height;
            var triggerNodes = new OrbitNode[triggerCount];
            int idx = 0;

            for (int i = 0; i < width; i++)
            {
                var coord = new Vector2Int(i, height - 1);
                var cellWorldPos = pixelGridView.GetWorldPosition(coord);
                triggerNodes[idx++] = new OrbitNode
                {
                    Position = new Vector3(cellWorldPos.x, y, minZ),
                    Edge = OrbitEdge.Bottom,
                    LineIndex = i,
                    IsTriggerNode = true,
                    PathYaw = GetPathYaw(OrbitEdge.Bottom)
                };
            }

            for (int i = 0; i < height; i++)
            {
                var coord = new Vector2Int(width - 1, height - 1 - i);
                var cellWorldPos = pixelGridView.GetWorldPosition(coord);
                triggerNodes[idx++] = new OrbitNode
                {
                    Position = new Vector3(maxX, y, cellWorldPos.z),
                    Edge = OrbitEdge.Right,
                    LineIndex = height - 1 - i,
                    IsTriggerNode = true,
                    PathYaw = GetPathYaw(OrbitEdge.Right)
                };
            }

            for (int i = 0; i < width; i++)
            {
                var coord = new Vector2Int(width - 1 - i, 0);
                var cellWorldPos = pixelGridView.GetWorldPosition(coord);
                triggerNodes[idx++] = new OrbitNode
                {
                    Position = new Vector3(cellWorldPos.x, y, maxZ),
                    Edge = OrbitEdge.Top,
                    LineIndex = width - 1 - i,
                    IsTriggerNode = true,
                    PathYaw = GetPathYaw(OrbitEdge.Top)
                };
            }

            for (int i = 0; i < height; i++)
            {
                var coord = new Vector2Int(0, i);
                var cellWorldPos = pixelGridView.GetWorldPosition(coord);
                triggerNodes[idx++] = new OrbitNode
                {
                    Position = new Vector3(minX, y, cellWorldPos.z),
                    Edge = OrbitEdge.Left,
                    LineIndex = i,
                    IsTriggerNode = true,
                    PathYaw = GetPathYaw(OrbitEdge.Left)
                };
            }

            int launchTriggerIndex = Mathf.Clamp(Mathf.RoundToInt(launchOffset / cellSize), 0, width - 1);

            var allNodes = ExpandWithCornerNodes(triggerNodes, cornerRadius, cornerOutwardOffset, cornerSegments);
            int launchNodeIndex = FindNodeIndex(allNodes, triggerNodes[launchTriggerIndex]);

            return new LaneUnitOrbitPath
            {
                Nodes = allNodes,
                LaunchNodeIndex = launchNodeIndex
            };
        }

        private static OrbitNode[] ExpandWithCornerNodes(OrbitNode[] triggerNodes, float radius, float outwardOffset, int segments)
        {
            var center = ComputePathCenter(triggerNodes);
            var result = new List<OrbitNode>(triggerNodes.Length + 4 * segments);

            for (int i = 0; i < triggerNodes.Length; i++)
            {
                var current = triggerNodes[i];
                var next = triggerNodes[(i + 1) % triggerNodes.Length];

                result.Add(current);

                if (current.Edge == next.Edge) continue;

                var corner = ComputeCornerPoint(current, next);
                float fromYaw = GetPathYaw(current.Edge);
                float toYaw = fromYaw + Mathf.DeltaAngle(fromYaw, GetPathYaw(next.Edge));

                if (segments <= 1 && radius == 0f && outwardOffset == 0f)
                {
                    result.Add(new OrbitNode
                    {
                        Position = corner,
                        Edge = next.Edge,
                        LineIndex = -1,
                        IsTriggerNode = false,
                        PathYaw = Mathf.Lerp(fromYaw, toYaw, 0.5f)
                    });
                    continue;
                }

                var outwardDir = (corner - center).normalized;
                var midpoint = (current.Position + next.Position) * 0.5f;
                var controlPoint = Vector3.Lerp(midpoint, corner, radius) + outwardDir * outwardOffset;

                for (int s = 1; s < segments; s++)
                {
                    float t = (float)s / segments;
                    var p = QuadraticBezier(current.Position, controlPoint, next.Position, t);
                    result.Add(new OrbitNode
                    {
                        Position = p,
                        Edge = next.Edge,
                        LineIndex = -1,
                        IsTriggerNode = false,
                        PathYaw = Mathf.Lerp(fromYaw, toYaw, t)
                    });
                }
            }

            return result.ToArray();
        }

        private static int FindNodeIndex(OrbitNode[] nodes, OrbitNode target)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].IsTriggerNode && nodes[i].Position == target.Position) return i;
            }
            return 0;
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

        private static float GetPathYaw(OrbitEdge edge) => edge switch
        {
            OrbitEdge.Bottom => 90f,
            OrbitEdge.Right  => 0f,
            OrbitEdge.Top    => 270f,
            OrbitEdge.Left   => 180f,
            _ => 0f
        };
    }
}