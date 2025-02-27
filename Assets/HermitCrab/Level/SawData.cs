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
    }
}