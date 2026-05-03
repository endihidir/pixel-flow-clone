using System;
using Cysharp.Threading.Tasks;
using Game.Lane.Configs;
using Game.Lane.Item;
using UnityEngine;

namespace Game.Views
{
    public sealed class LaneView : MonoBehaviour, ILaneView
    {
        [field: SerializeField] public Camera Camera { get; private set; }
        [field: SerializeField] public Transform Root { get; private set; }
        [field: SerializeField] public Transform LeftPoint { get; private set; }
        [field: SerializeField] public Transform RightPoint { get; private set; }
        [field: SerializeField] public LaneViewConfigSO LaneViewConfig { get; private set; }

        private Transform[] _laneRoots;
        private float _laneSpacing;
        private float _firstLaneLocalX;

        public event Action OnViewInitialized;

        public void Initialize(int laneCount)
        {
            BuildLaneRoots(laneCount);
            OnViewInitialized?.Invoke();
        }

        public Transform GetLaneRoot(int laneIndex) => _laneRoots[laneIndex];
        public Vector3 GetUnitLocalPosition(int slotIndex) => new(0f, 0f, -slotIndex * LaneViewConfig.UnitOffset);

        public void PlaceUnit(int laneIndex, int slotIndex, BaseLaneUnitObject unit)
        {
            var laneRoot = _laneRoots[laneIndex];
            unit.SetParent(laneRoot);
            unit.SetLocalPosition(GetUnitLocalPosition(slotIndex));
            unit.SetLocalRotation(Quaternion.identity);
        }

        public async UniTask AnimateUnitToSlot(int slotIndex, int moveOrder, BaseLaneUnitObject unit)
        {
            var unitLocalPos = GetUnitLocalPosition(slotIndex);
            var moveDuration = LaneViewConfig.AdvanceMoveDuration;
            var delay = moveOrder * LaneViewConfig.AdvanceStepDelay;
            await unit.Animation.MoveLocalTo(unitLocalPos, moveDuration, delay);
        }

        public bool TryGetLaneIndexAtScreenPoint(Vector2 screenPoint, out int laneIndex)
        {
            laneIndex = -1;
            if (_laneRoots == null || _laneRoots.Length == 0) return false;
            if (!TryRaycastToLocal(screenPoint, out var localHit)) return false;

            var relativeX = localHit.x - _firstLaneLocalX;
            var idx = Mathf.RoundToInt(relativeX / _laneSpacing);

            if (idx < 0 || idx >= _laneRoots.Length) return false;

            var laneCenterLocalX = _firstLaneLocalX + idx * _laneSpacing;
            if (Mathf.Abs(localHit.x - laneCenterLocalX) > LaneViewConfig.TapPlaneHalfWidth) return false;

            laneIndex = idx;
            return true;
        }

        public bool TryGetLaneUnitSlotAtScreenPoint(Vector2 screenPoint, int laneIndex, int unitCount, out int slotIndex)
        {
            slotIndex = -1;
            if (_laneRoots == null || laneIndex < 0 || laneIndex >= _laneRoots.Length || unitCount <= 0) return false;
            if (!TryRaycastToLocal(screenPoint, out var localHit)) return false;

            var laneRootLocalPos = _laneRoots[laneIndex].localPosition;

            var bestSqrDist = float.MaxValue;
            var bestSlotIndex = -1;

            for (int i = 0; i < unitCount; i++)
            {
                var unitLocalPos = laneRootLocalPos + GetUnitLocalPosition(i);
                var dx = Mathf.Abs(localHit.x - unitLocalPos.x);
                var dz = Mathf.Abs(localHit.z - unitLocalPos.z);

                if (dx > LaneViewConfig.UnitTapHalfWidth) continue;
                if (dz > LaneViewConfig.UnitTapHalfDepth) continue;

                var sqrDist = dx * dx + dz * dz;
                if (sqrDist >= bestSqrDist) continue;

                bestSqrDist = sqrDist;
                bestSlotIndex = i;
            }

            if (bestSlotIndex == -1) return false;

            slotIndex = bestSlotIndex;
            return true;
        }

        private bool TryRaycastToLocal(Vector2 screenPoint, out Vector3 localHit)
        {
            localHit = default;
            if (!Camera || !Root) return false;

            var planeOrigin = Root.position + Root.up * LaneViewConfig.TapPlaneHeightOffset;
            var plane = new Plane(Root.up, planeOrigin);
            var ray = Camera.ScreenPointToRay(screenPoint);
            if (!plane.Raycast(ray, out var enter)) return false;

            var worldHit = ray.GetPoint(enter);
            
            localHit = Root.InverseTransformPoint(worldHit);
            return true;
        }

        private void BuildLaneRoots(int laneCount)
        {
            ClearLaneRoots();

            _laneRoots = new Transform[laneCount];
            var leftLocal = Root.InverseTransformPoint(LeftPoint.position);
            var rightLocal = Root.InverseTransformPoint(RightPoint.position);

            var availableWidth = rightLocal.x - leftLocal.x;
            var desiredWidth = (laneCount - 1) * LaneViewConfig.LaneSpacing;
            _laneSpacing = desiredWidth > availableWidth && laneCount > 1 ? availableWidth / (laneCount - 1) : LaneViewConfig.LaneSpacing;

            var usedWidth = (laneCount - 1) * _laneSpacing;
            var centerX = (leftLocal.x + rightLocal.x) * 0.5f;
            _firstLaneLocalX = centerX - usedWidth * 0.5f;

            for (int i = 0; i < laneCount; i++)
            {
                var go = new GameObject($"Lane_{i + 1}");
                var t = go.transform;
                t.SetParent(Root, false);
                t.localPosition = new Vector3(_firstLaneLocalX + i * _laneSpacing, leftLocal.y, leftLocal.z);
                t.localRotation = Quaternion.identity;
                _laneRoots[i] = t;
            }
        }

        private void ClearLaneRoots()
        {
            if (_laneRoots == null) return;
            foreach (var t in _laneRoots) if (t) Destroy(t.gameObject);
            _laneRoots = null;
        }
    }
}