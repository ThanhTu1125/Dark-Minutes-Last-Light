using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using TMPro;
using System.Linq;

public static class HubSceneBuilder
{
    private const string HUB_SCENE_PATH = "Assets/Scenes/Hub.unity";
    private const string BG_IMAGE_PATH = "Assets/Art/UI/imagegame.png";

    // Sprites bước đi sang phải (8 frames)
    private const string MALE_WALK_TEXTURE_PATH = "Assets/Art/Sprites/The Male adventurer - Free/The Male adventurer - Free/Walk/walk_right_down.png";
    private const string FEMALE_WALK_TEXTURE_PATH = "Assets/The Female Adventurer - Free/The Female Adventurer - Free/Walk/walk_Right_Down.png";

    // 3 Vũ khí được chọn từ items_outdated
    private const string WEAPON_1_PATH = "Assets/items_outdated/item_outdated1.png";  // Kiếm Ánh Sáng
    private const string WEAPON_2_PATH = "Assets/items_outdated/item_outdated2.png";  // Rìu Huyết Thần
    private const string WEAPON_3_PATH = "Assets/items_outdated/item_outdated56.png"; // Trượng Ma Năng

    [MenuItem("Dark Minutes/Setup Hub UI (Tự động dựng Scene Hub)")]
    public static void BuildHubUI()
    {
        // 1. Mở Scene Hub
        if (!System.IO.File.Exists(HUB_SCENE_PATH))
        {
            EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy file scene tại {HUB_SCENE_PATH}!", "OK");
            return;
        }

        var scene = EditorSceneManager.OpenScene(HUB_SCENE_PATH, OpenSceneMode.Single);

        // 2. Dọn dẹp sạch sẽ các GameObject cũ
        GameObject oldCanvas = GameObject.Find("Canvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

        GameObject oldEventSystem = GameObject.Find("EventSystem");
        if (oldEventSystem != null) Object.DestroyImmediate(oldEventSystem);

        GameObject oldController = GameObject.Find("HubController");
        if (oldController != null) Object.DestroyImmediate(oldController);

        // Đảm bảo Camera tồn tại
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            mainCam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.04f, 0.04f, 0.06f, 1f);
            mainCam.orthographic = true;
        }

        // 3. Tạo EventSystem
        GameObject eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // 4. Tạo Canvas & Scaler chuẩn Full HD 1920x1080
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // 5. Tạo HubController
        GameObject controllerGo = new GameObject("HubController");
        HubManager hubManager = controllerGo.AddComponent<HubManager>();

        TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;

