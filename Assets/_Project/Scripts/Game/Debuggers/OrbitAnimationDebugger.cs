using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Lane.Item;
using Game.Utils;
using Game.Views;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Debugging
{
    public class OrbitAnimationDebugger : MonoBehaviour
    {
        [SerializeField] private PixelGridView _pixelGridView;
        [SerializeField] private BaseLaneUnitObject _testUnit;
        [SerializeField] private float _speed = 15f;

        private LaneUnitOrbitPath _path;

        private void OnEnable() => _pixelGridView = FindObjectOfType<PixelGridView>();

        [Button]
        public void StartTestOrbit()
        {
            if (!_pixelGridView || !_testUnit) return;

            _path = LaneUnitOrbitPathGenerator.Generate(_pixelGridView);

            int startIdx = _path.LaunchNodeIndex;
            _testUnit.transform.position = _path.Nodes[startIdx].Position;
            _testUnit.OrbitAnimation.SetPathRotationImmediate(_path.Nodes[startIdx].PathYaw);
            _testUnit.OrbitAnimation.ResetAimImmediate();

            OrbitRunner.Run(_testUnit, _path, startIdx, _speed, false, null).Forget();
        }
    }
}