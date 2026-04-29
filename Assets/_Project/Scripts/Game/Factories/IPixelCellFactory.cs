using Game.Grid.Item;
using UnityEngine;

namespace Game.View.Factories
{
    public interface IPixelCellFactory
    {
        T GetPixelCell<T>() where T : BasePixelCell;
        void ReleasePixelCell(BasePixelCell grid);
        void ReleasePixelCell(Transform item);
        void ReleasePixelCellsByType<T>() where T : BasePixelCell;
        void RemovePixelCellPoolByType<T>() where T : BasePixelCell;
    }
}