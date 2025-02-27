using System;
using UnityEngine;

namespace HermitCrab.Character
{
    /// <summary>
    /// Contains the core logic for character actions, independent of UnityEngine.
    /// Handles movement, jumping, attacks, and damage processing.
    /// </summary>
    public class CharacterLogic
    {
        private readonly CharacterData data;
        public int Health { get; private set; }
        public int Energy { get; private set; }
        private float energyAccumulator;

        public bool IsJumping { get; private set; }
        public bool CanDoubleJump { get; private set; }
        public bool IsRunning { get; private set; }
        public bool FacingRight { get; private set; } = true;

        // Events for notifying state changes and actions.
        public event Action OnDeath;
        public event Action<int> OnHealthChanged;
        public event Action<int> OnEnergyChanged;
        public event Action OnWalk;
        public event Action OnRun;
        public event Action OnJump;
        public event Action OnDoubleJump;
        public event Action OnShoot;
        public event Action OnPunch;
        public event Action OnJumpShoot;
        public event Action OnRunShoot;
        public event Action OnJumpPunch;
        public event Action<ProjectileInfo> OnProjectileCreated;

        /// <summary>
        /// Initializes a new instance of CharacterLogic with the specified data.
        /// </summary>
        /// <param name="data">The character configuration data.</param>
        public CharacterLogic(CharacterData data)
        {
            this.data = data;
            Health = data.maxHealth;
            Energy = data.maxEnergy;
        }

        /// <summary>
        /// Updates the logic, such as recharging energy over time.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last update.</param>
        public void Update(float deltaTime)
        {
            energyAccumulator += data.energyRechargeRate * deltaTime;
            while (energyAccumulator >= 1f)
            {
                if (Energy < data.maxEnergy)
                {
                    Energy++;
                    OnEnergyChanged?.Invoke(Energy);
                }
                energyAccumulator -= 1f;
            }
        }

        /// <summary>
        /// Processes damage reception and triggers death if health falls to zero or below.
        /// </summary>
        /// <param name="type">The type of damage.</param>
        /// <param name="damage">The amount of damage.</param>
        public void ReceiveDamage(DamageType type, int damage)
        {
            Health -= damage;
            OnHealthChanged?.Invoke(Health);
            if (Health <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Invokes the death event.
        /// </summary>
        private void Die()
        {
            OnDeath?.Invoke();
        }

        /// <summary>
        /// Processes movement input, updates the facing direction, and triggers walk/run events.
        /// </summary>
        /// <param name="direction">The horizontal input direction (-1 for left, 1 for right).</param>
        /// <param name="run">True if running; false for walking.</param>
        public void Move(float direction, bool run)
        {
            if (direction != 0)
            {
                FacingRight = direction > 0;
                if (run)
                {
                    IsRunning = true;
                    OnRun?.Invoke();
                }
                else
                {
                    IsRunning = false;
                    OnWalk?.Invoke();
                }
            }
        }

        /// <summary>
        /// Initiates a jump or a double jump.
        /// </summary>
        public void Jump()
        {
            if (!IsJumping)
            {
                IsJumping = true;
                CanDoubleJump = true;
                OnJump?.Invoke();
            }
            else if (CanDoubleJump)
            {
                CanDoubleJump = false;
                OnDoubleJump?.Invoke();
            }
        }

        /// <summary>
        /// Resets the jump state when landing.
        /// </summary>
        public void Land()
        {
            IsJumping = false;
            CanDoubleJump = false;
        }

        /// <summary>
        /// Initiates a shooting action.
        /// Deducts energy and creates a projectile with the attacker's position.
        /// </summary>
        /// <param name="attackerPosition">The position of the attacker.</param>
        public void Shoot(Vector3 attackerPosition)
        {
            if (Energy >= data.shootEnergyCost)
            {
                Energy -= data.shootEnergyCost;
                OnEnergyChanged?.Invoke(Energy);

                if (IsJumping)
                {
                    OnJumpShoot?.Invoke();
                }
                else if (IsRunning)
                {
                    OnRunShoot?.Invoke();
                }
                else
                {
                    OnShoot?.Invoke();
                }

                // Create a new projectile info with all necessary data.
                ProjectileInfo projectile = new ProjectileInfo
                {
                    Speed = data.projectileSpeed,
                    MaxDistance = data.projectileMaxDistance,
                    Damage = data.projectileDamage,
                    Direction = FacingRight ? new Vector2(1, 0) : new Vector2(-1, 0),
                    AttackerPosition = attackerPosition
                };

                OnProjectileCreated?.Invoke(projectile);
            }
        }

        /// <summary>
        /// Initiates a punch action.
        /// </summary>
        public void Punch()
        {
            if (IsJumping)
            {
                OnJumpPunch?.Invoke();
            }
            else
            {
                OnPunch?.Invoke();
            }
        }
    }
}
