using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class BuffData
    {
        public int Index;
        public int ResourceIndex;
        public V2Enum_TargetType BuffTargetType;
        public V2Enum_Stat BuffEffectType;
        public double BuffEffectValue;
        public int BuffEffectDuration; // 입력값 1 = 1분
    }

    public class BuffLocalTable : LocalTableBase
    {
        Dictionary<int, BuffData> m_buffDatas_Dic = new Dictionary<int, BuffData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Buff", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            List<BuffData> m_buffDatas = JsonConvert.DeserializeObject<List<BuffData>>(rows.ToJson());

            for (int i = 0; i < m_buffDatas.Count; ++i)
            {
                if (m_buffDatas_Dic.ContainsKey(m_buffDatas[i].Index) == true)
                    continue;

                m_buffDatas_Dic.Add(m_buffDatas[i].Index, m_buffDatas[i]);
            }
        }
        //------------------------------------------------------------------------------------
        public BuffData GetData(int index)
        {
            if (m_buffDatas_Dic.ContainsKey(index) == true)
                return m_buffDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}