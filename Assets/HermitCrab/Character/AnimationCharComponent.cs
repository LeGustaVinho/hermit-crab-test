using UnityEngine;

namespace HermitCrab.Character
{
    /// <summary>
    /// Provides animation-related functionalities for the character.
    /// </summary>
    public class AnimationCharComponent
    {
        private readonly Animator _animator;
        private readonly CharacterData _characterData;

        public AnimationCharComponent(Animator animator, CharacterData characterData)
        {
            _animator = animator;
            _characterData = characterData;
        }

        public void TriggerJump()
        {
            _animator.SetTrigger(_characterData.jumpTriggerHash);
        }

        public void TriggerDoubleJump()
        {
            _animator.SetTrigger(_characterData.doubleJumpTriggerHash);
        }

        public void TriggerShoot()
        {
            _animator.SetTrigger(_characterData.shootTriggerHash);
        }

        public void TriggerJumpShoot()
        {
            _animator.SetTrigger(_characterData.jumpShootTriggerHash);
        }

        public void TriggerRunShoot()
        {
            _animator.SetTrigger(_characterData.runShootTriggerHash);
        }

        public void TriggerPunch()
        {
            _animator.SetTrigger(_characterData.punchTriggerHash);
        }

        public void TriggerJumpPunch()
        {
            _animator.SetTrigger(_characterData.jumpPunchTriggerHash);
        }

        public void TriggerDeath()
        {
            _animator.SetTrigger(_characterData.dieTriggerHash);
        }

        public void TriggerLand()
        {
            _animator.SetTrigger(_characterData.landTriggerHash);
        }

        /// <summary>
        /// Updates the idle state based on horizontal movement input.
        /// </summary>
        /// <param name="horizontalInput">The horizontal movement input value.</param>
        public void UpdateMovementAnimation(float horizontalInput)
        {
            bool isMoving = Mathf.Abs(horizontalInput) > _characterData.horizontalInputThreshold;
            _animator.SetBool(_characterData.isIdleParamHash, !isMoving);
        }

        /// <summary>
        /// Sets the running and walking animation states.
        /// </summary>
        /// <param name="direction">Horizontal movement direction.</param>
        /// <param name="isRunning">True if running; otherwise, walking.</param>
        public void SetMovementState(float direction, bool isRunning)
        {
            if (Mathf.Abs(direction) > 0.1f)
            {
                if (isRunning)
                {
                    _animator.SetBool(_characterData.isRunningParamHash, true);
                    _animator.SetBool(_characterData.isWalkingParamHash, false);
                }
                else
                {
                    _animator.SetBool(_characterData.isRunningParamHash, false);
                    _animator.SetBool(_characterData.isWalkingParamHash, true);
                }
            }
            else
            {
                _animator.SetBool(_characterData.isRunningParamHash, false);
                _animator.SetBool(_characterData.isWalkingParamHash, false);
                _animator.SetBool(_characterData.isIdleParamHash, true);
            }
        }
    }
}
