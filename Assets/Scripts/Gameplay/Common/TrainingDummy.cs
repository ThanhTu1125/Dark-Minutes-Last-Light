using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class TrainingDummy : MonoBehaviour, IDamageable
{
    [Header("Chỉ số")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private SpriteRenderer sr;
    private Color originalColor = Color.white;
    private bool isDead = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        currentHealth = maxHealth;
    }

    public void TakeDamage(DamageInfo damage)
    {
        if (isDead) return;

        currentHealth -= damage.Amount;
        Debug.Log($"<color=yellow>[Bia Tập Bắn]</color> Nhận {damage.Amount:F0} sát thương ({damage.Type})! Máu còn: {Mathf.Max(0, currentHealth):F0}/{maxHealth}");

        // Hiệu ứng chớp đỏ khi trúng đòn (Hit Flash)
        StopAllCoroutines();
        StartCoroutine(FlashRed());

        // Hiệu ứng đẩy lùi nhẹ khi trúng đạn (Knockback)
        if (damage.Knockback > 0f)
        {
            transform.position += (Vector3)(damage.KnockbackDirection * (damage.Knockback * 0.05f));
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        if (sr != null)
        {
            sr.color = new Color(1f, 0.3f, 0.3f, 1f);
            yield return new WaitForSeconds(0.12f);
            sr.color = originalColor;
        }
    }

    private void Die()
    {
        isDead = true;
        GameSessionData.EnemiesKilled++;
        Debug.Log("<color=green>[Bia Tập Bắn]</color> Đã bị phá hủy! Rơi ra hạt kinh nghiệm và sẽ tự hồi sinh sau 2.5 giây.");

        // Tạo hạt kinh nghiệm rơi ra tại chỗ
        GameObject xpGo = new GameObject("Dummy_ExpDrop");
        xpGo.transform.position = transform.position;
        xpGo.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

        CircleCollider2D col = xpGo.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        SpriteRenderer xpSr = xpGo.AddComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null) xpSr.sprite = sr.sprite;
        xpSr.color = new Color(0.4f, 1f, 0.7f, 1f);
        xpGo.AddComponent<ExperiencePickup>();

        if (sr != null) sr.enabled = false;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(2.5f);
        currentHealth = maxHealth;
        isDead = false;
        if (sr != null)
        {
            sr.enabled = true;
            sr.color = originalColor;
        }
    }
}
