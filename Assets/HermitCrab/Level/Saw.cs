﻿using System.Collections;
using System.Collections.Generic;
using HermitCrab.Character;
using UnityEngine;
using CharacterController = HermitCrab.Character.CharacterController;

namespace HermitCrab.Level
{
    /// <summary>
    ///     Implements the Saw hazard behavior.
    ///     While a CharacterController remains in contact, it receives continuous damage and knockback.
    /// </summary>
    public class Saw : MonoBehaviour
    {
        [Tooltip("Configuration for the saw hazard.")]
        public SawData sawData;

        private readonly Dictionary<CharacterController, Coroutine> _activeCoroutines =
            new Dictionary<CharacterController, Coroutine>();

        // Fields for saw movement
        private Vector3 startPosition;
        private Vector3 movementDirection;
        private float halfDistance;

        private void Start()
        {
            startPosition = transform.position;
            halfDistance = sawData.movementDistance * 0.5f;
            // Set movement direction based on the configured type
            movementDirection = sawData.movementType == SawData.MovementType.Horizontal ? Vector3.right : Vector3.up;
        }

        private void Update()
        {
            // Calculate offset using PingPong to oscillate between -halfDistance and +halfDistance
            float offset = Mathf.PingPong(Time.time * sawData.movementSpeed, sawData.movementDistance) - halfDistance;
            transform.position = startPosition + movementDirection * offset;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            CharacterController character = other.GetComponent<CharacterController>();
            if (character != null && _activeCoroutines.ContainsKey(character))
            {
                StopCoroutine(_activeCoroutines[character]);
                _activeCoroutines.Remove(character);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            CharacterController character = other.GetComponent<CharacterController>();
            if (character != null && !_activeCoroutines.ContainsKey(character))
            {
                Coroutine routine = StartCoroutine(ApplySawDamage(character));
                _activeCoroutines.Add(character, routine);
            }
        }

        private IEnumerator ApplySawDamage(CharacterController character)
        {
            while (character != null && IsCharacterWithinBounds(character))
            {
                character.TakeDamage(DamageType.Environmental, sawData.DamagePerSecond, transform.position);

                // Apply knockback from the saw's center.
                Rigidbody2D rb = character.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector3 knockbackDirection = (character.transform.position - transform.position).normalized;
                    rb.AddForce(knockbackDirection * sawData.KnockbackForce, ForceMode2D.Impulse);
                }

                yield return new WaitForSeconds(1f);
            }

            _activeCoroutines.Remove(character);
        }

        private bool IsCharacterWithinBounds(CharacterController character)
        {
            Collider2D characterCollider = character.GetComponent<Collider2D>();
            Collider2D sawCollider = GetComponent<Collider2D>();
            return characterCollider != null && sawCollider != null &&
                   characterCollider.bounds.Intersects(sawCollider.bounds);
        }
    }
}