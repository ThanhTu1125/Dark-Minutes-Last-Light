using System.Collections.Generic;
using UnityEngine;

public class ChunkDecorator : MonoBehaviour
{
    [Header("Kho Tài Nguyên Vật Cản (Prefabs & Sprites)")]
    public GameObject[] treePrefabs;
    public Sprite[] rockSprites;
    public Sprite[] bushSprites;

    [Header("Số lượng tối đa mỗi Khối (Pool)")]
    [SerializeField] private int maxTrees = 4;
    [SerializeField] private int maxRocks = 3;
    [SerializeField] private int maxBushes = 4;

    private List<GameObject> pooledTrees = new List<GameObject>();
    private List<GameObject> pooledRocks = new List<GameObject>();
    private List<GameObject> pooledBushes = new List<GameObject>();
    private bool isPoolInitialized = false;

    private void Awake()
    {
        InitializePool();
    }

    public void InitializePool()
    {
        if (isPoolInitialized) return;
        isPoolInitialized = true;

        Transform decorParent = transform.Find("Decorations");
        if (decorParent == null)
        {
            GameObject parentGo = new GameObject("Decorations");
            parentGo.transform.SetParent(transform, false);
            decorParent = parentGo.transform;
        }

        // 1. Tạo Pool Cây Cối (Trees)
        if (treePrefabs != null && treePrefabs.Length > 0)
        {
            for (int i = 0; i < maxTrees; i++)
            {
                GameObject prefab = treePrefabs[i % treePrefabs.Length];
                if (prefab != null)
                {
                    GameObject treeGo = Instantiate(prefab, decorParent);
                    treeGo.name = $"Decor_Tree_{i}";
                    treeGo.tag = "Obstacle";

                    YSortOrder ySort = treeGo.GetComponent<YSortOrder>();
                    if (ySort == null) ySort = treeGo.AddComponent<YSortOrder>();
                    ySort.IsStatic = true;
                    ySort.YOffset = 0f;

                    treeGo.SetActive(false);
                    pooledTrees.Add(treeGo);
                }
            }
        }

        // 2. Tạo Pool Tảng Đá (Rocks)
        for (int i = 0; i < maxRocks; i++)
        {
            GameObject rockGo = new GameObject($"Decor_Rock_{i}");
            rockGo.transform.SetParent(decorParent, false);
            rockGo.tag = "Obstacle";

            SpriteRenderer sr = rockGo.AddComponent<SpriteRenderer>();
            if (rockSprites != null && rockSprites.Length > 0)
            {
                sr.sprite = rockSprites[i % rockSprites.Length];
            }
            sr.color = new Color(0.85f, 0.9f, 0.88f, 1f);

            CircleCollider2D col = rockGo.AddComponent<CircleCollider2D>();
            col.isTrigger = false;
            col.radius = 0.22f;
            col.offset = new Vector2(0f, 0f);

            YSortOrder ySort = rockGo.AddComponent<YSortOrder>();
            ySort.IsStatic = true;
            ySort.YOffset = 0f;

            rockGo.SetActive(false);
            pooledRocks.Add(rockGo);
        }

        // 3. Tạo Pool Bụi Cây & Rêu Phong (Bushes / Foliage)
        for (int i = 0; i < maxBushes; i++)
        {
            GameObject bushGo = new GameObject($"Decor_Bush_{i}");
            bushGo.transform.SetParent(decorParent, false);

            SpriteRenderer sr = bushGo.AddComponent<SpriteRenderer>();
            if (bushSprites != null && bushSprites.Length > 0)
            {
                sr.sprite = bushSprites[i % bushSprites.Length];
            }
            sr.color = new Color(0.7f, 0.85f, 0.75f, 1f);

            // Bụi rậm mỏng có thể đi xuyên qua để tránh cản trở di chuyển
            YSortOrder ySort = bushGo.AddComponent<YSortOrder>();
            ySort.IsStatic = true;
            ySort.YOffset = 0f;

            bushGo.SetActive(false);
            pooledBushes.Add(bushGo);
        }
    }

