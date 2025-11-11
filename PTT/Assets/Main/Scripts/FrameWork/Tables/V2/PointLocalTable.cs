using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{

    public class PointLocalTable : LocalTableBase
    {
        private List<PointData> m_pointLocalTableDatas = new List<PointData>();
        private Dictionary<int, PointData> m_pointLocalTableDatas_Dic = null;

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Point", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            m_pointLocalTableDatas = JsonConvert.DeserializeObject<List<PointData>>(rows.ToJson());
            m_pointLocalTableDatas_Dic = new Dictionary<int, PointData>(m_pointLocalTableDatas.Count);

            for (int i = 0; i < m_pointLocalTableDatas.Count; ++i)
            {
                m_pointLocalTableDatas[i].NameLocalStringKey = string.Format("point/{0}/name", m_pointLocalTableDatas[i].ResourceIndex);
                m_pointLocalTableDatas[i].IconStringKey = string.Format("point/{0}/icon", m_pointLocalTableDatas[i].ResourceIndex);

                m_pointLocalTableDatas_Dic.Add(m_pointLocalTableDatas[i].Index, m_pointLocalTableDatas[i]);
            }

            m_pointLocalTableDatas.Sort((x, y) =>
            {
                if (x.DisplayOrder > y.DisplayOrder)
                    return 1;
                else
                    return -1;
            });
        }
        //------------------------------------------------------------------------------------
        public PointData GetData(int id)
        {
            if (m_pointLocalTableDatas_Dic.ContainsKey(id) == true)
                return m_pointLocalTableDatas_Dic[id];

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<PointData> GetAllData()
        {
            return m_pointLocalTableDatas;
        }
        //------------------------------------------------------------------------------------
    }
}