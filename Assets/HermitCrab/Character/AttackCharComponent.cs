using HermitCrab.Level;
using UnityEngine;

namespace HermitCrab.Character
{
    /// <summary>
    /// Provides attack-related functionalities such as shooting and punching.
    /// </summary>
    public class AttackCharComponent
    {
        private readonly CharacterData _characterData;
        private readonly Transform _projectileSpawnPoint;
        private readonly float _punchRange;
        private float _lastPunchTime = -Mathf.Infinity;
        private float _lastShootTime = -Mathf.Infinity;

        public AttackCharComponent(CharacterData characterData, Transform projectileSpawnPoint, float punchRange)
        {
            _characterData = characterData;
            _projectileSpawnPoint = projectileSpawnPoint;
            _punchRange = punchRange;
        }

        /// <summary>
        /// Initiates a shooting action if the cooldown has elapsed.
        /// </summary>
        /// <param name="logic">Character logic instance to process shooting.</param>
        /// <param name="position">The character's current position.</param>
        public void Shoot(CharacterLogic logic, Vector3 position)
        {
            if (Time.time - _lastShootTime < _characterData.actionCooldown)
            {
                return;
            }

            _lastShootTime = Time.time;
            logic.Shoot(position);
        }

        /// <summary>
        /// Initiates a punch action if the cooldown has elapsed.
        /// Checks for colliders within range and applies damage.
        /// </summary>
        /// <param name="logic">Character logic instance to process punching.</param>
        /// <param name="ownerTransform">The transform of the character performing the punch.</param>
        public void Punch(CharacterLogic logic, Transform ownerTransform)
        {
            if (Time.time - _lastPunchTime < _characterData.actionCooldown)
            {
                return;
            }

            _lastPunchTime = Time.time;
            logic.Punch();

            // Calculate the punch origin based on the character's facing direction.
            Vector2 punchOrigin = (Vector2)ownerTransform.position +
                                  (logic.FacingRight ? Vector2.right : Vector2.left) *
                                  (_punchRange * _characterData.punchOffsetMultiplier);

            // Check for colliders within the punch range.
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(punchOrigin, _punchRange);
            foreach (Collider2D collider in hitColliders)
            {
                // Check if a Barrel was hit.
                var barrel = collider.GetComponent<Barrel>();
                if (barrel != null)
                {
                    barrel.ReceiveDamage();
                    continue;
                }

                // Check if an enemy CharacterController was hit.
                CharacterBehaviour target = collider.GetComponent<CharacterBehaviour>();
                if (target != null && target != ownerTransform.GetComponent<CharacterBehaviour>())
                {
                    target.TakeDamage(DamageType.Physical, _characterData.punchDamage, ownerTransform.position);
                }
            }
        }
    }
}
