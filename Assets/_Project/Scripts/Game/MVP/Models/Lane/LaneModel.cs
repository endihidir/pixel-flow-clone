using System;
using System.Collections.Generic;
using Game.Lane.Item;
using Game.Level.Data;

namespace Game.Models
{
    public sealed class LaneModel : ILaneModel
    {
        private LaneDefinition[] _laneDefinitions;
        private List<BaseLaneUnitObject>[] _laneUnits;

        public int LaneCount => _laneDefinitions?.Length ?? 0;

        public event Action<int, BaseLaneUnitObject> OnUnitAdvanced;

        public void Initialize(LaneDefinition[] lanes, BaseLaneUnitObject[][] laneUnits)
        {
            _laneDefinitions = lanes;
            _laneUnits = new List<BaseLaneUnitObject>[lanes.Length];

            for (int i = 0; i < lanes.Length; i++)
            {
                var units = laneUnits[i];
                _laneUnits[i] = new List<BaseLaneUnitObject>(units.Length);
                _laneUnits[i].AddRange(units);
            }
        }

        public BaseLaneUnitObject GetFrontUnit(int laneIndex)
        {
            var list = _laneUnits[laneIndex];
            return list.Count > 0 ? list[0] : null;
        }

        public bool TryGetLaneIndexOf(BaseLaneUnitObject unit, out int laneIndex)
        {
            for (int i = 0; i < _laneUnits.Length; i++)
            {
                if (_laneUnits[i].Count == 0) continue;
                if (_laneUnits[i][0] != unit) continue;
                laneIndex = i;
                return true;
            }
            laneIndex = -1;
            return false;
        }

        public void RemoveFrontUnit(int laneIndex)
        {
            var list = _laneUnits[laneIndex];
            if (list.Count == 0) return;
            list.RemoveAt(0);
            var newFront = list.Count > 0 ? list[0] : null;
            OnUnitAdvanced?.Invoke(laneIndex, newFront);
        }

        public int GetUnitCount(int laneIndex) => _laneUnits[laneIndex].Count;
        public BaseLaneUnitObject GetUnitAt(int laneIndex, int slot) => _laneUnits[laneIndex][slot];
    }
}