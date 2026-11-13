using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện bắt buộc để chuyển Scene

public class MainMenu : MonoBehaviour
{
    [Header("Tên Scene sẽ chuyển tới")]
    [SerializeField] private string gameSceneName = "GameScene";

    // Hàm này sẽ được gọi khi bấm nút Play
    public void PlayGame()
    {
        // Đảm bảo thời gian game chạy bình thường (phòng trường hợp trước đó bị Pause)
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    // (Tùy chọn) Hàm thoát game cho nút Quit
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Đã thoát game!"); // Hiển thị trong Editor để kiểm tra
    }
}