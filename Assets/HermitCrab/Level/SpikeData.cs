using UnityEngine;

namespace HermitCrab.Level
{
    /// <summary>
    ///     Defines the configuration for a Spike hazard.
    /// </summary>
    [CreateAssetMenu(fileName = "SpikeData", menuName = "Game/SpikeData")]
    public class SpikeData : ScriptableObject
    {
        public int DamagePerSecond = 20;
        public float KnockbackForce = 5f;
    }
}