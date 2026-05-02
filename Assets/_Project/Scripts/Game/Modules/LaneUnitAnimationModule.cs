using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Modules
{
    public sealed class LaneUnitAnimationModule : MonoBehaviour
    {
        [field: SerializeField, Required] public Transform Transform { get; private set; }
        [field: SerializeField, Required] public Transform UnitHolder { get; private set; }
        [field: SerializeField] public float OrbitSpeed { get; private set; } = 15f;
        [field: SerializeField] public float AimLocalYaw { get; private set; } = -90f;
        [field: SerializeField] public float AimRotateDuration { get; private set; } = 0.02f;
        [field: SerializeField] public float DefaultJumpDuration { get; private set; } = 0.4f;
        [field: SerializeField] public float DefaultJumpPower { get; private set; } = 10f;
        [field: SerializeField] public float ScaleDownDuration { get; private set; } = 0.2f;

        private Tween _moveTween;
        private Tween _rotateTween;
        private Tween _aimTween;
        private Tween _scaleTween;
        private Tween _jumpTween;
        
        public bool IsJumping => _jumpTween != null && _jumpTween.IsActive() && _jumpTween.IsPlaying();
        
        public Tween MoveLocalTo(Vector3 localPos, float duration, float delay = 0f, Ease ease = Ease.OutQuad)
        {
            KillTransformPositionTweens();
            _moveTween = Transform.DOLocalMove(localPos, duration).SetDelay(delay).SetEase(ease);
            return _moveTween;
        }

        public Tween JumpTo(Vector3 worldPos, float jumpPower = 0, float duration = 0)
        {
            KillTransformPositionTweens();
            var power = jumpPower > 0 ? jumpPower : DefaultJumpPower;
            var dur = duration > 0 ? duration : DefaultJumpDuration;
            _jumpTween = Transform.DOJump(worldPos, power, 1, dur).SetEase(Ease.Linear);
            return _jumpTween;
        }

        public Tween ScaleDown()
        {
            _scaleTween?.Kill();
            _scaleTween = Transform.DOScale(Vector3.zero, ScaleDownDuration).SetEase(Ease.InBack);
            return _scaleTween;
        }

        public Tween MoveSegment(Vector3 toPos, float duration)
        {
            KillTransformPositionTweens();
            _moveTween = Transform.DOMove(toPos, duration).SetEase(Ease.Linear);
            return _moveTween;
        }

        public Tween RotateToPathYaw(float yaw, float duration)
        {
            _rotateTween?.Kill();
            _rotateTween = Transform.DORotateQuaternion(Quaternion.Euler(0f, yaw, 0f), duration).SetEase(Ease.Linear);
            return _rotateTween;
        }

        public void SetPathRotationImmediate(float yaw)
        {
            _rotateTween?.Kill();
            Transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        public Tween AimAtPixelGrid()
        {
            _aimTween?.Kill();
            _aimTween = UnitHolder.DOLocalRotateQuaternion(Quaternion.Euler(0f, AimLocalYaw, 0f), AimRotateDuration).SetEase(Ease.OutSine);
            return _aimTween;
        }

        public void SetAimImmediate()
        {
            _aimTween?.Kill();
            UnitHolder.localRotation = Quaternion.Euler(0f, AimLocalYaw, 0f);
        }

        public Tween ResetAim(float duration = 0f)
        {
            _aimTween?.Kill();
            var dur = duration > 0 ? duration : AimRotateDuration;
            _aimTween = UnitHolder.DOLocalRotateQuaternion(Quaternion.identity, dur).SetEase(Ease.OutSine);
            return _aimTween;
        }

        public void ResetAimImmediate()
        {
            _aimTween?.Kill();
            UnitHolder.localRotation = Quaternion.identity;
        }
        
        private void KillTransformPositionTweens()
        {
            _moveTween?.Kill();
            _jumpTween?.Kill();
        }

        public void Dispose()
        {
            _moveTween?.Kill();
            _jumpTween?.Kill();
            _rotateTween?.Kill();
            _aimTween?.Kill();
            _scaleTween?.Kill();

            Transform.rotation = Quaternion.identity;
            UnitHolder.localRotation = Quaternion.identity;
            ResetScale();
        }
        
        public void ResetScale() => Transform.localScale = Vector3.one;

        private void OnDestroy()
        {
            _moveTween = null;
            _rotateTween = null;
            _aimTween = null;
            _scaleTween = null;
            _jumpTween = null;
        }
    }
}