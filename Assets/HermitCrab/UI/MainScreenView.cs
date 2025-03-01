using System;
using UnityEngine;
using UnityEngine.UI;

namespace HermitCrab.UI
{
    public class MainScreenView : MonoBehaviour
    {
        public Button StartLevelButton;

        public event Action OnStartLevelClicked;

        private void Awake()
        {
            if (StartLevelButton != null)
                StartLevelButton.onClick.AddListener(InvokeStartLevelClicked);
        }

        private void OnDestroy()
        {
            if (StartLevelButton != null)
                StartLevelButton.onClick.RemoveListener(InvokeStartLevelClicked);
        }

        private void InvokeStartLevelClicked()
        {
            OnStartLevelClicked?.Invoke();
        }

        // New method to show the view.
        public void Show()
        {
            gameObject.SetActive(true);
        }

        // New method to hide the view.
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}