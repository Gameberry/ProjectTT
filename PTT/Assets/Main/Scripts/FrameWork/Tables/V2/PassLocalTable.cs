using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using Gpm.Ui;

namespace GameBerry
{
    public class PassData : ShopDataBase
    {
        public V2Enum_PassType PassType;
        public ObscuredInt PassStep;

        public ObscuredInt PurchaseRewardGoodsParam11;
        public ObscuredDouble PurchaseRewardGoodsParam12;

        public ObscuredInt ConditionRewardGroupIndex;

        public double IsMinClearParam;
        public double IsMaxClearParam;

        public string PassDescStringKey;

        public List<PassConditionRewardData> passConditionRewardDatas = new List<PassConditionRewardData>();

        public ObscuredInt DisplayGoodsParam11;
        public ObscuredDouble DisplayGoodsParam12;

        public ObscuredInt DisplayGoodsParam21;
        public ObscuredDouble DisplayGoodsParam22;


        public List<RewardData> DisplayRewardData = new List<RewardData>();
    }

    public class PassConditionRewardData : InfiniteScrollData
    {
        public ObscuredInt Index;
        public ObscuredInt ConditionRewardGroupIndex;

        public ObscuredInt ConditionRewardOrder;

        public V2Enum_PassType PassType;
        public ObscuredInt ConditionClearParam;

        public ObscuredInt FreeRewardGoodsParam1;
        public ObscuredDouble FreeRewardGoodsParam2;

        public ObscuredInt PaidRewardGoodsParam1;
        public ObscuredDouble PaidRewardGoodsParam2;
    }

    public class PassLocalTable : LocalTableBase
    {
        private Dictionary<V2Enum_PassType, List<PassData>> m_passLists = new Dictionary<V2Enum_PassType, List<PassData>>();
        private Dictionary<ObscuredInt, PassData> passDataRewardGroup_Dic = new Dictionary<ObscuredInt, PassData>();

