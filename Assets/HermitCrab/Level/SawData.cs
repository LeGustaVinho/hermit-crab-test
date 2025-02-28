using UnityEngine;

namespace HermitCrab.Level
{
    /// <summary>
    ///     Defines the configuration for a Saw hazard.
    /// </summary>
    [CreateAssetMenu(fileName = "SawData", menuName = "Game/SawData")]
    public class SawData : ScriptableObject
    {
        public int DamagePerSecond = 20;
        public float KnockbackForce = 5f;

        // Movement configuration fields
        [Tooltip("Rotation speed of the saw in degrees per second.")]
        public float rotationSpeed = 360f;
        
        public enum MovementType { Horizontal, Vertical }
        public MovementType movementType = MovementType.Horizontal;
        public float movementSpeed = 1f; // Speed at which the saw moves (units per second)
        public float movementDistance = 3f; // Total distance the saw travels from one end to the other
    }
}