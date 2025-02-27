using System;
using UnityEngine;

namespace HermitCrab.Character
{
    public class CharacterController : MonoBehaviour
    {
        [Header("Configurações do Personagem")]
        public CharacterData characterData; // Dados e referências, inclusive o prefab do projetil

        [Header("Spawn e Ground Check")] public Transform projectileSpawnPoint; // Ponto de spawn do projetil
        public Transform groundCheck; // Ponto para verificação do chão
        public float groundCheckRadius = 0.1f; // Raio da verificação
        public LayerMask groundLayer; // Camada atribuída ao chão

        [Header("Configurações de Soco")] public float punchRange = 1f; // Alcance do soco
        public LayerMask enemyLayer; // Layer dos inimigos que receberão dano

        public event Action<Vector3> OnDamageReceived;
        
        private CharacterLogic logic;
        private Animator animator;
        private Rigidbody2D rb;

        // Variáveis para controle de input
        private float horizontalInput;
        private bool isRunning;

        // Variáveis para detectar mudanças no input
        private float lastHorizontalInput;
        private bool lastIsRunning;

        // Flags de estado
        private bool wasIdle = false;
        private bool isDead; // indica se o personagem já morreu

        // Variáveis de cooldown para ações
        private float lastPunchTime = -Mathf.Infinity;
        private float lastShootTime = -Mathf.Infinity;
        private const float actionCooldown = 1f; // 1 segundo de cooldown

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            logic = new CharacterLogic(characterData);
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            // Outros eventos (pulo, ataque, etc.) permanecem inalterados
            logic.OnJump += () =>
            {
                if (!isDead) animator.SetTrigger("JumpTrigger");
            };
            logic.OnDoubleJump += () =>
            {
                if (!isDead) animator.SetTrigger("DoubleJumpTrigger");
            };
            logic.OnShoot += () =>
            {
                if (!isDead) animator.SetTrigger("ShootTrigger");
            };
            logic.OnJumpShoot += () =>
            {
                if (!isDead) animator.SetTrigger("JumpShootTrigger");
            };
            logic.OnRunShoot += () =>
            {
                if (!isDead) animator.SetTrigger("RunShootTrigger");
            };
            logic.OnPunch += () =>
            {
                if (!isDead) animator.SetTrigger("PunchTrigger");
            };
            logic.OnJumpPunch += () =>
            {
                if (!isDead) animator.SetTrigger("JumpPunchTrigger");
            };
            logic.OnDeath += () =>
            {
                if (!isDead)
                {
                    animator.SetTrigger("DieTrigger");
                    Die();
                }
            };
            logic.OnProjectileCreated += CreateProjectile;
        }

        private void Update()
        {
            // Se estiver morto, não processa nenhuma lógica
            if (isDead) return;

            logic.Update(Time.deltaTime);
            CheckGround();
            UpdateSpriteDirection();
            UpdateAnimationState();
        }

        private void FixedUpdate()
        {
            if (isDead)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            // Aplica movimento horizontal sem deslizar quando não há input
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                float speed = isRunning ? characterData.runSpeed : characterData.walkSpeed;
                rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }

            // Aumenta a velocidade de queda se estiver caindo
            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (characterData.fallMultiplier - 1) * Time.deltaTime;
            }
        }

        // Atualiza o input de movimento; dispara atualizações somente se houver mudança
        public void Move(float direction, bool run)
        {
            if (isDead) return;

            if (Mathf.Approximately(direction, lastHorizontalInput) && run == lastIsRunning)
            {
                return;
            }

            lastHorizontalInput = direction;
            lastIsRunning = run;

            horizontalInput = direction;
            isRunning = run;
            logic.Move(direction, run);
            // Atualiza os parâmetros do Animator
            if (Mathf.Abs(direction) > 0.1f)
            {
                if (run)
                {
                    animator.SetBool("isRunning", true);
                    animator.SetBool("isWalking", false);
                }
                else
                {
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isWalking", true);
                }
            }
            else
            {
                animator.SetBool("isRunning", false);
                animator.SetBool("isWalking", false);
                animator.SetBool("isIdle", true);
            }
        }

        public void Jump()
        {
            if (isDead) return;
            if (IsGrounded() || logic.CanDoubleJump)
            {
                logic.Jump();
                rb.velocity = new Vector2(rb.velocity.x, characterData.jumpForce);
            }
        }

        public void Land()
        {
            if (isDead) return;
            logic.Land();
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }

        public void Shoot() 
        {
            if (isDead) return;
            if (Time.time - lastShootTime < actionCooldown)
                return;
            lastShootTime = Time.time;
            // Passa a posição atual do personagem como AttackerPosition
            logic.Shoot(transform.position);
        }

        public void Punch()
        {
            if (isDead) return;
            if (Time.time - lastPunchTime < actionCooldown) return;
            lastPunchTime = Time.time;
            logic.Punch();
            Vector2 punchOrigin = (Vector2)transform.position +
                                  (logic.FacingRight ? Vector2.right : Vector2.left) * (punchRange * 0.5f);
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(punchOrigin, punchRange, enemyLayer);
            foreach (Collider2D collider in hitColliders)
            {
                CharacterController target = collider.GetComponent<CharacterController>();
                if (target != null && target != this)
                {
                    target.TakeDamage(DamageType.Physical, characterData.punchDamage, transform.position);
                }
            }
        }

        // Cria o projetil a partir do prefab definido no CharacterData
        private void CreateProjectile(ProjectileInfo projInfo)
        {
            if (characterData.projectilePrefab && projectileSpawnPoint)
            {
                GameObject projInstance = Instantiate(characterData.projectilePrefab, projectileSpawnPoint.position,
                    Quaternion.identity);
                ProjectileController controller = projInstance.GetComponent<ProjectileController>();
                if (controller != null)
                {
                    controller.Initialize(projInfo);
                }
            }
        }

        // Verifica se o personagem está tocando o chão
        private void CheckGround()
        {
            if (rb.velocity.y <= 0)
            {
                bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
                if (isGrounded && logic.IsJumping)
                {
                    Land();
                    animator.SetTrigger("LandTrigger");
                }
            }
        }

        // Atualiza a direção do sprite conforme o movimento
        private void UpdateSpriteDirection()
        {
            Vector3 scale = transform.localScale;
            scale.x = logic.FacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // Atualiza os estados do Animator para manter o loop de movimento correto
        private void UpdateAnimationState()
        {
            if (isDead) return;
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                animator.SetBool("isIdle", false);
            }
            else
            {
                animator.SetBool("isIdle", true);
            }
        }

        // Verifica se o personagem está no chão
        private bool IsGrounded()
        {
            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Método para receber dano, agora com a posição do atacante.
        public void TakeDamage(DamageType damageType, int damage, Vector3 attackerPosition) {
            if (isDead) return;
            logic.ReceiveDamage(damageType, damage);

            // Dispara o evento para avisar que este personagem foi atingido.
            if (OnDamageReceived != null) {
                OnDamageReceived(attackerPosition);
            }
        }


        // Propriedade para saber se o personagem está vivo
        public bool IsAlive => !isDead;

        // Quando o personagem morre, desativa física e colisões e impede novas ações
        public void Die()
        {
            isDead = true;
            if (rb != null)
            {
                rb.simulated = false;
            }

            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }
        }
    }
}