using System;
using UnityEngine;
using Assets.Scripts.Models;
using Assets.Scripts.Terrain;
using UnityEngine.U2D;
using System.Collections.Generic;

namespace Assets.Scripts.Helpers
{
    public class SkeletonTerrainGeneratorHelper
    {
        private System.Random rng = new();
        public int DistanceBetweenPoint { get; set; }

        public void AppendStructureFromSplineToSkeletonRNG(TerrainSkeleton skeleton, GameObject structurePrefab, float structureChance = 0.1f, GameObject parentContainer = null)
        {
            if (rng.NextDouble() < structureChance)
            {
                AppendStructureFromSplineToSkeleton(skeleton, structurePrefab, parentContainer);
            }
        }

        public void AppendSplineStructureAndDecorationsToSkeleton(
            TerrainSkeleton skeleton,
            GameObject structurePrefab,
            Vector2 worldStartingPos,
            GameObject parentContainer = null)
        {
            AppendStructureFromSplineToSkeleton(skeleton, structurePrefab, parentContainer);
            InitDecorationsFromPrefab(worldStartingPos, structurePrefab, parentContainer);
        }

        public int GetStructurePrefabHeight(GameObject structurePrefab)
        {
            var ssc = structurePrefab.GetComponent<SpriteShapeController>();
            if (ssc == null || ssc.spline.GetPointCount() == 0)
            {
                Debug.LogWarning($"[SkeletonTerrainGenerator] Structure prefab {structurePrefab.name} has no SpriteShapeController or spline points.");
                return 0;
            }
            var spline = ssc.spline;
            // Find the lowest and highest Y positions in the spline
            float minY = spline.GetPosition(0).y;
            float maxY = spline.GetPosition(0).y;
            for (int i = 0; i < spline.GetPointCount() - 1; i++)
            {
                minY = spline.GetPosition(i).y < minY ? spline.GetPosition(i).y : minY;
                maxY = spline.GetPosition(i).y > maxY ? spline.GetPosition(i).y : maxY;
            }
            return (int)(maxY - minY);
        }

        public void AppendStructureFromSplineToSkeleton(TerrainSkeleton skeleton, GameObject structurePrefab, GameObject parentContainer)
        {
            Debug.Log($"[SkeletonTerrainGenerator] Appending structure from spline to skeleton. Structure Prefab: {structurePrefab.name}");
            Debug.Log($"[SkeletonTerrainGenerator] Structure prefab init location: {structurePrefab.transform.position}");
            var ssc = structurePrefab.GetComponent<SpriteShapeController>();

            var structureSpline = ssc.spline;
            if (structureSpline.GetPointCount() == 0 || skeleton.Points.Count == 0)
            {
                //Debug.Log($"Warning: [SkeletonTerrainGenerator] Structure spline or skeleton is empty. No points to append.");
                return;
            }

            var lastSkeletonPoint = skeleton.GetLastPoint();
            float offsetX = lastSkeletonPoint.Position.x + DistanceBetweenPoint;
            float offsetY = lastSkeletonPoint.Position.y;
            Debug.Log($"[SkeletonTerrainGenerator] Appending structure from spline. Last Skeleton Point: {lastSkeletonPoint.Position}");
            //Debug.Log($"[SkeletonTerrainGenerator] SplineShapeFirstIndex: {structureSpline.GetPosition(0)}");

            // Get the first structure point Y for relative adjustment
            var firstStructurePointLocalY = structureSpline.GetPosition(0).y;

            for (int i = 0; i < structureSpline.GetPointCount() - 1; i++)
            {
                var localPos = structureSpline.GetPosition(i);
                var realPos = new Vector3(
                    offsetX + localPos.x,
                    offsetY + (localPos.y - firstStructurePointLocalY), // shift whole structure relative to skeleton Y
                    0
                );

                var newPoint = new TerrainPoint
                {
                    Position = realPos,
                    PointTangentType = structureSpline.GetTangentMode(i),
                    TangentLeft = structureSpline.GetLeftTangent(i),
                    TangentRight = structureSpline.GetRightTangent(i)
                };

                skeleton.AddPoint(newPoint);
            }

        }

        public void InitDecorationsFromPrefab(Vector2 startingWorldPos, GameObject structurePrefab, GameObject parentContainer,
            bool isFirst = false)
        {
            try
            {
                var spline = structurePrefab.GetComponent<SpriteShapeController>().spline;
                var firstStructurePointLocalY = spline.GetPosition(0).y;
                var secondStructurePointLocalY = spline.GetPosition(1).y;

                float offsetX = 0, offsetY = 0;
                if (isFirst)
                {
                    offsetX = startingWorldPos.x;
                    offsetY = startingWorldPos.y;
                }
                else
                {
                    offsetX = startingWorldPos.x + DistanceBetweenPoint;
                    offsetY = startingWorldPos.y - firstStructurePointLocalY;
                }

                //Debug.Log($"[SkeletonTerrainGenerator] Initializing decorations from prefab. World Position: {startingWorldPos}," +
                //    $" Structure Prefab: {structurePrefab.name}, isFirst: {isFirst}");
                //Debug.Log($"[SkeletonTerrainGenerator] Offset x: {offsetX}, offset Y: {offsetY}, first structure point local Y:" +
                //    $" {firstStructurePointLocalY}, second structure point local Y: {spline.GetPosition(1)}");

                parentContainer = GetParentContainerForDecoration(parentContainer);
                var decorations = GetListOfDecorationTransforms(structurePrefab);
                foreach (var decoration in decorations)
                {
                    var x = decoration.transform.localPosition.x + offsetX;
                    var y = decoration.transform.localPosition.y + offsetY; // Adjust Y based on the first structure point
                    //Debug.Log($"[SkeletonTerrainGenerator] Decoration local position: {decoration.transform.localPosition}, adjusted position: ({x}, {y})");
                    Vector3 spawnPos = new Vector3(
                        x, y, decoration.transform.localPosition.z
                    );
                    //Debug.Log($"[SkeletonTerrainGenerator] Spawned {decoration.name} at {spawnPos}");
                    TerrainSprinker.Instance.PlaceDecoration(spawnPos, parentContainer, decoration.gameObject, Quaternion.identity);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SkeletonTerrainGenerator] Error: {e.Message}");
            }
        }

        private GameObject GetParentContainerForDecoration(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return TerrainSprinker.Instance.ParentContainer;
            }
            ;
            return gameObject;
        }

        /// <summary>
        /// Only call this function after you appended the structure itself
        /// <br></br>
        /// Appends all direct children of the structure prefab as decorations.
        /// </summary>
        /// <param name="structurePrefab"></param>
        public List<Transform> GetListOfDecorationTransforms(GameObject structurePrefab)
        {
            TerrainPrefabDecorController decorController = structurePrefab.GetComponent<TerrainPrefabDecorController>();
            if (decorController != null && decorController.IsThisPrefabHasChildren(structurePrefab.transform))
            {
                return decorController.ExtractDecorations(structurePrefab.transform);
            }
            else
            {
                Debug.LogWarning("[SkeletonTerrainGenerator] TerrainPrefabDecorController not found on the structure prefab.");
                return new();
            }
        }

    }
}
