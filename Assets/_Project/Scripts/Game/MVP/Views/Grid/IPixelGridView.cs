using System;
using Game.Grid.Item;
using Game.Lane.Configs;
using UnityEngine;

namespace Game.Views
{
    public interface IPixelGridView
    {
        event Action OnViewInitialized;
        Vector3 AreaPointAPosition { get; }
        Vector3 AreaPointBPosition { get; }
        float CellSize { get; }
        int Width { get; }
        int Height { get; }
        LaneOrbitPathConfigSO LaneOrbitPathConfig { get; }

        void Initialize(int width, int height);
        Vector3 GetWorldPosition(Vector2Int coord);
        void PlacePixel(BasePixelCellObject pixelCellObject, Vector2Int coord);
    }
}