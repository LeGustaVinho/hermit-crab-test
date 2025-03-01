using System.Collections;
using System.Collections.Generic;
using HermitCrab.Character;
using UnityEngine;

namespace HermitCrab.Level
{
    /// <summary>
    ///     Implements the Spike hazard behavior.
    ///     While a CharacterController remains in contact with the spike, it receives continuous damage and knockback.
    ///     Unlike the saw, the spike is immobile.
    /// </summary>
    public class Spike : MonoBehaviour
    {
        [Tooltip("Configuration for the spike hazard.")]
        public SpikeData spikeData;

        private readonly Dictionary<CharacterBehaviour, Coroutine> _activeCoroutines =
            new Dictionary<CharacterBehaviour, Coroutine>();

        private void OnTriggerExit2D(Collider2D other)
        {
            CharacterBehaviour character = other.GetComponent<CharacterBehaviour>();
            if (character != null && _activeCoroutines.ContainsKey(character))
            {
                if (_activeCoroutines[character] != null)
                {
                    StopCoroutine(_activeCoroutines[character]);
                    _activeCoroutines.Remove(character);
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            CharacterBehaviour character = other.GetComponent<CharacterBehaviour>();
            if (character != null && !_activeCoroutines.ContainsKey(character))
            {
                Coroutine routine = StartCoroutine(ApplySpikeDamage(character));
                _activeCoroutines.Add(character, routine);
            }
        }

        private IEnumerator ApplySpikeDamage(CharacterBehaviour character)
        {
            while (character != null && IsCharacterWithinBounds(character))
            {
                character.TakeDamage(DamageType.Environmental, spikeData.DamagePerSecond, transform.position);

                // Apply knockback from the spike's center.
                Rigidbody2D rb = character.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector3 knockbackDirection = (character.transform.position - transform.position).normalized;
                    rb.AddForce(knockbackDirection * spikeData.KnockbackForce, ForceMode2D.Impulse);
                }

                yield return new WaitForSeconds(1f);
            }

            _activeCoroutines.Remove(character);
        }

        private bool IsCharacterWithinBounds(CharacterBehaviour character)
        {
            Collider2D characterCollider = character.GetComponent<Collider2D>();
            Collider2D spikeCollider = GetComponent<Collider2D>();
            return characterCollider != null && spikeCollider != null &&
                   characterCollider.bounds.Intersects(spikeCollider.bounds);
        }

        private void OnDestroy()
        {
            foreach (var routine in _activeCoroutines.Values)
            {
                if (routine != null)
                {
                    StopCoroutine(routine);
                }
            }
            _activeCoroutines.Clear();
        }
    }
}