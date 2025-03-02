using UnityEngine;
using UnityEngine.Serialization;

namespace HermitCrab.Character
{
    /// <summary>
    /// Handles input for controlling the character.
    /// Maps keyboard input to character actions such as move, jump, shoot, and punch.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        [FormerlySerializedAs("characterController")] public CharacterBehaviour characterBehaviour;

        private void Update()
        {
            if (characterBehaviour == null) return;
            
            // Use GetAxisRaw to capture unsmoothed horizontal input.
            float horizontal = Input.GetAxisRaw("Horizontal");
            bool run = Input.GetKey(KeyCode.LeftShift);
            characterBehaviour.Move(horizontal, run);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                characterBehaviour.Jump();
            }

            if (Input.GetMouseButtonUp(0))
            {
                characterBehaviour.Shoot();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                characterBehaviour.Punch();
            }
        }

        /// <summary>
        /// Called by UI to move the character left.
        /// </summary>
        public void OnMoveLeft()
        {
            characterBehaviour.Move(-1f, false);
        }

        /// <summary>
        /// Called by UI to move the character right.
        /// </summary>
        public void OnMoveRight()
        {
            characterBehaviour.Move(1f, false);
        }

        /// <summary>
        /// Called by UI to run left.
        /// </summary>
        public void OnRunLeft()
        {
            characterBehaviour.Move(-1f, true);
        }

        /// <summary>
        /// Called by UI to run right.
        /// </summary>
        public void OnRunRight()
        {
            characterBehaviour.Move(1f, true);
        }

        /// <summary>
        /// Called by UI to make the character jump.
        /// </summary>
        public void OnJump()
        {
            characterBehaviour.Jump();
        }

        /// <summary>
        /// Called by UI to make the character shoot.
        /// </summary>
        public void OnShoot()
        {
            characterBehaviour.Shoot();
        }

        /// <summary>
        /// Called by UI to make the character punch.
        /// </summary>
        public void OnPunch()
        {
            characterBehaviour.Punch();
        }
    }
}