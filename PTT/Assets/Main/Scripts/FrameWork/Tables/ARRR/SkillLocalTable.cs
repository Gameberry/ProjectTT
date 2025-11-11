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
    public class SkillBaseData
    {
        public int Index;
        public string AnimNumber;
        public string AnimName = string.Empty;
        public int SkillBase;
        public int ResourceIndex;
        public string SoundId;
        public int SkillIconIndex;

        public float TargetRange;
        public float AttackRange;

        public V2Enum_ARR_TriggerType TriggerType;

        public V2Enum_ARR_CoolTimeType CoolTimeType;
        public float CoolTimeValue;

        public bool IsUseCoolTime = false;

        public Enum_ARR_TargetCheckType TargetCheckType;
        public V2Enum_ARR_TargetAttackType TargetAttackType;
        public Enum_ARR_TargetConditionType TargetCondition;
        public int TargetCount;


        public List<SkillEffectData> SkillEffect;

        public SkillDamageData SkillDamageIndex;

        public int SkillRepeatCount;
        public float SkillRepeatDelay;

        public string NameLocalKey;
        public string DescLocalKey;
    }


    /// <summary>
    /// 대미지
    /// </summary>
    public class SkillDamageData
    {
        public int Index;
        public V2Enum_ARR_DamageType DamageType;
        public float DamageParam;

        public float Duration;

        public int HitCount;

        public double DamageFactorBase;
        public double DamageFactorPerLevel;
    }

    /// <summary>
    /// 군중제어
    /// </summary>
    public class SkillEffectData
    {
        public int Index;
        public Enum_ARR_TargetCheckType TargetCheckType;
        public Enum_ARR_TargetStateType TargetState;

        public V2Enum_SkillEffectType SkillEffectType;

        public double SkillEffectValue;
        public double SkillEffectIncreasePerLevel;

        public double SkillEffectProb; // 스킬 효과가 적용될 확률
        public double SkillEffectProbIncreasePerLevel;

        public float Duration;
        public float DurationIncreasePerLevel;

        public string DescLocalKey;
    }

    public class SkillModuleData
    {
        public int Index;
        public int ResourceChange;

        public float Range;
        public double DamageIncrease;

        public float DecreaseCoolTime;

        public float ReduceCoolTime;

        public int IncreaseHitCount;

        public int IncreaseRepeatCount;
        public float IncreaseRepeatDelay;

        public double IncreaseCritChance;

        public float IncreaseDuration;
    }


    public class MainSkillData
    {
        public ObscuredInt Index;

        public V2Enum_ARR_MainSkillType MainSkillType;
        public ObscuredInt MainSkillTypeParam1;
        public ObscuredInt MainSkillTypeParam2;
        public ObscuredInt MainSkillTypeParam3;

        public int MainSkillIcon;

        public V2Enum_Grade MainSkillGrade;

        public string NameLocalKey;
        public string DescLocalKey;
    }

    public class SkillLocalTable : LocalTableBase
    {
        private Dictionary<int, SkillBaseData> _characterSkillDatas = new Dictionary<int, SkillBaseData>();

        private Dictionary<int, SkillDamageData> _characterSkillDamageDatas_Dic = new Dictionary<int, SkillDamageData>();
        private Dictionary<int, SkillEffectData> _characterSkillEffectDatas_Dic = new Dictionary<int, SkillEffectData>();
        private Dictionary<int, SkillModuleData> _characterSkillModuleDatas_Dic = new Dictionary<int, SkillModuleData>();

        private Dictionary<ObscuredInt, MainSkillData> _mainSkillData_Dic = new Dictionary<ObscuredInt, MainSkillData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            await SetDamage("SkillDamage");
            await SetEffect("SkillEffect");
            await SetModule("SkillModule");

            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Skill", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SkillBaseData skillBaseData = new SkillBaseData();

                skillBaseData.Index = rows[i]["Index"].ToString().ToInt();

                skillBaseData.AnimNumber = rows[i]["AnimNumber"].ToString();

                if(skillBaseData.AnimNumber != "-1")
                {
                    skillBaseData.AnimName = skillBaseData.AnimNumber;
                }
                

                skillBaseData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                skillBaseData.SoundId = skillBaseData.ResourceIndex.ToString();

                skillBaseData.SkillIconIndex = rows[i]["SkillIconIndex"].ToString().ToInt();

                skillBaseData.TargetRange = rows[i]["TargetRange"].ToString().ToFloat();
                
                skillBaseData.AttackRange = rows[i]["AttackRange"].ToString().ToFloat();

                skillBaseData.TriggerType = rows[i]["TriggerType"].ToString().ToInt().IntToEnum32<V2Enum_ARR_TriggerType>();

                skillBaseData.CoolTimeType = rows[i]["CoolTimeType"].ToString().ToInt().IntToEnum32<V2Enum_ARR_CoolTimeType>();

                string cooltimevalue = rows[i]["CoolTimeValue"].ToString();

                if (cooltimevalue == "-1")
                {
                    skillBaseData.IsUseCoolTime = false;
                }
                else
                {
                    skillBaseData.IsUseCoolTime = true;
                    skillBaseData.CoolTimeValue = cooltimevalue.ToFloat();
                }
                
                skillBaseData.TargetCheckType = rows[i]["TargetCheckType"].ToString().ToInt().IntToEnum32<Enum_ARR_TargetCheckType>();
                skillBaseData.TargetAttackType = rows[i]["TargetAttackType"].ToString().ToInt().IntToEnum32<V2Enum_ARR_TargetAttackType>();
                skillBaseData.TargetCondition = rows[i]["TargetCondition"].ToString().ToInt().IntToEnum32<Enum_ARR_TargetConditionType>();
                skillBaseData.TargetCount = rows[i]["TargetCount"].ToString().ToInt();

                for (int j = 1; j <= 5; ++j)
                {
                    try
                    {
                        string SkillEffect = string.Format("SkillEffect{0}", j);
                        int skillEffect = rows[i][SkillEffect].ToString().ToInt();
                        if (skillEffect == -1 || skillEffect == 0)
                            continue;

                        if (skillBaseData.SkillEffect == null)
                            skillBaseData.SkillEffect = new List<SkillEffectData>();

                        if (_characterSkillEffectDatas_Dic.ContainsKey(skillEffect) == true)
                            skillBaseData.SkillEffect.Add(_characterSkillEffectDatas_Dic[skillEffect]);
                    }
                    catch
                    {

                    }
                }

                int skillDamage = rows[i]["SkillDamageIndex"].ToString().ToInt();
                if (skillDamage != -1)
                {
                    if (_characterSkillDamageDatas_Dic.ContainsKey(skillDamage) == true)
                        skillBaseData.SkillDamageIndex = _characterSkillDamageDatas_Dic[skillDamage];
                }

                skillBaseData.SkillRepeatCount = rows[i]["SkillRepeatCount"].ToString().ToInt();
                skillBaseData.SkillRepeatDelay = rows[i]["SkillRepeatDelay"].ToString().ToFloat();

                skillBaseData.NameLocalKey = string.Format("skillname/{0}", skillBaseData.Index);
                skillBaseData.DescLocalKey = string.Format("skilldesc/{0}", skillBaseData.Index);

                _characterSkillDatas.Add(skillBaseData.Index, skillBaseData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("MainSkill", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                MainSkillData mainSkillData = new MainSkillData();

                mainSkillData.Index = rows[i]["Index"].ToString().ToInt();

                mainSkillData.MainSkillType = rows[i]["MainSkillType"].ToString().ToInt().IntToEnum32<V2Enum_ARR_MainSkillType>();

                mainSkillData.MainSkillTypeParam1 = rows[i]["MainSkillTypeParam1"].ToString().ToInt();
                mainSkillData.MainSkillTypeParam2 = rows[i]["MainSkillTypeParam2"].ToString().ToInt();
                mainSkillData.MainSkillTypeParam3 = rows[i]["MainSkillTypeParam3"].ToString().ToInt();

                mainSkillData.MainSkillIcon = rows[i]["MainSkillIcon"].ToString().ToInt();

                mainSkillData.MainSkillGrade = rows[i]["MainSkillGrade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                mainSkillData.NameLocalKey = string.Format("skillname/{0}", mainSkillData.Index);
                mainSkillData.DescLocalKey = string.Format("skilldesc/{0}", mainSkillData.Index);

                _mainSkillData_Dic.Add(mainSkillData.Index, mainSkillData);
            }


        }
        //------------------------------------------------------------------------------------
        private async UniTask SetDamage(string jsonStr)
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart(jsonStr, o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            List<SkillDamageData> m_characterSkillDamageDatas = JsonConvert.DeserializeObject<List<SkillDamageData>>(rows.ToJson());

            for (int i = 0; i < m_characterSkillDamageDatas.Count; ++i)
            {
                _characterSkillDamageDatas_Dic.TryAdd(m_characterSkillDamageDatas[i].Index, m_characterSkillDamageDatas[i]);
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask SetEffect(string jsonStr)
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart(jsonStr, o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            List<SkillEffectData> skillEffectDatas = JsonConvert.DeserializeObject<List<SkillEffectData>>(rows.ToJson());

            for (int i = 0; i < skillEffectDatas.Count; ++i)
            {
                SkillEffectData skillEffectData = skillEffectDatas[i];
                skillEffectData.SkillEffectValue *= Define.PerSkillEffectRecoverValue;
                skillEffectData.SkillEffectIncreasePerLevel *= Define.PerSkillEffectRecoverValue;

                skillEffectData.DescLocalKey = string.Format("skilleffectdesc/{0}", skillEffectData.SkillEffectType.Enum32ToInt());

                _characterSkillEffectDatas_Dic.TryAdd(skillEffectDatas[i].Index, skillEffectDatas[i]);
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask SetModule(string jsonStr)
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart(jsonStr, o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            List<SkillModuleData> skillModuleDatas = JsonConvert.DeserializeObject<List<SkillModuleData>>(rows.ToJson());

            float floatPerSkillEffectRecoverValue = (float)Define.PerSkillEffectRecoverValue;

            for (int i = 0; i < skillModuleDatas.Count; ++i)
            {
                skillModuleDatas[i].DamageIncrease *= Define.PerSkillEffectRecoverValue;
                skillModuleDatas[i].DecreaseCoolTime *= floatPerSkillEffectRecoverValue;
                _characterSkillModuleDatas_Dic.TryAdd(skillModuleDatas[i].Index, skillModuleDatas[i]);
            }
        }
        //------------------------------------------------------------------------------------
        public SkillBaseData GetSkillBaseData(int index)
        {
            if (_characterSkillDatas.ContainsKey(index) == true)
                return _characterSkillDatas[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SkillDamageData GetSkillDamageData(int index)
        {
            if (_characterSkillDamageDatas_Dic.ContainsKey(index) == true)
                return _characterSkillDamageDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SkillEffectData GetSkillEffectData(int index)
        {
            if (_characterSkillEffectDatas_Dic.ContainsKey(index) == true)
                return _characterSkillEffectDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SkillModuleData GetSkillModuleData(int index)
        {
            if (_characterSkillModuleDatas_Dic.ContainsKey(index) == true)
                return _characterSkillModuleDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public MainSkillData GetGambleSkillData(ObscuredInt index)
        {
            if (_mainSkillData_Dic.ContainsKey(index) == true)
                return _mainSkillData_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}