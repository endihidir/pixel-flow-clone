using System;
using Core.Utils;
using Game.Grid.Item;
using Game.Lane.Configs;
using UnityEngine;

namespace Game.Views
{
    public sealed class PixelGridView : MonoBehaviour, IPixelGridView
    {
        [field: SerializeField] public Transform PixelRoot { get; private set; }
        [field: SerializeField] public Transform PixelsParent { get; private set; }
        [field: SerializeField] public Transform AreaPointA { get; private set; }
        [field: SerializeField] public Transform AreaPointB { get; private set; }
        [field: SerializeField] public Transform OrbitRoot { get; private set; }
        [field: SerializeField] public float Spacing { get; private set; } = 0f;
        [field: SerializeField] public float PixelHeight { get; private set; } = 1f;
        
        [field: SerializeField] public LaneOrbitPathConfigSO LaneOrbitPathConfig { get; private set; }

        private Vector3 _startPosition;
        public Vector3 AreaPointAPosition => AreaPointA.position;
        public Vector3 AreaPointBPosition => AreaPointB.position;
        public float CellSize { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public event Action OnViewInitialized;

        public void Initialize(int width, int height)
        {
            Width = width;
            Height = height;
            CalculateLayout();
            OnViewInitialized?.Invoke();
        }

        public Vector3 GetWorldPosition(Vector2Int coord)
        {
            var step = CellSize + Spacing;

            return new Vector3(_startPosition.x + coord.x * step, _startPosition.y, _startPosition.z - coord.y * step);
        }

        public void PlacePixel(BasePixelCellObject pixelCellObject, Vector2Int coord)
        {
            pixelCellObject.SetParent(PixelsParent);
            pixelCellObject.SetPosition(GetWorldPosition(coord));
            pixelCellObject.SetScale(new Vector3(CellSize, PixelHeight, CellSize));
        }

        private void CalculateLayout()
        {
            if (!AreaPointA || !AreaPointB)
            {
                EditorLogger.LogError("PixelGridView area points are missing.");
                return;
            }

            var pointA = AreaPointA.position;
            var pointB = AreaPointB.position;

            var minX = Mathf.Min(pointA.x, pointB.x);
            var maxX = Mathf.Max(pointA.x, pointB.x);

            var minZ = Mathf.Min(pointA.z, pointB.z);
            var maxZ = Mathf.Max(pointA.z, pointB.z);

            var areaWidth = maxX - minX;
            var areaDepth = maxZ - minZ;

            var sizeByWidth = (areaWidth - Spacing * (Width - 1)) / Width;
            var sizeByDepth = (areaDepth - Spacing * (Height - 1)) / Height;

            CellSize = Mathf.Min(sizeByWidth, sizeByDepth);

            var totalWidth = Width * CellSize + (Width - 1) * Spacing;
            var totalDepth = Height * CellSize + (Height - 1) * Spacing;

            var centerX = (minX + maxX) * 0.5f;
            var centerZ = (minZ + maxZ) * 0.5f;
            var y = PixelRoot.position.y;

            var startX = centerX - totalWidth * 0.5f + CellSize * 0.5f;
            var startZ = centerZ + totalDepth * 0.5f - CellSize * 0.5f;

            _startPosition = new Vector3(startX, y, startZ);
        }
    }
}