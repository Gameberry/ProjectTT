using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class BattleSceneMap : MonoBehaviour
    {
        public List<BattleSceneMap_Aggro> _battleSceneMap_Aggros = new List<BattleSceneMap_Aggro>();

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

        public void SpawnMonster(int count)
        {
            for (int i = 0; i < _battleSceneMap_Aggros.Count; ++i)
            {
                _battleSceneMap_Aggros[i].SpawnMonster(count);
            }
        }

        public void ReleaseAllMonsters()
        {
            for (int i = 0; i < _battleSceneMap_Aggros.Count; ++i)
            {
                _battleSceneMap_Aggros[i].ReleaseAllMonsters();
            }
        }
    }
}
