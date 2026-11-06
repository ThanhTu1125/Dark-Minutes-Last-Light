using UnityEngine;

public class ExperiencePickup : Collectible
{
    protected override void OnCollect(GameObject player)
    {
        Debug.Log($"Đã nhận: +{itemData.value} EXP");
        Destroy(gameObject);
    }
}