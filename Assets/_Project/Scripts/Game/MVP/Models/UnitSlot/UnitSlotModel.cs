namespace Game.Models
{
    public sealed class UnitSlotModel : IUnitSlotModel
    {
        public int SlotCount { get; private set; }

        public void Initialize(int slotCount = 5)
        {
            SlotCount = slotCount;
        }
    }
}
