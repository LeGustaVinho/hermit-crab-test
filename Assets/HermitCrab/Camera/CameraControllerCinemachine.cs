using Cinemachine;
using UnityEngine;

namespace HermitCrab.Camera
{
    /// <summary>
    /// Controls the Cinemachine Virtual Camera for a 2D platformer.
    /// This controller assigns the follow target and configures framing offsets.
    /// </summary>
    public class CameraControllerCinemachine : MonoBehaviour
    {
        [Header("Target to Follow (usually the player)")]
        public Transform target;

        [Header("Cinemachine Virtual Camera")]
        public CinemachineVirtualCamera virtualCamera;

        [Header("Framing Transposer Settings")]
        [Tooltip("Horizontal screen position (0 = left, 1 = right, 0.5 = center). " +
                 "Lower values shift the camera to the right, allowing more view ahead.")]
        [Range(0f, 1f)]
        public float screenXOffset = 0.4f;

        [Tooltip("Vertical screen position (0 = bottom, 1 = top, 0.5 = center).")]
        [Range(0f, 1f)]
        public float screenYOffset = 0.5f;

        [Header("Confiner Options (Optional)")]
        public CinemachineConfiner confiner; // Optional: assign to limit the camera's view area

        /// <summary>
        /// Initializes the virtual camera by assigning the follow target and configuring the framing transposer.
        /// If no target is set, it attempts to find a GameObject tagged "Player".
        /// If a confiner is assigned, it adds (or configures) a CinemachineConfiner to limit camera boundaries.
        /// </summary>
        private void Start()
        {
            // If the virtualCamera is not assigned via the Inspector, get it from the GameObject.
            if (virtualCamera == null)
            {
                virtualCamera = GetComponent<CinemachineVirtualCamera>();
            }

            // If the target is not set, try to find a GameObject tagged "Player".
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            // Assign the target to the virtual camera.
            if (virtualCamera != null)
            {
                virtualCamera.Follow = target;

                // Retrieve the Framing Transposer and set custom offsets.
                CinemachineFramingTransposer framingTransposer =
                    virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (framingTransposer != null)
                {
                    framingTransposer.m_ScreenX = screenXOffset;
                    framingTransposer.m_ScreenY = screenYOffset;
                }

                // If a confiner is assigned, add or configure the CinemachineConfiner to limit the camera's boundaries.
                if (confiner != null)
                {
                    // Attempt to get the existing CinemachineConfiner component.
                    CinemachineConfiner cameraConfiner = virtualCamera.GetComponent<CinemachineConfiner>();
                    if (cameraConfiner == null)
                    {
                        // If not found, add a new component.
                        cameraConfiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner>();
                    }
                    cameraConfiner.m_BoundingShape2D = confiner.m_BoundingShape2D;
                }
            }
        }
    }
}