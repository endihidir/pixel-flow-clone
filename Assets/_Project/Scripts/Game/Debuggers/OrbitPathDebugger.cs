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
        
        private bool IsAppPlaying => Application.isPlaying;

        [Button, ShowIf(nameof(IsAppPlaying))]
        public void GeneratePath()
        {
            if (!_pixelGridView) return;
            _path = LaneUnitOrbitPathGenerator.Generate(_pixelGridView);
            _hasPath = true;
        }

        private void OnDrawGizmos()
        {
            if (!_drawNodeIndices) return;
            if (!_hasPath) return;

            for (int i = 0; i < _path.Nodes.Length; i++)
            {
                var node = _path.Nodes[i];
                if (i == _path.LaunchNodeIndex) Gizmos.color = Color.green;
                else if (node.IsTriggerNode) Gizmos.color = Color.yellow;
                else Gizmos.color = Color.gray;
                Gizmos.DrawSphere(node.Position, _nodeRadius);
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < _path.Nodes.Length; i++)
            {
                var current = _path.Nodes[i].Position;
                var next = _path.Nodes[(i + 1) % _path.Nodes.Length].Position;
                Gizmos.DrawLine(current, next);
            }
        }
    }
}