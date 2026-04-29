using Game.Grid.Item;
using UnityEngine;

namespace Game.Factories
{
    public interface IPixelCellFactory
    {
        T GetPixelCell<T>() where T : BasePixelCellObject;
        void ReleasePixelCell(BasePixelCellObject grid);
        void ReleasePixelCell(Transform item);
        void ReleasePixelCellsByType<T>() where T : BasePixelCellObject;
        void RemovePixelCellPoolByType<T>() where T : BasePixelCellObject;
    }
}