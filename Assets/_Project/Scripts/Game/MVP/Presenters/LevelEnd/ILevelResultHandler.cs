using System;

namespace Game.Handlers
{
    public interface ILevelResultHandler
    {
        event Action OnLevelSuccess;
        event Action OnLevelFail;
        void Reset();
        void Dispose();
    }
}