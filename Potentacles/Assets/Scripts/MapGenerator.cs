using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapGenerator : MonoBehaviour
{
    public Player Player;

    [Header("Wall Prefabs")]
    [SerializeField] private GameObject[] roadPrefabs;

    [Header("Wall Prefabs")]
    [SerializeField] private GameObject[] wallPrefabs;

    [Header("Generation Settings")]
    [SerializeField] private float wallWidth = 10f; // Width/length of each wall segment
    [SerializeField] private int visibleWallCount = 10; // How many walls ahead to keep active

    private Queue<GameObject> activeRoads = new Queue<GameObject>();
    private Queue<GameObject> activeWalls = new Queue<GameObject>();

    private float nextSpawnZ = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        // Check if we need to spawn more walls
        float playerZ = Player.transform.position.z;
        float despawnThreshold = playerZ - wallWidth;
        float spawnThreshold = playerZ + (visibleWallCount * wallWidth);

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

        // Spawn new walls ahead
        while (nextSpawnZ < spawnThreshold)
        {
            SpawnRoad();
            SpawnWall();

            // Move spawn position forward
            nextSpawnZ += wallWidth;
        }
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
        GameObject wallPrefab = wallPrefabs[Random.Range(0, wallPrefabs.Length)];

        Vector3 spawnPosLeft = new Vector3(-6f, 0, nextSpawnZ);
        Vector3 spawnPosRight = new Vector3(6f, 0, nextSpawnZ);

        // Instantiate the wall
        GameObject wallLeft = Instantiate(wallPrefab, spawnPosLeft, Quaternion.identity);
        GameObject wallRight = Instantiate(wallPrefab, spawnPosRight, Quaternion.identity);
        wallRight.transform.localScale = new Vector3 (-1, 1, 1);


        activeWalls.Enqueue(wallLeft);
        activeWalls.Enqueue(wallRight);
    }
}
