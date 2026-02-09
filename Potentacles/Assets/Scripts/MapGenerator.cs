using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WallRarity
{
    public string name;
    public GameObject[] wallPrefabs;
    [Range(0f, 100f)]
    public float weight = 1f;
}

public class MapGenerator : MonoBehaviour
{
    public Player Player;
    [Header("Wall Prefabs")]
    [SerializeField] private GameObject[] roadPrefabs;

    [Header("Level Data")]
    [SerializeField] private List<MapSpawnTable> LevelSpawnTable;

    [Header("ATM Prefabs")]
    [SerializeField] private GameObject ATM;

    [Header("Generation Settings")]
    [SerializeField] private float wallWidth = 10f; // Width/length of each wall segment
    [SerializeField] private int visibleWallCount = 10; // How many walls ahead to keep active
    [SerializeField] private float initialLevelLength = 50f; // Starting level length

    private Queue<GameObject> activeRoads = new Queue<GameObject>();
    private Queue<GameObject> activeWalls = new Queue<GameObject>();
    private float nextSpawnZ = 0f;
    private float LevelLength;
    private float currentLevelProgress = 0f;
    private bool spawnedATM = false;

    private int currentLevel = 0;

    private MapSpawnTable CurrentTable;

    // Track ATM sections
    private List<float> atmSectionPositions = new List<float>();
    private HashSet<float> passedATMSections = new HashSet<float>(); // Track which ATMs we've passed
    public float ClosestATMZ { get; private set; }

    // Weighted random cache
    private float totalWeight;

    void Start()
    {
        LevelLength = initialLevelLength;
    }

    void SetTable()
    {
        int clampedLevel = Mathf.Clamp(currentLevel, 0, LevelSpawnTable.Count - 1);
        CurrentTable = LevelSpawnTable[clampedLevel];
    }

    void CalculateTotalWeight()
    {
        totalWeight = 0f;
        foreach (var rarity in CurrentTable.wallRarities)
        {
            totalWeight += rarity.weight;
        }
    }

    GameObject GetRandomWallByRarity()
    {
        // Generate random value between 0 and total weight
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        // Find which rarity tier we hit
        foreach (var rarity in CurrentTable.wallRarities)
        {
            cumulativeWeight += rarity.weight;

            if (randomValue <= cumulativeWeight)
            {
                // Check if this rarity has any prefabs
                if (rarity.wallPrefabs == null || rarity.wallPrefabs.Length == 0)
                {
                    Debug.LogWarning($"Rarity tier '{rarity.name}' has no prefabs assigned!");
                    continue;
                }

                // Pick a random prefab from this rarity tier
                return rarity.wallPrefabs[Random.Range(0, rarity.wallPrefabs.Length)];
            }
        }

        // Fallback: if something went wrong, use the first available prefab
        foreach (var rarity in CurrentTable.wallRarities)
        {
            if (rarity.wallPrefabs != null && rarity.wallPrefabs.Length > 0)
            {
                return rarity.wallPrefabs[0];
            }
        }

        Debug.LogError("No wall prefabs available in any rarity tier!");
        return null;
    }

    void Generate()
    {
        // Check if we need to spawn more walls
        float playerZ = Player.transform.position.z;
        float despawnThreshold = playerZ - wallWidth;
        float spawnThreshold = playerZ + (visibleWallCount * wallWidth);

        // Check if player passed through any ATM sections
        CheckATMPassed(playerZ);

        // Update closest ATM
        UpdateClosestATM(playerZ);

        // Remove walls behind the player
        while (activeRoads.Count > 0 && activeRoads.Peek().transform.position.z < despawnThreshold)
        {
            GameObject oldRoad = activeRoads.Dequeue();
            Destroy(oldRoad);
        }
        while (activeWalls.Count > 0 && activeWalls.Peek().transform.position.z < despawnThreshold)
        {
            GameObject oldWall = activeWalls.Dequeue();
            Destroy(oldWall);
        }

        // Remove ATM positions that are behind the player
        atmSectionPositions.RemoveAll(z => z < despawnThreshold);

        // Spawn new walls ahead
        while (nextSpawnZ < spawnThreshold)
        {
            var lengthIncrease = GameStateManager.Instance.GetLevelLengthIncrease(currentLevel);

            // Check if we've reached the level length and haven't spawned ATM yet
            if (currentLevelProgress >= LevelLength && !spawnedATM)
            {
                // Spawn 1 ATM section
                SpawnATMSection();
                spawnedATM = true;

                // Increase level length for next time
                LevelLength += lengthIncrease;
                Debug.Log($"Level length increased to: {LevelLength}");

                currentLevel++;

                SetTable();
                CalculateTotalWeight();

                // Continue to spawn more if needed
                continue;
            }

            // Check if we've passed the ATM section, reset for next level
            if (spawnedATM && currentLevelProgress >= LevelLength - lengthIncrease + wallWidth)
            {
                currentLevelProgress = 0f;
                spawnedATM = false;
            }

            // Normal spawning
            SpawnRoad();
            SpawnWall();

            // Move spawn position forward
            nextSpawnZ += wallWidth;
            currentLevelProgress += wallWidth;
        }
    }

    void Update()
    {
        if (GameStateManager.Instance.CurrentState == GameState.Walking)
        {
            Generate();
        }
    }

