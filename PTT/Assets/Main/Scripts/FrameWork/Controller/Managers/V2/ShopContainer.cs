using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class PlayerShopInfo
    {
        public ObscuredInt Id;
        
        public ObscuredInt AccumCount;
        public ObscuredInt Count;

        // Rotate 상품은 구매 시점의 타임을 찍어주니 조심
        public double InitTimeStemp = 0.0;
    }

    public class PlayerShopPackageRelayGroupInfo
    {
        public int GroupId;
    }

    public static class ShopContainer
    {
        public static Dictionary<int, PlayerShopInfo> m_shopInfo = new Dictionary<int, PlayerShopInfo>();
        public static Dictionary<int, PlayerShopPackageRelayGroupInfo> m_showRelayGroupInfo = new Dictionary<int, PlayerShopPackageRelayGroupInfo>();

        public static Dictionary<int, ShopDataBase> m_allShopDatas = new Dictionary<int, ShopDataBase>();
        public static Dictionary<V2Enum_IntervalType, List<ShopDataBase>> m_shopIntervalDatas = new Dictionary<V2Enum_IntervalType, List<ShopDataBase>>();

        public static Dictionary<V2Enum_IntervalType, List<ShopPackageSpecialData>> m_shopSpecialPackageIntervalDatas = new Dictionary<V2Enum_IntervalType, List<ShopPackageSpecialData>>();
        public static Dictionary<V2Enum_IntervalType, List<ShopPackageEventData>> m_shopEventPackageIntervalDatas = new Dictionary<V2Enum_IntervalType, List<ShopPackageEventData>>();

        public static Dictionary<V2Enum_IntervalType, List<ShopPackageRotateGroupData>> m_shopRotatePackageIntervalDatas = new Dictionary<V2Enum_IntervalType, List<ShopPackageRotateGroupData>>();

        public static Dictionary<V2Enum_OpenConditionType, List<ShopPackageRelayGroupData>> m_activeCheckDatas = new Dictionary<V2Enum_OpenConditionType, List<ShopPackageRelayGroupData>>();
        public static Dictionary<V2Enum_OpenConditionType, List<ShopPackageRelayData>> m_productConditionDatas = new Dictionary<V2Enum_OpenConditionType, List<ShopPackageRelayData>>();

        public static Dictionary<V2Enum_OpenConditionType, List<ShopPackageSpecialData>> m_productConditionSpcialDatas = new Dictionary<V2Enum_OpenConditionType, List<ShopPackageSpecialData>>();

        public static Dictionary<V2Enum_OpenConditionType, List<ShopPackageEventData>> m_productConditionEventDatas = new Dictionary<V2Enum_OpenConditionType, List<ShopPackageEventData>>();

        public static List<ShopDataBase> m_freeProductData = new List<ShopDataBase>();

        public static List<ShopDataBase> m_adProductDatas = new List<ShopDataBase>();

        public static int TotalBuyPrice = 0;

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in m_shopInfo)
            {
                SerializeString.Append(pair.Value.Id);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.AccumCount);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Count);
                SerializeString.Append(',');
                SerializeString.Append((long)pair.Value.InitTimeStemp);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                PlayerShopInfo playerShopInfo = new PlayerShopInfo();
                playerShopInfo.Id = arrcontent[0].ToInt();
                playerShopInfo.AccumCount = arrcontent[1].ToInt();
                playerShopInfo.Count = arrcontent[2].ToInt();
                playerShopInfo.InitTimeStemp = arrcontent[3].ToDouble();

                m_shopInfo.Add(playerShopInfo.Id, playerShopInfo);
            }
        }
    }

    public static class ShopOperator
    {
        private static ShopLocalTable m_shopLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            m_shopLocalTable = Managers.TableManager.Instance.GetTableClass<ShopLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static void AddStoreDataBase(ShopDataBase shopDataBase)
        {
            if (shopDataBase == null)
                return;

            ShopContainer.m_allShopDatas.Add(shopDataBase.Index, shopDataBase);
            if (shopDataBase.PID == "-1")
                ShopContainer.m_freeProductData.Add(shopDataBase);
            else if (IsAD(shopDataBase) == true)
            {
                ShopContainer.m_adProductDatas.Add(shopDataBase);
            }
        }
        //------------------------------------------------------------------------------------
        public static bool IsAD(ShopDataBase shopDataBase)
        {
            if (shopDataBase == null)
                return false;

            return shopDataBase.PID.GetDecrypted().Contains("shoppackage_AD");
        }
        //------------------------------------------------------------------------------------

        public static void AddStoreIntervalCheckData(ShopDataBase shopDataBase)
        {
            if (shopDataBase == null)
                return;

            if (ShopContainer.m_shopIntervalDatas.ContainsKey(shopDataBase.IntervalType) == false)
                ShopContainer.m_shopIntervalDatas.Add(shopDataBase.IntervalType, new List<ShopDataBase>());

            ShopContainer.m_shopIntervalDatas[shopDataBase.IntervalType].Add(shopDataBase);
        }
        //------------------------------------------------------------------------------------
        public static void AddStoreSpecialPackageDurationCheckData(ShopPackageSpecialData shopDataBase)
        {
            if (shopDataBase == null)
                return;

            if (ShopContainer.m_shopSpecialPackageIntervalDatas.ContainsKey(shopDataBase.DurationType) == false)
                ShopContainer.m_shopSpecialPackageIntervalDatas.Add(shopDataBase.DurationType, new List<ShopPackageSpecialData>());

            ShopContainer.m_shopSpecialPackageIntervalDatas[shopDataBase.DurationType].Add(shopDataBase);
        }
        //------------------------------------------------------------------------------------
        public static void AddStoreEventPackageDurationCheckData(ShopPackageEventData shopDataBase)
        {
            if (shopDataBase == null)
                return;

            if (ShopContainer.m_shopEventPackageIntervalDatas.ContainsKey(shopDataBase.DurationType) == false)
                ShopContainer.m_shopEventPackageIntervalDatas.Add(shopDataBase.DurationType, new List<ShopPackageEventData>());

            ShopContainer.m_shopEventPackageIntervalDatas[shopDataBase.DurationType].Add(shopDataBase);
        }
        //------------------------------------------------------------------------------------
        public static List<ShopDiamondChargeData> GetShopDiamondChargeDatas()
        {
            return m_shopLocalTable.GetDiamondChargeDatas();
        }
        //------------------------------------------------------------------------------------
        public static List<ShopPackageData> GetPackageDatas()
        {
            return m_shopLocalTable.GetPackageDatas();
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<int, ShopPackageRelayGroupData> GetPackageRelayDatas()
        {
            return m_shopLocalTable.GetPackageRelayDatas();
        }
        //------------------------------------------------------------------------------------
        public static ShopPackageRelayGroupData GetPackageRelayGroupData(int index)
        {
            return m_shopLocalTable.GetPackageRelayGroupData(index);
        }
        //------------------------------------------------------------------------------------
        public static List<ShopPackageSpecialData> GetPackageSpecialDatas()
        {
            return m_shopLocalTable.GetPackageSpecialDatas();
        }
        //------------------------------------------------------------------------------------
        public static List<ShopPackageEventData> GetPackageEventDatas()
        {
            return m_shopLocalTable.GetPackageEventDatas();
        }
        //------------------------------------------------------------------------------------
        public static List<ShopIngameStoreData> GetShopIngameStoreDatas()
        {
            return m_shopLocalTable.GetShopIngameStoreDatas();
        }
        //------------------------------------------------------------------------------------
        public static List<ShopPackageRotationData> GetPackageRotationDatas()
        {
            return m_shopLocalTable.GetPackageRotationDatas();
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<int, ShopPackageRotateGroupData> GetPackageRotateDatas()
        {
            return m_shopLocalTable.GetPackageRotateDatas();
        }
        //------------------------------------------------------------------------------------
        public static ShopPackageRotateGroupData GetPackageRotateGroupData(int index)
        {
            return m_shopLocalTable.GetPackageRotateGroupData(index);
        }
        //------------------------------------------------------------------------------------
        public static ShopDataBase GetShopData(int index)
        {
            if (ShopContainer.m_allShopDatas.ContainsKey(index) == true)
                return ShopContainer.m_allShopDatas[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public static PlayerShopInfo GetPlayerShopInfo(ShopDataBase shopDataBase)
        {
            if (shopDataBase == null)
                return null;

            if (ShopContainer.m_shopInfo.ContainsKey(shopDataBase.Index) == true)
                return ShopContainer.m_shopInfo[shopDataBase.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public static void AddRelayGroupDataActiveCheck(ShopPackageRelayGroupData shopPackageRelayGroupData)
        {
            if (shopPackageRelayGroupData == null)
                return;

            if (shopPackageRelayGroupData.FocusShopPackageData == null)
                return;

            V2Enum_OpenConditionType v2Enum_ProductConditionType = shopPackageRelayGroupData.FocusShopPackageData.ProductActivateConditionType;

            if (ShopContainer.m_activeCheckDatas.ContainsKey(v2Enum_ProductConditionType) == false)
                ShopContainer.m_activeCheckDatas.Add(v2Enum_ProductConditionType, new List<ShopPackageRelayGroupData>());

            if (ShopContainer.m_activeCheckDatas[v2Enum_ProductConditionType].Contains(shopPackageRelayGroupData) == false)
                ShopContainer.m_activeCheckDatas[v2Enum_ProductConditionType].Add(shopPackageRelayGroupData);
        }
        //------------------------------------------------------------------------------------
        public static void AddRelayDataProductPurchaseCheck(V2Enum_OpenConditionType v2Enum_ProductConditionType, ShopPackageRelayData shopPackageRelayData)
        {
            if (shopPackageRelayData == null)
                return;

            if (ShopContainer.m_productConditionDatas.ContainsKey(v2Enum_ProductConditionType) == false)
                ShopContainer.m_productConditionDatas.Add(v2Enum_ProductConditionType, new List<ShopPackageRelayData>());

            ShopContainer.m_productConditionDatas[v2Enum_ProductConditionType].Add(shopPackageRelayData);
        }
        //------------------------------------------------------------------------------------
        public static void AddSpecialProductPurchaseCheck(ShopPackageSpecialData shopPackageSpecialData)
        {
            if (shopPackageSpecialData == null)
                return;

            if (ShopContainer.m_productConditionSpcialDatas.ContainsKey(shopPackageSpecialData.OpenConditionType) == false)
                ShopContainer.m_productConditionSpcialDatas.Add(shopPackageSpecialData.OpenConditionType, new List<ShopPackageSpecialData>());

            ShopContainer.m_productConditionSpcialDatas[shopPackageSpecialData.OpenConditionType].Add(shopPackageSpecialData);
        }
        //------------------------------------------------------------------------------------
        public static void AddEventProductPurchaseCheck(ShopPackageEventData shopDataBase)
        {
            if (shopDataBase == null)
                return;

            if (ShopContainer.m_productConditionEventDatas.ContainsKey(shopDataBase.OpenConditionType) == false)
                ShopContainer.m_productConditionEventDatas.Add(shopDataBase.OpenConditionType, new List<ShopPackageEventData>());

            ShopContainer.m_productConditionEventDatas[shopDataBase.OpenConditionType].Add(shopDataBase);
        }
        //------------------------------------------------------------------------------------
        public static PlayerShopInfo CreatePlayerShopInfo(ShopDataBase shopDataBase)
        {
            if (shopDataBase == null)
                return null;

            if (ShopContainer.m_shopInfo.ContainsKey(shopDataBase.Index) == true)
                return ShopContainer.m_shopInfo[shopDataBase.Index];

            PlayerShopInfo playerShopInfo = new PlayerShopInfo();
            playerShopInfo.Id = shopDataBase.Index;
            playerShopInfo.AccumCount = 0;
            playerShopInfo.Count = 0;
            playerShopInfo.InitTimeStemp = 0.0;

            ShopContainer.m_shopInfo.Add(playerShopInfo.Id, playerShopInfo);

            return playerShopInfo;
        }
        //------------------------------------------------------------------------------------
    }
}