    /// <summary>
    /// Trang trí khối bản đồ theo thuật toán Pseudo-Random Seed dựa trên tọa độ khối (chunkCoord).
    /// Đảm bảo tính nhất quán (deterministic): quay lại cùng tọa độ sẽ thấy cây cối ở vị trí cũ!
    /// </summary>
    public void Decorate(Vector2 chunkCoord)
    {
        InitializePool();

        // Ẩn toàn bộ pool trước khi phân bố lại
        foreach (var t in pooledTrees) if (t != null) t.SetActive(false);
        foreach (var r in pooledRocks) if (r != null) r.SetActive(false);
        foreach (var b in pooledBushes) if (b != null) b.SetActive(false);

        // Trường hợp đặc biệt: Khối xuất phát (0, 0)
        // Chừa khoảng trống an toàn xung quanh nhân vật (bán kính 8m) để bắt đầu trận đấu thoải mái
        if (chunkCoord == Vector2.zero)
        {
            DecorateSpawnChunk();
            return;
        }

        // Tạo seed ngẫu nhiên duy nhất cho tọa độ khối này
        int seed = ((Mathf.RoundToInt(chunkCoord.x) * 73856093) ^ (Mathf.RoundToInt(chunkCoord.y) * 19349663)) & 0x7FFFFFFF;
        System.Random rng = new System.Random(seed);

        // Chia khối 30x30 thành 9 ô lưới phụ (Sub-cells) 10x10 để ngăn vật cản đè nhau
        List<Vector2> subCells = new List<Vector2>
        {
            new Vector2(-10f, 10f), new Vector2(0f, 10f), new Vector2(10f, 10f),
            new Vector2(-10f, 0f),  new Vector2(0f, 0f),  new Vector2(10f, 0f),
            new Vector2(-10f, -10f),new Vector2(0f, -10f),new Vector2(10f, -10f)
        };

        // Trộn ngẫu nhiên các ô lưới
        for (int i = 0; i < subCells.Count; i++)
        {
            int swapIdx = rng.Next(i, subCells.Count);
            (subCells[i], subCells[swapIdx]) = (subCells[swapIdx], subCells[i]);
        }

        int cellIdx = 0;

        // 1. Phân bố Cây Cối (2 đến 3 cây mỗi khối)
        int treeCount = rng.Next(2, Mathf.Min(4, pooledTrees.Count + 1));
        for (int i = 0; i < treeCount && i < pooledTrees.Count && cellIdx < subCells.Count; i++)
        {
            GameObject tree = pooledTrees[i];
            if (tree == null) continue;

            Vector2 basePos = subCells[cellIdx++];
            float jitterX = (float)(rng.NextDouble() * 3.0 - 1.5);
            float jitterY = (float)(rng.NextDouble() * 3.0 - 1.5);
            Vector3 localPos = new Vector3(basePos.x + jitterX, basePos.y + jitterY, 0f);

            tree.transform.localPosition = localPos;
            float scale = 0.65f + (float)(rng.NextDouble() * 0.12f);
            bool flipX = rng.Next(0, 2) == 1;
            tree.transform.localScale = new Vector3(flipX ? -scale : scale, scale, 1f);

            tree.SetActive(true);

            YSortOrder ySort = tree.GetComponent<YSortOrder>();
            if (ySort != null) ySort.UpdateSortingOrder();
        }

        // 2. Phân bố Tảng Đá (1 đến 2 tảng đá mỗi khối)
        int rockCount = rng.Next(1, Mathf.Min(3, pooledRocks.Count + 1));
        for (int i = 0; i < rockCount && i < pooledRocks.Count && cellIdx < subCells.Count; i++)
        {
            GameObject rock = pooledRocks[i];
            if (rock == null) continue;

            Vector2 basePos = subCells[cellIdx++];
            float jitterX = (float)(rng.NextDouble() * 3.0 - 1.5);
            float jitterY = (float)(rng.NextDouble() * 3.0 - 1.5);
            Vector3 localPos = new Vector3(basePos.x + jitterX, basePos.y + jitterY, 0f);

            rock.transform.localPosition = localPos;
            float scale = 2.4f + (float)(rng.NextDouble() * 0.8f);
            bool flipX = rng.Next(0, 2) == 1;
            rock.transform.localScale = new Vector3(flipX ? -scale : scale, scale, 1f);

            SpriteRenderer sr = rock.GetComponent<SpriteRenderer>();
            if (sr != null && rockSprites != null && rockSprites.Length > 0)
            {
                sr.sprite = rockSprites[rng.Next(0, rockSprites.Length)];
            }

            rock.SetActive(true);

            YSortOrder ySort = rock.GetComponent<YSortOrder>();
            if (ySort != null) ySort.UpdateSortingOrder();
        }

        // 3. Phân bố Bụi Cây (2 đến 3 bụi cây trang trí)
        int bushCount = rng.Next(2, Mathf.Min(4, pooledBushes.Count + 1));
        for (int i = 0; i < bushCount && i < pooledBushes.Count && cellIdx < subCells.Count; i++)
        {
            GameObject bush = pooledBushes[i];
            if (bush == null) continue;

            Vector2 basePos = subCells[cellIdx++];
            float jitterX = (float)(rng.NextDouble() * 4.0 - 2.0);
            float jitterY = (float)(rng.NextDouble() * 4.0 - 2.0);
            Vector3 localPos = new Vector3(basePos.x + jitterX, basePos.y + jitterY, 0f);

            bush.transform.localPosition = localPos;
            float scale = 2.2f + (float)(rng.NextDouble() * 0.6f);
            bool flipX = rng.Next(0, 2) == 1;
            bush.transform.localScale = new Vector3(flipX ? -scale : scale, scale, 1f);

            SpriteRenderer sr = bush.GetComponent<SpriteRenderer>();
            if (sr != null && bushSprites != null && bushSprites.Length > 0)
            {
                sr.sprite = bushSprites[rng.Next(0, bushSprites.Length)];
            }

            bush.SetActive(true);

            YSortOrder ySort = bush.GetComponent<YSortOrder>();
            if (ySort != null) ySort.UpdateSortingOrder();
        }
    }

