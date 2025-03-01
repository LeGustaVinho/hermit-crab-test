using System;
using UnityEngine;
using HermitCrab.Core; // for GameController
using HermitCrab.Character; // for InputController
using UnityEngine.UI;

namespace HermitCrab.UI
{
    public class UIController : MonoBehaviour
    {
        // References to UI Views (assigned via Inspector)
        public MainScreenView MainScreenView;
        public LevelScreenView LevelScreenView;
        public PopupView PopupView;
        public EndScreenView EndScreenView;

        // References to existing controllers (assigned via Inspector)
        public GameController GameController;
        public InputController InputController;

        // Event raised when UI requests to show the main screen (if needed by other systems)
        public event Action OnMainScreenRequested;

        private void OnEnable()
        {
            // Subscribe to view events
            if (MainScreenView != null)
                MainScreenView.OnStartLevelClicked += HandleStartLevelClicked;
            if (LevelScreenView != null)
            {
                LevelScreenView.OnBackClicked += HandleLevelBackClicked;
                LevelScreenView.OnMoveRightClicked += HandleMoveRightClicked;
                LevelScreenView.OnMoveLeftClicked += HandleMoveLeftClicked;
                LevelScreenView.OnJumpClicked += HandleJumpClicked;
                LevelScreenView.OnShootClicked += HandleShootClicked;
                LevelScreenView.OnPunchClicked += HandlePunchClicked;
            }
            if (PopupView != null)
                PopupView.OnCloseClicked += HandlePopupCloseClicked;
            if (EndScreenView != null)
                EndScreenView.OnBackClicked += HandleEndScreenBackClicked;

            // Subscribe to GameController events.
            if (GameController != null)
            {
                GameController.OnGameOver += HandleGameOver;
                GameController.OnGameWin += HandleWin;
                GameController.OnLivesChanged += HandleLivesChanged;
                GameController.OnLevelChanged += HandleLevelChanged;
                GameController.OnPointsChanged += HandlePointsChanged;
                GameController.OnPlayerHealthChanged += HandleHealthChanged;
                GameController.OnPlayerEnergyChanged += HandleEnergyChanged;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from view events
            if (MainScreenView != null)
                MainScreenView.OnStartLevelClicked -= HandleStartLevelClicked;
            if (LevelScreenView != null)
            {
                LevelScreenView.OnBackClicked -= HandleLevelBackClicked;
                LevelScreenView.OnMoveRightClicked -= HandleMoveRightClicked;
                LevelScreenView.OnMoveLeftClicked -= HandleMoveLeftClicked;
                LevelScreenView.OnJumpClicked -= HandleJumpClicked;
                LevelScreenView.OnShootClicked -= HandleShootClicked;
                LevelScreenView.OnPunchClicked -= HandlePunchClicked;
            }
            if (PopupView != null)
                PopupView.OnCloseClicked -= HandlePopupCloseClicked;
            if (EndScreenView != null)
                EndScreenView.OnBackClicked -= HandleEndScreenBackClicked;

            // Unsubscribe from GameController events.
            if (GameController != null)
            {
                GameController.OnGameOver -= HandleGameOver;
                GameController.OnGameWin -= HandleWin;
                GameController.OnLivesChanged -= HandleLivesChanged;
                GameController.OnLevelChanged -= HandleLevelChanged;
                GameController.OnPointsChanged -= HandlePointsChanged;
                GameController.OnPlayerHealthChanged -= HandleHealthChanged;
                GameController.OnPlayerEnergyChanged -= HandleEnergyChanged;
            }
        }

        // Show only the Main Screen view.
        public void ShowMainScreen()
        {
            if (MainScreenView != null)
                MainScreenView.Show();
            if (LevelScreenView != null)
                LevelScreenView.Hide();
            if (PopupView != null)
                PopupView.Hide();
            if (EndScreenView != null)
                EndScreenView.Hide();
        }

        // Show only the Level Screen view.
        public void ShowLevelScreen()
        {
            if (MainScreenView != null)
                MainScreenView.Hide();
            if (LevelScreenView != null)
                LevelScreenView.Show();
            if (PopupView != null)
                PopupView.Hide();
            if (EndScreenView != null)
                EndScreenView.Hide();
        }

        // Show the popup view with the provided message.
        public void ShowPopup(string message)
        {
            if (PopupView != null)
            {
                PopupView.SetMessage(message);
                PopupView.Show();
            }
        }

        // Hide the popup view.
        public void HidePopup()
        {
            if (PopupView != null)
                PopupView.Hide();
        }

        // Show the End Screen view (for Victory/Defeat) with the provided outcome and message.
        public void ShowEndScreen(bool isVictory)
        {
            if (MainScreenView != null)
                MainScreenView.Hide();
            if (LevelScreenView != null)
                LevelScreenView.Hide();
            if (PopupView != null)
                PopupView.Hide();
            if (EndScreenView != null)
                EndScreenView.Show(isVictory, GameController.FinalScore);
        }

        // Event handler for the Main Screen "Start Level" button click.
        private void HandleStartLevelClicked()
        {
            if (GameController != null)
            {
                GameController.StarLevel();
            }
            ShowLevelScreen();
        }

        // Event handler for the Level Screen "Back" button click.
        private void HandleLevelBackClicked()
        {
            ShowMainScreen();
        }

        // Event handler for the Popup "Close" button click.
        private void HandlePopupCloseClicked()
        {
            HidePopup();
        }

        // Event handler for the End Screen "Back" button click.
        private void HandleEndScreenBackClicked()
        {
            ShowMainScreen();
        }

        // Mobile input event handlers – these forward events to the InputController.
        private void HandleMoveRightClicked()
        {
            if (InputController != null)
                InputController.OnMoveRight();
        }

        private void HandleMoveLeftClicked()
        {
            if (InputController != null)
                InputController.OnMoveLeft();
        }

        private void HandleJumpClicked()
        {
            if (InputController != null)
                InputController.OnJump();
        }

        private void HandleShootClicked()
        {
            if (InputController != null)
                InputController.OnShoot();
        }

        private void HandlePunchClicked()
        {
            if (InputController != null)
                InputController.OnPunch();
        }

        // Handler for Game Over event.
        private void HandleGameOver()
        {
            ShowEndScreen(false);
        }

        // Handler for Win event.
        private void HandleWin()
        {
            ShowEndScreen(true);
        }

        // Handler for Lives Changed event.
        private void HandleLivesChanged(int lives)
        {
            if (LevelScreenView != null)
                LevelScreenView.UpdateLives(lives);
        }

        // Handler for Level Changed event.
        private void HandleLevelChanged(int level)
        {
            // Optionally update level-specific UI elements.
        }

        // Handler for Points Changed event.
        private void HandlePointsChanged(int points)
        {
            if (LevelScreenView != null)
                LevelScreenView.UpdateScore(points);
        }

        // Handler for Health Changed event.
        private void HandleHealthChanged(int health)
        {
            if (LevelScreenView != null)
                LevelScreenView.UpdateHealth(health);
        }

        // Handler for Energy Changed event.
        private void HandleEnergyChanged(int energy)
        {
            if (LevelScreenView != null)
                LevelScreenView.UpdateEnergy(energy);
        }
    }
}
