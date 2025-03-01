using System;
using System.Collections;
using HermitCrab.Level;
using HermitCrab.Level.HermitCrab.Items;
using UnityEngine;

namespace HermitCrab.Character
{
    /// <summary>
    /// Facade MonoBehaviour for the character.
    /// Delegates responsibilities to dedicated services.
    /// </summary>
    public class CharacterBehaviour : MonoBehaviour
    {
        #region Public Fields

        [Header("Character Settings")]
        public CharacterData characterData;

        [Header("Spawn and Ground Check")]
        public Transform projectileSpawnPoint;
        public Transform groundCheck;
        public float groundCheckRadius = 0.1f;
        public LayerMask groundLayer;

        [Header("Punch Settings")]
        public float punchRange = 1f;
        public LayerMask enemyLayer;

        #endregion

        #region Public Events

        public event Action<Vector3> OnDamageReceived;
        public event Action OnPlayerDeath;
        public event Action OnEnemyDeath;
        public event Action<CollectibleItemData> OnItemCollected;
        public event Action<int> OnHealthChanged;
        public event Action<int> OnEnergyChanged;

        #endregion

        #region Private Fields

        private CharacterLogic _logic;
        private Animator _animator;
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;

        private MovimentCharComponent movimentCharComponent;
        private AttackCharComponent attackCharComponent;
        private AnimationCharComponent animationCharComponent;
        private DamageFxCharComponent damageFxCharComponent;

        private float _horizontalInput;
        private bool _isRunning;
        private bool _isDead;
        private bool _isExternallyAffected = false;
        private Coroutine _flashRedCoroutine;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }

            _logic = new CharacterLogic(characterData);
            SubscribeToLogicEvents();

            movimentCharComponent = new MovimentCharComponent(characterData, _rigidbody, groundCheck, groundCheckRadius, groundLayer);
            attackCharComponent = new AttackCharComponent(characterData, projectileSpawnPoint, punchRange);
            animationCharComponent = new AnimationCharComponent(_animator, characterData);
            damageFxCharComponent = new DamageFxCharComponent(_spriteRenderer);

