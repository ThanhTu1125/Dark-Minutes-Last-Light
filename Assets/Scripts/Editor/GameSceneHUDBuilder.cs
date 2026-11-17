using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Linq;

public static class GameSceneHUDBuilder
{
    private const string GAME_SCENE_PATH = "Assets/Scenes/GameScene.unity";

    // Đường dẫn sprite cho hoạt ảnh Aiden
    private const string WALK_DOWN_PATH = "Assets/Art/Sprites/The Male adventurer - Free/The Male adventurer - Free/Walk/walk_down.png";
    private const string WALK_UP_PATH = "Assets/Art/Sprites/The Male adventurer - Free/The Male adventurer - Free/Walk/walk_up.png";
    private const string WALK_SIDE_PATH = "Assets/Art/Sprites/The Male adventurer - Free/The Male adventurer - Free/Walk/walk_right_down.png";
    private const string IDLE_DOWN_PATH = "Assets/Art/Sprites/The Male adventurer - Free/The Male adventurer - Free/Idle/idle_down.png";
    private const string IDLE_SIDE_PATH = "Assets/Art/Sprites/The Male adventurer - Free/The Male adventurer - Free/Idle/idle_right_down.png";

    [MenuItem("Dark Minutes/Setup In-Game HUD (Tự động dựng HUD GameScene)")]
    public static void BuildGameSceneHUD()
    {
        // 1. Mở GameScene
        if (!System.IO.File.Exists(GAME_SCENE_PATH))
        {
            EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy file scene tại {GAME_SCENE_PATH}!", "OK");
            return;
        }

        var scene = EditorSceneManager.OpenScene(GAME_SCENE_PATH, OpenSceneMode.Single);

        // 2. XÓA BỎ 'Player_Walk' (Dummy test object bị lỗi đứng quay vòng 8 hướng tại chỗ)
        GameObject dummyWalk = GameObject.Find("Player_Walk");
        if (dummyWalk != null)
        {
            Object.DestroyImmediate(dummyWalk);
            Debug.Log("Đã xóa bỏ 'Player_Walk' thừa trong scene.");
        }

        // 3. Cấu hình GameObject 'Player' thực sự
        GameObject playerGo = GameObject.Find("Player");
        if (playerGo == null)
        {
            EditorUtility.DisplayDialog("Cảnh báo", "Không tìm thấy GameObject 'Player' trong GameScene!", "OK");
            return;
        }

        playerGo.tag = "Player";

        // Vô hiệu hóa Animator cũ (nguyên nhân gây quay tròn 8 hướng liên tục)
        Animator oldAnim = playerGo.GetComponent<Animator>();
        if (oldAnim != null)
        {
            oldAnim.enabled = false;
        }

        // Cấu hình Rigidbody2D
        Rigidbody2D rb = playerGo.GetComponent<Rigidbody2D>();
        if (rb == null) rb = playerGo.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Tỷ lệ kích thước nhân vật (3.2x tương đương 1 ô gạch bản đồ, chuẩn tỷ lệ Top-down)
        playerGo.transform.localScale = new Vector3(3.2f, 3.2f, 1f);

        SpriteRenderer pSr = playerGo.GetComponent<SpriteRenderer>();
        if (pSr != null)
        {
            pSr.sortingOrder = 10;
        }

        // Cấu hình Collider2D vừa vặn phần chân/thân nhân vật
        CircleCollider2D circleCol = playerGo.GetComponent<CircleCollider2D>();
        if (circleCol == null) circleCol = playerGo.AddComponent<CircleCollider2D>();
        circleCol.radius = 0.15f;
        circleCol.offset = new Vector2(0f, -0.05f);
        circleCol.isTrigger = false;

        // Thêm các Component logic người chơi
        PlayerStatus playerStatus = playerGo.GetComponent<PlayerStatus>();
        if (playerStatus == null) playerStatus = playerGo.AddComponent<PlayerStatus>();
        playerStatus.maxHealth = 3;
        playerStatus.currentHealth = 3;

        PlayerController playerController = playerGo.GetComponent<PlayerController>();
        if (playerController == null) playerController = playerGo.AddComponent<PlayerController>();

        PlayerDash playerDash = playerGo.GetComponent<PlayerDash>();
        if (playerDash == null) playerGo.AddComponent<PlayerDash>();

        // Thêm PlayerAnimation để điều khiển hướng bước đi chuẩn theo WASD
        PlayerAnimation playerAnim = playerGo.GetComponent<PlayerAnimation>();
        if (playerAnim == null) playerAnim = playerGo.AddComponent<PlayerAnimation>();

        // Nạp các Frame hoạt ảnh chuyển động chuẩn theo hướng
        playerAnim.walkDownFrames = LoadSortedSprites(WALK_DOWN_PATH);
        playerAnim.walkUpFrames = LoadSortedSprites(WALK_UP_PATH);
        playerAnim.walkSideFrames = LoadSortedSprites(WALK_SIDE_PATH);
        playerAnim.idleDownFrames = LoadSortedSprites(IDLE_DOWN_PATH);
        playerAnim.idleSideFrames = LoadSortedSprites(IDLE_SIDE_PATH);

        // Thêm YSortOrder để xử lý độ sâu hình ảnh 2D top-down (đứng trước/sau cây cối tự nhiên)
        YSortOrder playerYSort = playerGo.GetComponent<YSortOrder>();
        if (playerYSort == null) playerYSort = playerGo.AddComponent<YSortOrder>();
        playerYSort.IsStatic = false;
        playerYSort.YOffset = -0.3f;

        // 4. CẤU HÌNH HỆ THỐNG VŨ KHÍ & NGẮM BẮN (Weapon Holding & Aiming)
        GameObject bulletPrefab = EnsureBulletPrefab();

        Transform weaponPivot = playerGo.transform.Find("WeaponPivot");
        if (weaponPivot == null)
        {
            GameObject pivotGo = new GameObject("WeaponPivot");
            pivotGo.transform.SetParent(playerGo.transform, false);
            pivotGo.transform.localPosition = new Vector3(0f, -0.04f, 0f);
            pivotGo.transform.localRotation = Quaternion.identity;
            pivotGo.transform.localScale = Vector3.one;
            weaponPivot = pivotGo.transform;
        }
        else
        {
            weaponPivot.localPosition = new Vector3(0f, -0.04f, 0f);
        }

        Transform weaponHolder = weaponPivot.Find("WeaponHolder");
        if (weaponHolder == null)
        {
            GameObject holderGo = new GameObject("WeaponHolder");
            holderGo.transform.SetParent(weaponPivot, false);
            holderGo.transform.localPosition = new Vector3(0.08f, 0f, 0f);
            holderGo.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);
            holderGo.transform.localScale = Vector3.one;
            weaponHolder = holderGo.transform;
        }
        else
        {
            weaponHolder.localPosition = new Vector3(0.08f, 0f, 0f);
            weaponHolder.localRotation = Quaternion.Euler(0f, 0f, -45f);
        }

