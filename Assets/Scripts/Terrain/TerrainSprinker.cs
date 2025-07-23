using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

public class TerrainSprinker : MonoBehaviour
{
    [Tooltip("The distance between each decoration placed along the spline.")]
    public float spacing = 30f;
    public GameObject ParentContainer;
    public int MaxAttempts = 20;
    public List<DecorationPrefab> decorations;

    private string previousDecorTag;

    [Header("Coin Settings")]
    [SerializeField] private GameObject coinPrefab;
    public int minCoinCount = 3;
    public int maxCoinCount = 6;
    public float coinSpawningChance = 0.2f;
    public float coinSpacing = 1f;
    public static TerrainSprinker Instance { get; private set; }
    [Header("Power up Settings")]
    [SerializeField] private List<GameObject> powerUpPrefab;
    public float powerUpChance = 0.1f; // 10% chance to spawn a power-up

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

    /// <summary>
    /// Places decorations along the spline of a SpriteShapeController.
    /// </summary>
    /// <param name="ssc">The <code>SpriteShapeController</code></param>
    public void PlaceDecorations(SpriteShapeController ssc, GameObject dynamicParentContainer)
    {
        Vector2 previousDecorationPos = Vector2.zero;
        try
        {
            var spline = ssc.spline;
            for (int i = 4; i < spline.GetPointCount() - 2; i++)
            {
                // Convert to world space
                Vector3 prevPoint = ssc.transform.TransformPoint(spline.GetPosition(i - 1));
                Vector3 currentPoint = ssc.transform.TransformPoint(spline.GetPosition(i));
                Vector3 nextPoint = ssc.transform.TransformPoint(spline.GetPosition(i + 1));

                // Get random decoration prefab
                DecorationPrefab decor = null;
                // Make sure no same decoration with the same tag is place continously
                if (!string.IsNullOrEmpty(previousDecorTag) && previousDecorTag == "Rock")
                {
                    var altDecor = GetDecorNotContainingTag(previousDecorTag);
                    if (altDecor != null)
                        decor = altDecor;
                    else
                        decor = GetRandomDecorationPrefab();
                }
                else
                {
                    decor = GetRandomDecorationPrefab();
                }

                if (decor != null)
                {
                    var go = decor.Prefab;
                    for (int j = 0; j < decor.ObjectCount; j++)
                    {
                        // Pick a random point along this segment
                        var spawnPos = GetRandomXPlacement(prevPoint, currentPoint, previousDecorationPos);

                        // Adjust Y to surface, apply normal if needed
                        var (y, normal) = GetYPlacementRaycast(spawnPos);
                        spawnPos.y = y;
                        Quaternion rotation = Quaternion.identity;
                        if (decor.IsAlignToTerrain)
                        {
                            var fullRotation = Quaternion.FromToRotation(Vector3.up, normal);
                            rotation = Quaternion.Slerp(Quaternion.identity, fullRotation, 0.8f); // Half rotation
                        }

                        // Instantiate the decoration prefab
                        var spawned = PlaceDecoration(spawnPos, dynamicParentContainer, go, rotation);
                        RandomSize(decor.IsRandomSize, spawnPos, go);

                        // Update the previous decoration position
                        previousDecorationPos = spawnPos;
                        previousDecorTag = spawned.tag;
                    }
                }

                bool isCoinSpawned = false;
                // Place coins with a chance
                if (Random.value < coinSpawningChance)
                {
                    PlaceCoinRow(prevPoint, currentPoint, dynamicParentContainer);
                    isCoinSpawned = true;
                }
                if (!isCoinSpawned && Random.value < powerUpChance)
                {
                    PlacePowerup(prevPoint, currentPoint, dynamicParentContainer);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"[TerrainSprinker] Error placing decorations: {e.Message}");
        }
    }

    private GameObject GetRandomPowerupPrefab()
    {
        if (powerUpPrefab == null || powerUpPrefab.Count == 0)
        {
            Debug.LogWarning("[TerrainSprinker] No power-up prefabs available.");
            return null;
        }
        int randomIndex = Random.Range(0, powerUpPrefab.Count);
        return powerUpPrefab[randomIndex];
    }

    private void PlacePowerup(Vector3 prevPoint, Vector3 currentPoint, GameObject dynamicParentContainer)
    {
        var powerupPrefab = GetRandomPowerupPrefab();
        if (powerupPrefab == null)
        {
            Debug.LogWarning($"[TerrainSprinker] No power-up prefab found to place.");
        }
        float t = Random.Range(0.2f, 0.8f);
        Vector3 spawnPos = Vector3.Lerp(prevPoint, currentPoint, t);
        var (y, normal) = GetYPlacementRaycast(spawnPos);
        spawnPos.y = y;
        Quaternion rotation = Quaternion.identity;
        var fullRotation = Quaternion.FromToRotation(Vector3.up, normal);
        rotation = Quaternion.Slerp(Quaternion.identity, fullRotation, 0.8f); // Half rotation

        // Instantiate the power-up prefab
        var spawnedPowerup = PlaceDecoration(spawnPos, dynamicParentContainer, powerupPrefab, rotation);
    }

    public Vector3 GetRandomXPlacement(Vector3 prevPoint, Vector3 currentPoint, Vector2 previousDecorationPos)
    {
        float t = Random.Range(0.2f, 0.8f);
        Vector3 spawnPos = Vector3.Lerp(prevPoint, currentPoint, t);
        if (previousDecorationPos != Vector2.zero)
        {
            float distance = Vector3.Distance(previousDecorationPos, spawnPos);
            int attempts = 0;
            //Debug.Log($"[TerrainSprinker] Retry status: Distance: {distance} spacing: {spacing}, MaxAttempts: {MaxAttempts}");
            while (distance < spacing && attempts < MaxAttempts)
            {
                //Debug.Log($"[TerrainSprinker] Distance between decorations at {previousDecorationPos} and {spawnPos} is too small: {distance}, re-adjusting. Point range {prevPoint} - {currentPoint}");
                t = Random.Range(0.2f, 0.8f);
                spawnPos = Vector3.Lerp(previousDecorationPos, currentPoint, t);
                distance = Vector3.Distance(previousDecorationPos, spawnPos);
                //Debug.Log($"[TerrainSprinker] New spawn position: {spawnPos}, distance: {distance}, spacing: {spacing}, max attempts:" + $" {MaxAttempts}");
                attempts++;
            }
            // Gurantee that the decoration is placed at least `spacing` distance away from the previous decoration if random is not enough
            if (distance < spacing)
            {
                spawnPos.x += Mathf.Sign(spawnPos.x - previousDecorationPos.x) * spacing;
            }
            //Debug.Log($"[TerrainSprinker] Final spawn position after adjustment: {spawnPos}, distance: {distance}, attempts: {attempts}");

        }
        return spawnPos;
    }


    public (float y, Vector2 normal) GetYPlacementRaycast(Vector2 position)
    {
        position.y = position.y + 100f;
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ground"));
        RaycastHit2D hitTest = Physics2D.Raycast(position, Vector2.down, Mathf.Infinity);
        //Debug.Log($"[TerrainSprinker] Raycast hit layer: {hitTest.collider.name}");
        // Debug only
        Debug.DrawRay(position, Vector2.down * 200f, Color.red, 100f);
        if (hit.collider != null)
        {
            //Debug.Log($"[TerrainSprinker] Ground found at position {position} with hit point {hit.point} and normal {hit.normal}");
            return (hit.point.y, hit.normal);
        }
        //Debug.Log($"[TerrainSprinker] No ground found for position {position}. Using default Y position.");
        return (position.y, Vector2.up);
    }

    public GameObject PlaceDecoration(Vector3 worldPosition, GameObject parentContainer, GameObject prefab, Quaternion rotation)
    {
        return Instantiate(prefab, worldPosition, rotation, parentContainer.transform);
    }

    public DecorationPrefab GetRandomDecorationPrefab()
    {
        if (decorations.Count == 0) return null;
        int i = Random.Range(0, decorations.Count);
        DecorationPrefab decoration = decorations[i];
        return decoration;
    }

    public DecorationPrefab GetDecorNotContainingTag(string decorTag)
    {
        if (decorations.Count == 0) return null;
        List<DecorationPrefab> filteredDecorations = decorations.FindAll(d => !d.Prefab.CompareTag(decorTag));

        if (filteredDecorations.Count == 0) return null;
        int i = Random.Range(0, filteredDecorations.Count);
        DecorationPrefab decoration = filteredDecorations[i];
        return decoration;
    }

    private void RandomSize(bool IsRandomSize, Vector3 spawnPos, GameObject go)
    {
        // Optional random size
        if (IsRandomSize)
        {
            Debug.Log($"[TerrainSprinker] Random size for {go.name} at position {spawnPos}, IsRandomSize: {IsRandomSize}");
            float noise = Mathf.PerlinNoise(spawnPos.x * 0.05f, 100f);
            float scale = Mathf.Lerp(0.9f, 2f, noise);
            go.transform.localScale = go.transform.localScale * scale;
        }
    }


    public void PlaceCoinRow(Vector3 startPoint, Vector3 endPoint, GameObject parentContainer)
    {
        int coinCount = Random.Range(minCoinCount, maxCoinCount + 1);
        Vector3 direction = (endPoint - startPoint).normalized;

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPos = startPoint + direction * (i * coinSpacing);

            var (y, normal) = GetYPlacementRaycast(spawnPos);
            spawnPos.y = y;
            Quaternion rotation = Quaternion.identity;
            var fullRotation = Quaternion.FromToRotation(Vector3.up, normal);
            rotation = Quaternion.Slerp(Quaternion.identity, fullRotation, 0.8f); // Half rotation

            Instantiate(coinPrefab, spawnPos, rotation, parentContainer.transform);
        }
    }

}
