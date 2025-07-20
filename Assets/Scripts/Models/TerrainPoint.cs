using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

namespace Assets.Scripts.Models
{
    public class TerrainPoint
    {
        public Vector2 Position { get; set; } // The position itself
        public string PositionTrend { get; set; } = ""; // Increasing , Decreasing, Constant
        public ShapeTangentMode PointTangentType { get; set; } // use the one that Unity has
        public Vector3 TangentLeft { get; set; }
        public Vector3 TangentRight { get; set; }
    }
}
