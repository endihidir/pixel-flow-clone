using System;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Modules
{
    public sealed class ProjectileAnimationModule : MonoBehaviour
    {
        [field: SerializeField, Required] public Transform Transform { get; private set; }
        [field: SerializeField] public float BallSpeed { get; private set; } = 50f;
        [field: SerializeField] public float MinBallDuration { get; private set; } = 0.05f;
        [field: SerializeField] public float MaxBallDuration { get; private set; } = 0.2f;

        public bool IsAlive => _moveTween != null && _moveTween.IsActive() && !_moveTween.IsComplete();

        private Tween _moveTween;

        public Tween MoveTo(Vector3 worldPosition, float duration, Action onComplete = null, Ease ease = Ease.Linear)
        {
            _moveTween?.Kill();

            _moveTween = Transform.DOMove(worldPosition, duration)
                .SetEase(ease)
                .OnComplete(() => onComplete?.Invoke());

            return _moveTween;
        }

        public Tween MoveToTarget(Vector3 startPosition, Vector3 targetPosition, Action onComplete = null, Ease ease = Ease.Linear)
        {
            float duration = Mathf.Clamp(Vector3.Distance(startPosition, targetPosition) / BallSpeed, MinBallDuration, MaxBallDuration);
            return MoveTo(targetPosition, duration, onComplete, ease);
        }

        public void Dispose() => _moveTween?.Kill();

        private void OnDestroy()
        {
            _moveTween = null;
        }
    }
}