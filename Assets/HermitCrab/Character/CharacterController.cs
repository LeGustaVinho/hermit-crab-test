using System;
using System.Collections;
using HermitCrab.Level;
using HermitCrab.Level.HermitCrab.Items;
using UnityEngine;

namespace HermitCrab.Character
{
    /// <summary>
    ///     Controls the character behavior including movement, jumping, attacking, and damage reception.
    ///     This component interfaces with the CharacterLogic class to process actions and updates the Animator
    ///     and physics (Rigidbody2D) accordingly.
    /// </summary>
    public class CharacterController : MonoBehaviour
    {
        #region Public Fields

        [Header("Character Settings")]
        // Character configuration data (includes references such as the projectile prefab).
        public CharacterData characterData;

        [Header("Spawn and Ground Check")]
        // Transform used as the spawn point for projectiles.
        public Transform projectileSpawnPoint;

        // Transform used to check if the character is on the ground.
        public Transform groundCheck;

        // Radius used for ground detection.
        public float groundCheckRadius = 0.1f;

        // Layer mask specifying which layers count as ground.
        public LayerMask groundLayer;

        [Header("Punch Settings")]
        // The range within which a punch can hit enemies.
        public float punchRange = 1f;

        // Layer mask specifying which layers contain enemies.
        public LayerMask enemyLayer;

        #endregion

        #region Public Events

        /// <summary>
        ///     Event raised when the character receives damage.
        ///     The event passes the position of the attacker.
        /// </summary>
        public event Action<Vector3> OnDamageReceived;
        
        // New event to notify when the player dies.
        public event Action OnPlayerDeath;
        
        /// <summary>
        /// Event raised when an enemy dies.
        /// </summary>
        public event Action OnEnemyDeath;
        
        /// <summary>
        /// Event raised when an item is collected.
        /// </summary>
        public event Action<CollectibleItemData> OnItemCollected;

        // New events to notify health and energy changes.
        public event Action<int> OnHealthChanged;
        public event Action<int> OnEnergyChanged;

        #endregion

        #region Private Fields

        private CharacterLogic logic;
        private Animator animator;
        private Rigidbody2D rb;

        // Variables to control movement input.
        private float horizontalInput;
        private bool isRunning;

        // Variables to track changes in input.
        private float lastHorizontalInput;
        private bool lastIsRunning;

        // State flags.
        private bool isDead; // Indicates if the character is dead.
        private bool isExternallyAffected = false;

        // Cooldown timers for actions (in seconds).
        private float lastPunchTime = -Mathf.Infinity;
        private float lastShootTime = -Mathf.Infinity;
        private const float actionCooldown = 1f;

        // Added fields for visual feedback on damage.
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private bool isPoisoned = false;

        // New field to track the FlashRed coroutine.
        private Coroutine flashRedCoroutine;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the health changed event from logic.
        /// </summary>
        private void HandleHealthChanged(int health)
        {
            OnHealthChanged?.Invoke(health);
        }

        /// <summary>
        /// Handles the energy changed event from logic.
        /// </summary>
        private void HandleEnergyChanged(int energy)
        {
            OnEnergyChanged?.Invoke(energy);
        }

        /// <summary>
        /// Handles the jump event from logic.
        /// </summary>
        private void OnJumpHandler()
        {
            if (!isDead)
            {
                animator.SetTrigger(characterData.jumpTriggerHash);
            }
        }

        /// <summary>
        /// Handles the double jump event from logic.
        /// </summary>
        private void OnDoubleJumpHandler()
        {
            if (!isDead)
            {
                animator.SetTrigger(characterData.doubleJumpTriggerHash);
            }
        }

        /// <summary>
        /// Handles the shoot event from logic.
        /// </summary>
        private void OnShootHandler()
        {
            if (!isDead)
            {
                animator.SetTrigger(characterData.shootTriggerHash);
            }
        }

        /// <summary>
        /// Handles the jump shoot event from logic.
        /// </summary>
        private void OnJumpShootHandler()
        {
            if (!isDead)
            {
                animator.SetTrigger(characterData.jumpShootTriggerHash);
            }
        }

        /// <summary>
        /// Handles the run shoot event from logic.
        /// </summary>
        private void OnRunShootHandler()
        {
            if (!isDead)
            {
                animator.SetTrigger(characterData.runShootTriggerHash);
            }
        }

