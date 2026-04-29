using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Services
{
    public interface ILevelDefinitionProvider
    {
        int GetGridWidth();
        int GetGridHeight();
        int GetLevelNumber(bool useLevelCompletionCount = false);
        Vector2Int GetGridSize();
        LaneDefinition[] GetLanes();
        ColorId[] GetPixels();
    }
}