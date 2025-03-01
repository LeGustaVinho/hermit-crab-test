using UnityEngine;

namespace HermitCrab.Camera
{
    [CreateAssetMenu(fileName = "CameraSettingsData", menuName = "Game/CameraSettingsData")]
    public class CameraSettingsData : ScriptableObject 
    {
        [Header("General")]
        public string playerTag = "Player";
    
        [Header("Framing Transposer Settings")]
        [Tooltip("Horizontal screen position (0 = left, 1 = right, 0.5 = center). " +
                 "Lower values shift the camera to the right, allowing more view ahead.")]
        [Range(0f, 1f)]
        public float screenXOffset = 0.4f;
        
        [Tooltip("Vertical screen position (0 = bottom, 1 = top, 0.5 = center).")]
        [Range(0f, 1f)]
        public float screenYOffset = 0.5f;
    }
}