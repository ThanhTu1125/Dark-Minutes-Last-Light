
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MainMenuNavigation : MonoBehaviour
{
    [Header("Danh sách Button")]
    public Button[] menuButtons;

    [Header("Mũi tên chọn")]
    public RectTransform menuArrow;

    private int currentIndex = 0;

    void Start()
    {
        if (menuButtons.Length == 0) return;

        SelectButton(0);
    }

    void Update()
    {
        if (menuButtons.Length == 0) return;

        if (Keyboard.current == null) return;

        // Di chuyển lên
        if (Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame)
        {
            currentIndex--;

            if (currentIndex < 0)
                currentIndex = menuButtons.Length - 1;

            SelectButton(currentIndex);
        }

        // Di chuyển xuống
        if (Keyboard.current.downArrowKey.wasPressedThisFrame ||
            Keyboard.current.sKey.wasPressedThisFrame)
        {
            currentIndex++;

            if (currentIndex >= menuButtons.Length)
                currentIndex = 0;

            SelectButton(currentIndex);
        }

        // Nhấn Enter
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            menuButtons[currentIndex].onClick.Invoke();
        }
    }
    [Header("Vị trí mũi tên")]
    public float arrowOffsetX = 400f;


    void SelectButton(int index)
    {
        currentIndex = index;

        RectTransform buttonRect =
            menuButtons[currentIndex].GetComponent<RectTransform>();

        // Mũi tên nằm bên trái nút
        float offsetX = buttonRect.rect.width / 2 + 145f;

        menuArrow.position = buttonRect.position
            + new Vector3(-offsetX, 0f, 0f);

        EventSystem.current.SetSelectedGameObject(
            menuButtons[currentIndex].gameObject
        );
    }

}
