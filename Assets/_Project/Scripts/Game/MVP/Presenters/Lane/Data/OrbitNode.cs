using UnityEngine;

namespace Game.Data
{
    public struct OrbitNode
    {
        public Vector3 Position;
        public OrbitEdge Edge;
        public int LineIndex;
        public bool IsTriggerNode;
        public float PathYaw;
    }
}