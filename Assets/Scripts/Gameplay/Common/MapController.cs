using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("Cài đặt Bản đồ")]
    public GameObject chunkPrefab; // Kéo prefab MapChunk vào đây
    public Transform player;       // Kéo nhân vật Aiden vào đây
    public float chunkSize = 30f;  // Kích thước của 1 khối (VD: 30 đơn vị)

    private Vector2 currentChunkPosition;
    private Dictionary<Vector2, GameObject> activeChunks = new Dictionary<Vector2, GameObject>();

    void Start()
    {
        currentChunkPosition = GetChunkPosition(player.position);
        SpawnInitialChunks();
    }

    void Update()
    {
        // Kiểm tra xem người chơi đã sang khối bản đồ mới chưa
        Vector2 newChunkPosition = GetChunkPosition(player.position);
        if (newChunkPosition != currentChunkPosition)
        {
            currentChunkPosition = newChunkPosition;
            UpdateChunks();
        }
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
                Vector3 spawnPos = new Vector3(chunkPos.x * chunkSize, chunkPos.y * chunkSize, 0);
                GameObject newChunk = Instantiate(chunkPrefab, spawnPos, Quaternion.identity, transform);
                activeChunks.Add(chunkPos, newChunk);
            }
        }
    }

    // Tái chế (dịch chuyển) các khối bị bỏ lại phía sau lên phía trước
    private void UpdateChunks()
    {
        List<Vector2> chunksToKeep = new List<Vector2>();

        // Xác định 9 vị trí khối cần có quanh người chơi hiện tại
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                chunksToKeep.Add(currentChunkPosition + new Vector2(x, y));
            }
        }

        List<Vector2> chunksToRemove = new List<Vector2>();
        List<GameObject> chunksToRecycle = new List<GameObject>();

        // Tìm các khối đã nằm ngoài bán kính 3x3 để tái chế
        foreach (var chunk in activeChunks)
        {
            if (!chunksToKeep.Contains(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key);
                chunksToRecycle.Add(chunk.Value);
            }
        }

        // Dịch chuyển khối cũ sang vị trí mới
        int recycleIndex = 0;
        foreach (Vector2 newPos in chunksToKeep)
        {
            if (!activeChunks.ContainsKey(newPos))
            {
                GameObject recycledChunk = chunksToRecycle[recycleIndex];
                recycledChunk.transform.position = new Vector3(newPos.x * chunkSize, newPos.y * chunkSize, 0);

                // Cập nhật Dictionary
                activeChunks.Remove(chunksToRemove[recycleIndex]);
                activeChunks.Add(newPos, recycledChunk);

                recycleIndex++;
            }
        }
    }
}