        SpriteRenderer weaponSr = weaponHolder.GetComponent<SpriteRenderer>();
        if (weaponSr == null) weaponSr = weaponHolder.gameObject.AddComponent<SpriteRenderer>();
        weaponSr.sortingOrder = 11;

        Transform firePoint = weaponHolder.Find("FirePoint");
        if (firePoint == null)
        {
            GameObject fpGo = new GameObject("FirePoint");
            fpGo.transform.SetParent(weaponHolder, false);
            fpGo.transform.localPosition = new Vector3(0.12f, 0.12f, 0f);
            fpGo.transform.localRotation = Quaternion.identity;
            fpGo.transform.localScale = Vector3.one;
            firePoint = fpGo.transform;
        }
        else
        {
            firePoint.localPosition = new Vector3(0.12f, 0.12f, 0f);
        }

        WeaponController weaponCtrl = playerGo.GetComponent<WeaponController>();
        if (weaponCtrl == null) weaponCtrl = playerGo.AddComponent<WeaponController>();

        // Nạp Sprite cho 3 vũ khí
        Sprite bladeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/items_outdated/item_outdated1.png");
        Sprite axeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/items_outdated/item_outdated2.png");
        Sprite staffSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/items_outdated/item_outdated56.png");

        Sprite waterBullet = LoadFirstSprite("Assets/Effect and Bullet 16x16/Effect and Bullet 16x16/Water Effect and Bullet 16x16.png");
        Sprite fireBullet = LoadFirstSprite("Assets/Effect and Bullet 16x16/Effect and Bullet 16x16/Fire Effect and Bullet 16x16.png");
        Sprite purpleBullet = LoadFirstSprite("Assets/Effect and Bullet 16x16/Effect and Bullet 16x16/Purple Effect and Bullet 16x16.png");

        if (bladeSprite != null) weaponCtrl.bladeConfig.weaponSprite = bladeSprite;
        if (waterBullet != null) weaponCtrl.bladeConfig.bulletSprite = waterBullet;

        if (axeSprite != null) weaponCtrl.axeConfig.weaponSprite = axeSprite;
        if (fireBullet != null) weaponCtrl.axeConfig.bulletSprite = fireBullet;

        if (staffSprite != null) weaponCtrl.staffConfig.weaponSprite = staffSprite;
        if (purpleBullet != null) weaponCtrl.staffConfig.bulletSprite = purpleBullet;

        SpriteRenderer playerSr = playerGo.GetComponent<SpriteRenderer>();
        weaponCtrl.SetComponents(weaponPivot, weaponHolder, weaponSr, firePoint, playerSr, bulletPrefab);
        weaponCtrl.ApplyWeapon(GameSessionData.SelectedWeapon);

