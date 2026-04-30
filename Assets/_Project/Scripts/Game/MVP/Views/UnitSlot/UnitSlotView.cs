using Game.Lane.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Views
{
    public sealed class UnitSlotView : MonoBehaviour, IUnitSlotView
    {
        [field: SerializeField] public Transform Root { get; private set; }
        [field: SerializeField] public Transform LeftPoint { get; private set; }
        [field: SerializeField] public Transform RightPoint { get; private set; }
        [field: SerializeField] public Transform[] Slots { get; private set; }
        
        public int SlotCount => Slots?.Length ?? 0;

        public void PlaceUnit(int slotIndex, BaseLaneUnitObject unit)
        {
            var slot = Slots[slotIndex];
            unit.SetParent(slot);
            unit.SetLocalPosition(Vector3.zero);
            unit.SetLocalRotation(Quaternion.identity);
        }

        public bool TryGetSlotIndexAtScreenPoint(Vector2 screenPoint, Camera cam, out int slotIndex)
        {
            slotIndex = -1;
            if (!cam || Slots == null || Slots.Length == 0 || !LeftPoint || !RightPoint) return false;

            var plane = new Plane(Root.up, Root.position);
            var ray = cam.ScreenPointToRay(screenPoint);
            if (!plane.Raycast(ray, out float enter)) return false;

            var localHit = Root.InverseTransformPoint(ray.GetPoint(enter));
            var leftLocal = Root.InverseTransformPoint(LeftPoint.position);
            var rightLocal = Root.InverseTransformPoint(RightPoint.position);

            float minX = Mathf.Min(leftLocal.x, rightLocal.x);
            float maxX = Mathf.Max(leftLocal.x, rightLocal.x);
            if (localHit.x < minX || localHit.x > maxX) return false;

            float minZ = Mathf.Min(leftLocal.z, rightLocal.z);
            float maxZ = Mathf.Max(leftLocal.z, rightLocal.z);
            if (localHit.z < minZ || localHit.z > maxZ) return false;

            float bestDist = float.MaxValue;
            int bestIdx = -1;
            for (int i = 0; i < Slots.Length; i++)
            {
                if (!Slots[i]) continue;
                var slotLocal = Root.InverseTransformPoint(Slots[i].position);
                float dist = Mathf.Abs(localHit.x - slotLocal.x);
                if (dist >= bestDist) continue;
                bestDist = dist;
                bestIdx = i;
            }

            if (bestIdx == -1) return false;
            slotIndex = bestIdx;
            return true;
        }

        [Button("Distribute Slots Between Left/Right")]
        private void DistributeSlots()
        {
            if (Slots == null || Slots.Length == 0 || !LeftPoint || !RightPoint || !Root) return;

            var leftLocal = Root.InverseTransformPoint(LeftPoint.position);
            var rightLocal = Root.InverseTransformPoint(RightPoint.position);

            float midY = (leftLocal.y + rightLocal.y) * 0.5f;
            float midZ = (leftLocal.z + rightLocal.z) * 0.5f;

            if (Slots.Length == 1)
            {
                if (Slots[0]) Slots[0].localPosition = new Vector3((leftLocal.x + rightLocal.x) * 0.5f, midY, midZ);
                return;
            }

            for (int i = 0; i < Slots.Length; i++)
            {
                if (!Slots[i]) continue;
                float t = (float)i / (Slots.Length - 1);
                Slots[i].localPosition = new Vector3(Mathf.Lerp(leftLocal.x, rightLocal.x, t), midY, midZ);
            }
        }
    }
}