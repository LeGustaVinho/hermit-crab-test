using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HermitCrab.UI
{
    public class EndScreenView : MonoBehaviour
    {
        public Button BackButton;

        public event Action OnBackClicked;
        
        // New fields for alternating end game panels.
        public GameObject panelVictory;
        public GameObject panelLose;
        
        public TextMeshProUGUI ScoreIndicator;
        
        private void Awake()
        {
            if (BackButton != null)
                BackButton.onClick.AddListener(HandleBackButtonClicked);
        }

        // Called when the Back button is clicked.
        private void HandleBackButtonClicked()
        {
            OnBackClicked?.Invoke();
        }
        
        private void OnDestroy()
        {
            if (BackButton != null)
                BackButton.onClick.RemoveListener(HandleBackButtonClicked);
        }

        // New method to show the end screen with the appropriate panel.
        public void Show(bool isVictory, int score)
        {
            if (isVictory)
            {
                if (panelVictory != null)
                    panelVictory.SetActive(true);
                if (panelLose != null)
                    panelLose.SetActive(false);
            }
            else
            {
                if (panelVictory != null)
                    panelVictory.SetActive(false);
                if (panelLose != null)
                    panelLose.SetActive(true);
            }
            
            // Update the score indicator.
            if (ScoreIndicator != null)
                ScoreIndicator.text = score.ToString();
            
            // Optionally update the message text.
            gameObject.SetActive(true);
        }

        // New method to hide the end screen.
        public void Hide()
        {
            if (panelVictory != null)
                panelVictory.SetActive(false);
            if (panelLose != null)
                panelLose.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}