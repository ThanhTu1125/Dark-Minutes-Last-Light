using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemySpawner : MonoBehaviour
{
    [Header("Cấu hình Prefab Quái Vật")]
    [SerializeField] private GameObject shadowBeastPrefab;
    [SerializeField] private GameObject forestGhoulPrefab;
    [SerializeField] private GameObject darkWraithPrefab;

    [Header("Cài đặt Spawn Quái")]
    [SerializeField] private float initialSpawnInterval = 1.4f;
    [SerializeField] private float minSpawnInterval = 0.35f;
    [SerializeField] private int maxEnemiesAlive = 70;
    [SerializeField] private float spawnDistanceMin = 14f;
    [SerializeField] private float spawnDistanceMax = 18f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugHotkeys = true;

    [Header("Giới hạn Khu Đất (Arena Bounds)")]
    [SerializeField] private bool useArenaBounds = true;
    [SerializeField] private Vector2 arenaMin = new Vector2(-13.8f, -10.8f);
    [SerializeField] private Vector2 arenaMax = new Vector2(13.8f, 10.8f);

    private Transform player;
    private float spawnTimer = 0f;
    private float survivalTime = 0f;
    private List<GameObject> activeEnemies = new List<GameObject>();

    public int ActiveEnemyCount => activeEnemies.Count;

    public void SetPrefabs(GameObject shadowBeast, GameObject forestGhoul, GameObject darkWraith)
    {
        shadowBeastPrefab = shadowBeast;
        forestGhoulPrefab = forestGhoul;
        darkWraithPrefab = darkWraith;
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (Time.timeScale <= 0.001f) return;

        if (GameSessionData.SurvivalTime > survivalTime)
        {
            survivalTime = GameSessionData.SurvivalTime;
        }
        else
        {
            survivalTime += Time.deltaTime;
            GameSessionData.SurvivalTime = survivalTime;
        }

        if (player == null)
        {
            FindPlayer();
            return;
        }

        // Loại bỏ quái đã bị tiêu diệt khỏi danh sách
        activeEnemies.RemoveAll(e => e == null);

        // Tính toán tốc độ sinh quái (dồn dập dần theo 20 phút = 1200s)
        float progress = Mathf.Clamp01(survivalTime / 1200f);
        float currentInterval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, progress);

        // Cảnh báo đợt cao trào Final Wave (19 phút 30s = 1170s)
        if (survivalTime >= 1170f)
        {
            currentInterval = minSpawnInterval * 0.7f;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentInterval)
        {
            spawnTimer = 0f;
            if (activeEnemies.Count < maxEnemiesAlive)
            {
                SpawnEnemy();
            }
        }

        // Phím nóng B: Sinh ngay 5 quái xung quanh để test chiến đấu tức thì
        if (enableDebugHotkeys && Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            Debug.Log("<color=red>[SPAWNER DEBUG]</color> Sinh nhanh 5 quái vật xung quanh (Phím B)");
            for (int i = 0; i < 5; i++)
            {
                SpawnEnemy(true);
            }
        }
    }

    private void SpawnEnemy(bool isCloseForTesting = false)
    {
        if (player == null) return;

        // Lựa chọn loại quái theo mốc thời gian sinh tồn
        GameObject prefabToSpawn = shadowBeastPrefab;

        float ghoulChance = Mathf.Clamp01((survivalTime - 60f) / 300f) * 0.4f; // Xuất hiện từ phút 1
        float wraithChance = Mathf.Clamp01((survivalTime - 180f) / 600f) * 0.25f; // Xuất hiện từ phút 3

        float roll = Random.value;
        if (roll < wraithChance && darkWraithPrefab != null)
        {
            prefabToSpawn = darkWraithPrefab;
        }
        else if (roll < (wraithChance + ghoulChance) && forestGhoulPrefab != null)
        {
            prefabToSpawn = forestGhoulPrefab;
        }

        if (prefabToSpawn == null)
        {
            prefabToSpawn = shadowBeastPrefab;
        }

        if (prefabToSpawn == null) return;

        Vector3 spawnPos;

        if (isCloseForTesting)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(3.5f, 5.5f);
            spawnPos = player.position + new Vector3(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist, 0f);
        }
        else if (useArenaBounds)
        {
            // Sinh quái từ 4 mép bìa rừng của khu đất hiện tại
            int edge = Random.Range(0, 4);
            switch (edge)
            {
                case 0: // Mép trên
                    spawnPos = new Vector3(Random.Range(arenaMin.x + 1f, arenaMax.x - 1f), arenaMax.y, 0f);
                    break;
                case 1: // Mép dưới
                    spawnPos = new Vector3(Random.Range(arenaMin.x + 1f, arenaMax.x - 1f), arenaMin.y, 0f);
                    break;
                case 2: // Mép trái
                    spawnPos = new Vector3(arenaMin.x, Random.Range(arenaMin.y + 1f, arenaMax.y - 1f), 0f);
                    break;
                default: // Mép phải
                    spawnPos = new Vector3(arenaMax.x, Random.Range(arenaMin.y + 1f, arenaMax.y - 1f), 0f);
                    break;
            }
        }
        else
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(spawnDistanceMin, spawnDistanceMax);
            spawnPos = player.position + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);
        }

        // Đảm bảo quái vật 100% sinh ra trên mặt đất, không rơi ra ngoài khoảng không
        spawnPos.x = Mathf.Clamp(spawnPos.x, arenaMin.x + 0.3f, arenaMax.x - 0.3f);
        spawnPos.y = Mathf.Clamp(spawnPos.y, arenaMin.y + 0.3f, arenaMax.y - 0.3f);

        GameObject enemyGo = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemyGo);
    }
}
