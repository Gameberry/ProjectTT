using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using LitJson;

namespace GameBerry
{
    public class RelicData
    {
        public ObscuredInt Index;
        public ObscuredInt ResourceIndex;
        public ObscuredInt MainSkill;

        public MainSkillData SynergySkillData;

        public string NameLocalKey;
        public string DescLocalKey;
    }

    public class RelicLevelUpCostData
    {
        public ObscuredInt Index;
        public ObscuredInt RelicLevel;
        public ObscuredInt LevelUpCostGoodsParam1;
    }

    public class RelicLocalTable : LocalTableBase
    {
        private Dictionary<ObscuredInt, RelicData> _relicData_Dic = new Dictionary<ObscuredInt, RelicData>();
        private Dictionary<ObscuredInt, RelicLevelUpCostData> _relicLevelUpCostData_Dic = new Dictionary<ObscuredInt, RelicLevelUpCostData>();

        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Relic", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                RelicData relicData = new RelicData();

                relicData.Index = rows[i]["Index"].ToString().ToInt();

                relicData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                relicData.MainSkill = rows[i]["MainSkill"].ToString().ToInt();

                relicData.NameLocalKey = string.Format("relicname/{0}", relicData.Index);
                relicData.DescLocalKey = string.Format("relicdesc/{0}", relicData.Index);

                if (_relicData_Dic.ContainsKey(relicData.Index) == false)
                    _relicData_Dic.Add(relicData.Index, relicData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RelicLevelUpCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                RelicLevelUpCostData relicLevelUpCostData = new RelicLevelUpCostData();

                relicLevelUpCostData.Index = rows[i]["Index"].ToString().ToInt();

                relicLevelUpCostData.RelicLevel = rows[i]["RelicLevel"].ToString().ToInt();

                relicLevelUpCostData.LevelUpCostGoodsParam1 = rows[i]["LevelUpCostGoodsParam1"].ToString().ToInt();

                if (_relicLevelUpCostData_Dic.ContainsKey(relicLevelUpCostData.RelicLevel) == false)
                    _relicLevelUpCostData_Dic.Add(relicLevelUpCostData.RelicLevel, relicLevelUpCostData);
            }
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, RelicData> GetAllRelicData()
        {
            return _relicData_Dic;
        }
        //------------------------------------------------------------------------------------
        public RelicData GetRelicData(ObscuredInt index)
        {
            if (_relicData_Dic.ContainsKey(index) == true)
                return _relicData_Dic[index];
            return null;
        }
        //------------------------------------------------------------------------------------
        public RelicLevelUpCostData GetRelicLevelUpCostData(ObscuredInt level)
        {
            if (_relicLevelUpCostData_Dic.ContainsKey(level) == true)
                return _relicLevelUpCostData_Dic[level];
            return null;
        }
        //------------------------------------------------------------------------------------
    }
}