using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Tham chiếu Player")]
    [SerializeField] private PlayerStatus playerStatus;

    [Header("Thanh Máu (Health Bar)")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Hệ thống 3 Trái Tim (Hearts UI)")]
    [SerializeField] private System.Collections.Generic.List<Image> heartImages = new System.Collections.Generic.List<Image>();
    [SerializeField] private Sprite heartFullSprite;
    [SerializeField] private Sprite heartEmptySprite;
    [SerializeField] private TextMeshProUGUI heartText;

    [Header("Thanh Kinh Nghiệm (XP Bar)")]
    [SerializeField] private Image xpFillImage;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Đồng hồ Sinh Tồn (Timer)")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI waveWarningText;
    [SerializeField] private float targetSurvivalSeconds = 1200f; // 20 phút = 1200 giây
    [SerializeField] private float finalWaveSeconds = 1170f;     // 19 phút 30 giây = 1170 giây

    [Header("Debug / Phím Tắt Thử Nghiệm")]
    [Tooltip("Bật để dùng phím tắt kiểm tra: T (Tua thời gian), H (Mất máu), J (Hồi máu), X (Nhận EXP)")]
    [SerializeField] private bool enableDebugHotkeys = true;

    private float currentSurvivalTime = 0f;
    private bool isGameRunning = true;
    private bool isFinalWaveActive = false;
    private bool isBossActive = false;

    public float CurrentSurvivalTime => currentSurvivalTime;

    private float targetHealthFill = 1f;
    private float targetXpFill = 0f;

    private void Start()
    {
        // 1. Tự động tìm PlayerStatus nếu chưa gán thủ công
        if (playerStatus == null)
        {
            playerStatus = FindFirstObjectByType<PlayerStatus>();
        }

        // 2. Đăng ký sự kiện từ PlayerStatus
        if (playerStatus != null)
        {
            playerStatus.OnHealthChanged += UpdateHealthUI;
            playerStatus.OnExperienceChanged += UpdateXpUI;
            playerStatus.OnLevelUp += UpdateLevelUI;
            playerStatus.OnDied += HandlePlayerDeath;

            // Khởi tạo hiển thị giá trị ban đầu
            UpdateHealthUI(playerStatus.currentHealth, playerStatus.maxHealth);
            UpdateXpUI(playerStatus.currentExperience, playerStatus.experienceToNextLevel);
            UpdateLevelUI(playerStatus.currentLevel);
        }
        else
        {
            Debug.LogWarning("HUDController: Không tìm thấy PlayerStatus trong Scene!");
        }

        // 3. Khởi tạo Timer và cảnh báo
        if (waveWarningText != null)
        {
            waveWarningText.gameObject.SetActive(false);
        }

        currentSurvivalTime = 0f;
        UpdateTimerDisplay();
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện tránh rò rỉ bộ nhớ
        if (playerStatus != null)
        {
            playerStatus.OnHealthChanged -= UpdateHealthUI;
            playerStatus.OnExperienceChanged -= UpdateXpUI;
            playerStatus.OnLevelUp -= UpdateLevelUI;
            playerStatus.OnDied -= HandlePlayerDeath;
        }
    }

    private void Update()
    {
        // Xử lý đếm thời gian
        if (isGameRunning)
        {
            currentSurvivalTime += Time.deltaTime;
            GameSessionData.SurvivalTime = currentSurvivalTime;
            CheckMilestones();
            UpdateTimerDisplay();
        }

        // Chạy hiệu ứng trượt mượt mà cho thanh Máu và XP
        SmoothUpdateBars();

        // Xử lý phím tắt test nhanh
        if (enableDebugHotkeys)
        {
            HandleDebugHotkeys();
        }
    }

    // =========================================================================
    // HỆ THỐNG MÁU & LEVEL & XP
    // =========================================================================

    public void SetHeartComponents(System.Collections.Generic.List<Image> hearts, Sprite fullSprite, Sprite emptySprite, TextMeshProUGUI text)
    {
        heartImages = hearts;
        heartFullSprite = fullSprite;
        heartEmptySprite = emptySprite;
        heartText = text;
    }

    public void UpdateHealthUI(float current, float max)
    {
        if (max <= 0) max = 1;
        targetHealthFill = Mathf.Clamp01(current / max);

        int currentHearts = Mathf.RoundToInt(current);
        int maxHearts = Mathf.RoundToInt(max);

        // Cập nhật từng Trái Tim trên giao diện HUD
        if (heartImages != null && heartImages.Count > 0)
        {
            for (int i = 0; i < heartImages.Count; i++)
            {
                if (heartImages[i] == null) continue;

                if (i < currentHearts)
                {
                    // Trái tim đầy (Đỏ tươi)
                    if (heartFullSprite != null) heartImages[i].sprite = heartFullSprite;
                    heartImages[i].color = Color.white;
                    heartImages[i].transform.localScale = Vector3.one;
                }
                else
                {
                    // Trái tim đã mất (Tối xám mờ)
                    if (heartEmptySprite != null) heartImages[i].sprite = heartEmptySprite;
                    heartImages[i].color = new Color(1f, 1f, 1f, 0.35f);
                    heartImages[i].transform.localScale = Vector3.one * 0.9f;
                }
            }

            // Hiệu ứng cảnh báo khi chỉ còn 1 tim (Thót tim)
            if (currentHearts == 1 && heartImages.Count > 0 && heartImages[0] != null)
            {
                StartCoroutine(HeartPulseRoutine(heartImages[0].transform));
            }
        }

        if (heartText != null)
        {
            heartText.text = $"{currentHearts} / {maxHearts}";
        }

        if (healthText != null)
        {
            healthText.text = $"HP  {currentHearts} / {maxHearts}";
        }
    }

    private System.Collections.IEnumerator HeartPulseRoutine(Transform heartTf)
    {
        if (heartTf == null) yield break;
        float elapsed = 0f;
        while (elapsed < 0.25f && heartTf != null)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(elapsed * Mathf.PI * 4f) * 0.25f;
            heartTf.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        if (heartTf != null) heartTf.localScale = Vector3.one;
    }

    public void UpdateXpUI(int current, int toNext)
    {
        if (toNext <= 0) toNext = 1;
        targetXpFill = Mathf.Clamp01((float)current / toNext);

        if (xpText != null)
        {
            xpText.text = $"XP  {current} / {toNext}";
        }
    }

    public void UpdateLevelUI(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"LV. {level}";
        }
    }

    private void HandlePlayerDeath()
    {
        isGameRunning = false;
        if (heartImages != null)
        {
            foreach (var h in heartImages)
            {
                if (h != null)
                {
                    if (heartEmptySprite != null) h.sprite = heartEmptySprite;
                    h.color = new Color(0.4f, 0.4f, 0.4f, 0.25f);
                }
            }
        }
        if (heartText != null) heartText.text = "0 / 3 (DEAD)";
        if (healthText != null) healthText.text = "HP  0 / DEAD";
        targetHealthFill = 0f;
    }

    private void SmoothUpdateBars()
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetHealthFill, Time.deltaTime * smoothSpeed);
        }

        if (xpFillImage != null)
        {
            xpFillImage.fillAmount = Mathf.Lerp(xpFillImage.fillAmount, targetXpFill, Time.deltaTime * smoothSpeed);
        }
    }

    // =========================================================================
    // HỆ THỐNG ĐỒNG HỒ & CẢNH BÁO
    // =========================================================================

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        if (isBossActive)
        {
            timerText.text = "<color=#FF2222>BOSS: ECLIPSE GUARDIAN</color>";
            return;
        }

        int minutes = Mathf.FloorToInt(currentSurvivalTime / 60F);
        int seconds = Mathf.FloorToInt(currentSurvivalTime % 60F);

        timerText.text = $"TIME  {minutes:00}:{seconds:00}";
    }

    private void CheckMilestones()
    {
        // 1. Mốc 19:30: FINAL WAVE
        if (currentSurvivalTime >= finalWaveSeconds && !isFinalWaveActive && !isBossActive)
        {
            isFinalWaveActive = true;
            if (waveWarningText != null)
            {
                waveWarningText.gameObject.SetActive(true);
                waveWarningText.text = "⚠ FINAL WAVE ⚠";
                waveWarningText.color = new Color(1f, 0.25f, 0.25f, 1f);
            }
        }

        // 2. Mốc 20:00: BOSS XUẤT HIỆN
        if (currentSurvivalTime >= targetSurvivalSeconds && !isBossActive)
        {
            isBossActive = true;
            if (waveWarningText != null)
            {
                waveWarningText.gameObject.SetActive(true);
                waveWarningText.text = "THE ECLIPSE GUARDIAN HAS AWAKENED!";
                waveWarningText.color = new Color(0.95f, 0.15f, 0.15f, 1f);
            }
        }
    }

    // =========================================================================
    // PHÍM TẮT DEBUG DÙNG NEW INPUT SYSTEM
    // =========================================================================

    private void HandleDebugHotkeys()
    {
        if (Keyboard.current == null) return;

        // Phím T: Tua nhanh thời gian
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (currentSurvivalTime < finalWaveSeconds - 5f)
            {
                currentSurvivalTime = finalWaveSeconds - 3f; // Nhảy ngay tới trước 19:30
                GameSessionData.SurvivalTime = currentSurvivalTime;
                Debug.Log("Debug: Đã nhảy tới 19:27 (chuẩn bị Final Wave)");
            }
            else if (currentSurvivalTime < targetSurvivalSeconds - 5f)
            {
                currentSurvivalTime = targetSurvivalSeconds - 3f; // Nhảy ngay tới trước 20:00
                GameSessionData.SurvivalTime = currentSurvivalTime;
                Debug.Log("Debug: Đã nhảy tới 19:57 (chuẩn bị Boss)");
            }
            else
            {
                currentSurvivalTime = targetSurvivalSeconds; // Hoàn thành 20:00 kích hoạt Chiến thắng
                GameSessionData.SurvivalTime = currentSurvivalTime;
                Debug.Log("Debug: Đã nhảy tới 20:00 (Chiến thắng)");
            }
        }

        // Phím H: Trừ 1 tim (Test nhận sát thương)
        if (Keyboard.current.hKey.wasPressedThisFrame && playerStatus != null)
        {
            playerStatus.TakeDamage(1);
        }

        // Phím J: Hồi 1 tim (Test hồi phục)
        if (Keyboard.current.jKey.wasPressedThisFrame && playerStatus != null)
        {
            playerStatus.Heal(1);
        }

        // Phím X: Nhận 35 EXP (Test thanh XP và thăng cấp)
        if (Keyboard.current.xKey.wasPressedThisFrame && playerStatus != null)
        {
            playerStatus.GainExperience(35);
        }
    }
}
