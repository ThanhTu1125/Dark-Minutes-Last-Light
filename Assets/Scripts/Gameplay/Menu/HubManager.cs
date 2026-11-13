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
    [SerializeField] public Image characterPreviewImage;
    [SerializeField] public TextMeshProUGUI characterNameText;
    [SerializeField] public TextMeshProUGUI characterDescText;
    [SerializeField] public Sprite malePreviewSprite;
    [SerializeField] public Sprite femalePreviewSprite;
    [SerializeField] public Button maleButton;
    [SerializeField] public Button femaleButton;

    [Header("Weapon UI")]
    [SerializeField] public TextMeshProUGUI weaponNameText;
    [SerializeField] public TextMeshProUGUI weaponStatsText;
    [SerializeField] public Button pistolButton;
    [SerializeField] public Button shotgunButton;
    [SerializeField] public Button arcRifleButton;

    [Header("Color Highlights")]
    [SerializeField] public Color selectedColor = new Color(1f, 0.85f, 0.3f, 1f); // Vàng sáng
    [SerializeField] public Color normalColor = Color.white;

    private void Start()
    {
        // Gán sự kiện cho các nút nếu có
        if (maleButton != null) maleButton.onClick.AddListener(() => SelectCharacter(0));
        if (femaleButton != null) femaleButton.onClick.AddListener(() => SelectCharacter(1));

        if (pistolButton != null) pistolButton.onClick.AddListener(() => SelectWeapon(0));
        if (shotgunButton != null) shotgunButton.onClick.AddListener(() => SelectWeapon(1));
        if (arcRifleButton != null) arcRifleButton.onClick.AddListener(() => SelectWeapon(2));

        // Khởi tạo lựa chọn ban đầu
        SelectCharacter((int)GameSessionData.SelectedCharacter);
        SelectWeapon((int)GameSessionData.SelectedWeapon);
    }

    public void SelectCharacter(int characterIndex)
    {
        GameSessionData.SelectedCharacter = (CharacterType)characterIndex;

        if (GameSessionData.SelectedCharacter == CharacterType.Male)
        {
            if (characterNameText != null) characterNameText.text = "AIDEN (CHIẾN BINH NAM)";
            if (characterDescText != null) 
                characterDescText.text = "Nhà thám hiểm kiên cường của tổ chức A.R.K.\n- Máu cơ bản: 100\n- Năng lượng: 100\n- Thể lực và tốc độ ổn định trong đêm tối.";
            if (characterPreviewImage != null && malePreviewSprite != null) 
                characterPreviewImage.sprite = malePreviewSprite;

            HighlightButton(maleButton, true);
            HighlightButton(femaleButton, false);
        }
        else
        {
            if (characterNameText != null) characterNameText.text = "ELENA (NỮ ĐIỆP VIÊN)";
            if (characterDescText != null) 
                characterDescText.text = "Thành viên tinh nhuệ của đội đặc nhiệm A.R.K.\n- Nhanh nhẹn, phản xạ né tránh cao\n- Khả năng hồi phục năng lượng lướt nhanh hơn.";
            if (characterPreviewImage != null && femalePreviewSprite != null) 
                characterPreviewImage.sprite = femalePreviewSprite;

            HighlightButton(maleButton, false);
            HighlightButton(femaleButton, true);
        }
    }

    public void SelectWeapon(int weaponIndex)
    {
        GameSessionData.SelectedWeapon = (WeaponType)weaponIndex;

        HighlightButton(pistolButton, false);
        HighlightButton(shotgunButton, false);
        HighlightButton(arcRifleButton, false);

        switch (GameSessionData.SelectedWeapon)
        {
            case WeaponType.Pistol:
                if (weaponNameText != null) weaponNameText.text = "VŨ KHÍ: PISTOL CƠ BẢN";
                if (weaponStatsText != null)
                    weaponStatsText.text = "• Sát thương: 10 HP\n• Số đạn: 1 viên/lần bắn\n• Tốc độ bắn: Trung bình (0.35s)\n• Tầm bắn: Trung bình\n• Đánh giá: Cân bằng, dễ sử dụng cho tân thủ.";
                HighlightButton(pistolButton, true);
                break;

            case WeaponType.Shotgun:
                if (weaponNameText != null) weaponNameText.text = "VŨ KHÍ: SHOTGUN ĐỘT PHÁ";
                if (weaponStatsText != null)
                    weaponStatsText.text = "• Sát thương: 8 x 5 viên (40 HP tối đa)\n• Số đạn: 5 viên tỏa chùm\n• Tốc độ bắn: Chậm (0.8s)\n• Tầm bắn: Ngắn, góc quét rộng\n• Đánh giá: Cực mạnh ở cự ly gần và dọn đám đông.";
                HighlightButton(shotgunButton, true);
                break;

            case WeaponType.ArcRifle:
                if (weaponNameText != null) weaponNameText.text = "VŨ KHÍ: ARC RIFLE NĂNG LƯỢNG";
                if (weaponStatsText != null)
                    weaponStatsText.text = "• Sát thương: 25 HP\n• Số đạn: 1 tia năng lượng xuyên thấu\n• Tốc độ bắn: Rất chậm (1.2s)\n• Tầm bắn: Rất xa\n• Đánh giá: Sát thương lớn, thích hợp bắn tỉa quái lớn & Boss.";
                HighlightButton(arcRifleButton, true);
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
        Debug.Log($"Bắt đầu trận với: Nhân vật = {GameSessionData.SelectedCharacter}, Vũ khí = {GameSessionData.SelectedWeapon}");
        SceneManager.LoadScene(gamePlaySceneName);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
