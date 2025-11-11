using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class DungeonInitInfo
    {
        public V2Enum_Dungeon V2Enum_Dungeon;
        public ObscuredInt ToDayAdEnterCount = 0;
        public ObscuredDouble InitTimeStemp = 0;
    }

    public static class DungeonDataContainer
    {
        public static ObscuredInt Diamond_Dungeon_MaxClear = 0;
        public static ObscuredInt Tower_Dungeon_MaxClear = 0;

        public static Dictionary<V2Enum_Dungeon, DungeonInitInfo> m_dungeonInitInfo = new Dictionary<V2Enum_Dungeon, DungeonInitInfo>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetDungeonInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in m_dungeonInitInfo)
            {
                SerializeString.Append(pair.Value.V2Enum_Dungeon.Enum32ToInt());
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.ToDayAdEnterCount);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.InitTimeStemp);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetDungeonInfoDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                DungeonInitInfo dungeonInitInfo = new DungeonInitInfo();
                dungeonInitInfo.V2Enum_Dungeon = arrcontent[0].ToInt().IntToEnum32<V2Enum_Dungeon>();
                dungeonInitInfo.ToDayAdEnterCount = arrcontent[1].ToInt();
                dungeonInitInfo.InitTimeStemp = arrcontent[2].ToDouble();

                m_dungeonInitInfo.Add(dungeonInitInfo.V2Enum_Dungeon, dungeonInitInfo);
            }
        }

        public static string GetDungeonRecordSerializeString(Dictionary<V2Enum_DungeonDifficultyType, ObscuredInt> record)
        {
            SerializeString.Clear();

            foreach (var pair in record)
            {
                SerializeString.Append(pair.Value.GetDecrypted());
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetDungeonRecordSerializeString(string data, Dictionary<V2Enum_DungeonDifficultyType, ObscuredInt> record)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split(',');

            for (int i = 0; i < arr.Length; ++i)
            {
                V2Enum_DungeonDifficultyType v2Enum_DungeonDifficultyType = (i + V2Enum_DungeonDifficultyType.Normal.Enum32ToInt()).IntToEnum32<V2Enum_DungeonDifficultyType>();

                record.Add(v2Enum_DungeonDifficultyType, arr[i].ToInt());
            }
        }

        public static string GetDungeonRecordSerializeString(Dictionary<V2Enum_DungeonDifficultyType, ObscuredDouble> record)
        {
            SerializeString.Clear();

            foreach (var pair in record)
            {
                SerializeString.Append(pair.Value.GetDecrypted());
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetDungeonRecordSerializeString(string data, Dictionary<V2Enum_DungeonDifficultyType, ObscuredDouble> record)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split(',');

            for (int i = 0; i < arr.Length; ++i)
            {
                V2Enum_DungeonDifficultyType v2Enum_DungeonDifficultyType = (i + V2Enum_DungeonDifficultyType.Normal.Enum32ToInt()).IntToEnum32<V2Enum_DungeonDifficultyType>();

                record.Add(v2Enum_DungeonDifficultyType, arr[i].ToDouble());
            }
        }
    }

    public static class DungeonDataOperator
    {
        private static DungeonLocalTable m_dungeonLocalTable;
        private static DiamondDungeonLocalTable m_diamondDungeonLocalTable;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            m_dungeonLocalTable = Managers.TableManager.Instance.GetTableClass<DungeonLocalTable>();
            m_diamondDungeonLocalTable = Managers.TableManager.Instance.GetTableClass<DiamondDungeonLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static List<DungeonData> GetDungeonAllData()
        {
            return m_dungeonLocalTable.GetDungeonAllData();
        }
        //------------------------------------------------------------------------------------
        public static DungeonData GetDungeonData(V2Enum_Dungeon v2Enum_Dungeon)
        {
            return m_dungeonLocalTable.GetData(v2Enum_Dungeon);
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<V2Enum_Dungeon, DungeonInitInfo> GetDungeonAdAllInfo()
        {
            return DungeonDataContainer.m_dungeonInitInfo;
        }
        //------------------------------------------------------------------------------------
        public static DungeonInitInfo GetDungeonInitInfo(V2Enum_Dungeon v2Enum_Dungeon)
        {
            if (DungeonDataContainer.m_dungeonInitInfo.ContainsKey(v2Enum_Dungeon) == true)
                return DungeonDataContainer.m_dungeonInitInfo[v2Enum_Dungeon];

            return null;
        }
        //------------------------------------------------------------------------------------
        #region DiaDungeon
        //------------------------------------------------------------------------------------
        public static DungeonModeBase GetDiamondDungeonData(int step)
        {
            return m_diamondDungeonLocalTable.GetDiaData(step);
        }
        //------------------------------------------------------------------------------------
        public static DungeonModeBase GetMaxEnterDiamondDungeonData()
        {
            DungeonModeBase diamondDungeonData = null;

            int record = (int)GetDungeonRecord(V2Enum_Dungeon.DiamondDungeon);

            if (record <= 0)
                diamondDungeonData = GetDiamondDungeonData(1);
            else
            {
                diamondDungeonData = GetDiamondDungeonData(record);
                if (diamondDungeonData.NextData != null)
                    diamondDungeonData = GetDiamondDungeonData(diamondDungeonData.NextData.DungeonNumber);
            }

            return diamondDungeonData;
        }
        //------------------------------------------------------------------------------------
        public static DungeonModeBase GetMaxClearDiamondDungeonData()
        {
            return GetDiamondDungeonData((int)GetDungeonRecord(V2Enum_Dungeon.DiamondDungeon));
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TowerDungeon
        //------------------------------------------------------------------------------------
        public static DungeonModeBase GetTowerDungeonData(int step)
        {
            return m_diamondDungeonLocalTable.GetTowerData(step);
        }
        //------------------------------------------------------------------------------------
        public static DungeonModeBase GetMaxEnterTowerDungeonData()
        {
            DungeonModeBase diamondDungeonData = null;

            int record = (int)GetDungeonRecord(V2Enum_Dungeon.TowerDungeon);

            if (record <= 0)
                diamondDungeonData = GetTowerDungeonData(1);
            else
            {
                diamondDungeonData = GetTowerDungeonData(record);
                if (diamondDungeonData.NextData != null)
                    diamondDungeonData = GetTowerDungeonData(diamondDungeonData.NextData.DungeonNumber);
            }

            return diamondDungeonData;
        }
        //------------------------------------------------------------------------------------
        public static DungeonModeBase GetMaxClearTowerDungeonData()
        {
            return GetTowerDungeonData((int)GetDungeonRecord(V2Enum_Dungeon.TowerDungeon));
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        public static DungeonModeBase GetMaxEnterDungeonModeData(V2Enum_Dungeon v2Enum_Dungeon)
        {
            switch (v2Enum_Dungeon)
            {
                case V2Enum_Dungeon.DiamondDungeon:
                    {
                        return GetMaxEnterDiamondDungeonData();
                    }
                case V2Enum_Dungeon.TowerDungeon:
                    {
                        return GetMaxEnterTowerDungeonData();
                    }
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public static DungeonModeBase GetMaxClearDungeonModeData(V2Enum_Dungeon v2Enum_Dungeon)
        {
            switch (v2Enum_Dungeon)
            {
                case V2Enum_Dungeon.DiamondDungeon:
                    {
                        return GetMaxClearDiamondDungeonData();
                    }
                case V2Enum_Dungeon.TowerDungeon:
                    {
                        return GetMaxClearTowerDungeonData();
                    }
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public static double GetDungeonRecord(V2Enum_Dungeon v2Enum_Dungeon)
        {
            switch (v2Enum_Dungeon)
            {
                case V2Enum_Dungeon.DiamondDungeon:
                    {
                        return DungeonDataContainer.Diamond_Dungeon_MaxClear;
                    }
                case V2Enum_Dungeon.TowerDungeon:
                    {
                        return DungeonDataContainer.Tower_Dungeon_MaxClear;
                    }
            }

            return 1;
        }
        //------------------------------------------------------------------------------------
        public static bool SetDungeonRecord(V2Enum_Dungeon v2Enum_Dungeon, double record)
        {
            if (GetDungeonRecord(v2Enum_Dungeon) >= record)
                return false;

            switch (v2Enum_Dungeon)
            {
                case V2Enum_Dungeon.DiamondDungeon:
                    {
                        int step = (int)record;
                        DungeonDataContainer.Diamond_Dungeon_MaxClear = step;
                        break;
                    }
                case V2Enum_Dungeon.TowerDungeon:
                    {
                        int step = (int)record;
                        DungeonDataContainer.Tower_Dungeon_MaxClear = step;
                        break;
                    }
            }

            return true;
        }
        //------------------------------------------------------------------------------------
    }
}