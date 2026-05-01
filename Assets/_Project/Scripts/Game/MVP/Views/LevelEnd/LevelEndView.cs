using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views
{
    public sealed class LevelEndView : MonoBehaviour, ILevelEndView
    {
        [field: SerializeField, Required] public GameObject WinPanel { get; private set; }
        [field: SerializeField, Required] public GameObject FailPanel { get; private set; }
        [field: SerializeField, Required] public Button NextLevelButton { get; private set; }
        [field: SerializeField, Required] public Button TryAgainButton { get; private set; }

        public event Action OnNextButtonClicked;
        public event Action OnTryAgainButtonClicked;

        private void Awake()
        {
            NextLevelButton.onClick.AddListener(InvokeNextButtonClicked);
            TryAgainButton.onClick.AddListener(InvokeTryAgainButtonClicked);
        }

        private void OnDestroy()
        {
            NextLevelButton.onClick.RemoveListener(InvokeNextButtonClicked);
            TryAgainButton.onClick.RemoveListener(InvokeTryAgainButtonClicked);
        }

        public void ShowWin()
        {
            WinPanel.SetActive(true);
            FailPanel.SetActive(false);
        }

        public void ShowFail()
        {
            WinPanel.SetActive(false);
            FailPanel.SetActive(true);
        }

        public void HideAll()
        {
            WinPanel.SetActive(false);
            FailPanel.SetActive(false);
        }

        private void InvokeNextButtonClicked() => OnNextButtonClicked?.Invoke();
        private void InvokeTryAgainButtonClicked() => OnTryAgainButtonClicked?.Invoke();
    }
}