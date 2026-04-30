using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Game.Services
{
    public sealed class InputService : IInputService, IDisposable
    {
        private readonly InputActions _actions = new();
        public event Action<Vector2> OnTap;

        private bool _isPointerOverUI;

        public InputService()
        {
            Input.multiTouchEnabled = false;
        }

        public void Enable()
        {
            _actions.Enable();
            _actions.Board.Press.canceled += OnPressCanceled;
        }

        private void OnPressCanceled(InputAction.CallbackContext ctx)
        {
            if (_isPointerOverUI) return;
            
            var mousePos = _actions.Board.Position.ReadValue<Vector2>();
            
            OnTap?.Invoke(mousePos);
        }

        public void Disable()
        {
            _actions.Board.Press.canceled -= OnPressCanceled;
            _actions.Disable();
        }

        public void Dispose() => Disable();

        private static bool GetPointerOverUI()
        {
            if (!EventSystem.current) return false;
            if (EventSystem.current.IsPointerOverGameObject()) return true;

            if (Touchscreen.current == null) return false;
            var touch = Touchscreen.current.primaryTouch;
            if (!touch.press.isPressed) return false;

            return EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue());
        }

        public void Update() => _isPointerOverUI = GetPointerOverUI();
    }
}
