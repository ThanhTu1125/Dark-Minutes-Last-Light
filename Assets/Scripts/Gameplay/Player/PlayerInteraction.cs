using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    private IInteractable currentInteractable;
    private PlayerStatus playerStatus;

    private void Awake()
    {
        playerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        if (playerStatus.isDead) return;

        DetectInteractable();

        if (Input.GetKeyDown(interactionKey))
        {
            Interact();
        }
    }

    private void DetectInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            interactionRange,
            interactableLayer
        );

        currentInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            IInteractable interactable =
                hit.GetComponent<IInteractable>();

            if (interactable == null)
                interactable = hit.GetComponentInParent<IInteractable>();

            if (interactable == null) continue;

            float distance = Vector2.Distance(
                transform.position,
                hit.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentInteractable = interactable;
            }
        }
    }

    private void Interact()
    {
        if (currentInteractable == null) return;

        currentInteractable.Interact();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionRange
        );
    }
}