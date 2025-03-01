using UnityEngine;


namespace HermitCrab.Character
{
    [CreateAssetMenu(fileName = "EnemyConfigData", menuName = "Game/EnemyConfigData")]
    public class EnemyConfigData : ScriptableObject 
    {
        [Header("Detection & Attack")]
        public float detectionRange = 10f;
        public float attackRange = 2f;
        public EnemyType enemyType; // Melee or Ranged
    
        [Header("Patrol Settings")]
        public float patrolDistanceMin = 3f;
        public float patrolDistanceMax = 6f;
        public float patrolWaitMin = 2f;
        public float patrolWaitMax = 4f;

        [Header("Ground Detection")]
        public float groundDetectionDistance = 1f;
    
        [Header("Hurt Behavior")]
        public float hurtDistanceThreshold = 0.2f;
        public float hurtDuration = 5f;
    }
}