        /// <summary>
        /// Handles the punch event from logic.
        /// </summary>
        private void OnPunchHandler()
        {
            if (!isDead)
            {
                animator.SetTrigger(characterData.punchTriggerHash);
            }
        }

        /// <summary>
        /// Handles the jump punch event from logic.
        /// </summary>
        private void OnJumpPunchHandler()
        {
            if (!isDead)
            {
                animator.SetTrigger(characterData.jumpPunchTriggerHash);
            }
        }

        /// <summary>
        /// Handles the death event from logic.
        /// </summary>
        private void OnDeathHandler()
        {
            if (!isDead)
            {
                animator.SetTrigger(characterData.dieTriggerHash);
                Die();
            }
        }

        #endregion

        #region Unity Methods

        /// <summary>
        ///     Initializes the component by obtaining references and initializing logic.
        ///     Subscribes to events from the CharacterLogic.
        /// </summary>
        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            logic = new CharacterLogic(characterData);
            SubscribeToEvents();
            // Initialize spriteRenderer and original color.
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            // Re-fire logic events for health and energy changes.
            logic.OnHealthChanged += HandleHealthChanged;
            logic.OnEnergyChanged += HandleEnergyChanged;
        }

        /// <summary>
        ///     Subscribes to events from CharacterLogic to update the animator triggers.
        /// </summary>
        private void SubscribeToEvents()
        {
            // Subscribe to jump event.
            logic.OnJump += OnJumpHandler;
            // Subscribe to double jump event.
            logic.OnDoubleJump += OnDoubleJumpHandler;
            // Subscribe to shoot event.
            logic.OnShoot += OnShootHandler;
            // Subscribe to jump shoot event.
            logic.OnJumpShoot += OnJumpShootHandler;
            // Subscribe to run shoot event.
            logic.OnRunShoot += OnRunShootHandler;
            // Subscribe to punch event.
            logic.OnPunch += OnPunchHandler;
            // Subscribe to jump punch event.
            logic.OnJumpPunch += OnJumpPunchHandler;
            // Subscribe to death event.
            logic.OnDeath += OnDeathHandler;
            // Subscribe to projectile creation event.
            logic.OnProjectileCreated += CreateProjectile;
        }

        /// <summary>
        ///     Updates the character logic and animation state every frame.
        /// </summary>
        private void Update()
        {
            // If the character is dead, do not process further updates.
            if (isDead)
            {
                return;
            }

            // Update character logic (e.g., energy recharge).
            logic.Update(Time.deltaTime);
            // Check if the character is on the ground and handle landing.
            CheckGround();
            // Update the sprite's facing direction.
            UpdateSpriteDirection();
            // Update animator states for idle and movement.
            UpdateAnimationState();
        }

        /// <summary>
        ///     Applies horizontal movement and gravity adjustments in FixedUpdate.
        /// </summary>
        private void FixedUpdate()
        {
            if (isDead)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            // Se não estiver sob efeito de força externa, processa o input do jogador
            if (!isExternallyAffected)
            {
                // Apply horizontal movement if there is significant input.
                if (Mathf.Abs(horizontalInput) > 0.1f)
                {
                    float speed = isRunning ? characterData.runSpeed : characterData.walkSpeed;
                    rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);
                }
                else
                {
                    // Stop horizontal movement when no input is present.
                    rb.velocity = new Vector2(0, rb.velocity.y);
                }
            }

