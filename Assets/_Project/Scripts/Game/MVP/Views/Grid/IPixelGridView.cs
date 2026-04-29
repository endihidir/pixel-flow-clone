using System;
using Game.Grid.Item;
using UnityEngine;

namespace Game.Views
{
    public interface IPixelGridView
    {
        public event Action OnViewInitialized;
        Transform PixelsParent { get; }
        void Initialize(int width, int height);
        Vector3 GetWorldPosition(Vector2Int coord);
        void PlacePixel(PixelCellObject pixelCellObject, Vector2Int coord);
    }
}