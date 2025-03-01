using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HermitCrab.UI
{
    public class ContinuousPointerEventUpdate : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        // Event fired repeatedly while the pointer is held down.
        public event Action OnRepeatEvent;

        private bool _isPressed;

        // Called when the pointer is pressed down on this UI element.
        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
        }

        // Called when the pointer is released from this UI element.
        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
        }

        private void Update()
        {
            if (_isPressed)
            {
                OnRepeatEvent?.Invoke();
            }
        }
    }
}