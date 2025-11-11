using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;

namespace GameBerry
{


    public class SummonTicketLocalTable : LocalTableBase
    {
        private Dictionary<ObscuredInt, SummonTicketData> m_summonTicketData_Dic = new Dictionary<ObscuredInt, SummonTicketData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SummonTicket", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SummonTicketData summonData = new SummonTicketData();
                summonData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                summonData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                summonData.SummonType = rows[i]["SummonType"].ToString().ToInt().IntToEnum32<V2Enum_SummonType>();
                summonData.ReturnGrade = rows[i]["ReturnGrade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                summonData.NameLocalStringKey = string.Format("summonTicket/{0}/name", summonData.ResourceIndex);

                m_summonTicketData_Dic.Add(summonData.Index, summonData);
            }
        }
        //------------------------------------------------------------------------------------
        public SummonTicketData GetSummonTicketData(ObscuredInt index)
        {
            if (m_summonTicketData_Dic.ContainsKey(index) == true)
                return m_summonTicketData_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, SummonTicketData> GetAllSummonTicketData()
        {
            return m_summonTicketData_Dic;
        }
        //------------------------------------------------------------------------------------
    }
}