using UnityEngine;

public class CoinPickup : Collectible
{
    protected override void OnCollect(GameObject player)
    {
        Debug.Log($"Đã nhặt: {itemData.itemName} (+{itemData.value} Vàng)");
        Destroy(gameObject);
    }
}