        // 5. CẤU HÌNH CAMERA ĐI THEO NGƯỜI CHƠI (CameraFollow)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
            if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow>();
            camFollow.SetTarget(playerGo.transform);
        }

        // 6. CẤU HÌNH BẢN ĐỒ RỪNG ELDERVALE (GIỚI HẠN TRONG KHU ĐẤT HIỆN TẠI)
        GameObject mapCtrlGo = GameObject.Find("MapController");
        if (mapCtrlGo == null)
        {
            mapCtrlGo = new GameObject("MapController");
        }

        MapController mapCtrl = mapCtrlGo.GetComponent<MapController>();
        if (mapCtrl == null) mapCtrl = mapCtrlGo.AddComponent<MapController>();
        mapCtrl.isInfiniteMap = false;
        mapCtrl.player = playerGo.transform;
        mapCtrl.chunkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Ground.prefab");
        mapCtrl.arenaMin = new Vector2(-14.5f, -11.5f);
        mapCtrl.arenaMax = new Vector2(14.5f, 11.5f);

        // Xóa sạch các chunk cũ đọng lại
        for (int i = mapCtrlGo.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(mapCtrlGo.transform.GetChild(i).gameObject);
        }

        // Căn chỉnh khu đất hiện tại và tạo tường biên vật lý
        mapCtrl.SetupBoundedArena();

        // Dựng cây cối, tảng đá và vật cản gói gọn bên trong khu đất hiện tại
        EnsureArenaDecor(mapCtrlGo.transform);

        // 7. CẤU HÌNH BỘ SINH QUÁI VẬT (EnemySpawner & Prefabs)
        var (beastPrefab, ghoulPrefab, wraithPrefab) = EnsureEnemyPrefabs();

        GameObject spawnerGo = GameObject.Find("EnemySpawner");
        if (spawnerGo == null)
        {
            spawnerGo = new GameObject("EnemySpawner");
        }
        EnemySpawner spawner = spawnerGo.GetComponent<EnemySpawner>();
        if (spawner == null) spawner = spawnerGo.AddComponent<EnemySpawner>();
        spawner.SetPrefabs(beastPrefab, ghoulPrefab, wraithPrefab);

        // 8. XÓA VÀ DỰNG LẠI CANVAS HUD
        GameObject oldCanvas = GameObject.Find("InGameHUD_Canvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

        GameObject eventSystemGo = GameObject.Find("EventSystem");
        if (eventSystemGo == null)
        {
            eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Canvas chuẩn Full HD 1920x1080
        GameObject canvasGo = new GameObject("InGameHUD_Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;

        // CỤM TIMER & CẢNH BÁO TRÊN ĐỈNH
        GameObject timerPanel = CreatePanel("TimerPanel", canvasGo.transform, new Vector2(0, -45), new Vector2(340, 65), new Color(0.08f, 0.09f, 0.13f, 0.88f));
        SetAnchored(timerPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0, -45), new Vector2(340, 65));

        GameObject timerBorder = CreatePanel("Border", timerPanel.transform, Vector2.zero, new Vector2(336, 61), new Color(0.18f, 0.2f, 0.28f, 0.6f));
        SetFullScreenStretch(timerBorder.GetComponent<RectTransform>());

        GameObject timerTextGo = CreateText("TimerText", timerPanel.transform, "TIME  00:00", 32, FontStyles.Bold, new Color(1f, 0.85f, 0.25f, 1f), defaultFont);
        SetFullScreenStretch(timerTextGo.GetComponent<RectTransform>());

        GameObject warningGo = CreateText("WaveWarningText", canvasGo.transform, "⚠ FINAL WAVE ⚠", 28, FontStyles.Bold, new Color(1f, 0.25f, 0.25f, 1f), defaultFont);
        SetAnchored(warningGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0, -100), new Vector2(800, 40));
        warningGo.SetActive(false);

        // CỤM STATUS PANEL (LEVEL, MÁU, XP) GÓC TRÊN BÊN TRÁI
        GameObject statusPanel = CreatePanel("StatusPanel", canvasGo.transform, Vector2.zero, new Vector2(460, 130), new Color(0.08f, 0.09f, 0.13f, 0.90f));
        SetAnchored(statusPanel.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(250, -80), new Vector2(460, 130));

        GameObject levelBox = CreatePanel("LevelBadge", statusPanel.transform, new Vector2(-175, 0), new Vector2(76, 76), new Color(0.16f, 0.18f, 0.25f, 1f));
        GameObject levelTextGo = CreateText("LevelText", levelBox.transform, "LV. 1", 24, FontStyles.Bold, new Color(1f, 0.75f, 0.2f, 1f), defaultFont);
        SetFullScreenStretch(levelTextGo.GetComponent<RectTransform>());

        // CỤM 3 TRÁI TIM (HEARTS UI - ROGUELITE RETRO STYLE)
        Sprite heartFullSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Heart_Full.png");
        Sprite heartEmptySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Heart_Empty.png");

        GameObject heartsContainer = CreateUIElement("HeartsContainer", statusPanel.transform);
        SetAnchored(heartsContainer.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(50, 24), new Vector2(320, 42));

        // Nhãn "LIFE" đỏ cam phong cách retro
        GameObject lifeLabel = CreateText("LifeLabel", heartsContainer.transform, "LIFE", 17f, FontStyles.Bold, new Color(1f, 0.4f, 0.45f, 1f), defaultFont);
        SetAnchored(lifeLabel.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(25, 0), new Vector2(50, 30));

        // 3 Slot Trái Tim
        var heartImagesList = new System.Collections.Generic.List<Image>();
        float[] heartOffsetsX = new float[] { 85f, 132f, 179f };

        for (int i = 0; i < 3; i++)
        {
            GameObject heartSlot = CreateUIElement($"HeartSlot_{i + 1}", heartsContainer.transform);
            SetAnchored(heartSlot.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(heartOffsetsX[i], 0), new Vector2(38, 38));
            Image heartImg = heartSlot.AddComponent<Image>();
            if (heartFullSprite != null) heartImg.sprite = heartFullSprite;
            heartImg.color = Color.white;
            heartImagesList.Add(heartImg);
        }

        // Chữ số "3 / 3"
        GameObject heartTextGo = CreateText("HeartCountText", heartsContainer.transform, "3 / 3", 18f, FontStyles.Bold, Color.white, defaultFont);
        SetAnchored(heartTextGo.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(245, 0), new Vector2(70, 30));

        // THANH XP
        GameObject xpBg = CreatePanel("XPBar_Bg", statusPanel.transform, new Vector2(50, -24), new Vector2(320, 26), new Color(0.18f, 0.2f, 0.25f, 1f));
        GameObject xpFillGo = CreateUIElement("XPFill", xpBg.transform);
        SetFullScreenStretch(xpFillGo.GetComponent<RectTransform>());
        Image xpFillImg = xpFillGo.AddComponent<Image>();
        xpFillImg.color = new Color(0.18f, 0.82f, 0.65f, 1f);
        xpFillImg.type = Image.Type.Filled;
        xpFillImg.fillMethod = Image.FillMethod.Horizontal;
        xpFillImg.fillAmount = 0f;

        GameObject xpTextGo = CreateText("XPText", xpBg.transform, "XP  0 / 100", 15, FontStyles.Bold, Color.white, defaultFont);
        SetFullScreenStretch(xpTextGo.GetComponent<RectTransform>());

        // 7. GẮN COMPONENT HUDController
        HUDController hudController = canvasGo.AddComponent<HUDController>();
        SetFieldValue(hudController, "playerStatus", playerStatus);
        hudController.SetHeartComponents(heartImagesList, heartFullSprite, heartEmptySprite, heartTextGo.GetComponent<TextMeshProUGUI>());
        SetFieldValue(hudController, "xpFillImage", xpFillImg);
        SetFieldValue(hudController, "xpText", xpTextGo.GetComponent<TextMeshProUGUI>());
        SetFieldValue(hudController, "levelText", levelTextGo.GetComponent<TextMeshProUGUI>());
        SetFieldValue(hudController, "timerText", timerTextGo.GetComponent<TextMeshProUGUI>());
        SetFieldValue(hudController, "waveWarningText", warningGo.GetComponent<TextMeshProUGUI>());

        // 8. TẠO MÀN HÌNH NÂNG CẤP KHI LÊN CẤP (Upgrade Panel)
        BuildUpgradeUI(canvasGo, defaultFont, playerStatus, weaponCtrl);

        // 9. TẠO MÀN HÌNH KẾT THÚC TRẬN (Game Over / Defeat & Victory UI)
        BuildGameOverUI(canvasGo, defaultFont, playerStatus, hudController);

        // 10. TẠO 2 VẬT PHẨM MẪU GẦN PLAYER ĐỂ TEST NHẶT
        CreateTestPickups(playerGo.transform.position);

        // Lưu scene
        EditorUtility.SetDirty(playerGo);
        EditorUtility.SetDirty(canvasGo);
        if (mainCam != null) EditorUtility.SetDirty(mainCam.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Thành công!", 
            "Đã hoàn tất cấu hình GameScene toàn diện (Bao gồm Bước 5: Rừng Eldervale & Vật cản)!\n\n" +
            "• RỪNG ELDERVALE ĐÃ ĐƯỢC PHỦ ĐẦY VẬT CẢN (TREES, ROCKS, BUSHES):\n" +
            "   - Đại thụ huyền bí (Ancient Trees) có bóng đổ râm mát, thân cây có PolygonCollider2D cản đạn và nhân vật\n" +
            "   - Tảng đá rêu phong (Rocks) & Bụi cỏ rừng già (Bushes) sinh ngẫu nhiên theo thuật toán Seed mượt mà\n" +
            "   - Phân bố 10x10 sub-cells thông minh, giữ khoảng trống trung tâm an toàn khi xuất phát\n" +
            "   - Độ sâu hình ảnh 2D (YSortOrder): nhân vật đứng trước đè gốc cây, đứng sau bị tán lá che khuất tự nhiên\n" +
            "   - Quái vật tự động trượt lướt vòng qua thân cây khi đuổi theo người chơi\n\n" +
            "• PHÍM TẮT TEST TOÀN DIỆN:\n" +
            "   - WASD / Mũi tên: Di chuyển mượt mà lượn qua các gốc cây\n" +
            "   - Space: Lướt thần tốc (Dash)\n" +
            "   - Chuột trái / J / K: Ngắm bắn 360 độ (Đạn chạm cây/đá sẽ va chạm)\n" +
            "   - B: Triệu hồi 5 quái đuổi theo qua các khe cây\n" +
            "   - U: Mở bảng chọn 3 thẻ nâng cấp\n" +
            "   - K / V: Mở thử màn hình Thất bại / Chiến thắng", 
            "Tuyệt vời!");
    }

    private static void BuildUpgradeUI(GameObject canvasGo, TMP_FontAsset defaultFont, PlayerStatus playerStatus, WeaponController weaponCtrl)
    {
        // Xóa panel nâng cấp cũ nếu có
        Transform oldPanel = canvasGo.transform.Find("Upgrade_Panel");
        if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);

        // Khung nền đen mờ bao phủ toàn màn hình
        GameObject upgradePanelGo = CreateUIElement("Upgrade_Panel", canvasGo.transform);
        SetFullScreenStretch(upgradePanelGo.GetComponent<RectTransform>());
        Image bgOverlay = upgradePanelGo.AddComponent<Image>();
        bgOverlay.color = new Color(0.04f, 0.06f, 0.1f, 0.94f);

        // Header Tiêu đề
        GameObject headerGo = CreateUIElement("Header", upgradePanelGo.transform);
        SetAnchored(headerGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(900f, 120f));

        GameObject levelTitleGo = CreateText("LevelUp_Title", headerGo.transform, "LEVEL UP!", 46f, FontStyles.Bold, new Color(1f, 0.85f, 0.2f, 1f), defaultFont);
        SetAnchored(levelTitleGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(900f, 60f));

        GameObject subtitleGo = CreateText("LevelUp_Subtitle", headerGo.transform, "CHỌN 1 TRONG 3 THẺ NÂNG CẤP KỸ NĂNG DƯỚI ĐÂY", 20f, FontStyles.Normal, new Color(0.75f, 0.9f, 1f, 0.9f), defaultFont);
        SetAnchored(subtitleGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.2f), Vector2.zero, new Vector2(900f, 35f));

        // Khung chứa 3 thẻ bài
        GameObject cardsContainer = CreateUIElement("Cards_Container", upgradePanelGo.transform);
        SetAnchored(cardsContainer.GetComponent<RectTransform>(), new Vector2(0.5f, 0.44f), Vector2.zero, new Vector2(1300f, 520f));

        float[] cardPositions = new float[] { -440f, 0f, 440f };
        UpgradeUIController.UpgradeCardUI[] cardUIs = new UpgradeUIController.UpgradeCardUI[3];

        for (int i = 0; i < 3; i++)
        {
            cardUIs[i] = CreateUpgradeCard($"Card_{i}", cardsContainer.transform, new Vector2(cardPositions[i], 0f), defaultFont);
        }

        UpgradeUIController upgradeCtrl = canvasGo.GetComponent<UpgradeUIController>();
        if (upgradeCtrl == null) upgradeCtrl = canvasGo.AddComponent<UpgradeUIController>();

        SetFieldValue(upgradeCtrl, "panelRoot", upgradePanelGo);
        SetFieldValue(upgradeCtrl, "levelTitleText", levelTitleGo.GetComponent<TextMeshProUGUI>());
        SetFieldValue(upgradeCtrl, "cards", cardUIs);
        SetFieldValue(upgradeCtrl, "playerStatus", playerStatus);
        SetFieldValue(upgradeCtrl, "weaponController", weaponCtrl);

        upgradeCtrl.upgradePool = new System.Collections.Generic.List<UpgradeUIController.UpgradeDefinition>
        {
            new UpgradeUIController.UpgradeDefinition
            {
                type = UpgradeUIController.UpgradeType.PowerShot,
                title = "Hỏa Lực Cực Đại",
                description = "Tăng thêm +15 sát thương cho mọi đòn tấn công của vũ khí.",
                icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/items_outdated/item_outdated14.png"),
                accentColor = new Color(1f, 0.35f, 0.2f, 1f)
            },
            new UpgradeUIController.UpgradeDefinition
            {
                type = UpgradeUIController.UpgradeType.RapidFire,
                title = "Tốc Kích Cuồng Phong",
                description = "Tăng thêm 20% tốc độ tấn công (giảm thời gian hồi chiêu).",
                icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/items_outdated/item_outdated20.png"),
                accentColor = new Color(0.2f, 0.85f, 1f, 1f)
            },
            new UpgradeUIController.UpgradeDefinition
            {
                type = UpgradeUIController.UpgradeType.PiercingEdge,
                title = "Mũi Tên Xuyên Thấu",
                description = "Đòn đánh xuyên qua thêm +1 kẻ địch trên đường bay.",
                icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/items_outdated/item_outdated1.png"),
                accentColor = new Color(0.95f, 0.85f, 0.25f, 1f)
            },
            new UpgradeUIController.UpgradeDefinition
            {
                type = UpgradeUIController.UpgradeType.SwiftBoots,
                title = "Bộ Pháp Thần Tốc",
                description = "Tăng thêm +25% tốc độ di chuyển và lướt của nhân vật.",
                icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/items_outdated/item_outdated3.png"),
                accentColor = new Color(0.35f, 1f, 0.55f, 1f)
            },
            new UpgradeUIController.UpgradeDefinition
            {
                type = UpgradeUIController.UpgradeType.Vitality,
                title = "Sinh Lực Bất Diệt",
                description = "Tăng thêm +1 Trái Tim tối đa và hồi phục toàn bộ Trái Tim ngay lập tức.",
                icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Heart_Full.png") ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/items_outdated/item_outdated15.png"),
                accentColor = new Color(1f, 0.28f, 0.45f, 1f)
            },
            new UpgradeUIController.UpgradeDefinition
            {
                type = UpgradeUIController.UpgradeType.HyperVelocity,
                title = "Xung Lực Đạn",
                description = "Tăng 30% tốc độ bay của đạn và tăng lực đẩy lùi (Knockback).",
                icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/items_outdated/item_outdated56.png"),
                accentColor = new Color(0.8f, 0.35f, 1f, 1f)
            }
        };

        // Mặc định ẩn bảng nâng cấp lúc ban đầu
        upgradePanelGo.SetActive(false);
    }

    private static UpgradeUIController.UpgradeCardUI CreateUpgradeCard(string name, Transform parent, Vector2 pos, TMP_FontAsset font)
    {
        var card = new UpgradeUIController.UpgradeCardUI();

        // 1. Thân thẻ bài
        GameObject root = CreateUIElement(name, parent);
        SetAnchored(root.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), pos, new Vector2(380f, 500f));
        Image cardBg = root.AddComponent<Image>();
        cardBg.color = new Color(0.1f, 0.13f, 0.2f, 0.98f);
        card.cardRoot = root;

        // 2. Viền màu Accent ở đỉnh thẻ bài
        GameObject borderGo = CreateUIElement("Border", root.transform);
        SetAnchored(borderGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(372f, 8f));
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = new Color(1f, 0.8f, 0.2f, 1f);
        card.borderImage = borderImg;

        // 3. Phân loại kỹ năng
        GameObject catGo = CreateText("Category", root.transform, "NÂNG CẤP KỸ NĂNG", 15f, FontStyles.Bold, new Color(0.9f, 0.9f, 0.9f, 0.7f), font);
        SetAnchored(catGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(340f, 30f));

        // 4. Khung Icon
        GameObject iconFrame = CreateUIElement("IconFrame", root.transform);
        SetAnchored(iconFrame.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(96f, 96f));
        Image frameBg = iconFrame.AddComponent<Image>();
        frameBg.color = new Color(0.06f, 0.08f, 0.12f, 1f);

        // Icon Image
        GameObject iconGo = CreateUIElement("Icon", iconFrame.transform);
        SetAnchored(iconGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(76f, 76f));
        Image iconImg = iconGo.AddComponent<Image>();
        card.iconImage = iconImg;

        // 5. Tiêu đề Kỹ năng
        GameObject titleGo = CreateText("Title", root.transform, "Tên Kỹ Năng", 22f, FontStyles.Bold, Color.white, font);
        SetAnchored(titleGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -195f), new Vector2(340f, 50f));
        card.titleText = titleGo.GetComponent<TextMeshProUGUI>();

        // Đường gạch ngang trang trí
        GameObject divGo = CreateUIElement("Divider", root.transform);
        SetAnchored(divGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -230f), new Vector2(300f, 2f));
        Image divImg = divGo.AddComponent<Image>();
        divImg.color = new Color(1f, 1f, 1f, 0.15f);

        // 6. Mô tả kỹ năng
        GameObject descGo = CreateText("Description", root.transform, "Mô tả chi tiết kỹ năng...", 17f, FontStyles.Normal, new Color(0.85f, 0.88f, 0.92f, 1f), font);
        SetAnchored(descGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -290f), new Vector2(320f, 100f));
        card.descText = descGo.GetComponent<TextMeshProUGUI>();

        // 7. Nút Chọn Nâng Cấp
        GameObject btnGo = CreateUIElement("SelectButton", root.transform);
        SetAnchored(btnGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(280f, 52f));
        Image btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.58f, 0.78f, 1f);
        Button btn = btnGo.AddComponent<Button>();
        card.selectButton = btn;

        GameObject btnTextGo = CreateText("BtnText", btnGo.transform, "CHỌN NÂNG CẤP", 19f, FontStyles.Bold, Color.white, font);
        SetFullScreenStretch(btnTextGo.GetComponent<RectTransform>());

        return card;
    }

    private static void BuildGameOverUI(GameObject canvasGo, TMP_FontAsset defaultFont, PlayerStatus playerStatus, HUDController hudController)
    {
        // 1. Xóa các panel GameOver cũ nếu có
        Transform oldDefeat = canvasGo.transform.Find("Defeat_Panel");
        if (oldDefeat != null) Object.DestroyImmediate(oldDefeat.gameObject);

        Transform oldVictory = canvasGo.transform.Find("Victory_Panel");
        if (oldVictory != null) Object.DestroyImmediate(oldVictory.gameObject);

        // =========================================================================
        // A. BẢNG THẤT BẠI (DEFEAT PANEL) - SIGNAL LOST -> SURVIVAL FAILED
        // =========================================================================
        GameObject defeatPanelGo = CreateUIElement("Defeat_Panel", canvasGo.transform);
        SetFullScreenStretch(defeatPanelGo.GetComponent<RectTransform>());
        Image defeatOverlay = defeatPanelGo.AddComponent<Image>();
        defeatOverlay.color = new Color(0.04f, 0.015f, 0.015f, 0.95f); // Đỏ thẫm u tối huyền bí

        // Khung trung tâm
        GameObject defeatCard = CreatePanel("Defeat_Card", defeatPanelGo.transform, Vector2.zero, new Vector2(740f, 620f), new Color(0.08f, 0.05f, 0.05f, 0.98f));
        // Viền đỏ rực trên đỉnh
        GameObject defeatTopBorder = CreateUIElement("TopBorder", defeatCard.transform);
        SetAnchored(defeatTopBorder.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(730f, 8f));
        Image dBorderImg = defeatTopBorder.AddComponent<Image>();
        dBorderImg.color = new Color(0.95f, 0.2f, 0.2f, 1f);

        // Tiêu đề Thất Bại
        GameObject dTitleGo = CreateText("Defeat_Title", defeatCard.transform, "SIGNAL LOST", 50f, FontStyles.Bold, new Color(1f, 0.22f, 0.22f, 1f), defaultFont);
        SetAnchored(dTitleGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -65f), new Vector2(700f, 60f));

        GameObject dSubGo = CreateText("Defeat_Subtitle", defeatCard.transform, "SURVIVAL FAILED - ĐÊM ĐEN ĐÃ NUỐT CHỬNG BẠN", 17f, FontStyles.Normal, new Color(1f, 0.7f, 0.7f, 0.9f), defaultFont);
        SetAnchored(dSubGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -115f), new Vector2(700f, 30f));

        // Hộp thống kê (Stats Box)
        GameObject dStatsBox = CreatePanel("StatsBox", defeatCard.transform, new Vector2(0f, -10f), new Vector2(620f, 220f), new Color(0.05f, 0.03f, 0.03f, 0.9f));

        GameObject dStatsBorder = CreatePanel("Border", dStatsBox.transform, Vector2.zero, new Vector2(616f, 216f), new Color(0.3f, 0.1f, 0.1f, 0.4f));
        SetFullScreenStretch(dStatsBorder.GetComponent<RectTransform>());

        GameObject dTimeGo = CreateText("TimeText", dStatsBox.transform, "THỜI GIAN SỐNG: <color=#FFAA00>00:00</color>", 24f, FontStyles.Bold, Color.white, defaultFont);
        SetAnchored(dTimeGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, 55f), new Vector2(580f, 40f));

        GameObject dKillGo = CreateText("KillText", dStatsBox.transform, "QUÁI ĐÃ TIÊU DIỆT: <color=#FF4444>0</color>", 24f, FontStyles.Bold, Color.white, defaultFont);
        SetAnchored(dKillGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(580f, 40f));

        GameObject dLevelGo = CreateText("LevelText", dStatsBox.transform, "CẤP ĐỘ ĐẠT ĐƯỢC: <color=#00EEFF>CẤP 1</color>", 24f, FontStyles.Bold, Color.white, defaultFont);
        SetAnchored(dLevelGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -55f), new Vector2(580f, 40f));

        // Nút bấm Retry & Hub
        Button dRetryBtn = CreateSimpleButton("RetryBtn", defeatCard.transform, new Vector2(-155f, -225f), new Vector2(270f, 58f), new Color(0.75f, 0.2f, 0.2f, 1f), "THỬ LẠI (RETRY)", defaultFont);
        Button dHubBtn = CreateSimpleButton("HubBtn", defeatCard.transform, new Vector2(155f, -225f), new Vector2(270f, 58f), new Color(0.2f, 0.25f, 0.35f, 1f), "VỀ HUB (HUB)", defaultFont);

        defeatPanelGo.SetActive(false);

        // =========================================================================
        // B. BẢNG CHIẾN THẮNG (VICTORY PANEL) - ELDERVALE CLEARED!
        // =========================================================================
        GameObject victoryPanelGo = CreateUIElement("Victory_Panel", canvasGo.transform);
        SetFullScreenStretch(victoryPanelGo.GetComponent<RectTransform>());
        Image victoryOverlay = victoryPanelGo.AddComponent<Image>();
        victoryOverlay.color = new Color(0.015f, 0.04f, 0.025f, 0.95f); // Xanh lục ngọc đậm trang nghiêm

        // Khung trung tâm
        GameObject victoryCard = CreatePanel("Victory_Card", victoryPanelGo.transform, Vector2.zero, new Vector2(740f, 620f), new Color(0.05f, 0.08f, 0.06f, 0.98f));
        // Viền vàng kim trên đỉnh
        GameObject victoryTopBorder = CreateUIElement("TopBorder", victoryCard.transform);
        SetAnchored(victoryTopBorder.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(730f, 8f));
        Image vBorderImg = victoryTopBorder.AddComponent<Image>();
        vBorderImg.color = new Color(1f, 0.85f, 0.2f, 1f);

        // Tiêu đề Chiến Thắng
        GameObject vTitleGo = CreateText("Victory_Title", victoryCard.transform, "ELDERVALE CLEARED!", 50f, FontStyles.Bold, new Color(1f, 0.85f, 0.2f, 1f), defaultFont);
        SetAnchored(vTitleGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -65f), new Vector2(700f, 60f));

        GameObject vSubGo = CreateText("Victory_Subtitle", victoryCard.transform, "BÌNH MINH ĐÃ ĐẾN - BẠN ĐÃ CHIẾN THẮNG ĐÊM ĐEN!", 17f, FontStyles.Normal, new Color(0.7f, 1f, 0.8f, 0.9f), defaultFont);
        SetAnchored(vSubGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -115f), new Vector2(700f, 30f));

        // Hộp thống kê (Stats Box)
        GameObject vStatsBox = CreatePanel("StatsBox", victoryCard.transform, new Vector2(0f, -10f), new Vector2(620f, 220f), new Color(0.02f, 0.05f, 0.03f, 0.9f));

        GameObject vStatsBorder = CreatePanel("Border", vStatsBox.transform, Vector2.zero, new Vector2(616f, 216f), new Color(0.1f, 0.35f, 0.2f, 0.4f));
        SetFullScreenStretch(vStatsBorder.GetComponent<RectTransform>());

        GameObject vTimeGo = CreateText("TimeText", vStatsBox.transform, "TỔNG THỜI GIAN: <color=#00FF88>20:00 (HOÀN THÀNH)</color>", 24f, FontStyles.Bold, Color.white, defaultFont);
        SetAnchored(vTimeGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, 55f), new Vector2(580f, 40f));

        GameObject vKillGo = CreateText("KillText", vStatsBox.transform, "QUÁI ĐÃ TIÊU DIỆT: <color=#FFAA00>0</color>", 24f, FontStyles.Bold, Color.white, defaultFont);
        SetAnchored(vKillGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(580f, 40f));

        GameObject vLevelGo = CreateText("LevelText", vStatsBox.transform, "CẤP ĐỘ HOÀN THÀNH: <color=#00EEFF>CẤP 1</color>", 24f, FontStyles.Bold, Color.white, defaultFont);
        SetAnchored(vLevelGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -55f), new Vector2(580f, 40f));

        // Nút bấm Retry & Hub
        Button vRetryBtn = CreateSimpleButton("RetryBtn", victoryCard.transform, new Vector2(-155f, -225f), new Vector2(270f, 58f), new Color(0.18f, 0.65f, 0.35f, 1f), "CHƠI LẠI (PLAY AGAIN)", defaultFont);
        Button vHubBtn = CreateSimpleButton("HubBtn", victoryCard.transform, new Vector2(155f, -225f), new Vector2(270f, 58f), new Color(0.2f, 0.25f, 0.35f, 1f), "VỀ HUB (HUB)", defaultFont);

        victoryPanelGo.SetActive(false);

        // =========================================================================
        // C. KẾT NỐI VÀO GameOverUIController
        // =========================================================================
        GameOverUIController gameOverCtrl = canvasGo.GetComponent<GameOverUIController>();
        if (gameOverCtrl == null) gameOverCtrl = canvasGo.AddComponent<GameOverUIController>();

        gameOverCtrl.SetComponents(
            defeatPanelGo, dTimeGo.GetComponent<TextMeshProUGUI>(), dKillGo.GetComponent<TextMeshProUGUI>(), dLevelGo.GetComponent<TextMeshProUGUI>(), dRetryBtn, dHubBtn,
            victoryPanelGo, vTimeGo.GetComponent<TextMeshProUGUI>(), vKillGo.GetComponent<TextMeshProUGUI>(), vLevelGo.GetComponent<TextMeshProUGUI>(), vRetryBtn, vHubBtn
        );
        SetFieldValue(gameOverCtrl, "playerStatus", playerStatus);
        SetFieldValue(gameOverCtrl, "hudController", hudController);

        EditorUtility.SetDirty(gameOverCtrl);
    }

    private static Button CreateSimpleButton(string name, Transform parent, Vector2 pos, Vector2 size, Color bgColor, string labelText, TMP_FontAsset font)
    {
        GameObject btnGo = CreateUIElement(name, parent);
        SetAnchored(btnGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), pos, size);
        Image img = btnGo.AddComponent<Image>();
        img.color = bgColor;
        Button btn = btnGo.AddComponent<Button>();

        ColorBlock cb = btn.colors;
        cb.normalColor = bgColor;
        cb.highlightedColor = bgColor * 1.2f;
        cb.pressedColor = bgColor * 0.8f;
        btn.colors = cb;

        GameObject textGo = CreateText("Label", btnGo.transform, labelText, 19f, FontStyles.Bold, Color.white, font);
        SetFullScreenStretch(textGo.GetComponent<RectTransform>());

        return btn;
    }

    private static void EnsureArenaDecor(Transform parent)
    {
        Transform decorRoot = parent.Find("Arena_Decorations");
        if (decorRoot != null)
        {
            Object.DestroyImmediate(decorRoot.gameObject);
        }

        GameObject decorRootGo = new GameObject("Arena_Decorations");
        decorRootGo.transform.SetParent(parent, false);
        decorRoot = decorRootGo.transform;

        // 1. Nạp các Prefab Cây Cối từ The Higalina Vault
        string[] treePaths = new string[]
        {
            "Assets/The Higalina Vault/Trees/prefabs/shadow/tree_medium_s_01.prefab",
            "Assets/The Higalina Vault/Trees/prefabs/shadow/tree_medium_s_05.prefab",
            "Assets/The Higalina Vault/Trees/prefabs/shadow/tree_medium_s_10.prefab",
            "Assets/The Higalina Vault/Trees/prefabs/shadow/tree_medium_s_15.prefab",
            "Assets/The Higalina Vault/Trees/prefabs/shadow/tree_medium_s_20.prefab",
            "Assets/The Higalina Vault/Trees/prefabs/shadow/tree_medium_s_25.prefab"
        };

        var treesList = new System.Collections.Generic.List<GameObject>();
        foreach (var tp in treePaths)
        {
            GameObject t = AssetDatabase.LoadAssetAtPath<GameObject>(tp);
            if (t != null) treesList.Add(t);
        }

        // 2. Nạp các Sprite Tảng Đá
        Sprite[] grasslandSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Seasonal Tilesets/Seasonal Tilesets/1 - Grassland/Grassland_entities (16 x 16).png").OfType<Sprite>().ToArray();
        Sprite singleRock = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Seasonal Tilesets/Seasonal Tilesets/5 - Misc. universal tiles/Rock_shape (16 x 16).png");

        var rockList = new System.Collections.Generic.List<Sprite>();
        if (singleRock != null) rockList.Add(singleRock);
        foreach (var s in grasslandSprites)
        {
            if (s.name.Contains("_4") || s.name.Contains("_5") || s.name.Contains("_6") || s.name.Contains("_7") || s.name.Contains("_8"))
            {
                rockList.Add(s);
            }
        }

        // 3. Nạp các Sprite Bụi Cây & Rêu Phong
        Sprite[] plantSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Seasonal Tilesets/Seasonal Tilesets/1 - Grassland/Extra_plants (16 x 16).png").OfType<Sprite>().ToArray();

        var bushList = new System.Collections.Generic.List<Sprite>();
        foreach (var s in grasslandSprites)
        {
            if (s.name.Contains("_0") || s.name.Contains("_1") || s.name.Contains("_2") || s.name.Contains("_3"))
            {
                bushList.Add(s);
            }
        }
        foreach (var s in plantSprites)
        {
            bushList.Add(s);
        }

        // 4. Tạo Hàng Rào Cây Cổ Thụ Bao Quanh 4 Cạnh Đất (12 Cây Biên)
        // Tạo khung viền tự nhiên rất đẹp cho khu đất, người chơi nhìn vào thấy rừng rậm khép kín
        Vector2[] borderTreePositions = new Vector2[]
        {
            // Cạnh trên (North)
            new Vector2(-12f, 10.5f), new Vector2(-6f, 10.8f), new Vector2(0f, 10.5f), new Vector2(6f, 10.8f), new Vector2(12f, 10.5f),
            // Cạnh dưới (South)
            new Vector2(-12f, -10.5f), new Vector2(-6f, -10.8f), new Vector2(0f, -10.5f), new Vector2(6f, -10.8f), new Vector2(12f, -10.5f),
            // Cạnh trái & phải
            new Vector2(-13.5f, 4.5f), new Vector2(-13.5f, -4.5f),
            new Vector2(13.5f, 4.5f), new Vector2(13.5f, -4.5f)
        };

        if (treesList.Count > 0)
        {
            int treeIdx = 0;
            foreach (var pos in borderTreePositions)
            {
                GameObject treePrefab = treesList[treeIdx % treesList.Count];
                GameObject treeInstance = (GameObject)PrefabUtility.InstantiatePrefab(treePrefab, decorRoot);
                treeInstance.name = $"BorderTree_{treeIdx + 1}";
                treeInstance.transform.position = new Vector3(pos.x, pos.y, 0f);
                treeInstance.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                treeInstance.tag = "Obstacle";

                YSortOrder ySort = treeInstance.GetComponent<YSortOrder>();
                if (ySort == null) ySort = treeInstance.AddComponent<YSortOrder>();
                ySort.IsStatic = true;
                ySort.YOffset = 0f;
                ySort.UpdateSortingOrder();

                treeIdx++;
            }

            // 5. Thêm 4 Cây Chiến Thuật Ở Trung Tâm (để người chơi thả diều / nấp quái vật)
            Vector2[] tacticalTreePositions = new Vector2[]
            {
                new Vector2(-6f, 3.5f),
                new Vector2(6f, 3.5f),
                new Vector2(-6f, -3.5f),
                new Vector2(6f, -3.5f)
            };

            int tacIdx = 0;
            foreach (var pos in tacticalTreePositions)
            {
                GameObject treePrefab = treesList[(tacIdx + 2) % treesList.Count];
                GameObject treeInstance = (GameObject)PrefabUtility.InstantiatePrefab(treePrefab, decorRoot);
                treeInstance.name = $"TacticalTree_{tacIdx + 1}";
                treeInstance.transform.position = new Vector3(pos.x, pos.y, 0f);
                treeInstance.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
                treeInstance.tag = "Obstacle";

                YSortOrder ySort = treeInstance.GetComponent<YSortOrder>();
                if (ySort == null) ySort = treeInstance.AddComponent<YSortOrder>();
                ySort.IsStatic = true;
                ySort.YOffset = 0f;
                ySort.UpdateSortingOrder();

                tacIdx++;
            }
        }

        // 6. Thêm 4 Tảng Đá Chiến Thuật (Có va chạm vật lý để cản đạn/quái)
        Vector2[] rockPositions = new Vector2[]
        {
            new Vector2(-3f, 2f),
            new Vector2(3f, -2f),
            new Vector2(-8.5f, -2f),
            new Vector2(8.5f, 2f)
        };

        for (int i = 0; i < rockPositions.Length; i++)
        {
            GameObject rockGo = new GameObject($"TacticalRock_{i + 1}");
            rockGo.transform.SetParent(decorRoot, false);
            rockGo.transform.position = new Vector3(rockPositions[i].x, rockPositions[i].y, 0f);
            rockGo.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            rockGo.tag = "Obstacle";

            SpriteRenderer sr = rockGo.AddComponent<SpriteRenderer>();
            if (rockList.Count > 0)
            {
                sr.sprite = rockList[i % rockList.Count];
            }
            sr.color = new Color(0.85f, 0.9f, 0.88f, 1f);

            CircleCollider2D col = rockGo.AddComponent<CircleCollider2D>();
            col.isTrigger = false;
            col.radius = 0.25f;

            YSortOrder ySort = rockGo.AddComponent<YSortOrder>();
            ySort.IsStatic = true;
            ySort.YOffset = 0f;
            ySort.UpdateSortingOrder();
        }

        // 7. Thêm Bụi Cây & Thảo Mộc Rải Rác Bên Trong (Chỉ trang trí làm đẹp, không cản bước)
        Vector2[] bushPositions = new Vector2[]
        {
            new Vector2(-2f, 3.5f),
            new Vector2(2f, 3.5f),
            new Vector2(-4f, -2.5f),
            new Vector2(4f, -2.5f),
            new Vector2(-7f, 1.5f),
            new Vector2(7f, -1.5f),
            new Vector2(-1f, -4f),
            new Vector2(1f, 4f)
        };

        for (int i = 0; i < bushPositions.Length; i++)
        {
            GameObject bushGo = new GameObject($"DecorBush_{i + 1}");
            bushGo.transform.SetParent(decorRoot, false);
            bushGo.transform.position = new Vector3(bushPositions[i].x, bushPositions[i].y, 0f);
            bushGo.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

            SpriteRenderer sr = bushGo.AddComponent<SpriteRenderer>();
            if (bushList.Count > 0)
            {
                sr.sprite = bushList[i % bushList.Count];
            }
            sr.color = new Color(0.7f, 0.85f, 0.75f, 1f);

            YSortOrder ySort = bushGo.AddComponent<YSortOrder>();
            ySort.IsStatic = true;
            ySort.YOffset = 0f;
            ySort.UpdateSortingOrder();
        }

        Debug.Log("<color=green>[Arena Decor]</color> Đã dựng hoàn tất 16 Cây Rừng, 4 Tảng Đá và 8 Bụi Cây gói gọn bên trong khu đất hiện tại!");
    }

    private static (GameObject shadowBeast, GameObject forestGhoul, GameObject darkWraith) EnsureEnemyPrefabs()
    {
        string pBeast = "Assets/Prefab/Enemy_ShadowBeast.prefab";
        string pGhoul = "Assets/Prefab/Enemy_ForestGhoul.prefab";
        string pWraith = "Assets/Prefab/Enemy_DarkWraith.prefab";

        GameObject b = AssetDatabase.LoadAssetAtPath<GameObject>(pBeast);
        GameObject g = AssetDatabase.LoadAssetAtPath<GameObject>(pGhoul);
        GameObject w = AssetDatabase.LoadAssetAtPath<GameObject>(pWraith);

        Sprite[] monsterSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/32rogues-0.5.0/32rogues/monsters.png").OfType<Sprite>().ToArray();

        Sprite sBeast = (monsterSprites.Length > 2) ? monsterSprites[2] : null;
        Sprite sGhoul = (monsterSprites.Length > 10) ? monsterSprites[10] : null;
        Sprite sWraith = (monsterSprites.Length > 15) ? monsterSprites[15] : null;

        if (b == null) b = CreateEnemyPrefab(pBeast, "Hắc Thú (Shadow Beast)", 40f, 3.2f, 10, 15, sBeast, new Color(0.95f, 0.45f, 0.45f, 1f));
        if (g == null) g = CreateEnemyPrefab(pGhoul, "Xác Sống (Forest Ghoul)", 75f, 2.2f, 15, 25, sGhoul, new Color(0.5f, 0.95f, 0.55f, 1f));
        if (w == null) w = CreateEnemyPrefab(pWraith, "U Hồn (Dark Wraith)", 120f, 2.8f, 20, 40, sWraith, new Color(0.7f, 0.45f, 1f, 1f));

        return (b, g, w);
    }

    private static GameObject CreateEnemyPrefab(string path, string name, float hp, float speed, int damage, int exp, Sprite sprite, Color tint)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefab"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefab");
        }

        GameObject go = new GameObject(name);
        go.tag = "Enemy";
        go.transform.localScale = new Vector3(3.2f, 3.2f, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (sprite != null) sr.sprite = sprite;
        sr.color = tint;
        sr.sortingOrder = 8;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.18f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        EnemyController ec = go.AddComponent<EnemyController>();
        ec.SetStats(hp, speed, damage, exp, name);

        YSortOrder ySort = go.AddComponent<YSortOrder>();
        ySort.IsStatic = false;
        ySort.YOffset = -0.2f;

        GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return saved;
    }

    private static GameObject EnsureBulletPrefab()
    {
        string prefabPath = "Assets/Prefab/PlayerBullet.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder("Assets/Prefab"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefab");
        }

        GameObject bulletGo = new GameObject("PlayerBullet");
        bulletGo.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

        SpriteRenderer sr = bulletGo.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 20000;
        Sprite defaultBulletSprite = LoadFirstSprite("Assets/Effect and Bullet 16x16/Effect and Bullet 16x16/Water Effect and Bullet 16x16.png");
        if (defaultBulletSprite != null) sr.sprite = defaultBulletSprite;

        CircleCollider2D col = bulletGo.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;

        Rigidbody2D rb = bulletGo.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        bulletGo.AddComponent<Projectile>();

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(bulletGo, prefabPath);
        Object.DestroyImmediate(bulletGo);

        return savedPrefab;
    }

    private static Sprite LoadFirstSprite(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .FirstOrDefault();
    }

    private static Sprite[] LoadSortedSprites(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();
    }

    private static void CreateTestPickups(Vector3 playerPos)
    {
        GameObject oldHp = GameObject.Find("Test_HealthPickup");
        if (oldHp != null) Object.DestroyImmediate(oldHp);

        GameObject hpGo = new GameObject("Test_HealthPickup");
        hpGo.transform.position = playerPos + new Vector3(2.5f, 0.8f, 0f);
        CircleCollider2D hpCol = hpGo.AddComponent<CircleCollider2D>();
        hpCol.isTrigger = true;
        hpCol.radius = 0.5f;
        SpriteRenderer hpSr = hpGo.AddComponent<SpriteRenderer>();
        Sprite hpSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Heart_Full.png") ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/items_outdated/item_outdated14.png");
        if (hpSprite != null) hpSr.sprite = hpSprite;
        hpSr.color = Color.white;
        hpSr.sortingOrder = 5;
        hpGo.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
        HealthPickup pickupComp = hpGo.AddComponent<HealthPickup>();
        pickupComp.customHealAmount = 1;

        GameObject oldXp = GameObject.Find("Test_ExpPickup");
        if (oldXp != null) Object.DestroyImmediate(oldXp);

        GameObject xpGo = new GameObject("Test_ExpPickup");
        xpGo.transform.position = playerPos + new Vector3(2.5f, -0.8f, 0f);
        CircleCollider2D xpCol = xpGo.AddComponent<CircleCollider2D>();
        xpCol.isTrigger = true;
        xpCol.radius = 0.5f;
        SpriteRenderer xpSr = xpGo.AddComponent<SpriteRenderer>();
        Sprite xpSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/items_outdated/item_outdated22.png");
        if (xpSprite != null) xpSr.sprite = xpSprite;
        xpSr.color = new Color(0.4f, 1f, 0.7f, 1f);
        xpSr.sortingOrder = 5;
        xpGo.AddComponent<ExperiencePickup>();

        // Tạo Bia Tập Bắn (Training Dummy) để người chơi test bắn đạn ngay lập tức
        GameObject oldDummy = GameObject.Find("Test_TrainingDummy");
        if (oldDummy != null) Object.DestroyImmediate(oldDummy);

        GameObject dummyGo = new GameObject("Test_TrainingDummy");
        dummyGo.tag = "Enemy";
        dummyGo.transform.position = playerPos + new Vector3(4.5f, 0f, 0f);
        dummyGo.transform.localScale = new Vector3(3.2f, 3.2f, 1f);

        CircleCollider2D dummyCol = dummyGo.AddComponent<CircleCollider2D>();
        dummyCol.isTrigger = true;
        dummyCol.radius = 0.25f;

        SpriteRenderer dummySr = dummyGo.AddComponent<SpriteRenderer>();
        Sprite monsterSprite = LoadFirstSprite("Assets/32rogues-0.5.0/32rogues/monsters.png");
        if (monsterSprite != null) dummySr.sprite = monsterSprite;
        else dummySr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/items_outdated/item_outdated1.png");
        dummySr.color = new Color(1f, 0.55f, 0.55f, 1f);

        YSortOrder dummyYSort = dummyGo.AddComponent<YSortOrder>();
        dummyYSort.IsStatic = false;
        dummyYSort.YOffset = -0.3f;

        dummyGo.AddComponent<TrainingDummy>();
    }

    private static void SetFieldValue(object obj, string fieldName, object val)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (field != null)
        {
            field.SetValue(obj, val);
        }
    }

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

    private static void SetAnchored(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
    {
        GameObject panelGo = CreateUIElement(name, parent);
        SetAnchored(panelGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), pos, size);
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
}