        public ObscuredInt berserkerKillPass = 0;

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("PassConditionReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            List<PassConditionRewardData> passConditionRewardData = new List<PassConditionRewardData>();
            for (int i = 0; i < rows.Count; ++i)
            {
                PassConditionRewardData passData = new PassConditionRewardData();

                passData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                passData.ConditionRewardGroupIndex = rows[i]["ConditionRewardGroupIndex"].ToString().ToInt();

                passData.ConditionRewardOrder = rows[i]["ConditionRewardOrder"].ToString().ToInt();

                passData.PassType = rows[i]["PassType"].ToString().ToInt().IntToEnum32<V2Enum_PassType>();
                passData.ConditionClearParam = rows[i]["ConditionClearParam"].ToString().ToInt();

                //passData.FreeRewardGoodsType = rows[i]["FreeRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                passData.FreeRewardGoodsParam1 = rows[i]["FreeRewardGoodsParam1"].ToString().ToInt();
                passData.FreeRewardGoodsParam2 = rows[i]["FreeRewardGoodsParam2"].ToString().ToDouble();

                //passData.PaidRewardGoodsType = rows[i]["PaidRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                passData.PaidRewardGoodsParam1 = rows[i]["PaidRewardGoodsParam1"].ToString().ToInt();
                passData.PaidRewardGoodsParam2 = rows[i]["PaidRewardGoodsParam2"].ToString().ToDouble();

                passConditionRewardData.Add(passData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Pass", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                PassData passData = new PassData();

                passData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                passData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                passData.PassType = rows[i]["PassType"].ToString().ToInt().IntToEnum32<V2Enum_PassType>();
                passData.PassStep = rows[i]["PassStep"].ToString().ToInt();

                passData.TagString = rows[i]["TagString"].ToString();

                passData.IntervalType = rows[i]["IntervalType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
                passData.IntervalParam = rows[i]["IntervalParam"].ToString().ToInt();

                passData.PID = rows[i]["PID"].ToString();

                passData.PriceKR = rows[i]["PriceKR"].ToString().ToInt();

                passData.Description = rows[i]["Description"].ToString();

                passData.PurchaseRewardGoodsParam11 = rows[i]["PurchaseRewardGoodsParam11"].ToString().ToInt();
                passData.PurchaseRewardGoodsParam12 = rows[i]["PurchaseRewardGoodsParam12"].ToString().ToDouble();

                passData.ConditionRewardGroupIndex = rows[i]["ConditionRewardGroupIndex"].ToString().ToInt();

                passData.TitleLocalStringKey = string.Format("pass/{0}/title", passData.ResourceIndex);
                passData.SubTitleLocalStringKey = string.Format("pass/{0}/subTitle", passData.ResourceIndex);
                passData.MailTitleLocalStringKey = string.Format("pass/{0}/mailTitle", passData.ResourceIndex);
                passData.MailDescLocalStringKey = string.Format("pass/{0}/mailDesc", passData.ResourceIndex);
                passData.PassDescStringKey = string.Format("pass/{0}/subTitle", passData.ResourceIndex);


                passData.DisplayGoodsParam11 = rows[i]["DisplayGoodsParam11"].ToString().ToInt();
                passData.DisplayGoodsParam12 = rows[i]["DisplayGoodsParam12"].ToString().ToDouble();

                if (passData.DisplayGoodsParam11 != -1)
                {
                    RewardData baseRewardData = new RewardData();
                    baseRewardData.Index = passData.DisplayGoodsParam11;
                    baseRewardData.Amount = passData.DisplayGoodsParam12;

                    passData.DisplayRewardData.Add(baseRewardData);
                }

                passData.DisplayGoodsParam21 = rows[i]["DisplayGoodsParam21"].ToString().ToInt();
                passData.DisplayGoodsParam22 = rows[i]["DisplayGoodsParam22"].ToString().ToDouble();

                if (passData.DisplayGoodsParam21 != -1)
                {
                    RewardData baseRewardData = new RewardData();
                    baseRewardData.Index = passData.DisplayGoodsParam21;
                    baseRewardData.Amount = passData.DisplayGoodsParam22;

                    passData.DisplayRewardData.Add(baseRewardData);
                }



                if (passData.PurchaseRewardGoodsParam12 > 0)
                {
                    RewardData baseRewardData = new RewardData();
                    baseRewardData.Index = passData.PurchaseRewardGoodsParam11;
                    baseRewardData.Amount = passData.PurchaseRewardGoodsParam12;

                    passData.ShopRewardData.Add(baseRewardData);
                }

                passData.passConditionRewardDatas = passConditionRewardData.FindAll(x => x.ConditionRewardGroupIndex.GetDecrypted() == passData.ConditionRewardGroupIndex.GetDecrypted());
                passData.passConditionRewardDatas.Sort((x, y) =>
                {
                    if (x.ConditionRewardOrder.GetDecrypted() > y.ConditionRewardOrder.GetDecrypted())
                        return 1;
                    else
                        return -1;
                });

                if (passData.passConditionRewardDatas.Count > 0)
                {
                    passData.IsMinClearParam = passData.passConditionRewardDatas[0].ConditionClearParam.GetDecrypted();
                    passData.IsMaxClearParam = passData.passConditionRewardDatas[passData.passConditionRewardDatas.Count - 1].ConditionClearParam.GetDecrypted();
                }

                if (m_passLists.ContainsKey(passData.PassType) == false)
                    m_passLists.Add(passData.PassType, new List<PassData>());

                m_passLists[passData.PassType].Add(passData);

                ShopOperator.AddStoreDataBase(passData);

                if (passDataRewardGroup_Dic.ContainsKey(passData.ConditionRewardGroupIndex) == false)
                    passDataRewardGroup_Dic.Add(passData.ConditionRewardGroupIndex, passData);
            }

            foreach (var pair in m_passLists)
            {
                pair.Value.Sort((x, y) =>
                {
                    if (x.PassStep.GetDecrypted() > y.PassStep.GetDecrypted())
                        return 1;
                    else
                        return -1;
                });
            }
        }
        //------------------------------------------------------------------------------------
        public PassData GetPassData(int index)
        {
            foreach (var pair in m_passLists)
            {
                PassData passData = pair.Value.Find(x => x.Index == index);
                if (passData != null)
                    return passData;
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public PassData GetPassData_RewardIndex(int rewardIndex)
        {
            if (passDataRewardGroup_Dic.ContainsKey(rewardIndex) == true)
                return passDataRewardGroup_Dic[rewardIndex];

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<PassData> GetPassDatas(V2Enum_PassType v2Enum_PassType)
        {
            if (m_passLists.ContainsKey(v2Enum_PassType) == true)
                return m_passLists[v2Enum_PassType];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}