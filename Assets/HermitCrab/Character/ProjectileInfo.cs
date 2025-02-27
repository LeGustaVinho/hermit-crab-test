using UnityEngine;

namespace HermitCrab.Character
{
    /// <summary>
    /// Encapsulates data for initializing a projectile.
    /// </summary>
    public class ProjectileInfo 
    {
        /// <summary>
        /// Speed at which the projectile moves.
        /// </summary>
        public float Speed;

        /// <summary>
        /// Maximum distance the projectile can travel.
        /// </summary>
        public float MaxDistance;

        /// <summary>
        /// Damage dealt by the projectile.
        /// </summary>
        public int Damage;

        /// <summary>
        /// Direction of the projectile's movement.
        /// </summary>
        public Vector2 Direction;

        /// <summary>
        /// Position of the attacker who fired the projectile.
        /// </summary>
        public Vector3 AttackerPosition;
    }
}