namespace Game.Level.Data
{
    public readonly struct LaneUnitDefinition
    {
        public ColorId Color { get; }
        public int Ammo { get; }

        public LaneUnitDefinition(ColorId color, int ammo)
        {
            Color = color;
            Ammo = ammo;
        }

        public override string ToString() => $"Color:{Color}, Ammo:{Ammo}";
    }
}