using Game.Lane.Item;
using Game.Level.Data;

namespace Game.Factory.Handlers
{
    public interface ILaneUnitFactoryHandler
    {
        void PopulateLaneUnits(LaneDefinition[] lanes, out BaseLaneUnitObject[][] laneUnits);
    }
}