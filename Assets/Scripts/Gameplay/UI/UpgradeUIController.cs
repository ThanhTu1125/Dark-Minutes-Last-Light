using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class UpgradeUIController : MonoBehaviour
{
    public enum UpgradeType
    {
        PowerShot,      // Tăng sát thương (+15)
        RapidFire,      // Tăng tốc độ tấn công (giảm 20% hồi chiêu)
        PiercingEdge,   // Xuyên thấu thêm +1 mục tiêu
        SwiftBoots,     // Tăng tốc chạy (+25%)
        Vitality,       // Tăng máu tối đa (+50) và hồi đầy máu
        HyperVelocity   // Tăng tốc độ bay của đạn (+30%) và đẩy lùi
    }

    [System.Serializable]
    public class UpgradeDefinition
    {
        public UpgradeType type;
        public string title;
        [TextArea(2, 3)]
        public string description;
        public Sprite icon;
        public Color accentColor = Color.white;
    }

    [System.Serializable]
    public class UpgradeCardUI
    {
        public GameObject cardRoot;
        public Image borderImage;
        public Image iconImage;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descText;
        public Button selectButton;
    }

    [Header("Tham chiếu Component")]
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI levelTitleText;
    [SerializeField] private UpgradeCardUI[] cards = new UpgradeCardUI[3];

    [Header("Danh mục Nâng cấp")]
    public List<UpgradeDefinition> upgradePool = new List<UpgradeDefinition>();

    private int pendingLevelUps = 0;
    private List<UpgradeDefinition> currentDisplayedUpgrades = new List<UpgradeDefinition>();
    private bool isPanelOpen = false;

    public bool IsPanelOpen => isPanelOpen;

    private void Awake()
    {
        if (playerStatus == null) playerStatus = FindFirstObjectByType<PlayerStatus>();
        if (weaponController == null) weaponController = FindFirstObjectByType<WeaponController>();
    }

    private void Start()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        // Đăng ký sự kiện lên cấp từ PlayerStatus
        if (playerStatus != null)
        {
            playerStatus.OnLevelUp += HandleLevelUp;
        }

        // Gán sự kiện click cho 3 thẻ bài
        for (int i = 0; i < cards.Length; i++)
        {
            int index = i;
            if (cards[i].selectButton != null)
            {
                cards[i].selectButton.onClick.AddListener(() => OnCardSelected(index));
            }
        }
    }

    private void OnDestroy()
    {
        if (playerStatus != null)
        {
            playerStatus.OnLevelUp -= HandleLevelUp;
        }
    }

    private void Update()
    {
        // Phím nóng U để mở thử màn hình nâng cấp ngay lập tức
        if (Keyboard.current != null && Keyboard.current.uKey.wasPressedThisFrame)
        {
            if (!isPanelOpen)
            {
                Debug.Log("<color=cyan>[DEBUG]</color> Mở thử Màn hình Nâng Cấp (Phím U)");
                pendingLevelUps++;
                ShowNextUpgrade(playerStatus != null ? playerStatus.currentLevel : 2);
            }
        }
    }

    public void HandleLevelUp(int newLevel)
    {
        pendingLevelUps++;

        if (!isPanelOpen)
        {
            ShowNextUpgrade(newLevel);
        }
    }

    private void ShowNextUpgrade(int level)
    {
        if (panelRoot == null || upgradePool == null || upgradePool.Count == 0) return;

        isPanelOpen = true;

        // Tạm dừng trò chơi (Freeze gameplay)
        Time.timeScale = 0f;

        if (levelTitleText != null)
        {
            levelTitleText.text = $"LEVEL UP! (CẤP {level})";
        }

        // Chọn ngẫu nhiên 3 thẻ bài không trùng lặp
        List<UpgradeDefinition> available = new List<UpgradeDefinition>(upgradePool);
        currentDisplayedUpgrades.Clear();

        for (int i = 0; i < 3 && available.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, available.Count);
            currentDisplayedUpgrades.Add(available[randomIndex]);
            available.RemoveAt(randomIndex);
        }

        // Hiển thị dữ liệu lên 3 thẻ bài
        for (int i = 0; i < cards.Length; i++)
        {
            if (i < currentDisplayedUpgrades.Count)
            {
                UpgradeDefinition up = currentDisplayedUpgrades[i];
                cards[i].cardRoot.SetActive(true);

                if (cards[i].titleText != null) cards[i].titleText.text = up.title;
                if (cards[i].descText != null) cards[i].descText.text = up.description;
                if (cards[i].iconImage != null && up.icon != null) cards[i].iconImage.sprite = up.icon;
                if (cards[i].borderImage != null) cards[i].borderImage.color = up.accentColor;
            }
            else
            {
                cards[i].cardRoot.SetActive(false);
            }
        }

        panelRoot.SetActive(true);
    }

    public void OnCardSelected(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= currentDisplayedUpgrades.Count) return;

        UpgradeDefinition chosen = currentDisplayedUpgrades[cardIndex];
        ApplyUpgrade(chosen.type);

        Debug.Log($"<color=green>[NÂNG CẤP THÀNH CÔNG]</color> Đã chọn: {chosen.title}");

        pendingLevelUps--;

        if (pendingLevelUps > 0)
        {
            // Nếu còn cấp độ tích lũy chưa chọn, tiếp tục mở thẻ bài cho cấp tiếp theo
            int nextLevel = (playerStatus != null) ? playerStatus.currentLevel : 1;
            ShowNextUpgrade(nextLevel);
        }
        else
        {
            // Đóng bảng và tiếp tục trò chơi
            panelRoot.SetActive(false);
            isPanelOpen = false;
            Time.timeScale = 1f;
        }
    }

    private void ApplyUpgrade(UpgradeType type)
    {
        if (weaponController == null) weaponController = FindFirstObjectByType<WeaponController>();
        if (playerStatus == null) playerStatus = FindFirstObjectByType<PlayerStatus>();

        switch (type)
        {
            case UpgradeType.PowerShot:
                if (weaponController != null)
                {
                    weaponController.AddDamage(15f);
                }
                break;

            case UpgradeType.RapidFire:
                if (weaponController != null)
                {
                    weaponController.ReduceCooldown(0.20f);
                }
                break;

            case UpgradeType.PiercingEdge:
                if (weaponController != null)
                {
                    weaponController.AddPierce(1);
                }
                break;

            case UpgradeType.SwiftBoots:
                if (playerStatus != null)
                {
                    playerStatus.speed += 2.5f;
                }
                break;

            case UpgradeType.Vitality:
                if (playerStatus != null)
                {
                    playerStatus.maxHealth += 1; // Tăng thêm 1 Trái Tim tối đa
                    playerStatus.currentHealth = playerStatus.maxHealth;
                    playerStatus.Heal(1);
                }
                break;

            case UpgradeType.HyperVelocity:
                if (weaponController != null)
                {
                    weaponController.AddProjectileSpeed(5f);
                }
                break;
        }
    }
}
