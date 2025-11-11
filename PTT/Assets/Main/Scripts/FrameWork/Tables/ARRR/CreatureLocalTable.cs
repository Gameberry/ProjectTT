using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using CodeStage.AntiCheat.ObscuredTypes;
using Gpm.Ui;

namespace GameBerry
{
    public class CreatureData : InfiniteScrollData
    {
        public int Index;

        public int ResourceIndex; // 모델링 인덱스
        public string DefaultSkin; // 해당 리소스에 몇번째 스킨을 불러올 것인지
        public string ResourceSkin; // 해당 리소스에 몇번째 스킨을 불러올 것인지

        public int AnimNumber; // 모델링 기본 스킨 번호

        public V2Enum_ARR_MonsterGradeType MonsterGradeType;
        public V2Enum_ARR_MonsterRoleType MonsterRoleType;

        public float Scale = 1.0f;


        public int BasicAttack;
        public SkillBaseData BasicAttackData;

        public int ActiveSkill1;
        public SkillBaseData ActiveSkillData;

        public List<SkillBaseData> PassiveDatas;

        public int PassiveSkill1;

        public int PassiveSkill2;

        public int PassiveSkill3;


        public Dictionary<V2Enum_Stat, CreatureBaseStatElement> StatValue = new Dictionary<V2Enum_Stat, CreatureBaseStatElement>();

        public double GetBaseStatValue(V2Enum_Stat v2Enum_Stat)
        {
            if (StatValue.ContainsKey(v2Enum_Stat) == true)
                return StatValue[v2Enum_Stat].BaseValue;

            return 0.0;
        }

        public bool ContainsStat(V2Enum_Stat v2Enum_Stat)
        {
            return StatValue.ContainsKey(v2Enum_Stat);
        }
    }

    public class CreatureLevelUpStatData
    {
        public ObscuredInt Index;

        public V2Enum_ARR_MonsterRoleType RoleType;
        public ObscuredInt MaximumLevel;

        public Dictionary<V2Enum_Stat, CreatureBaseStatElement> StatValue = new Dictionary<V2Enum_Stat, CreatureBaseStatElement>();
    }

    public class CreatureLocalTable : LocalTableBase
    {
        private Dictionary<int, CreatureData> _creatureDatas_Dic = new Dictionary<int, CreatureData>();

        private Dictionary<V2Enum_ARR_MonsterRoleType, List<CreatureLevelUpStatData>> _creatureRoleLevelUpDatas_Dic = new Dictionary<V2Enum_ARR_MonsterRoleType, List<CreatureLevelUpStatData>>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Monster", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            List<CreatureData> creatureDatas = JsonConvert.DeserializeObject<List<CreatureData>>(rows.ToJson());


            List<V2Enum_Stat> searchStat = new List<V2Enum_Stat>();
            searchStat.Add(V2Enum_Stat.Attack);

            searchStat.Add(V2Enum_Stat.HP);

            searchStat.Add(V2Enum_Stat.Defence);

            searchStat.Add(V2Enum_Stat.MoveSpeed);
            searchStat.Add(V2Enum_Stat.CritChance);

            searchStat.Add(V2Enum_Stat.ResistanceStat);
            searchStat.Add(V2Enum_Stat.ResistancePenetration);
            searchStat.Add(V2Enum_Stat.Evasion);

            searchStat.Add(V2Enum_Stat.Accuracy);

            for (int i = 0; i < creatureDatas.Count; ++i)
            {
                if (_creatureDatas_Dic.ContainsKey(creatureDatas[i].Index) == false)
                    _creatureDatas_Dic.Add(creatureDatas[i].Index, creatureDatas[i]);

                for (int statid = 0; statid < searchStat.Count; ++statid)
                {
                    V2Enum_Stat v2Enum_Stat = searchStat[statid];

                    try
                    {
                        double statvalue = rows[i][v2Enum_Stat.ToString()].ToString().ToDouble();
                        CreatureBaseStatElement allyBaseStatElement = new CreatureBaseStatElement();
                        allyBaseStatElement.BaseStat = v2Enum_Stat;
                        allyBaseStatElement.BaseValue = statvalue;

                        creatureDatas[i].StatValue.Add(allyBaseStatElement.BaseStat, allyBaseStatElement);
                    }
                    catch
                    {

                    }
                }
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("MonsterLevelUpStat", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            searchStat.Clear();
            searchStat.Add(V2Enum_Stat.Attack);

            searchStat.Add(V2Enum_Stat.HP);

            searchStat.Add(V2Enum_Stat.Defence);

            for (int i = 0; i < rows.Count; ++i)
            {
                CreatureLevelUpStatData creatureLevelUpStatData = new CreatureLevelUpStatData();

                creatureLevelUpStatData.Index = rows[i]["Index"].ToString().ToInt();
                creatureLevelUpStatData.RoleType = rows[i]["RoleType"].ToString().ToInt().IntToEnum32<V2Enum_ARR_MonsterRoleType>();

                creatureLevelUpStatData.MaximumLevel = rows[i]["MaximumLevel"].ToString().ToInt();


                for (int statid = 0; statid < searchStat.Count; ++statid)
                {
                    V2Enum_Stat v2Enum_Stat = searchStat[statid];

                    try
                    {
                        double statvalue = rows[i][v2Enum_Stat.ToString()].ToString().ToDouble();
                        CreatureBaseStatElement allyBaseStatElement = new CreatureBaseStatElement();
                        allyBaseStatElement.BaseStat = v2Enum_Stat;
                        allyBaseStatElement.BaseValue = statvalue;

                        creatureLevelUpStatData.StatValue.Add(allyBaseStatElement.BaseStat, allyBaseStatElement);
                    }
                    catch
                    {

                    }
                }

                if (_creatureRoleLevelUpDatas_Dic.ContainsKey(creatureLevelUpStatData.RoleType) == false)
                    _creatureRoleLevelUpDatas_Dic.Add(creatureLevelUpStatData.RoleType, new List<CreatureLevelUpStatData>());

                _creatureRoleLevelUpDatas_Dic[creatureLevelUpStatData.RoleType].Add(creatureLevelUpStatData);
            }

            foreach (var pair in _creatureRoleLevelUpDatas_Dic)
            {
                pair.Value.Sort((x, y) =>
                {
                    if (x.MaximumLevel.GetDecrypted() < y.MaximumLevel.GetDecrypted())
                        return -1;
                    else if (x.MaximumLevel.GetDecrypted() > y.MaximumLevel.GetDecrypted())
                        return 1;

                    return 0;
                });
            }
        }
        //------------------------------------------------------------------------------------
        public Dictionary<int, CreatureData> GetAllCreatureData()
        {
            return _creatureDatas_Dic;
        }
        //------------------------------------------------------------------------------------
        public CreatureData GetCreatureData(int index)
        {
            if (_creatureDatas_Dic.ContainsKey(index) == true)
                return _creatureDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<CreatureLevelUpStatData> GetCreatureLevelUpStatDatas(V2Enum_ARR_MonsterRoleType enum_ARR_RoleType)
        {
            if (_creatureRoleLevelUpDatas_Dic.ContainsKey(enum_ARR_RoleType) == true)
                return _creatureRoleLevelUpDatas_Dic[enum_ARR_RoleType];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}