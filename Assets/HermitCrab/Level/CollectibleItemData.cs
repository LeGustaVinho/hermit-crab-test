namespace HermitCrab.Level
{
    using UnityEngine;

    namespace HermitCrab.Items
    {
        /// <summary>
        /// Contains configuration data for a collectible item.
        /// </summary>
        [CreateAssetMenu(fileName = "CollectibleItemData", menuName = "Game/CollectibleItemData")]
        public class CollectibleItemData : ScriptableObject
        {
            [Header("Item Restoration Values")]
            [Tooltip("Amount of energy to restore when collected.")]
            public int energyRestore = 50;
        
            [Tooltip("Amount of health to restore when collected.")]
            public int healthRestore = 50;
        }
    }
}