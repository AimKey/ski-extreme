using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TerrainManager : MonoBehaviour
{
    [SerializeField] private GameObject terrainChunkPrefab;
    [SerializeField] private GameObject terrainChunkStartPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private int chunkWidth = -1;

    private List<GameObject> _activeChunks = new List<GameObject>();
    private int _currentChunkIndex = 0;

    [FormerlySerializedAs("defaultSpeed")] public float baseSpeed = 50f;
    public float currentSpeed;

    public static TerrainManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        chunkWidth = terrainChunkPrefab.GetComponent<TerrainChunk>().ChunkWidth; // TODO: in the feature, select dynamically based on the chunk prefab
        SpawnChunk(0);
        currentSpeed = baseSpeed;
    }

    public void SpawnNextChunk()
    {
        _currentChunkIndex += 1;
        SpawnChunk(_currentChunkIndex);
    }

    void SpawnChunk(int index)
    {
        // Get the position of the last point of the last chunk
        Vector3 pos = new Vector3(0, 0, 0);
        var lastChunk = _activeChunks.Count > 0 ? _activeChunks[_activeChunks.Count - 1] : null;
        if (lastChunk != null)
        {
            TerrainChunk lastChunkTerrain = lastChunk.GetComponent<TerrainChunk>();
            pos = lastChunkTerrain.GetBeforeLastPointWorldPosition();
            Debug.Log($"[TerrainManager] Before last point position: {lastChunkTerrain.GetBeforeLastPointLocalPos()}, world position: {lastChunkTerrain.GetBeforeLastPointWorldPosition()}");
            Debug.Log($"[TerrainManager] Last point position: {lastChunkTerrain.GetLastPointLocalPos()}");
        }
        Debug.Log("[TerrainManager] Spawning chunk at index: " + index + ", position: " + pos);
        GameObject chunk;
        if (_currentChunkIndex == 0)
        {
            chunk = Instantiate(terrainChunkStartPrefab, pos, transform.rotation, transform);
        }
        else
        {
            chunk = Instantiate(terrainChunkPrefab, pos, transform.rotation, transform);
        }
        //chunk = Instantiate(terrainChunkStartPrefab, pos, transform.rotation, transform);
        chunk.GetComponent<TerrainChunk>().Init(pos, Random.Range(0f, 1000f), _currentChunkIndex);
        _activeChunks.Add(chunk);
        
        // Apply current speed to the new chunk
        var surfaceEffector2D = chunk.GetComponent<SurfaceEffector2D>();
        if (surfaceEffector2D != null)
        {
            surfaceEffector2D.speed = currentSpeed;
        }

        RemoveOldChunks();
    }

    void RemoveOldChunks()
    {
        while (_activeChunks.Count > 2)
        {
            Debug.LogWarning("Removing old chunk at index: " + 0 + ", current active chunks count: " + _activeChunks.Count);
            Destroy(_activeChunks[0]);
            _activeChunks.RemoveAt(0);
        }
    }

    public void SetSurfaceSpeed(float speed)
    {
        // Apply speed to all active chunks to ensure consistent behavior
        currentSpeed = speed;
        foreach (GameObject chunk in _activeChunks)
        {
            if (chunk != null)
            {
                var surfaceEffector2D = chunk.GetComponent<SurfaceEffector2D>();
                if (surfaceEffector2D != null)
                {
                    surfaceEffector2D.speed = speed;
                }
            }
        }
    }

    public void ResetSurfaceSpeed()
    {
        SetSurfaceSpeed(baseSpeed);
    }
}
