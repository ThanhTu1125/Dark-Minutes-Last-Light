
using UnityEngine;
using System;

public class PlayerStatus : MonoBehaviour
{
    [Header("Máu")]
    public int maxHealth = 100;
    public float currentHealth;

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
    public bool IsInvincible { get; set; } = false;

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
    // HỆ THỐNG MÁU
    // =========================

    public void TakeDamage(int damage)
    {
        if (isDead || IsInvincible || damage <= 0)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log("Aiden nhận " + damage +
                  " sát thương. HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
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

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log("Đã hồi máu. HP: " + currentHealth);
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        IsInvincible = false;

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

        // Tăng chỉ số nhân vật
        maxHealth += 20;
        maxEnergy += 10;
        maxStrength += 5;

        // Cập nhật chỉ số hiện tại
        currentHealth = maxHealth;
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
