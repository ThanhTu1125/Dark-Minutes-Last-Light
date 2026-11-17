using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("Cài đặt Tốc độ Hoạt ảnh")]
    [SerializeField] private float frameRate = 8f; // 8 khung hình/giây

    [Header("Kích thước hiển thị nhân vật")]
    [Tooltip("Tỷ lệ phóng to nhân vật (3.2x tương đương 1 ô gạch bản đồ, vừa vặn trên màn hình Full HD)")]
    [SerializeField] private Vector3 characterScale = new Vector3(3.2f, 3.2f, 1f);

    [Header("Sprites Hoạt Ảnh Theo Hướng")]
    public Sprite[] idleDownFrames;
    public Sprite[] idleSideFrames;
    public Sprite[] walkDownFrames;
    public Sprite[] walkUpFrames;
    public Sprite[] walkSideFrames;

    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    private WeaponController weaponController;

    private Sprite[] currentFrames;
    private int currentFrameIndex = 0;
    private float timer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sortingOrder < 10)
        {
            spriteRenderer.sortingOrder = 10;
        }
        playerController = GetComponent<PlayerController>();
        weaponController = GetComponentInChildren<WeaponController>();

        // Tự động căn chỉnh kích thước nhân vật tương xứng với bản đồ và UI
        if (characterScale != Vector3.zero)
        {
            transform.localScale = characterScale;
        }

        // Tự động căn chỉnh CircleCollider2D cho ôm khít phần chân/thân nhân vật
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null && col.radius > 0.25f)
        {
            col.radius = 0.15f;
            col.offset = new Vector2(0f, -0.05f);
        }

        // Tắt Animator nếu có để tránh bị ghi đè bởi clip xoay 48 hướng cũ
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = false;
        }
    }

    private void Start()
    {
        // Mặc định đứng nhìn xuống
        if (idleDownFrames != null && idleDownFrames.Length > 0)
        {
            SetAnimationState(idleDownFrames);
        }
        else if (idleSideFrames != null && idleSideFrames.Length > 0)
        {
            SetAnimationState(idleSideFrames);
        }
    }

    private void Update()
    {
        if (Time.timeScale <= 0.001f) return;
        if (playerController == null) return;
        if (weaponController == null) weaponController = GetComponentInChildren<WeaponController>();

        Vector2 move = playerController.Movement;
        Vector2 lastDir = playerController.LastMoveDirection;

        // Xác định hướng lật mặt (Ưu tiên theo hướng ngắm vũ khí nếu có)
        if (weaponController != null)
        {
            spriteRenderer.flipX = (weaponController.AimDirection.x < 0f);
        }

        // 1. Nếu đang bấm di chuyển
        if (move.sqrMagnitude > 0.01f)
        {
            if (weaponController == null && Mathf.Abs(move.x) > 0.1f)
            {
                spriteRenderer.flipX = (move.x < 0f);
            }

            if (Mathf.Abs(move.x) > 0.1f)
            {
                SetAnimationState(walkSideFrames);
            }
            else if (move.y > 0.1f)
            {
                SetAnimationState(walkUpFrames != null && walkUpFrames.Length > 0 ? walkUpFrames : walkSideFrames);
            }
            else
            {
                SetAnimationState(walkDownFrames != null && walkDownFrames.Length > 0 ? walkDownFrames : walkSideFrames);
            }
        }
        else
        {
            // 2. Đứng yên (Idle)
            if (weaponController == null && Mathf.Abs(lastDir.x) > 0.1f)
            {
                spriteRenderer.flipX = (lastDir.x < 0f);
            }

            bool lookSide = (weaponController != null)
                ? (Mathf.Abs(weaponController.AimDirection.x) > 0.35f)
                : (Mathf.Abs(lastDir.x) > 0.1f);

            if (lookSide)
            {
                SetAnimationState(idleSideFrames != null && idleSideFrames.Length > 0 ? idleSideFrames : idleDownFrames);
            }
            else
            {
                SetAnimationState(idleDownFrames != null && idleDownFrames.Length > 0 ? idleDownFrames : idleSideFrames);
            }
        }

        // 3. Cập nhật khung hình
        AnimateFrames();
    }

    private void SetAnimationState(Sprite[] targetFrames)
    {
        if (targetFrames == null || targetFrames.Length == 0) return;
        if (currentFrames == targetFrames) return;

        currentFrames = targetFrames;
        currentFrameIndex = 0;
        timer = 0f;
        UpdateSprite();
    }

    private void AnimateFrames()
    {
        if (currentFrames == null || currentFrames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;
            currentFrameIndex = (currentFrameIndex + 1) % currentFrames.Length;
            UpdateSprite();
        }
    }

    private void UpdateSprite()
    {
        if (spriteRenderer != null && currentFrames != null && currentFrames.Length > 0)
        {
            spriteRenderer.sprite = currentFrames[currentFrameIndex];
        }
    }
}
