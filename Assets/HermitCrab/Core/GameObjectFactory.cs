using HermitCrab.Character;
using UnityEngine;

namespace HermitCrab.Factory
{
    /// <summary>
    /// Factory class for creating GameObjects.
    /// </summary>
    public static class GameObjectFactory
    {
        /// <summary>
        /// Instantiates a projectile and initializes its controller.
        /// </summary>
        /// <param name="projectilePrefab">Projectile prefab to instantiate.</param>
        /// <param name="spawnPosition">Position where the projectile is spawned.</param>
        /// <param name="projInfo">Initialization data for the projectile.</param>
        /// <returns>The instantiated projectile GameObject.</returns>
        public static GameObject CreateProjectile(GameObject projectilePrefab, Vector3 spawnPosition, ProjectileInfo projInfo)
        {
            if (projectilePrefab == null)
                return null;

            GameObject projInstance = GameObject.Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            ProjectileController controller = projInstance.GetComponent<ProjectileController>();
            if (controller != null)
            {
                controller.Initialize(projInfo);
            }
            return projInstance;
        }

        /// <summary>
        /// Instantiates an environment GameObject.
        /// </summary>
        /// <param name="prefab">Prefab to instantiate.</param>
        /// <param name="position">Position where the prefab is instantiated.</param>
        /// <param name="parent">Optional parent transform.</param>
        /// <returns>The instantiated environment GameObject.</returns>
        public static GameObject CreateEnvironmentObject(GameObject prefab, Vector3 position, Transform parent = null)
        {
            if (prefab == null)
                return null;

            return GameObject.Instantiate(prefab, position, Quaternion.identity, parent);
        }
    }
}