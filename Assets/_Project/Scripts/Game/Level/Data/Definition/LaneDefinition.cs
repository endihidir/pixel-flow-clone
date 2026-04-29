namespace Game.Level.Data
{
    public sealed class LaneDefinition
    {
        public LaneUnitDefinition[] LaneUnits { get; }
        public int UnitCount => LaneUnits?.Length ?? 0;

        public LaneDefinition(LaneUnitDefinition[] laneUnits)
        {
            LaneUnits = laneUnits;
        }

        public override string ToString() => $"LaneUnits:{UnitCount}";
    }
}