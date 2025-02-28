using UnityEngine;

namespace HermitCrab.Character
{
    /// <summary>
    /// Handles input for controlling the character.
    /// Maps keyboard input to character actions such as move, jump, shoot, and punch.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        public CharacterController characterController;

        private void Update()
        {
            if (characterController == null) return;
            
            // Use GetAxisRaw to capture unsmoothed horizontal input.
            float horizontal = Input.GetAxisRaw("Horizontal");
            bool run = Input.GetKey(KeyCode.LeftShift);
            characterController.Move(horizontal, run);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                characterController.Jump();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                characterController.Shoot();
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                characterController.Punch();
            }
        }

        /// <summary>
        /// Called by UI to move the character left.
        /// </summary>
        public void OnMoveLeft()
        {
            characterController.Move(-1f, false);
        }

        /// <summary>
        /// Called by UI to move the character right.
        /// </summary>
        public void OnMoveRight()
        {
            characterController.Move(1f, false);
        }

        /// <summary>
        /// Called by UI to run left.
        /// </summary>
        public void OnRunLeft()
        {
            characterController.Move(-1f, true);
        }

        /// <summary>
        /// Called by UI to run right.
        /// </summary>
        public void OnRunRight()
        {
            characterController.Move(1f, true);
        }

        /// <summary>
        /// Called by UI to make the character jump.
        /// </summary>
        public void OnJump()
        {
            characterController.Jump();
        }

        /// <summary>
        /// Called by UI to make the character shoot.
        /// </summary>
        public void OnShoot()
        {
            characterController.Shoot();
        }

        /// <summary>
        /// Called by UI to make the character punch.
        /// </summary>
        public void OnPunch()
        {
            characterController.Punch();
        }
    }
}