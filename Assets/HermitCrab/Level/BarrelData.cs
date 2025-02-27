using UnityEngine;

namespace HermitCrab.Level
{
    [CreateAssetMenu(fileName = "BarrelData", menuName = "Game/BarrelData")]
    public class BarrelData : ScriptableObject
    {
        [Header("Barrel Type")] public BarrelType Type = BarrelType.Explosive;

        [Header("VFX")]
        public GameObject explosionVfx;
        public float explosionVfxLifetime;
        
        [Header("Explosive Settings")] public int ExplosiveDamage = 80;

        public float ExplosionRadius = 5f;
        public float KnockbackForce = 10f;

        [Header("Poison Settings")] public int PoisonDamagePerSecond = 10;

        public float PoisonDuration = 5f;
        public float PoisonRadius = 5f;
    }
}