using System;
using UnityEngine;
using UnityEngine.UI;

namespace HermitCrab.UI
{
    public class EndScreenView : MonoBehaviour
    {
        public Text MessageText;
        public Button BackButton;

        public event Action OnBackClicked;

        private void Awake()
        {
            if (BackButton != null)
                BackButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        }

        // Sets the end screen message (e.g., Victory or Defeat).
        public void SetMessage(string message)
        {
            if (MessageText != null)
                MessageText.text = message;
        }
    }
}