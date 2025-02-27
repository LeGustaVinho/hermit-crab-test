using UnityEngine;

namespace HermitCrab.Character
{
    /// <summary>
    /// Holds the configuration data and constants for a character.
    /// This ScriptableObject includes status, action costs, projectile settings, movement speeds, and gravity multiplier.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Game/CharacterData")]
    public class CharacterData : ScriptableObject {
        [Header("Status")]
        public int maxHealth = 100;
        public int maxEnergy = 100;
        public float energyRechargeRate = 1f;

        [Header("Actions - Energy and Damage")]
        public int shootEnergyCost = 10;
        public int projectileDamage = 20;
        public int punchDamage = 15;

        [Header("Projectile")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 10f;
        public float projectileMaxDistance = 15f;

        [Header("Movement")]
        public float walkSpeed = 2f;
        public float runSpeed = 4f;
        public float jumpForce = 5f;
        
        [Header("Gravity")]
        [Tooltip("Multiplier for gravity when the character is falling.")]
        public float fallMultiplier = 2.5f;
    }
}