using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SKillEffectInfo
    {
        public Enum_TargetCheckType TargetCheckType;
        public Enum_TargetStateType TargetState;

        public V2Enum_SkillEffectType SkillEffectType;

        public double SkillEffectValue;
        public double SkillEffectIncreasePerLevel;

        public double SkillEffectProb; // 스킬 효과가 적용될 확률
        public double SkillEffectProbIncreasePerLevel;

        public float Duration;
        public float DurationIncreasePerLevel;

        public void SetSkillEffectData(SkillEffectData skillEffectData)
        {
            TargetCheckType = skillEffectData.TargetCheckType;
            TargetState = skillEffectData.TargetState;

            SkillEffectType = skillEffectData.SkillEffectType;

            SkillEffectValue = skillEffectData.SkillEffectValue;
            SkillEffectIncreasePerLevel = skillEffectData.SkillEffectIncreasePerLevel;

            SkillEffectProb = skillEffectData.SkillEffectProb;
            SkillEffectProbIncreasePerLevel = skillEffectData.SkillEffectProbIncreasePerLevel;

            Duration = skillEffectData.Duration;
            DurationIncreasePerLevel = skillEffectData.DurationIncreasePerLevel;
        }

        public void AddSkillEffectData(SkillEffectData skillEffectData)
        {
            SkillEffectValue += skillEffectData.SkillEffectValue;
            SkillEffectIncreasePerLevel += skillEffectData.SkillEffectIncreasePerLevel;

            SkillEffectProb += skillEffectData.SkillEffectProb;
            SkillEffectProbIncreasePerLevel += skillEffectData.SkillEffectProbIncreasePerLevel;

            Duration += skillEffectData.Duration;
            DurationIncreasePerLevel += skillEffectData.DurationIncreasePerLevel;
        }
    }

    public static class SKillEffectInfoPool
    {
        private static Common.ObjectPoolClass<SKillEffectInfo> _SKillEffectInfo = new Common.ObjectPoolClass<SKillEffectInfo>();

        public static SKillEffectInfo GetSKillEffectInfo()
        {
            SKillEffectInfo creatureController = _SKillEffectInfo.GetObject() ?? new SKillEffectInfo();

            return creatureController;
        }
        //------------------------------------------------------------------------------------
        public static void PoolSKillEffectInfo(SKillEffectInfo creatureController)
        {
            _SKillEffectInfo.PoolObject(creatureController);
        }
        //------------------------------------------------------------------------------------
    }

    public class SkillManageInfo
    {
        public SkillInfo MySkillInfo;
        public int SkillLevel = 0;
        public SkillBaseData SkillBaseData { get { return _skillBaseData; } }
        private SkillBaseData _skillBaseData;

        public DescendData myDescend = null;

        public List<SKillEffectInfo> MotherTargetEffectDatas = new List<SKillEffectInfo>();
        public List<SKillEffectInfo> OtherSkillEffectDatas = new List<SKillEffectInfo>();

        public List<SkillModuleData> AddSkillModuleDatas = new List<SkillModuleData>();

        public List<SkillManageInfo> AfterSkill = new List<SkillManageInfo>();

        public bool IsMainSkill = false;
        public V2Enum_Grade SkillGrade;

        public int OnTimePlayCount = 0;

        public Enum_SynergyType SynergyType = Enum_SynergyType.Max;

        public bool SetSkillBaseData(SkillBaseData skillBaseData, SkillInfo skillInfo, V2Enum_Grade skillGrade)
        {
            MySkillInfo = skillInfo;

            if (MySkillInfo == null)
                SkillLevel = Define.PlayerSkillDefaultLevel;
            else
            { 
                SkillLevel = MySkillInfo.Level;
                myDescend = MySkillInfo.descend;
            }

            _skillBaseData = skillBaseData;

            if (skillGrade != V2Enum_Grade.Max)
            {
                SkillGrade = skillGrade;
            }
            else
            {
                if (skillInfo != null)
                {
                    SkillGrade = Managers.ARRRSkillManager.Instance.GetARRRSkillGrade(skillBaseData);
                }
                else
                {
                    SkillGrade = V2Enum_Grade.Max;
                }
            }
            

            bool needCheckSkillEffect = false;


            if (_skillBaseData != null)
            {
                if (_skillBaseData.SkillEffect != null)
                {
                    for (int i = 0; i < _skillBaseData.SkillEffect.Count; ++i)
                    {
                        if (_skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.Revive
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.KillingMoney
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.DoubleInterest
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.GoldIncreaseAtt
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.GoldIncreaseArmor
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.WasteGoldBuff
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseGasGambleProb
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseGasHpHeal
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseJokerProb
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.DecreaseGamblePrice
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseInterestRate

                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseSynergyCount
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseHealEffect

                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseAttFireCount
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseAttGoldCount
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseAttWaterCount
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseAttThunderCount

                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseDefFireCount
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseDefGoldCount
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseDefWaterCount
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseDefThunderCount

                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseHpFireCount
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseHpGoldCount
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseHpWaterCount
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseHpThunderCount


                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.VampiricDmgFire 
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.VampiricDmgGold 
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.VampiricDmgWater
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.VampiricDmgThunder

                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.DescendDmgFireCount 
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.DescendDmgGoldCount 
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.DescendDmgWaterCount
                            || _skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.DescendDmgThunderCount

                            )
                        {
                            needCheckSkillEffect = true;
                            continue;
                        }

                        AddEffectData(_skillBaseData.SkillEffect[i]);
                    }
                }
            }

            return needCheckSkillEffect;
        }

        public void SetSynergyType(Enum_SynergyType Enum_SynergyType)
        {
            SynergyType = Enum_SynergyType;
        }

        public void SetMainSkill(bool isMainSkill)
        {
            IsMainSkill = isMainSkill;
        }

        public bool IsMain()
        {
            return IsMainSkill;
        }

        public void SetSkillGrade(V2Enum_Grade v2Enum_Grade)
        {
            SkillGrade = v2Enum_Grade;
        }

        public V2Enum_Grade GetSkillGrade()
        {
            return SkillGrade;
        }

        public void AddSkillLevel()
        {
            SkillLevel++;
        }

        public int GetSkillLevel()
        { 
            return SkillLevel;
        }

        public float GetAttackRange()
        {
            return SkillBaseData.AttackRange + AddAttackRange;
        }

        public float GetTargetRange()
        {
            return SkillBaseData.TargetRange;
        }

        public double GetDamageFactor()
        {
            if (SkillBaseData.SkillDamageIndex == null)
                return 0;

            SkillDamageData skillDamageData = SkillBaseData.SkillDamageIndex;

            int level = GetSkillLevel();

            double factor = skillDamageData.DamageFactorBase + (skillDamageData.DamageFactorPerLevel * level);
            factor += factor * AddDamageIncrease;
            return factor * Define.PerSkillEffectRecoverValue;
        }

        public int GetTargetCount()
        {
            return _skillBaseData.TargetCount;
        }

        public int GetHitCount()
        {
            if (SkillBaseData.SkillDamageIndex == null)
                return 0;

            return SkillBaseData.SkillDamageIndex.HitCount + AddHitCount;
        }

        public int GetRepeatCount()
        {
            return SkillBaseData.SkillRepeatCount + AddIncreaseRepeatCount;
        }

        public float GetRepeatDelay()
        {
            return SkillBaseData.SkillRepeatDelay + AddIncreaseRepeatDelay;
        }

        public double GetAddIncreaseCritChance()
        {
            return AddIncreaseCritChance;
        }

        public float GetDuration()
        {
            if (SkillBaseData.SkillDamageIndex == null)
                return 0;

            return SkillBaseData.SkillDamageIndex.Duration + AddIncreaseDuration;
        }

        public string GetAniNumber()
        {
            return _skillBaseData.AnimNumber;
        }

        public string GetAniName()
        { 
            return _skillBaseData.AnimName;
        }

        public void AddAfterSkill(SkillManageInfo skillManageInfo)
        {
            AfterSkill.Add(skillManageInfo);
        }

        public void AddEffectData(SkillEffectData skillEffectData)
        {
            List<SKillEffectInfo> selectinfo = null;

            if (skillEffectData.TargetCheckType == Enum_TargetCheckType.Mother
                || skillEffectData.TargetCheckType == SkillBaseData.TargetCheckType)
                selectinfo = MotherTargetEffectDatas;
            else
                selectinfo = OtherSkillEffectDatas;

            SKillEffectInfo sKillEffectInfo = selectinfo.Find(
                    x =>
                    x.TargetCheckType == skillEffectData.TargetCheckType
                && x.TargetState == skillEffectData.TargetState
                && x.SkillEffectType == skillEffectData.SkillEffectType);

            if (sKillEffectInfo == null)
            {
                sKillEffectInfo = SKillEffectInfoPool.GetSKillEffectInfo();
                sKillEffectInfo.SetSkillEffectData(skillEffectData);
                selectinfo.Add(sKillEffectInfo);
            }
            else
                sKillEffectInfo.AddSkillEffectData(skillEffectData);
        }

        public void AddModuleData(SkillModuleData skillModuleData)
        {
            if (skillModuleData == null)
                return;

            AddAttackRange += skillModuleData.Range;
            AddDamageIncrease += skillModuleData.DamageIncrease;

            AddDecreaseCoolTime += skillModuleData.DecreaseCoolTime;
            AddReduceCoolTime += skillModuleData.ReduceCoolTime;

            AddHitCount += skillModuleData.IncreaseHitCount;

            AddIncreaseRepeatCount += skillModuleData.IncreaseRepeatCount;
            AddIncreaseRepeatDelay += skillModuleData.IncreaseRepeatDelay;

            AddIncreaseCritChance += skillModuleData.IncreaseCritChance;
            AddIncreaseDuration += skillModuleData.IncreaseDuration;
            

            if (AddDecreaseCoolTime > Define.SkillMaxCoolDecrease)
                AddDecreaseCoolTime = Define.SkillMaxCoolDecrease;

            SetOutputOriginRemainTime();

            AddSkillModuleDatas.Add(skillModuleData);
        }

        public float GetRemainCoolTime()
        {
            if (IsCountCoolTime(_skillBaseData.CoolTimeType))
            {
                return RemainCount;
            }
            else if (_skillBaseData.CoolTimeType == Enum_CoolTimeType.HPPercentOver
                || _skillBaseData.CoolTimeType == Enum_CoolTimeType.HPPercentBelow)
            {
                return IsReady == true ? 0.0f : 1.0f;
            }
            else
            {
                return RemainTime;
            }
        }

        public float GetRemainCoolRatio()
        {
            if (_skillBaseData.CoolTimeType == Enum_CoolTimeType.HPPercentOver
                || _skillBaseData.CoolTimeType == Enum_CoolTimeType.HPPercentBelow)
            {
                return 1.0f;
            }


            if (SkillManageInfo.IsCountCoolTime(_skillBaseData.CoolTimeType))
            {
                return (float)RemainCount / OriginRemainTime;
            }
            else
            {
                return RemainTime / OriginRemainTime;
            }
        }

        private float AddAttackRange = 0;
        private double AddDamageIncrease = 0;
        private float AddDecreaseCoolTime = 0;
        private float AddReduceCoolTime = 0;
        private int AddHitCount = 0;
        private int AddIncreaseRepeatCount = 0;
        private float AddIncreaseRepeatDelay = 0;

        private double AddIncreaseCritChance = 0;
        private float AddIncreaseDuration = 0;

        public void ResetEffectModuleAndEffect()
        {
            for (int i = 0; i < MotherTargetEffectDatas.Count; ++i)
            {
                SKillEffectInfoPool.PoolSKillEffectInfo(MotherTargetEffectDatas[i]);
            }

            MotherTargetEffectDatas.Clear();

            for (int i = 0; i < OtherSkillEffectDatas.Count; ++i)
            {
                SKillEffectInfoPool.PoolSKillEffectInfo(OtherSkillEffectDatas[i]);
            }

            myDescend = null;

            OtherSkillEffectDatas.Clear();

            AddSkillModuleDatas.Clear();
            AfterSkill.Clear();

            SkillLevel = 0;

            AddAttackRange = 0;
            AddDamageIncrease = 0;
            AddDecreaseCoolTime = 0;
            AddReduceCoolTime = 0;
            AddHitCount = 0;
            
            AddIncreaseRepeatCount = 0;
            AddIncreaseRepeatDelay = 0;

            AddIncreaseCritChance = 0;
            AddIncreaseDuration = 0;

            IsReady = false;
            IsApplySkill = false;

            IsMainSkill = false;
            SkillGrade = V2Enum_Grade.Max;
            OnTimePlayCount = 0;

            SynergyType = Enum_SynergyType.Max;
        }

        public void SetOriginRemainTime(float cooltime)
        {
            SettingRemainTime = cooltime;
            SetOutputOriginRemainTime();
        }

        private void SetOutputOriginRemainTime()
        {
            OriginRemainTime = SettingRemainTime;
            OriginRemainTime -= AddReduceCoolTime;

            OriginRemainTime -= OriginRemainTime * AddDecreaseCoolTime;

            if(IsCountCoolTime(_skillBaseData.CoolTimeType) == true)
            {
                if (OriginRemainTime < Define.SkillMinCoolCount)
                    OriginRemainTime = Define.SkillMinCoolCount;
            }
            else
            {
                if (OriginRemainTime < Define.SkillMinCoolTime)
                    OriginRemainTime = Define.SkillMinCoolTime;
            }
        }

        public float GetOutputOriginRemainTime()
        {
            return OriginRemainTime;
        }

        public float RemainTime;
        public int RemainCount;

        private float SettingRemainTime;
        private float OriginRemainTime;

        public bool IsReady = false;

        public bool IsApplySkill = false;

        public static bool IsCountCoolTime(Enum_CoolTimeType Enum_CoolTimeType)
        {
            if (Enum_CoolTimeType == Enum_CoolTimeType.AttackCount
                    || Enum_CoolTimeType == Enum_CoolTimeType.KillingCount
                    || Enum_CoolTimeType == Enum_CoolTimeType.HitCount
                    || Enum_CoolTimeType == Enum_CoolTimeType.CriticalAttack
                    || Enum_CoolTimeType == Enum_CoolTimeType.IsBoss
                    || Enum_CoolTimeType == Enum_CoolTimeType.IsNormal
                    || Enum_CoolTimeType == Enum_CoolTimeType.Interest
                    || Enum_CoolTimeType == Enum_CoolTimeType.WaveClear
                    || Enum_CoolTimeType == Enum_CoolTimeType.WaveStart

                    || Enum_CoolTimeType == Enum_CoolTimeType.GambleCardReward
                    || Enum_CoolTimeType == Enum_CoolTimeType.FailGasGamble
                    || Enum_CoolTimeType == Enum_CoolTimeType.GainGasReward
                    || Enum_CoolTimeType == Enum_CoolTimeType.MissionComplete

                    || Enum_CoolTimeType == Enum_CoolTimeType.GambleCardRewardFire
                    || Enum_CoolTimeType == Enum_CoolTimeType.GambleCardRewardGold
                    || Enum_CoolTimeType == Enum_CoolTimeType.GambleCardRewardWater
                    || Enum_CoolTimeType == Enum_CoolTimeType.GambleCardRewardThunder
                    || Enum_CoolTimeType == Enum_CoolTimeType.Revive
                    )
            {
                return true;
            }

            return false;
        }
    }

    public static class SkillManageInfoPool
    {
        private static Common.ObjectPoolClass<SkillManageInfo> _skillManageInfo = new Common.ObjectPoolClass<SkillManageInfo>();


        public static SkillManageInfo GetSkillManageInfo()
        {
            SkillManageInfo creatureController = _skillManageInfo.GetObject() ?? new SkillManageInfo();
            
            return creatureController;
        }
        //------------------------------------------------------------------------------------
        public static void PoolSkillManageInfo(SkillManageInfo creatureController)
        {
            creatureController.ResetEffectModuleAndEffect();
            _skillManageInfo.PoolObject(creatureController);
        }
        //------------------------------------------------------------------------------------
    }

    public class SkillActiveController
    {
        protected CharacterControllerBase _characterControllerBase;

        // 평타
        protected SkillManageInfo _defaultSkill;
        public SkillManageInfo DefaultSkill { get { return _defaultSkill; } }

        protected SkillManageInfo _readyActiveSkill;


        // 쿨타임 체크 스킬들
        protected Dictionary<SkillBaseData, SkillManageInfo> _skillLibrary = new Dictionary<SkillBaseData, SkillManageInfo>();
        protected LinkedList<SkillManageInfo> _checkCoolTime = new LinkedList<SkillManageInfo>();
        protected LinkedList<SkillManageInfo> _checkHPPercentCoolTime = new LinkedList<SkillManageInfo>();
        protected List<SkillManageInfo> _originSkillList = new List<SkillManageInfo>();

        public Dictionary<SkillBaseData, SkillManageInfo> SkillLibrary { get { return _skillLibrary; } }

        public List<SkillManageInfo> OriginSkillList { get { return _originSkillList; } }

        private LinkedList<SkillManageInfo> _applyNoneCoolTimePassiveBuff = new LinkedList<SkillManageInfo>();
        private LinkedList<MainSkillData> _addGambleSkill = new LinkedList<MainSkillData>();

        public LinkedList<MainSkillData> MyAddGambleSkill { get { return _addGambleSkill; } }

        protected float _skillTimerIncreaseRate = 1.0f;
        protected float _skillCoolTimeDecreaseRate = 0.0f;

        private int NeedOperAttackCount = 0;
        private int NeedOperKillCount = 0;
        private int NeedOperHitCount = 0;
        private int NeedOperCriticalCount = 0;
        private int NeedOperBossHitCount = 0;
        private int NeedOperNormalHitCount = 0;
        private int NeedOperInterestCount = 0;
        private int NeedOperWaveClearCount = 0;
        private int NeedOperWaveStartCount = 0;

        private int NeedOperGambleCardRewardCount = 0;
        private int NeedOperFailGasGambleCount = 0;
        private int NeedOperGainGasRewardCount = 0;
        private int NeedOperMissionCompleteCount = 0;


        private int NeedOperGambleCardRewardFire = 0;
        private int NeedOperGambleCardRewardGold = 0;
        private int NeedOperGambleCardRewardWater = 0;
        private int NeedOperGambleCardRewardThunder = 0;
        private int NeedOperRevive = 0;

        private Enum_SynergyType _lastChoice_SynergyType = Enum_SynergyType.Max;
        public Enum_SynergyType LastChoice_SynergyType { get { return _lastChoice_SynergyType; } }

        public System.Action<MainSkillData> AddGambleSkillAction;
        public System.Action RefreshCoolTimeAction;

        

        //------------------------------------------------------------------------------------
        public void SetCharacterController(CharacterControllerBase characterControllerBase)
        {
            _characterControllerBase = characterControllerBase;
        }
        //------------------------------------------------------------------------------------
        public void ReStartSkill()
        {
            _readyActiveSkill = _defaultSkill;
        }
        //------------------------------------------------------------------------------------
        public void SetDefaultSkill(SkillBaseData skillBaseData)
        {
            _defaultSkill = AddSkillData(skillBaseData);

            _readyActiveSkill = null;
        }
        //------------------------------------------------------------------------------------
        public SkillManageInfo GetReadyActiveSkill()
        {
            return _readyActiveSkill;
        }
        //------------------------------------------------------------------------------------
        public void AddGambleSkill(MainSkillData gambleSkillData)
        {
            _addGambleSkill.AddLast(gambleSkillData);
            AddGambleSkillAction?.Invoke(gambleSkillData);
        }
        //------------------------------------------------------------------------------------
        public void AddGambleSkillBaseData(MainSkillData gambleSkillData, Enum_SynergyType Enum_SynergyType, SkillInfo skillInfo)
        {
            SkillBaseData skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(gambleSkillData.MainSkillTypeParam1);

            if (_skillLibrary.ContainsKey(skillBaseData) == true)
            {
                SkillManageInfo skillManageInfo = _skillLibrary[skillBaseData];
                skillManageInfo.AddSkillLevel();
                return;
            }

            SkillManageInfo addSkillManageInfo = AddSkillData(skillBaseData, false, skillInfo, gambleSkillData.MainSkillGrade);
            addSkillManageInfo.SetSynergyType(Enum_SynergyType);
            if (skillBaseData.CoolTimeType == Enum_CoolTimeType.Default
                || skillBaseData.CoolTimeType == Enum_CoolTimeType.GamebleCoolTime)
                addSkillManageInfo.RemainTime = 0.1f;
        }
        //------------------------------------------------------------------------------------
        public void AddAfterData(SkillBaseData skillBaseData, SkillBaseData afterData, SkillInfo afterInfo = null)
        {
            if (_skillLibrary.ContainsKey(skillBaseData) == false)
                return;

            SkillManageInfo skillManageInfo = _skillLibrary[skillBaseData];
            if (afterData.SkillDamageIndex == null)
            {
                for (int i = 0; i < afterData.SkillEffect.Count; ++i)
                    skillManageInfo.AddEffectData(afterData.SkillEffect[i]);
            }
            else
            {
                SkillManageInfo afterSkillManageInfo = AddNewSkillManageInfo(afterData, afterInfo);

                skillManageInfo.AddAfterSkill(afterSkillManageInfo);
            }
        }
        //------------------------------------------------------------------------------------
        public void AddModuleData(SkillBaseData skillBaseData, SkillModuleData skillModuleData)
        {
            if (_skillLibrary.ContainsKey(skillBaseData) == false)
                return;

            SkillManageInfo skillManageInfo = _skillLibrary[skillBaseData];
            skillManageInfo.AddModuleData(skillModuleData);
        }
        //------------------------------------------------------------------------------------
        public SkillManageInfo AddNewSkillManageInfo(SkillBaseData skillBaseData, SkillInfo skillInfo = null, V2Enum_Grade skillGrade = V2Enum_Grade.Max)
        {
            SkillManageInfo skillManageInfo = SkillManageInfoPool.GetSkillManageInfo();

            skillManageInfo.MySkillInfo = null;
            bool needCheckSkillEffect = skillManageInfo.SetSkillBaseData(skillBaseData, skillInfo, skillGrade);

            if (_characterControllerBase == null)
            {
                Debug.LogError("_characterControllerBase == null");
                return skillManageInfo;
            }

            if (needCheckSkillEffect == true)
            {
                if (skillBaseData.SkillEffect != null)
                {
                    for (int i = 0; i < skillBaseData.SkillEffect.Count; ++i)
                    {
                        int level = skillInfo == null ? Define.PlayerSkillDefaultLevel : skillInfo.Level;

                        V2CCData v2CCData = Managers.SkillManager.Instance.GetV2CCData(level, skillBaseData.SkillEffect[i], _characterControllerBase.gameObject.transform.position);

                        if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.Revive)
                            _characterControllerBase.AddRevive(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.KillingMoney)
                            _characterControllerBase.AddKillingMoney(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.DoubleInterest)
                            _characterControllerBase.AddDoubleInterest(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.GoldIncreaseAtt
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.GoldIncreaseArmor)
                            _characterControllerBase.AddGoldIncrease(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.WasteGoldBuff)
                            _characterControllerBase.AddGoldUse(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseGasGambleProb)
                            _characterControllerBase.AddIncreaseGasGambleProb(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseGasHpHeal)
                            _characterControllerBase.AddIncreaseGasHpHeal(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseJokerProb)
                            _characterControllerBase.AddIncreaseJokerProb(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.DecreaseGamblePrice)
                            _characterControllerBase.AddDecreaseGamblePriceQueue(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseInterestRate)
                            _characterControllerBase.AddIncreaseInterestRate(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseSynergyCount)
                            _characterControllerBase.AddIncreaseSynergyCount(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.VampiricDmgFire
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.VampiricDmgGold
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.VampiricDmgWater
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.VampiricDmgThunder)
                            _characterControllerBase.AddSynergyVampiricDmg(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.DescendDmgFireCount
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.DescendDmgGoldCount
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.DescendDmgWaterCount
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.DescendDmgThunderCount)
                            _characterControllerBase.AddSynergyDescendDmg(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseHealEffect)
                            _characterControllerBase.AddIncreaseHealEffect(v2CCData.CCValue);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseAttFireCount
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseAttGoldCount
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseAttWaterCount
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseAttThunderCount
                            )
                            _characterControllerBase.AddSynergyStatBuff_Att(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseDefFireCount
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseDefGoldCount
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseDefWaterCount
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseDefThunderCount
                            )
                            _characterControllerBase.AddSynergyStatBuff_Def(v2CCData);
                        else if (skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseHpFireCount
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseHpGoldCount
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseHpWaterCount
                            || skillBaseData.SkillEffect[i].SkillEffectType == V2Enum_SkillEffectType.IncreaseHpThunderCount
                            )
                            _characterControllerBase.AddSynergyStatBuff_Hp(v2CCData);
                    }
                }
            }

            return skillManageInfo;
        }
        //------------------------------------------------------------------------------------
        private void CheckCoolTimeSkill(SkillBaseData skillBaseData, SkillManageInfo skillManageInfo)
        {
            if (skillBaseData.IsUseCoolTime == false)
            {
                _applyNoneCoolTimePassiveBuff.AddLast(skillManageInfo);
                _characterControllerBase.EnablePassiveBuffSkill(skillManageInfo);
            }
            else
            {
                if (skillBaseData.CoolTimeType == Enum_CoolTimeType.HPPercentOver
                    || skillBaseData.CoolTimeType == Enum_CoolTimeType.HPPercentBelow)
                    _checkHPPercentCoolTime.AddLast(skillManageInfo);
                else
                {
                    _checkCoolTime.AddLast(skillManageInfo);
                    SetSkillOriginRemainTime(skillManageInfo);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetRandomEquipSkillCoolTimeDecrease(double decreaseValue)
        {
            if (_originSkillList == null || _originSkillList.Count <= 0)
                return;

            SkillManageInfo skillManageInfo = _originSkillList[Random.Range(0, _originSkillList.Count)];

            skillManageInfo.RemainCount -= (int)decreaseValue;
            skillManageInfo.RemainTime -= (float)decreaseValue;
        }
        //------------------------------------------------------------------------------------
        public void ResetSkillCoolTime(SkillManageInfo skillManageInfo)
        {
            if (skillManageInfo == null)
                return;

            skillManageInfo.RemainCount = 0;
            skillManageInfo.RemainTime = 0;
        }
        //------------------------------------------------------------------------------------
        public SkillManageInfo GetSkillManagerInfo(SkillBaseData skillBaseData)
        {
            if (_skillLibrary.ContainsKey(skillBaseData) == true)
            {
                return _skillLibrary[skillBaseData];
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        private void SetNextReadySkill()
        {
            foreach (var pair in _checkCoolTime)
            {
                SkillManageInfo skillManageInfo = pair;
                
                if (skillManageInfo.IsReady == true)
                {
                    SetReadySkill(skillManageInfo);

                    return;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void SetReadySkill(SkillManageInfo skillManageInfo)
        {
            skillManageInfo.OnTimePlayCount = 0;

            if (skillManageInfo.SkillBaseData.TriggerType == Enum_TriggerType.Passive)
            {
                PlayPassiveSkill(skillManageInfo);
                return;
            }

            if (_readyActiveSkill == null || _readyActiveSkill == _defaultSkill)
                _readyActiveSkill = skillManageInfo;
        }
        //------------------------------------------------------------------------------------
        public void UseSkill(SkillManageInfo skillManageInfo)
        {
            if (skillManageInfo == null)
                return;

            if (_readyActiveSkill == skillManageInfo)
                _readyActiveSkill = null;

            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

            if (_skillLibrary.ContainsKey(skillBaseData) == true)
            {
                skillManageInfo.IsReady = false;

                SetSkillOriginRemainTime(skillManageInfo);

                SetNextReadySkill();
            }
        }
        //------------------------------------------------------------------------------------
        private void PlayPassiveSkill(SkillManageInfo skillManageInfo)
        {
            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

            bool playProjectile = false;
            bool playDirectVisioning = false;

            bool playPierce = false;
            bool playVoid = false;
            bool playRepeatAttack = false;

            CharacterControllerBase _attackTarget = null;

            if (skillBaseData != null && skillBaseData.SkillDamageIndex != null)
            {
                _attackTarget = Managers.AggroManager.Instance.GetIFFTargetCharacter(skillBaseData.TargetCondition, _characterControllerBase, skillManageInfo.GetTargetRange());
                if (_attackTarget == null)
                    return;

                if (skillBaseData.SkillDamageIndex.DamageType == Enum_DamageType.Projectile)
                {
                    playProjectile = true;
                }
                else if (skillBaseData.SkillDamageIndex.DamageType == Enum_DamageType.DirectVisioning)
                {
                    playDirectVisioning = true;
                }
                else if (skillBaseData.SkillDamageIndex.DamageType == Enum_DamageType.Pierce)
                {
                    playPierce = true;
                }
                else if (skillBaseData.SkillDamageIndex.DamageType == Enum_DamageType.Void)
                {
                    playVoid = true;
                }
                else if (skillBaseData.SkillDamageIndex.DamageType == Enum_DamageType.RepeatAttack)
                {
                    playRepeatAttack = true;
                }
            }

            if (playProjectile == true)
                _characterControllerBase.PlayProjectile(skillManageInfo, _attackTarget);
            else if (playDirectVisioning == true)
                _characterControllerBase.PlayDirectVisioning(skillManageInfo, _attackTarget);

            else if (playPierce == true)
                _characterControllerBase.PlayPierce(skillManageInfo, _attackTarget);
            else if (playVoid == true)
                _characterControllerBase.PlayVoid(skillManageInfo, _attackTarget);
            else if (playRepeatAttack == true)
                _characterControllerBase.PlayRepeatAttack(skillManageInfo, _attackTarget);

            else
                _characterControllerBase.PlaySkill(skillManageInfo, _attackTarget);

            if (_skillLibrary.ContainsKey(skillBaseData) == true)
            {
                skillManageInfo.IsReady = false;

                SetSkillOriginRemainTime(skillManageInfo);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetSkillCoolTimeRate(float rate)
        {
            _skillCoolTimeDecreaseRate = rate;
        }
        //------------------------------------------------------------------------------------
        public void SetSkillOriginRemainTime(SkillManageInfo skillManageInfo)
        {
            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

            if (SkillManageInfo.IsCountCoolTime(skillBaseData.CoolTimeType))
            {
                skillManageInfo.SetOriginRemainTime(skillBaseData.CoolTimeValue);
                skillManageInfo.RemainCount = (int)skillManageInfo.GetOutputOriginRemainTime();
            }
            else
            {
                skillManageInfo.SetOriginRemainTime(GetSkillRemainCoolTime(skillBaseData));
                skillManageInfo.RemainTime = skillManageInfo.GetOutputOriginRemainTime();
            }
        }
        //------------------------------------------------------------------------------------
        public float GetSkillRemainCoolTime(SkillBaseData skillBaseData)
        {
            if (skillBaseData.CoolTimeType == Enum_CoolTimeType.Default)
                return skillBaseData.CoolTimeValue - (skillBaseData.CoolTimeValue * _skillCoolTimeDecreaseRate);
            else
                return skillBaseData.CoolTimeValue;
        }
        //------------------------------------------------------------------------------------
        public void AddAttackCount(int count)
        {
            NeedOperAttackCount += count;
        }
        //------------------------------------------------------------------------------------
        public void AddKillCount(int count)
        {
            NeedOperKillCount += count;
        }
        //------------------------------------------------------------------------------------
        public void AddHitCount(int count)
        {
            NeedOperHitCount += count;
        }
        //------------------------------------------------------------------------------------
        public void AddCriticalCount(int count)
        {
            NeedOperCriticalCount += count;
        }
        //------------------------------------------------------------------------------------
        public void AddBossHitCount(int count)
        {
            NeedOperBossHitCount += count;
        }
        //------------------------------------------------------------------------------------
        public void AddNormalHitCount(int count)
        {
            NeedOperNormalHitCount += count;
        }
        //------------------------------------------------------------------------------------
        public void AddInterestCount(int count)
        {
            NeedOperInterestCount += count;
        }
        //------------------------------------------------------------------------------------
        public void AddWaveClearCount(int count)
        {
            NeedOperWaveClearCount += count;
        }
        //------------------------------------------------------------------------------------
        public void AddWaveStartCount(int count)
        {
            NeedOperWaveStartCount += count;
        }
        //------------------------------------------------------------------------------------
        public void AddGambleCardRewardCount(int count, Enum_SynergyType Enum_SynergyType)
        {
            NeedOperGambleCardRewardCount += count;
            _lastChoice_SynergyType = Enum_SynergyType;

            if (Enum_SynergyType == Enum_SynergyType.Red)
                NeedOperGambleCardRewardFire += count;
            else if (Enum_SynergyType == Enum_SynergyType.Yellow)
                NeedOperGambleCardRewardGold += count;
            else if (Enum_SynergyType == Enum_SynergyType.Blue)
                NeedOperGambleCardRewardWater += count;
            else if (Enum_SynergyType == Enum_SynergyType.White)
                NeedOperGambleCardRewardThunder += count;
        }
        //------------------------------------------------------------------------------------
        public void AddFailGasGambleCount(int count)
        {
            NeedOperFailGasGambleCount += count;
        }
        //------------------------------------------------------------------------------------
        public void AddGainGasRewardCount(int count)
        {
            NeedOperGainGasRewardCount += count;
        }
        //------------------------------------------------------------------------------------
        public void AddMissionCompleteCount(int count)
        {
            NeedOperMissionCompleteCount += count;
        }
        //------------------------------------------------------------------------------------
        public void AddReviveCount(int count)
        {
            NeedOperRevive += count;
        }
        //------------------------------------------------------------------------------------
        public SkillManageInfo AddSkillData(SkillBaseData skillBaseData, bool isOriginSkill = false, SkillInfo skillInfo = null, V2Enum_Grade skillGrade = V2Enum_Grade.Max)
        {
            SkillManageInfo skillManageInfo = AddNewSkillManageInfo(skillBaseData, skillInfo);

            if (isOriginSkill == true)
            { 
                _originSkillList.Add(skillManageInfo);
                skillManageInfo.SetMainSkill(isOriginSkill);
            }

            CheckCoolTimeSkill(skillBaseData, skillManageInfo);

            _skillLibrary.Add(skillBaseData, skillManageInfo);

            return skillManageInfo;
        }
        //------------------------------------------------------------------------------------
        public void ReleaseSkill()
        {
            foreach (var pair in _applyNoneCoolTimePassiveBuff)
            {
                _characterControllerBase.ReleasePassiveBuffSkill(pair);
            }

            foreach (var pair in _checkCoolTime)
            {
                SkillManageInfoPool.PoolSkillManageInfo(pair);
            }

            foreach (var pair in _checkHPPercentCoolTime)
            {
                SkillManageInfoPool.PoolSkillManageInfo(pair);
            }

            _originSkillList.Clear();
            _skillLibrary.Clear();
            _checkCoolTime.Clear();
            _checkHPPercentCoolTime.Clear();
            _applyNoneCoolTimePassiveBuff.Clear();
            _addGambleSkill.Clear();
        }
        //------------------------------------------------------------------------------------
        public void Updated()
        {
            //string remaincooltime = string.Empty;

            if (_characterControllerBase == null)
            {
                Debug.LogError("_characterControllerBase == null");
                return;
            }

            bool isHalfOver = _characterControllerBase.IsHalfOver();

            foreach (var pair in _checkHPPercentCoolTime)
            {
                SkillManageInfo skillManageInfo = pair;
                SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

                if (skillBaseData.TargetCheckType == Enum_TargetCheckType.Self)
                {
                    //HPPercentOver = 15, // 현재 HP가 50% 이상
                    //HPPercentBelow = 16, // 현재 HP가 50% 미만
                    if (skillBaseData.CoolTimeType == Enum_CoolTimeType.HPPercentOver)
                        //&& isHalfOver == true)
                    {
                        if (isHalfOver == true && skillManageInfo.IsApplySkill == false)
                        {
                            skillManageInfo.IsReady = true;
                            skillManageInfo.IsApplySkill = true;

                            //SetReadySkill(skillManageInfo);
                            _characterControllerBase.EnablePassiveBuffSkill(skillManageInfo);
                        }
                        else if (isHalfOver == false && skillManageInfo.IsApplySkill == true)
                        {
                            skillManageInfo.IsReady = false;
                            skillManageInfo.IsApplySkill = false;

                            //SetReadySkill(skillManageInfo);
                            _characterControllerBase.ReleasePassiveBuffSkill(skillManageInfo);
                        }

                    }
                    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.HPPercentBelow)
                        //&& isHalfOver == false)
                    {
                        if (isHalfOver == false && skillManageInfo.IsApplySkill == false)
                        {
                            skillManageInfo.IsReady = true;
                            skillManageInfo.IsApplySkill = true;

                            //SetReadySkill(skillManageInfo);
                            _characterControllerBase.EnablePassiveBuffSkill(skillManageInfo);
                        }
                        else if (isHalfOver == true && skillManageInfo.IsApplySkill == true)
                        {
                            skillManageInfo.IsReady = false;
                            skillManageInfo.IsApplySkill = false;

                            //SetReadySkill(skillManageInfo);
                            _characterControllerBase.ReleasePassiveBuffSkill(skillManageInfo);
                        }
                    }
                }
            }

            if (Input.GetKeyUp(KeyCode.Alpha0))
            {
                foreach (var pair in _checkCoolTime)
                {
                    SkillManageInfo skillManageInfo = pair;

                    skillManageInfo.RemainCount = 0;
                    skillManageInfo.RemainTime = 0;
                }
            }

            //foreach (var pair in _checkCoolTime)
            //{
            //    SkillManageInfo skillManageInfo = pair;
            //    SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

            //    if (skillManageInfo.IsReady == true)
            //    {
            //        SetReadySkill(skillManageInfo);

            //        continue;
            //    }


            //    if (skillBaseData.CoolTimeType == Enum_CoolTimeType.AttackCount)
            //        skillManageInfo.RemainCount -= NeedOperAttackCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.KillingCount)
            //        skillManageInfo.RemainCount -= NeedOperKillCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.HitCount)
            //        skillManageInfo.RemainCount -= NeedOperHitCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.CriticalAttack)
            //        skillManageInfo.RemainCount -= NeedOperCriticalCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.IsBoss)
            //        skillManageInfo.RemainCount -= NeedOperBossHitCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.IsNormal)
            //        skillManageInfo.RemainCount -= NeedOperNormalHitCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.Interest)
            //        skillManageInfo.RemainCount -= NeedOperInterestCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.WaveClear)
            //        skillManageInfo.RemainCount -= NeedOperWaveClearCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.WaveStart)
            //        skillManageInfo.RemainCount -= NeedOperWaveStartCount;

            //    if (SkillManageInfo.IsCountCoolTime(skillBaseData.CoolTimeType))
            //    {
            //        if (skillManageInfo.GetRemainCoolTime() <= 0)
            //        {
            //            skillManageInfo.IsReady = true;

            //            SetReadySkill(skillManageInfo);
            //        }
            //    }
            //    else
            //    {
            //        skillManageInfo.RemainTime -= Time.deltaTime * _skillTimerIncreaseRate;

            //        //remaincooltime += string.Format("{0}   ", skillManageInfo.RemainTime);

            //        if (skillManageInfo.GetRemainCoolTime() <= 0.0f)
            //        {
            //            skillManageInfo.IsReady = true;

            //            SetReadySkill(skillManageInfo);
            //        }
            //    }

            //}

            var node = _checkCoolTime.First;

            while (node != null)
            {
                SkillManageInfo skillManageInfo = node.Value;
                SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

                if (skillManageInfo.IsReady == true)
                {
                    SetReadySkill(skillManageInfo);

                    node = node.Next;
                    continue;
                }

                if (skillBaseData.CoolTimeType == Enum_CoolTimeType.AttackCount)
                    skillManageInfo.RemainCount -= NeedOperAttackCount;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.KillingCount)
                    skillManageInfo.RemainCount -= NeedOperKillCount;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.HitCount)
                    skillManageInfo.RemainCount -= NeedOperHitCount;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.CriticalAttack)
                    skillManageInfo.RemainCount -= NeedOperCriticalCount;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.IsBoss)
                    skillManageInfo.RemainCount -= NeedOperBossHitCount;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.IsNormal)
                    skillManageInfo.RemainCount -= NeedOperNormalHitCount;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.Interest)
                    skillManageInfo.RemainCount -= NeedOperInterestCount;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.WaveClear)
                    skillManageInfo.RemainCount -= NeedOperWaveClearCount;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.WaveStart)
                    skillManageInfo.RemainCount -= NeedOperWaveStartCount;

                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.GambleCardReward)
                    skillManageInfo.RemainCount -= NeedOperGambleCardRewardCount;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.FailGasGamble)
                    skillManageInfo.RemainCount -= NeedOperFailGasGambleCount;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.GainGasReward)
                    skillManageInfo.RemainCount -= NeedOperGainGasRewardCount;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.MissionComplete)
                    skillManageInfo.RemainCount -= NeedOperMissionCompleteCount;


                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.GambleCardRewardFire)
                    skillManageInfo.RemainCount -= NeedOperGambleCardRewardFire;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.GambleCardRewardGold)
                    skillManageInfo.RemainCount -= NeedOperGambleCardRewardGold;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.GambleCardRewardWater)
                    skillManageInfo.RemainCount -= NeedOperGambleCardRewardWater;
                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.GambleCardRewardThunder)
                    skillManageInfo.RemainCount -= NeedOperGambleCardRewardThunder;

                else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.Revive)
                    skillManageInfo.RemainCount -= NeedOperRevive;

                node = node.Next;
            }

            //foreach (var pair in _checkCoolTime)
            //{
            //    SkillManageInfo skillManageInfo = pair;
            //    SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

            //    if (skillManageInfo.IsReady == true)
            //    {
            //        SetReadySkill(skillManageInfo);

            //        continue;
            //    }


            //    if (skillBaseData.CoolTimeType == Enum_CoolTimeType.AttackCount)
            //        skillManageInfo.RemainCount -= NeedOperAttackCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.KillingCount)
            //        skillManageInfo.RemainCount -= NeedOperKillCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.HitCount)
            //        skillManageInfo.RemainCount -= NeedOperHitCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.CriticalAttack)
            //        skillManageInfo.RemainCount -= NeedOperCriticalCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.IsBoss)
            //        skillManageInfo.RemainCount -= NeedOperBossHitCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.IsNormal)
            //        skillManageInfo.RemainCount -= NeedOperNormalHitCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.Interest)
            //        skillManageInfo.RemainCount -= NeedOperInterestCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.WaveClear)
            //        skillManageInfo.RemainCount -= NeedOperWaveClearCount;
            //    else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.WaveStart)
            //        skillManageInfo.RemainCount -= NeedOperWaveStartCount;
            //}

            //if (_checkCoolTime.Count > 0)
            //    Debug.Log(remaincooltime);

            RefreshCoolTimeAction?.Invoke();

            NeedOperAttackCount = 0;
            NeedOperKillCount = 0;
            NeedOperHitCount = 0;

            NeedOperCriticalCount = 0;
            NeedOperBossHitCount = 0;
            NeedOperNormalHitCount = 0;
            NeedOperInterestCount = 0;
            NeedOperWaveClearCount = 0;
            NeedOperWaveStartCount = 0;

            NeedOperGambleCardRewardCount = 0;
            NeedOperFailGasGambleCount = 0;
            NeedOperGainGasRewardCount = 0;
            NeedOperMissionCompleteCount = 0;

            NeedOperGambleCardRewardFire = 0;
            NeedOperGambleCardRewardGold = 0;
            NeedOperGambleCardRewardWater = 0;
            NeedOperGambleCardRewardThunder = 0;

            NeedOperRevive = 0;

            node = _checkCoolTime.First;
            while (node != null)
            {
                SkillManageInfo skillManageInfo = node.Value;
                SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

                if (skillManageInfo.IsReady == true)
                {
                    SetReadySkill(skillManageInfo);

                    node = node.Next;
                    continue;
                }

                if (SkillManageInfo.IsCountCoolTime(skillBaseData.CoolTimeType))
                {
                    if (skillManageInfo.GetRemainCoolTime() <= 0)
                    {
                        skillManageInfo.IsReady = true;

                        SetReadySkill(skillManageInfo);
                    }
                }
                else
                {
                    skillManageInfo.RemainTime -= Time.deltaTime * _skillTimerIncreaseRate;

                    //remaincooltime += string.Format("{0}   ", skillManageInfo.RemainTime);

                    if (skillManageInfo.GetRemainCoolTime() <= 0.0f)
                    {
                        skillManageInfo.IsReady = true;

                        SetReadySkill(skillManageInfo);
                    }
                }

                node = node.Next;
            }

            //foreach (var pair in _checkCoolTime)
            //{
            //    SkillManageInfo skillManageInfo = pair;
            //    SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

            //    if (skillManageInfo.IsReady == true)
            //    {
            //        SetReadySkill(skillManageInfo);

            //        continue;
            //    }

            //    if (SkillManageInfo.IsCountCoolTime(skillBaseData.CoolTimeType))
            //    {
            //        if (skillManageInfo.GetRemainCoolTime() <= 0)
            //        {
            //            skillManageInfo.IsReady = true;

            //            SetReadySkill(skillManageInfo);
            //        }
            //    }
            //    else
            //    {
            //        skillManageInfo.RemainTime -= Time.deltaTime * _skillTimerIncreaseRate;

            //        //remaincooltime += string.Format("{0}   ", skillManageInfo.RemainTime);

            //        if (skillManageInfo.GetRemainCoolTime() <= 0.0f)
            //        {
            //            skillManageInfo.IsReady = true;

            //            SetReadySkill(skillManageInfo);
            //        }
            //    }

            //}


            if (_readyActiveSkill != null && _readyActiveSkill != _defaultSkill)
            {
                SkillBaseData skillBaseData = _readyActiveSkill.SkillBaseData;

                CharacterControllerBase _attackTarget = null;
                if (skillBaseData != null && skillBaseData.SkillDamageIndex != null)
                {
                    _attackTarget = Managers.AggroManager.Instance.GetIFFTargetCharacter(skillBaseData.TargetCondition, _characterControllerBase, _readyActiveSkill.GetTargetRange());
                    if (_attackTarget == null)
                    {
                        _readyActiveSkill = _defaultSkill;
                        return;
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}