using UnityEngine;

public class PreLoadNextZone : MonoBehaviour
{
    private TerrainManager _terrainManager;
    private bool isPreloaded = false;

    private void Start()
    {
        _terrainManager = TerrainManager.Instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPreloaded)
            return;
        isPreloaded = true;
        if (collision.CompareTag("Player"))
        {
            _terrainManager.SpawnNextChunk();
        }
    }
}