            // Subscribe to health and energy events from logic.
            _logic.OnHealthChanged += HandleHealthChanged;
            _logic.OnEnergyChanged += HandleEnergyChanged;
        }

        private void Update()
        {
            if (_isDead)
            {
                return;
            }

            _logic.Update(Time.deltaTime);
            CheckGround();
            movimentCharComponent.UpdateSpriteDirection(transform, _logic.FacingRight);
            animationCharComponent.UpdateMovementAnimation(_horizontalInput);
        }

        private void FixedUpdate()
        {
            if (_isDead)
            {
                _rigidbody.velocity = Vector2.zero;
                return;
            }

            if (!_isExternallyAffected)
            {
                movimentCharComponent.FixedUpdateMovement(_horizontalInput, _isRunning, _isExternallyAffected);
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            if (_logic != null)
            {
                UnsubscribeFromLogicEvents();
                _logic.OnHealthChanged -= HandleHealthChanged;
                _logic.OnEnergyChanged -= HandleEnergyChanged;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies a collectible item’s effects.
        /// </summary>
        /// <param name="itemData">Collectible item data.</param>
        public void CollectItem(CollectibleItemData itemData)
        {
            if (itemData == null)
                return;

            if (itemData.energyRestore > 0)
            {
                _logic.RestoreEnergy(itemData.energyRestore);
            }

            if (itemData.healthRestore > 0)
            {
                _logic.RestoreHealth(itemData.healthRestore);
            }

            OnItemCollected?.Invoke(itemData);
        }

        /// <summary>
        /// Enables or disables external forces affecting the character.
        /// </summary>
        public void SetExternalForceActive(bool active)
        {
            _isExternallyAffected = active;
        }

        /// <summary>
        /// Processes movement input.
        /// </summary>
        /// <param name="direction">Horizontal direction input.</param>
        /// <param name="run">True if running; otherwise, walking.</param>
        public void Move(float direction, bool run)
        {
            if (_isDead)
            {
                return;
            }

            _horizontalInput = direction;
            _isRunning = run;
            movimentCharComponent.Move(direction, run, _logic);
            animationCharComponent.SetMovementState(direction, run);
        }

        /// <summary>
        /// Initiates a jump action.
        /// </summary>
        public void Jump()
        {
            if (_isDead)
            {
                return;
            }

            if (IsGrounded() || _logic.CanDoubleJump)
            {
                movimentCharComponent.Jump(_logic);
            }
        }

        /// <summary>
        /// Processes landing action.
        /// </summary>
        public void Land()
        {
            if (_isDead)
            {
                return;
            }

            movimentCharComponent.Land(_logic);
            _isExternallyAffected = false;
        }

        /// <summary>
        /// Initiates a shooting action.
        /// </summary>
        public void Shoot()
        {
            if (_isDead)
            {
                return;
            }

            attackCharComponent.Shoot(_logic, transform.position);
        }

        /// <summary>
        /// Initiates a punch action.
        /// </summary>
        public void Punch()
        {
            if (_isDead)
            {
                return;
            }

            attackCharComponent.Punch(_logic, transform);
        }

        /// <summary>
        /// Resets health and energy to their maximum values.
        /// </summary>
        public void ResetStats()
        {
            _logic.ResetStats();
        }

        /// <summary>
        /// Processes damage reception and applies visual feedback.
        /// </summary>
        /// <param name="damageType">Type of damage.</param>
        /// <param name="damage">Damage amount.</param>
        /// <param name="attackerPosition">Attacker’s position.</param>
        public void TakeDamage(DamageType damageType, int damage, Vector3 attackerPosition)
        {
            if (_isDead)
            {
                return;
            }

            _logic.ReceiveDamage(damageType, damage);
            OnDamageReceived?.Invoke(attackerPosition);

            if (damageType != DamageType.Poison)
            {
                if (_flashRedCoroutine != null)
                {
                    StopCoroutine(_flashRedCoroutine);
                    _flashRedCoroutine = null;
                }
                _flashRedCoroutine = StartCoroutine(damageFxCharComponent.FlashRed());
            }
        }

        /// <summary>
        /// Gets a value indicating whether the character is alive.
        /// </summary>
        public bool IsAlive => !_isDead;

        /// <summary>
        /// Handles the death process of the character.
        /// </summary>
        public void Die()
        {
            StopAllCoroutines();
            _isDead = true;
            if (_rigidbody != null)
            {
                _rigidbody.simulated = false;
            }

            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }

            OnPlayerDeath?.Invoke();

            if (!gameObject.CompareTag("Player"))
            {
                OnEnemyDeath?.Invoke();
            }
        }

        /// <summary>
        /// Sets the poisoned state.
        /// </summary>
        /// <param name="state">True to set as poisoned; false otherwise.</param>
        public void SetPoisoned(bool state)
        {
            damageFxCharComponent.SetPoisoned(state);
        }

        #endregion

        #region Private Methods

        private void CheckGround()
        {
            if (_rigidbody.velocity.y <= 0)
            {
                bool grounded = IsGrounded();
                if (grounded && (_logic.IsJumping || _isExternallyAffected))
                {
                    Land();
                    animationCharComponent.TriggerLand();
                }
            }
        }

        private bool IsGrounded()
        {
            return movimentCharComponent.IsGrounded();
        }

        private void HandleHealthChanged(int health)
        {
            OnHealthChanged?.Invoke(health);
        }

        private void HandleEnergyChanged(int energy)
        {
            OnEnergyChanged?.Invoke(energy);
        }

        private void SubscribeToLogicEvents()
        {
            _logic.OnJump += OnJumpHandler;
            _logic.OnDoubleJump += OnDoubleJumpHandler;
            _logic.OnShoot += OnShootHandler;
            _logic.OnJumpShoot += OnJumpShootHandler;
            _logic.OnRunShoot += OnRunShootHandler;
            _logic.OnPunch += OnPunchHandler;
            _logic.OnJumpPunch += OnJumpPunchHandler;
            _logic.OnDeath += OnDeathHandler;
            _logic.OnProjectileCreated += CreateProjectile;
        }

        private void UnsubscribeFromLogicEvents()
        {
            _logic.OnJump -= OnJumpHandler;
            _logic.OnDoubleJump -= OnDoubleJumpHandler;
            _logic.OnShoot -= OnShootHandler;
            _logic.OnJumpShoot -= OnJumpShootHandler;
            _logic.OnRunShoot -= OnRunShootHandler;
            _logic.OnPunch -= OnPunchHandler;
            _logic.OnJumpPunch -= OnJumpPunchHandler;
            _logic.OnDeath -= OnDeathHandler;
            _logic.OnProjectileCreated -= CreateProjectile;
        }

        private void OnJumpHandler()
        {
            if (!_isDead)
            {
                animationCharComponent.TriggerJump();
            }
        }

        private void OnDoubleJumpHandler()
        {
            if (!_isDead)
            {
                animationCharComponent.TriggerDoubleJump();
            }
        }

        private void OnShootHandler()
        {
            if (!_isDead)
            {
                animationCharComponent.TriggerShoot();
            }
        }

        private void OnJumpShootHandler()
        {
            if (!_isDead)
            {
                animationCharComponent.TriggerJumpShoot();
            }
        }

        private void OnRunShootHandler()
        {
            if (!_isDead)
            {
                animationCharComponent.TriggerRunShoot();
            }
        }

        private void OnPunchHandler()
        {
            if (!_isDead)
            {
                animationCharComponent.TriggerPunch();
            }
        }

        private void OnJumpPunchHandler()
        {
            if (!_isDead)
            {
                animationCharComponent.TriggerJumpPunch();
            }
        }

        private void OnDeathHandler()
        {
            if (!_isDead)
            {
                animationCharComponent.TriggerDeath();
                Die();
            }
        }

        private void CreateProjectile(ProjectileInfo projInfo)
        {
            if (characterData.projectilePrefab && projectileSpawnPoint)
            {
                HermitCrab.Factory.GameObjectFactory.CreateProjectile(characterData.projectilePrefab, projectileSpawnPoint.position, projInfo);
            }
        }

        #endregion
    }
}