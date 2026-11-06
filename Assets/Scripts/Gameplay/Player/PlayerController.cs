
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStatus))]
public class PlayerController : MonoBehaviour
{
    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 10f;

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
        if (!CanMove || playerStatus.isDead)
        {
            movement = Vector2.zero;
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

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
    }

    public void StopMovement()
    {
        movement = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }
}
