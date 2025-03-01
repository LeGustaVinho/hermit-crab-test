﻿using System;
using System.Collections;
using UnityEngine;
using HermitCrab.Character;
using HermitCrab.Camera;
using HermitCrab.Core.HermitCrab.Core;
using HermitCrab.Level;
using CharacterController = HermitCrab.Character.CharacterController;

namespace HermitCrab.Core
{
    public class GameController : MonoBehaviour
    {
        [Header("Game Configuration")] public GameConfig gameConfig;

        [Header("Scene References")] public InputController inputController;
        public CameraControllerCinemachine cameraController;
        public ProceduralLevelGeneratorRuntime levelGenerator;

        private GameLogic gameLogic;
        private GameObject currentPlayer;
        private DoorTrigger currentDoorTrigger; // Current instance of DoorTrigger

        // Events for state changes
        public event Action OnLevelStarted;
        public event Action OnLevelEnded;
        public event Action<int> OnLevelChanged;
        public event Action<int> OnLivesChanged;
        public event Action OnPlayerDied;
        public event Action OnGameOver;
        public event Action OnGameWin;

        private void Start()
        {
            // Initialize game logic with settings from the ScriptableObject.
            gameLogic = new GameLogic(gameConfig.initialLives, gameConfig.levelsToWin);
            // Subscribe to game logic events and re-fire them through GameController events.
            gameLogic.OnLevelStart += () => { OnLevelStarted?.Invoke(); };
            gameLogic.OnLevelEnd += () => { OnLevelEnded?.Invoke(); };
            gameLogic.OnLevelChanged += (level) => { OnLevelChanged?.Invoke(level); };
            gameLogic.OnLivesChanged += (lives) => { OnLivesChanged?.Invoke(lives); };
            gameLogic.OnGameOver += () => { OnGameOver?.Invoke(); };
            gameLogic.OnWin += () => { OnGameWin?.Invoke(); };
            
        }

        public void StarLevel()
        {
            StartCoroutine(StartLevelRoutine());
        }
        
        /// <summary>
        /// Generates a new level, instantiates the player and binds scene references.
        /// </summary>
        private IEnumerator StartLevelRoutine()
        {
            // Signal level end if there was a previous level.
            if (currentPlayer != null)
            {
                OnLevelEnded?.Invoke();
                Destroy(currentPlayer);
            }

            // Destroy previous level's procedural objects.
            GameObject previousLevel = GameObject.Find("ProceduralLevel");
            if (previousLevel != null)
            {
                Destroy(previousLevel);
            }

            // Generate a new level.
            levelGenerator.GenerateLevel();

            // Wait one frame to allow level generation to complete.
            yield return null;

            // Find the spawn position from "DoorLocked(Clone)".
            GameObject doorLocked = GameObject.Find("DoorLocked(Clone)");
            if (doorLocked == null)
            {
                Debug.LogError("DoorLocked(Clone) not found in the scene. Cannot spawn player.");
                yield break;
            }

            Vector3 spawnPosition = doorLocked.transform.position;
            currentPlayer = Instantiate(gameConfig.playerPrefab, spawnPosition, Quaternion.identity);

            // Bind the player to the input and camera controllers.
            CharacterController playerController = currentPlayer.GetComponent<CharacterController>();
            if (playerController != null)
            {
                inputController.characterController = playerController;
                cameraController.target = currentPlayer.transform;
                cameraController.Initialize();

                // Bind player reference to every EnemyAIController in the scene.
                EnemyAIController[] enemyControllers = FindObjectsOfType<EnemyAIController>();
                foreach (var enemy in enemyControllers)
                {
                    enemy.player = currentPlayer.transform;
                }

                // Subscribe to the player's death event.
                playerController.OnPlayerDeath += HandlePlayerDeath;
                
                // Subscribe to the player's item collection event.
                playerController.OnItemCollected += (itemData) =>
                {
                    gameLogic.AddPoints(gameConfig.collectibleItemPoints);
                };

                // Subscribe to enemy death events.
                foreach (var enemyAI in enemyControllers)
                {
                    CharacterController enemyController = enemyAI.enemyController;
                    if (enemyController != null)
                    {
                        enemyController.OnEnemyDeath += () =>
                        {
                            gameLogic.AddPoints(gameConfig.enemyKillPoints);
                        };
                    }
                }
            }
            else
            {
                Debug.LogError("Player prefab does not have a CharacterController component.");
            }

            // Find the door trigger instance from "DoorOpen(Clone)" and subscribe to its event.
            GameObject doorOpen = GameObject.Find("DoorOpen(Clone)");
            if (doorOpen != null)
            {
                // Remove any previous subscription if exists.
                if (currentDoorTrigger != null)
                {
                    currentDoorTrigger.OnDoorTriggered -= HandleDoorTriggered;
                }

                currentDoorTrigger = doorOpen.GetComponent<DoorTrigger>();
                if (currentDoorTrigger != null)
                {
                    currentDoorTrigger.OnDoorTriggered += HandleDoorTriggered;
                }
                else
                {
                    Debug.LogError("DoorOpen(Clone) does not have a DoorTrigger component.");
                }
            }
            else
            {
                Debug.LogError("DoorOpen(Clone) not found in the scene.");
            }

            // Signal that a new level has started.
            gameLogic.StartLevel();
        }

        /// <summary>
        /// Handles the player's death event.
        /// </summary>
        private void HandlePlayerDeath()
        {
            OnPlayerDied?.Invoke();
            gameLogic.PlayerDied();
            if (gameLogic.Lives > 0)
            {
                StartCoroutine(StartLevelRoutine());
            }
        }

        /// <summary>
        /// Handles the door trigger event (player reached the door).
        /// </summary>
        private void HandleDoorTriggered()
        {
            gameLogic.LevelCompleted();
            StartCoroutine(StartLevelRoutine());
        }
    }
}