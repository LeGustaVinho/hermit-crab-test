using HermitCrab.Level;
using UnityEngine;

namespace HermitCrab.Character
{
    /// <summary>
    /// Controls the projectile's movement and collision behavior in 2D.
    /// The projectile moves at a constant speed and is destroyed after exceeding its maximum distance.
    /// On collision with a CharacterController, it inflicts damage using the attacker's position.
    /// </summary>
    public class ProjectileController : MonoBehaviour
    {
        private float speed;
        private float maxDistance;
        private int damage;
        private Vector2 direction;
        private Vector3 startPosition;
        private Vector3 attackerPosition; // Stores the position of the attacker
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Initializes the projectile with the provided information.
        /// Sets speed, damage, direction, maximum distance, and attacker position.
        /// </summary>
        /// <param name="info">Projectile information including attacker position.</param>
        public void Initialize(ProjectileInfo info)
        {
            speed = info.Speed;
            maxDistance = info.MaxDistance;
            damage = info.Damage;
            direction = info.Direction;
            attackerPosition = info.AttackerPosition; // Set the attacker's position
            startPosition = transform.position;

            // Set the projectile's velocity based on direction and speed.
            if (rb != null)
            {
                rb.velocity = direction * speed;
            }

            // Adjust the sprite's orientation; the artwork is naturally facing right.
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }

        private void Update()
        {
            // Destroy the projectile if it has traveled beyond the maximum distance.
            if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Destroy(gameObject);
            
            // First, check if the collided object is a Barrel.
            Barrel barrel = collision.gameObject.GetComponent<Barrel>();
            if (barrel != null)
            {
                barrel.ReceiveDamage();
                return;
            }

            // Otherwise, check if it's a CharacterController.
            CharacterController target = collision.gameObject.GetComponent<CharacterController>();
            if (target != null)
            {
                target.TakeDamage(DamageType.Projectile, damage, attackerPosition);
            }
        }
    }
}