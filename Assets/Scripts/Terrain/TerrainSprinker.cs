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

    public static TerrainSprinker Instance { get; private set; }
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

                // Instantiate decoration
                DecorationPrefab decor = GetRandomDecorationPrefab();
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
                            rotation = Quaternion.Slerp(Quaternion.identity, fullRotation, 0.5f); // Half rotation
                        }
                        // Instantiate the decoration prefab
                        PlaceDecoration(spawnPos, dynamicParentContainer, go, rotation);
                        RandomSize(decor.IsRandomSize, spawnPos, go);

                        // Update the previous decoration position
                        previousDecorationPos = spawnPos;
                    }
                }
            }

        }
        catch (Exception e)
        {
            Debug.Log($"[TerrainSprinker] Error placing decorations: {e.Message}");
        }
    }

    public Vector3 GetRandomXPlacement(Vector3 prevPoint, Vector3 currentPoint, Vector2 previousDecorationPos)
    {
        float t = Random.Range(0.2f, 0.8f);
        Vector3 spawnPos = Vector3.Lerp(prevPoint, currentPoint, t);
        if (previousDecorationPos != Vector2.zero)
        {
            float distance = Vector3.Distance(previousDecorationPos, spawnPos);
            int attempts = 0;
            Debug.Log($"[TerrainSprinker] Retry status: Distance: {distance} spacing: {spacing}, MaxAttempts: {MaxAttempts}");
            while (distance < spacing && attempts < MaxAttempts)
            {
                Debug.Log($"[TerrainSprinker] Distance between decorations at {previousDecorationPos} and {spawnPos} is too small: {distance}, re-adjusting. Point range {prevPoint} - {currentPoint}");
                t = Random.Range(0.2f, 0.8f);
                spawnPos = Vector3.Lerp(previousDecorationPos, currentPoint, t);
                distance = Vector3.Distance(previousDecorationPos, spawnPos);
                Debug.Log($"[TerrainSprinker] New spawn position: {spawnPos}, distance: {distance}, spacing: {spacing}, max attempts:" + $" {MaxAttempts}");
                attempts++;
            }
            // Gurantee that the decoration is placed at least `spacing` distance away from the previous decoration if random is not enough
            if (distance < spacing)
            {
                spawnPos.x += Mathf.Sign(spawnPos.x - previousDecorationPos.x) * spacing;
            }
            Debug.Log($"[TerrainSprinker] Final spawn position after adjustment: {spawnPos}, distance: {distance}, attempts: {attempts}");

        }
        return spawnPos;
    }


    public (float y, Vector2 normal) GetYPlacementRaycast(Vector2 position)
    {
        position.y = position.y + 200f;
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ground"));
        // Debug only
        Debug.DrawRay(position, Vector2.down * 200f, Color.red, 100f);
        if (hit.collider != null)
        {
            return (hit.point.y, hit.normal);
        }
        return (position.y, Vector2.up);
    }

    public void PlaceDecoration(Vector3 worldPosition, GameObject parentContainer, GameObject prefab, Quaternion rotation)
    {
        Instantiate(prefab, worldPosition, rotation, parentContainer.transform);
    }

    public DecorationPrefab GetRandomDecorationPrefab()
    {
        if (decorations.Count == 0) return null;
        int i = Random.Range(0, decorations.Count);
        DecorationPrefab decoration = decorations[i];
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

}
