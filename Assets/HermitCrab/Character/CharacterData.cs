using UnityEngine;

namespace HermitCrab.Character
{
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

        [Header("Miscellaneous")]
        public float actionCooldown = 1f;
        public float horizontalInputThreshold = 0.1f;
        public float punchOffsetMultiplier = 0.5f;
        public float flashRedDuration = 0.2f;

        [Header("Animator Parameters")]
        [SerializeField] private string jumpTrigger = "JumpTrigger";
        public int jumpTriggerHash;

        [SerializeField] private string doubleJumpTrigger = "DoubleJumpTrigger";
        public int doubleJumpTriggerHash;

        [SerializeField] private string shootTrigger = "ShootTrigger";
        public int shootTriggerHash;

        [SerializeField] private string jumpShootTrigger = "JumpShootTrigger";
        public int jumpShootTriggerHash;

        [SerializeField] private string runShootTrigger = "RunShootTrigger";
        public int runShootTriggerHash;

        [SerializeField] private string punchTrigger = "PunchTrigger";
        public int punchTriggerHash;

        [SerializeField] private string jumpPunchTrigger = "JumpPunchTrigger";
        public int jumpPunchTriggerHash;

        [SerializeField] private string dieTrigger = "DieTrigger";
        public int dieTriggerHash;

        [SerializeField] private string landTrigger = "LandTrigger";
        public int landTriggerHash;

        [SerializeField] private string isRunningParam = "isRunning";
        public int isRunningParamHash;

        [SerializeField] private string isWalkingParam = "isWalking";
        public int isWalkingParamHash;

        [SerializeField] private string isIdleParam = "isIdle";
        public int isIdleParamHash;

        private void OnValidate()
        {
            jumpTriggerHash = Animator.StringToHash(jumpTrigger);
            doubleJumpTriggerHash = Animator.StringToHash(doubleJumpTrigger);
            shootTriggerHash = Animator.StringToHash(shootTrigger);
            jumpShootTriggerHash = Animator.StringToHash(jumpShootTrigger);
            runShootTriggerHash = Animator.StringToHash(runShootTrigger);
            punchTriggerHash = Animator.StringToHash(punchTrigger);
            jumpPunchTriggerHash = Animator.StringToHash(jumpPunchTrigger);
            dieTriggerHash = Animator.StringToHash(dieTrigger);
            landTriggerHash = Animator.StringToHash(landTrigger);
            isRunningParamHash = Animator.StringToHash(isRunningParam);
            isWalkingParamHash = Animator.StringToHash(isWalkingParam);
            isIdleParamHash = Animator.StringToHash(isIdleParam);
        }
    }
}
