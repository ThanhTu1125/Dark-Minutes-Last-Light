using UnityEngine;

public class ExperiencePickup : Collectible
{
    [SerializeField] public int customExpAmount = 15;

    protected override void OnCollect(GameObject player)
    {
        PlayerStatus status = player.GetComponent<PlayerStatus>();

        if (status != null && !status.isDead)
        {
            int expAmount = customExpAmount > 0 ? customExpAmount : (itemData != null ? (int)itemData.value : 15);
            status.GainExperience(expAmount);
            Destroy(gameObject);
        }
    }
}
