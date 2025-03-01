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
        }

        // Show only the Main Screen view.
        public void ShowMainScreen()
        {
            if (MainScreenView != null)
                MainScreenView.gameObject.SetActive(true);
            if (LevelScreenView != null)
                LevelScreenView.gameObject.SetActive(false);
            if (PopupView != null)
                PopupView.gameObject.SetActive(false);
            if (EndScreenView != null)
                EndScreenView.gameObject.SetActive(false);
        }

        // Show only the Level Screen view.
        public void ShowLevelScreen()
        {
            if (MainScreenView != null)
                MainScreenView.gameObject.SetActive(false);
            if (LevelScreenView != null)
                LevelScreenView.gameObject.SetActive(true);
            if (PopupView != null)
                PopupView.gameObject.SetActive(false);
            if (EndScreenView != null)
                EndScreenView.gameObject.SetActive(false);
        }

        // Show the popup view with the provided message.
        public void ShowPopup(string message)
        {
            if (PopupView != null)
            {
                PopupView.SetMessage(message);
                PopupView.gameObject.SetActive(true);
            }
        }

        // Hide the popup view.
        public void HidePopup()
        {
            if (PopupView != null)
                PopupView.gameObject.SetActive(false);
        }

        // Show the End Screen view (for Victory/Defeat) with the provided message.
        public void ShowEndScreen(string message)
        {
            if (MainScreenView != null)
                MainScreenView.gameObject.SetActive(false);
            if (LevelScreenView != null)
                LevelScreenView.gameObject.SetActive(false);
            if (PopupView != null)
                PopupView.gameObject.SetActive(false);
            if (EndScreenView != null)
            {
                EndScreenView.SetMessage(message);
                EndScreenView.gameObject.SetActive(true);
            }
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
    }
}