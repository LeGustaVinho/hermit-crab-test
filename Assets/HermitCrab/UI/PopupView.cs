using System;
using UnityEngine;
using UnityEngine.UI;

namespace HermitCrab.UI
{
    public class PopupView : MonoBehaviour
    {
        public Text MessageText;
        public Button CloseButton;

        public event Action OnCloseClicked;

        private void Awake()
        {
            if (CloseButton != null)
                CloseButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        // Sets the popup message text.
        public void SetMessage(string message)
        {
            if (MessageText != null)
                MessageText.text = message;
        }
    }
}