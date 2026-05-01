using System;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Modules
{
    public sealed class ProjectileAnimationModule : MonoBehaviour
    {
        [field: SerializeField, Required] public Transform Transform { get; private set; }

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

        public void Dispose() => _moveTween?.Kill();
        private void OnDestroy() => Dispose();
    }
}