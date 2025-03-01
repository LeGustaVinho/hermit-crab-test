using System;
using System.Collections.Generic;
using HermitCrab.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HermitCrab.Character
{
    public enum EnemyType
    {
        Melee,
        Ranged
    }

    /// <summary>
    /// Implements AI for enemy characters.
    /// This controller uses a behavior tree to manage enemy states: hurt, chase, and patrol.
    /// When the enemy is hurt or the player is detected, it interrupts patrol and switches behavior accordingly.
    /// </summary>
    public class EnemyAIController : MonoBehaviour
    {
        public bool AutoInitializeOnStart = false;
        
        [Header("Enemy Settings")] 
        public EnemyConfigData enemyConfig;
        public CharacterController enemyController; // Reference to this enemy's CharacterController
        public Transform player; // Reference to the player

        [Header("Ground Detection (Avoid Cliffs)")]
        public Transform groundDetector; // Point used to detect ground ahead
        public LayerMask groundLayer; // Ground layer

        // Variables for hurt behavior (when the enemy takes damage)
        private bool isHurt;
        private Vector3 lastKnownPlayerPos;
        private float hurtTimer; // Duration of hurt state (e.g., 5 seconds)

        // Variables for random patrol behavior
        private bool isPatrolling;
        private float patrolDistanceRemaining;
        private float patrolDirection; // -1 (left) or 1 (right)
        private float patrolStartPosition;
        private bool isWaiting;
        private float waitTimer;

        // Behavior tree root node.
        private BTNode behaviorTree;

        // Cache for the player's CharacterController to avoid repeated GetComponent calls.
        private CharacterController playerController;

        private void Start()
        {
            if(AutoInitializeOnStart) Initialize();
        }

        /// <summary>
        /// Initializes the enemy AI, sets up necessary references, and builds the behavior tree.
        /// Also subscribes to the damage event from the CharacterController.
        /// </summary>
        public void Initialize()
        {
            if (enemyController == null)
            {
                enemyController = GetComponent<CharacterController>();
            }

            if (player == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null)
                {
                    player = p.transform;
                }
            }

            // Cache the player's CharacterController to avoid repeated GetComponent calls.
            if (player != null)
            {
                playerController = player.GetComponent<CharacterController>();
            }

            // Subscribe to the OnDamageReceived event to react when the enemy takes damage.
            enemyController.OnDamageReceived += OnDamaged;
            behaviorTree = BuildBehaviorTree();
        }

        /// <summary>
        /// Unsubscribes from the OnDamageReceived event when this object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (enemyController != null)
            {
                enemyController.OnDamageReceived -= OnDamaged;
            }
        }

        /// <summary>
        /// Updates the behavior tree every frame.
        /// Also decrements the hurt timer if the enemy is in a hurt state.
        /// </summary>
        private void Update()
        {
            if (isHurt)
            {
                hurtTimer -= Time.deltaTime;
                if (hurtTimer < 0f) hurtTimer = 0f;
            }

            if (enemyController == null || player == null)
                return;

            behaviorTree.Execute();
        }

        /// <summary>
        /// Draws gizmos for visualization in the Editor.
        /// Shows the detection range and ground detection ray.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyConfig.detectionRange);
            if (groundDetector != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(groundDetector.position,
                    groundDetector.position + Vector3.down * enemyConfig.groundDetectionDistance);
            }
        }

        /// <summary>
        /// Checks if there is ground ahead using a raycast.
        /// </summary>
        /// <returns>True if ground is detected; otherwise, false.</returns>
        private bool HasGroundAhead()
        {
            if (groundDetector == null)
            {
                return true;
            }

            RaycastHit2D hit = Physics2D.Raycast(groundDetector.position, Vector2.down, enemyConfig.groundDetectionDistance, groundLayer);
            return hit.collider != null;
        }

        /// <summary>
        /// Event handler for when the enemy takes damage.
        /// Sets the hurt state and records the attacker's position.
        /// Also interrupts any patrol behavior.
        /// </summary>
        /// <param name="attackerPosition">The position of the attacker (usually the player).</param>
        public void OnDamaged(Vector3 attackerPosition)
        {
            isHurt = true;
            lastKnownPlayerPos = attackerPosition;
            hurtTimer = 5f;
            // Immediately cancel any ongoing patrol or waiting.
            isPatrolling = false;
            isWaiting = false;
        }

        /// <summary>
        /// Hurt branch of the behavior tree.
        /// Moves the enemy toward the last known player position when hurt.
        /// </summary>
        /// <returns>BTStatus indicating Running until destination reached and wait time elapses, then Success.</returns>
        private BTStatus HurtBranch()
        {
            if (!isHurt)
            {
                return BTStatus.Failure;
            }

            float dist = Vector2.Distance(transform.position, lastKnownPlayerPos);
            if (dist > 0.2f)
            {
                Vector2 direction = (lastKnownPlayerPos - transform.position).normalized;
                enemyController.Move(direction.x, false);
                return BTStatus.Running;
            }

            enemyController.Move(0, false);
            if (hurtTimer > 0f)
            {
                return BTStatus.Running;
            }

            isHurt = false;
            return BTStatus.Success;
        }

        /// <summary>
        /// Chase branch of the behavior tree.
        /// Chases and attacks the player if within detection range.
        /// </summary>
        /// <returns>BTStatus Running while chasing, and Success when attack is executed.</returns>
        private BTStatus ChaseBranch()
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            if (distToPlayer <= enemyConfig.attackRange)
            {
                // Ensure enemy faces the player before attacking
                float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
                enemyController.Move(directionToPlayer, false);
                enemyController.Move(0, false);
                
                if (enemyConfig.enemyType == EnemyType.Melee)
                {
                    enemyController.Punch();
                }
                else
                {
                    enemyController.Shoot();
                }
                return BTStatus.Success;
            }

            if (HasGroundAhead())
            {
                Vector2 direction = (player.position - transform.position).normalized;
                enemyController.Move(direction.x, false);
            }
            else
            {
                enemyController.Move(0, false);
            }
            return BTStatus.Running;
        }

        /// <summary>
        /// Patrol branch of the behavior tree.
        /// Executes random patrol when the player is not detected.
        /// Patrol is interrupted if the enemy is hurt or the player is detected.
        /// </summary>
        /// <returns>BTStatus Running during patrol and waiting, Success when a patrol cycle completes, or Failure if interrupted.</returns>
        private BTStatus PatrolBranch()
        {
            // Interrupt patrol if enemy is hurt or player is within detection range and alive.
            if (isHurt || (Vector2.Distance(transform.position, player.position) <= enemyConfig.detectionRange &&
                playerController?.IsAlive == true))
            {
                isPatrolling = false;
                isWaiting = false;
                return BTStatus.Failure;
            }

            if (!isPatrolling && !isWaiting)
            {
                // Start a new patrol cycle: choose a random direction and distance.
                isPatrolling = true;
                patrolDirection = Random.value < 0.5f ? -1f : 1f;
                patrolDistanceRemaining = Random.Range(enemyConfig.patrolDistanceMin, enemyConfig.patrolDistanceMax);
                patrolStartPosition = transform.position.x;
                return BTStatus.Running;
            }

            if (isPatrolling)
            {
                float traveled = Mathf.Abs(transform.position.x - patrolStartPosition);
                if (traveled < patrolDistanceRemaining)
                {
                    if (HasGroundAhead())
                    {
                        enemyController.Move(patrolDirection, false);
                    }
                    else
                    {
                        enemyController.Move(0, false);
                    }
                    return BTStatus.Running;
                }

                // Reached patrol destination; begin waiting period.
                enemyController.Move(0, false);
                isPatrolling = false;
                isWaiting = true;
                waitTimer = Random.Range(enemyConfig.patrolWaitMin, enemyConfig.patrolWaitMax);
                return BTStatus.Running;
            }

            if (isWaiting)
            {
                waitTimer -= Time.deltaTime;
                enemyController.Move(0, false);
                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    return BTStatus.Success;
                }
                return BTStatus.Running;
            }

            return BTStatus.Success;
        }

        /// <summary>
        /// Builds the enemy's behavior tree.
        /// Priority order: Hurt > Chase > Patrol.
        /// </summary>
        /// <returns>The root node of the behavior tree.</returns>
        private BTNode BuildBehaviorTree()
        {
            BTNode hurtNode = new BTAction(() => HurtBranch());
            BTNode chaseNode = new BTAction(() =>
            {
                // If the player is within detection range and alive, execute the chase branch.
                if (Vector2.Distance(transform.position, player.position) <= enemyConfig.detectionRange &&
                    playerController.IsAlive)
                {
                    return ChaseBranch();
                }
                return BTStatus.Failure;
            });
            BTNode patrolNode = new BTAction(() => PatrolBranch());

            BTNode root = new StatelessBTSelector(new List<BTNode> { hurtNode, chaseNode, patrolNode });
            return root;
        }
    }
}