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

    public class BoxLocalTable : LocalTableBase
    {
        private Dictionary<ObscuredInt, BoxData> m_boxData_Dic = new Dictionary<ObscuredInt, BoxData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Box", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                BoxData boxData = new BoxData();
                boxData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                boxData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                List<V2Enum_Goods> v2Enum_Goodslist = JsonConvert.DeserializeObject<List<V2Enum_Goods>>(rows[i]["arrGoodsType"].ToString());
                List<int> indexlist = JsonConvert.DeserializeObject<List<int>>(rows[i]["arrGoodsIndex"].ToString());
                List<double> amountlist = JsonConvert.DeserializeObject<List<double>>(rows[i]["arrGoodsAmount"].ToString());

                for (int index = 0; index < v2Enum_Goodslist.Count; ++index)
                {
                    if (indexlist.Count > index && v2Enum_Goodslist.Count > index)
                    {
                        RewardData rewardData = new RewardData();
                        rewardData.V2Enum_Goods = v2Enum_Goodslist[index];
                        rewardData.Index = indexlist[index];
                        rewardData.Amount = amountlist[index];

                        boxData.RewardDatas.Add(rewardData);
                    }
                }

                boxData.BoxType = rows[i]["arrBoxType"].ToString().ToInt().IntToEnum32<V2Enum_BoxType>();

                if (boxData.BoxType == V2Enum_BoxType.RandomTypeBox)
                {
                    boxData.RandomTypeBoxReward = new List<BoxComponentsData>();
                    boxData.weightedRandomPicker_RandomTypeBox = new WeightedRandomPicker<BoxComponentsData>();

                    List<double> Weights = JsonConvert.DeserializeObject<List<double>>(rows[i]["arrGoodsProb"].ToString());

                    for (int index = 0; index < Weights.Count; ++index)
                    {
                        if (boxData.RewardDatas.Count > index && Weights.Count > index)
                        {
                            BoxComponentsData randomBoxRewardData = new BoxComponentsData();
                            randomBoxRewardData.rewardData = boxData.RewardDatas[index];
                            randomBoxRewardData.GoodsProb = Weights[index];

                            boxData.weightedRandomPicker_RandomTypeBox.Add(randomBoxRewardData, randomBoxRewardData.GoodsProb);
                            boxData.RandomTypeBoxReward.Add(randomBoxRewardData);
                        }
                    }
                }


                boxData.NameLocalStringKey = string.Format("box/{0}/name", boxData.ResourceIndex);
                boxData.IconStringKey = string.Format("box/{0}/icon", boxData.ResourceIndex);

                m_boxData_Dic.Add(boxData.Index, boxData);
            }
        }
        //------------------------------------------------------------------------------------
        public BoxData GetBoxData(ObscuredInt index)
        {
            if (m_boxData_Dic.ContainsKey(index) == true)
                return m_boxData_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, BoxData> GetAllBoxData()
        {
            return m_boxData_Dic;
        }
        //------------------------------------------------------------------------------------
    }
}