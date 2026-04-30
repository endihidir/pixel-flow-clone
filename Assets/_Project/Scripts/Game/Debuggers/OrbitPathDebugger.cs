using Game.Data;
using Game.Utils;
using Game.Views;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Debugging
{
    public class OrbitPathDebugger : MonoBehaviour
    {
        [SerializeField] private PixelGridView _pixelGridView;
        [SerializeField] private float _nodeRadius = 0.4f;
        [SerializeField] private bool _drawNodeIndices = true;

        private LaneUnitOrbitPath _path;
        private bool _hasPath;

        [Button]
        public void GeneratePath()
        {
            if (!_pixelGridView) return;
            _path = LaneUnitOrbitPathGenerator.Generate(_pixelGridView);
            _hasPath = true;
        }

        private void OnDrawGizmos()
        {
            if (!_hasPath) return;
            
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _path.Nodes.Length; i++)
            {
                Gizmos.color = i == _path.LaunchNodeIndex ? Color.green : Color.yellow;
                Gizmos.DrawSphere(_path.Nodes[i].Position, _nodeRadius);
            }
            
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _path.Waypoints.Length; i++)
            {
                var current = _path.Waypoints[i];
                var next = _path.Waypoints[(i + 1) % _path.Waypoints.Length];
                Gizmos.DrawLine(current, next);
            }
        }
    }
}