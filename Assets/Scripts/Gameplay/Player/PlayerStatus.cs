
using UnityEngine;
using System;

public class PlayerStatus : MonoBehaviour
{
    [Header("Máu (Hệ thống 3 Trái Tim)")]
    public int maxHealth = 3;
    public float currentHealth = 3f;
    [SerializeField] private float invincibilityDuration = 1.0f;

    [Header("Năng lượng")]
    public int maxEnergy = 100;
    public int currentEnergy;

    [Header("Sức mạnh")]
    public int maxStrength = 100;
    public int currentStrength;

    [Header("Tốc độ")]
    public float speed = 10f;

    [Header("Kinh nghiệm")]
    public int currentExperience = 0;
    public int experienceToNextLevel = 100;

    [Header("Cấp độ")]
    public int currentLevel = 1;

    [Header("Trạng thái")]
    public bool isDead = false;

    private bool isHitInvincible = false;
    private bool isDashInvincible = false;

    public bool IsInvincible
    {
        get => isHitInvincible || isDashInvincible;
        set => isDashInvincible = value;
    }

    // Events để UI và các hệ thống khác theo dõi
    public event Action<float, float> OnHealthChanged;
    public event Action<int, int> OnEnergyChanged;
    public event Action<int, int> OnExperienceChanged;
    public event Action<int> OnLevelUp;
    public event Action OnDied;

    private void Awake()
    {
        // Khởi tạo chỉ số ban đầu
        maxHealth = Mathf.Max(1, maxHealth);
        maxEnergy = Mathf.Max(0, maxEnergy);
        maxStrength = Mathf.Max(0, maxStrength);
        speed = Mathf.Max(0f, speed);

        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentStrength = maxStrength;

        currentLevel = Mathf.Max(1, currentLevel);
        experienceToNextLevel = Mathf.Max(1, experienceToNextLevel);

        currentExperience = Mathf.Max(0, currentExperience);
    }

    private void Start()
    {
        Debug.Log("Aiden đã được khởi tạo thành công!");
    }

    // =========================
    // HỆ THỐNG MÁU (3 TRÁI TIM)
    // =========================

    public void TakeDamage(int damage)
    {
        if (isDead || IsInvincible || damage <= 0)
            return;

        // Mỗi lần va chạm mất 1 trái tim
        int heartsLost = 1;
        currentHealth -= heartsLost;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"Aiden mất {heartsLost} Trái Tim. Máu còn lại: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Bật thời gian bất tử ngắn và chớp nháy Sprite
            TriggerInvincibility(invincibilityDuration);
        }
    }

    public void takeDamage(int damage)
    {
        TakeDamage(damage);
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
            return;

        // Hồi 1 trái tim
        int heartsGained = 1;
        currentHealth += heartsGained;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"Đã hồi phục {heartsGained} Trái Tim. Máu: {currentHealth}/{maxHealth}");
    }

    public void TriggerInvincibility(float duration)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(InvincibilityRoutine(duration));
        }
    }

    private System.Collections.IEnumerator InvincibilityRoutine(float duration)
    {
        isHitInvincible = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;

        float elapsed = 0f;
        float blinkInterval = 0.1f;
        bool isDimmed = false;

        while (elapsed < duration)
        {
            elapsed += blinkInterval;
            isDimmed = !isDimmed;
            if (sr != null)
            {
                sr.color = isDimmed ? new Color(originalColor.r, originalColor.g, originalColor.b, 0.25f) : originalColor;
            }
            yield return new WaitForSeconds(blinkInterval);
        }

        if (sr != null) sr.color = originalColor;
        isHitInvincible = false;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        isHitInvincible = false;
        isDashInvincible = false;

        Debug.Log("Aiden đã chết! GAME OVER");

        OnDied?.Invoke();
    }

    //Level

    public void GainExperience(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentExperience += amount;

        Debug.Log("Nhận được " + amount + " EXP.");

        // Hỗ trợ lên nhiều cấp nếu nhận lượng EXP lớn
        while (currentExperience >= experienceToNextLevel)
        {
            LevelUp();

            if (isDead) return;
        }

        OnExperienceChanged?.Invoke(
            currentExperience,
            experienceToNextLevel
        );
    }

    public void LevelUp()
    {
        if (isDead) return;

        currentExperience -= experienceToNextLevel;

        currentLevel++;

        // Tăng lượng EXP cần thiết cho cấp tiếp theo
        experienceToNextLevel += 50;

        // Tăng chỉ số nhân vật (Hồi phục toàn bộ trái tim khi lên cấp)
        currentHealth = maxHealth;
        maxEnergy += 10;
        maxStrength += 5;

        // Cập nhật chỉ số hiện tại
        currentEnergy = maxEnergy;
        currentStrength = maxStrength;

        // Thông báo cho UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

        OnLevelUp?.Invoke(currentLevel);

        Debug.Log("LEVEL UP! Cấp hiện tại: " + currentLevel);
    }

    // Năng lượng
    public void UseEnergy(int amount)
    {
        if (isDead || amount <= 0)
            return;

        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;

            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

            Debug.Log("Đã sử dụng năng lượng. Energy: "
                      + currentEnergy);
        }
        else
        {
            Debug.Log("Không đủ năng lượng!");
        }
    }
    // Hồi phục năng lượng
    public void RestoreEnergy(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

        Debug.Log("Đã hồi năng lượng. Energy: " + currentEnergy);
    }
    //Khôi phục toàn bộ trạng thái của nhân vật
    public void RestoreFullStatus()
    {
        if (isDead) return;

        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentStrength = maxStrength;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

        Debug.Log("Đã hồi phục toàn bộ trạng thái!");
    }
}
