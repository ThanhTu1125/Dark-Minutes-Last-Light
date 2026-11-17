using UnityEngine;

public class HealthPickup : Collectible
{
    [SerializeField] public int customHealAmount = 1;

    protected override void OnCollect(GameObject player)
    {
        PlayerStatus status = player.GetComponent<PlayerStatus>();

        if (status != null && !status.isDead)
        {
            int healAmount = customHealAmount > 0 ? customHealAmount : 1;
            status.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
