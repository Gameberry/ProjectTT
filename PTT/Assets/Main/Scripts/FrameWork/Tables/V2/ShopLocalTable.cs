using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class ShopDataBase
    {
        public ObscuredInt Index;
        public ObscuredInt ResourceIndex;
        public V2Enum_ShopMenuType MenuType;
        public string TagString;
        public V2Enum_DisplayType DisplayType;
        public V2Enum_IntervalType IntervalType;
        public ObscuredInt IntervalParam;
        public ObscuredString PID = string.Empty;

        public int PriceKR;

        public string TitleLocalStringKey;
        public string SubTitleLocalStringKey;
        public string MailTitleLocalStringKey;
        public string MailDescLocalStringKey;

        public string PackageIconStringKey;

        public string Description;

        public List<RewardData> ShopRewardData = new List<RewardData>();
    }

    public class ShopDiamondChargeData : ShopDataBase
    {
        public V2Enum_OpenConditionType OpenConditionType;
        public ObscuredInt OpenConditionParam;

        public V2Enum_IntervalType BonusIntervalType;
        public ObscuredInt BonusIntervalParam;

        public ObscuredInt BaseGoodsParam1;
        public ObscuredDouble BaseGoodsParam2;
        public ObscuredInt BonusGoodsParam1;
        public ObscuredDouble BonusGoodsParam2;



        public List<RewardData> BonusGoods = new List<RewardData>();
    }

    public class ShopPackageData : ShopDataBase
    {
        public V2Enum_OpenConditionType OpenConditionType;
        public ObscuredInt OpenConditionParam;

        public ObscuredInt ReturnGoodsParam11;
        public ObscuredDouble ReturnGoodsParam12;

        public ObscuredInt ReturnGoodsParam21;
        public ObscuredDouble ReturnGoodsParam22;

        public ObscuredInt ReturnGoodsParam31;
        public ObscuredDouble ReturnGoodsParam32;

        public ObscuredInt ReturnGoodsParam41;
        public ObscuredDouble ReturnGoodsParam42;

        public ObscuredInt ReturnGoodsParam51;
        public ObscuredDouble ReturnGoodsParam52;
    }

    public class ShopPackageRelayData : ShopDataBase
    {
        public ObscuredInt RelayGroupIndex;
        public ObscuredInt GroupOrder;


        public V2Enum_OpenConditionType ProductActivateConditionType;
        public ObscuredInt ProductActivateConditionParam;


        public V2Enum_OpenConditionType ProductPurchaseConditionType1;
        public ObscuredInt ProductPurchaseConditionParam1;

        public V2Enum_OpenConditionType ProductPurchaseConditionType2;
        public ObscuredInt ProductPurchaseConditionParam2;


        public V2Enum_Goods ReturnGoodsType1;
        public ObscuredInt ReturnGoodsParam11;
        public ObscuredDouble ReturnGoodsParam12;

        public V2Enum_Goods ReturnGoodsType2;
        public ObscuredInt ReturnGoodsParam21;
        public ObscuredDouble ReturnGoodsParam22;

        public V2Enum_Goods ReturnGoodsType3;
        public ObscuredInt ReturnGoodsParam31;
        public ObscuredDouble ReturnGoodsParam32;

        public V2Enum_Goods ReturnGoodsType4;
        public ObscuredInt ReturnGoodsParam41;
        public ObscuredDouble ReturnGoodsParam42;

        public V2Enum_Goods ReturnGoodsType5;
        public ObscuredInt ReturnGoodsParam51;
        public ObscuredDouble ReturnGoodsParam52;

        public V2Enum_Goods SelectiveReturnGoodsType;
        public string SelectiveReturnGoodsParam1;
        public List<ObscuredInt> SelectiveReturnGoodsParam1_Goods = new List<ObscuredInt>();
        public ObscuredDouble SelectiveReturnGoodsParam2;

        public RewardData SelectRewardData = null;
    }

    public class ShopPackageRelayGroupData
    {
        public int RelayGroupIndex;
        public bool IsActive = false;
        public bool IsSoldOut = false;

        public ShopPackageRelayData FocusShopPackageData = null;

        public List<ShopPackageRelayData> ShopPackageRelayDatas = new List<ShopPackageRelayData>();
    }

    public class ShopPackageSpecialData : ShopDataBase
    {
        public V2Enum_OpenConditionType OpenConditionType;
        public ObscuredInt OpenConditionParam;

        public V2Enum_IntervalType DurationType;
        public ObscuredInt DurationParam;

        public V2Enum_Goods ReturnGoodsType1;
        public ObscuredInt ReturnGoodsParam11;
        public ObscuredDouble ReturnGoodsParam12;
        public ObscuredInt ReturnGoodsParam13;

        public V2Enum_Goods ReturnGoodsType2;
        public ObscuredInt ReturnGoodsParam21;
        public ObscuredDouble ReturnGoodsParam22;
        public ObscuredInt ReturnGoodsParam23;

        public V2Enum_Goods ReturnGoodsType3;
        public ObscuredInt ReturnGoodsParam31;
        public ObscuredDouble ReturnGoodsParam32;
        public ObscuredInt ReturnGoodsParam33;

        public V2Enum_Goods ReturnGoodsType4;
        public ObscuredInt ReturnGoodsParam41;
        public ObscuredDouble ReturnGoodsParam42;
        public ObscuredInt ReturnGoodsParam43;

        public V2Enum_Goods SelectiveReturnGoodsType;
        public string SelectiveReturnGoodsParam1;
        public List<ObscuredInt> SelectiveReturnGoodsParam1_Goods = new List<ObscuredInt>();
        public ObscuredDouble SelectiveReturnGoodsParam2;

        public RewardData SelectRewardData = null;
    }

    public class ShopPackageRotationData : ShopDataBase
    {
        public ObscuredInt RelayGroupIndex;
        public ObscuredInt GroupOrder;

        public V2Enum_OpenConditionType PurchaseConditionType;
        public ObscuredInt PurchaseConditionParam;

        public V2Enum_OpenConditionType OpenConditionType;
        public ObscuredInt OpenConditionParam;

        public V2Enum_Goods ReturnGoodsType1;
        public ObscuredInt ReturnGoodsParam11;
        public ObscuredDouble ReturnGoodsParam12;

        public V2Enum_Goods ReturnGoodsType2;
        public ObscuredInt ReturnGoodsParam21;
        public ObscuredDouble ReturnGoodsParam22;

        public V2Enum_Goods ReturnGoodsType3;
        public ObscuredInt ReturnGoodsParam31;
        public ObscuredDouble ReturnGoodsParam32;

        public V2Enum_Goods ReturnGoodsType4;
        public ObscuredInt ReturnGoodsParam41;
        public ObscuredDouble ReturnGoodsParam42;

        public ObscuredInt AllyCurrentStar;
    }

    public class ShopPackageRotateGroupData
    {
        public int RelayGroupIndex;
        public bool IsActive = false;
        public bool IsSoldOut = false;

        public V2Enum_IntervalType IntervalType;
        public ObscuredInt IntervalParam;

        public V2Enum_OpenConditionType OpenConditionType;
        public ObscuredInt OpenConditionParam;

        public ShopPackageRotationData FirstShopPackageData = null;

        public ShopPackageRotationData FocusShopPackageData = null;

        public List<ShopPackageRotationData> ShopPackageRelayDatas = new List<ShopPackageRotationData>();
    }

    public class ShopPackageEventData : ShopDataBase
    {
        public ObscuredInt IconResourceIndex;
        public string LobbyIconStringKey;
        

        public V2Enum_OpenConditionType OpenConditionType;
        public ObscuredInt OpenConditionParam;

        public V2Enum_IntervalType DurationType;
        public ObscuredInt DurationParam;
    }

    public class ShopIngameStoreData : ShopDataBase
    {
        public RewardData CostGoods = new RewardData();
        public RewardData ReturnGoods = new RewardData();
    }


    public class ShopLocalTable : LocalTableBase
    {
        private List<ShopDiamondChargeData> m_shopDiamondChargeDatas = new List<ShopDiamondChargeData>();
        private List<ShopPackageData> m_shopPackageDatas = new List<ShopPackageData>();
        private List<ShopPackageSpecialData> m_shopPackageSpecialDatas = new List<ShopPackageSpecialData>();

        private List<ShopPackageRotationData> m_shopPackageRotationDatas = new List<ShopPackageRotationData>();
        private List<ShopPackageEventData> m_shopPackageEvents = new List<ShopPackageEventData>();

        private List<ShopIngameStoreData> _shopIngameStoreDatas = new List<ShopIngameStoreData>();

        

        private Dictionary<int, ShopPackageRelayGroupData> m_shopPackageRelayGroupDatas = new Dictionary<int, ShopPackageRelayGroupData>();
        private Dictionary<int, ShopPackageRotateGroupData> m_shopPackageRotateGroupDatas = new Dictionary<int, ShopPackageRotateGroupData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("ShopDiamondCharge", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);


            for (int i = 0; i < rows.Count; ++i)
            {
                ShopDiamondChargeData shopDiamondChargeData = new ShopDiamondChargeData();
                shopDiamondChargeData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                shopDiamondChargeData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                shopDiamondChargeData.MenuType = rows[i]["MenuType"].ToString().ToInt().IntToEnum32<V2Enum_ShopMenuType>();
                shopDiamondChargeData.TagString = rows[i]["TagString"].ToString();

                shopDiamondChargeData.DisplayType = rows[i]["DisplayType"].ToString().ToInt().IntToEnum32<V2Enum_DisplayType>();
                shopDiamondChargeData.IntervalType = rows[i]["IntervalType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
                shopDiamondChargeData.IntervalParam = rows[i]["IntervalParam"].ToString().ToInt();

                shopDiamondChargeData.BonusIntervalType = rows[i]["BonusIntervalType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
                shopDiamondChargeData.BonusIntervalParam = rows[i]["BonusIntervalParam"].ToString().ToInt();
                
                shopDiamondChargeData.PID = rows[i]["PID"].ToString();

                shopDiamondChargeData.PriceKR = rows[i]["PriceKR"].ToString().ToInt();

                shopDiamondChargeData.Description = rows[i]["Description"].ToString();

                shopDiamondChargeData.TitleLocalStringKey = string.Format("shopDiamondCharge/{0}/title", shopDiamondChargeData.ResourceIndex);
                shopDiamondChargeData.SubTitleLocalStringKey = string.Format("shopDiamondCharge/{0}/subTitle", shopDiamondChargeData.ResourceIndex);
                shopDiamondChargeData.MailTitleLocalStringKey = string.Format("shopDiamondCharge/{0}/mailTitle", shopDiamondChargeData.ResourceIndex);
                shopDiamondChargeData.MailDescLocalStringKey = string.Format("shopDiamondCharge/{0}/mailDesc", shopDiamondChargeData.ResourceIndex);
                shopDiamondChargeData.PackageIconStringKey = string.Format("Icon/shop/dia/{0}", shopDiamondChargeData.ResourceIndex);

                shopDiamondChargeData.OpenConditionType = rows[i]["OpenConditionType"].ToString().ToInt().IntToEnum32<V2Enum_OpenConditionType>();
                shopDiamondChargeData.OpenConditionParam = rows[i]["OpenConditionParam"].ToString().ToInt();

                shopDiamondChargeData.BaseGoodsParam1 = rows[i]["BaseGoodsParam1"].ToString().ToInt();
                shopDiamondChargeData.BaseGoodsParam2 = rows[i]["BaseGoodsParam2"].ToString().ToDouble();

                shopDiamondChargeData.BonusGoodsParam1 = rows[i]["BonusGoodsParam1"].ToString().ToInt();
                shopDiamondChargeData.BonusGoodsParam2 = rows[i]["BonusGoodsParam2"].ToString().ToDouble();

                ShopOperator.AddStoreDataBase(shopDiamondChargeData);
                ShopOperator.AddStoreIntervalCheckData(shopDiamondChargeData);

                RewardData baseRewardData = new RewardData();
                baseRewardData.Index = shopDiamondChargeData.BaseGoodsParam1;
                baseRewardData.Amount = shopDiamondChargeData.BaseGoodsParam2;

                shopDiamondChargeData.ShopRewardData.Add(baseRewardData);


                RewardData bonusRewardData = new RewardData();
                bonusRewardData.Index = shopDiamondChargeData.BonusGoodsParam1;
                bonusRewardData.Amount = shopDiamondChargeData.BonusGoodsParam2;

                shopDiamondChargeData.BonusGoods.Add(bonusRewardData);

                m_shopDiamondChargeDatas.Add(shopDiamondChargeData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("ShopPackage", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ShopPackageData shopPackageData = new ShopPackageData();
                shopPackageData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                shopPackageData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                shopPackageData.MenuType = rows[i]["MenuType"].ToString().ToInt().IntToEnum32<V2Enum_ShopMenuType>();

                shopPackageData.DisplayType = rows[i]["DisplayType"].ToString().ToInt().IntToEnum32<V2Enum_DisplayType>();
                shopPackageData.IntervalType = rows[i]["IntervalType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
                shopPackageData.IntervalParam = rows[i]["IntervalParam"].ToString().ToInt();

                shopPackageData.PID = rows[i]["PID"].ToString();

                shopPackageData.PriceKR = rows[i]["PriceKR"].ToString().ToInt();

                shopPackageData.Description = rows[i]["Description"].ToString();

                shopPackageData.TitleLocalStringKey = string.Format("shopPackage/title/{0}", shopPackageData.ResourceIndex);
                shopPackageData.SubTitleLocalStringKey = string.Format("shopPackage/subTitle/{0}", shopPackageData.ResourceIndex);
                shopPackageData.MailTitleLocalStringKey = string.Format("shopPackage/mail/{0}", shopPackageData.ResourceIndex);
                shopPackageData.MailDescLocalStringKey = string.Format("shopPackage/mail/Desc/{0}", shopPackageData.ResourceIndex);

                shopPackageData.OpenConditionType = rows[i]["OpenConditionType"].ToString().ToInt().IntToEnum32<V2Enum_OpenConditionType>();
                shopPackageData.OpenConditionParam = rows[i]["OpenConditionParam"].ToString().ToInt();



                shopPackageData.ReturnGoodsParam11 = rows[i]["ReturnGoodsParam11"].ToString().ToInt();
                shopPackageData.ReturnGoodsParam12 = rows[i]["ReturnGoodsParam12"].ToString().ToDouble();

                shopPackageData.ReturnGoodsParam21 = rows[i]["ReturnGoodsParam21"].ToString().ToInt();
                shopPackageData.ReturnGoodsParam22 = rows[i]["ReturnGoodsParam22"].ToString().ToDouble();

                shopPackageData.ReturnGoodsParam31 = rows[i]["ReturnGoodsParam31"].ToString().ToInt();
                shopPackageData.ReturnGoodsParam32 = rows[i]["ReturnGoodsParam32"].ToString().ToDouble();

                shopPackageData.ReturnGoodsParam41 = rows[i]["ReturnGoodsParam41"].ToString().ToInt();
                shopPackageData.ReturnGoodsParam42 = rows[i]["ReturnGoodsParam42"].ToString().ToDouble();

                shopPackageData.ReturnGoodsParam51 = rows[i]["ReturnGoodsParam51"].ToString().ToInt();
                shopPackageData.ReturnGoodsParam52 = rows[i]["ReturnGoodsParam52"].ToString().ToDouble();



                ShopOperator.AddStoreDataBase(shopPackageData);
                ShopOperator.AddStoreIntervalCheckData(shopPackageData);

                if (shopPackageData.ReturnGoodsParam11 != -1)
                {
                    RewardData rewardData = new RewardData();
                    rewardData.Index = shopPackageData.ReturnGoodsParam11;
                    rewardData.Amount = shopPackageData.ReturnGoodsParam12;
                    shopPackageData.ShopRewardData.Add(rewardData);
                }

                if (shopPackageData.ReturnGoodsParam21 != -1)
                {
                    RewardData rewardData = new RewardData();
                    rewardData.Index = shopPackageData.ReturnGoodsParam21;
                    rewardData.Amount = shopPackageData.ReturnGoodsParam22;
                    shopPackageData.ShopRewardData.Add(rewardData);
                }

                if (shopPackageData.ReturnGoodsParam31 != -1)
                {
                    RewardData rewardData = new RewardData();
                    rewardData.Index = shopPackageData.ReturnGoodsParam31;
                    rewardData.Amount = shopPackageData.ReturnGoodsParam32;
                    shopPackageData.ShopRewardData.Add(rewardData);
                }

                if (shopPackageData.ReturnGoodsParam41 != -1)
                {
                    RewardData rewardData = new RewardData();
                    rewardData.Index = shopPackageData.ReturnGoodsParam41;
                    rewardData.Amount = shopPackageData.ReturnGoodsParam42;
                    shopPackageData.ShopRewardData.Add(rewardData);
                }

                if (shopPackageData.ReturnGoodsParam51 != -1)
                {
                    RewardData rewardData = new RewardData();
                    rewardData.Index = shopPackageData.ReturnGoodsParam51;
                    rewardData.Amount = shopPackageData.ReturnGoodsParam52;
                    shopPackageData.ShopRewardData.Add(rewardData);
                }

                m_shopPackageDatas.Add(shopPackageData);
            }

            //rows = null;

            //TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("ShopPackageRelay", o =>
            //{ rows = o; });

            //await UniTask.WaitUntil(() => rows != null);

            //for (int i = 0; i < rows.Count; ++i)
            //{
            //    ShopPackageRelayData shopPackageRelayData = new ShopPackageRelayData();
            //    shopPackageRelayData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
            //    shopPackageRelayData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

            //    shopPackageRelayData.MenuType = rows[i]["MenuType"].ToString().ToInt().IntToEnum32<V2Enum_ShopMenuType>();
            //    shopPackageRelayData.TagTypeList = JsonConvert.DeserializeObject<List<V2Enum_ShopTagType>>(rows[i]["TagType"].ToString());

            //    shopPackageRelayData.DisplayType = rows[i]["DisplayType"].ToString().ToInt().IntToEnum32<V2Enum_DisplayType>();
            //    shopPackageRelayData.IntervalType = rows[i]["IntervalType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
            //    shopPackageRelayData.IntervalParam = rows[i]["IntervalParam"].ToString().ToInt();

            //    shopPackageRelayData.PID = rows[i]["PID"].ToString();

            //    shopPackageRelayData.PriceKR = rows[i]["PriceKR"].ToString().ToInt();

            //    shopPackageRelayData.Description = rows[i]["Description"].ToString();

            //    shopPackageRelayData.TitleLocalStringKey = string.Format("shopPackageRelay/{0}/title", shopPackageRelayData.ResourceIndex);
            //    shopPackageRelayData.SubTitleLocalStringKey = string.Format("shopPackageRelay/{0}/subTitle", shopPackageRelayData.ResourceIndex);
            //    shopPackageRelayData.MailTitleLocalStringKey = string.Format("shopPackageRelay/{0}/mailTitle", shopPackageRelayData.ResourceIndex);
            //    shopPackageRelayData.MailDescLocalStringKey = string.Format("shopPackageRelay/{0}/mailDesc", shopPackageRelayData.ResourceIndex);
            //    shopPackageRelayData.WorthLocalStringKey = string.Format("shopPackageRelay/{0}/worth", shopPackageRelayData.ResourceIndex);

            //    shopPackageRelayData.RelayGroupIndex = rows[i]["RelayGroupIndex"].ToString().ToInt();
            //    shopPackageRelayData.GroupOrder = rows[i]["GroupOrder"].ToString().ToInt();


            //    shopPackageRelayData.ProductActivateConditionType = rows[i]["ProductActivateConditionType"].ToString().ToInt().IntToEnum32<V2Enum_ProductConditionType>();
            //    shopPackageRelayData.ProductActivateConditionParam = rows[i]["ProductActivateConditionParam"].ToString().ToInt();

            //    shopPackageRelayData.ProductPurchaseConditionType1 = rows[i]["ProductPurchaseConditionType1"].ToString().ToInt().IntToEnum32<V2Enum_ProductConditionType>();
            //    shopPackageRelayData.ProductPurchaseConditionParam1 = rows[i]["ProductPurchaseConditionParam1"].ToString().ToInt();

            //    shopPackageRelayData.ProductPurchaseConditionType2 = rows[i]["ProductPurchaseConditionType2"].ToString().ToInt().IntToEnum32<V2Enum_ProductConditionType>();
            //    shopPackageRelayData.ProductPurchaseConditionParam2 = rows[i]["ProductPurchaseConditionParam2"].ToString().ToInt();


            //    shopPackageRelayData.ReturnGoodsType1 = rows[i]["ReturnGoodsType1"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageRelayData.ReturnGoodsParam11 = rows[i]["ReturnGoodsParam11"].ToString().ToInt();
            //    shopPackageRelayData.ReturnGoodsParam12 = rows[i]["ReturnGoodsParam12"].ToString().ToDouble();

            //    shopPackageRelayData.ReturnGoodsType2 = rows[i]["ReturnGoodsType2"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageRelayData.ReturnGoodsParam21 = rows[i]["ReturnGoodsParam21"].ToString().ToInt();
            //    shopPackageRelayData.ReturnGoodsParam22 = rows[i]["ReturnGoodsParam22"].ToString().ToDouble();

            //    shopPackageRelayData.ReturnGoodsType3 = rows[i]["ReturnGoodsType3"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageRelayData.ReturnGoodsParam31 = rows[i]["ReturnGoodsParam31"].ToString().ToInt();
            //    shopPackageRelayData.ReturnGoodsParam32 = rows[i]["ReturnGoodsParam32"].ToString().ToDouble();

            //    shopPackageRelayData.ReturnGoodsType4 = rows[i]["ReturnGoodsType4"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageRelayData.ReturnGoodsParam41 = rows[i]["ReturnGoodsParam41"].ToString().ToInt();
            //    shopPackageRelayData.ReturnGoodsParam42 = rows[i]["ReturnGoodsParam42"].ToString().ToDouble();

            //    shopPackageRelayData.ReturnGoodsType5 = rows[i]["ReturnGoodsType5"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageRelayData.ReturnGoodsParam51 = rows[i]["ReturnGoodsParam51"].ToString().ToInt();
            //    shopPackageRelayData.ReturnGoodsParam52 = rows[i]["ReturnGoodsParam52"].ToString().ToDouble();

            //    shopPackageRelayData.SelectiveReturnGoodsType = rows[i]["SelectiveReturnGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageRelayData.SelectiveReturnGoodsParam1 = rows[i]["SelectiveReturnGoodsParam1"].ToString();
            //    shopPackageRelayData.SelectiveReturnGoodsParam2 = rows[i]["SelectiveReturnGoodsParam2"].ToString().ToDouble();


            //    ShopOperator.AddStoreDataBase(shopPackageRelayData);
            //    ShopOperator.AddStoreIntervalCheckData(shopPackageRelayData);

            //    ShopPackageRelayGroupData shopPackageRelayGroupData = null;

            //    if (m_shopPackageRelayGroupDatas.ContainsKey(shopPackageRelayData.RelayGroupIndex) == false)
            //    {
            //        shopPackageRelayGroupData = new ShopPackageRelayGroupData();
            //        shopPackageRelayGroupData.RelayGroupIndex = shopPackageRelayData.RelayGroupIndex;
            //        m_shopPackageRelayGroupDatas.Add(shopPackageRelayData.RelayGroupIndex, shopPackageRelayGroupData);
            //    }
            //    else
            //        shopPackageRelayGroupData = m_shopPackageRelayGroupDatas[shopPackageRelayData.RelayGroupIndex];

            //    if (shopPackageRelayGroupData == null)
            //        continue;

            //    shopPackageRelayGroupData.ShopPackageRelayDatas.Add(shopPackageRelayData);

            //    if (shopPackageRelayData.ReturnGoodsType1.Enum32ToInt() != -1
            //        && shopPackageRelayData.ReturnGoodsType1.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageRelayData.ReturnGoodsType1;
            //        rewardData.Index = shopPackageRelayData.ReturnGoodsParam11;
            //        rewardData.Amount = shopPackageRelayData.ReturnGoodsParam12;
            //        shopPackageRelayData.ShopRewardData.Add(rewardData);
            //    }

            //    if (shopPackageRelayData.ReturnGoodsType2.Enum32ToInt() != -1
            //        && shopPackageRelayData.ReturnGoodsType2.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageRelayData.ReturnGoodsType2;
            //        rewardData.Index = shopPackageRelayData.ReturnGoodsParam21;
            //        rewardData.Amount = shopPackageRelayData.ReturnGoodsParam22;
            //        shopPackageRelayData.ShopRewardData.Add(rewardData);
            //    }

            //    if (shopPackageRelayData.ReturnGoodsType3.Enum32ToInt() != -1
            //        && shopPackageRelayData.ReturnGoodsType3.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageRelayData.ReturnGoodsType3;
            //        rewardData.Index = shopPackageRelayData.ReturnGoodsParam31;
            //        rewardData.Amount = shopPackageRelayData.ReturnGoodsParam32;
            //        shopPackageRelayData.ShopRewardData.Add(rewardData);
            //    }

            //    if (shopPackageRelayData.ReturnGoodsType4.Enum32ToInt() != -1
            //        && shopPackageRelayData.ReturnGoodsType4.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageRelayData.ReturnGoodsType4;
            //        rewardData.Index = shopPackageRelayData.ReturnGoodsParam41;
            //        rewardData.Amount = shopPackageRelayData.ReturnGoodsParam42;
            //        shopPackageRelayData.ShopRewardData.Add(rewardData);
            //    }

            //    if (shopPackageRelayData.ReturnGoodsType5.Enum32ToInt() != -1
            //        && shopPackageRelayData.ReturnGoodsType5.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageRelayData.ReturnGoodsType5;
            //        rewardData.Index = shopPackageRelayData.ReturnGoodsParam51;
            //        rewardData.Amount = shopPackageRelayData.ReturnGoodsParam52;
            //        shopPackageRelayData.ShopRewardData.Add(rewardData);
            //    }

            //    if (shopPackageRelayData.SelectiveReturnGoodsType.Enum32ToInt() != -1
            //        && shopPackageRelayData.SelectiveReturnGoodsType.Enum32ToInt() != -0)
            //    {

            //        List<int> selectGoods = JsonConvert.DeserializeObject<List<int>>(shopPackageRelayData.SelectiveReturnGoodsParam1);

            //        for (int selectidx = 0; selectidx < selectGoods.Count; ++selectidx)
            //        {
            //            ObscuredInt obscuredInt = selectGoods[selectidx];
            //            shopPackageRelayData.SelectiveReturnGoodsParam1_Goods.Add(obscuredInt);
            //        }

            //        if (shopPackageRelayData.SelectiveReturnGoodsParam1_Goods.Count > 0)
            //        {
            //            RewardData rewardData = new RewardData();
            //            rewardData.V2Enum_Goods = shopPackageRelayData.SelectiveReturnGoodsType;
            //            rewardData.Index = shopPackageRelayData.SelectiveReturnGoodsParam1_Goods[0].GetDecrypted();
            //            rewardData.Amount = shopPackageRelayData.SelectiveReturnGoodsParam2;
            //            shopPackageRelayData.ShopRewardData.Add(rewardData);

            //            shopPackageRelayData.SelectRewardData = rewardData;
            //        }
            //    }
            //}

            //foreach (var pair in m_shopPackageRelayGroupDatas)
            //{
            //    pair.Value.ShopPackageRelayDatas.Sort((x, y) =>
            //    {
            //        if (x.GroupOrder > y.GroupOrder)
            //            return 1;
            //        else
            //            return -1;
            //    });
            //}

            //rows = null;

            //TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("ShopPackageSpecial", o =>
            //{ rows = o; });

            //await UniTask.WaitUntil(() => rows != null);

            //for (int i = 0; i < rows.Count; ++i)
            //{
            //    ShopPackageSpecialData shopPackageSpecialData = new ShopPackageSpecialData();
            //    shopPackageSpecialData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
            //    shopPackageSpecialData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

            //    shopPackageSpecialData.MenuType = rows[i]["MenuType"].ToString().ToInt().IntToEnum32<V2Enum_ShopMenuType>();
            //    shopPackageSpecialData.TagTypeList = JsonConvert.DeserializeObject<List<V2Enum_ShopTagType>>(rows[i]["TagType"].ToString());

            //    shopPackageSpecialData.DisplayType = rows[i]["DisplayType"].ToString().ToInt().IntToEnum32<V2Enum_DisplayType>();
            //    shopPackageSpecialData.IntervalType = rows[i]["IntervalType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
            //    shopPackageSpecialData.IntervalParam = rows[i]["IntervalParam"].ToString().ToInt();

            //    shopPackageSpecialData.PID = rows[i]["PID"].ToString();

            //    shopPackageSpecialData.PriceKR = rows[i]["PriceKR"].ToString().ToInt();

            //    shopPackageSpecialData.Description = rows[i]["Description"].ToString();

            //    shopPackageSpecialData.TitleLocalStringKey = string.Format("shopPackageSpecial/{0}/title", shopPackageSpecialData.ResourceIndex);
            //    shopPackageSpecialData.SubTitleLocalStringKey = string.Format("shopPackageSpecial/{0}/subTitle", shopPackageSpecialData.ResourceIndex);
            //    shopPackageSpecialData.MailTitleLocalStringKey = string.Format("shopPackageSpecial/{0}/mailTitle", shopPackageSpecialData.ResourceIndex);
            //    shopPackageSpecialData.MailDescLocalStringKey = string.Format("shopPackageSpecial/{0}/mailDesc", shopPackageSpecialData.ResourceIndex);
            //    shopPackageSpecialData.WorthLocalStringKey = string.Format("shopPackageSpecial/{0}/worth", shopPackageSpecialData.ResourceIndex);

            //    shopPackageSpecialData.OpenConditionType = rows[i]["OpenConditionType"].ToString().ToInt().IntToEnum32<V2Enum_ProductConditionType>();
            //    shopPackageSpecialData.OpenConditionParam = rows[i]["OpenConditionParam"].ToString().ToInt();

            //    shopPackageSpecialData.DurationType = rows[i]["DurationType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
            //    shopPackageSpecialData.DurationParam = rows[i]["DurationParam"].ToString().ToInt();



            //    shopPackageSpecialData.ReturnGoodsType1 = rows[i]["ReturnGoodsType1"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageSpecialData.ReturnGoodsParam11 = rows[i]["ReturnGoodsParam11"].ToString().ToInt();
            //    shopPackageSpecialData.ReturnGoodsParam12 = rows[i]["ReturnGoodsParam12"].ToString().ToDouble();
            //    shopPackageSpecialData.ReturnGoodsParam13 = rows[i]["ReturnGoodsParam13"].ToString().ToInt();

            //    shopPackageSpecialData.ReturnGoodsType2 = rows[i]["ReturnGoodsType2"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageSpecialData.ReturnGoodsParam21 = rows[i]["ReturnGoodsParam21"].ToString().ToInt();
            //    shopPackageSpecialData.ReturnGoodsParam22 = rows[i]["ReturnGoodsParam22"].ToString().ToDouble();
            //    shopPackageSpecialData.ReturnGoodsParam23 = rows[i]["ReturnGoodsParam23"].ToString().ToInt();

            //    shopPackageSpecialData.ReturnGoodsType3 = rows[i]["ReturnGoodsType3"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageSpecialData.ReturnGoodsParam31 = rows[i]["ReturnGoodsParam31"].ToString().ToInt();
            //    shopPackageSpecialData.ReturnGoodsParam32 = rows[i]["ReturnGoodsParam32"].ToString().ToDouble();
            //    shopPackageSpecialData.ReturnGoodsParam33 = rows[i]["ReturnGoodsParam33"].ToString().ToInt();

            //    shopPackageSpecialData.ReturnGoodsType4 = rows[i]["ReturnGoodsType4"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageSpecialData.ReturnGoodsParam41 = rows[i]["ReturnGoodsParam41"].ToString().ToInt();
            //    shopPackageSpecialData.ReturnGoodsParam42 = rows[i]["ReturnGoodsParam42"].ToString().ToDouble();
            //    shopPackageSpecialData.ReturnGoodsParam43 = rows[i]["ReturnGoodsParam43"].ToString().ToInt();

            //    shopPackageSpecialData.SelectiveReturnGoodsType = rows[i]["SelectiveReturnGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageSpecialData.SelectiveReturnGoodsParam1 = rows[i]["SelectiveReturnGoodsParam1"].ToString();
            //    shopPackageSpecialData.SelectiveReturnGoodsParam2 = rows[i]["SelectiveReturnGoodsParam2"].ToString().ToDouble();


            //    ShopOperator.AddStoreDataBase(shopPackageSpecialData);
            //    ShopOperator.AddSpecialProductPurchaseCheck(shopPackageSpecialData);

            //    if (shopPackageSpecialData.ReturnGoodsType1.Enum32ToInt() != -1
            //        && shopPackageSpecialData.ReturnGoodsType1.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageSpecialData.ReturnGoodsType1;
            //        rewardData.Index = shopPackageSpecialData.ReturnGoodsParam11;
            //        rewardData.Amount = shopPackageSpecialData.ReturnGoodsParam12;
            //        shopPackageSpecialData.ShopRewardData.Add(rewardData);
            //    }

            //    if (shopPackageSpecialData.ReturnGoodsType2.Enum32ToInt() != -1
            //        && shopPackageSpecialData.ReturnGoodsType2.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageSpecialData.ReturnGoodsType2;
            //        rewardData.Index = shopPackageSpecialData.ReturnGoodsParam21;
            //        rewardData.Amount = shopPackageSpecialData.ReturnGoodsParam22;
            //        shopPackageSpecialData.ShopRewardData.Add(rewardData);
            //    }

            //    if (shopPackageSpecialData.ReturnGoodsType3.Enum32ToInt() != -1
            //        && shopPackageSpecialData.ReturnGoodsType3.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageSpecialData.ReturnGoodsType3;
            //        rewardData.Index = shopPackageSpecialData.ReturnGoodsParam31;
            //        rewardData.Amount = shopPackageSpecialData.ReturnGoodsParam32;
            //        shopPackageSpecialData.ShopRewardData.Add(rewardData);
            //    }

            //    if (shopPackageSpecialData.ReturnGoodsType4.Enum32ToInt() != -1
            //        && shopPackageSpecialData.ReturnGoodsType4.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageSpecialData.ReturnGoodsType4;
            //        rewardData.Index = shopPackageSpecialData.ReturnGoodsParam41;
            //        rewardData.Amount = shopPackageSpecialData.ReturnGoodsParam42;
            //        shopPackageSpecialData.ShopRewardData.Add(rewardData);
            //    }

            //    if (shopPackageSpecialData.SelectiveReturnGoodsType.Enum32ToInt() != -1
            //        && shopPackageSpecialData.SelectiveReturnGoodsType.Enum32ToInt() != -0)
            //    {
            //        List<int> selectGoods = JsonConvert.DeserializeObject<List<int>>(shopPackageSpecialData.SelectiveReturnGoodsParam1);

            //        for (int selectidx = 0; selectidx < selectGoods.Count; ++selectidx)
            //        {
            //            ObscuredInt obscuredInt = selectGoods[selectidx];
            //            shopPackageSpecialData.SelectiveReturnGoodsParam1_Goods.Add(obscuredInt);
            //        }

            //        if (shopPackageSpecialData.SelectiveReturnGoodsParam1_Goods.Count > 0)
            //        {
            //            RewardData rewardData = new RewardData();
            //            rewardData.V2Enum_Goods = shopPackageSpecialData.SelectiveReturnGoodsType;
            //            rewardData.Index = shopPackageSpecialData.SelectiveReturnGoodsParam1_Goods[0].GetDecrypted();
            //            rewardData.Amount = shopPackageSpecialData.SelectiveReturnGoodsParam2;
            //            shopPackageSpecialData.ShopRewardData.Add(rewardData);

            //            shopPackageSpecialData.SelectRewardData = rewardData;
            //        }
            //    }

            //    m_shopPackageSpecialDatas.Add(shopPackageSpecialData);
            //}

            //rows = null;

            //TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("ShopPackageRotation", o =>
            //{ rows = o; });

            //await UniTask.WaitUntil(() => rows != null);

            //for (int i = 0; i < rows.Count; ++i)
            //{
            //    ShopPackageRotationData shopPackageRotationData = new ShopPackageRotationData();
            //    shopPackageRotationData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
            //    shopPackageRotationData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

            //    shopPackageRotationData.DisplayType = rows[i]["DisplayType"].ToString().ToInt().IntToEnum32<V2Enum_DisplayType>();

            //    shopPackageRotationData.RelayGroupIndex = rows[i]["RelayGroupIndex"].ToString().ToInt();

            //    if (shopPackageRotationData.RelayGroupIndex == -1)
            //        continue;

            //    shopPackageRotationData.GroupOrder = rows[i]["GroupOrder"].ToString().ToInt();

            //    shopPackageRotationData.PurchaseConditionType = rows[i]["PurchaseConditionType"].ToString().ToInt().IntToEnum32<V2Enum_ProductConditionType>();
            //    shopPackageRotationData.PurchaseConditionParam = rows[i]["PurchaseConditionParam"].ToString().ToInt();

            //    shopPackageRotationData.IntervalType = rows[i]["IntervalType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
            //    shopPackageRotationData.IntervalParam = rows[i]["IntervalParam"].ToString().ToInt();

            //    shopPackageRotationData.OpenConditionType = rows[i]["OpenConditionType"].ToString().ToInt().IntToEnum32<V2Enum_ProductConditionType>();
            //    shopPackageRotationData.OpenConditionParam = rows[i]["OpenConditionParam"].ToString().ToInt();


            //    shopPackageRotationData.PID = rows[i]["PID"].ToString();

            //    shopPackageRotationData.PriceKR = rows[i]["PriceKR"].ToString().ToInt();

            //    shopPackageRotationData.Description = rows[i]["Description"].ToString();

            //    shopPackageRotationData.TitleLocalStringKey = string.Format("shopPackageRotation/{0}/title", shopPackageRotationData.ResourceIndex);
            //    shopPackageRotationData.SubTitleLocalStringKey = string.Format("shopPackageRotation/{0}/subTitle", shopPackageRotationData.ResourceIndex);
            //    shopPackageRotationData.MailTitleLocalStringKey = string.Format("shopPackageRotation/{0}/mailTitle", shopPackageRotationData.ResourceIndex);
            //    shopPackageRotationData.MailDescLocalStringKey = string.Format("shopPackageRotation/{0}/mailDesc", shopPackageRotationData.ResourceIndex);
            //    shopPackageRotationData.WorthLocalStringKey = string.Format("shopPackageRotation/{0}/worth", shopPackageRotationData.ResourceIndex);


            //    shopPackageRotationData.ReturnGoodsType1 = rows[i]["ReturnGoodsType1"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageRotationData.ReturnGoodsParam11 = rows[i]["ReturnGoodsParam11"].ToString().ToInt();
            //    shopPackageRotationData.ReturnGoodsParam12 = rows[i]["ReturnGoodsParam12"].ToString().ToDouble();

            //    shopPackageRotationData.ReturnGoodsType2 = rows[i]["ReturnGoodsType2"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageRotationData.ReturnGoodsParam21 = rows[i]["ReturnGoodsParam21"].ToString().ToInt();
            //    shopPackageRotationData.ReturnGoodsParam22 = rows[i]["ReturnGoodsParam22"].ToString().ToDouble();

            //    shopPackageRotationData.ReturnGoodsType3 = rows[i]["ReturnGoodsType3"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageRotationData.ReturnGoodsParam31 = rows[i]["ReturnGoodsParam31"].ToString().ToInt();
            //    shopPackageRotationData.ReturnGoodsParam32 = rows[i]["ReturnGoodsParam32"].ToString().ToDouble();

            //    shopPackageRotationData.ReturnGoodsType4 = rows[i]["ReturnGoodsType4"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
            //    shopPackageRotationData.ReturnGoodsParam41 = rows[i]["ReturnGoodsParam41"].ToString().ToInt();
            //    shopPackageRotationData.ReturnGoodsParam42 = rows[i]["ReturnGoodsParam42"].ToString().ToDouble();

            //    shopPackageRotationData.AllyCurrentStar = rows[i]["AllyCurrentStar"].ToString().ToInt();


            //    ShopOperator.AddStoreDataBase(shopPackageRotationData);

            //    ShopPackageRotateGroupData shopPackageRotateGroupData = null;

            //    if (m_shopPackageRotateGroupDatas.ContainsKey(shopPackageRotationData.RelayGroupIndex) == false)
            //    {
            //        shopPackageRotateGroupData = new ShopPackageRotateGroupData();
            //        shopPackageRotateGroupData.RelayGroupIndex = shopPackageRotationData.RelayGroupIndex;
            //        shopPackageRotateGroupData.IntervalType = shopPackageRotationData.IntervalType;
            //        shopPackageRotateGroupData.IntervalParam = shopPackageRotationData.IntervalParam;

            //        shopPackageRotateGroupData.OpenConditionType = shopPackageRotationData.OpenConditionType;
            //        shopPackageRotateGroupData.OpenConditionParam = shopPackageRotationData.OpenConditionParam;

            //        shopPackageRotateGroupData.FirstShopPackageData = shopPackageRotationData;

            //        m_shopPackageRotateGroupDatas.Add(shopPackageRotationData.RelayGroupIndex, shopPackageRotateGroupData);
            //    }
            //    else
            //        shopPackageRotateGroupData = m_shopPackageRotateGroupDatas[shopPackageRotationData.RelayGroupIndex];

            //    if (shopPackageRotateGroupData == null)
            //        continue;

            //    shopPackageRotateGroupData.ShopPackageRelayDatas.Add(shopPackageRotationData);


            //    if (shopPackageRotationData.ReturnGoodsType1.Enum32ToInt() != -1
            //        && shopPackageRotationData.ReturnGoodsType1.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageRotationData.ReturnGoodsType1;
            //        rewardData.Index = shopPackageRotationData.ReturnGoodsParam11;
            //        rewardData.Amount = shopPackageRotationData.ReturnGoodsParam12;

            //        shopPackageRotationData.ShopRewardData.Add(rewardData);
            //    }

            //    if (shopPackageRotationData.ReturnGoodsType2.Enum32ToInt() != -1
            //        && shopPackageRotationData.ReturnGoodsType2.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageRotationData.ReturnGoodsType2;
            //        rewardData.Index = shopPackageRotationData.ReturnGoodsParam21;
            //        rewardData.Amount = shopPackageRotationData.ReturnGoodsParam22;

            //        shopPackageRotationData.ShopRewardData.Add(rewardData);
            //    }

            //    if (shopPackageRotationData.ReturnGoodsType3.Enum32ToInt() != -1
            //        && shopPackageRotationData.ReturnGoodsType3.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageRotationData.ReturnGoodsType3;
            //        rewardData.Index = shopPackageRotationData.ReturnGoodsParam31;
            //        rewardData.Amount = shopPackageRotationData.ReturnGoodsParam32;

            //        shopPackageRotationData.ShopRewardData.Add(rewardData);
            //    }

            //    if (shopPackageRotationData.ReturnGoodsType4.Enum32ToInt() != -1
            //        && shopPackageRotationData.ReturnGoodsType4.Enum32ToInt() != -0)
            //    {
            //        RewardData rewardData = new RewardData();
            //        rewardData.V2Enum_Goods = shopPackageRotationData.ReturnGoodsType4;
            //        rewardData.Index = shopPackageRotationData.ReturnGoodsParam41;
            //        rewardData.Amount = shopPackageRotationData.ReturnGoodsParam42;

            //        shopPackageRotationData.ShopRewardData.Add(rewardData);
            //    }

            //    m_shopPackageRotationDatas.Add(shopPackageRotationData);
            //}


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("ShopPackageLimitTime", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);


            for (int i = 0; i < rows.Count; ++i)
            {
                ShopPackageEventData shopPackageEventData = new ShopPackageEventData();
                shopPackageEventData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                shopPackageEventData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                shopPackageEventData.IconResourceIndex = rows[i]["IconResourceIndex"].ToString().ToInt();

                shopPackageEventData.LobbyIconStringKey = string.Format("Icon/shop/timelimit/{0}_lobby", shopPackageEventData.IconResourceIndex);

                shopPackageEventData.PackageIconStringKey = string.Format("Icon/shop/timelimit/{0}", shopPackageEventData.ResourceIndex);

                shopPackageEventData.MenuType = rows[i]["MenuType"].ToString().ToInt().IntToEnum32<V2Enum_ShopMenuType>();
                shopPackageEventData.TagString = rows[i]["TagString"].ToString();

                shopPackageEventData.DisplayType = rows[i]["DisplayType"].ToString().ToInt().IntToEnum32<V2Enum_DisplayType>();

                shopPackageEventData.IntervalType = rows[i]["IntervalType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
                shopPackageEventData.IntervalParam = rows[i]["IntervalParam"].ToString().ToInt();

                shopPackageEventData.OpenConditionType = rows[i]["OpenConditionType"].ToString().ToInt().IntToEnum32<V2Enum_OpenConditionType>();
                shopPackageEventData.OpenConditionParam = rows[i]["OpenConditionParam"].ToString().ToInt();

                shopPackageEventData.PID = rows[i]["PID"].ToString();

                shopPackageEventData.DurationType = rows[i]["DurationType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
                shopPackageEventData.DurationParam = rows[i]["DurationParam"].ToString().ToInt();

                shopPackageEventData.PriceKR = rows[i]["PriceKR"].ToString().ToInt();

                shopPackageEventData.Description = rows[i]["Description"].ToString();

                shopPackageEventData.TitleLocalStringKey = string.Format("shopPackageLimitTime/title/{0}", shopPackageEventData.ResourceIndex);
                shopPackageEventData.SubTitleLocalStringKey = string.Format("shopPackageLimitTime/subTitle/{0}", shopPackageEventData.ResourceIndex);
                shopPackageEventData.MailTitleLocalStringKey = string.Format("shopPackageLimitTime/mailTitle/{0}", shopPackageEventData.ResourceIndex);
                shopPackageEventData.MailDescLocalStringKey = string.Format("shopPackageLimitTime/mailDesc/{0}", shopPackageEventData.ResourceIndex);

                ShopOperator.AddStoreDataBase(shopPackageEventData);
                ShopOperator.AddEventProductPurchaseCheck(shopPackageEventData);

                for (int rewardidx = 1; rewardidx <= 4; ++rewardidx)
                {
                    int idx = rows[i][string.Format("ReturnGoodsParam{0}1", rewardidx)].ToString().ToInt();
                    if (idx == -1)
                        continue;
                    double amount = rows[i][string.Format("ReturnGoodsParam{0}2", rewardidx)].ToString().ToDouble();

                    RewardData rewardData = new RewardData();
                    rewardData.Index = idx;
                    rewardData.Amount = amount;
                    shopPackageEventData.ShopRewardData.Add(rewardData);
                }

                m_shopPackageEvents.Add(shopPackageEventData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("ShopIngameStore", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);


            for (int i = 0; i < rows.Count; ++i)
            {
                ShopIngameStoreData shopIngameStoreData = new ShopIngameStoreData();
                shopIngameStoreData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                shopIngameStoreData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                shopIngameStoreData.MenuType = rows[i]["MenuType"].ToString().ToInt().IntToEnum32<V2Enum_ShopMenuType>();
                shopIngameStoreData.TagString = rows[i]["TagString"].ToString();

                shopIngameStoreData.IntervalType = rows[i]["IntervalType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
                shopIngameStoreData.IntervalParam = rows[i]["IntervalParam"].ToString().ToInt();

                shopIngameStoreData.CostGoods.Index = rows[i]["CostgoodsParam1"].ToString().ToInt();
                shopIngameStoreData.CostGoods.Amount = rows[i]["CostgoodsParam2"].ToString().ToDouble();

                shopIngameStoreData.ReturnGoods.Index = rows[i]["ReturnGoodsParam1"].ToString().ToInt();
                shopIngameStoreData.ReturnGoods.Amount = rows[i]["ReturnGoodsParam2"].ToString().ToDouble();

                shopIngameStoreData.TitleLocalStringKey = string.Format("shopIngameStoreData/{0}/title", shopIngameStoreData.ResourceIndex);
                shopIngameStoreData.SubTitleLocalStringKey = string.Format("shopIngameStoreData/{0}/subTitle", shopIngameStoreData.ResourceIndex);
                shopIngameStoreData.MailTitleLocalStringKey = string.Format("shopIngameStoreData/{0}/mailTitle", shopIngameStoreData.ResourceIndex);
                shopIngameStoreData.MailDescLocalStringKey = string.Format("shopIngameStoreData/{0}/mailDesc", shopIngameStoreData.ResourceIndex);

                shopIngameStoreData.PackageIconStringKey = string.Format("Icon/shop/ingame/{0}", shopIngameStoreData.ResourceIndex);

                _shopIngameStoreDatas.Add(shopIngameStoreData);
                if (shopIngameStoreData.IntervalType.Enum32ToInt() != -1)
                    ShopOperator.AddStoreDataBase(shopIngameStoreData);
                ShopOperator.AddStoreIntervalCheckData(shopIngameStoreData);
            }


        }
        //------------------------------------------------------------------------------------
        public List<ShopDiamondChargeData> GetDiamondChargeDatas()
        {
            return m_shopDiamondChargeDatas;
        }
        //------------------------------------------------------------------------------------
        public List<ShopPackageData> GetPackageDatas()
        {
            return m_shopPackageDatas;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<int, ShopPackageRelayGroupData> GetPackageRelayDatas()
        {
            return m_shopPackageRelayGroupDatas;
        }
        //------------------------------------------------------------------------------------
        public ShopPackageRelayGroupData GetPackageRelayGroupData(int index)
        {
            if (m_shopPackageRelayGroupDatas.ContainsKey(index) == true)
            {
                return m_shopPackageRelayGroupDatas[index];
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<ShopPackageSpecialData> GetPackageSpecialDatas()
        {
            return m_shopPackageSpecialDatas;
        }
        //------------------------------------------------------------------------------------
        public List<ShopPackageRotationData> GetPackageRotationDatas()
        {
            return m_shopPackageRotationDatas;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<int, ShopPackageRotateGroupData> GetPackageRotateDatas()
        {
            return m_shopPackageRotateGroupDatas;
        }
        //------------------------------------------------------------------------------------
        public ShopPackageRotateGroupData GetPackageRotateGroupData(int index)
        {
            if (m_shopPackageRotateGroupDatas.ContainsKey(index) == true)
            {
                return m_shopPackageRotateGroupDatas[index];
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<ShopPackageEventData> GetPackageEventDatas()
        {
            return m_shopPackageEvents;
        }
        //------------------------------------------------------------------------------------
        public List<ShopIngameStoreData> GetShopIngameStoreDatas()
        {
            return _shopIngameStoreDatas;
        }

        //------------------------------------------------------------------------------------
    }


    public class ShopRandomStoreData
    {
        public ObscuredInt Index;

        public RewardData CostGoods = new RewardData();
        public RewardData ReturnGoods = new RewardData();

        public ObscuredDouble Prob;

        public ObscuredInt DiscountPercent;

        public string TitleLocalStringKey;
    }


    public class ShopFreeGoodsData
    {
        public ObscuredInt Index;
        public ObscuredInt ResourceIndex;

        public V2Enum_ShopMenuType MenuType;

        public V2Enum_IntervalType IntervalType;
        public ObscuredInt IntervalParam;

        public RewardData ReturnGoods = new RewardData();

        public ObscuredInt FreeBuyCount;
        public ObscuredInt FreeAdCount;
    }

    public class ShopRandomStoreLocalTable : LocalTableBase
    {
        public List<ShopRandomStoreData> _shopRandomStoreDatas = new List<ShopRandomStoreData>();

        private List<ShopFreeGoodsData> _shopFreeGoodsDatas = new List<ShopFreeGoodsData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("ShopRandomStore", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);


            for (int i = 0; i < rows.Count; ++i)
            {
                ShopRandomStoreData shopRandomStoreData = new ShopRandomStoreData();
                shopRandomStoreData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();

                shopRandomStoreData.CostGoods.Index = rows[i]["CostgoodsParam1"].ToString().ToInt();
                shopRandomStoreData.CostGoods.Amount = rows[i]["CostgoodsParam2"].ToString().ToDouble();

                shopRandomStoreData.ReturnGoods.Index = rows[i]["ReturnGoodsParam1"].ToString().ToInt();
                shopRandomStoreData.ReturnGoods.Amount = rows[i]["ReturnGoodsParam2"].ToString().ToDouble();

                shopRandomStoreData.Prob = rows[i]["Prob"].ToString().ToDouble();
                shopRandomStoreData.DiscountPercent = rows[i]["DiscountPercent"].ToString().ToInt();

                shopRandomStoreData.TitleLocalStringKey = string.Format("shopRandomStore/{0}/title", shopRandomStoreData.Index);

                _shopRandomStoreDatas.Add(shopRandomStoreData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("ShopFreeGoods", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);


            for (int i = 0; i < rows.Count; ++i)
            {
                ShopFreeGoodsData shopFreeGoodsData = new ShopFreeGoodsData();
                shopFreeGoodsData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                shopFreeGoodsData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                shopFreeGoodsData.MenuType = rows[i]["MenuType"].ToString().ToInt().IntToEnum32<V2Enum_ShopMenuType>();

                shopFreeGoodsData.IntervalType = rows[i]["IntervalType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
                shopFreeGoodsData.IntervalParam = rows[i]["IntervalParam"].ToString().ToInt();

                shopFreeGoodsData.ReturnGoods.Index = rows[i]["ReturnGoodsParam1"].ToString().ToInt();
                shopFreeGoodsData.ReturnGoods.Amount = rows[i]["ReturnGoodsParam2"].ToString().ToDouble();


                _shopFreeGoodsDatas.Add(shopFreeGoodsData);
            }
        }
        //------------------------------------------------------------------------------------
        public List<ShopRandomStoreData> GetShopRandomStoreDatas()
        {
            return _shopRandomStoreDatas;
        }
        //------------------------------------------------------------------------------------
        public List<ShopFreeGoodsData> GetShopFreeGoodsDatas()
        {
            return _shopFreeGoodsDatas;
        }
        //------------------------------------------------------------------------------------
    }
}