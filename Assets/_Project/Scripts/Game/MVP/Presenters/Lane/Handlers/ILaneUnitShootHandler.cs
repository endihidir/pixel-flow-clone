using Game.Lane.Item;

namespace Game.Handlers
{
    public interface ILaneUnitShootHandler
    {
        void OnFrontUnitTapped(BaseLaneUnitObject unit);
        void OnSlotUnitTapped(int slotIndex, BaseLaneUnitObject unit);
        void Dispose();
    }
}