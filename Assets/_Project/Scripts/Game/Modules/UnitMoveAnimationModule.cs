using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Modules
{
    public class UnitMoveAnimationModule : MonoBehaviour
    {
        [field: SerializeField, Required] public Transform Transform { get; private set; }

        public bool IsAlive => _moveTween != null && _moveTween.IsActive() && !_moveTween.IsComplete();

        private Tween _moveTween;

        public Transform GetTransform() => Transform ?? transform;

        public virtual Tween MoveTo(Vector3 position, float duration, float delay = 0f, Ease ease = Ease.OutQuad, bool useUnscaledTime = false)
        {
            _moveTween?.Kill();

            _moveTween = GetTransform().DOLocalMove(position, duration)
                .SetEase(ease)
                .SetDelay(delay)
                .SetUpdate(useUnscaledTime);

            return _moveTween;
        }

        public void Dispose() => _moveTween?.Kill(true);
        private void OnDestroy() => Dispose();
    }
}