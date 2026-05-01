using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Modules
{
    public sealed class LaneUnitMotionAnimationModule : MonoBehaviour
    {
        [field: SerializeField, Required] public Transform Transform { get; private set; }
        [field: SerializeField] public float ScaleDownDuration { get; private set; } = 0.2f;

        private Tween _moveTween;
        private Tween _scaleTween;

        public Tween MoveTo(Vector3 worldPos, float duration, Ease ease = Ease.OutQuad)
        {
            _moveTween?.Kill();
            _moveTween = Transform.DOMove(worldPos, duration).SetEase(ease);
            return _moveTween;
        }

        public Tween JumpTo(Vector3 worldPos, float jumpPower, float duration)
        {
            _moveTween?.Kill();
            _moveTween = Transform.DOJump(worldPos, jumpPower, 1, duration).SetEase(Ease.Linear);
            return _moveTween;
        }

        public Tween ScaleDown()
        {
            _scaleTween?.Kill();
            _scaleTween = Transform.DOScale(Vector3.zero, ScaleDownDuration).SetEase(Ease.InBack);
            return _scaleTween;
        }

        public void ResetScale() => Transform.localScale = Vector3.one;

        public void Dispose()
        {
            _moveTween?.Kill();
            _scaleTween?.Kill();
        }

        private void OnDestroy() => Dispose();
    }
}