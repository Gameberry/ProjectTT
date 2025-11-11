using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class PointData
    {
        public int Index;
        public int ResourceIndex;

        public int DisplayOrder;

        public int IsShow;

        public string NameLocalStringKey;
        public string IconStringKey;
    }

    public class BoxData
    {
        public int Index;
        public int ResourceIndex;

        public List<RewardData> RewardDatas = new List<RewardData>();

        public V2Enum_BoxType BoxType;
        public List<BoxComponentsData> RandomTypeBoxReward = null;
        public WeightedRandomPicker<BoxComponentsData> weightedRandomPicker_RandomTypeBox = null;

        public string NameLocalStringKey;
        public string IconStringKey;
    }

    public class BoxComponentsData
    {
        public RewardData rewardData = new RewardData();
        public double GoodsProb;
    }

    public class SummonTicketData
    {
        public ObscuredInt Index;
        public ObscuredInt ResourceIndex;

        public V2Enum_SummonType SummonType;

        public V2Enum_Grade ReturnGrade;

        public string NameLocalStringKey;
    }


    public class StackableLocalTable : LocalTableBase
    {
        private Dictionary<ObscuredInt, V2Enum_Goods> _goodsTypes = new Dictionary<ObscuredInt, V2Enum_Goods>();

        private List<PointData> m_pointLocalTableDatas = new List<PointData>();
        private Dictionary<int, PointData> m_pointLocalTableDatas_Dic = null;

        private Dictionary<ObscuredInt, BoxData> _boxData_Dic = new Dictionary<ObscuredInt, BoxData>();

        private Dictionary<ObscuredInt, SummonTicketData> m_summonTicketData_Dic = new Dictionary<ObscuredInt, SummonTicketData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GoodsType", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ObscuredInt ItemIndex = rows[i]["ItemIndex"].ToString().ToInt();
                V2Enum_Goods GoodsType = rows[i]["GoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                _goodsTypes.Add(ItemIndex, GoodsType);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("DefaultGoods", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ObscuredInt ItemIndex = rows[i]["GoodsIndex"].ToString().ToInt();
                ObscuredDouble GoodsType = rows[i]["GoodsValue"].ToString().ToDouble();
                ARRRContainer.DefaultGoods.Add(ItemIndex, GoodsType);
            }



            rows = null;

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


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Box", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                BoxData boxData = new BoxData();
                boxData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                boxData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                boxData.BoxType = rows[i]["BoxType"].ToString().ToInt().IntToEnum32<V2Enum_BoxType>();

                if (boxData.BoxType == V2Enum_BoxType.RandomTypeBox)
                {
                    boxData.RandomTypeBoxReward = new List<BoxComponentsData>();
                    boxData.weightedRandomPicker_RandomTypeBox = new WeightedRandomPicker<BoxComponentsData>();
                }

                boxData.NameLocalStringKey = string.Format("box/{0}/name", boxData.ResourceIndex);
                boxData.IconStringKey = string.Format("box/{0}/icon", boxData.ResourceIndex);

                _boxData_Dic.Add(boxData.Index, boxData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("BoxComponents", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                int boxIndex = rows[i]["BoxIndex"].ToString().ToInt();
                if (_boxData_Dic.ContainsKey(boxIndex) == false)
                    continue;

                BoxComponentsData boxComponentsData = new BoxComponentsData();

                boxComponentsData.rewardData.V2Enum_Goods = rows[i]["GoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                boxComponentsData.rewardData.Index = rows[i]["GoodsIndex"].ToString().ToInt();
                boxComponentsData.rewardData.Amount = rows[i]["GoodsAmount"].ToString().ToDouble();

                boxComponentsData.GoodsProb = rows[i]["GoodsProb"].ToString().ToDouble();

                BoxData boxData = _boxData_Dic[boxIndex];
                boxData.RewardDatas.Add(boxComponentsData.rewardData);

                if (boxData.BoxType == V2Enum_BoxType.RandomTypeBox)
                {
                    boxData.RandomTypeBoxReward.Add(boxComponentsData);
                    boxData.weightedRandomPicker_RandomTypeBox.Add(boxComponentsData, boxComponentsData.GoodsProb);
                }
            }




            rows = null;

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
        public V2Enum_Goods GetGoodsType(ObscuredInt index)
        {
            if (_goodsTypes.ContainsKey(index) == true)
                return _goodsTypes[index];

            return V2Enum_Goods.Max;
        }
        //------------------------------------------------------------------------------------
        public PointData GetPointData(int id)
        {
            if (m_pointLocalTableDatas_Dic.ContainsKey(id) == true)
                return m_pointLocalTableDatas_Dic[id];

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<PointData> GetAllPointData()
        {
            return m_pointLocalTableDatas;
        }
        //------------------------------------------------------------------------------------
        public BoxData GetBoxData(ObscuredInt index)
        {
            if (_boxData_Dic.ContainsKey(index) == true)
                return _boxData_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, BoxData> GetAllBoxData()
        {
            return _boxData_Dic;
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