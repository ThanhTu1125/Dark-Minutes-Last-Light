using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HubManager : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] public string gamePlaySceneName = "GameScene";
    [SerializeField] public string mainMenuSceneName = "MainMenu";

    [Header("Character UI")]
    [SerializeField] public UICharacterAnimator characterAnimator;
    [SerializeField] public Image characterPreviewImage;
    [SerializeField] public TextMeshProUGUI characterNameText;
    [SerializeField] public TextMeshProUGUI characterDescText;
    [SerializeField] public Button maleButton;
    [SerializeField] public Button femaleButton;

    [Header("Weapon UI")]
    [SerializeField] public Image weaponPreviewImage;
    [SerializeField] public TextMeshProUGUI weaponNameText;
    [SerializeField] public TextMeshProUGUI weaponStatsText;
    [SerializeField] public Sprite weapon1Sprite; // item_outdated1 (Kiếm Ánh Sáng)
    [SerializeField] public Sprite weapon2Sprite; // item_outdated2 (Rìu Huyết Thần)
    [SerializeField] public Sprite weapon3Sprite; // item_outdated56 (Trượng Ma Năng)
    [SerializeField] public Button weapon1Button;
    [SerializeField] public Button weapon2Button;
    [SerializeField] public Button weapon3Button;

    // Giữ tương thích với tên biến cũ nếu có code gọi
    public Button pistolButton { get => weapon1Button; set => weapon1Button = value; }
    public Button shotgunButton { get => weapon2Button; set => weapon2Button = value; }
    public Button arcRifleButton { get => weapon3Button; set => weapon3Button = value; }

    [Header("Color Highlights")]
    [SerializeField] public Color selectedColor = new Color(1f, 0.85f, 0.3f, 1f); // Vàng sáng
    [SerializeField] public Color normalColor = Color.white;

    private void Start()
    {
        // Gán sự kiện click nếu có
        if (maleButton != null) maleButton.onClick.AddListener(() => SelectCharacter(0));
        if (femaleButton != null) femaleButton.onClick.AddListener(() => SelectCharacter(1));

        if (weapon1Button != null) weapon1Button.onClick.AddListener(() => SelectWeapon(0));
        if (weapon2Button != null) weapon2Button.onClick.AddListener(() => SelectWeapon(1));
        if (weapon3Button != null) weapon3Button.onClick.AddListener(() => SelectWeapon(2));

        // Khởi tạo lựa chọn ban đầu
        SelectCharacter((int)GameSessionData.SelectedCharacter);
        SelectWeapon((int)GameSessionData.SelectedWeapon);
    }

    public void SelectCharacter(int characterIndex)
    {
        GameSessionData.SelectedCharacter = (CharacterType)characterIndex;

        if (characterAnimator != null)
        {
            characterAnimator.SetCharacter(GameSessionData.SelectedCharacter);
        }

        if (GameSessionData.SelectedCharacter == CharacterType.Male)
        {
            if (characterNameText != null) characterNameText.text = "AIDEN (CHIẾN BINH NAM)";
            if (characterDescText != null)
            {
                characterDescText.text = "<b>Nhà thám hiểm kiên cường của tổ chức A.R.K.</b>\n" +
                                         "• Chỉ số cơ bản: Máu 100 | Năng lượng 100 | Tốc độ 10\n" +
                                         "• Kỹ năng Dash: Lướt nhanh với thời gian bất tử (i-frame)\n" +
                                         "• Sở trường: Cận chiến linh hoạt, thích nghi môi trường khắc nghiệt.";
            }

            HighlightButton(maleButton, true);
            HighlightButton(femaleButton, false);
        }
        else
        {
            if (characterNameText != null) characterNameText.text = "ELENA (NỮ ĐIỆP VIÊN)";
            if (characterDescText != null)
            {
                characterDescText.text = "<b>Đặc vụ bóng đêm tinh nhuệ của biệt đội A.R.K.</b>\n" +
                                         "• Chỉ số cơ bản: Máu 90 | Năng lượng 120 | Tốc độ 11\n" +
                                         "• Kỹ năng Dash: Tốc độ lướt cực cao, hồi năng lượng nhanh hơn\n" +
                                         "• Sở trường: Thao túng vũ khí ma năng và thả diều quái vật.";
            }

            HighlightButton(maleButton, false);
            HighlightButton(femaleButton, true);
        }
    }

    public void SelectWeapon(int weaponIndex)
    {
        GameSessionData.SelectedWeapon = (WeaponType)weaponIndex;

        HighlightButton(weapon1Button, false);
        HighlightButton(weapon2Button, false);
        HighlightButton(weapon3Button, false);

        switch (GameSessionData.SelectedWeapon)
        {
            case WeaponType.Blade:
                if (weaponPreviewImage != null && weapon1Sprite != null) weaponPreviewImage.sprite = weapon1Sprite;
                if (weaponNameText != null) weaponNameText.text = "KIẾM ÁNH SÁNG (ECLIPSE BLADE)";
                if (weaponStatsText != null)
                {
                    weaponStatsText.text = "<b>Loại vũ khí:</b> Cận chiến nhanh & Cân bằng\n" +
                                           "• <b>Sát thương:</b> 25 HP / đòn\n" +
                                           "• <b>Tốc độ chém:</b> 0.35s (Nhanh)\n" +
                                           "• <b>Phạm vi:</b> Hình cung 120° phía trước\n" +
                                           "• <b>Đặc tính:</b> Đòn chém xuyên qua 2 quái vật cùng lúc. Dễ sử dụng, thích hợp cho người chơi mới làm quen nhịp độ.";
                }
                HighlightButton(weapon1Button, true);
                break;

            case WeaponType.Axe:
                if (weaponPreviewImage != null && weapon2Sprite != null) weaponPreviewImage.sprite = weapon2Sprite;
                if (weaponNameText != null) weaponNameText.text = "RÌU HUYẾT THẦN (CRIMSON CLEAVER)";
                if (weaponStatsText != null)
                {
                    weaponStatsText.text = "<b>Loại vũ khí:</b> Trọng kích & Càn quét đám đông\n" +
                                           "• <b>Sát thương:</b> 55 HP / đòn (Cực lớn)\n" +
                                           "• <b>Tốc độ chém:</b> 0.85s (Chậm)\n" +
                                           "• <b>Phạm vi:</b> Vòng quét rộng 180° xung quanh\n" +
                                           "• <b>Đặc tính:</b> Lực hất lùi (Knockback) cực mạnh, làm choáng nhẹ kẻ thù. Rất uy lực khi bị bầy quái vật vây hãm.";
                }
                HighlightButton(weapon2Button, true);
                break;

            case WeaponType.Staff:
                if (weaponPreviewImage != null && weapon3Sprite != null) weaponPreviewImage.sprite = weapon3Sprite;
                if (weaponNameText != null) weaponNameText.text = "TRƯỢNG MA NĂNG (VOID SCEPTER)";
                if (weaponStatsText != null)
                {
                    weaponStatsText.text = "<b>Loại vũ khí:</b> Phép thuật tầm xa & Thả diều (Kite)\n" +
                                           "• <b>Sát thương:</b> 35 HP / tia ma năng\n" +
                                           "• <b>Tốc độ bắn:</b> 0.60s (Trung bình)\n" +
                                           "• <b>Phạm vi:</b> Rất xa (Toàn màn hình)\n" +
                                           "• <b>Đặc tính:</b> Bắn ra tia năng lượng hắc ám tự động tìm đến mục tiêu gần nhất. Giúp giữ khoảng cách an toàn tuyệt đối.";
                }
                HighlightButton(weapon3Button, true);
                break;
        }
    }

    private void HighlightButton(Button btn, bool isSelected)
    {
        if (btn == null) return;
        ColorBlock cb = btn.colors;
        cb.normalColor = isSelected ? selectedColor : normalColor;
        cb.selectedColor = isSelected ? selectedColor : normalColor;
        btn.colors = cb;
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        Debug.Log($"Vào trận sinh tồn: Nhân vật = {GameSessionData.SelectedCharacter}, Vũ khí = {GameSessionData.SelectedWeapon}");
        SceneManager.LoadScene(gamePlaySceneName);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
