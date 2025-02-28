namespace HermitCrab.Core
{
    using System;

    /// <summary>
    /// Encapsulates pure game logic: tracking lives, levels and game state changes.
    /// </summary>
    public class GameLogic
    {
        public event Action OnLevelStart;
        public event Action OnLevelEnd;
        public event Action OnGameOver;
        public event Action OnWin;
        public event Action<int> OnLivesChanged; // Sends updated lives count.
        public event Action<int> OnLevelChanged; // Sends updated level count.

        private int lives;
        private int currentLevel;
        private int levelsToWin;

        public GameLogic(int initialLives, int levelsToWin)
        {
            lives = initialLives;
            currentLevel = 1;
            this.levelsToWin = levelsToWin;
        }

        public int Lives => lives;
        public int CurrentLevel => currentLevel;

        /// <summary>
        /// Call to signal that a new level is starting.
        /// </summary>
        public void StartLevel()
        {
            OnLevelStart?.Invoke();
        }

        /// <summary>
        /// Call when the level ends (either by death or level completion).
        /// </summary>
        public void EndLevel()
        {
            OnLevelEnd?.Invoke();
        }

        /// <summary>
        /// Should be called when the player dies.
        /// </summary>
        public void PlayerDied()
        {
            lives--;
            OnLivesChanged?.Invoke(lives);
            if (lives > 0)
            {
                currentLevel++;
                OnLevelChanged?.Invoke(currentLevel);
                StartLevel();
            }
            else
            {
                OnGameOver?.Invoke();
            }
        }

        /// <summary>
        /// Should be called when the player completes a level by reaching the door.
        /// </summary>
        public void LevelCompleted()
        {
            currentLevel++;
            OnLevelChanged?.Invoke(currentLevel);
            if (currentLevel > levelsToWin)
            {
                OnWin?.Invoke();
            }
            else
            {
                StartLevel();
            }
        }
    }

}