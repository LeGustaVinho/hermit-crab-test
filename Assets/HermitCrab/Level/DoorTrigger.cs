using System;
using UnityEngine;
using CharacterController = HermitCrab.Character.CharacterController;

namespace HermitCrab.Level
{
    public class DoorTrigger : MonoBehaviour
    {
        // Instance event for door triggering.
        public event Action OnDoorTriggered;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Check if the colliding object is the player by looking for a CharacterController component.
            if (collision.GetComponent<CharacterController>() != null)
            {
                OnDoorTriggered?.Invoke();
            }
        }
    }
}