using System.Collections;
using HermitCrab.Character;
using UnityEngine;

namespace HermitCrab.Level
{
    /// <summary>
    ///     Implements the Barrel hazard behavior.
    ///     When the barrel receives damage it activates. Depending on its type,
    ///     it either explodes (dealing area damage and applying knockback) or applies a poison effect.
    /// </summary>
    public class Barrel : MonoBehaviour
    {
        [Tooltip("Configuration for the barrel hazard.")]
        public BarrelData barrelData;

        private BarrelLogic _logic;

        private void Awake()
        {
            if (barrelData == null)
            {
                Debug.LogError("BarrelData is not assigned.");
                enabled = false;
                return;
            }

            _logic = new BarrelLogic(barrelData);
            _logic.OnActivated += HandleActivation;
        }

        private void OnDrawGizmosSelected()
        {
            if (barrelData != null && barrelData.Type == BarrelType.Explosive)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, barrelData.ExplosionRadius);
            }
        }

        /// <summary>
        ///     This method can be called externally when the barrel is hit.
        /// </summary>
        public void ReceiveDamage()
        {
            _logic.ReceiveDamage();
        }

        /// <summary>
        ///     Applies a poison effect to any CharacterController overlapping a small radius around the barrel.
        ///     The effect applies damage per second for the configured duration.
        /// </summary>
        private void ApplyPoison()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, barrelData.PoisonRadius);
            foreach (Collider2D hit in hits)
            {
                CharacterBehaviour character = hit.GetComponent<CharacterBehaviour>();
                if (character != null)
                {
                    character.StartCoroutine(ApplyPoisonDamage(character, barrelData.PoisonDuration, 
                        barrelData.PoisonDamagePerSecond, transform.position));
                }
            }
        }

        private IEnumerator ApplyPoisonDamage(CharacterBehaviour character, float duration, int dps, Vector3 position)
        {
            // Set poison effect on the character.
            character.SetPoisoned(true);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                character.TakeDamage(DamageType.Poison, dps, position);
                yield return new WaitForSeconds(1f);
                elapsed += 1f;
            }
            // Remove poison effect.
            character.SetPoisoned(false);
        }

        /// <summary>
        ///     Explodes the barrel, applying damage and knockback to all CharacterControllers in range.
        /// </summary>
        private void Explode()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, barrelData.ExplosionRadius);
            if (barrelData.explosionVfx != null)
            {
                GameObject projInstance = Instantiate(barrelData.explosionVfx, transform.position, Quaternion.identity);
                Destroy(projInstance, barrelData.explosionVfxLifetime);
            }

            foreach (Collider2D hit in hits)
            {
                // Assumes CharacterController has a TakeDamage method as in your existing code.
                CharacterBehaviour character = hit.GetComponent<CharacterBehaviour>();
                if (character != null)
                {
                    character.TakeDamage(DamageType.Environmental, barrelData.ExplosiveDamage, transform.position);
                    // Compute knockback direction: from explosion center to character.
                    Vector3 knockbackDirection = (character.transform.position - transform.position).normalized;
                    Debug.DrawRay(transform.position, knockbackDirection * barrelData.KnockbackForce, Color.cyan, 5);
                    Rigidbody2D rb = character.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.AddForce(knockbackDirection * barrelData.KnockbackForce, ForceMode2D.Impulse);
                        character.SetExternalForceActive(true);
                    }
                }
            }
        }

        private void HandleActivation()
        {
            // Execute effect based on barrel type.
            if (barrelData.Type == BarrelType.Explosive)
            {
                Explode();
            }
            else if (barrelData.Type == BarrelType.Poison)
            {
                ApplyPoison();
            }

            // Optionally, destroy the barrel after activation.
            Destroy(gameObject);
        }
    }
}