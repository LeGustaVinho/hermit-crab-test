using System;
using HermitCrab.Character;
using UnityEngine;
using CharacterController = HermitCrab.Character.CharacterController;

namespace HermitCrab.Level
{
    namespace HermitCrab.Items
    {
        /// <summary>
        /// Handles collectible item behavior.
        /// When a character collides with the item, it applies its restoration effects and is then destroyed.
        /// </summary>
        public class CollectibleItem : MonoBehaviour
        {
            [Header("Item Data")]
            [Tooltip("Reference to the item's configuration data.")]
            public CollectibleItemData itemData;

            /// <summary>
            /// Event raised when the item is collected.
            /// </summary>
            public event Action OnCollected;

            private void OnTriggerEnter2D(Collider2D collision)
            {
                CharacterController character = collision.GetComponent<CharacterController>();
                if (character != null && itemData != null)
                {
                    // Apply item effects to the character.
                    character.CollectItem(itemData);
                    OnCollected?.Invoke();
                    // Destroy the item after collection.
                    Destroy(gameObject);
                }
            }
        }
    }
}