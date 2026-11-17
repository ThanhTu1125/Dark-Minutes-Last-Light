using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Chỉ số Quái vật")]
    [SerializeField] private float maxHealth = 45f;
    [SerializeField] private float moveSpeed = 3.2f;
    [SerializeField] private int touchDamage = 10;
    [SerializeField] private float damageCooldown = 0.5f;
    [SerializeField] private int expReward = 15;
    [SerializeField] private float healthDropChance = 0.08f;

    [Header("Thông tin nhận diện")]
    public string enemyName = "Hắc Thú (Shadow Beast)";

    private float currentHealth;
    [Header("Giới hạn Khu đất (Arena Bounds)")]
    [SerializeField] private bool clampToArena = true;
    [SerializeField] private Vector2 arenaMin = new Vector2(-14.3f, -11.3f);
    [SerializeField] private Vector2 arenaMax = new Vector2(14.3f, 11.3f);

    private bool isDead = false;
    private float lastDamageTime = 0f;

    private Transform targetPlayer;
    private PlayerStatus playerStatus;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;

    public bool IsDead => isDead;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (targetPlayer == null)
        {
            GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
            {
                targetPlayer = playerGo.transform;
                playerStatus = playerGo.GetComponent<PlayerStatus>();
            }
        }
    }

    private void Update()
    {
        if (isDead || Time.timeScale <= 0.001f) return;

        if (targetPlayer == null)
        {
            FindPlayer();
            return;
        }

        // Đuổi theo người chơi
        Vector2 dir = (targetPlayer.position - transform.position);
        if (dir.sqrMagnitude > 0.01f)
        {
            Vector2 moveDir = dir.normalized;
            float stepDistance = moveSpeed * Time.deltaTime;

            // Kiểm tra va chạm với vật cản tĩnh (Cây cối / Tảng đá)
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.22f, moveDir, stepDistance);
            if (hit.collider != null && !hit.collider.isTrigger && hit.collider.CompareTag("Obstacle"))
            {
                // Trượt mượt mà theo tiếp tuyến của vật cản thay vì đâm xuyên qua
                Vector2 normal = hit.normal;
                Vector2 slideDir = new Vector2(-normal.y, normal.x);
                if (Vector2.Dot(slideDir, moveDir) < 0f) slideDir = -slideDir;

                transform.position += (Vector3)(slideDir * (moveSpeed * 0.85f * Time.deltaTime));
            }
            else
            {
                transform.position += (Vector3)(moveDir * stepDistance);
            }

            // Quay mặt quái theo hướng di chuyển
            if (spriteRenderer != null && Mathf.Abs(moveDir.x) > 0.05f)
            {
                spriteRenderer.flipX = (moveDir.x < 0f);
            }
        }

        // Giới hạn quái vật nghiêm ngặt trong khu đất
        if (clampToArena)
        {
            Vector3 pos = transform.position;
            float cX = Mathf.Clamp(pos.x, arenaMin.x, arenaMax.x);
            float cY = Mathf.Clamp(pos.y, arenaMin.y, arenaMax.y);
            if (cX != pos.x || cY != pos.y)
            {
                transform.position = new Vector3(cX, cY, pos.z);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDead || Time.timeScale <= 0.001f) return;

        if (collision.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                lastDamageTime = Time.time;
                if (playerStatus != null)
                {
                    playerStatus.TakeDamage(touchDamage);
                }
            }
        }
    }

    public void TakeDamage(DamageInfo damage)
    {
        if (isDead) return;

        currentHealth -= damage.Amount;

        // Hiệu ứng chớp đỏ khi trúng đạn
        StopAllCoroutines();
        StartCoroutine(FlashRed());

        // Đẩy lùi (Knockback)
        if (damage.Knockback > 0f)
        {
            transform.position += (Vector3)(damage.KnockbackDirection * (damage.Knockback * 0.08f));
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.25f, 0.25f, 1f);
            yield return new WaitForSeconds(0.09f);
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        GameSessionData.EnemiesKilled++;

        // Rơi hạt ngọc kinh nghiệm
        DropExperience();

        // Tỷ lệ nhỏ rơi bình máu
        if (Random.value < healthDropChance)
        {
            DropHealth();
        }

        Destroy(gameObject);
    }

    private void DropExperience()
    {
        GameObject xpGo = new GameObject("ExpDrop_" + enemyName);
        xpGo.transform.position = transform.position;
        xpGo.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

        CircleCollider2D col = xpGo.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        SpriteRenderer sr = xpGo.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.2f, 1f, 0.6f, 1f);
        sr.sortingOrder = 9;

        // Gán sprite hạt ngọc
        Sprite gemSprite = Resources.Load<Sprite>("item_outdated22");
        if (gemSprite != null) sr.sprite = gemSprite;

        ExperiencePickup expPickup = xpGo.AddComponent<ExperiencePickup>();
        expPickup.customExpAmount = expReward;
    }

    private void DropHealth()
    {
        GameObject hpGo = new GameObject("HealthDrop");
        hpGo.transform.position = transform.position + new Vector3(0.3f, 0f, 0f);
        hpGo.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

        CircleCollider2D col = hpGo.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        SpriteRenderer sr = hpGo.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.35f, 0.35f, 1f);
        sr.sortingOrder = 9;

        Sprite hpSprite = Resources.Load<Sprite>("item_outdated14");
        if (hpSprite != null) sr.sprite = hpSprite;

        HealthPickup hpPickup = hpGo.AddComponent<HealthPickup>();
        hpPickup.customHealAmount = 1;
    }

    public void SetStats(float hp, float speed, int damage, int exp, string name)
    {
        maxHealth = hp;
        currentHealth = hp;
        moveSpeed = speed;
        touchDamage = damage;
        expReward = exp;
        enemyName = name;
    }
}
