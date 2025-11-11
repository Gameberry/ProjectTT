using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class VipPackageData
    {
        public ObscuredInt Index;

        public V2Enum_IntervalType DurationType;
        public ObscuredInt DurationParam;

        public ObscuredInt IgnoreAd;
        public ObscuredInt IsSpeedUp;

        public ObscuredInt ReceiveDiaEveryday;
        public ObscuredInt ReceiveDiaIndex;

        public ObscuredInt AdBuffCountIncrease;

        public ObscuredInt IsOpenResearchSlot;

        public ObscuredInt AdBuffAlways;

        public ObscuredInt SweepUnlimited;

        public string MailTitleLocalStringKey;
        public string MailDescLocalStringKey;

        public string NameLocalKey;
    }

    public class VipPackageShopData : ShopDataBase
    {

        public V2Enum_OpenConditionType OpenConditionType;
        public ObscuredInt OpenConditionParam;

        public string NameLocalKey;
    }

    public class VipPackageLocalTable : LocalTableBase
    {
        private Dictionary<ObscuredInt, VipPackageData> _vipPackageDatas = new Dictionary<ObscuredInt, VipPackageData>();
        private List<VipPackageShopData> _shopPackageEvents = new List<VipPackageShopData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("VipPackage", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);


            for (int i = 0; i < rows.Count; ++i)
            {
                VipPackageData vipPackageData = new VipPackageData();

                vipPackageData.Index = rows[i]["Index"].ToString().ToInt();

                vipPackageData.DurationType = rows[i]["DurationType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
                vipPackageData.DurationParam = rows[i]["DurationParam"].ToString().ToInt();

                vipPackageData.IgnoreAd = rows[i]["IgnoreAd"].ToString().ToInt();
                vipPackageData.IsSpeedUp = rows[i]["IsSpeedUp"].ToString().ToInt();

                vipPackageData.ReceiveDiaEveryday = rows[i]["ReceiveDiaEveryday"].ToString().ToInt();
                vipPackageData.ReceiveDiaIndex = rows[i]["ReceiveDiaIndex"].ToString().ToInt();

                vipPackageData.AdBuffCountIncrease = rows[i]["AdBuffCountIncrease"].ToString().ToInt();

                vipPackageData.IsOpenResearchSlot = rows[i]["IsOpenResearchSlot"].ToString().ToInt();

                vipPackageData.AdBuffAlways = rows[i]["AdBuffAlways"].ToString().ToInt();

                vipPackageData.SweepUnlimited = rows[i]["SweepUnlimited"].ToString().ToInt();

                vipPackageData.MailTitleLocalStringKey = string.Format("vipPackage/mailTitle/{0}", vipPackageData.Index);
                vipPackageData.MailDescLocalStringKey = string.Format("vipPackage/mailDesc/{0}", vipPackageData.Index);

                vipPackageData.NameLocalKey = string.Format("vippackagename/{0}", vipPackageData.Index);

                if (_vipPackageDatas.ContainsKey(vipPackageData.Index) == false)
                {
                    _vipPackageDatas.Add(vipPackageData.Index, vipPackageData);
                }
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("VipPackageShop", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);


            for (int i = 0; i < rows.Count; ++i)
            {
                VipPackageShopData vipPackageShopData = new VipPackageShopData();
                vipPackageShopData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                vipPackageShopData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                vipPackageShopData.MenuType = rows[i]["MenuType"].ToString().ToInt().IntToEnum32<V2Enum_ShopMenuType>();
                vipPackageShopData.TagString = rows[i]["TagString"].ToString();
                vipPackageShopData.DisplayType = rows[i]["DisplayType"].ToString().ToInt().IntToEnum32<V2Enum_DisplayType>();

                vipPackageShopData.IntervalType = rows[i]["IntervalType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
                vipPackageShopData.IntervalParam = rows[i]["IntervalParam"].ToString().ToInt();

                vipPackageShopData.OpenConditionType = rows[i]["OpenConditionType"].ToString().ToInt().IntToEnum32<V2Enum_OpenConditionType>();
                vipPackageShopData.OpenConditionParam = rows[i]["OpenConditionParam"].ToString().ToInt();

                vipPackageShopData.PID = rows[i]["PID"].ToString();


                for (int rewardidx = 1; rewardidx <= 2; ++rewardidx)
                {
                    int idx = rows[i][string.Format("ReturnGoodsParam{0}1", rewardidx)].ToString().ToInt();
                    if (idx == -1)
                        continue;
                    double amount = rows[i][string.Format("ReturnGoodsParam{0}2", rewardidx)].ToString().ToDouble();

                    RewardData rewardData = new RewardData();
                    rewardData.Index = idx;
                    rewardData.Amount = amount;
                    vipPackageShopData.ShopRewardData.Add(rewardData);
                }


                vipPackageShopData.PriceKR = rows[i]["PriceKR"].ToString().ToInt();

                vipPackageShopData.Description = rows[i]["Description"].ToString();

                vipPackageShopData.PackageIconStringKey = string.Format("Icon/shop/vippackage/{0}", vipPackageShopData.ResourceIndex);

                vipPackageShopData.TitleLocalStringKey = string.Format("vipPackageShop/title/{0}", vipPackageShopData.ResourceIndex);
                vipPackageShopData.SubTitleLocalStringKey = string.Format("vipPackageShop/subTitle/{0}", vipPackageShopData.ResourceIndex);
                vipPackageShopData.MailTitleLocalStringKey = string.Format("vipPackageShop/mailTitle/{0}", vipPackageShopData.ResourceIndex);
                vipPackageShopData.MailDescLocalStringKey = string.Format("vipPackageShop/mailDesc/{0}", vipPackageShopData.ResourceIndex);

                ShopOperator.AddStoreDataBase(vipPackageShopData);

                _shopPackageEvents.Add(vipPackageShopData);
            }

        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, VipPackageData> GetVipPackageDatas()
        {
            return _vipPackageDatas;
        }
        //------------------------------------------------------------------------------------
        public VipPackageData GetVipPackageData(int index)
        {
            if (_vipPackageDatas.ContainsKey(index) == true)
            {
                return _vipPackageDatas[index];
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<VipPackageShopData> GetVipPackageShopDatas()
        {
            return _shopPackageEvents;
        }
        //------------------------------------------------------------------------------------
    }
}