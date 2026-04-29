using Game.Grid.Item;
using Game.Level.Data;

namespace Game.Grid.Handlers
{
    public interface IPixelCellFactoryHandler
    {
        void PopulatePixelCells(ColorId[] pixelColorIds, int width, int height, out PixelCell[,] pixelCells);
    }
}