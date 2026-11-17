using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSortOrder : MonoBehaviour
{
    [Tooltip("Base order offset để luôn nổi trên nền gạch Ground (Order = 0)")]
    [SerializeField] private int baseOrder = 5000;

    [Tooltip("Hệ số nhân tọa độ Y. Mặc định 10 (mỗi 0.1m tương đương 1 đơn vị sorting)")]
    [SerializeField] private float precision = 10f;

    [Tooltip("Độ lệch Y của điểm tiếp đất (chân nhân vật hoặc gốc cây)")]
    [SerializeField] private float yOffset = 0f;

    [Tooltip("Nếu là vật thể tĩnh (Cây cối, Tảng đá), chỉ cần tính 1 lần lúc sinh ra để tối ưu 60+ FPS")]
    [SerializeField] private bool isStatic = true;

    private SpriteRenderer sr;

    public bool IsStatic
    {
        get => isStatic;
        set => isStatic = value;
    }

    public float YOffset
    {
        get => yOffset;
        set => yOffset = value;
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateSortingOrder();
    }

    private void LateUpdate()
    {
        if (!isStatic)
        {
            UpdateSortingOrder();
        }
    }

    public void UpdateSortingOrder()
    {
        if (sr != null)
        {
            float footY = transform.position.y + yOffset;
            sr.sortingOrder = baseOrder - Mathf.RoundToInt(footY * precision);
        }
    }
}
