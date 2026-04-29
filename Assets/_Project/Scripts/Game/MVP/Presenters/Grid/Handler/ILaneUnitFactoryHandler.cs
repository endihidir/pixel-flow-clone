using Game.Lane.Item;
using Game.Level.Data;

namespace Game.Factory.Handlers
{
    public interface ILaneUnitFactoryHandler
    {
        void PopulatePixelCells(ColorId[] pixelColorIds, int width, int height, out PigUnitObject[,] pixelCells);
    }
}