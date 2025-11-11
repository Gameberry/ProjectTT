using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class ShopFreeGoodsInfo
    {
        public ObscuredInt Index = 0;

        public ObscuredInt DiaFreeRecvCount = 0;
        public ObscuredInt DiaAdViewRecvCount = 0;

        public ObscuredDouble InitTime = 0;
    }

    public static class ShopRandomStoreContainer
    {
        public static ObscuredDouble DailyInitTimeStemp = 0;

        public static List<ObscuredInt> BuyRandomStoreInfo = new List<ObscuredInt>();

        public static List<ObscuredInt> StoreDisPlayList = new List<ObscuredInt>();

        public static Dictionary<ObscuredInt, ShopFreeGoodsInfo> ShopFreeGoodsInfos = new Dictionary<ObscuredInt, ShopFreeGoodsInfo>();

        public static ObscuredInt StoreResetAdViewCount = 0;
        public static ObscuredInt StoreResetDiaCount = 0;

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetBuyHonorShopInfoSerializeString()
        {
            SerializeString.Clear();

            for (int i = 0; i < BuyRandomStoreInfo.Count; ++i)
            {
                SerializeString.Append(BuyRandomStoreInfo[i] - 150050000);
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetBuyHonorShopInfoDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split(',');

            for (int i = 0; i < arr.Length; ++i)
            {
                BuyRandomStoreInfo.Add(arr[i].ToInt() + 150050000);
            }
        }


        public static string GetHonorShopDisPlayListSerializeString()
        {
            SerializeString.Clear();

            for (int i = 0; i < StoreDisPlayList.Count; ++i)
            {
                SerializeString.Append(StoreDisPlayList[i] - 150050000);
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetHonorShopDisPlayListDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split(',');

            for (int i = 0; i < arr.Length; ++i)
            {
                StoreDisPlayList.Add(arr[i].ToInt() + 150050000);
            }
        }


        public static string GetSynergyInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in ShopFreeGoodsInfos)
            {
                SerializeString.Append(pair.Value.Index - 150060000);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.DiaFreeRecvCount);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.DiaAdViewRecvCount);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.InitTime);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetSynergyInfoDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                ShopFreeGoodsInfo skillInfo = new ShopFreeGoodsInfo();
                skillInfo.Index = arrcontent[0].ToInt() + 150060000;
                skillInfo.DiaFreeRecvCount = arrcontent[1].ToInt();
                skillInfo.DiaAdViewRecvCount = arrcontent[2].ToInt();
                skillInfo.InitTime = arrcontent[3].ToDouble();

                ShopFreeGoodsInfos.Add(skillInfo.Index, skillInfo);
            }
        }
    }

    public static class ShopRandomStoreOperator
    {
        private static ShopRandomStoreLocalTable _shopRandomStoreLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            _shopRandomStoreLocalTable = Managers.TableManager.Instance.GetTableClass<ShopRandomStoreLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static List<ShopRandomStoreData> GetShopRandomStoreDatas()
        {
            return _shopRandomStoreLocalTable.GetShopRandomStoreDatas();
        }
        //------------------------------------------------------------------------------------
        public static List<ShopFreeGoodsData> GetShopFreeGoodsDatas()
        {
            return _shopRandomStoreLocalTable.GetShopFreeGoodsDatas();
        }
        //------------------------------------------------------------------------------------
    }
}