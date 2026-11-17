using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [System.Serializable]
    public class WeaponData
    {
        public WeaponType weaponType;
        public string weaponName;
        public Sprite weaponSprite;
        public Sprite bulletSprite;
        public Color bulletColor = Color.white;
        public float damage = 25f;
        public float fireRate = 0.3f;        // Giây giữa các lần tấn công
        public float bulletSpeed = 16f;
        public float knockback = 4f;
        public int pierceCount = 1;
        public DamageType damageType = DamageType.Physical;
        public bool spinBullet = false;
        public Vector3 weaponOffset = new Vector3(0.08f, 0f, 0f);
        public Vector3 weaponScale = new Vector3(1f, 1f, 1f);
    }

    [Header("Cấu hình 3 Vũ Khí")]
    public WeaponData bladeConfig = new WeaponData
    {
        weaponType = WeaponType.Blade,
        weaponName = "Kiếm Ánh Sáng (Eclipse Blade)",
        damage = 25f,
        fireRate = 0.28f,
        bulletSpeed = 18f,
        knockback = 4f,
        pierceCount = 2,
        damageType = DamageType.Physical,
        bulletColor = new Color(0.35f, 0.95f, 1f, 1f),
        weaponOffset = new Vector3(0.08f, 0f, 0f),
        weaponScale = new Vector3(1f, 1f, 1f)
    };

    public WeaponData axeConfig = new WeaponData
    {
        weaponType = WeaponType.Axe,
        weaponName = "Rìu Huyết Thần (Crimson Cleaver)",
        damage = 55f,
        fireRate = 0.65f,
        bulletSpeed = 13f,
        knockback = 8f,
        pierceCount = 3,
        damageType = DamageType.Physical,
        spinBullet = true,
        bulletColor = new Color(1f, 0.3f, 0.3f, 1f),
        weaponOffset = new Vector3(0.08f, 0f, 0f),
        weaponScale = new Vector3(1.1f, 1.1f, 1f)
    };

    public WeaponData staffConfig = new WeaponData
    {
        weaponType = WeaponType.Staff,
        weaponName = "Trượng Ma Năng (Void Scepter)",
        damage = 35f,
        fireRate = 0.42f,
        bulletSpeed = 15f,
        knockback = 5f,
        pierceCount = 1,
        damageType = DamageType.Magic,
        bulletColor = new Color(0.85f, 0.35f, 1f, 1f),
        weaponOffset = new Vector3(0.08f, 0f, 0f),
        weaponScale = new Vector3(1f, 1f, 1f)
    };

    [Header("Tham chiếu Component")]
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    [SerializeField] private Transform firePoint;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Hiệu ứng Giật lùi (Recoil)")]
    [SerializeField] private float recoilDistance = 0.03f;
    [SerializeField] private float recoilRecoverSpeed = 16f;

    private WeaponData currentWeapon;
    private float fireTimer = 0f;
    private Vector2 aimDirection = Vector2.right;
    private Vector3 initialHolderLocalPos;

    // Các biến chỉ số cộng thêm từ nâng cấp (Upgrade buffs)
    private float bonusDamage = 0f;
    private float cooldownReductionPercent = 0f;
    private int extraPierce = 0;
    private float bonusProjectileSpeed = 0f;

    public Vector2 AimDirection => aimDirection;
    public WeaponData CurrentWeapon => currentWeapon;

    public void SetComponents(
        Transform pivot,
        Transform holder,
        SpriteRenderer weaponSr,
        Transform muzzle,
        SpriteRenderer playerSr,
        GameObject bullet)
    {
        weaponPivot = pivot;
        weaponHolder = holder;
        weaponSpriteRenderer = weaponSr;
        firePoint = muzzle;
        playerSpriteRenderer = playerSr;
        bulletPrefab = bullet;
    }

    private void Awake()
    {
        if (weaponPivot == null)
        {
            Transform foundPivot = transform.Find("WeaponPivot");
            if (foundPivot != null) weaponPivot = foundPivot;
            else weaponPivot = transform;
        }

        if (weaponHolder == null && weaponPivot != null)
        {
            weaponHolder = weaponPivot.Find("WeaponHolder");
        }

        if (weaponSpriteRenderer == null && weaponHolder != null)
        {
            weaponSpriteRenderer = weaponHolder.GetComponent<SpriteRenderer>();
        }

        if (firePoint == null && weaponHolder != null)
        {
            firePoint = weaponHolder.Find("FirePoint");
        }

        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer = GetComponentInParent<SpriteRenderer>();
        }

        if (weaponHolder != null)
        {
            initialHolderLocalPos = weaponHolder.localPosition;
        }
    }

    private void Start()
    {
        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer = GetComponentInParent<SpriteRenderer>();
        }

        // Đảm bảo nhân vật luôn có sortingOrder >= 10 để nổi trên mặt đất (Ground = 0)
        if (playerSpriteRenderer != null && playerSpriteRenderer.sortingOrder < 10)
        {
            playerSpriteRenderer.sortingOrder = 10;
        }

        // Tự động nạp loại vũ khí người chơi đã chọn tại Hub
        ApplyWeapon(GameSessionData.SelectedWeapon);
    }

    public void ApplyWeapon(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Blade:
                currentWeapon = bladeConfig;
                break;
            case WeaponType.Axe:
                currentWeapon = axeConfig;
                break;
            case WeaponType.Staff:
                currentWeapon = staffConfig;
                break;
            default:
                currentWeapon = bladeConfig;
                break;
        }

        if (currentWeapon != null && weaponSpriteRenderer != null)
        {
            if (currentWeapon.weaponSprite != null)
            {
                weaponSpriteRenderer.sprite = currentWeapon.weaponSprite;
            }

            if (weaponHolder != null)
            {
                weaponHolder.localPosition = currentWeapon.weaponOffset;
                weaponHolder.localScale = currentWeapon.weaponScale;
                initialHolderLocalPos = currentWeapon.weaponOffset;
            }
        }
    }

    private void Update()
    {
        // Khi game đang tạm dừng (ví dụ: đang mở Màn hình Nâng Cấp), không ngắm và không bắn đạn
        if (Time.timeScale <= 0.001f) return;

        UpdateAiming();
        UpdateShooting();
        UpdateRecoil();
    }

    private void UpdateAiming()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // 1. Ngắm theo Con trỏ chuột (Mouse Aim)
        if (Mouse.current != null)
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z));
            mouseWorld.z = 0f;

            Vector2 dir = (mouseWorld - weaponPivot.position);
            if (dir.sqrMagnitude > 0.0001f)
            {
                aimDirection = dir.normalized;
            }
        }

        // 2. Hỗ trợ Gamepad Cần gạt phải nếu có
        if (Gamepad.current != null)
        {
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
            if (rightStick.sqrMagnitude > 0.09f)
            {
                aimDirection = rightStick.normalized;
            }
        }

        // 3. Xoay WeaponPivot 360 độ theo hướng ngắm
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        // 4. Căn chỉnh vũ khí dính sát vào tay nhân vật và ngắm đúng đầu mũi
        if (weaponHolder != null && currentWeapon != null)
        {
            if (aimDirection.x < 0f)
            {
                // Ngắm sang trái: lật trục Y và xoay bù +45 độ để lưỡi kiếm chỉ thẳng tâm ngắm
                weaponHolder.localScale = new Vector3(currentWeapon.weaponScale.x, -currentWeapon.weaponScale.y, currentWeapon.weaponScale.z);
                weaponHolder.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }
            else
            {
                // Ngắm sang phải: giữ nguyên trục Y và xoay -45 độ để lưỡi kiếm chỉ thẳng tâm ngắm
                weaponHolder.localScale = currentWeapon.weaponScale;
                weaponHolder.localRotation = Quaternion.Euler(0f, 0f, -45f);
            }
        }

        // 5. Luôn giữ vũ khí hiển thị trên cùng (Sorting Order), không bao giờ bị ẩn sau map hay nhân vật
        if (weaponSpriteRenderer != null)
        {
            weaponSpriteRenderer.flipY = false;
            int baseOrder = (playerSpriteRenderer != null) ? Mathf.Max(10, playerSpriteRenderer.sortingOrder) : 10;
            weaponSpriteRenderer.sortingOrder = baseOrder + 1;
            if (playerSpriteRenderer != null)
            {
                weaponSpriteRenderer.sortingLayerID = playerSpriteRenderer.sortingLayerID;
            }
        }
    }

    private void UpdateShooting()
    {
        fireTimer += Time.deltaTime;

        if (currentWeapon == null) return;

        bool fireInput = false;

        // Bấm hoặc giữ Chuột trái để bắn / vung vũ khí
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            fireInput = true;
        }

        // Cò phải Gamepad (Right Trigger)
        if (Gamepad.current != null && Gamepad.current.rightTrigger.isPressed)
        {
            fireInput = true;
        }

        // Phím nóng J hoặc K trên bàn phím
        if (Keyboard.current != null && (Keyboard.current.jKey.isPressed || Keyboard.current.kKey.isPressed))
        {
            fireInput = true;
        }

        float effectiveCooldown = Mathf.Max(0.08f, currentWeapon.fireRate * (1f - cooldownReductionPercent));

        if (fireInput && fireTimer >= effectiveCooldown)
        {
            fireTimer = 0f;
            Fire();
        }
    }

    private void Fire()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = (firePoint != null)
            ? firePoint.position
            : weaponPivot.position + (Vector3)(aimDirection * 0.4f);

        GameObject bulletGo = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // Cài đặt Sprite và màu sắc cho viên đạn
        SpriteRenderer bulletSr = bulletGo.GetComponent<SpriteRenderer>();
        if (bulletSr != null)
        {
            if (currentWeapon.bulletSprite != null)
            {
                bulletSr.sprite = currentWeapon.bulletSprite;
            }
            bulletSr.color = currentWeapon.bulletColor;

            if (playerSpriteRenderer != null)
            {
                bulletSr.sortingLayerID = playerSpriteRenderer.sortingLayerID;
                bulletSr.sortingOrder = playerSpriteRenderer.sortingOrder + 2;
            }
        }

        // Khởi tạo đạn
        Projectile proj = bulletGo.GetComponent<Projectile>();
        if (proj == null) proj = bulletGo.AddComponent<Projectile>();

        float finalDamage = currentWeapon.damage + bonusDamage;
        float finalSpeed = currentWeapon.bulletSpeed + bonusProjectileSpeed;
        int finalPierce = currentWeapon.pierceCount + extraPierce;

        proj.Initialize(
            aimDirection,
            finalDamage,
            finalSpeed,
            currentWeapon.knockback,
            finalPierce,
            currentWeapon.damageType,
            transform.root.gameObject
        );
        proj.spinInFlight = currentWeapon.spinBullet;

        // Hiệu ứng giật lùi vũ khí
        if (weaponHolder != null)
        {
            weaponHolder.localPosition = initialHolderLocalPos - new Vector3(recoilDistance, 0f, 0f);
        }
    }

    private void UpdateRecoil()
    {
        if (weaponHolder != null)
        {
            weaponHolder.localPosition = Vector3.Lerp(
                weaponHolder.localPosition,
                initialHolderLocalPos,
                Time.deltaTime * recoilRecoverSpeed
            );
        }
    }

    // Các hàm hỗ trợ cho Màn hình Nâng Cấp (Level Up Upgrades)
    public void AddDamage(float amount)
    {
        bonusDamage += amount;
    }

    public void ReduceCooldown(float percent)
    {
        cooldownReductionPercent = Mathf.Clamp(cooldownReductionPercent + percent, 0f, 0.75f);
    }

    public void AddPierce(int amount)
    {
        extraPierce += amount;
    }

    public void AddProjectileSpeed(float amount)
    {
        bonusProjectileSpeed += amount;
    }
}
