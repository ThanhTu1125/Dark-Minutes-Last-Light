using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStatus))]
public class PlayerController : MonoBehaviour
{
    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("Giới hạn Khu đất (Arena Bounds)")]
    [SerializeField] private bool clampToArena = true;
    [SerializeField] private Vector2 arenaMin = new Vector2(-14.2f, -11.2f);
    [SerializeField] private Vector2 arenaMax = new Vector2(14.2f, 11.2f);

    private Rigidbody2D rb;
    private PlayerStatus playerStatus;

    private Vector2 movement;
    private Vector2 lastMoveDirection = Vector2.down;

    public Vector2 Movement => movement;
    public Vector2 LastMoveDirection => lastMoveDirection;

    public bool CanMove { get; set; } = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0.001f || !CanMove || playerStatus.isDead)
        {
            movement = Vector2.zero;
            return;
        }

        float moveX = 0f;
        float moveY = 0f;

        // Đọc phím từ New Input System (hỗ trợ cả WASD và phím mũi tên)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY -= 1f;
        }

        // Hỗ trợ Gamepad cần gạt trái nếu có cắm tay cầm
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.sqrMagnitude > 0.04f)
            {
                moveX = stick.x;
                moveY = stick.y;
            }
        }

        movement = new Vector2(moveX, moveY).normalized;

        if (movement != Vector2.zero)
        {
            lastMoveDirection = movement;
        }
    }

    private void FixedUpdate()
    {
        if (!CanMove || playerStatus.isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float currentSpeed = playerStatus.speed > 0
            ? playerStatus.speed
            : moveSpeed;

        rb.linearVelocity = movement * currentSpeed;

        // Giới hạn nhân vật luôn ở trong khu vực đất hiện tại
        if (clampToArena)
        {
            Vector3 pos = transform.position;
            float cX = Mathf.Clamp(pos.x, arenaMin.x, arenaMax.x);
            float cY = Mathf.Clamp(pos.y, arenaMin.y, arenaMax.y);
            if (cX != pos.x || cY != pos.y)
            {
                transform.position = new Vector3(cX, cY, pos.z);
            }
        }
    }

    public void StopMovement()
    {
        movement = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }
}
