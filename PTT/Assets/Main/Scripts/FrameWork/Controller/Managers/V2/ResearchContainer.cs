using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class PlayerResearchInfo
    {
        public ObscuredInt Id = -1;
        public ObscuredDouble CompleteTime = 0;
        public ObscuredInt Level = Define.PlayerJewelryDefaultLevel;
    }

    public class PlayerResearchSlotInfo
    {
        public ObscuredInt Id = -1;
        public ObscuredInt ResearchTarget = -1;
        public ObscuredInt IsLock = 1;

        public void SetResearchTarget(int researchTarget)
        {
            ResearchTarget = researchTarget;
        }
    }

    public static class ResearchContainer
    {
        public static Dictionary<int, PlayerResearchInfo> ResearchInfo = new Dictionary<int, PlayerResearchInfo>();
        public static Dictionary<int, PlayerResearchSlotInfo> ResearchSlot = new Dictionary<int, PlayerResearchSlotInfo>();

        public static ObscuredInt TodayAdViewCount = 0;
        public static ObscuredDouble DailyInitTime = 0;

        public static ObscuredDouble ReserchLastChargeTime = 0;
        public static ObscuredDouble ChargeResearchCount = 0;

        public static List<int> ResearchEndViewQueue = new List<int>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in ResearchInfo)
            {
                SerializeString.Append(pair.Value.Id);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.CompleteTime);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Level);
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

                PlayerResearchInfo skillInfo = new PlayerResearchInfo();
                skillInfo.Id = arrcontent[0].ToInt();
                skillInfo.CompleteTime = arrcontent[1].ToDouble();
                skillInfo.Level = arrcontent[2].ToInt();

                ResearchInfo.Add(skillInfo.Id, skillInfo);
            }
        }

        public static string GetSlotSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in ResearchSlot)
            {
                SerializeString.Append(pair.Value.Id);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.ResearchTarget);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.IsLock);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetSlotDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                PlayerResearchSlotInfo playerResearchSlotInfo = new PlayerResearchSlotInfo();
                playerResearchSlotInfo.Id = arrcontent[0].ToInt();
                playerResearchSlotInfo.ResearchTarget = arrcontent[1].ToInt();
                playerResearchSlotInfo.IsLock = arrcontent[2].ToInt();

                ResearchSlot.Add(playerResearchSlotInfo.Id, playerResearchSlotInfo);
            }
        }


        public static string GetViewQueueSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in ResearchEndViewQueue)
            {
                SerializeString.Append(pair);
                SerializeString.Append(',');

            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetViewQueueDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split(',');

            for (int i = 0; i < arr.Length; ++i)
            {
                ResearchEndViewQueue.Add(arr[i].ToInt());
            }
        }
    }

    public static class ResearchOperator
    {
        private static ResearchLocalTable _researchLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            _researchLocalTable = Managers.TableManager.Instance.GetTableClass<ResearchLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static ResearchData GetResearchData(int index)
        {
            return _researchLocalTable.GetData(index);
        }
        //------------------------------------------------------------------------------------
        public static List<ResearchData> GetNoneRootData()
        {
            return _researchLocalTable.GetNoneRootData();
        }
        //------------------------------------------------------------------------------------
        public static List<List<ResearchData>> GetNormalAllData()
        {
            return _researchLocalTable.GetNormalAllData();
        }
        //------------------------------------------------------------------------------------
        public static ResearchOpenConditionData GetResearchOpenConditionData(int index)
        {
            return _researchLocalTable.GetResearchOpenConditionData(index);
        }
        //------------------------------------------------------------------------------------
        public static PlayerResearchInfo GetPlayerResearchInfo(ResearchData masteryData)
        {
            if (ResearchContainer.ResearchInfo.ContainsKey(masteryData.Index) == true)
                return ResearchContainer.ResearchInfo[masteryData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public static PlayerResearchInfo AddNewPlayerResearchInfo(ResearchData masteryData)
        {
            if (ResearchContainer.ResearchInfo.ContainsKey(masteryData.Index) == true)
                return ResearchContainer.ResearchInfo[masteryData.Index];
            else
            {
                PlayerResearchInfo playerResearchInfo = new PlayerResearchInfo();
                playerResearchInfo.Id = masteryData.Index;
                playerResearchInfo.Level = Define.PlayerJewelryDefaultLevel;
                playerResearchInfo.CompleteTime = 0;
                ResearchContainer.ResearchInfo.Add(playerResearchInfo.Id, playerResearchInfo);

                return playerResearchInfo;
            }
        }
        //------------------------------------------------------------------------------------
        public static void RemovePlayerResearchInfo(ResearchData masteryData)
        {
            PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(masteryData);
            if (playerResearchInfo != null)
            {
                ResearchContainer.ResearchInfo.Remove(playerResearchInfo.Id);
            }
        }
        //------------------------------------------------------------------------------------
        public static double GetNeedLevelUPSpPoint(ResearchData masteryData)
        {
            if (masteryData == null)
                return double.MaxValue;

            PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(masteryData);

            int level = playerResearchInfo == null ? 0 : playerResearchInfo.Level;

            return masteryData.LevelUpCostGoodsParam2 + (masteryData.LevelUpCostGoodsParam2 * level);
        }
        //------------------------------------------------------------------------------------
        public static double GetOwnEffectValue(ResearchData masteryData)
        {
            if (masteryData == null)
                return 0.0;

            int mylevel = Define.PlayerJewelryDefaultLevel;

            PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(masteryData);
            if (playerResearchInfo != null)
            {
                mylevel = playerResearchInfo.Level;
            }
            else
                return 0.0;

            return masteryData.ResearchEffectTypeLevelUpValue * mylevel;
        }
        //------------------------------------------------------------------------------------
        public static long GetResearchAccumCount(List<List<ResearchData>> researchDatas)
        {
            if (researchDatas == null)
                return 0;

            int accumtotalcount = 0;

            try
            {
                

                for (int i = 0; i < researchDatas.Count; ++i)
                {
                    for (int j = 0; j < researchDatas[i].Count; ++j)
                    {
                        PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(researchDatas[i][j]);
                        if (playerResearchInfo == null)
                            continue;

                        accumtotalcount += playerResearchInfo.Level;
                    }
                }
            }
            catch
            {
                return 0;
            }

            return accumtotalcount;
        }
        //------------------------------------------------------------------------------------
        public static double GetOwnEffectValue_NextLevel(ResearchData researchData)
        {
            if (researchData == null)
                return 0.0;

            int mylevel = Define.PlayerJewelryDefaultLevel;

            PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(researchData);
            if (playerResearchInfo != null)
            {
                mylevel = playerResearchInfo.Level;
            }

            mylevel++;

            return researchData.ResearchEffectTypeLevelUpValue * mylevel;
        }
        //------------------------------------------------------------------------------------
    }
}
