using Assets.Scripts.Models;
using Assets.Scripts.Terrain;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using static Unity.Burst.Intrinsics.X86;
using Random = UnityEngine.Random;

public class TerrainChunk : MonoBehaviour
{
    [SerializeField] private SpriteShapeController shape;
    [SerializeField] private EdgeCollider2D edge;
    [SerializeField] private GameObject loadNextChunkPrefab;
    [SerializeField] private GameObject terrainStructurePrefab;
    [Tooltip("The part where the terrain ends (Where you place the ramp for the player to jump to next chunk)")]
    [SerializeField] private GameObject terrainTail; // The part where the terrain ends (Where you place the ramp for the player to jump to next chunk)
    [Tooltip("The part where the terrain starts (Where the player lands)")]
    [SerializeField] private GameObject terrainHead; // The part where the terrain starts
    private TerrainSprinker _sprinkler;

    // Stats related to this chunk
    [Tooltip("Height scale of the terrain. This is used to multiply the Perlin noise value to get the height of the terrain.")]
    [FormerlySerializedAs("scale")] public int terrainHeightScale = 10;

    public int NumOfPoints = 150;

    [Tooltip("Distance between each point in the terrain.")]
    public int DistanceBetweenPoints = 30;

    [Tooltip("Scale of the Perlin noise used to generate terrain height.")]
    public float NoiseScale = 0.1f;

    [Tooltip("Smooth hills by modifying tangents of the points in the terrain. (The higher the smoother")]
    [SerializeField] private float smoothAmount = 5f;

    public float DistanceUntilNextChunk = -1;

    [Tooltip("The amount of terrain that will be dropped when the player lands on this chunk. This is used to create a smooth transition between chunks.")]
    public float CulmulativeDropAmount = 50f;
    private int _currentChunkIndex;

    // Calculated properties
    public int ChunkWidth => (NumOfPoints - 1) * DistanceBetweenPoints;

    private float _perlinSeed;
    public Vector3 startingPos;

    public TerrainSkeleton skeleton;
    private void Awake()
    {
        Debug.Log("Chunk info: " +
                  $"ChunkWidth: {ChunkWidth}, " +
                  $"numOfPoints: {NumOfPoints}, " +
                  $"distanceBetweenPoints: {DistanceBetweenPoints}");
    }


    public void Init(Vector3 startingPos, float perlinSeed, int currentChunkIndex)
    {
        this._perlinSeed = perlinSeed;
        shape = GetComponent<SpriteShapeController>();
        edge = GetComponent<EdgeCollider2D>();
        _sprinkler = TerrainSprinker.Instance;
        skeleton = new(DistanceBetweenPoints);
        this.startingPos = startingPos;

        if (DistanceUntilNextChunk == -1)
        {
            DistanceUntilNextChunk = DistanceBetweenPoints;
        }
        this._currentChunkIndex = currentChunkIndex;

        GenerateChunk();
    }

    private void GenerateChunk()
    {
        GenerateSkeleton(shape);
        skeleton.AdjustBeginningPointToMatchLastPoint(skeleton.GetPointCount() - 1);
        skeleton.AddClosingTerrainPointForSkeleton();
        skeleton.AdjustBeginningPointToMatchLastPoint(skeleton.GetPointCount() - 1);
        GenerateTerrainFromSkeleton(skeleton);

        shape.BakeCollider();
        shape.BakeMesh();

        // Place decorations for this chunk
        StartCoroutine(WaitForColliderAndPlaceDecorations());
        Debug.Log("[TerrainChunk] Chunk generated with " + shape.spline.GetPointCount() + " points.");
    }

    public void GenerateTerrainFromSkeleton(TerrainSkeleton skeleton)
    {
        var spline = shape.spline;
        spline.Clear();

        int i = 0;
        foreach (var point in skeleton.Points)
        {
            int index = spline.GetPointCount();
            spline.InsertPointAt(index, point.Position);
            spline.SetTangentMode(index, point.PointTangentType);
            spline.SetLeftTangent(index, point.TangentLeft);
            spline.SetRightTangent(index, point.TangentRight);
            // If this is the 2/3 of the total point, spawn a load next chunk prefab
            if (i == skeleton.GetPointCount() * 2 / 3)
            {
                SpawnLoadNextChunkPrefab(point.Position);
            }
            i++;
        }
    }

