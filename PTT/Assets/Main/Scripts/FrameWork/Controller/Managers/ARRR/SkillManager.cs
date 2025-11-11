using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class CCStateData
    {
        public V2Enum_SkillEffectType CCTypeEnum;
        public float CCStartTime;
        public float CCTime;
        public double CCValue;
        public Vector3 AttackerPos;

        public float LastApplyTime;
    }

    public class EffectBuffData
    {
        public V2Enum_SkillEffectType CCTypeEnum;
        public V2Enum_Stat Enum_Stat;
        public double StatValue;
        public float BuffEndTime;

        public bool IsDebuff = false;
    }

    public class ShieldData
    {
        public double RemainShield;
        public float ShieldEndTime;
    }

    public class SkillManager : MonoSingleton<SkillManager>
    {
        private Queue<CCStateData> _cCStateDataPool = new Queue<CCStateData>();
        private Queue<EffectBuffData> _effectBuffDataPool = new Queue<EffectBuffData>();
        private Queue<ShieldData> _shieldDataPool = new Queue<ShieldData>();

        private static string _FriendSkillTrigger = "IFF_FriendSkillTrigger";
        private static string _FoeSkillTrigger = "IFF_FoeSkillTrigger";

        private SkillLocalTable _skillLocalTable;

        private Dictionary<int, Sprite> _skillIcons = new Dictionary<int, Sprite>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _skillLocalTable = TableManager.Instance.GetTableClass<SkillLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public SkillBaseData GetSkillBaseData(int index)
        {
            return _skillLocalTable.GetSkillBaseData(index);
        }
        //------------------------------------------------------------------------------------
        public double GetSkillEffectValue(SkillEffectData skillEffectData, int level)
        {
            return skillEffectData.SkillEffectValue + (skillEffectData.SkillEffectIncreasePerLevel * level);
        }
        //------------------------------------------------------------------------------------
        public SkillDamageData GetSkillDamageData(int index)
        {
            return _skillLocalTable.GetSkillDamageData(index);
        }
        //------------------------------------------------------------------------------------
        public SkillEffectData GetSkillEffectData(int index)
        {
            return _skillLocalTable.GetSkillEffectData(index);
        }
        //------------------------------------------------------------------------------------
        public SkillModuleData GetSkillModuleData(int index)
        {
            return _skillLocalTable.GetSkillModuleData(index);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSkillIcon(SkillBaseData skillBaseData)
        {
            if (skillBaseData == null)
                return null;

            return GetSkillIcon(skillBaseData.SkillIconIndex);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSkillIcon(int iconIndex)
        {
            Sprite sp = null;

            if (_skillIcons.ContainsKey(iconIndex) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(Define.SkillIconPath, iconIndex), o =>
                {
                    sp = o as Sprite;
                    _skillIcons.Add(iconIndex, sp);
                });
            }
            else
                sp = _skillIcons[iconIndex];

            return sp;
        }
        //------------------------------------------------------------------------------------
        public MainSkillData GetMainSkillData(ObscuredInt index)
        {
            return _skillLocalTable.GetGambleSkillData(index);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetMainSkillIcon(MainSkillData gambleSkillData)
        {
            if (gambleSkillData == null)
                return null;

            return GetSkillIcon(gambleSkillData.MainSkillIcon);
        }
        //------------------------------------------------------------------------------------
        public IFFType GetEnemyIFFType(IFFType iFFType)
        {
            if (iFFType == IFFType.IFF_Foe)
                return IFFType.IFF_Friend;

            return IFFType.IFF_Foe;
        }
        //------------------------------------------------------------------------------------
        public string GetLayerTrigger(IFFType iFFType)
        {
            if (iFFType == IFFType.IFF_Friend)
                return _FriendSkillTrigger;

            return _FoeSkillTrigger;
        }
        //------------------------------------------------------------------------------------
        public V2SkillAttackData GetCreatureSkillAttackData(SkillManageInfo skillManageInfo, CharacterControllerBase creatureController, Vector3 actorPosition)
        {
            V2SkillAttackData v2SkillAttackData = new V2SkillAttackData();

            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

            v2SkillAttackData.skillBaseData = skillBaseData;

            if (skillBaseData == null)
            {
                v2SkillAttackData.v2DamageDatas = new V2DamageData();
                v2SkillAttackData.actorType = IFFType.IFF_None;

                return v2SkillAttackData;
            }

            v2SkillAttackData.HitCount = skillManageInfo.GetHitCount();

            v2SkillAttackData.IsMain = skillManageInfo.IsMain();

            int level = skillManageInfo.GetSkillLevel();

            double attackValue = creatureController.GetOutPutMyStat(V2Enum_Stat.Attack);

            v2SkillAttackData.v2DamageDatas = new V2DamageData();
            v2SkillAttackData.v2DamageDatas.SkillValue = skillManageInfo.GetDamageFactor();
            if (skillManageInfo.myDescend != null)
            {
                v2SkillAttackData.v2DamageDatas.SkillValue += v2SkillAttackData.v2DamageDatas.SkillValue * (Managers.DescendManager.Instance.GetInGameDescendDamageRatio(skillManageInfo.myDescend) * 0.01);

                v2SkillAttackData.v2DamageDatas.SkillValue += v2SkillAttackData.v2DamageDatas.SkillValue * Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.GetOutputSynergyDescendDmg();
            }

            v2SkillAttackData.v2DamageDatas.AttackValue = attackValue;


            if (skillManageInfo.MotherTargetEffectDatas != null && skillManageInfo.MotherTargetEffectDatas.Count > 0)
            {
                v2SkillAttackData.v2CCDatas = new List<V2CCData>();

                List<SKillEffectInfo> characterSkillDataBaseList = skillManageInfo.MotherTargetEffectDatas;

                for (int i = 0; i < characterSkillDataBaseList.Count; ++i)
                {
                    SKillEffectInfo skillEffectData = characterSkillDataBaseList[i];
                    if (skillEffectData == null)
                        continue;

                    if (creatureController != null)
                    {
                        V2CCData v2CCData = GetV2CCData(level, skillEffectData, actorPosition);

                        if (skillEffectData.SkillEffectType == V2Enum_SkillEffectType.AdditionalDmg)
                        {
                            if (v2SkillAttackData.AddDamageEffecter == null)
                                v2SkillAttackData.AddDamageEffecter = new List<V2CCData>();

                            v2SkillAttackData.AddDamageEffecter.Add(v2CCData);
                        }
                        else if (skillEffectData.SkillEffectType == V2Enum_SkillEffectType.Death)
                        {
                            if (v2SkillAttackData.DeadEffecter == null)
                                v2SkillAttackData.DeadEffecter = new List<V2CCData>();

                            v2SkillAttackData.DeadEffecter.Add(v2CCData);
                        }
                        else if (skillEffectData.SkillEffectType == V2Enum_SkillEffectType.ResetUsedSkillCooltime)
                        {
                            if (v2SkillAttackData.SelfEffecter == null)
                                v2SkillAttackData.SelfEffecter = new List<V2CCData>();

                            v2SkillAttackData.SelfEffecter.Add(v2CCData);
                        }
                        else if (skillEffectData.SkillEffectType == V2Enum_SkillEffectType.VampiricDmg)
                        {
                            if (v2SkillAttackData.VampiricDmgEffecter == null)
                                v2SkillAttackData.VampiricDmgEffecter = new List<V2CCData>();

                            v2SkillAttackData.VampiricDmgEffecter.Add(v2CCData);
                        }
                        else if (skillEffectData.SkillEffectType == V2Enum_SkillEffectType.DamageBoost)
                        {
                            v2SkillAttackData.DamageBoost += v2CCData.CCValue;
                        }
                        else
                            v2SkillAttackData.v2CCDatas.Add(v2CCData);
                    }
                }
            }
            else
                v2SkillAttackData.v2CCDatas = null;

            v2SkillAttackData.characterControllerBase = creatureController;
            v2SkillAttackData.criticalChance = creatureController.GetOutPutMyStat(V2Enum_Stat.CritChance);
            v2SkillAttackData.criticalChance += skillManageInfo.GetAddIncreaseCritChance();
            v2SkillAttackData.actorType = creatureController.IFFType;

            return v2SkillAttackData;
        }
        //------------------------------------------------------------------------------------
        public V2CCData GetV2CCData(int mylevel, SkillEffectData skillEffectData, Vector3 pos)
        {
            V2CCData v2CCData;

            double value = skillEffectData.SkillEffectValue + (skillEffectData.SkillEffectIncreasePerLevel * mylevel);
            float durationValue = skillEffectData.Duration + (skillEffectData.DurationIncreasePerLevel * mylevel);
            double prop = skillEffectData.SkillEffectProb + (skillEffectData.SkillEffectProbIncreasePerLevel * mylevel);

            v2CCData.CCTypeEnum = skillEffectData.SkillEffectType;
            v2CCData.TargetCondition = skillEffectData.TargetState;
            v2CCData.CCTime = durationValue;
            v2CCData.CCValue = value;
            v2CCData.CCProb = prop;
            v2CCData.AttackerPos = pos;

            return v2CCData;
        }
        //------------------------------------------------------------------------------------
        public V2CCData GetV2CCData(int mylevel, SKillEffectInfo skillEffectData, Vector3 pos)
        {
            V2CCData v2CCData;

            double value = skillEffectData.SkillEffectValue + (skillEffectData.SkillEffectIncreasePerLevel * mylevel);
            float durationValue = skillEffectData.Duration + (skillEffectData.DurationIncreasePerLevel * mylevel);
            double prop = skillEffectData.SkillEffectProb + (skillEffectData.SkillEffectProbIncreasePerLevel * mylevel);

            v2CCData.CCTypeEnum = skillEffectData.SkillEffectType;
            v2CCData.TargetCondition = skillEffectData.TargetState;
            v2CCData.CCTime = durationValue;
            v2CCData.CCValue = value;
            v2CCData.CCProb = prop;
            v2CCData.AttackerPos = pos;

            return v2CCData;
        }
        //------------------------------------------------------------------------------------
        public CCStateData GetCCStateData()
        {
            if (_cCStateDataPool.Count > 0)
                return _cCStateDataPool.Dequeue();

            return new CCStateData();
        }
        //-----------------------------------------------------------------------------------
        public void PoolCCStateData(CCStateData cCStateData)
        {
            _cCStateDataPool.Enqueue(cCStateData);
        }
        //-----------------------------------------------------------------------------------
        public EffectBuffData GetEffectBuffData()
        {
            if (_effectBuffDataPool.Count > 0)
                return _effectBuffDataPool.Dequeue();

            return new EffectBuffData();
        }
        //-----------------------------------------------------------------------------------
        public string GetEffectLocalString(SkillEffectData skillEffectData, int level = 0, double overridevalue = 0)
        {
            string localstring = string.Empty;

            double addvalue = (skillEffectData.SkillEffectIncreasePerLevel * level);
            double value = skillEffectData.SkillEffectValue + addvalue;

            if (skillEffectData.SkillEffectType != V2Enum_SkillEffectType.GoldGainTimer
                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.EarnInterest
                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.RandomSynergyGain

                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.GetGas
                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.MinorJoker
                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.GetSynergyFire
                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.GetSynergyGold
                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.GetSynergyWater
                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.GetSynergyThunder
                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.ReduceSynergyFire
                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.ReduceSynergyGold
                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.ReduceSynergyWater
                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.ReduceSynergyThunder
                && skillEffectData.SkillEffectType != V2Enum_SkillEffectType.ReduceDescendExp)
            {
                addvalue *= 100;
                value *= 100;
            }

            float adddurationValue = (skillEffectData.DurationIncreasePerLevel * level);
            float durationValue = skillEffectData.Duration + adddurationValue;

            double addprop = (skillEffectData.SkillEffectProbIncreasePerLevel * level);
            double prop = skillEffectData.SkillEffectProb + addprop;

            if(overridevalue != 0)
            { 

            }

            try
            {
                localstring = string.Format(Managers.LocalStringManager.Instance.GetLocalString(skillEffectData.DescLocalKey)
                , value, addvalue
                , prop, addprop
                , durationValue, adddurationValue);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("EffectDesc Error {0}\n{1}", skillEffectData.Index, Managers.LocalStringManager.Instance.GetLocalString(skillEffectData.DescLocalKey)));
            }

            return localstring;
        }
        //-----------------------------------------------------------------------------------
        public void PoolEffectBuffData(EffectBuffData effectBuffData)
        {
            _effectBuffDataPool.Enqueue(effectBuffData);
        }
        //-----------------------------------------------------------------------------------
        public ShieldData GetShieldData()
        {
            if (_shieldDataPool.Count > 0)
                return _shieldDataPool.Dequeue();

            return new ShieldData();
        }
        //-----------------------------------------------------------------------------------
        public void PoolShieldData(ShieldData effectBuffData)
        {
            _shieldDataPool.Enqueue(effectBuffData);
        }
        //-----------------------------------------------------------------------------------
    }
}