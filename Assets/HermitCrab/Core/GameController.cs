using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HermitCrab.Character;
using HermitCrab.Camera;
using HermitCrab.Core.HermitCrab.Core;
using HermitCrab.Level;
using HermitCrab.Level.HermitCrab.Items;
using CharacterController = HermitCrab.Character.CharacterController;

namespace HermitCrab.Core
{
    public class GameController : MonoBehaviour
    {
        [Header("Game Configuration")] 
        public GameConfig gameConfig;

        [Header("Scene References")] 
        public InputController inputController;
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
        public event Action<int> OnPlayerHealthChanged;
        public event Action<int> OnPlayerEnergyChanged;
        public event Action<int> OnPointsChanged;

        // Lista para controlar os inimigos inscritos no evento de morte.
        private List<CharacterController> enemyDeathSubscribers = new List<CharacterController>();

        private void Start()
        {
            // Initialize game logic.
            ResetGame();
        }

        /// <summary>
        /// Resets the game by creating a new GameLogic instance and subscribing to its events.
        /// </summary>
        public void ResetGame()
        {
            if (gameLogic != null)
            {
                UnsubscribeGameLogicEvents(gameLogic);
            }
            gameLogic = new GameLogic(gameConfig.initialLives, gameConfig.levelsToWin);
            SubscribeGameLogicEvents(gameLogic);
        }

        private void SubscribeGameLogicEvents(GameLogic logic)
        {
            logic.OnLevelStart += GameLogic_OnLevelStart;
            logic.OnLevelEnd += GameLogic_OnLevelEnd;
            logic.OnLevelChanged += GameLogic_OnLevelChanged;
            logic.OnLivesChanged += GameLogic_OnLivesChanged;
            logic.OnGameOver += GameLogic_OnGameOver;
            logic.OnWin += GameLogic_OnWin;
            logic.OnPointsChanged += GameLogic_OnPointsChanged;
        }

        private void UnsubscribeGameLogicEvents(GameLogic logic)
        {
            if (logic != null)
            {
                logic.OnLevelStart -= GameLogic_OnLevelStart;
                logic.OnLevelEnd -= GameLogic_OnLevelEnd;
                logic.OnLevelChanged -= GameLogic_OnLevelChanged;
                logic.OnLivesChanged -= GameLogic_OnLivesChanged;
                logic.OnGameOver -= GameLogic_OnGameOver;
                logic.OnWin -= GameLogic_OnWin;
                logic.OnPointsChanged -= GameLogic_OnPointsChanged;
            }
        }

        private void GameLogic_OnLevelStart()
        {
            OnLevelStarted?.Invoke();
        }

        private void GameLogic_OnLevelEnd()
        {
            OnLevelEnded?.Invoke();
        }

        private void GameLogic_OnLevelChanged(int level)
        {
            OnLevelChanged?.Invoke(level);
        }

        private void GameLogic_OnLivesChanged(int lives)
        {
            OnLivesChanged?.Invoke(lives);
        }

        private void GameLogic_OnGameOver()
        {
            OnGameOver?.Invoke();
        }

        private void GameLogic_OnWin()
        {
            OnGameWin?.Invoke();
        }

        private void GameLogic_OnPointsChanged(int points)
        {
            OnPointsChanged?.Invoke(points);
        }

        // Expose the final score.
        public int FinalScore => gameLogic != null ? gameLogic.Points : 0;

        public void StarLevel()
        {
            StartCoroutine(StartLevelRoutine());
        }
        
