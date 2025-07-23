using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

namespace Assets.Scripts.Helpers
{
    public class TerrainHelper : MonoBehaviour
    {
        public float minDistanceBetweenObjects = 0.1f;  // Adjust as needed (0 to 1 range)

        public static TerrainHelper Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

    }
}