    void CheckATMPassed(float playerZ)
    {
        foreach (float atmZ in atmSectionPositions)
        {
            // Check if player has passed this ATM and we haven't notified yet
            if (playerZ > atmZ && !passedATMSections.Contains(atmZ))
            {
                passedATMSections.Add(atmZ);
                OnATMPassed();
            }
        }

        // Clean up old passed ATM records
        passedATMSections.RemoveWhere(z => z < playerZ - (wallWidth * 2));
    }

    void OnATMPassed()
    {
        Debug.Log("Player passed through ATM section!");
        // You can add more logic here, such as:
        // - Trigger an event
        // - Show UI notification
        // - Play a sound
        // - Award points/currency

        // Example: If you want to notify the Player script
        if (Player != null)
        {
            Player.OnATMPassed();
            GameStateManager.Instance.OnATMPassed();
        }
    }

    void UpdateClosestATM(float playerZ)
    {
        if (atmSectionPositions.Count == 0)
        {
            ClosestATMZ = float.MaxValue;
            return;
        }

        float closestDistance = float.MaxValue;
        float closest = atmSectionPositions[0];

        foreach (float atmZ in atmSectionPositions)
        {
            float distance = Mathf.Abs(atmZ - playerZ);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = atmZ;
            }
        }

        ClosestATMZ = closest;
    }

    void SpawnRoad()
    {
        // Pick a random wall from the pool
        GameObject roadprefab = roadPrefabs[Random.Range(0, roadPrefabs.Length)];
        // Instantiate the wall
        GameObject road = Instantiate(roadprefab, new Vector3(0, 0, nextSpawnZ), Quaternion.identity);
        activeRoads.Enqueue(road);
    }

    void SpawnWall()
    {
        // Pick random walls based on rarity weights
        GameObject wallPrefabLeft = GetRandomWallByRarity();
        GameObject wallPrefabRight = GetRandomWallByRarity();

        if (wallPrefabLeft == null || wallPrefabRight == null)
        {
            Debug.LogError("Failed to get wall prefabs!");
            return;
        }

        Vector3 spawnPosLeft = new Vector3(-6f, 0, nextSpawnZ);
        Vector3 spawnPosRight = new Vector3(6f, 0, nextSpawnZ);

        // Instantiate the wall
        GameObject wallLeft = Instantiate(wallPrefabLeft, spawnPosLeft, Quaternion.identity);
        GameObject wallRight = Instantiate(wallPrefabRight, spawnPosRight, Quaternion.identity);
        wallRight.transform.localScale = new Vector3(-1, 1, 1);

        activeWalls.Enqueue(wallLeft);
        activeWalls.Enqueue(wallRight);
    }

    void SpawnATMSection()
    {
        // Spawn road
        GameObject roadprefab = roadPrefabs[Random.Range(0, roadPrefabs.Length)];
        GameObject road = Instantiate(roadprefab, new Vector3(0, 0, nextSpawnZ), Quaternion.identity);
        activeRoads.Enqueue(road);

        // Spawn ATM walls on both sides
        Vector3 spawnPosLeft = new Vector3(-6f, 0, nextSpawnZ);
        Vector3 spawnPosRight = new Vector3(6f, 0, nextSpawnZ);

        var spawnLeft = Random.Range(0, 2) == 1;

        GameObject randomWall = GetRandomWallByRarity();

        GameObject wallLeft = null;
        GameObject wallRight = null;

        if (spawnLeft)
        {
            wallLeft = Instantiate(ATM, spawnPosLeft, Quaternion.identity);
            wallRight = Instantiate(randomWall, spawnPosRight, Quaternion.identity);
        }
        else
        {
            wallLeft = Instantiate(randomWall, spawnPosLeft, Quaternion.identity);
            wallRight = Instantiate(ATM, spawnPosRight, Quaternion.identity);
        }

        wallRight.transform.localScale = new Vector3(-1, 1, 1);

        activeWalls.Enqueue(wallLeft);
        activeWalls.Enqueue(wallRight);

        // Track this ATM section's position
        atmSectionPositions.Add(nextSpawnZ);

        // Increment here
        nextSpawnZ += wallWidth;
        currentLevelProgress += wallWidth;
    }

    internal void SetLevelLength(float value)
    {
        LevelLength = value;
    }

    public void ResetMapGenerator(float initialLength)
    {
        // Destroy all active roads
        while (activeRoads.Count > 0)
        {
            GameObject road = activeRoads.Dequeue();
            if (road != null)
            {
                Destroy(road);
            }
        }

        // Destroy all active walls
        while (activeWalls.Count > 0)
        {
            GameObject wall = activeWalls.Dequeue();
            if (wall != null)
            {
                Destroy(wall);
            }
        }

        // Clear all tracking data
        atmSectionPositions.Clear();
        passedATMSections.Clear();

        // Reset variables to initial state
        nextSpawnZ = 0f;
        currentLevelProgress = 0f;
        spawnedATM = false;
        currentLevel = 0;
        LevelLength = initialLength;
        ClosestATMZ = float.MaxValue;

        // Recalculate weights in case they changed


        SetTable();

        CalculateTotalWeight();

        Generate();
  

        Debug.Log("Map Generator reset!");
    }
}