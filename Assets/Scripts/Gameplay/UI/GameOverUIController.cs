using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GameOverUIController : MonoBehaviour
{
    [Header("Tham chiếu Đối tượng")]
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private HUDController hudController;

    [Header("Bảng Thất Bại (Defeat Panel)")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private TextMeshProUGUI defeatTimeText;
    [SerializeField] private TextMeshProUGUI defeatKillCountText;
    [SerializeField] private TextMeshProUGUI defeatLevelText;
    [SerializeField] private Button defeatRetryBtn;
    [SerializeField] private Button defeatHubBtn;

    [Header("Bảng Chiến Thắng (Victory Panel)")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryTimeText;
    [SerializeField] private TextMeshProUGUI victoryKillCountText;
    [SerializeField] private TextMeshProUGUI victoryLevelText;
    [SerializeField] private Button victoryRetryBtn;
    [SerializeField] private Button victoryHubBtn;

    [Header("Cài đặt")]
    [SerializeField] private float targetSurvivalSeconds = 1200f; // 20 phút
    [SerializeField] private bool enableDebugHotkeys = true;

    private bool isGameOver = false;

    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        if (playerStatus == null) playerStatus = FindFirstObjectByType<PlayerStatus>();
        if (hudController == null) hudController = FindFirstObjectByType<HUDController>();
    }

    private void Start()
    {
        // Ẩn cả 2 bảng khi mới vào trận
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        // Đăng ký sự kiện từ PlayerStatus
        if (playerStatus != null)
        {
            playerStatus.OnDied += OnPlayerDied;
        }

        // Gán sự kiện click cho các nút
        if (defeatRetryBtn != null) defeatRetryBtn.onClick.AddListener(OnRetryClicked);
        if (defeatHubBtn != null) defeatHubBtn.onClick.AddListener(OnHubClicked);
        if (victoryRetryBtn != null) victoryRetryBtn.onClick.AddListener(OnRetryClicked);
        if (victoryHubBtn != null) victoryHubBtn.onClick.AddListener(OnHubClicked);
    }

    private void OnDestroy()
    {
        if (playerStatus != null)
        {
            playerStatus.OnDied -= OnPlayerDied;
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        // Đồng bộ thời gian với HUDController nếu có
        if (hudController != null)
        {
            GameSessionData.SurvivalTime = hudController.CurrentSurvivalTime;
        }

        // Kiểm tra điều kiện Chiến Thắng (Sống sót đủ 20 phút)
        if (GameSessionData.SurvivalTime >= targetSurvivalSeconds)
        {
            TriggerVictory();
        }

        // Phím tắt Debug để kiểm tra nhanh giao diện
        if (enableDebugHotkeys && Keyboard.current != null)
        {
            // Phím K: Test ngay màn hình Thất bại (Defeat)
            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                Debug.Log("<color=red>[DEBUG]</color> Mở thử Màn hình Thất bại (Phím K)");
                TriggerDefeat();
            }

            // Phím V: Test ngay màn hình Chiến thắng (Victory)
            if (Keyboard.current.vKey.wasPressedThisFrame)
            {
                Debug.Log("<color=green>[DEBUG]</color> Mở thử Màn hình Chiến thắng (Phím V)");
                TriggerVictory();
            }
        }
    }

    private void OnPlayerDied()
    {
        if (isGameOver) return;
        StartCoroutine(ShowDefeatWithDelay(1.2f));
    }

    private IEnumerator ShowDefeatWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        TriggerDefeat();
    }

    public void TriggerDefeat()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Tạm dừng game
        Time.timeScale = 0f;

        if (hudController != null)
        {
            GameSessionData.SurvivalTime = hudController.CurrentSurvivalTime;
        }

        // Cập nhật số liệu thống kê
        int totalSecs = Mathf.FloorToInt(GameSessionData.SurvivalTime);
        int mins = totalSecs / 60;
        int secs = totalSecs % 60;

        if (defeatTimeText != null)
        {
            defeatTimeText.text = $"THỜI GIAN SỐNG: <color=#FFAA00>{mins:00}:{secs:00}</color>";
        }

        if (defeatKillCountText != null)
        {
            defeatKillCountText.text = $"QUÁI ĐÃ TIÊU DIỆT: <color=#FF4444>{GameSessionData.EnemiesKilled}</color>";
        }

        if (defeatLevelText != null)
        {
            int lvl = playerStatus != null ? playerStatus.currentLevel : 1;
            defeatLevelText.text = $"CẤP ĐỘ ĐẠT ĐƯỢC: <color=#00EEFF>CẤP {lvl}</color>";
        }

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }
    }

    public void TriggerVictory()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Tạm dừng game
        Time.timeScale = 0f;

        if (victoryTimeText != null)
        {
            victoryTimeText.text = "TỔNG THỜI GIAN: <color=#00FF88>20:00 (HOÀN THÀNH)</color>";
        }

        if (victoryKillCountText != null)
        {
            victoryKillCountText.text = $"QUÁI ĐÃ TIÊU DIỆT: <color=#FFAA00>{GameSessionData.EnemiesKilled}</color>";
        }

        if (victoryLevelText != null)
        {
            int lvl = playerStatus != null ? playerStatus.currentLevel : 1;
            victoryLevelText.text = $"CẤP ĐỘ HOÀN THÀNH: <color=#00EEFF>CẤP {lvl}</color>";
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    public void OnRetryClicked()
    {
        Debug.Log("Chơi lại trận đấu...");
        Time.timeScale = 1f;
        GameSessionData.ResetStats();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnHubClicked()
    {
        Debug.Log("Quay về Hub...");
        Time.timeScale = 1f;
        GameSessionData.ResetStats();
        SceneManager.LoadScene("Hub");
    }

    public void SetComponents(
        GameObject dPanel, TextMeshProUGUI dTime, TextMeshProUGUI dKills, TextMeshProUGUI dLvl, Button dRetry, Button dHub,
        GameObject vPanel, TextMeshProUGUI vTime, TextMeshProUGUI vKills, TextMeshProUGUI vLvl, Button vRetry, Button vHub)
    {
        defeatPanel = dPanel;
        defeatTimeText = dTime;
        defeatKillCountText = dKills;
        defeatLevelText = dLvl;
        defeatRetryBtn = dRetry;
        defeatHubBtn = dHub;

        victoryPanel = vPanel;
        victoryTimeText = vTime;
        victoryKillCountText = vKills;
        victoryLevelText = vLvl;
        victoryRetryBtn = vRetry;
        victoryHubBtn = vHub;
    }
}
