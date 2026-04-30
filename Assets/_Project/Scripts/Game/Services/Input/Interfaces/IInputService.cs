using System;
using UnityEngine;

namespace Game.Services
{
    public interface IInputService
    {
        event Action<Vector2> OnTap;
        void Enable();
        void Update();
        void Disable();
    }
}