        /// <summary>
        /// Generates a new level, instantiates the player and binds scene references.
        /// </summary>
        private IEnumerator StartLevelRoutine()
        {
            // Se já houver um player na cena, desinscreve os eventos e o destrói.
            if (currentPlayer != null)
            {
                CharacterController oldPlayerController = currentPlayer.GetComponent<CharacterController>();
                if (oldPlayerController != null)
                {
                    oldPlayerController.OnPlayerDeath -= HandlePlayerDeath;
                    oldPlayerController.OnHealthChanged -= Player_OnHealthChanged;
                    oldPlayerController.OnEnergyChanged -= Player_OnEnergyChanged;
                    oldPlayerController.OnItemCollected -= Player_OnItemCollected;
                }
                OnLevelEnded?.Invoke();
                Destroy(currentPlayer);
                currentPlayer = null;
            }

            // Desinscreve os eventos de morte dos inimigos do nível anterior.
            EnemyAIController[] oldEnemyControllers = FindObjectsOfType<EnemyAIController>();
            foreach (var enemy in oldEnemyControllers)
            {
                CharacterController enemyController = enemy.enemyController;
                if (enemyController != null)
                {
                    enemyController.OnEnemyDeath -= Enemy_OnEnemyDeath;
                }
            }
            enemyDeathSubscribers.Clear();

            // Destroy previous level's procedural objects.
            if (levelGenerator.ProceduralLevelInstance != null)
            {
                Destroy(levelGenerator.ProceduralLevelInstance);
            }

            // Generate a new level.
            levelGenerator.GenerateLevel();

            // Wait one frame to allow level generation to complete.
            yield return null;

            // Get the spawn position from DoorLockedInstance.
            if (levelGenerator.DoorLockedInstance == null)
            {
                Debug.LogError("DoorLocked instance not set. Cannot spawn player.");
                yield break;
            }
            Vector3 spawnPosition = levelGenerator.DoorLockedInstance.transform.position;
            currentPlayer = Instantiate(gameConfig.playerPrefab, spawnPosition, Quaternion.identity);

            // Bind the player to the input and camera controllers.
            CharacterController playerController = currentPlayer.GetComponent<CharacterController>();
            if (playerController != null)
            {
                // Subscribe to player's events BEFORE resetting stats.
                playerController.OnPlayerDeath += HandlePlayerDeath;
                playerController.OnHealthChanged += Player_OnHealthChanged;
                playerController.OnEnergyChanged += Player_OnEnergyChanged;
                playerController.OnItemCollected += Player_OnItemCollected;

                inputController.characterController = playerController;
                cameraController.target = currentPlayer.transform;
                cameraController.Initialize();

                // Bind player reference to every EnemyAIController in the scene.
                EnemyAIController[] enemyControllers = FindObjectsOfType<EnemyAIController>();
                foreach (var enemy in enemyControllers)
                {
                    enemy.player = currentPlayer.transform;
                }

                // Subscribe to enemy death events.
                foreach (var enemyAI in enemyControllers)
                {
                    CharacterController enemyController = enemyAI.enemyController;
                    if (enemyController != null)
                    {
                        enemyController.OnEnemyDeath += Enemy_OnEnemyDeath;
                        enemyDeathSubscribers.Add(enemyController);
                    }
                }

                // Now reset player stats so that health and energy are at maximum.
                playerController.ResetStats();
            }
            else
            {
                Debug.LogError("Player prefab does not have a CharacterController component.");
            }

            // Get the door trigger instance from DoorOpenInstance and subscribe to its event.
            if (levelGenerator.DoorOpenInstance != null)
            {
                // Remove any previous subscription if exists.
                if (currentDoorTrigger != null)
                {
                    currentDoorTrigger.OnDoorTriggered -= HandleDoorTriggered;
                }

                currentDoorTrigger = levelGenerator.DoorOpenInstance.GetComponent<DoorTrigger>();
                if (currentDoorTrigger != null)
                {
                    currentDoorTrigger.OnDoorTriggered += HandleDoorTriggered;
                }
                else
                {
                    Debug.LogError("DoorOpen instance does not have a DoorTrigger component.");
                }
            }
            else
            {
                Debug.LogError("DoorOpen instance not found in the scene.");
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

        private void Player_OnHealthChanged(int health)
        {
            OnPlayerHealthChanged?.Invoke(health);
        }

        private void Player_OnEnergyChanged(int energy)
        {
            OnPlayerEnergyChanged?.Invoke(energy);
        }

        private void Player_OnItemCollected(CollectibleItemData itemData)
        {
            gameLogic.AddPoints(gameConfig.collectibleItemPoints);
        }

        private void Enemy_OnEnemyDeath()
        {
            gameLogic.AddPoints(gameConfig.enemyKillPoints);
        }

        private void OnDestroy()
        {
            // Desinscreve os eventos do GameLogic.
            UnsubscribeGameLogicEvents(gameLogic);

            // Desinscreve os eventos do player, se existir.
            if (currentPlayer != null)
            {
                CharacterController playerController = currentPlayer.GetComponent<CharacterController>();
                if (playerController != null)
                {
                    playerController.OnPlayerDeath -= HandlePlayerDeath;
                    playerController.OnHealthChanged -= Player_OnHealthChanged;
                    playerController.OnEnergyChanged -= Player_OnEnergyChanged;
                    playerController.OnItemCollected -= Player_OnItemCollected;
                }
            }

            // Desinscreve os eventos de morte dos inimigos.
            foreach (var enemyController in enemyDeathSubscribers)
            {
                if (enemyController != null)
                {
                    enemyController.OnEnemyDeath -= Enemy_OnEnemyDeath;
                }
            }
            enemyDeathSubscribers.Clear();

            // Desinscreve o evento do DoorTrigger.
            if (currentDoorTrigger != null)
            {
                currentDoorTrigger.OnDoorTriggered -= HandleDoorTriggered;
            }
        }
    }
}