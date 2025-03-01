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
        public bool InitializeOnStart;
        
        [Header("Target to Follow (usually the player)")]
        public Transform target;

        [Header("Cinemachine Virtual Camera")]
        public CinemachineVirtualCamera virtualCamera;

        [Header("Confiner Options (Optional)")]
        public CinemachineConfiner confiner;

        [Header("Configuration")]
        public CameraSettingsData cameraSettings;

        private void Start()
        {
            if (InitializeOnStart)
                Initialize();
        }

        /// <summary>
        /// Initializes the virtual camera by assigning the follow target and configuring the framing transposer.
        /// If no target is set, it attempts to find a GameObject tagged "Player".
        /// If a confiner is assigned, it adds (or configures) a CinemachineConfiner to limit camera boundaries.
        /// </summary>
        public void Initialize()
        {
            if (virtualCamera == null)
                virtualCamera = GetComponent<CinemachineVirtualCamera>();

            // Use a tag definida no ScriptableObject para encontrar o player.
            if (target == null && cameraSettings != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(cameraSettings.playerTag);
                if (player != null)
                    target = player.transform;
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
                    framingTransposer.m_ScreenX = cameraSettings.screenXOffset;
                    framingTransposer.m_ScreenY = cameraSettings.screenYOffset;
                }

                // If a confiner is assigned, add or configure the CinemachineConfiner to limit the camera's boundaries.
                if (confiner != null)
                {
                    CinemachineConfiner cameraConfiner = virtualCamera.GetComponent<CinemachineConfiner>();
                    if (cameraConfiner == null)
                        cameraConfiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner>();

                    cameraConfiner.m_BoundingShape2D = confiner.m_BoundingShape2D;
                }
            }
        }
    }
}
