using UnityEngine;

namespace HermitCrab.Character
{
    /// <summary>
    /// Provides movement-related functionalities for the character.
    /// </summary>
    public class MovimentCharComponent
    {
        private readonly CharacterData _characterData;
        private readonly Rigidbody2D _rigidbody;
        private readonly Transform _groundCheck;
        private readonly float _groundCheckRadius;
        private readonly LayerMask _groundLayer;

        // Tracks the previous input to avoid redundant processing.
        private float _lastHorizontalInput;
        private bool _lastIsRunning;

        public MovimentCharComponent(CharacterData characterData, Rigidbody2D rigidbody, Transform groundCheck, float groundCheckRadius, LayerMask groundLayer)
        {
            _characterData = characterData;
            _rigidbody = rigidbody;
            _groundCheck = groundCheck;
            _groundCheckRadius = groundCheckRadius;
            _groundLayer = groundLayer;
        }

        /// <summary>
        /// Processes horizontal movement input.
        /// </summary>
        /// <param name="direction">Horizontal direction (-1 for left, 1 for right).</param>
        /// <param name="run">True if running; otherwise, walking.</param>
        /// <param name="logic">Character logic instance to update movement state.</param>
        public void Move(float direction, bool run, CharacterLogic logic)
        {
            // Process input only if it has changed.
            if (Mathf.Approximately(direction, _lastHorizontalInput) && run == _lastIsRunning)
            {
                return;
            }

            _lastHorizontalInput = direction;
            _lastIsRunning = run;
            logic.Move(direction, run);
        }

        /// <summary>
        /// Processes a jump action.
        /// </summary>
        /// <param name="logic">Character logic instance to process the jump.</param>
        public void Jump(CharacterLogic logic)
        {
            logic.Jump();
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _characterData.jumpForce);
        }

        /// <summary>
        /// Processes the landing action.
        /// </summary>
        /// <param name="logic">Character logic instance to process landing.</param>
        public void Land(CharacterLogic logic)
        {
            logic.Land();
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0f);
        }

        /// <summary>
        /// Applies horizontal movement and gravity adjustments.
        /// </summary>
        /// <param name="horizontalInput">The current horizontal input value.</param>
        /// <param name="isRunning">Indicates if the character is running.</param>
        /// <param name="isExternallyAffected">If true, skips applying player input.</param>
        public void FixedUpdateMovement(float horizontalInput, bool isRunning, bool isExternallyAffected)
        {
            if (isExternallyAffected)
            {
                return;
            }

            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                float speed = isRunning ? _characterData.runSpeed : _characterData.walkSpeed;
                _rigidbody.velocity = new Vector2(horizontalInput * speed, _rigidbody.velocity.y);
            }
            else
            {
                _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
            }

            // Increase falling speed if the character is falling.
            if (_rigidbody.velocity.y < 0)
            {
                _rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (_characterData.fallMultiplier - 1) * Time.deltaTime;
            }
        }

        /// <summary>
        /// Checks whether the character is grounded.
        /// </summary>
        /// <returns>True if grounded; otherwise, false.</returns>
        public bool IsGrounded()
        {
            return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        }

        /// <summary>
        /// Updates the character’s sprite facing direction.
        /// </summary>
        /// <param name="transform">The character's transform.</param>
        /// <param name="facingRight">True if facing right; otherwise, false.</param>
        public void UpdateSpriteDirection(Transform transform, bool facingRight)
        {
            Vector3 scale = transform.localScale;
            scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}