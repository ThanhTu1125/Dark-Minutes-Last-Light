using UnityEngine;

public class HealthPickup : Collectible
{
    protected override void OnCollect(GameObject player)
    {
        PlayerStatus status = player.GetComponent<PlayerStatus>();

        if (status != null && !status.isDead)
        {
            Debug.Log($"Đã hồi: +{itemData.value} Máu");
            Destroy(gameObject);
        }
    }
}