        // 6. Background nền & Overlay tối
        GameObject bgGo = CreateUIElement("Background", canvasGo.transform);
        SetFullScreenStretch(bgGo.GetComponent<RectTransform>());
        Image bgImg = bgGo.AddComponent<Image>();
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BG_IMAGE_PATH);
        if (bgSprite != null)
        {
            bgImg.sprite = bgSprite;
            bgImg.color = new Color(0.22f, 0.22f, 0.28f, 1f);
        }
        else
        {
            bgImg.color = new Color(0.07f, 0.07f, 0.1f, 1f);
        }

        GameObject overlayGo = CreateUIElement("DarkOverlay", canvasGo.transform);
        SetFullScreenStretch(overlayGo.GetComponent<RectTransform>());
        Image overlayImg = overlayGo.AddComponent<Image>();
        overlayImg.color = new Color(0.03f, 0.03f, 0.05f, 0.72f);

        // 7. Header tiêu đề
        GameObject headerGo = CreateText("HeaderTitle", canvasGo.transform, "A.R.K TACTICAL DOCK", 46, FontStyles.Bold, new Color(1f, 0.85f, 0.25f, 1f), defaultFont);
        RectTransform headerRt = headerGo.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0.5f, 1f);
        headerRt.anchorMax = new Vector2(0.5f, 1f);
        headerRt.pivot = new Vector2(0.5f, 1f);
        headerRt.anchoredPosition = new Vector2(0, -35);
        headerRt.sizeDelta = new Vector2(1200, 55);

        GameObject subHeaderGo = CreateText("HeaderSubtitle", canvasGo.transform, "LỰA CHỌN CHIẾN BINH VÀ VŨ KHÍ TRƯỚC KHI TIẾN VÀO KHU RỪNG ELDERVALE (00:20:00)", 17, FontStyles.Normal, new Color(0.85f, 0.85f, 0.85f, 1f), defaultFont);
        RectTransform subHeaderRt = subHeaderGo.GetComponent<RectTransform>();
        subHeaderRt.anchorMin = new Vector2(0.5f, 1f);
        subHeaderRt.anchorMax = new Vector2(0.5f, 1f);
        subHeaderRt.pivot = new Vector2(0.5f, 1f);
        subHeaderRt.anchoredPosition = new Vector2(0, -92);
        subHeaderRt.sizeDelta = new Vector2(1200, 35);

        // =========================================================================
        // 8. BÊN TRÁI: PANEL CHỌN CHIẾN BINH (CÓ HOẠT ẢNH BƯỚC ĐI TẠI CHỖ)
        // =========================================================================
        GameObject charPanel = CreatePanel("CharacterSelectionPanel", canvasGo.transform, new Vector2(-430, -15), new Vector2(740, 720), new Color(0.1f, 0.11f, 0.15f, 0.94f));

        GameObject charTitle = CreateText("Title", charPanel.transform, "1. CHỌN CHIẾN BINH", 27, FontStyles.Bold, new Color(1f, 0.85f, 0.3f, 1f), defaultFont);
        SetAnchored(charTitle.GetComponent<RectTransform>(), new Vector2(0, 310), new Vector2(600, 45));

        // Nút chọn Nam & Nữ
        Button maleBtn = CreateButton("MaleButton", charPanel.transform, "AIDEN (NAM)", new Vector2(-150, 245), new Vector2(250, 52), defaultFont);
        Button femaleBtn = CreateButton("FemaleButton", charPanel.transform, "ELENA (NỮ)", new Vector2(150, 245), new Vector2(250, 52), defaultFont);

        // Khung Preview Nhân vật với Animation bước đi
        GameObject previewBox = CreatePanel("PreviewBox", charPanel.transform, new Vector2(0, 75), new Vector2(240, 240), new Color(0.06f, 0.07f, 0.09f, 1f));
        GameObject previewImgGo = CreateUIElement("CharacterAnimatedImage", previewBox.transform);
        Image previewImg = previewImgGo.AddComponent<Image>();
        previewImg.preserveAspect = true;
        SetAnchored(previewImgGo.GetComponent<RectTransform>(), Vector2.zero, new Vector2(200, 200));

        // Gắn component Animation bước đi
        UICharacterAnimator charAnim = previewImgGo.AddComponent<UICharacterAnimator>();

        // Load 8 frames bước đi cho Nam và Nữ
        Sprite[] maleFrames = AssetDatabase.LoadAllAssetsAtPath(MALE_WALK_TEXTURE_PATH)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();

        Sprite[] femaleFrames = AssetDatabase.LoadAllAssetsAtPath(FEMALE_WALK_TEXTURE_PATH)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();

        charAnim.maleWalkFrames = maleFrames;
        charAnim.femaleWalkFrames = femaleFrames;

        // Tên và mô tả nhân vật
        GameObject charNameGo = CreateText("CharacterNameText", charPanel.transform, "AIDEN (CHIẾN BINH NAM)", 23, FontStyles.Bold, new Color(1f, 0.85f, 0.3f, 1f), defaultFont);
        SetAnchored(charNameGo.GetComponent<RectTransform>(), new Vector2(0, -90), new Vector2(680, 40));

        GameObject charDescGo = CreateText("CharacterDescText", charPanel.transform, "Thông tin nhân vật...", 18, FontStyles.Normal, new Color(0.9f, 0.9f, 0.9f, 1f), defaultFont);
        TextMeshProUGUI charDescTmp = charDescGo.GetComponent<TextMeshProUGUI>();
        charDescTmp.alignment = TextAlignmentOptions.TopLeft;
        charDescTmp.lineSpacing = 12;
        SetAnchored(charDescGo.GetComponent<RectTransform>(), new Vector2(0, -210), new Vector2(660, 160));

        // =========================================================================
        // 9. BÊN PHẢI: PANEL CHỌN VŨ KHÍ CỔ ĐẠI (TỪ items_outdated)
        // =========================================================================
        GameObject wepPanel = CreatePanel("WeaponSelectionPanel", canvasGo.transform, new Vector2(430, -15), new Vector2(740, 720), new Color(0.1f, 0.11f, 0.15f, 0.94f));

        GameObject wepTitle = CreateText("Title", wepPanel.transform, "2. KHO VŨ KHÍ CỔ ĐẠI", 27, FontStyles.Bold, new Color(1f, 0.85f, 0.3f, 1f), defaultFont);
        SetAnchored(wepTitle.GetComponent<RectTransform>(), new Vector2(0, 310), new Vector2(600, 45));

        // 3 Nút chọn vũ khí
        Button wep1Btn = CreateButton("Weapon1Button", wepPanel.transform, "KIẾM ÁNH SÁNG", new Vector2(-220, 245), new Vector2(200, 52), defaultFont);
        Button wep2Btn = CreateButton("Weapon2Button", wepPanel.transform, "RÌU HUYẾT THẦN", new Vector2(0, 245), new Vector2(200, 52), defaultFont);
        Button wep3Btn = CreateButton("Weapon3Button", wepPanel.transform, "TRƯỢNG MA NĂNG", new Vector2(220, 245), new Vector2(200, 52), defaultFont);

        // Khung chi tiết vũ khí
        GameObject wepBox = CreatePanel("WeaponDetailBox", wepPanel.transform, new Vector2(0, 5), new Vector2(680, 380), new Color(0.06f, 0.07f, 0.09f, 1f));

        // Khung Icon xem trước của vũ khí
        GameObject wepIconBox = CreatePanel("WeaponIconBox", wepBox.transform, new Vector2(-230, 70), new Vector2(130, 130), new Color(0.1f, 0.12f, 0.18f, 1f));
        GameObject wepIconGo = CreateUIElement("WeaponIcon", wepIconBox.transform);
        Image wepIconImg = wepIconGo.AddComponent<Image>();
        wepIconImg.preserveAspect = true;
        SetAnchored(wepIconGo.GetComponent<RectTransform>(), Vector2.zero, new Vector2(100, 100));

        // Tên và thông số vũ khí
        GameObject wepNameGo = CreateText("WeaponNameText", wepBox.transform, "KIẾM ÁNH SÁNG (ECLIPSE BLADE)", 23, FontStyles.Bold, new Color(1f, 0.85f, 0.3f, 1f), defaultFont);
        TextMeshProUGUI wepNameTmp = wepNameGo.GetComponent<TextMeshProUGUI>();
        wepNameTmp.alignment = TextAlignmentOptions.Left;
        SetAnchored(wepNameGo.GetComponent<RectTransform>(), new Vector2(80, 110), new Vector2(440, 40));

        GameObject wepStatsGo = CreateText("WeaponStatsText", wepBox.transform, "Thông số vũ khí...", 19, FontStyles.Normal, new Color(0.92f, 0.92f, 0.92f, 1f), defaultFont);
        TextMeshProUGUI wepStatsTmp = wepStatsGo.GetComponent<TextMeshProUGUI>();
        wepStatsTmp.alignment = TextAlignmentOptions.TopLeft;
        wepStatsTmp.lineSpacing = 12;
        SetAnchored(wepStatsGo.GetComponent<RectTransform>(), new Vector2(0, -70), new Vector2(620, 210));

        // Load 3 sprite vũ khí từ items_outdated
        Sprite wep1Sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WEAPON_1_PATH);
        Sprite wep2Sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WEAPON_2_PATH);
        Sprite wep3Sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WEAPON_3_PATH);

        // Ghi chú cơ chế
        GameObject noteGo = CreateText("TacticalNote", wepPanel.transform, "* Gợi ý: Mỗi loại vũ khí định hình lối chơi độc nhất. Hãy chọn vũ khí phù hợp với phong cách sinh tồn của bạn.", 16, FontStyles.Italic, new Color(0.72f, 0.72f, 0.72f, 1f), defaultFont);
        SetAnchored(noteGo.GetComponent<RectTransform>(), new Vector2(0, -260), new Vector2(680, 50));

        // =========================================================================
        // 10. PHÍA DƯỚI: NÚT BACK VÀ NÚT START/DEPLOY
        // =========================================================================
        Button backBtn = CreateButton("BackButton", canvasGo.transform, "< QUAY LẠI MENU", new Vector2(-430, -460), new Vector2(300, 65), defaultFont);
        backBtn.GetComponent<Image>().color = new Color(0.28f, 0.28f, 0.35f, 1f);

        Button deployBtn = CreateButton("DeployButton", canvasGo.transform, "VÀO TRẬN (DEPLOY) >", new Vector2(430, -460), new Vector2(440, 75), defaultFont);
        deployBtn.GetComponent<Image>().color = new Color(0.75f, 0.16f, 0.16f, 1f);
        deployBtn.GetComponentInChildren<TextMeshProUGUI>().fontSize = 26;

        // =========================================================================
        // 11. GÁN THAM CHIẾU VÀO HUB MANAGER
        // =========================================================================
        hubManager.characterAnimator = charAnim;
        hubManager.characterPreviewImage = previewImg;
        hubManager.characterNameText = charNameGo.GetComponent<TextMeshProUGUI>();
        hubManager.characterDescText = charDescTmp;

        hubManager.maleButton = maleBtn;
        hubManager.femaleButton = femaleBtn;

        hubManager.weaponPreviewImage = wepIconImg;
        hubManager.weaponNameText = wepNameTmp;
        hubManager.weaponStatsText = wepStatsTmp;

        hubManager.weapon1Sprite = wep1Sprite;
        hubManager.weapon2Sprite = wep2Sprite;
        hubManager.weapon3Sprite = wep3Sprite;

        hubManager.weapon1Button = wep1Btn;
        hubManager.weapon2Button = wep2Btn;
        hubManager.weapon3Button = wep3Btn;

        // Gán sự kiện cho nút Back và Deploy
        UnityEventTools.AddPersistentListener(backBtn.onClick, hubManager.BackToMainMenu);
        UnityEventTools.AddPersistentListener(deployBtn.onClick, hubManager.StartGame);

        // Lưu scene
        EditorUtility.SetDirty(controllerGo);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Thành công!", 
            "Đã nâng cấp Scene Hub hoàn tất!\n\n" +
            "• Nhân vật Preview có hoạt ảnh bước đi sang phải tại chỗ\n" +
            "• Đã tích hợp 3 Vũ khí từ items_outdated (Kiếm, Rìu, Trượng) kèm ảnh hiển thị và thông số\n" +
            "• Đã kết nối đầy đủ các sự kiện!", "Tuyệt vời!");
    }

    // =========================================================================
    // HÀM TIỆN ÍCH TẠO UI
    // =========================================================================
    private static GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void SetFullScreenStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetAnchored(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
    {
        GameObject panelGo = CreateUIElement(name, parent);
        SetAnchored(panelGo.GetComponent<RectTransform>(), pos, size);
        Image img = panelGo.AddComponent<Image>();
        img.color = color;
        return panelGo;
    }

    private static GameObject CreateText(string name, Transform parent, string text, float fontSize, FontStyles style, Color color, TMP_FontAsset font)
    {
        GameObject textGo = CreateUIElement(name, parent);
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        return textGo;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 pos, Vector2 size, TMP_FontAsset font)
    {
        GameObject btnGo = CreateUIElement(name, parent);
        SetAnchored(btnGo.GetComponent<RectTransform>(), pos, size);

        Image img = btnGo.AddComponent<Image>();
        img.color = new Color(0.2f, 0.22f, 0.28f, 1f);

        Button btn = btnGo.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.35f, 0.38f, 0.48f, 1f);
        cb.pressedColor = new Color(0.15f, 0.16f, 0.2f, 1f);
        btn.colors = cb;

        GameObject textGo = CreateUIElement("Label", btnGo.transform);
        SetFullScreenStretch(textGo.GetComponent<RectTransform>());
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 19;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;

        return btn;
    }
}
