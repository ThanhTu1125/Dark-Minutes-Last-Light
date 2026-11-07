using NUnit.Framework.Interfaces;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] protected ItemData itemData;
    [SerializeField] protected float magnetSpeed = 8f;

    protected Transform playerTransform;
    protected bool isBeingCollected = false;

    protected virtual void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    protected virtual void Update()
    {
        // Hiệu ứng hút vật phẩm về phía Player khi vào tầm
        if (isBeingCollected && playerTransform != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerTransform.position,
                magnetSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerTransform = collision.transform;
            OnCollect(collision.gameObject);
        }
    }

    // Hàm trừu tượng để các lớp con tự định nghĩa logic nhặt
    protected abstract void OnCollect(GameObject player);
}