using System;
using UnityEngine;

namespace Core.Models
{
    public interface IBaseGridModel<T> where T : class
    {
        int Width { get; }
        int Height { get; }
        int GridLenght { get; }
        Vector2Int GridSize { get; }

        bool[,] ActiveCells { get; }
        T[,] GridArray { get; }

        event Action<T> OnGridObjectInitialized;
        event Action<T> OnUpdateCellData;
        event Action OnModelInitialized;

        IBaseGridModel<T> Initialize(T[,] value, int width, int height, out bool[,] activeCells);

        // Grid access (safe)
        bool TryGetGridObject(Vector2Int coord, out T gridObject);
        T GetGridObject(Vector2Int coord);
        T GetGridObject(int x, int y) => GetGridObject(new Vector2Int(x, y));
        void SetGridObject(Vector2Int coord, T value);

        // Neighbour queries (safe)
        bool TryGetNeighbourCoord(Vector2Int sourceCoord, Vector2Int direction, out Vector2Int neighbourCoord);
        bool TryGetNeighbour(Vector2Int sourceCoord, Vector2Int direction, out T neighbour);
        bool TryGetNeighboursNonAlloc(Vector2Int sourceCoord, Span<T> resultBuffer, out int count);
        // Cell state
        bool IsCellActive(Vector2Int coord);
        
        bool IsInRange(Vector2Int coord);
        bool IsInRange(int x, int y) => IsInRange(new Vector2Int(x, y));
        bool IsAllNull();
    }
}