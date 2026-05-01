using System;

namespace Game.Views
{
    public interface ILevelEndView
    {
        event Action OnNextButtonClicked;
        event Action OnTryAgainButtonClicked;
 
        void ShowWin();
        void ShowFail();
        void HideAll();
    }
}