using Assets.Scripts.Helpers;
using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;

namespace Assets.Scripts.Terrain
{
    public class TerrainSkeleton
    {
        public SkeletonTerrainGeneratorHelper TerrainGenerator { get; set; } = new();
        public List<TerrainPoint> Points { get; set; } = new();
        public int PointOffSet { get; set; }
        private readonly int DistanceBetweenPoint;

        public TerrainSkeleton(int distanceBetweenPoint = 80)
        {
            DistanceBetweenPoint = distanceBetweenPoint;
            TerrainGenerator = new SkeletonTerrainGeneratorHelper()
            {
                DistanceBetweenPoint = distanceBetweenPoint
            };
        }


        // Helper function to interact with this one
        public List<TerrainPoint> ConvertExistingSplineIntoSkeleton(SpriteShapeController ssc)
        {
            PointOffSet = ssc.spline.GetPointCount() - 1; // For use later

            //Debug.Log($"[TerrainChunk] Converting existing spline into skeleton. Spline Point Count: {ssc.spline.GetPointCount()}");
            Points.Clear();
            var spline = ssc.spline;
            // Read the spline and add all of them into our skeleton
            for (int i = 0; i < spline.GetPointCount(); i++)
            {
                var point = new TerrainPoint
                {
                    Position = spline.GetPosition(i),
                    PointTangentType = spline.GetTangentMode(i),
                    TangentLeft = spline.GetLeftTangent(i),
                    TangentRight = spline.GetRightTangent(i)
                };
                Points.Add(point);
            }
            return Points;
        }

        public void SmoothOutTerrainSkeletonPoint(float leftSmoothAmount, float rightSmoothAmount, int pointIndex)
        {
            // Smooth out the terrain by adjusting the tangents of the points
            if (pointIndex < 0 || pointIndex >= Points.Count)
            {
                Debug.LogWarning($"[TerrainChunk] Invalid point index: {pointIndex}");
                return;
            }

            // Set the tangent mode to continuous
            var point = Points[pointIndex];
            point.PointTangentType = ShapeTangentMode.Continuous;
            point.TangentLeft = new Vector3(-leftSmoothAmount, 0, 0);
            point.TangentRight = new Vector3(rightSmoothAmount, 0, 0);
        }

        // Make the beginning point Y match the last point Y
        public void AdjustBeginningPointToMatchLastPoint(int newPointIndex)
        {
            var newPoint = Points[newPointIndex];
            var beginningPoint = Points[0];

            beginningPoint.Position = new Vector3(beginningPoint.Position.x, newPoint.Position.y, 0);
            Points[0] = beginningPoint;

            //Debug.Log($"[TerrainChunk] Forced beginning point Y to {newPoint.Position.y}");
        }

        public void AdjustBeginningPointToMatchLastPoint(TerrainPoint newPoint)
        {
            var beginningPoint = Points[0];
            if (newPoint.Position.y < beginningPoint.Position.y)
            {
                //Debug.Log($"[TerrainChunk] Adjusting beginning point to match last point. New Point Y: {newPoint.Position.y}, Beginning Point Y: {beginningPoint.Position.y}");
                beginningPoint.Position = new Vector3(beginningPoint.Position.x, newPoint.Position.y, 0);
                UpdatePointAtIndex(0, beginningPoint);
            }
        }

        // Add a point to the end of the spline to close the terrain shape
        // This point is a little bit behind the point before it to create a cliff like effect
        public void AddClosingTerrainPointForSkeleton()
        {
            var beginningPoint = GetPointAtIndex(0);
            var finalPoint = GetLastPoint();
            var closingPointPos = new Vector3(finalPoint.Position.x - DistanceBetweenPoint, finalPoint.Position.y - 100f, 0);
            TerrainPoint closingPoint = new TerrainPoint
            {
                Position = closingPointPos,
                PointTangentType = ShapeTangentMode.Linear,
                TangentLeft = new Vector3(-10f, 0, 0), // Adjust as needed
                TangentRight = new Vector3(10f, 0, 0) // Adjust as needed
            };

            AddPoint(closingPoint);
            //Debug.Log($"[TerrainChunk] Added closing point at position {closingPoint.Position}");
        }

        public void AddPoint(TerrainPoint point)
        {
            Points.Add(point);
        }

        public void InsertPointAtIndex(int index, TerrainPoint point)
        {
            if (index < 0 || index > Points.Count)
            {
                throw new IndexOutOfRangeException("Index is out of range of the points list.");
            }
            Points.Insert(index, point);
        }

        public void ClearPoints()
        {
            Points.Clear();
        }

        public TerrainPoint GetPointAtIndex(int index)
        {
            if (index < 0 || index >= Points.Count)
            {
                throw new IndexOutOfRangeException("Index is out of range of the points list.");
            }
            return Points[index];
        }

        public TerrainPoint GetLastPoint()
        {
            if (Points.Count == 0)
            {
                return null;
            }
            return Points[Points.Count - 1];
        }

        public TerrainPoint GetBeforeLastPoint()
        {
            if (Points.Count < 2)
            {
                throw new InvalidOperationException("Not enough points available in the list.");
            }
            return Points[Points.Count - 2];
        }

        public int GetPointCount()
        {
            return Points.Count;
        }

        public void RemovePointAtIndex(int index)
        {
            if (index < 0 || index >= Points.Count)
            {
                throw new IndexOutOfRangeException("Index is out of range of the points list.");
            }
            Points.RemoveAt(index);
        }

        public void UpdatePointAtIndex(int index, TerrainPoint newPoint)
        {
            if (index < 0 || index >= Points.Count)
            {
                throw new IndexOutOfRangeException("Index is out of range of the points list.");
            }
            Points[index] = newPoint;
        }

        public int GetLastPointIndex()
        {
            if (Points.Count == 0)
            {
                throw new InvalidOperationException("No points available in the list.");
            }
            return Points.Count - 1;
        }

    }
}
