using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class SynergyContainer
    {
        // 플레이어가 소유한 시너지
        public static Dictionary<ObscuredInt, SkillInfo> SynergyInfo = new Dictionary<ObscuredInt, SkillInfo>();

        // 플레이어 장착 시너지 <SlotID, ArrrSkillId>
        public static Dictionary<V2Enum_ARR_SynergyType, Dictionary<ObscuredInt, SynergyEffectData>> SynergyEquip_Dic = new Dictionary<V2Enum_ARR_SynergyType, Dictionary<ObscuredInt, SynergyEffectData>>();

        // 플레이어 장착 시너지 <SlotID, ArrrSkillId>
        public static Dictionary<V2Enum_ARR_SynergyType, List<ObscuredInt>> TempSynergyEquipData = new Dictionary<V2Enum_ARR_SynergyType, List<ObscuredInt>>();

        // 플레이어 신규 시너지 <SynergyIndex>
        public static Dictionary<SynergyEffectData, int> NewSynergys = new Dictionary<SynergyEffectData, int>();

        
        
        public static HashSet<ObscuredInt> Runes = new HashSet<ObscuredInt>();

        // 플레이어 신규 룬 <RuneIndex>
        public static Dictionary<SynergyBreakthroughData, int> NewRunes = new Dictionary<SynergyBreakthroughData, int>();



        public static ObscuredInt SynergyEffectCurrentTotalLevel = 0;
        public static ObscuredInt SynergyEffectAccumLevel = 0;


        public static ObscuredInt SynergyServerRecvExp = 0;
        public static ObscuredInt SynergyContentExp = 0;

        public static SynergyTotalLevelCostData SynergyTotalLevelCostData = new SynergyTotalLevelCostData();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSynergyInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in SynergyInfo)
            {
                SerializeString.Append(pair.Value.Id - 107021100);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Count);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Level);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.LimitCompleteLevel);
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

                SkillInfo skillInfo = new SkillInfo();
                skillInfo.Id = arrcontent[0].ToInt() + 107021100;
                skillInfo.Count = arrcontent[1].ToInt();
                skillInfo.Level = arrcontent[2].ToInt();
                skillInfo.LimitCompleteLevel = arrcontent[3].ToInt();

                SynergyInfo.Add(skillInfo.Id, skillInfo);
            }
        }

        public static string GetSynergyInfoSerializeString(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType)
        {
            SerializeString.Clear();

            foreach (var pair in SynergyInfo)
            {
                SynergyEffectData synergyEffectData = SynergyOperator.GetSynergyEffectData(pair.Value.Id);
                if (synergyEffectData == null)
                    continue;

                if (synergyEffectData.SynergyType != v2Enum_ARR_SynergyType)
                    continue;

                SerializeString.Append(pair.Value.Id - 107021100);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Level);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static string GetSynergyInfoEquipSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in SynergyEquip_Dic)
            {
                SerializeString.Append(pair.Key.Enum32ToInt());
                SerializeString.Append(':');

                foreach (var pairkey in pair.Value)
                {
                    SerializeString.Append(pairkey.Value.Index - 107021100);
                    SerializeString.Append(',');
                }

                if (pair.Value.Count > 0)
                    SerializeString.Remove(SerializeString.Length - 1, 1);

                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static string GetLogEquipInfo()
        {
            SerializeString.Clear();

            foreach (var pair in SynergyEquip_Dic)
            {
                foreach (var pairkey in pair.Value)
                {
                    if (SynergyInfo.ContainsKey(pairkey.Value.Index) == true)
                    { 
                        SkillInfo skillInfo = SynergyInfo[pairkey.Value.Index];

                        SerializeString.Append(skillInfo.Level);
                        SerializeString.Append(',');
                    }

                }

                if (pair.Value.Count > 0)
                    SerializeString.Remove(SerializeString.Length - 1, 1);

                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetSynergyInfoEquipSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(':');

                V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType = arrcontent[0].ToInt().IntToEnum32<V2Enum_ARR_SynergyType>();

                if (TempSynergyEquipData.ContainsKey(v2Enum_ARR_SynergyType) == false)
                    TempSynergyEquipData.Add(v2Enum_ARR_SynergyType, new List<ObscuredInt>());

                if (arrcontent.Length > 1)
                {
                    string[] arrcontentindex = arrcontent[1].Split(',');

                    for (int j = 0; j < arrcontentindex.Length; ++j)
                    {
                        TempSynergyEquipData[v2Enum_ARR_SynergyType].Add(arrcontentindex[j].ToInt() + 107021100);
                    }
                }
            }
        }



        public static string GetRuneInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in Runes)
            {
                SerializeString.Append(pair - 107031000);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetRuneInfoDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                Runes.Add(arr[i].ToInt() + 107031000);
            }
        }



        public static string GetNewSynergySerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in NewSynergys)
            {
                if (pair.Key == null)
                    continue;

                SerializeString.Append(pair.Key.Index - 107031000);
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static string GetNewRuneSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in NewRunes)
            {
                if (pair.Key == null)
                    continue;

                SerializeString.Append(pair.Key.Index - 107031000);
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }
    }

    public static class SynergyOperator
    {
        private static SynergyLocalTable _synergyLocalTable = null;

        private static System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            _synergyLocalTable = Managers.TableManager.Instance.GetTableClass<SynergyLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<V2Enum_ARR_SynergyType, SynergyData> GetAllGambleSynergyData()
        {
            return _synergyLocalTable.GetAllGambleSynergyData();
        }
        //------------------------------------------------------------------------------------
        public static SynergyData GetGambleSynergyData(V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType)
        {
            return _synergyLocalTable.GetGambleSynergyData(v2Enum_ARR_GambleSynergyType);
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, SynergyEffectData> GetAllSynergyEffectDatas()
        {
            return _synergyLocalTable.GetAllSynergyEffectDatas();
        }
        //------------------------------------------------------------------------------------
        public static SynergyEffectData GetSynergyEffectData(ObscuredInt index)
        {
            return _synergyLocalTable.GetSynergyEffectData(index);
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<V2Enum_ARR_SynergyType, List<SynergyEffectData>> GetAllGambleSynergyEffectDataList()
        {
            return _synergyLocalTable.GetAllGambleSynergyEffectDataList();
        }
        //------------------------------------------------------------------------------------
        public static List<SynergyEffectData> GetGambleSynergyEffectDataList(V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType)
        {
            return _synergyLocalTable.GetGambleSynergyEffectDataList(v2Enum_ARR_GambleSynergyType);
        }
        //------------------------------------------------------------------------------------
        public static SynergyLevelUpCostData GetSynergyLevelUpCostData(V2Enum_Grade v2Enum_Grade)
        {
            return _synergyLocalTable.GetSynergyLevelUpCostData(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public static List<SynergyCombineData> GetAllGambleSynergyCombineData()
        {
            return _synergyLocalTable.GetAllGambleSynergyCombineData();
        }
        //------------------------------------------------------------------------------------
        public static SynergyBreakthroughCostData GetSynergyLevelUpLimitData(V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType, ObscuredInt level)
        {
            return _synergyLocalTable.GetSynergyLevelUpLimitData(v2Enum_ARR_GambleSynergyType, level);
        }
        //------------------------------------------------------------------------------------
        //public static Dictionary<ObscuredInt, SynergyLevelUpLimitData> GetAllSynergyGradeLevelUpLimitData(V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType, V2Enum_Grade v2Enum_Grade)
        //{
        //    return _synergyLocalTable.GetAllSynergyGradeLevelUpLimitData(v2Enum_ARR_GambleSynergyType, v2Enum_Grade);
        //}
        //------------------------------------------------------------------------------------
        public static SynergyDuplicationData GetSynergyDuplicationData(V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType, V2Enum_Grade v2Enum_Grade)
        {
            return _synergyLocalTable.GetSynergyDuplicationData(v2Enum_ARR_GambleSynergyType, v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public static V2Enum_ARR_SynergyType ConvertEffectToSynergyType(V2Enum_SkillEffectType v2Enum_SkillEffectType)
        {
            if (v2Enum_SkillEffectType == V2Enum_SkillEffectType.GetSynergyFire
                || v2Enum_SkillEffectType == V2Enum_SkillEffectType.ReduceSynergyFire)
                return V2Enum_ARR_SynergyType.Red;
            else if (v2Enum_SkillEffectType == V2Enum_SkillEffectType.GetSynergyGold
                || v2Enum_SkillEffectType == V2Enum_SkillEffectType.ReduceSynergyGold)
                return V2Enum_ARR_SynergyType.Yellow;
            else if (v2Enum_SkillEffectType == V2Enum_SkillEffectType.GetSynergyWater
                || v2Enum_SkillEffectType == V2Enum_SkillEffectType.ReduceSynergyWater)
                return V2Enum_ARR_SynergyType.Blue;
            else if (v2Enum_SkillEffectType == V2Enum_SkillEffectType.GetSynergyThunder
                || v2Enum_SkillEffectType == V2Enum_SkillEffectType.ReduceSynergyThunder)
                return V2Enum_ARR_SynergyType.White;

            return V2Enum_ARR_SynergyType.Max;
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, SynergyBreakthroughData> GetAllSynergyRuneData()
        {
            return _synergyLocalTable.GetAllSynergyRuneData();
        }
        //------------------------------------------------------------------------------------
        public static SynergyBreakthroughData GetSynergyRuneData(ObscuredInt index)
        {
            return _synergyLocalTable.GetSynergyRuneData(index);
        }
        //------------------------------------------------------------------------------------
        public static SynergyDuplicationData GetSynergyRuneDuplicationData(V2Enum_Grade v2Enum_Grade)
        {
            return _synergyLocalTable.GetSynergyRuneDuplicationData(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public static SynergyReinforceStatData GetSynergyReinforceStatData(ObscuredInt level)
        {
            return _synergyLocalTable.GetSynergyReinforceStatData(level - 1);
        }
        //------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------
        public static string GetCardGradeLimitBreakLocalString(int level)
        {
            List<GambleGradeProbData> gambleProbDatas = GambleOperator.GetGambleProbDatas(level);

            double totalProb = 0;

            _stringBuilder.Clear();

            for (int i = 0; i < gambleProbDatas.Count; ++i)
            {
                totalProb += gambleProbDatas[i].GambleProb;
            }

            //_stringBuilder.Append(string.Format("{0} : <color=#1FF0FF>{1}</color>", Managers.LocalStringManager.Instance.GetLocalString("gamblelevel/desc"), level));

            //_stringBuilder.Append("\n");
            //_stringBuilder.Append("\n");

            //_stringBuilder.Append(Managers.LocalStringManager.Instance.GetLocalString("gamblelevel/subdesc"));



            for (int i = 0; i < gambleProbDatas.Count; ++i)
            {
                GambleGradeProbData gambleGradeProbData = gambleProbDatas[i];

                if (i == 0)
                    _stringBuilder.Append(string.Format("{0} : <color=#1FF0FF>{1:0.##}%</color>", Managers.LocalStringManager.Instance.GetLocalString(string.Format("gamblegrade/desc/{0}", gambleGradeProbData.CardGambleGrade)),
                        (gambleGradeProbData.GambleProb / totalProb) * 100));
                else
                    _stringBuilder.Append(string.Format("\n{0} : <color=#1FF0FF>{1:0.##}%</color>", Managers.LocalStringManager.Instance.GetLocalString(string.Format("gamblegrade/desc/{0}", gambleGradeProbData.CardGambleGrade)),
                        (gambleGradeProbData.GambleProb / totalProb) * 100));
            }

            return _stringBuilder.ToString();
        }
        //------------------------------------------------------------------------------------
        public static string GetCardGradeLimitBreakLocalString_Origin(int level)
        {
            List<GambleGradeProbData> gambleProbDatas = GambleOperator.GetGambleProbDatas(level);

            double totalProb = 0;

            _stringBuilder.Clear();

            for (int i = 0; i < gambleProbDatas.Count; ++i)
            {
                totalProb += gambleProbDatas[i].GambleProb;
            }

            _stringBuilder.Append(string.Format("{0} : <color=#1FF0FF>{1}</color>", Managers.LocalStringManager.Instance.GetLocalString("gamblelevel/desc"), level));

            _stringBuilder.Append("\n");
            _stringBuilder.Append("\n");

            _stringBuilder.Append(Managers.LocalStringManager.Instance.GetLocalString("gamblelevel/subdesc"));



            for (int i = 0; i < gambleProbDatas.Count; ++i)
            {
                GambleGradeProbData gambleGradeProbData = gambleProbDatas[i];

                //if (i == 0)
                //    _stringBuilder.Append(string.Format("{0} : {1:0.##}", Managers.LocalStringManager.Instance.GetLocalString(string.Format("gamblegrade/desc/{0}", gambleGradeProbData.CardGambleGrade)),
                //        (gambleGradeProbData.GambleProb / totalProb) * 100));
                //else
                _stringBuilder.Append(string.Format("\n{0} : <color=#1FF0FF>{1:0.##}%</color>", Managers.LocalStringManager.Instance.GetLocalString(string.Format("gamblegrade/desc/{0}", gambleGradeProbData.CardGambleGrade)),
                    (gambleGradeProbData.GambleProb / totalProb) * 100));
            }

            return _stringBuilder.ToString();
        }
        //------------------------------------------------------------------------------------
    }
}