using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerStatus))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] private float invincibleDuration = 0.5f;

    private Rigidbody2D rb;
    private PlayerController playerController;
    private PlayerStatus playerStatus;

    private bool isDashing = false;
    private bool canDash = true;

    public bool IsDashing => isDashing;
    public bool CanDash => canDash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        playerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0.001f || playerStatus.isDead) return;

        bool dashInput = false;

        // Đọc phím Space từ New Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            dashInput = true;
        }

        // Đọc nút A / X trên tay cầm
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            dashInput = true;
        }

        if (dashInput && canDash && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        // Khóa di chuyển bình thường
        playerController.CanMove = false;

        // Xác định hướng Dash
        Vector2 dashDirection = playerController.Movement != Vector2.zero
            ? playerController.Movement
            : playerController.LastMoveDirection;

        // Kích hoạt bất tử
        playerStatus.IsInvincible = true;

        // Bắt đầu Dash
        rb.linearVelocity = dashDirection * dashSpeed;

        // Duy trì Dash
        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        // Kết thúc Dash
        rb.linearVelocity = Vector2.zero;
        playerController.CanMove = true;

        // Nếu i-frame dài hơn Dash thì tiếp tục bất tử
        float remainingInvincibleTime = invincibleDuration - dashDuration;

        if (remainingInvincibleTime > 0)
        {
            yield return new WaitForSeconds(remainingInvincibleTime);
        }

        playerStatus.IsInvincible = false;

        // Chờ hết cooldown
        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    private void OnDisable()
    {
        if (playerStatus != null)
        {
            playerStatus.IsInvincible = false;
        }

        if (playerController != null)
        {
            playerController.CanMove = true;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        isDashing = false;
        canDash = true;
    }
}
