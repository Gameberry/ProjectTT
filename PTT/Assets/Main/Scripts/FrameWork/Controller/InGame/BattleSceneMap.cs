using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class BattleSceneMap : MonoBehaviour
    {
        // 뭐 없어서 우선 요정도만
        public List<GameObject> Map = new List<GameObject>();

        public void SetMap(int index)
        {
            if (Map.Count <= 0)
                return;

            int setidx = Mathf.Clamp(index, 0, Map.Count);

            for (int i = 0; i < Map.Count; ++i)
            {
                Map[i].gameObject.SetActive(i == setidx);
            }
        }
    }
}