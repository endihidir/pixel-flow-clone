using Game.Level.Data;

namespace Game.Models
{
    public interface ILaneModel
    {
        void Initialize(LaneDefinition[] lanes);
    }
}