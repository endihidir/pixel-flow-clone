using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Modules
{
    public sealed class PixelCellAnimationModule : MonoBehaviour
    {
        [field: SerializeField, Required] public Transform Transform { get; private set; }

        [field: SerializeField] public Vector3 PunchScale { get; private set; } = new (0.3f, 0.3f, 0.3f);
        [field: SerializeField] public float PunchDuration { get; private set; } = 0.15f;
        [field: SerializeField] public float ScaleDownDuration { get; private set; } = 0.15f;

        private Sequence _sequence;

        public bool IsAlive => _sequence != null && _sequence.IsActive() && !_sequence.IsComplete();

        public Sequence PlayBounceAndScaleDown()
        {
            _sequence?.Kill();

            _sequence = DOTween.Sequence();
            _sequence.Append(Transform.DOPunchScale(PunchScale, PunchDuration));
            _sequence.Append(Transform.DOScale(Vector3.zero, ScaleDownDuration).SetEase(Ease.InBack));

            return _sequence;
        }

        public void Dispose() => _sequence?.Kill();
        private void OnDestroy()
        {
            _sequence = null;
        }
    }
}