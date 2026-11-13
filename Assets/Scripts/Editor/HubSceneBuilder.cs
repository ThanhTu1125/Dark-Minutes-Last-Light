using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using TMPro;

public static class HubSceneBuilder
{
    private const string HUB_SCENE_PATH = "Assets/Scenes/Hub.unity";
    private const string MALE_SPRITE_PATH = "Assets/Art/Sprites/The Male adventurer - Free/The Male adventurer - Free/Idle/Idle_down.png";
    private const string FEMALE_SPRITE_PATH = "Assets/The Female Adventurer - Free/The Female Adventurer - Free/Idle/Idle_down.png";
    private const string BG_IMAGE_PATH = "Assets/Art/UI/imagegame.png";

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

        // 2. Xóa các đối tượng UI cũ nếu đã tồn tại để tạo mới hoàn toàn sạch sẽ
        GameObject oldCanvas = GameObject.Find("Canvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

        GameObject oldEventSystem = GameObject.Find("EventSystem");
        if (oldEventSystem != null) Object.DestroyImmediate(oldEventSystem);

        GameObject oldController = GameObject.Find("HubController");
        if (oldController != null) Object.DestroyImmediate(oldController);

        // Đảm bảo có Camera
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

        // 4. Tạo Canvas & Scaler
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

        // Lấy font TMP mặc định
        TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;

        // 6. Tạo Background (Nền)
        GameObject bgGo = CreateUIElement("Background", canvasGo.transform);
        SetFullScreenStretch(bgGo.GetComponent<RectTransform>());
        Image bgImg = bgGo.AddComponent<Image>();
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BG_IMAGE_PATH);
        if (bgSprite != null)
        {
            bgImg.sprite = bgSprite;
            bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f); // Màu tối huyền bí
        }
        else
        {
            bgImg.color = new Color(0.07f, 0.07f, 0.1f, 1f);
        }

        // Tạo Dark Overlay mờ
        GameObject overlayGo = CreateUIElement("DarkOverlay", canvasGo.transform);
        SetFullScreenStretch(overlayGo.GetComponent<RectTransform>());
        Image overlayImg = overlayGo.AddComponent<Image>();
        overlayImg.color = new Color(0.03f, 0.03f, 0.05f, 0.7f);

        // 7. Tạo Header (Tiêu đề trên cùng)
        GameObject headerGo = CreateText("HeaderTitle", canvasGo.transform, "A.R.K PREPARATION DOCK", 44, FontStyles.Bold, new Color(1f, 0.82f, 0.25f, 1f), defaultFont);
        RectTransform headerRt = headerGo.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0.5f, 1f);
        headerRt.anchorMax = new Vector2(0.5f, 1f);
        headerRt.pivot = new Vector2(0.5f, 1f);
        headerRt.anchoredPosition = new Vector2(0, -40);
        headerRt.sizeDelta = new Vector2(1200, 60);

        GameObject subHeaderGo = CreateText("HeaderSubtitle", canvasGo.transform, "CHUẨN BỊ CHIẾN BINH VÀ TRANG BỊ TRƯỚC KHI TIẾN VÀO KHU RỪNG ELDERVALE (00:20:00)", 18, FontStyles.Normal, new Color(0.8f, 0.8f, 0.8f, 1f), defaultFont);
        RectTransform subHeaderRt = subHeaderGo.GetComponent<RectTransform>();
        subHeaderRt.anchorMin = new Vector2(0.5f, 1f);
        subHeaderRt.anchorMax = new Vector2(0.5f, 1f);
        subHeaderRt.pivot = new Vector2(0.5f, 1f);
        subHeaderRt.anchoredPosition = new Vector2(0, -100);
        subHeaderRt.sizeDelta = new Vector2(1200, 40);

        // =========================================================================
        // 8. BÊN TRÁI: PANEL CHỌN NHÂN VẬT
        // =========================================================================
        GameObject charPanel = CreatePanel("CharacterSelectionPanel", canvasGo.transform, new Vector2(-420, -15), new Vector2(760, 720), new Color(0.1f, 0.11f, 0.15f, 0.92f));

        // Tiêu đề Panel Nhân vật
        GameObject charTitle = CreateText("Title", charPanel.transform, "1. CHỌN CHIẾN BINH", 28, FontStyles.Bold, new Color(1f, 0.85f, 0.3f, 1f), defaultFont);
        SetAnchored(charTitle.GetComponent<RectTransform>(), new Vector2(0, 310), new Vector2(600, 50));

        // Nút Nam (Aiden) & Nữ (Elena)
        Button maleBtn = CreateButton("MaleButton", charPanel.transform, "AIDEN (NAM)", new Vector2(-150, 240), new Vector2(260, 55), defaultFont);
        Button femaleBtn = CreateButton("FemaleButton", charPanel.transform, "ELENA (NỮ)", new Vector2(150, 240), new Vector2(260, 55), defaultFont);

        // Khung Preview Nhân vật
        GameObject previewBox = CreatePanel("PreviewBox", charPanel.transform, new Vector2(0, 70), new Vector2(240, 260), new Color(0.06f, 0.07f, 0.09f, 1f));
        GameObject previewImgGo = CreateUIElement("CharacterImage", previewBox.transform);
        Image previewImg = previewImgGo.AddComponent<Image>();
        previewImg.preserveAspect = true;
        SetAnchored(previewImgGo.GetComponent<RectTransform>(), Vector2.zero, new Vector2(220, 240));

        // Tên và mô tả nhân vật
        GameObject charNameGo = CreateText("CharacterNameText", charPanel.transform, "AIDEN (CHIẾN BINH NAM)", 24, FontStyles.Bold, new Color(1f, 0.85f, 0.3f, 1f), defaultFont);
        SetAnchored(charNameGo.GetComponent<RectTransform>(), new Vector2(0, -95), new Vector2(680, 40));

        GameObject charDescGo = CreateText("CharacterDescText", charPanel.transform, "Thông tin mô tả...", 19, FontStyles.Normal, new Color(0.9f, 0.9f, 0.9f, 1f), defaultFont);
        TextMeshProUGUI charDescTmp = charDescGo.GetComponent<TextMeshProUGUI>();
        charDescTmp.alignment = TextAlignmentOptions.Top;
        SetAnchored(charDescGo.GetComponent<RectTransform>(), new Vector2(0, -210), new Vector2(680, 160));

        // =========================================================================
        // 9. BÊN PHẢI: PANEL CHỌN VŨ KHÍ
        // =========================================================================
        GameObject wepPanel = CreatePanel("WeaponSelectionPanel", canvasGo.transform, new Vector2(420, -15), new Vector2(760, 720), new Color(0.1f, 0.11f, 0.15f, 0.92f));

        // Tiêu đề Panel Vũ khí
        GameObject wepTitle = CreateText("Title", wepPanel.transform, "2. CHỌN VŨ KHÍ XUẤT PHÁT", 28, FontStyles.Bold, new Color(1f, 0.85f, 0.3f, 1f), defaultFont);
        SetAnchored(wepTitle.GetComponent<RectTransform>(), new Vector2(0, 310), new Vector2(600, 50));

        // 3 Nút Vũ khí
        Button pistolBtn = CreateButton("PistolButton", wepPanel.transform, "PISTOL", new Vector2(-220, 240), new Vector2(200, 55), defaultFont);
        Button shotgunBtn = CreateButton("ShotgunButton", wepPanel.transform, "SHOTGUN", new Vector2(0, 240), new Vector2(200, 55), defaultFont);
        Button arcRifleBtn = CreateButton("ArcRifleButton", wepPanel.transform, "ARC RIFLE", new Vector2(220, 240), new Vector2(200, 55), defaultFont);

        // Khung chi tiết vũ khí
        GameObject wepBox = CreatePanel("WeaponDetailBox", wepPanel.transform, new Vector2(0, 10), new Vector2(680, 370), new Color(0.06f, 0.07f, 0.09f, 1f));

        GameObject wepNameGo = CreateText("WeaponNameText", wepBox.transform, "VŨ KHÍ: PISTOL CƠ BẢN", 24, FontStyles.Bold, new Color(1f, 0.85f, 0.3f, 1f), defaultFont);
        SetAnchored(wepNameGo.GetComponent<RectTransform>(), new Vector2(0, 135), new Vector2(640, 40));

        GameObject wepStatsGo = CreateText("WeaponStatsText", wepBox.transform, "Thông số vũ khí...", 21, FontStyles.Normal, new Color(0.92f, 0.92f, 0.92f, 1f), defaultFont);
        TextMeshProUGUI wepStatsTmp = wepStatsGo.GetComponent<TextMeshProUGUI>();
        wepStatsTmp.alignment = TextAlignmentOptions.TopLeft;
        wepStatsTmp.lineSpacing = 15;
        SetAnchored(wepStatsGo.GetComponent<RectTransform>(), new Vector2(0, -35), new Vector2(620, 250));

        // Ghi chú cơ chế
        GameObject noteGo = CreateText("TacticalNote", wepPanel.transform, "* Gợi ý: Mỗi vũ khí định hình lối chơi riêng. Vũ khí có thể tiếp tục được nâng cấp trong trận sinh tồn.", 16, FontStyles.Italic, new Color(0.7f, 0.7f, 0.7f, 1f), defaultFont);
        SetAnchored(noteGo.GetComponent<RectTransform>(), new Vector2(0, -260), new Vector2(680, 50));

        // =========================================================================
        // 10. PHÍA DƯỚI: NÚT BACK VÀ NÚT START/DEPLOY
        // =========================================================================
        Button backBtn = CreateButton("BackButton", canvasGo.transform, "< QUAY LẠI MENU", new Vector2(-420, -460), new Vector2(300, 65), defaultFont);
        backBtn.GetComponent<Image>().color = new Color(0.28f, 0.28f, 0.35f, 1f);

        Button deployBtn = CreateButton("DeployButton", canvasGo.transform, "VÀO TRẬN (DEPLOY) >", new Vector2(420, -460), new Vector2(450, 75), defaultFont);
        deployBtn.GetComponent<Image>().color = new Color(0.75f, 0.16f, 0.16f, 1f); // Đỏ huyết sắc nổi bật
        deployBtn.GetComponentInChildren<TextMeshProUGUI>().fontSize = 26;

        // =========================================================================
        // 11. GÁN THAM CHIẾU VÀO HUB MANAGER
        // =========================================================================
        hubManager.characterPreviewImage = previewImg;
        hubManager.characterNameText = charNameGo.GetComponent<TextMeshProUGUI>();
        hubManager.characterDescText = charDescTmp;

        hubManager.malePreviewSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MALE_SPRITE_PATH);
        hubManager.femalePreviewSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FEMALE_SPRITE_PATH);

        hubManager.maleButton = maleBtn;
        hubManager.femaleButton = femaleBtn;

        hubManager.weaponNameText = wepNameGo.GetComponent<TextMeshProUGUI>();
        hubManager.weaponStatsText = wepStatsTmp;

        hubManager.pistolButton = pistolBtn;
        hubManager.shotgunButton = shotgunBtn;
        hubManager.arcRifleButton = arcRifleBtn;

        // Gán sự kiện cho nút Back và Deploy
        UnityEventTools.AddPersistentListener(backBtn.onClick, hubManager.BackToMainMenu);
        UnityEventTools.AddPersistentListener(deployBtn.onClick, hubManager.StartGame);

        // Lưu scene
        EditorUtility.SetDirty(controllerGo);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Thành công!", 
            "Đã tự động dựng xong toàn bộ giao diện cho Scene Hub!\n\n" +
            "• Đã kết nối Nhân vật Nam (Aiden) & Nữ (Elena)\n" +
            "• Đã cấu hình 3 loại Vũ khí (Pistol, Shotgun, Arc Rifle)\n" +
            "• Đã nối các sự kiện chuyển Scene với GameSessionData", "Tuyệt vời!");
    }

    // =========================================================================
    // HÀM TIỆN ÍCH TẠO UI NHANH
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

        // Label bên trong nút
        GameObject textGo = CreateUIElement("Label", btnGo.transform);
        SetFullScreenStretch(textGo.GetComponent<RectTransform>());
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;

        return btn;
    }
}