    private void DecorateSpawnChunk()
    {
        // Tại khối xuất phát, chỉ đặt 2 cây ở 2 góc xa để người chơi có bãi đất trống rộng rãi
        if (pooledTrees.Count >= 2)
        {
            GameObject tree1 = pooledTrees[0];
            if (tree1 != null)
            {
                tree1.transform.localPosition = new Vector3(-11f, 11f, 0f);
                tree1.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                tree1.SetActive(true);
                tree1.GetComponent<YSortOrder>()?.UpdateSortingOrder();
            }

            GameObject tree2 = pooledTrees[1];
            if (tree2 != null)
            {
                tree2.transform.localPosition = new Vector3(11f, -11f, 0f);
                tree2.transform.localScale = new Vector3(-0.7f, 0.7f, 1f);
                tree2.SetActive(true);
                tree2.GetComponent<YSortOrder>()?.UpdateSortingOrder();
            }
        }

        // Đặt 2 tảng đá ở góc
        if (pooledRocks.Count >= 2)
        {
            GameObject rock1 = pooledRocks[0];
            if (rock1 != null)
            {
                rock1.transform.localPosition = new Vector3(-12f, -10f, 0f);
                rock1.transform.localScale = new Vector3(2.6f, 2.6f, 1f);
                rock1.SetActive(true);
                rock1.GetComponent<YSortOrder>()?.UpdateSortingOrder();
            }

            GameObject rock2 = pooledRocks[1];
            if (rock2 != null)
            {
                rock2.transform.localPosition = new Vector3(12f, 10f, 0f);
                rock2.transform.localScale = new Vector3(-2.6f, 2.6f, 1f);
                rock2.SetActive(true);
                rock2.GetComponent<YSortOrder>()?.UpdateSortingOrder();
            }
        }
    }
}
