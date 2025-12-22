using UnityEngine;
using System.Collections.Generic;

namespace GameBerry
{
    [System.Serializable]
    public class ComboData
    {
        public int ComboCount;
        public int ConditionIndex;
    }

    [CreateAssetMenu(fileName = "ComboData", menuName = "Table/ComboData", order = 1)]
    public class ComboDataAsset : ScriptableObject
    {
        public List<ComboData> comboDatas = new List<ComboData>();

        //------------------------------------------------------------------------------------
        public ComboData GetData(int count)
            => comboDatas.Find(x => x.ComboCount == count);
        //------------------------------------------------------------------------------------
    }
}