            // Increase falling speed using a gravity multiplier if the character is falling.
            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (characterData.fallMultiplier - 1) * Time.deltaTime;
            }
        }

        /// <summary>
        /// Unsubscribes from all events when this object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            StopAllCoroutines();
            if (logic != null)
            {
                logic.OnJump -= OnJumpHandler;
                logic.OnDoubleJump -= OnDoubleJumpHandler;
                logic.OnShoot -= OnShootHandler;
                logic.OnJumpShoot -= OnJumpShootHandler;
                logic.OnRunShoot -= OnRunShootHandler;
                logic.OnPunch -= OnPunchHandler;
                logic.OnJumpPunch -= OnJumpPunchHandler;
                logic.OnDeath -= OnDeathHandler;
                logic.OnProjectileCreated -= CreateProjectile;
                logic.OnHealthChanged -= HandleHealthChanged;
                logic.OnEnergyChanged -= HandleEnergyChanged;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies the collectible item's effects to the character.
        /// </summary>
        /// <param name="itemData">The collectible item's configuration data.</param>
        public void CollectItem(CollectibleItemData itemData)
        {
            if (itemData == null)
                return;

            // Restore energy if applicable.
            if (itemData.energyRestore > 0)
            {
                logic.RestoreEnergy(itemData.energyRestore);
            }

            // Restore health if applicable.
            if (itemData.healthRestore > 0)
            {
                logic.RestoreHealth(itemData.healthRestore);
            }

            OnItemCollected?.Invoke(itemData);
        }
        
        public void SetExternalForceActive(bool active)
        {
            isExternallyAffected = active;
        }
        /// <summary>
        ///     Updates the movement input.
        ///     Only processes new input if it has changed, then calls the CharacterLogic Move method
        ///     and updates Animator parameters.
        /// </summary>
        /// <param name="direction">Horizontal direction (-1 for left, 1 for right).</param>
        /// <param name="run">True if the character is running; otherwise, walking.</param>
        public void Move(float direction, bool run)
        {
            if (isDead)
            {
                return;
            }

            // Process input only if it has changed.
            if (Mathf.Approximately(direction, lastHorizontalInput) && run == lastIsRunning)
            {
                return;
            }

            lastHorizontalInput = direction;
            lastIsRunning = run;

            horizontalInput = direction;
            isRunning = run;
            // Pass input to the character logic.
            logic.Move(direction, run);
            // Update Animator boolean parameters for movement.
            if (Mathf.Abs(direction) > 0.1f)
            {
                if (run)
                {
                    animator.SetBool(characterData.isRunningParamHash, true);
                    animator.SetBool(characterData.isWalkingParamHash, false);
                }
                else
                {
                    animator.SetBool(characterData.isRunningParamHash, false);
                    animator.SetBool(characterData.isWalkingParamHash, true);
                }
            }
            else
            {
                animator.SetBool(characterData.isRunningParamHash, false);
                animator.SetBool(characterData.isWalkingParamHash, false);
                animator.SetBool(characterData.isIdleParamHash, true);
            }
        }

        /// <summary>
        ///     Makes the character jump if on the ground or if double jump is allowed.
        /// </summary>
        public void Jump()
        {
            if (isDead)
            {
                return;
            }

            if (IsGrounded() || logic.CanDoubleJump)
            {
                logic.Jump();
                rb.velocity = new Vector2(rb.velocity.x, characterData.jumpForce);
            }
        }

        /// <summary>
        ///     Resets the jump state when the character lands.
        /// </summary>
        public void Land()
        {
            if (isDead)
            {
                return;
            }

            logic.Land();
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            isExternallyAffected = false;
        }

        /// <summary>
        ///     Initiates a shooting action if the cooldown has elapsed.
        ///     Passes the current position as the attacker's position.
        /// </summary>
        public void Shoot()
        {
            if (isDead)
            {
                return;
            }

            if (Time.time - lastShootTime < actionCooldown)
            {
                return;
            }

            lastShootTime = Time.time;
            // Pass the character's current position as the attacker's position.
            logic.Shoot(transform.position);
        }

        /// <summary>
        ///     Initiates a punch action if the cooldown has elapsed.
        ///     Performs an overlap circle check to detect enemy collisions and applies damage.
        /// </summary>
        public void Punch()
        {
            if (isDead)
            {
                return;
            }

            // Verificação de cooldown:
            if (Time.time - lastPunchTime < characterData.actionCooldown)
            {
                return;
            }

            lastPunchTime = Time.time;
            logic.Punch();

            // Determine the origin of the punch based on character position and facing direction.
            Vector2 punchOrigin = (Vector2)transform.position +
                                  (logic.FacingRight ? Vector2.right : Vector2.left) * (punchRange * characterData.punchOffsetMultiplier);

            // Check for colliders within the punch range (without filtering by layer).
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(punchOrigin, punchRange);
            foreach (Collider2D collider in hitColliders)
            {
                // First, check if we hit a Barrel.
                var barrel = collider.GetComponent<Barrel>();
                if (barrel != null)
                {
                    barrel.ReceiveDamage();
                    continue; // Skip further processing for this collider.
                }

                // Next, check if we hit an enemy CharacterController.
                CharacterController target = collider.GetComponent<CharacterController>();
                if (target != null && target != this)
                {
                    target.TakeDamage(DamageType.Physical, characterData.punchDamage, transform.position);
                }
            }
        }
        
        /// <summary>
        /// Resets the character's health and energy to maximum.
        /// </summary>
        public void ResetStats()
        {
            logic.ResetStats();
        }

        /// <summary>
        ///     Instantiates a projectile based on the provided ProjectileInfo.
        /// </summary>
        /// <param name="projInfo">Information needed to create and initialize the projectile.</param>
        private void CreateProjectile(ProjectileInfo projInfo)
        {
            if (characterData.projectilePrefab && projectileSpawnPoint)
            {
                GameObject projInstance = Instantiate(characterData.projectilePrefab, projectileSpawnPoint.position,
                    Quaternion.identity);

                ProjectileController controller = projInstance.GetComponent<ProjectileController>();
                if (controller != null)
                {
                    controller.Initialize(projInfo);
                }
            }
        }

        /// <summary>
        ///     Checks if the character is grounded using a circular overlap.
        ///     If grounded and the character is in a jump state, triggers landing.
        /// </summary>
        private void CheckGround()
        {
            if (rb.velocity.y <= 0)
            {
                bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
                if (isGrounded && (logic.IsJumping || isExternallyAffected))
                {
                    Land();
                    animator.SetTrigger(characterData.landTriggerHash);
                }
            }
        }

        /// <summary>
        ///     Updates the sprite's facing direction based on the character's movement.
        /// </summary>
        private void UpdateSpriteDirection()
        {
            Vector3 scale = transform.localScale;
            scale.x = logic.FacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        /// <summary>
        ///     Updates the animator state for idle versus movement.
        ///     Sets the "isIdle" boolean based on whether there is horizontal input.
        /// </summary>
        private void UpdateAnimationState()
        {
            if (isDead)
            {
                return;
            }

            animator.SetBool(characterData.isIdleParamHash, !(Mathf.Abs(horizontalInput) > characterData.horizontalInputThreshold));
        }

        /// <summary>
        ///     Determines if the character is grounded by performing a circular overlap check.
        /// </summary>
        /// <returns>True if the character is grounded; otherwise, false.</returns>
        private bool IsGrounded()
        {
            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        /// <summary>
        ///     Receives damage with the specified damage type, amount, and the attacker's position.
        ///     If the character is not dead, it forwards damage processing to the logic and raises the OnDamageReceived event.
        /// </summary>
        /// <param name="damageType">Type of damage.</param>
        /// <param name="damage">Amount of damage.</param>
        /// <param name="attackerPosition">Position of the attacker.</param>
        public void TakeDamage(DamageType damageType, int damage, Vector3 attackerPosition)
        {
            if (isDead)
            {
                return;
            }

            logic.ReceiveDamage(damageType, damage);

            // Raise the OnDamageReceived event to notify listeners of the damage and attacker position.
            OnDamageReceived?.Invoke(attackerPosition);

            // Flash red if the damage is not poison.
            if (damageType != DamageType.Poison)
            {
                if (flashRedCoroutine != null)
                {
                    StopCoroutine(flashRedCoroutine);
                    flashRedCoroutine = null;
                }
                flashRedCoroutine = StartCoroutine(FlashRed());
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the character is alive.
        /// </summary>
        public bool IsAlive => !isDead;

        /// <summary>
        ///     Handles character death by disabling physics simulation and colliders.
        ///     Prevents further actions after death.
        /// </summary>
        public void Die()
        {
            StopAllCoroutines();
            isDead = true;
            if (rb != null)
            {
                rb.simulated = false;
            }

            // Disable all Collider2D components on this GameObject.
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }
            
            // Fire the player death event.
            OnPlayerDeath?.Invoke();
            
            // If this is an enemy (i.e. not tagged "Player"), raise the enemy death event.
            if (!gameObject.CompareTag("Player"))
            {
                OnEnemyDeath?.Invoke();
            }
        }

        /// <summary>
        ///     Sets the poison state on the character.
        ///     When poisoned, the sprite remains green.
        /// </summary>
        /// <param name="state">True to set poisoned, false to remove poison effect.</param>
        public void SetPoisoned(bool state)
        {
            isPoisoned = state;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isPoisoned ? Color.green : originalColor;
            }
        }

        /// <summary>
        ///     Coroutine to flash the sprite red when taking damage.
        /// </summary>
        private IEnumerator FlashRed()
        {
            if (spriteRenderer != null)
            {
                Color currentColor = spriteRenderer.color;
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                spriteRenderer.color = isPoisoned ? Color.green : originalColor;
            }
            flashRedCoroutine = null;
        }

        #endregion
    }
}