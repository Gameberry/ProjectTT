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
    public class SummonCostData
    {
        public V2Enum_SummonType SummonType;

        public ObscuredInt summonCount;
    }

    public class SummonData
    {
        public ObscuredInt Index;
        public ObscuredInt ResourceIndex;

        public V2Enum_SummonType SummonType;

        public ObscuredInt SummonCostParam11;
        public ObscuredDouble SummonCostParam12;
        public ObscuredInt SummonCostParam13;
        public ObscuredDouble SummonCostParam14;


        public List<SummonCostData> SummonCostDatas = new List<SummonCostData>();

        public ObscuredInt SummonCountViaAd;
        public ObscuredInt SummonBonusCountViaAd;
        public ObscuredInt MaxAdViewCountPerDate;
        public ObscuredInt SummonMaxCountViaAd;

        public string NameLocalStringKey;
        public string IconStringKey;
    }

    public class SummonElementData
    {
        public ObscuredInt Index;
        //public V2Enum_Grade Grade;
        public ObscuredInt GoodsIndex;
        public ObscuredDouble GoodsValue;

        public ObscuredDouble GoodsProb = 0;
    }

    public class SummonGroupData
    {
        public V2Enum_SummonType SummonType;
        public List<SummonElementData> SummonElementDatas = new List<SummonElementData>();
        public ObscuredDouble TotalWeight;

        public WeightedRandomPicker<SummonElementData> WeightedRandomPicker = new WeightedRandomPicker<SummonElementData>();
    }

    public class SummonForGuideData
    {
        public int Index;
        public int ResourceIndex;

        public V2Enum_SummonType SummonType;
        public string ReturnGoods;
        public List<ObscuredInt> ReturnGoodsList = new List<ObscuredInt>();
    }

    public class SummonConfirmCountData
    {
        public V2Enum_SummonType SummonType;
        public ObscuredInt RequiredCount;
    }

    public class SummonLocalTable : LocalTableBase
    {
        private Dictionary<V2Enum_SummonType, SummonData> m_summonDatas_Dic = new Dictionary<V2Enum_SummonType, SummonData>();
        private ConcurrentDictionary<V2Enum_SummonType, SummonGroupData> m_summonGroupDatas_Dic = new ConcurrentDictionary<V2Enum_SummonType, SummonGroupData>();
        public List<SummonForGuideData> summonForGuideDatas = new List<SummonForGuideData>();

        private Dictionary<V2Enum_SummonType, SummonConfirmCountData> _summonConfirmCountData_Dic = new Dictionary<V2Enum_SummonType, SummonConfirmCountData>();
        private Dictionary<V2Enum_SummonType, SummonGroupData> _summonConfirmDrawGroupData_Dic = new Dictionary<V2Enum_SummonType, SummonGroupData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Summon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SummonData summonData = new SummonData();
                summonData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                summonData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                summonData.SummonType = rows[i]["SummonType"].ToString().ToInt().IntToEnum32<V2Enum_SummonType>();


                summonData.SummonCostParam11 = rows[i]["SummonCostParam11"].ToString().ToInt();
                summonData.SummonCostParam12 = rows[i]["SummonCostParam12"].ToString().ToDouble();
                summonData.SummonCostParam13 = rows[i]["SummonCostParam13"].ToString().ToInt();
                summonData.SummonCostParam14 = rows[i]["SummonCostParam14"].ToString().ToDouble();


                SummonCostData summonCostData1 = new SummonCostData();
                summonCostData1.SummonType = summonData.SummonType;
                summonCostData1.summonCount = rows[i]["SummonCount1"].ToString().ToInt();
                summonData.SummonCostDatas.Add(summonCostData1);

                int sumcount2 = rows[i]["SummonCount2"].ToString().ToInt();
                if (sumcount2 > 0)
                {
                    SummonCostData summonCostData2 = new SummonCostData();
                    summonCostData2.SummonType = summonData.SummonType;
                    summonCostData2.summonCount = sumcount2;

                    summonData.SummonCostDatas.Add(summonCostData2);
                }

                summonData.SummonCountViaAd = rows[i]["SummonCountViaAd"].ToString().ToInt();
                summonData.SummonBonusCountViaAd = rows[i]["SummonBonusCountViaAd"].ToString().ToInt();
                summonData.MaxAdViewCountPerDate = rows[i]["MaxAdViewCountPerDate"].ToString().ToInt();
                summonData.SummonMaxCountViaAd = rows[i]["SummonMaxCountViaAd"].ToString().ToInt();

                summonData.NameLocalStringKey = string.Format("summon/{0}/name", summonData.ResourceIndex);
                summonData.IconStringKey = string.Format("summon/{0}/icon", summonData.ResourceIndex);

                m_summonDatas_Dic.Add(summonData.SummonType, summonData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SummonConfirmCount", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SummonConfirmCountData summonForGuideData = new SummonConfirmCountData();

                summonForGuideData.SummonType = rows[i]["SummonType"].ToString().ToInt().IntToEnum32<V2Enum_SummonType>();
                summonForGuideData.RequiredCount = rows[i]["RequiredCount"].ToString().ToInt();

                _summonConfirmCountData_Dic.Add(summonForGuideData.SummonType, summonForGuideData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SummonConfirmDraw", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                V2Enum_SummonType v2Enum_SummonType = rows[i]["SummonType"].ToString().ToInt().IntToEnum32<V2Enum_SummonType>();

                SummonGroupData summonConfirmDrawGroupData = null;
                if (_summonConfirmDrawGroupData_Dic.ContainsKey(v2Enum_SummonType) == false)
                {
                    summonConfirmDrawGroupData = new SummonGroupData();
                    _summonConfirmDrawGroupData_Dic.Add(v2Enum_SummonType, summonConfirmDrawGroupData);
                }
                else
                    summonConfirmDrawGroupData = _summonConfirmDrawGroupData_Dic[v2Enum_SummonType];


                SummonElementData summonElementData = new SummonElementData();
                summonElementData.Index = rows[i]["Index"].ToString().FastStringToInt();
                summonElementData.GoodsIndex = rows[i]["GoodsIndex"].ToString().FastStringToInt();
                summonElementData.GoodsValue = rows[i]["GoodsValue"].ToString().ToDouble();
                summonElementData.GoodsProb = rows[i]["GoodsProb"].ToString().ToDouble();

                summonConfirmDrawGroupData.SummonElementDatas.Add(summonElementData);
                summonConfirmDrawGroupData.TotalWeight += summonElementData.GoodsProb;

                summonConfirmDrawGroupData.WeightedRandomPicker.Add(summonElementData, summonElementData.GoodsProb);
            }

            await SetGroupDatas(V2Enum_SummonType.SummonGear);
            await SetGroupDatas(V2Enum_SummonType.SummonNormal);
            await SetGroupDatas(V2Enum_SummonType.SummonRelic);
            await SetGroupDatas(V2Enum_SummonType.SummonRune);
        }
        //------------------------------------------------------------------------------------
        private async UniTask SetGroupDatas(V2Enum_SummonType v2Enum_SummonType)
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart(v2Enum_SummonType.ToString(), o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            List<SummonElementData> summonElementDatas = new List<SummonElementData>();

            for (int i = 0; i < rows.Count; ++i)
            {
                SummonElementData summonElementData = new SummonElementData();
                summonElementData.Index = rows[i]["Index"].ToString().FastStringToInt();
                summonElementData.GoodsIndex = rows[i]["GoodsIndex"].ToString().FastStringToInt();
                summonElementData.GoodsValue = rows[i]["GoodsValue"].ToString().ToDouble();

                summonElementData.GoodsProb = rows[i]["GoodsProb"].ToString().ToDouble();

                summonElementDatas.Add(summonElementData);
            }

            if (m_summonGroupDatas_Dic.ContainsKey(v2Enum_SummonType) == true)
            {
                return;
            }

            SummonGroupData summonGroupData = new SummonGroupData();
            summonGroupData.SummonType = v2Enum_SummonType;
            m_summonGroupDatas_Dic.TryAdd(v2Enum_SummonType, summonGroupData);

            for (int i = 0; i < summonElementDatas.Count; ++i)
            {
                SummonElementData summonElementData = summonElementDatas[i];

                summonGroupData.SummonElementDatas.Add(summonElementData);
                double weight = summonElementData.GoodsProb.GetDecrypted();
                summonGroupData.TotalWeight += weight;
                summonGroupData.WeightedRandomPicker.Add(summonElementData, weight);
            }
        }
        //------------------------------------------------------------------------------------
        public Dictionary<V2Enum_SummonType, SummonData> GetSummonDatas()
        {
            return m_summonDatas_Dic;
        }
        //------------------------------------------------------------------------------------
        public SummonData GetSummonData(V2Enum_SummonType v2Enum_SummonType)
        {
            if (m_summonDatas_Dic.ContainsKey(v2Enum_SummonType) == true)
                return m_summonDatas_Dic[v2Enum_SummonType];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SummonGroupData GetSummonGroupData(V2Enum_SummonType v2Enum_SummonType)
        {
            if (m_summonGroupDatas_Dic.ContainsKey(v2Enum_SummonType) == true)
            {
                return m_summonGroupDatas_Dic[v2Enum_SummonType];
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public SummonConfirmCountData GetSummonConfirmCountData(V2Enum_SummonType v2Enum_SummonType)
        {
            if (_summonConfirmCountData_Dic.ContainsKey(v2Enum_SummonType) == true)
            {
                return _summonConfirmCountData_Dic[v2Enum_SummonType];
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public SummonGroupData GetSummonConfirmDrawGroupData(V2Enum_SummonType v2Enum_SummonType)
        {
            if (_summonConfirmDrawGroupData_Dic.ContainsKey(v2Enum_SummonType) == true)
            {
                return _summonConfirmDrawGroupData_Dic[v2Enum_SummonType];
            }

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}