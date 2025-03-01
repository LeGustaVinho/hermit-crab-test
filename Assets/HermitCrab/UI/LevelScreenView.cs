using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HermitCrab.UI
{
    public class LevelScreenView : MonoBehaviour
    {
        public Slider HealthSlider;
        public Slider EnergySlider;
        public Image[] LivesImages;
        public TextMeshProUGUI ScoreText;
        public Button BackButton;

        // Mobile input buttons
        public ContinuousPointerEventUpdate MoveRightButton;
        public ContinuousPointerEventUpdate MoveLeftButton;
        public Button JumpButton;
        public Button ShootButton;
        public Button PunchButton;

        public event Action OnBackClicked;
        public event Action OnMoveRightClicked;
        public event Action OnMoveLeftClicked;
        public event Action OnJumpClicked;
        public event Action OnShootClicked;
        public event Action OnPunchClicked;

        private void Awake()
        {
            if (BackButton != null)
                BackButton.onClick.AddListener(() => OnBackClicked?.Invoke());
            if (MoveRightButton != null)
                MoveRightButton.OnRepeatEvent += (() => OnMoveRightClicked?.Invoke());
            if (MoveLeftButton != null)
                MoveLeftButton.OnRepeatEvent += (() => OnMoveLeftClicked?.Invoke());
            if (JumpButton != null)
                JumpButton.onClick.AddListener(() => OnJumpClicked?.Invoke());
            if (ShootButton != null)
                ShootButton.onClick.AddListener(() => OnShootClicked?.Invoke());
            if (PunchButton != null)
                PunchButton.onClick.AddListener(() => OnPunchClicked?.Invoke());
        }

        // Updates the health progress bar.
        public void UpdateHealth(float value)
        {
            if (HealthSlider != null)
                HealthSlider.value = value;
        }

        // Updates the energy progress bar.
        public void UpdateEnergy(float value)
        {
            if (EnergySlider != null)
                EnergySlider.value = value;
        }

        // Updates the lives indicator (enables images for remaining lives).
        public void UpdateLives(int lives)
        {
            if (LivesImages != null)
            {
                for (int i = 0; i < LivesImages.Length; i++)
                {
                    LivesImages[i].enabled = i < lives;
                }
            }
        }

        // Updates the score text.
        public void UpdateScore(int score)
        {
            if (ScoreText != null)
                ScoreText.text = score.ToString();
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