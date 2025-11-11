using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class DungeonModeBase
    {
        public int Index;

        public int DungeonNumber;

        public int ClearRewardParam1;
        public double ClearRewardParam2;

        public V2Enum_DungeonDifficultyType DungeonDifficulty;

        public int MonsterSetIndex;
        public List<OperatorOverrideStat> BossDungeonOverrideStats = new List<OperatorOverrideStat>();

        public DungeonModeBase PrevData = null;
        public DungeonModeBase NextData = null;
    }

    public class DiamondDungeonLocalTable : LocalTableBase
    {
        private Dictionary<int, DungeonModeBase> m_diamondDungeonDatas_Dic = new Dictionary<int, DungeonModeBase>();

        private Dictionary<int, DungeonModeBase> m_towerDungeonDatas_Dic = new Dictionary<int, DungeonModeBase>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            List<DungeonModeBase> dungeonDatas = new List<DungeonModeBase>();

            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("DiamondDungeon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                DungeonModeBase diamondDungeonData = new DungeonModeBase();

                diamondDungeonData.Index = rows[i]["Index"].ToString().ToInt();
                diamondDungeonData.DungeonNumber = rows[i]["DungeonNumber"].ToString().ToInt();
                diamondDungeonData.MonsterSetIndex = rows[i]["MonsterSetIndex"].ToString().ToInt();

                try
                {
                    OperatorOverrideStat operatorOverrideStat = new OperatorOverrideStat();
                    operatorOverrideStat.BaseStat = V2Enum_Stat.Attack;
                    operatorOverrideStat.OverrideStatBaseValue = rows[i]["OverrideAttack"].ToString().ToDouble();

                    diamondDungeonData.BossDungeonOverrideStats.Add(operatorOverrideStat);
                }
                catch
                { 

                }

                try
                {
                    OperatorOverrideStat operatorOverrideStat = new OperatorOverrideStat();
                    operatorOverrideStat.BaseStat = V2Enum_Stat.HP;
                    operatorOverrideStat.OverrideStatBaseValue = rows[i]["OverrideHP"].ToString().ToDouble();

                    diamondDungeonData.BossDungeonOverrideStats.Add(operatorOverrideStat);
                }
                catch
                {

                }

                try
                {
                    OperatorOverrideStat operatorOverrideStat = new OperatorOverrideStat();
                    operatorOverrideStat.BaseStat = V2Enum_Stat.Defence;
                    operatorOverrideStat.OverrideStatBaseValue = rows[i]["OverrideDefence"].ToString().ToDouble();

                    diamondDungeonData.BossDungeonOverrideStats.Add(operatorOverrideStat);
                }
                catch
                {

                }




                diamondDungeonData.ClearRewardParam1 = V2Enum_Point.Dia.Enum32ToInt();
                diamondDungeonData.ClearRewardParam2 = rows[i]["ClearReward"].ToString().ToDouble();



                if (m_diamondDungeonDatas_Dic.ContainsKey(diamondDungeonData.DungeonNumber) == false)
                    m_diamondDungeonDatas_Dic.Add(diamondDungeonData.DungeonNumber, diamondDungeonData);

                dungeonDatas.Add(diamondDungeonData);
            }

            for (int i = 0; i < dungeonDatas.Count; ++i)
            {
                DungeonModeBase diamondDungeonData = dungeonDatas[i];

                if (i != 0)
                {
                    diamondDungeonData.PrevData = dungeonDatas[i - 1];
                }

                if (i != dungeonDatas.Count - 1)
                {
                    diamondDungeonData.NextData = dungeonDatas[i + 1];
                }
            }


            dungeonDatas.Clear();

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("TowerDungeon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                DungeonModeBase diamondDungeonData = new DungeonModeBase();

                diamondDungeonData.Index = rows[i]["Index"].ToString().ToInt();
                diamondDungeonData.DungeonNumber = rows[i]["DungeonNumber"].ToString().ToInt();
                diamondDungeonData.MonsterSetIndex = rows[i]["MonsterSetIndex"].ToString().ToInt();

                try
                {
                    OperatorOverrideStat operatorOverrideStat = new OperatorOverrideStat();
                    operatorOverrideStat.BaseStat = V2Enum_Stat.Attack;
                    operatorOverrideStat.OverrideStatBaseValue = rows[i]["OverrideAttack"].ToString().ToDouble();

                    diamondDungeonData.BossDungeonOverrideStats.Add(operatorOverrideStat);
                }
                catch
                {

                }

                try
                {
                    OperatorOverrideStat operatorOverrideStat = new OperatorOverrideStat();
                    operatorOverrideStat.BaseStat = V2Enum_Stat.HP;
                    operatorOverrideStat.OverrideStatBaseValue = rows[i]["OverrideHP"].ToString().ToDouble();

                    diamondDungeonData.BossDungeonOverrideStats.Add(operatorOverrideStat);
                }
                catch
                {

                }

                try
                {
                    OperatorOverrideStat operatorOverrideStat = new OperatorOverrideStat();
                    operatorOverrideStat.BaseStat = V2Enum_Stat.Defence;
                    operatorOverrideStat.OverrideStatBaseValue = rows[i]["OverrideDefence"].ToString().ToDouble();

                    diamondDungeonData.BossDungeonOverrideStats.Add(operatorOverrideStat);
                }
                catch
                {

                }




                diamondDungeonData.ClearRewardParam1 = rows[i]["ClearRewardIndex"].ToString().ToInt();
                diamondDungeonData.ClearRewardParam2 = rows[i]["ClearRewardValue"].ToString().ToDouble();



                if (m_towerDungeonDatas_Dic.ContainsKey(diamondDungeonData.DungeonNumber) == false)
                    m_towerDungeonDatas_Dic.Add(diamondDungeonData.DungeonNumber, diamondDungeonData);

                dungeonDatas.Add(diamondDungeonData);
            }

            for (int i = 0; i < dungeonDatas.Count; ++i)
            {
                DungeonModeBase diamondDungeonData = dungeonDatas[i];

                if (i != 0)
                {
                    diamondDungeonData.PrevData = dungeonDatas[i - 1];
                }

                if (i != dungeonDatas.Count - 1)
                {
                    diamondDungeonData.NextData = dungeonDatas[i + 1];
                }
            }
        }
        //------------------------------------------------------------------------------------
        public DungeonModeBase GetDiaData(int dungeonNumber)
        {
            if (m_diamondDungeonDatas_Dic.ContainsKey(dungeonNumber) == true)
                return m_diamondDungeonDatas_Dic[dungeonNumber];

            return null;
        }
        //------------------------------------------------------------------------------------
        public DungeonModeBase GetTowerData(int dungeonNumber)
        {
            if (m_towerDungeonDatas_Dic.ContainsKey(dungeonNumber) == true)
                return m_towerDungeonDatas_Dic[dungeonNumber];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}