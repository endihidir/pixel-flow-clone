using Game.Grid.Item;
using Game.Level.Configs;
using Game.Level.Data;
using Game.Utils;
using Game.Factories;
using UnityEngine;

namespace Game.Factory.Handlers
{
    public sealed class PixelCellFactoryHandler : IPixelCellFactoryHandler
    {
        private readonly IPixelCellFactory _pixelCellFactory;
        private readonly ColorPaletteSO _colorPalette;
        
        public PixelCellFactoryHandler(IPixelCellFactory pixelCellFactory, ColorPaletteSO colorPalette)
        {
            _pixelCellFactory = pixelCellFactory;
            _colorPalette = colorPalette;
        }

        public void PopulatePixelCells(ColorId[] pixelColorIds, int width, int height, out PixelCellObject[,] pixelCells)
        {
            pixelCells = new PixelCellObject[width, height];

            for (var i = 0; i < width * height; i++)
            {
                var coord = GridIndexUtil.ToCoord(i, width);
                var x = coord.x;
                var y = coord.y;

                var colorId = pixelColorIds[i];

                if (colorId == ColorId.None) continue;

                pixelCells[x, y] = CreatePixelCell(colorId, coord);
            }
        }
        
        private PixelCellObject CreatePixelCell(ColorId colorId, Vector2Int coord)
        {
            var pixelCell = _pixelCellFactory.GetPixelCell<PixelCellObject>();
            
            var color = _colorPalette.GetColor(colorId);
            
            pixelCell.Initialize(colorId, color);

            return pixelCell;
        }
        
        public void ReleaseAllPixelCells() => _pixelCellFactory.ReleasePixelCellsByType<PixelCellObject>();
    }
}