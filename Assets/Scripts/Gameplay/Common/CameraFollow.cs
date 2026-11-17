using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Mục tiêu theo dõi")]
    [SerializeField] private Transform target;

    [Header("Cài đặt")]
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float orthographicSize = 5f;

    [Header("Giới hạn Camera trong Khu đất (Arena Clamping)")]
    [SerializeField] private bool clampToBounds = true;
    [SerializeField] private Vector2 minCameraPos = new Vector2(-5.8f, -6.5f);
    [SerializeField] private Vector2 maxCameraPos = new Vector2(5.8f, 6.5f);

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = orthographicSize;
        }

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // Giới hạn camera không lia ra ngoài vùng đất
        if (clampToBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minCameraPos.x, maxCameraPos.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minCameraPos.y, maxCameraPos.y);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
