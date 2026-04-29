using Game.Lane.Item;
using Game.Level.Configs;
using Game.Level.Data;
using Game.Utils;
using Game.Factories;
using UnityEngine;

namespace Game.Factory.Handlers
{
    public sealed class LaneUnitFactoryHandler : ILaneUnitFactoryHandler
    {
        private readonly ILaneUnitFactory _laneUnitFactory;
        private readonly ColorPaletteSO _colorPalette;
        
        public LaneUnitFactoryHandler(ILaneUnitFactory laneUnitFactory, ColorPaletteSO colorPalette)
        {
            _laneUnitFactory = laneUnitFactory;
            _colorPalette = colorPalette;
        }
        
        public void PopulatePixelCells(ColorId[] pixelColorIds, int width, int height, out PigUnitObject[,] pixelCells)
        {
            pixelCells = new PigUnitObject[width, height];

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
        
        private PigUnitObject CreatePixelCell(ColorId colorId, Vector2Int coord)
        {
            var pixelCell = _laneUnitFactory.GetLaneUnit<PigUnitObject>();
            
            var color = _colorPalette.GetColor(colorId);
            
            //pixelCell.Initialize(colorId, color);

            return pixelCell;
        }
    }
}