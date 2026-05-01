using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Modules
{
    public sealed class LaneUnitOrbitAnimationModule : MonoBehaviour
    {
        [field: SerializeField, Required] public Transform Transform { get; private set; }
        [field: SerializeField, Required] public Transform UnitHolder { get; private set; }
        [field: SerializeField] public float OrbitSpeed { get; private set; } = 15f;
        [field: SerializeField] public float AimLocalYaw { get; private set; } = -90f;
        [field: SerializeField] public float AimRotateDuration { get; private set; } = 0.02f;

        private Tween _moveTween;
        private Tween _rotateTween;
        private Tween _aimTween;

        public Tween MoveSegment(Vector3 toPos, float duration)
        {
            _moveTween?.Kill();
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

        public Tween AimAtPixel()
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

        public void ResetAimImmediate()
        {
            _aimTween?.Kill();
            UnitHolder.localRotation = Quaternion.identity;
        }

        public void Dispose()
        {
            _moveTween?.Kill();
            _rotateTween?.Kill();
            _aimTween?.Kill();

            Transform.rotation = Quaternion.identity;
            UnitHolder.localRotation = Quaternion.identity;
        }

        private void OnDestroy()
        {
            _moveTween = null;
            _rotateTween = null;
            _aimTween = null;
        }
    }
}