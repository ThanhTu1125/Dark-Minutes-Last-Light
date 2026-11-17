using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Chỉ số cơ bản")]
    [SerializeField] private float speed = 16f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float knockback = 5f;
    [SerializeField] private int pierceCount = 1;
    [SerializeField] private DamageType damageType = DamageType.Physical;
    [SerializeField] private float lifetime = 2.5f;

    [Header("Hiệu ứng xoay khi bay (dành cho Rìu)")]
    public bool spinInFlight = false;
    [SerializeField] private float spinSpeed = 900f;

    private Vector2 moveDirection;
    private GameObject sourceOwner;
    private bool isInitialized = false;

    public void Initialize(
        Vector2 direction,
        float dmg,
        float spd,
        float kb,
        int pierce,
        DamageType type,
        GameObject owner)
    {
        moveDirection = direction.normalized;
        damage = dmg;
        speed = spd;
        knockback = kb;
        pierceCount = Mathf.Max(1, pierce);
        damageType = type;
        sourceOwner = owner;
        isInitialized = true;

        // Xoay đạn theo hướng bắn ban đầu
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Đảm bảo Collider là Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Tự hủy sau khi bay hết tầm
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Di chuyển đạn
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        // Xoay tròn khi bay (cho Rìu)
        if (spinInFlight)
        {
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Bỏ qua va chạm với Player hoặc các viên đạn khác của đồng minh
        if (other.CompareTag("Player") || other.GetComponent<Projectile>() != null)
        {
            return;
        }

        // Kiểm tra đối tượng có thể nhận sát thương (Quái vật, Thùng, Boss...)
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = other.GetComponentInParent<IDamageable>();
        }

        if (damageable != null)
        {
            DamageInfo damageInfo = new DamageInfo(damage, damageType, knockback, moveDirection, sourceOwner);
            damageable.TakeDamage(damageInfo);

            pierceCount--;
            if (pierceCount <= 0)
            {
                Destroy(gameObject);
            }
        }
        else if (!other.isTrigger && !other.CompareTag("Player"))
        {
            // Trúng va chạm vật lý đặc (chướng ngại vật, cây cối, bờ tường) thì tự hủy
            Destroy(gameObject);
        }
    }
}