    // Generate layout with only terrain points, no decorations
    public void GenerateSkeleton(SpriteShapeController initShape)
    {
        try
        {
            //Check to see if this is the first chunk
            if (_currentChunkIndex == 0)
            {
                skeleton.ConvertExistingSplineIntoSkeleton(initShape);
            }
            else
            {
                // This is not the first chunk, use the prefab TerrainHead to generate the landing ground for the player
                var headShape = terrainHead.GetComponent<SpriteShapeController>();
                skeleton.ConvertExistingSplineIntoSkeleton(headShape);
                skeleton.TerrainGenerator.InitDecorationsFromPrefab(startingPos, terrainHead, this.gameObject, isFirst: true);
            }

            //skeleton.ConvertExistingSplineIntoSkeleton(initShape);

            float cumulativeDrop = 100f;
            var isOnFlat = false;
            // Start from the last point in the skeleton
            for (int i = 0; i < NumOfPoints; i++)
            {
                var lastPoint = skeleton.GetLastPoint();
                var posX = lastPoint.Position.x + DistanceBetweenPoints;

                float baseNoise = Mathf.PerlinNoise(posX * NoiseScale, _perlinSeed);
                float posY = (1f - baseNoise) * terrainHeightScale;

                if (Random.value < 0.5f)
                    isOnFlat = !isOnFlat;
                if (!isOnFlat)
                    cumulativeDrop += CulmulativeDropAmount;

                var newPos = new Vector3(posX, posY - cumulativeDrop, 0);

                TerrainPoint newPoint = new()
                {
                    Position = newPos,
                    PointTangentType = ShapeTangentMode.Continuous,
                    TangentLeft = new Vector3(-smoothAmount, 0, 0),
                    TangentRight = new Vector3(smoothAmount, 0, 0)
                };

                skeleton.AddPoint(newPoint);

                // Generate terrain structure every 10 points
                if (i % 10 == 0)
                {
                    Vector2 spawnPos = startingPos + newPos;

                    var generator = skeleton.TerrainGenerator;
                    generator.AppendStructureFromSplineToSkeletonRNG(skeleton, terrainStructurePrefab, 1f, gameObject);
                    generator.InitDecorationsFromPrefab(spawnPos, terrainStructurePrefab, gameObject);
                    float height = generator.GetStructurePrefabHeight(terrainStructurePrefab);
                    cumulativeDrop += height;
                    Debug.Log($"[TerrainChunk] Terrain height: {height}");
                }
            }
            var tailSpawningPos = new Vector2(startingPos.x + skeleton.GetLastPoint().Position.x, startingPos.y + skeleton.GetLastPoint().Position.y);
            // Generate the tail
            skeleton.TerrainGenerator.AppendSplineStructureAndDecorationsToSkeleton(skeleton, terrainTail, tailSpawningPos, this.gameObject);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void AdjustBeginningPointToMatchWithCurrentPoint(Vector2 newPointVector)
    {
        var beginningVector = shape.spline.GetPosition(0);
        if (newPointVector.y < beginningVector.y)
        {
            Debug.Log($"[TerrainChunk] Adjusting beginning point from {beginningVector.y} to {newPointVector.y}");
            beginningVector.y = newPointVector.y;
            shape.spline.SetPosition(0, beginningVector);
        }
    }

    private void SpawnLoadNextChunkPrefab(Vector2 newPointVector)
    {
        Vector3 newPos = (Vector3)(newPointVector);
        Debug.Log("[TerrainChunk] Spawning LoadNextChunkPrefab at position: " + newPos);
        // Draw a green line in the editor for debugging
        var realPos = shape.transform.TransformPoint(newPos);
        Debug.DrawLine(realPos, realPos + Vector3.up * 100, Color.green, 20f);
        Instantiate(loadNextChunkPrefab, realPos, Quaternion.identity, transform);
    }

    // Close the terrain by adjusting the final point's heightto match the beginning point's height
    public void AdjustFinalPointToCloseTerrain()
    {
        var beginningPoint = shape.spline.GetPosition(0);
        var finalPoint = shape.spline.GetPosition(GetSplineLastPointIndex());
        if (finalPoint.y < beginningPoint.y)
        {
            Debug.Log($"[TerrainChunk] Adjusting beginning point from {beginningPoint} to {finalPoint}");
            beginningPoint.y = finalPoint.y;
            shape.spline.SetPosition(0, beginningPoint);
        }
        else
        {
            Debug.Log($"[TerrainChunk] Adjusting final point to match beginning point: {finalPoint} => {beginningPoint}");
            finalPoint.y = beginningPoint.y;
            shape.spline.SetPosition(GetSplineLastPointIndex(), finalPoint);
        }
    }

    private IEnumerator WaitForColliderAndPlaceDecorations()
    {
        var edge = shape.GetComponent<EdgeCollider2D>();

        yield return new WaitUntil(() => edge != null && edge.pointCount >= NumOfPoints * 2);
        yield return null; // Wait one frame to ensure the collider is ready

        Debug.Log("[TerrainChunk] EdgeCollider2D is ready to be painted with point count: " + edge.pointCount);
        _sprinkler.PlaceDecorations(shape, this.gameObject);
    }

    // Note: Last point is the point where we close the spline
    // Before last point is the point where we can extend our terrain
    public int GetSplineLastPointIndex()
    {
        if (shape.spline.GetPointCount() == 0)
        {
            return -1;
        }
        return shape.spline.GetPointCount() - 1;
    }

    public Vector2 GetLastPointLocalPos()
    {
        if (shape.spline.GetPointCount() == 0)
        {
            return Vector2.zero;
        }
        return shape.spline.GetPosition(GetSplineLastPointIndex());
    }

    public int GetBeforeLastPointIndex()
    {
        if (shape.spline.GetPointCount() == 0)
        {
            return -1;
        }
        return shape.spline.GetPointCount() - 2;
    }

    public Vector2 GetBeforeLastPointLocalPos()
    {
        if (shape.spline.GetPointCount() < 2)
        {
            return Vector2.zero;
        }
        return shape.spline.GetPosition(GetBeforeLastPointIndex());
    }

    /// <summary>
    /// Returns the position of the last point in the terrain chunk.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetLastPointWorldPosition()
    {
        Debug.Log($"[Terrain chunk] Last point position: {GetLastPointLocalPos()}");
        var lastPointInWorldSpace = shape.transform.TransformPoint(GetLastPointLocalPos());
        lastPointInWorldSpace.x += DistanceUntilNextChunk;
        return lastPointInWorldSpace;
    }

    public Vector2 GetBeforeLastPointWorldPosition()
    {
        Debug.Log($"[Terrain chunk] Before last point position: {GetBeforeLastPointLocalPos()}");
        var lastPointInWorldSpace = shape.transform.TransformPoint(GetBeforeLastPointLocalPos());
        lastPointInWorldSpace.x += DistanceUntilNextChunk;
        return lastPointInWorldSpace;
    }

}