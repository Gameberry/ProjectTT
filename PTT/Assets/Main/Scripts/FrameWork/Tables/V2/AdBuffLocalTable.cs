using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class AdBuffActiveData
    {
        public int Index;
        public int ResourceIndex;

        public V2Enum_BuffEffectType BuffEffectType;
        public double BuffValue;
        public double BuffDuration;
    }

    public class AdBuffLocalTable : LocalTableBase
    {
        private Dictionary<int, AdBuffActiveData> m_adBuffActiveData = new Dictionary<int, AdBuffActiveData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("AdBuff", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                AdBuffActiveData addBuffActiveData = new AdBuffActiveData();

                addBuffActiveData.Index = rows[i]["Index"].ToString().FastStringToInt();
                addBuffActiveData.ResourceIndex = rows[i]["ResourceIndex"].ToString().FastStringToInt();

                addBuffActiveData.BuffEffectType = rows[i]["BuffEffectType"].ToString().ToInt().IntToEnum32<V2Enum_BuffEffectType>();
                addBuffActiveData.BuffValue = rows[i]["BuffValue"].ToString().ToDouble();
                addBuffActiveData.BuffDuration = rows[i]["BuffDuration"].ToString().ToDouble();

                m_adBuffActiveData.Add(addBuffActiveData.Index, addBuffActiveData);
            }
        }
        //------------------------------------------------------------------------------------
        public Dictionary<int, AdBuffActiveData> GetAllBuffActivaData()
        {
            return m_adBuffActiveData;
        }
        //------------------------------------------------------------------------------------
        public AdBuffActiveData GetBuffActiveData(int index)
        {
            if (m_adBuffActiveData.ContainsKey(index) == true)
                return m_adBuffActiveData[index];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}