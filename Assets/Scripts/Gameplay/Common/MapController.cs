using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("Chế độ Bản Đồ")]
    [Tooltip("Nếu tắt, game sẽ giới hạn trong 1 khu đất hiện tại cố định thay vì sinh map vô tận")]
    public bool isInfiniteMap = false;

    [Header("Giới hạn Khu đất (Arena Bounds)")]
    public Vector2 arenaMin = new Vector2(-14.5f, -11.5f);
    public Vector2 arenaMax = new Vector2(14.5f, 11.5f);

    [Header("Cài đặt Bản đồ Vô tận (Nếu bật)")]
    public GameObject chunkPrefab; // Kéo prefab MapChunk vào đây
    public Transform player;       // Kéo nhân vật Aiden vào đây
    public float chunkSize = 30f;  // Kích thước của 1 khối (VD: 30 đơn vị)

    private Vector2 currentChunkPosition;
    private Dictionary<Vector2, GameObject> activeChunks = new Dictionary<Vector2, GameObject>();

    void Start()
    {
        // Tự động tìm chunkPrefab nếu chưa được gán trong Inspector
        if (chunkPrefab == null)
        {
#if UNITY_EDITOR
            chunkPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Ground.prefab");
#endif
            if (chunkPrefab == null)
            {
                Debug.LogWarning("[MapController] chunkPrefab chưa được gán! Vui lòng gán Ground.prefab vào MapController trong Inspector.");
            }
        }

        // Tự động tìm Player nếu chưa được gán trong Inspector
        if (player == null)
        {
            GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
            {
                player = playerGo.transform;
            }
        }

        // Nếu chế độ giới hạn trong khu đất hiện tại (Arena Mode)
        if (!isInfiniteMap)
        {
            SetupBoundedArena();
            return;
        }

        if (player != null && chunkPrefab != null)
        {
            currentChunkPosition = GetChunkPosition(player.position);
            SpawnInitialChunks();
        }
    }

    void Update()
    {
        if (!isInfiniteMap) return;
        if (player == null || chunkPrefab == null) return;

        // Kiểm tra xem người chơi đã sang khối bản đồ mới chưa
        Vector2 newChunkPosition = GetChunkPosition(player.position);
        if (newChunkPosition != currentChunkPosition)
        {
            currentChunkPosition = newChunkPosition;
            UpdateChunks();
        }
    }

    public void SetupBoundedArena()
    {
        // 1. Tìm hoặc gán Ground trong khu đất hiện tại
        Transform existingGround = transform.Find("Ground_Arena");
        if (existingGround == null)
        {
            GameObject sceneGround = GameObject.Find("Grid");
            if (sceneGround != null && sceneGround.transform.parent != transform)
            {
                sceneGround.transform.position = new Vector3(-7f, -7f, 0f);
            }
            else if (chunkPrefab != null)
            {
                GameObject groundObj = Instantiate(chunkPrefab, new Vector3(-7f, -7f, 0f), Quaternion.identity, transform);
                groundObj.name = "Ground_Arena";
            }
        }
        else
        {
            existingGround.position = new Vector3(-7f, -7f, 0f);
        }

        // 2. Tạo 4 bức tường biên vật lý quanh khu đất (không cho bất kỳ ai lọt ra ngoài)
        SetupBoundaryWalls();
    }

    public void SetupBoundaryWalls()
    {
        Transform boundsParent = transform.Find("MapBoundaries");
        if (boundsParent == null)
        {
            GameObject bp = new GameObject("MapBoundaries");
            bp.transform.SetParent(transform, false);
            boundsParent = bp.transform;
        }

        // Tường trên (North)
        EnsureWall(boundsParent, "Wall_North", new Vector2(0f, 12.5f), new Vector2(34f, 2f));
        // Tường dưới (South)
        EnsureWall(boundsParent, "Wall_South", new Vector2(0f, -12.5f), new Vector2(34f, 2f));
        // Tường trái (West)
        EnsureWall(boundsParent, "Wall_West", new Vector2(-15.5f, 0f), new Vector2(2f, 28f));
        // Tường phải (East)
        EnsureWall(boundsParent, "Wall_East", new Vector2(15.5f, 0f), new Vector2(2f, 28f));
    }

    private void EnsureWall(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        Transform wall = parent.Find(name);
        GameObject wallGo = wall != null ? wall.gameObject : new GameObject(name);
        wallGo.transform.SetParent(parent, false);
        wallGo.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        wallGo.tag = "Obstacle";

        BoxCollider2D box = wallGo.GetComponent<BoxCollider2D>();
        if (box == null) box = wallGo.AddComponent<BoxCollider2D>();
        box.size = size;
        box.isTrigger = false;
    }

    // Tính toán tọa độ khối dựa trên vị trí thực tế
    private Vector2 GetChunkPosition(Vector3 playerPos)
    {
        int x = Mathf.RoundToInt(playerPos.x / chunkSize);
        int y = Mathf.RoundToInt(playerPos.y / chunkSize);
        return new Vector2(x, y);
    }

    // Tạo 9 khối bản đồ (3x3) bao quanh người chơi lúc đầu game
    private void SpawnInitialChunks()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 chunkPos = currentChunkPosition + new Vector2(x, y);
                Vector3 spawnPos = new Vector3(chunkPos.x * chunkSize, chunkPos.y * chunkSize, 0f);

                if (!activeChunks.ContainsKey(chunkPos))
                {
                    GameObject newChunk = null;
                    if (chunkPrefab != null)
                    {
                        newChunk = Instantiate(chunkPrefab, spawnPos, Quaternion.identity, transform);
                    }
                    if (newChunk != null)
                    {
                        var decor = newChunk.GetComponent<ChunkDecorator>();
                        if (decor != null) decor.Decorate(chunkPos);

                        activeChunks.Add(chunkPos, newChunk);
                    }
                }
            }
        }
    }

    // Tái chế (dịch chuyển) các khối bị bỏ lại phía sau lên phía trước an toàn
    private void UpdateChunks()
    {
        // 1. Xác định 9 vị trí khối cần có quanh người chơi
        List<Vector2> chunksToKeep = new List<Vector2>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                chunksToKeep.Add(currentChunkPosition + new Vector2(x, y));
            }
        }

        // 2. Tìm các khối nằm ngoài bán kính 3x3 để tái chế
        List<Vector2> chunksToRemove = new List<Vector2>();
        List<GameObject> chunksToRecycle = new List<GameObject>();

        foreach (var chunk in activeChunks)
        {
            if (!chunksToKeep.Contains(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key);
                if (chunk.Value != null)
                {
                    chunksToRecycle.Add(chunk.Value);
                }
            }
        }

        // 3. Xóa các vị trí cũ khỏi Dictionary trước để tránh trùng lặp Key
        for (int i = 0; i < chunksToRemove.Count; i++)
        {
            activeChunks.Remove(chunksToRemove[i]);
        }

        // 4. Tìm các vị trí mới còn thiếu
        List<Vector2> missingPositions = new List<Vector2>();
        foreach (Vector2 pos in chunksToKeep)
        {
            if (!activeChunks.ContainsKey(pos))
            {
                missingPositions.Add(pos);
            }
        }

        // 5. Gán tái chế an toàn (Không bao giờ bị OutOfRange)
        int recycleIndex = 0;
        foreach (Vector2 newPos in missingPositions)
        {
            Vector3 spawnPos = new Vector3(newPos.x * chunkSize, newPos.y * chunkSize, 0f);
            GameObject chunkObj = null;

            if (recycleIndex < chunksToRecycle.Count)
            {
                chunkObj = chunksToRecycle[recycleIndex];
                if (chunkObj != null)
                {
                    chunkObj.transform.position = spawnPos;
                    var decor = chunkObj.GetComponent<ChunkDecorator>();
                    if (decor != null) decor.Decorate(newPos);
                }
                recycleIndex++;
            }
            else if (chunkPrefab != null)
            {
                chunkObj = Instantiate(chunkPrefab, spawnPos, Quaternion.identity, transform);
                if (chunkObj != null)
                {
                    var decor = chunkObj.GetComponent<ChunkDecorator>();
                    if (decor != null) decor.Decorate(newPos);
                }
            }

            if (chunkObj != null)
            {
                activeChunks[newPos] = chunkObj;
            }
        }

        // 6. Tiêu hủy các khối dư thừa nếu có
        for (int i = recycleIndex; i < chunksToRecycle.Count; i++)
        {
            if (chunksToRecycle[i] != null)
            {
                Destroy(chunksToRecycle[i]);
            }
        }
    }
}
