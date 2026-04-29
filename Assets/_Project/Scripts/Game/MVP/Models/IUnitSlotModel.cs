namespace Game.Models
{
    public interface IUnitSlotModel
    {
        int SlotCount { get; }
        void Initialize(int slotCount = 5);
    }
}