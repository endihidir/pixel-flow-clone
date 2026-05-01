using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Lane.Item;
using UnityEngine;

namespace Game.Utils
{
    public static class OrbitRunner
    {
        public delegate UniTask<bool> NodeHandler(BaseLaneUnitObject unit, int nodeIndex, bool aimLocked);

        public static async UniTask Run(BaseLaneUnitObject unit, LaneUnitOrbitPath path, int startIdx, float speed, bool aimLocked, NodeHandler onTriggerNode)
        {
            int endIdx = FindPreviousTriggerIndex(path, startIdx);
            int currentIdx = startIdx;

            if (path.Nodes[currentIdx].IsTriggerNode && onTriggerNode != null)
            {
                aimLocked = await onTriggerNode(unit, currentIdx, aimLocked);

                if (ShouldStop()) return;
            }

            while (currentIdx != endIdx)
            {
                int toIdx = (currentIdx + 1) % path.Nodes.Length;
                var fromNode = path.Nodes[currentIdx];
                var toNode = path.Nodes[toIdx];

                float distance = Vector3.Distance(fromNode.Position, toNode.Position);
                float duration = speed > 0f ? distance / speed : 0f;

                if (!Mathf.Approximately(Mathf.DeltaAngle(fromNode.PathYaw, toNode.PathYaw), 0f))
                    unit.OrbitAnimation.RotateToPathYaw(toNode.PathYaw, duration);

                await unit.OrbitAnimation.MoveSegment(toNode.Position, duration);

                if (ShouldStop()) return;

                currentIdx = toIdx;

                if (path.Nodes[currentIdx].IsTriggerNode && onTriggerNode != null)
                {
                    aimLocked = await onTriggerNode(unit, currentIdx, aimLocked);

                    if (ShouldStop()) return;
                }
            }

            return;

            bool ShouldStop()
            {
                return unit.Ammo <= 0 || !unit.gameObject.activeSelf;
            }
        }

        private static int FindPreviousTriggerIndex(LaneUnitOrbitPath path, int fromIndex)
        {
            int len = path.Nodes.Length;
            int idx = (fromIndex - 1 + len) % len;
            while (idx != fromIndex)
            {
                if (path.Nodes[idx].IsTriggerNode) return idx;
                idx = (idx - 1 + len) % len;
            }
            return fromIndex;
        }
    }
}