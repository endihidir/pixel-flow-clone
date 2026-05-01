using System;
using Game.Lane.Configs;
using Game.Lane.Item;
using UnityEngine;

namespace Game.Views
{
    public sealed class LaneView : MonoBehaviour, ILaneView
    {
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

        public bool TryGetLaneIndexAtScreenPoint(Vector2 screenPoint, Camera cam, out int laneIndex)
        {
            laneIndex = -1;
            if (cam == null || _laneRoots == null || _laneRoots.Length == 0) return false;

            var planeOrigin = Root.position;
            var planeNormal = Root.up;
            var plane = new Plane(planeNormal, planeOrigin);

            var ray = cam.ScreenPointToRay(screenPoint);
            if (!plane.Raycast(ray, out float enter)) return false;

            var worldHit = ray.GetPoint(enter);
            var localHit = Root.InverseTransformPoint(worldHit);

            float relativeX = localHit.x - _firstLaneLocalX;
            int idx = Mathf.RoundToInt(relativeX / _laneSpacing);

            if (idx < 0 || idx >= _laneRoots.Length) return false;

            float laneCenterLocalX = _firstLaneLocalX + idx * _laneSpacing;
            if (Mathf.Abs(localHit.x - laneCenterLocalX) > LaneViewConfig.TapPlaneHalfWidth) return false;

            laneIndex = idx;
            return true;
        }

        private void BuildLaneRoots(int laneCount)
        {
            ClearLaneRoots();

            _laneRoots = new Transform[laneCount];
            var leftLocal = Root.InverseTransformPoint(LeftPoint.position);
            var rightLocal = Root.InverseTransformPoint(RightPoint.position);

            float availableWidth = rightLocal.x - leftLocal.x;
            float desiredWidth = (laneCount - 1) * LaneViewConfig.LaneSpacing;
            _laneSpacing = desiredWidth > availableWidth && laneCount > 1
                ? availableWidth / (laneCount - 1)
                : LaneViewConfig.LaneSpacing;

            float usedWidth = (laneCount - 1) * _laneSpacing;
            float centerX = (leftLocal.x + rightLocal.x) * 0.5f;
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