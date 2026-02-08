using System.Collections.Generic;
using UnityEngine;


public class MapGenerator : MonoBehaviour
{
    public Player Player;
    [Header("Wall Prefabs")]
    [SerializeField] private GameObject[] roadPrefabs;
    [Header("Wall Prefabs")]
    [SerializeField] private GameObject[] wallPrefabs;
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

    // Track ATM sections
    private List<float> atmSectionPositions = new List<float>();
    private HashSet<float> passedATMSections = new HashSet<float>(); // Track which ATMs we've passed
    public float ClosestATMZ { get; private set; }

    void Start()
    {
        LevelLength = initialLevelLength;
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
        // Pick a random wall from the pool
        GameObject wallPrefabLeft = wallPrefabs[Random.Range(0, wallPrefabs.Length)];
        GameObject wallPrefabRight = wallPrefabs[Random.Range(0, wallPrefabs.Length)];
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

        GameObject atmLeft = Instantiate(ATM, spawnPosLeft, Quaternion.identity);
        GameObject atmRight = Instantiate(ATM, spawnPosRight, Quaternion.identity);
        atmRight.transform.localScale = new Vector3(-1, 1, 1);

        activeWalls.Enqueue(atmLeft);
        activeWalls.Enqueue(atmRight);

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

        Generate();

        Debug.Log("Map Generator reset!");
    }
}