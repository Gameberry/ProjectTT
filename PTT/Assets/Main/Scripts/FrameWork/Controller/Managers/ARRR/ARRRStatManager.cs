using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using System.Collections.Concurrent;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry.Managers
{
    public class ARRRStatManager : MonoSingleton<ARRRStatManager>
    {
        private Event.RefreshCharacterInfoListMsg m_refreshV3AllyInfoListMsg = new Event.RefreshCharacterInfoListMsg();
        private Event.RefreshCharacterInfoListMsg m_refreshV3AllyInfo_EnhanceMsg = new Event.RefreshCharacterInfoListMsg();
        private Event.RefreshAddBuffMsg _refreshAddBuffMsg = new Event.RefreshAddBuffMsg();

        private Event.RefreshBattlePowerUIMsg _refreshBattlePowerUIMsg = new Event.RefreshBattlePowerUIMsg();

        private CharacterLocalTable _characterLocalTable;

        private Dictionary<V2Enum_Stat, ObscuredDouble> _arrrBaseStatValues = new Dictionary<V2Enum_Stat, ObscuredDouble>();
        private Dictionary<V2Enum_Stat, ObscuredDouble> _arrrLevelUpStatValues = new Dictionary<V2Enum_Stat, ObscuredDouble>();
        private Dictionary<V2Enum_Stat, ObscuredDouble> _arrrLimitStatValues = new Dictionary<V2Enum_Stat, ObscuredDouble>();


        private Dictionary<int, Sprite> _statIcons = new Dictionary<int, Sprite>();

        private List<string> _changeInfoUpdate = new List<string>();

        [SerializeField]
        private float _battlePower_WaitTime = 0.3f;
        private float _refreshBattlePowerTime = 0.0f;
        private bool _checkingRefreshBattlePowerTimer = false;

        private Dictionary<V2Enum_Stat, ObscuredDouble> _battlePowerOperationStat = new Dictionary<V2Enum_Stat, ObscuredDouble>();

        private double _battlePower = 0.0;

        private int _synergyInCreaseCount = 0;
        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _changeInfoUpdate.Add(Define.PlayerARRRInfoTable);
            _changeInfoUpdate.Add(Define.PlayerPointTable);

            _characterLocalTable = TableManager.Instance.GetTableClass<CharacterLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public void InitARRRStatContent()
        {
            RefreshARRRBaseStat(ref _arrrBaseStatValues);
            RefreshARRRTotalLevelUpStat(ARRRContainer.ARRRLevel, ref _arrrLevelUpStatValues);
            RefreshARRRTotalLimitStat(ARRRContainer.ARRRLimitCompleteLevel, ref _arrrLimitStatValues);
        }
        //------------------------------------------------------------------------------------
        #region Data
        //------------------------------------------------------------------------------------
        public CharacterBaseStatData GetCharacterBaseStatData(V2Enum_Stat type)
        {
            return _characterLocalTable.GetCharacterBaseStatData(type);
        }
        //------------------------------------------------------------------------------------
        public CharacterLevelUpCostData GetCharacterLevelUpCostData(ObscuredInt currlevel)
        {
            if (IsMaxLevel(currlevel) == true)
                return null;

            return _characterLocalTable.GetCharacterLevelUpCostData(currlevel + 1);
        }
        //------------------------------------------------------------------------------------
        public CharacterLevelUpStatData GetCharacterLevelUpStatData(ObscuredInt level)
        {
            return _characterLocalTable.GetCharacterLevelUpStatData(level);
        }
        //------------------------------------------------------------------------------------
        public List<CharacterLevelUpStatData> GetCharacterLevelUpStatDatas()
        {
            return _characterLocalTable.GetCharacterLevelUpStatDatas();
        }
        //------------------------------------------------------------------------------------
        public CharacterLevelUpLimitData GetCharacterLevelUpLimitData(ObscuredInt level)
        {
            return _characterLocalTable.GetCharacterLevelUpLimitData(level);
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, CharacterLevelUpLimitData> GetCharacterLevelUpLimitDatas()
        {
            return _characterLocalTable.GetCharacterLevelUpLimitDatas();
        }
        //------------------------------------------------------------------------------------
        public ObscuredInt GetCharacterLevel()
        {
            return ARRRContainer.ARRRLevel;
        }
        //------------------------------------------------------------------------------------
        public ObscuredInt GetCharacterARRRLimitCompleteLevel()
        {
            return ARRRContainer.ARRRLimitCompleteLevel;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetStatSprite(V2Enum_Stat type)
        {
            Sprite sp = null;

            CharacterBaseStatData characterBaseStatData = _characterLocalTable.GetCharacterBaseStatData(type);
            if (characterBaseStatData == null)
                return sp;

            int iconIndex = characterBaseStatData.ResourceIndex;

            if (_statIcons.ContainsKey(iconIndex) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(Define.StatIconPath, iconIndex), o =>
                {
                    sp = o as Sprite;
                    _statIcons.Add(iconIndex, sp);
                });
            }
            else
                sp = _statIcons[iconIndex];

            return sp;
        }
        //------------------------------------------------------------------------------------
        public string GetStatLocal(V2Enum_Stat type)
        {
            Sprite sp = null;

            CharacterBaseStatData characterBaseStatData = _characterLocalTable.GetCharacterBaseStatData(type);
            if (characterBaseStatData == null)
                return string.Empty;

            return characterBaseStatData.NameLocalStringKey;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region StatRefresh
        //------------------------------------------------------------------------------------
        public void GetPlayerARRRDefaultStat(ref Dictionary<V2Enum_Stat, ObscuredDouble> defaultStat)
        {
            defaultStat.Clear();

            Dictionary<V2Enum_Stat, ObscuredDouble> synergyStat = SynergyManager.Instance.ArrrSynergyTotalStatValues;
            Dictionary<V2Enum_Stat, ObscuredDouble> descendStat = DescendManager.Instance.ArrrSynergyTotalStatValues;
            Dictionary<V2Enum_Stat, ObscuredDouble> researchStat = ResearchManager.Instance.ArrrSynergyTotalStatValues;
            Dictionary<V2Enum_Stat, ObscuredDouble> gearStat = GearManager.Instance.ArrrSynergyTotalStatValues;
            Dictionary<V2Enum_Stat, ObscuredDouble> JobStat = JobManager.Instance.ArrrSynergyTotalStatValues;

            foreach (var pair in _arrrBaseStatValues)
            {
                defaultStat.Add(pair.Key, pair.Value);
                
                //if (_arrrLevelUpStatValues.ContainsKey(pair.Key) == true)
                //{
                //    defaultStat[pair.Key] += _arrrLevelUpStatValues[pair.Key];
                //}

                //if (_arrrLimitStatValues.ContainsKey(pair.Key) == true)
                //{
                //    defaultStat[pair.Key] += _arrrLimitStatValues[pair.Key];
                //}

                if (synergyStat.ContainsKey(pair.Key) == true)
                {
                    defaultStat[pair.Key] += synergyStat[pair.Key];
                }

                if (descendStat.ContainsKey(pair.Key) == true)
                {
                    defaultStat[pair.Key] += descendStat[pair.Key];
                }

                if (researchStat.ContainsKey(pair.Key) == true)
                {
                    defaultStat[pair.Key] += researchStat[pair.Key];
                }

                if (gearStat.ContainsKey(pair.Key) == true)
                {
                    defaultStat[pair.Key] += gearStat[pair.Key];
                }

                if (JobStat.ContainsKey(pair.Key) == true)
                {
                    defaultStat[pair.Key] += JobStat[pair.Key];
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshBattlePower()
        {
            _refreshBattlePowerTime = Time.time + _battlePower_WaitTime;
            if (_checkingRefreshBattlePowerTimer == false)
            {
                _checkingRefreshBattlePowerTimer = true;
                RefreshBattlePowerTimer().Forget();
            }
        }
        //------------------------------------------------------------------------------------
        public void SetBattlePower()
        {
            double power = 0;

            double opwervalue = 0;

            GetPlayerARRRDefaultStat(ref _battlePowerOperationStat);

            foreach (var pair in _battlePowerOperationStat)
            {
                opwervalue = _characterLocalTable.GetBattlePowerConvertValue(pair.Key);
                power += opwervalue * pair.Value;
            }

            // ½Ã³ÊÁö ÃÑÇÕ ·¹º§ ÆÄ¿ö
            opwervalue = _characterLocalTable.GetBattlePowerConvertValue(Enum_SynergyPowerType.LevelUpSynergy);
            power += opwervalue * SynergyContainer.SynergyEffectCurrentTotalLevel;

            // ½Ã³ÊÁö ·é È¹µæ ÆÄ¿ö
            foreach (var pair in SynergyContainer.Runes)
            {
                SynergyBreakthroughData synergyRuneData = Managers.SynergyManager.Instance.GetSynergyBreakthroughData(pair);
                if(synergyRuneData.Grade == V2Enum_Grade.C)
                    opwervalue = _characterLocalTable.GetBattlePowerConvertValue(Enum_SynergyPowerType.GetCSynergyRune);
                else if (synergyRuneData.Grade == V2Enum_Grade.B)
                    opwervalue = _characterLocalTable.GetBattlePowerConvertValue(Enum_SynergyPowerType.GetBSynergyRune);
                else if (synergyRuneData.Grade == V2Enum_Grade.A)
                    opwervalue = _characterLocalTable.GetBattlePowerConvertValue(Enum_SynergyPowerType.GetASynergyRune);
                else if (synergyRuneData.Grade == V2Enum_Grade.S)
                    opwervalue = _characterLocalTable.GetBattlePowerConvertValue(Enum_SynergyPowerType.GetSSynergyRune);

                power += opwervalue;
            }


            // °­¸² È¹µæ ÆÄ¿ö
            opwervalue = _characterLocalTable.GetBattlePowerConvertValue(Enum_DescendPowerType.GetDescend);
            power += opwervalue * DescendContainer.SynergyInfo.Count;

            // °­¸² ÃÑÇÕ ·¹º§ ÆÄ¿ö
            opwervalue = _characterLocalTable.GetBattlePowerConvertValue(Enum_DescendPowerType.LevelupDescend);
            power += opwervalue * DescendContainer.SynergyAccumLevel;


            // À¯¹° È¹µæ ÆÄ¿ö
            opwervalue = _characterLocalTable.GetBattlePowerConvertValue(Enum_RelicPowerType.GetRelic);
            power += opwervalue * RelicContainer.SynergyInfo.Count;

            // °­¸² ÃÑÇÕ ·¹º§ ÆÄ¿ö
            opwervalue = _characterLocalTable.GetBattlePowerConvertValue(Enum_RelicPowerType.LevelUpRelic);
            power += opwervalue * RelicContainer.SynergyAccumLevel;


            _battlePower = power;

            int addsynergycount = 0;

            addsynergycount += Managers.GearManager.Instance.SynergyInCreaseCount;

            _synergyInCreaseCount = addsynergycount + 1;

        }
        //------------------------------------------------------------------------------------
        private async UniTask RefreshBattlePowerTimer()
        {
            while (Time.time < _refreshBattlePowerTime)
                await UniTask.NextFrame();

            double before = _battlePower;
            int beforegauge = _synergyInCreaseCount;

            SetBattlePower();
            double after = _battlePower;
            int aftergauge = _synergyInCreaseCount;

            Managers.RankManager.Instance.SetBattlePower(_battlePower);

            if (before != after)
                Contents.GlobalContent.ShowGlobalNotice_BattlePower(after, after - before);

            if (beforegauge != aftergauge)
                Contents.GlobalContent.ShowGlobalNotice_SynergyCount(beforegauge, aftergauge);

            Message.Send(_refreshBattlePowerUIMsg);

            _checkingRefreshBattlePowerTimer = false;
        }
        //------------------------------------------------------------------------------------
        public double GetBattlePower()
        {
            return _battlePower;
        }
        //------------------------------------------------------------------------------------
        public void RefreshARRRBaseStat(ref Dictionary<V2Enum_Stat, ObscuredDouble> DefauleStatValue)
        {
            DefauleStatValue.Clear();

            List<CharacterBaseStatData> characterBaseStatDatas = _characterLocalTable.GetCharacterBaseStatDatas();

            for (int i = 0; i < characterBaseStatDatas.Count; ++i)
            {
                CharacterBaseStatData characterBaseStatData = characterBaseStatDatas[i];
                DefauleStatValue.Add(characterBaseStatData.BaseStat, characterBaseStatData.BaseValue);
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshARRRTotalLevelUpStat(ObscuredInt level, ref Dictionary<V2Enum_Stat, ObscuredDouble> DefauleStatValue)
        {
            DefauleStatValue.Clear();

            List<CharacterLevelUpStatData> characterLevelUpStatDatas = GetCharacterLevelUpStatDatas();

            int checkLastDataMaxLevel = 0;

            for (int i = 0; i < characterLevelUpStatDatas.Count; ++i)
            {
                CharacterLevelUpStatData creatureLevelUpStatData = characterLevelUpStatDatas[i];

                int operLevel = 0;

                if (creatureLevelUpStatData.MaximumLevel < level)
                {
                    operLevel = creatureLevelUpStatData.MaximumLevel - checkLastDataMaxLevel;

                    foreach (var pair in creatureLevelUpStatData.StatValue)
                    {
                        if (DefauleStatValue.ContainsKey(pair.Key) == false)
                            DefauleStatValue.Add(pair.Key, 0);

                        DefauleStatValue[pair.Key] += pair.Value.BaseValue * operLevel;
                    }

                    checkLastDataMaxLevel = creatureLevelUpStatData.MaximumLevel;

                    continue;
                }

                operLevel = level - checkLastDataMaxLevel;

                foreach (var pair in creatureLevelUpStatData.StatValue)
                {
                    if (DefauleStatValue.ContainsKey(pair.Key) == false)
                        DefauleStatValue.Add(pair.Key, 0);

                    DefauleStatValue[pair.Key] += pair.Value.BaseValue * operLevel;
                }

                break;
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshARRRTotalLimitStat(ObscuredInt level, ref Dictionary<V2Enum_Stat, ObscuredDouble> DefauleStatValue)
        {
            DefauleStatValue.Clear();
            Dictionary<ObscuredInt, CharacterLevelUpLimitData> characterLevelUpLimitDatas = GetCharacterLevelUpLimitDatas();

            foreach (var pair in characterLevelUpLimitDatas)
            {
                if (pair.Key <= level)
                {
                    foreach (var pairvalue in pair.Value.LimitStatValue)
                    {
                        if (DefauleStatValue.ContainsKey(pairvalue.Key) == false)
                            DefauleStatValue.Add(pairvalue.Key, 0);

                        DefauleStatValue[pairvalue.Key] += pairvalue.Value.BaseValue;
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Level
        //------------------------------------------------------------------------------------
        public bool IsMaxLevel(ObscuredInt currlevel)
        {
            return _characterLocalTable.GetMaxLevel() <= currlevel;
        }
        //------------------------------------------------------------------------------------
        public bool NeedLimitBreak(ObscuredInt level)
        {
            if (ARRRContainer.ARRRLimitCompleteLevel >= level)
                return false;

            CharacterLevelUpLimitData characterLevelUpLimitData = GetCharacterLevelUpLimitData(level);

            if (characterLevelUpLimitData == null)
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
        public double GetLevelUpCostAmount(CharacterLevelUpCost characterLevelUpCost, ObscuredInt level)
        {
            int tempLevel = level - characterLevelUpCost.PrevMaxLevel;

            double need = characterLevelUpCost.LevelUpCostGoodsParam2 + (characterLevelUpCost.LevelUpCostGoodsParam3 * tempLevel);

            return need;
        }
        //------------------------------------------------------------------------------------
        public bool ReadyCharacterLevelUpCost(CharacterLevelUpCost characterLevelUpCost, ObscuredInt level)
        {
            double amount = GoodsManager.Instance.GetGoodsAmount(characterLevelUpCost.LevelUpCostGoodsType.Enum32ToInt(), characterLevelUpCost.LevelUpCostGoodsParam1);

            double need = GetLevelUpCostAmount(characterLevelUpCost, level);

            return amount >= need;
        }
        //------------------------------------------------------------------------------------
        public bool ReadyCharacterLimitLevelUpCost(CharacterLevelUpLimitCost characterLevelUpCost)
        {
            double amount = GoodsManager.Instance.GetGoodsAmount(characterLevelUpCost.LimitBreakCostGoodsType.Enum32ToInt(), characterLevelUpCost.LimitBreakCostGoodsParam1);

            double need = characterLevelUpCost.LimitBreakCostGoodsParam2;

            return amount >= need;
        }
        //------------------------------------------------------------------------------------
        public bool DoARRRLevelUp()
        {
            if (NeedLimitBreak(ARRRContainer.ARRRLevel) == true)
                return false;

            if (IsMaxLevel(ARRRContainer.ARRRLevel) == true)
                return false;

            CharacterLevelUpCostData characterLevelUpCostData = GetCharacterLevelUpCostData(ARRRContainer.ARRRLevel);

            if (characterLevelUpCostData == null)
                return false;

            CharacterLevelUpStatData characterLevelUpStatData = GetCharacterLevelUpStatData(ARRRContainer.ARRRLevel);

            if (characterLevelUpStatData == null)
                return false;

            for (int i = 0; i < characterLevelUpCostData.LevelUpCostGoods.Count; ++i)
            {
                if (ReadyCharacterLevelUpCost(characterLevelUpCostData.LevelUpCostGoods[i], ARRRContainer.ARRRLevel) == false)
                    return false;
            }

            List<int> used_type = new List<int>();
            List<double> former_quan = new List<double>();
            List<double> used_quan = new List<double>();
            List<double> keep_quan = new List<double>();


            for (int i = 0; i < characterLevelUpCostData.LevelUpCostGoods.Count; ++i)
            {
                CharacterLevelUpCost characterLevelUpCost = characterLevelUpCostData.LevelUpCostGoods[i];

                double needAmount = GetLevelUpCostAmount(characterLevelUpCost, ARRRContainer.ARRRLevel);

                used_type.Add(characterLevelUpCost.LevelUpCostGoodsParam1);
                former_quan.Add(GoodsManager.Instance.GetGoodsAmount(characterLevelUpCost.LevelUpCostGoodsType.Enum32ToInt(), characterLevelUpCost.LevelUpCostGoodsParam1));
                used_quan.Add(needAmount);

                GoodsManager.Instance.UseGoodsAmount(characterLevelUpCost.LevelUpCostGoodsType.Enum32ToInt(), characterLevelUpCost.LevelUpCostGoodsParam1, needAmount);

                keep_quan.Add(GoodsManager.Instance.GetGoodsAmount(characterLevelUpCost.LevelUpCostGoodsType.Enum32ToInt(), characterLevelUpCost.LevelUpCostGoodsParam1));

            }

            ARRRContainer.ARRRLevel++;


            foreach (var pair in characterLevelUpStatData.StatValue)
            {
                if (_arrrLevelUpStatValues.ContainsKey(pair.Key) == false)
                    _arrrLevelUpStatValues.Add(pair.Key, 0);

                _arrrLevelUpStatValues[pair.Key] += pair.Value.BaseValue;
            }

            //GuideQuestManager.Instance.AddEventCount(V2Enum_EventType.AllyLevelUp, 1);
            //MissionManager.Instance.AddMissionCount(V2Enum_MissionType.AllyLevelUpCount, 1);

            GameBerry.Managers.QuestManager.Instance.AddMissionCount(GameBerry.V2Enum_QuestGoalType.CharacterLevel, 1);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(_changeInfoUpdate, null);

            BattleSceneManager.Instance.RefreshMyARRRStat();

            ThirdPartyLog.Instance.SendLog_Character(ARRRContainer.ARRRLevel, MapContainer.MapLastEnter);
            ThirdPartyLog.Instance.SendLog_log_levelup(ARRRContainer.ARRRLevel, used_type, former_quan, used_quan, keep_quan);

            Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.CharacterLevel);
            Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.CharacterLevel);

            PassManager.Instance.CheckPassType(V2Enum_PassType.CharacterLevel);


            return true;
        }
        //------------------------------------------------------------------------------------
        public bool DoARRRLimitUp()
        {
            if (NeedLimitBreak(ARRRContainer.ARRRLevel) == false)
                return false;

            CharacterLevelUpLimitData characterLevelUpLimitData = GetCharacterLevelUpLimitData(ARRRContainer.ARRRLevel);

            if (characterLevelUpLimitData == null)
                return false;

            for (int i = 0; i < characterLevelUpLimitData.LimitCostGoods.Count; ++i)
            {
                if (ReadyCharacterLimitLevelUpCost(characterLevelUpLimitData.LimitCostGoods[i]) == false)
                    return false;
            }

            List<int> used_type = new List<int>();
            List<double> former_quan = new List<double>();
            List<double> used_quan = new List<double>();
            List<double> keep_quan = new List<double>();


            for (int i = 0; i < characterLevelUpLimitData.LimitCostGoods.Count; ++i)
            {
                CharacterLevelUpLimitCost characterLevelUpCost = characterLevelUpLimitData.LimitCostGoods[i];

                double needAmount = characterLevelUpCost.LimitBreakCostGoodsParam2;

                used_type.Add(characterLevelUpCost.LimitBreakCostGoodsParam1);
                former_quan.Add(GoodsManager.Instance.GetGoodsAmount(characterLevelUpCost.LimitBreakCostGoodsType.Enum32ToInt(), characterLevelUpCost.LimitBreakCostGoodsParam1));
                used_quan.Add(needAmount);

                GoodsManager.Instance.UseGoodsAmount(characterLevelUpCost.LimitBreakCostGoodsType.Enum32ToInt(), characterLevelUpCost.LimitBreakCostGoodsParam1, needAmount);

                keep_quan.Add(GoodsManager.Instance.GetGoodsAmount(characterLevelUpCost.LimitBreakCostGoodsType.Enum32ToInt(), characterLevelUpCost.LimitBreakCostGoodsParam1));
            }

            ARRRContainer.ARRRLimitCompleteLevel = ARRRContainer.ARRRLevel;

            foreach (var pair in characterLevelUpLimitData.LimitStatValue)
            {
                if (_arrrLimitStatValues.ContainsKey(pair.Key) == false)
                    _arrrLimitStatValues.Add(pair.Key, 0);

                _arrrLimitStatValues[pair.Key] += pair.Value.BaseValue;
            }

            //GuideQuestManager.Instance.AddEventCount(V2Enum_EventType.AllyLevelUp, 1);
            //MissionManager.Instance.AddMissionCount(V2Enum_MissionType.AllyLevelUpCount, 1);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_log_limitup(ARRRContainer.ARRRLevel, used_type, former_quan, used_quan, keep_quan);

            BattleSceneManager.Instance.RefreshMyARRRStat();

            Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.BreakthroughCount);

            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        public void RefreshBuffUI(V2Enum_Stat v2Enum_Stat)
        {
            _refreshAddBuffMsg.v2Enum_Stat = v2Enum_Stat;
            Message.Send(_refreshAddBuffMsg);
        }
        //------------------------------------------------------------------------------------
    }
}