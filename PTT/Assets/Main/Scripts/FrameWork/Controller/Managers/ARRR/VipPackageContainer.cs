using System;
using System.Collections.Generic;
using Gpm.Ui;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class VipPackageInfo
    {
        public ObscuredInt Index;
        public ObscuredDouble PackageEndTime = -1.0;
        public ObscuredDouble NextSendDiaTime = -1.0;
    }

    public class VipPackageShopInfo
    {
        public ObscuredInt Index;
        public ObscuredDouble NextBuyTime;
    }

    public static class VipPackageContainer
    {
        public static Dictionary<ObscuredInt, VipPackageInfo> VipPackageInfo = new Dictionary<ObscuredInt, VipPackageInfo>();
        public static Dictionary<ObscuredInt, VipPackageShopInfo> VipPackageShopInfo = new Dictionary<ObscuredInt, VipPackageShopInfo>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in VipPackageInfo)
            {
                SerializeString.Append(pair.Value.Index);
                SerializeString.Append(',');
                SerializeString.Append((long)pair.Value.PackageEndTime);
                SerializeString.Append(',');
                SerializeString.Append((long)pair.Value.NextSendDiaTime);
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

                VipPackageInfo vipPackageInfo = new VipPackageInfo();
                vipPackageInfo.Index = arrcontent[0].ToInt();
                vipPackageInfo.PackageEndTime = arrcontent[1].ToDouble();
                vipPackageInfo.NextSendDiaTime = arrcontent[2].ToDouble();

                VipPackageInfo.Add(vipPackageInfo.Index, vipPackageInfo);
            }
        }

        public static string GetShopSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in VipPackageShopInfo)
            {
                SerializeString.Append(pair.Value.Index);
                SerializeString.Append(',');
                SerializeString.Append((long)pair.Value.NextBuyTime);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetShopDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                VipPackageShopInfo vipPackageInfo = new VipPackageShopInfo();
                vipPackageInfo.Index = arrcontent[0].ToInt();
                vipPackageInfo.NextBuyTime = arrcontent[1].ToDouble();

                VipPackageShopInfo.Add(vipPackageInfo.Index, vipPackageInfo);
            }
        }
    }

    public static class VipPackageOperator
    {
        public static VipPackageLocalTable _vipPackageLocalTable;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            _vipPackageLocalTable = Managers.TableManager.Instance.GetTableClass<VipPackageLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, VipPackageData> GetVipPackageDatas()
        {
            return _vipPackageLocalTable.GetVipPackageDatas();
        }
        //------------------------------------------------------------------------------------
        public static VipPackageData GetVipPackageData(int index)
        {
            return _vipPackageLocalTable.GetVipPackageData(index);
        }
        //------------------------------------------------------------------------------------
        public static List<VipPackageShopData> GetVipPackageShopDatas()
        {
            return _vipPackageLocalTable.GetVipPackageShopDatas();
        }
        //------------------------------------------------------------------------------------
    }
}