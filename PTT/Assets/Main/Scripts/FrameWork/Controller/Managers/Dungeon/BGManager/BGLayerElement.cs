using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    [System.Serializable]
    public class BGGroup
    {
        public int Index;
        public List<Transform> BG = new List<Transform>();
    }

    public class BGLayerElement : MonoBehaviour
    {
        public List<BGGroup> BGImage = new List<BGGroup>();
    }
}