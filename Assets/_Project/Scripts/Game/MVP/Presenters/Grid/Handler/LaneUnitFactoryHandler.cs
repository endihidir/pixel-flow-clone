using Game.Factories;
using Game.Lane.Item;
using Game.Level.Configs;
using Game.Level.Data;

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

        public void PopulateLaneUnits(LaneDefinition[] lanes, out BaseLaneUnitObject[][] laneUnits)
        {
            laneUnits = new BaseLaneUnitObject[lanes.Length][];

            for (int li = 0; li < lanes.Length; li++)
            {
                var laneDef = lanes[li];
                var unitCount = laneDef.UnitCount;
                var units = new BaseLaneUnitObject[unitCount];

                for (int ui = 0; ui < unitCount; ui++)
                {
                    var unitDef = laneDef.LaneUnits[ui];
                    units[ui] = CreateLaneUnit(unitDef.Color, unitDef.Ammo);
                }

                laneUnits[li] = units;
            }
        }

        private BaseLaneUnitObject CreateLaneUnit(ColorId colorId, int ammo)
        {
            var unit = _laneUnitFactory.GetLaneUnit<PigUnitObject>();
            var color = _colorPalette.GetColor(colorId);
            unit.Initialize(colorId, ammo, color);
            return unit;
        }
        
        public void ReleaseAllLaneUnits() => _laneUnitFactory.ReleaseLaneUnitsByType<PigUnitObject>();
    }
}