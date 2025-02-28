using System;

namespace HermitCrab.Core
{
    namespace HermitCrab.Core
    {
        /// <summary>
        ///     Encapsulates pure game logic: tracking lives, levels, points and game state changes.
        /// </summary>
        public class GameLogic
        {
            public event Action OnLevelStart;
            public event Action OnLevelEnd;
            public event Action OnGameOver;
            public event Action OnWin;
            public event Action<int> OnLivesChanged; // Sends updated lives count.
            public event Action<int> OnLevelChanged; // Sends updated level count.
            public event Action<int> OnPointsChanged; // Sends updated points (score).

            private readonly int levelsToWin;

            public GameLogic(int initialLives, int levelsToWin)
            {
                Lives = initialLives;
                CurrentLevel = 1;
                this.levelsToWin = levelsToWin;
                Points = 0;
            }

            public int Lives { get; private set; }

            public int CurrentLevel { get; private set; }

            public int Points { get; private set; }

            /// <summary>
            ///     Adds the specified number of points and notifies listeners.
            /// </summary>
            /// <param name="pointsToAdd">Number of points to add.</param>
            public void AddPoints(int pointsToAdd)
            {
                Points += pointsToAdd;
                OnPointsChanged?.Invoke(Points);
            }

            /// <summary>
            ///     Call to signal that a new level is starting.
            /// </summary>
            public void StartLevel()
            {
                OnLevelStart?.Invoke();
            }

            /// <summary>
            ///     Call when the level ends (either by death or level completion).
            /// </summary>
            public void EndLevel()
            {
                OnLevelEnd?.Invoke();
            }

            /// <summary>
            ///     Should be called when the player dies.
            /// </summary>
            public void PlayerDied()
            {
                Lives--;
                OnLivesChanged?.Invoke(Lives);
                if (Lives > 0)
                {
                    CurrentLevel++;
                    OnLevelChanged?.Invoke(CurrentLevel);
                    StartLevel();
                }
                else
                {
                    OnGameOver?.Invoke();
                }
            }

            /// <summary>
            ///     Should be called when the player completes a level by reaching the door.
            /// </summary>
            public void LevelCompleted()
            {
                CurrentLevel++;
                OnLevelChanged?.Invoke(CurrentLevel);
                if (CurrentLevel > levelsToWin)
                    OnWin?.Invoke();
                else
                    StartLevel();
            }
        }
    }
}