using UnityEngine;

namespace Game.Data
{
    public struct LaneUnitOrbitPath
    {
        public Vector3[] Waypoints;
        public OrbitNode[] Nodes;
        public int LaunchNodeIndex;
    }
}