using System;
using Game.Grid.Item;
using UnityEngine;

namespace Game.Views
{
    public interface IPixelGridView
    {
        public event Action OnViewInitialized;
        void Initialize(int width, int height);
        Vector3 GetWorldPosition(Vector2Int coord);
        void PlacePixel(BasePixelCellObject pixelCellObject, Vector2Int coord);
    }
}