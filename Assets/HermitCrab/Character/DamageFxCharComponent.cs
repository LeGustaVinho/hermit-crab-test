using System.Collections;
using UnityEngine;

namespace HermitCrab.Character
{
    /// <summary>
    /// Provides damage-related functionalities such as visual feedback when damage is taken.
    /// </summary>
    public class DamageFxCharComponent
    {
        private readonly SpriteRenderer _spriteRenderer;
        private readonly Color _originalColor;
        private bool _isPoisoned;

        public DamageFxCharComponent(SpriteRenderer spriteRenderer)
        {
            _spriteRenderer = spriteRenderer;
            _originalColor = _spriteRenderer ? _spriteRenderer.color : Color.white;
        }

        /// <summary>
        /// Sets the poisoned state and updates the sprite color.
        /// </summary>
        /// <param name="state">True to mark as poisoned; false to revert.</param>
        public void SetPoisoned(bool state)
        {
            _isPoisoned = state;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _isPoisoned ? Color.green : _originalColor;
            }
        }

        /// <summary>
        /// Coroutine to flash the sprite red when damage is taken.
        /// </summary>
        /// <returns>An IEnumerator for the coroutine.</returns>
        public IEnumerator FlashRed()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                _spriteRenderer.color = _isPoisoned ? Color.green : _originalColor;
            }
        }
    }
}