using UnityEngine;
[System.Serializable]
public struct DamageInfo
{
    public float Amount;// Tổng sát thương gây ra, tính theo HP
    public DamageType Type;//Loại sát thương: Physical, Magic, Fire, Pure
    public float Knockback;//Độ lớn lực đẩy khi trúng đòn,
    public Vector2 KnockbackDirection;// Hướng lực đẩy khi trúng đòn, tính theo Vector2
    public GameObject Source;//Đối tượng gây ra sát thương

    public DamageInfo(
        float amount,
        DamageType type,
        float knockback,
        Vector2 knockbackDirection,
        GameObject source = null)
    {
        Amount = amount;
        Type = type;
        Knockback = knockback;
        KnockbackDirection = knockbackDirection.normalized; // Tự động chuẩn hóa Vector hướng
        Source = source;
    }
}