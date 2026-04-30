using System;
using Core.Utils;
using Game.Grid.Item;
using UnityEngine;

namespace Game.Views
{
    public sealed class PixelGridView : MonoBehaviour, IPixelGridView
    {
        [field: SerializeField] public Transform PixelRoot { get; private set; }
        [field: SerializeField] public Transform PixelsParent { get; private set; }
        [field: SerializeField] public Transform AreaPointA { get; private set; }
        [field: SerializeField] public Transform AreaPointB { get; private set; }
        [field: SerializeField] public float Spacing { get; private set; } = 0f;
        [field: SerializeField] public float PixelHeight { get; private set; } = 1f;
        
        private int _width;
        private int _height;
        private float _cellSize;
        private Vector3 _startPosition;
        public event Action OnViewInitialized;

        public void Initialize(int width, int height)
        {
            _width = width;
            _height = height;
            CalculateLayout();
            OnViewInitialized?.Invoke();
        }

        public Vector3 GetWorldPosition(Vector2Int coord)
        {
            var step = _cellSize + Spacing;

            return new Vector3(_startPosition.x + coord.x * step, _startPosition.y, _startPosition.z - coord.y * step);
        }

        public void PlacePixel(BasePixelCellObject pixelCellObject, Vector2Int coord)
        {
            pixelCellObject.SetParent(PixelsParent);
            pixelCellObject.SetPosition(GetWorldPosition(coord));
            pixelCellObject.SetScale(new Vector3(_cellSize, PixelHeight, _cellSize));
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

            var sizeByWidth = (areaWidth - Spacing * (_width - 1)) / _width;
            var sizeByDepth = (areaDepth - Spacing * (_height - 1)) / _height;

            _cellSize = Mathf.Min(sizeByWidth, sizeByDepth);

            var totalWidth = _width * _cellSize + (_width - 1) * Spacing;
            var totalDepth = _height * _cellSize + (_height - 1) * Spacing;

            var centerX = (minX + maxX) * 0.5f;
            var centerZ = (minZ + maxZ) * 0.5f;
            var y = PixelRoot.position.y;

            var startX = centerX - totalWidth * 0.5f + _cellSize * 0.5f;
            var startZ = centerZ + totalDepth * 0.5f - _cellSize * 0.5f;

            _startPosition = new Vector3(startX, y, startZ);
        }
    }
}