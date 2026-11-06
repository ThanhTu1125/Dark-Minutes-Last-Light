using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TreasureChest : MonoBehaviour, IInteractable
{
    [Header("Visual Settings")]
    [SerializeField] private Sprite openSprite;

    [Header("Loot Settings")]
    [Tooltip("Danh sách các Prefab vật phẩm có thể rớt ra (CoinPickup, HealthPickup, ExpPickup...)")]
    [SerializeField] private List<GameObject> dropItemPrefabs;

    [SerializeField] private int minDropAmount = 2;
    [SerializeField] private int maxDropAmount = 5;
    [SerializeField] private float dropForce = 3f; // Lực bắn vật phẩm ra xung quanh

    private SpriteRenderer spriteRenderer;
    private Collider2D chestCollider;
    private bool isOpen = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        chestCollider = GetComponent<Collider2D>();
    }

    public void Interact()
    {
        if (isOpen) return;

        isOpen = true;

        // 1. Đổi sprite rương mở
        if (openSprite != null)
        {
            spriteRenderer.sprite = openSprite;
        }

        // 2. Vô hiệu hóa khả năng tương tác lại
        if (chestCollider != null)
        {
            // Tùy chọn: có thể giữ va chạm vật lý để làm vật cản hoặc tắt Trigger
        }

        // 3. Rớt phần thưởng
        DropLoot();
    }

    private void DropLoot()
    {
        if (dropItemPrefabs == null || dropItemPrefabs.Count == 0)
        {
            Debug.LogWarning("Chưa gán Prefab vật phẩm vào Rương!");
            return;
        }

        // Xác định số lượng item sẽ rớt ra
        int dropAmount = Random.Range(minDropAmount, maxDropAmount + 1);

        for (int i = 0; i < dropAmount; i++)
        {
            // Chọn ngẫu nhiên 1 loại vật phẩm trong danh sách (Máu, Vàng, hoặc EXP)
            GameObject randomPrefab = dropItemPrefabs[Random.Range(0, dropItemPrefabs.Count)];

            // Tạo vật phẩm tại vị trí rương
            GameObject droppedItem = Instantiate(randomPrefab, transform.position, Quaternion.identity);

            // Tạo lực văng nhẹ ra xung quanh cho đẹp mắt
            Rigidbody2D itemRb = droppedItem.GetComponent<Rigidbody2D>();
            if (itemRb != null)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                itemRb.AddForce(randomDirection * dropForce, ForceMode2D.Impulse);
            }
        }

        Debug.Log($"Đã mở rương và rớt {dropAmount} phần thưởng!");
    }
}