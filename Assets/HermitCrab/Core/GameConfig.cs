using UnityEngine;

namespace HermitCrab.Core
{
    /// <summary>
    /// Stores game constants, configuration data and asset references.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [Header("Prefabs")]
        public GameObject playerPrefab;

        [Header("Game Settings")]
        public int initialLives = 3;
        public int levelsToWin = 5;
    }
}