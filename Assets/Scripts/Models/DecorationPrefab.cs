using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Models
{
    [Serializable]
    public class DecorationPrefab
    {
        public GameObject Prefab;
        public bool IsAlignToTerrain = true;
        public int ObjectCount = 1;
        public bool IsRandomSize = false;
    }
}
