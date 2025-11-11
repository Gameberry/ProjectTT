using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class SummonInfo
    {
        public ObscuredInt Id;
        public ObscuredInt Level = Define.PlayerDefaultSummonLevel;
        public ObscuredLong AccumCount = 0;
        public ObscuredInt Count = 0;
        public ObscuredInt AdSummonCount = 0;
        public ObscuredInt ToDayAdSummonCount = 0;
        public ObscuredDouble InitTimeStemp = 0.0;
        public ObscuredLong UseConfirmCount = 0;
    }

    public static class SummonContainer
    {
        public static Dictionary<V2Enum_SummonType, SummonInfo> m_summonInfo = new Dictionary<V2Enum_SummonType, SummonInfo>();
        public static string SummonLevelStringArr = "1, 1, 1, 1, 1, 1";

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in m_summonInfo)
            {
                SerializeString.Append(pair.Key.Enum32ToInt());
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Id - 130010000);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Level);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.AccumCount);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Count);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.AdSummonCount);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.ToDayAdSummonCount);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.InitTimeStemp);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.UseConfirmCount);
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

                SummonInfo dungeonInitInfo = new SummonInfo();
                dungeonInitInfo.Id = arrcontent[1].ToInt() + 130010000;
                dungeonInitInfo.Level = arrcontent[2].ToInt();
                dungeonInitInfo.AccumCount = arrcontent[3].ToLong();
                dungeonInitInfo.Count = arrcontent[4].ToInt();
                dungeonInitInfo.AdSummonCount = arrcontent[5].ToInt();
                dungeonInitInfo.ToDayAdSummonCount = arrcontent[6].ToInt();
                dungeonInitInfo.InitTimeStemp = arrcontent[7].ToDouble();
                dungeonInitInfo.UseConfirmCount = arrcontent[8].ToLong();

                m_summonInfo.Add(arrcontent[0].ToInt().IntToEnum32<V2Enum_SummonType>(), dungeonInitInfo);
            }
        }
    }

    public static class SummonOperator
    {
        private static SummonLocalTable m_summonLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            m_summonLocalTable = Managers.TableManager.Instance.GetTableClass<SummonLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<V2Enum_SummonType, SummonData> GetSummonDatas()
        {
            return m_summonLocalTable.GetSummonDatas();
        }
        //------------------------------------------------------------------------------------
        public static SummonData GetSummonData(V2Enum_SummonType v2Enum_SummonType)
        {
            return m_summonLocalTable.GetSummonData(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public static SummonGroupData GetSummonGroupData(V2Enum_SummonType v2Enum_SummonType)
        {
            return m_summonLocalTable.GetSummonGroupData(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public static SummonConfirmCountData GetSummonConfirmCountData(V2Enum_SummonType v2Enum_SummonType)
        {
            return m_summonLocalTable.GetSummonConfirmCountData(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public static SummonGroupData GetSummonConfirmDrawGroupData(V2Enum_SummonType v2Enum_SummonType)
        {
            return m_summonLocalTable.GetSummonConfirmDrawGroupData(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public static SummonInfo GetSummonInfo(V2Enum_SummonType v2Enum_SummonType)
        {
            if (SummonContainer.m_summonInfo.ContainsKey(v2Enum_SummonType) == true)
                return SummonContainer.m_summonInfo[v2Enum_SummonType];

            return null;
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<V2Enum_SummonType, SummonInfo> GetAllSummonInfo()
        {
            return SummonContainer.m_summonInfo;
        }
        //------------------------------------------------------------------------------------
        public static long GetAccumCount(V2Enum_SummonType v2Enum_SummonType)
        {
            SummonInfo summonInfo = GetSummonInfo(v2Enum_SummonType);
            if (summonInfo == null)
                return 0;

            return summonInfo.AccumCount;
        }
        //------------------------------------------------------------------------------------
        public static long GetUseConfirmCount(V2Enum_SummonType v2Enum_SummonType)
        {
            SummonInfo summonInfo = GetSummonInfo(v2Enum_SummonType);
            if (summonInfo == null)
                return 0;

            return summonInfo.UseConfirmCount;
        }
        //------------------------------------------------------------------------------------
        public static int GetAdSummonElementCount(V2Enum_SummonType v2Enum_SummonType)
        {
            SummonInfo summonInfo = GetSummonInfo(v2Enum_SummonType);
            SummonData summonData = GetSummonData(v2Enum_SummonType);

            int count = summonData.SummonCountViaAd + (summonData.SummonBonusCountViaAd * summonInfo.AdSummonCount);

            if (count > summonData.SummonMaxCountViaAd)
                count = summonData.SummonMaxCountViaAd;

            return count;
        }
        //------------------------------------------------------------------------------------
        public static int GetAdSummonRemainCount(V2Enum_SummonType v2Enum_SummonType)
        {
            SummonInfo summonInfo = GetSummonInfo(v2Enum_SummonType);
            SummonData summonData = GetSummonData(v2Enum_SummonType);

            return summonData.MaxAdViewCountPerDate - summonInfo.ToDayAdSummonCount;
        }
        //------------------------------------------------------------------------------------
        public static bool IsReadyAdSummon(V2Enum_SummonType v2Enum_SummonType)
        {
            if (v2Enum_SummonType == V2Enum_SummonType.SummonGear)
            {
                SummonInfo summonInfo = GetSummonInfo(v2Enum_SummonType);
                if (summonInfo == null
                    || summonInfo.AccumCount.GetDecrypted() <= 0)
                    return false;
            }
            
            
            return GetAdSummonRemainCount(v2Enum_SummonType) > 0;
        }
        //------------------------------------------------------------------------------------
        public static void SetLogSummonData()
        {
            string str = string.Empty;

            foreach (KeyValuePair<V2Enum_SummonType, SummonInfo> pair in GetAllSummonInfo())
            {
                SummonInfo summonInfo = pair.Value;

                if (pair.Key == V2Enum_SummonType.SummonGear)
                    str = summonInfo.Level.ToString();
                else
                    str = string.Format("{0}, {1}", str, summonInfo.Level);
            }

            SummonContainer.SummonLevelStringArr = str;
        }
        //------------------------------------------------------------------------------------
        public static List<SummonElementData> DoGachaSummon(V2Enum_SummonType v2Enum_SummonType, int count)
        {
            SummonInfo summonInfo = GetSummonInfo(v2Enum_SummonType);

            List<SummonElementData> summonElementDatas = new List<SummonElementData>();

            int pickcount = 0;

            while (count > pickcount)
            {
                SummonGroupData summonGroupData = GetSummonGroupData(v2Enum_SummonType);
                if (summonGroupData == null)
                    return null;

                if (v2Enum_SummonType == V2Enum_SummonType.SummonRelic && summonInfo.AccumCount == 0)
                {
                    SummonElementData tutodata = summonGroupData.SummonElementDatas.Find(x => x.GoodsIndex == 112010002);
                    if (tutodata == null)
                        summonElementDatas.Add(summonGroupData.WeightedRandomPicker.Pick());
                    else
                        summonElementDatas.Add(tutodata);
                }
                else if (v2Enum_SummonType == V2Enum_SummonType.SummonRune && summonInfo.AccumCount == 0)
                {
                    SummonElementData tutodata = summonGroupData.SummonElementDatas.Find(x => x.GoodsIndex == 118010013);
                    if (tutodata == null)
                        summonElementDatas.Add(summonGroupData.WeightedRandomPicker.Pick());
                    else
                        summonElementDatas.Add(tutodata);

                    Managers.GuideInteractorManager.Instance.SetGuideStep(99);
                }
                else if (v2Enum_SummonType == V2Enum_SummonType.SummonGear && summonInfo.AccumCount < 10)
                {
                    if(summonInfo.AccumCount == 0)
                    {
                        SummonElementData tutodata = summonGroupData.SummonElementDatas.Find(x => x.GoodsIndex == 111010003);
                        if (tutodata == null)
                            summonElementDatas.Add(summonGroupData.WeightedRandomPicker.Pick());
                        else
                            summonElementDatas.Add(tutodata);
                    }
                    else if (summonInfo.AccumCount == 1)
                    {
                        SummonElementData tutodata = summonGroupData.SummonElementDatas.Find(x => x.GoodsIndex == 111010008);
                        if (tutodata == null)
                            summonElementDatas.Add(summonGroupData.WeightedRandomPicker.Pick());
                        else
                            summonElementDatas.Add(tutodata);
                    }
                    else if (summonInfo.AccumCount == 2)
                    {
                        SummonElementData tutodata = summonGroupData.SummonElementDatas.Find(x => x.GoodsIndex == 111010013);
                        if (tutodata == null)
                            summonElementDatas.Add(summonGroupData.WeightedRandomPicker.Pick());
                        else
                            summonElementDatas.Add(tutodata);


                        Message.Send(new GameBerry.Event.HideGearTutorialGachaFocusMsg());

                        Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/gear2"));
                    }
                    else if (summonInfo.AccumCount == 8)
                    {
                        SummonElementData tutodata = summonGroupData.SummonElementDatas.Find(x => x.GoodsIndex == 111010026);
                        if (tutodata == null)
                            summonElementDatas.Add(summonGroupData.WeightedRandomPicker.Pick());
                        else
                            summonElementDatas.Add(tutodata);
                    }
                    else
                        summonElementDatas.Add(summonGroupData.WeightedRandomPicker.Pick());
                }
                else
                    summonElementDatas.Add(summonGroupData.WeightedRandomPicker.Pick());

                summonInfo.Count++;
                summonInfo.AccumCount += 1;
                pickcount++;
            }

            //Managers.ShopManager.Instance.RefreshProductContitionType(ShopOperator.ConvertSummonTypeToPConditionAccumType(v2Enum_SummonType));

            return summonElementDatas;
        }
        //------------------------------------------------------------------------------------
    }
}