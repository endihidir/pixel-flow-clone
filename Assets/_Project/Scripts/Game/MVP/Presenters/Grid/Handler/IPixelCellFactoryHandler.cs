using Game.Grid.Item;
using Game.Level.Data;

namespace Game.Factory.Handlers
{
    public interface IPixelCellFactoryHandler
    {
        void PopulatePixelCells(ColorId[] pixelColorIds, int width, int height, out PixelCellObject[,] pixelCells);
        void ReleaseAllPixelCells();
    }
}