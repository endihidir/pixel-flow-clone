using Core.Models;
using Game.Grid.Item;
using UnityEngine;

namespace Game.Models
{
    public sealed class PixelGridModel : BaseGridModel<PixelCellObject>, IPixelGridModel
    {
        protected override void OnInitialize()
        {
            
        }

        protected override void SetInternal(Vector2Int coord, PixelCellObject value, bool raiseEvent = true)
        {
            value?.SetCoord(coord);
            base.SetInternal(coord, value, raiseEvent);
        }
    }
}