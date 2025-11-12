using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class SynergyManager : MonoSingleton<SynergyManager>
    {
        private Dictionary<Enum_SynergyType, List<MainSkillData>> _synergySkillList = new Dictionary<Enum_SynergyType, List<MainSkillData>>();
        private Dictionary<Enum_SynergyType, int> _synergyStack = new Dictionary<Enum_SynergyType, int>();
        private Dictionary<Enum_SynergyType, int> _synergyLevel = new Dictionary<Enum_SynergyType, int>();
        private List<SynergyCombineData> _equipedSynergyCombineSkillList = new List<SynergyCombineData>();
        private List<SynergyCombineData> _readySynergyCombineSkillList = new List<SynergyCombineData>();
        private Dictionary<Enum_SynergyType, int> _synergyUnlockTier = new Dictionary<Enum_SynergyType, int>();

        public List<Enum_SynergyType> SynergySortView = new List<Enum_SynergyType>();

        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();
        private GameBerry.Event.ShowGambleSynergyDetailMsg _showGambleSynergyDetailMsg = new Event.ShowGambleSynergyDetailMsg();
        private GameBerry.Event.RefreshGambleSynergyMsg _refreshGambleSynergyMsg = new Event.RefreshGambleSynergyMsg();
        private GameBerry.Event.RefreshGambleSynergyCombineSkillMsg _refreshGambleSynergyCombineSkillMsg = new Event.RefreshGambleSynergyCombineSkillMsg();
        private GameBerry.Event.RefreshReadyGambleSynergyCombineSkillMsg _refreshReadyGambleSynergyCombineSkillMsg = new Event.RefreshReadyGambleSynergyCombineSkillMsg();
        private GameBerry.Event.NoticeNewLobbySynergyElementMsg _noticeNewLobbySynergyElementMsg = new Event.NoticeNewLobbySynergyElementMsg();
        private GameBerry.Event.ChangeEquipSynergyMsg _changeEquipSynergyMsg = new Event.ChangeEquipSynergyMsg();

        private GameBerry.Event.ShowInterestTextMsg _showInterestTextMsg = new Event.ShowInterestTextMsg();

        private GameBerry.Event.AddSkillSynergyMsg _addSkillSynergyMsg = new Event.AddSkillSynergyMsg();

        private GameBerry.Event.ShowNewSynergyMsg _showNewSynergyMsg = new Event.ShowNewSynergyMsg();

        private GameBerry.Event.RefreshCharacterInfo_StatMsg _refreshCharacterInfo_StatMsg = new Event.RefreshCharacterInfo_StatMsg();

        //private GameBerry.Event.ShowTotalExpDetailMsg _showTotalExpDetailMsg = new Event.ShowTotalExpDetailMsg();

        // 플레이어 장착 시너지 <SlotID, ArrrSkillId>
        public Dictionary<Enum_SynergyType, List<SynergyEffectData>> _equipEnterSynergyList = new Dictionary<Enum_SynergyType, List<SynergyEffectData>>();

        private List<string> m_changeInfoUpdate = new List<string>();

        public Dictionary<Enum_SynergyType, List<SynergyEffectData>> _newSynergyDatas = new Dictionary<Enum_SynergyType, List<SynergyEffectData>>();


        // totalLevel
        private Dictionary<V2Enum_Stat, ObscuredDouble> _arrrSynergyTotalStatValues = new Dictionary<V2Enum_Stat, ObscuredDouble>();
        public Dictionary<V2Enum_Stat, ObscuredDouble> ArrrSynergyTotalStatValues { get { return _arrrSynergyTotalStatValues; } }
        public ObscuredInt SynergyUnLockLevel { get; private set; }
        public ObscuredInt GambleGradeLevel { get; private set; }
        public ObscuredBool UnLockJoker { get; private set; }

        private Dictionary<int, Sprite> _skillIcons = new Dictionary<int, Sprite>();


        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerSynergyInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);

            SynergySortView.Add(Enum_SynergyType.Red);
            SynergySortView.Add(Enum_SynergyType.Blue);
            SynergySortView.Add(Enum_SynergyType.Yellow);
            SynergySortView.Add(Enum_SynergyType.White);

            SynergyOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitSynergyContent()
        {
            string newSynergyIndex = PlayerPrefs.GetString(Define.NewSynergyKey);
            if (string.IsNullOrEmpty(newSynergyIndex) == false)
            {
                string[] arr = newSynergyIndex.Split(',');

                for (int i = 0; i < arr.Length; ++i)
                {
                    int index = arr[i].ToInt() + 107021100;
                    SynergyEffectData synergyEffectData = GetSynergyEffectData(index);
                    if (synergyEffectData == null)
                        continue;

                    DoNewSynergyNotice(synergyEffectData);
                    //SynergyContainer.NewSynergys.Add(synergyEffectData, 1);
                }
            }

            newSynergyIndex = PlayerPrefs.GetString(Define.NewSynergyBreakKey);
            if (string.IsNullOrEmpty(newSynergyIndex) == false)
            {
                string[] arr = newSynergyIndex.Split(',');

                for (int i = 0; i < arr.Length; ++i)
                {
                    int index = arr[i].ToInt() + 118010000;
                    SynergyBreakthroughData synergyEffectData = GetSynergyBreakthroughData(index);
                    if (synergyEffectData == null)
                        continue;

                    DoNewSynergyBreakthroughNotice(synergyEffectData);
                    //SynergyContainer.NewSynergys.Add(synergyEffectData, 1);
                }
            }

            foreach (var pair in GetAllGambleSynergyCombineData())
            {
                SynergyCombineData synergyCombineData = pair;
                synergyCombineData.SynergySkillData = SkillManager.Instance.GetMainSkillData(synergyCombineData.MainSkillIndex);
            }

            foreach (var pair in GetAllGambleSynergyData())
            {
                List<ObscuredInt> tempEquipIndex = null;
                if (SynergyContainer.TempSynergyEquipData.ContainsKey(pair.Key) == true)
                    tempEquipIndex = SynergyContainer.TempSynergyEquipData[pair.Key];
                else
                    tempEquipIndex = new List<ObscuredInt>();

                SynergyData synergyData = pair.Value;

                if (SynergyContainer.SynergyEquip_Dic.ContainsKey(pair.Key) == false)
                    SynergyContainer.SynergyEquip_Dic.Add(pair.Key, new Dictionary<ObscuredInt, SynergyEffectData>());

                foreach (var tierDatas in synergyData.TierDatas)
                {
                    if (SynergyContainer.SynergyEquip_Dic[pair.Key].ContainsKey(tierDatas.Key) == false)
                        SynergyContainer.SynergyEquip_Dic[pair.Key].Add(tierDatas.Key, null);

                    for (int i = 0; i < tierDatas.Value.Count; ++i)
                    {
                        SynergyEffectData synergyEffectData = tierDatas.Value[i];

                        if (tempEquipIndex.Contains(synergyEffectData.Index) == true)
                        {
                            SynergyContainer.SynergyEquip_Dic[pair.Key][tierDatas.Key] = synergyEffectData;
                            break;
                        }
                    }

                    if (SynergyContainer.SynergyEquip_Dic[pair.Key][tierDatas.Key] == null)
                    {
                        if (tierDatas.Value.Count > 0)
                        {
                            SynergyContainer.SynergyEquip_Dic[pair.Key][tierDatas.Key] = tierDatas.Value[0];
                        }
                    }

                    if (SynergyContainer.SynergyEquip_Dic[pair.Key][tierDatas.Key] != null)
                    {
                        SkillInfo skillInfo = GetSynergyEffectSkillInfo(SynergyContainer.SynergyEquip_Dic[pair.Key][tierDatas.Key]);
                        if (skillInfo == null)
                            AddNewSkillInfo(SynergyContainer.SynergyEquip_Dic[pair.Key][tierDatas.Key]);
                    }
                }
            }

            SynergyContainer.SynergyEffectCurrentTotalLevel = 0;

            Dictionary<int, Dictionary<V2Enum_Stat, double>> value = new Dictionary<int, Dictionary<V2Enum_Stat, double>>();

            foreach (var pair in GetAllGambleSynergyEffectDataList())
            {
                for (int i = 0; i < pair.Value.Count; ++i)
                {
                    SynergyEffectData synergyEffectData = pair.Value[i];

                    synergyEffectData.SynergySkillData = SkillManager.Instance.GetMainSkillData(synergyEffectData.MainSkillIndex);
                    if (synergyEffectData.SynergySkillData != null)
                        synergyEffectData.V2Enum_Grade = synergyEffectData.SynergySkillData.MainSkillGrade;

                    for (int r = 0; r < synergyEffectData.SynergyRuneList.Count; ++r)
                    {
                        SynergyBreakthroughData synergyRuneData = synergyEffectData.SynergyRuneList[r];
                        synergyRuneData.SynergySkillData = SkillManager.Instance.GetMainSkillData(synergyRuneData.MainSkillIndex);
                    }

                    SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
                    if (skillInfo != null)
                    {
                        SynergyReinforceStatData synergyReinforceStatData = GetSynergyReinforceStatData(skillInfo.Level);

                        if (synergyReinforceStatData != null)
                        {
                            foreach (var pairstat in synergyReinforceStatData.AccEffectStat)
                            {
                                if (_arrrSynergyTotalStatValues.ContainsKey(pairstat.Key) == false)
                                    _arrrSynergyTotalStatValues.Add(pairstat.Key, 0);

                                _arrrSynergyTotalStatValues[pairstat.Key] += pairstat.Value;
                            }
                        }
                    }
                }
            }

            SynergyUnLockLevel = 5;
            GambleGradeLevel = 1;
            UnLockJoker = false;

            Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.StackSkillCount);
        }
        //------------------------------------------------------------------------------------
        public bool RefreshSynergyRedDot()
        {
            List<Enum_SynergyType> needRedDotList = new List<Enum_SynergyType>();

            foreach (var pair in SynergyContainer.SynergyInfo)
            {
                SynergyEffectData synergyEffectData = GetSynergyEffectData(pair.Key);
                if (synergyEffectData == null)
                    continue;

                if (ReadySynergyEnhance(synergyEffectData) == true)
                {
                    if (needRedDotList.Contains(synergyEffectData.SynergyType) == false)
                        needRedDotList.Add(synergyEffectData.SynergyType);
                }
            }

            if (needRedDotList.Count > 0)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbySynergy);
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbySynergy_AllEnhance);
            }

            //for (int i = 0; i < needRedDotList.Count; ++i)
            //{
            //    Managers.RedDotManager.Instance.ShowRedDot(ConvertRedDotEnum(needRedDotList[i]));
            //}

            RefreshNewSynergyIcon();

            return needRedDotList.Count > 0;
        }
        //------------------------------------------------------------------------------------
        public void ResetSynergyState()
        {
            _synergySkillList.Clear();
            _synergyStack.Clear();
            _synergyLevel.Clear();
            _equipedSynergyCombineSkillList.Clear();
            _readySynergyCombineSkillList.Clear();
            _equipEnterSynergyList.Clear();
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyNextData()
        {
            _equipEnterSynergyList.Clear();

            foreach (var pair in SynergyContainer.SynergyEquip_Dic)
            {
                if (_equipEnterSynergyList.ContainsKey(pair.Key) == false)
                    _equipEnterSynergyList.Add(pair.Key, new List<SynergyEffectData>());

                foreach (var pair2 in pair.Value)
                {
                    if (pair2.Value == null)
                        continue;

                    _equipEnterSynergyList[pair.Key].Add(pair2.Value);
                }
            }

            foreach (var pair in _equipEnterSynergyList)
            {
                pair.Value.Sort((x, y) =>
                {
                    if (x.SynergyTier < y.SynergyTier)
                        return -1;
                    else if (x.SynergyTier > y.SynergyTier)
                        return 1;
                    else
                    {
                        if (x.Index < y.Index)
                            return -1;
                        else if (x.Index > y.Index)
                            return 1;
                    }

                    return 0;
                });

                SynergyEffectData prevEffect = null;

                for (int i = 0; i < pair.Value.Count; ++i)
                {
                    SynergyEffectData gambleSynergyEffectData = pair.Value[i];
                    gambleSynergyEffectData.NextEffectData = null;

                    if (prevEffect != null)
                        prevEffect.NextEffectData = gambleSynergyEffectData;

                    prevEffect = gambleSynergyEffectData;
                }
            }
        }
        //------------------------------------------------------------------------------------
        //public void ShowTotalExpDetail(ContentDetailList contentDetailList, SynergyTotalLevelEffectData synergyTotalLevelEffectData = null)
        //{
        //    _showTotalExpDetailMsg.ContentDetailList = contentDetailList;
        //    _showTotalExpDetailMsg.SynergyTotalLevelEffectData = synergyTotalLevelEffectData;
        //    Message.Send(_showTotalExpDetailMsg);

        //    UI.IDialog.RequestDialogEnter<UI.LobbyTotalExpDetailViewDialog>();
        //}
        //------------------------------------------------------------------------------------
        #region SynergyData
        //------------------------------------------------------------------------------------
        public Dictionary<Enum_SynergyType, SynergyData> GetAllGambleSynergyData()
        {
            return SynergyOperator.GetAllGambleSynergyData();
        }
        //------------------------------------------------------------------------------------
        public SynergyData GetGambleSynergyData(Enum_SynergyType Enum_GambleSynergyType)
        {
            return SynergyOperator.GetGambleSynergyData(Enum_GambleSynergyType);
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, SynergyEffectData> GetAllSynergyEffectDatas()
        {
            return SynergyOperator.GetAllSynergyEffectDatas();
        }
        //------------------------------------------------------------------------------------
        public SynergyEffectData GetSynergyEffectData(ObscuredInt index)
        {
            return SynergyOperator.GetSynergyEffectData(index);
        }
        //------------------------------------------------------------------------------------
        public Dictionary<Enum_SynergyType, List<SynergyEffectData>> GetAllGambleSynergyEffectDataList()
        {
            return SynergyOperator.GetAllGambleSynergyEffectDataList();
        }
        //------------------------------------------------------------------------------------
        public List<SynergyEffectData> GetGambleSynergyEffectDataList(Enum_SynergyType Enum_GambleSynergyType)
        {
            return SynergyOperator.GetGambleSynergyEffectDataList(Enum_GambleSynergyType);
        }
        //------------------------------------------------------------------------------------
        public List<SynergyEffectData> GetInGameEquipSynergyEffectData(Enum_SynergyType Enum_GambleSynergyType)
        {
            if (_equipEnterSynergyList.ContainsKey(Enum_GambleSynergyType) == true)
                return _equipEnterSynergyList[Enum_GambleSynergyType];

            return null;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAccumLevel(Enum_SynergyType Enum_GambleSynergyType)
        {
            List<SynergyEffectData> SynergyEffectDatas = GetGambleSynergyEffectDataList(Enum_GambleSynergyType);

            int accumLevel = 0;

            for (int i = 0; i < SynergyEffectDatas.Count; ++i)
            {
                SkillInfo skillInfo = GetSynergyEffectSkillInfo(SynergyEffectDatas[i]);
                accumLevel += skillInfo.Level;
            }

            return accumLevel;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAccumBreakLevel(Enum_SynergyType Enum_GambleSynergyType)
        {
            List<SynergyEffectData> SynergyEffectDatas = GetGambleSynergyEffectDataList(Enum_GambleSynergyType);

            int accumLevel = 0;

            for (int i = 0; i < SynergyEffectDatas.Count; ++i)
            {
                SkillInfo skillInfo = GetSynergyEffectSkillInfo(SynergyEffectDatas[i]);
                accumLevel += skillInfo.LimitCompleteLevel;
            }

            return accumLevel;
        }
        //------------------------------------------------------------------------------------
        public SynergyLevelUpCostData GetSynergyLevelUpCostData(V2Enum_Grade v2Enum_Grade)
        {
            return SynergyOperator.GetSynergyLevelUpCostData(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public List<SynergyCombineData> GetAllGambleSynergyCombineData()
        {
            return SynergyOperator.GetAllGambleSynergyCombineData();
        }
        //------------------------------------------------------------------------------------
        public int SortGambleSynergyCombineDatas(SynergyCombineData x, SynergyCombineData y)
        {
            if (IsAlReadySynergyCombineSkill(x) == false && IsAlReadySynergyCombineSkill(y) == true)
                return -1;
            else if (IsAlReadySynergyCombineSkill(x) == true && IsAlReadySynergyCombineSkill(y) == false)
                return 1;

            if (GetSynergyCombineSkillReadyRatio(x) > GetSynergyCombineSkillReadyRatio(y))
                return -1;
            else if (GetSynergyCombineSkillReadyRatio(x) < GetSynergyCombineSkillReadyRatio(y))
                return 1;
            else
            {
                if (x.Index < y.Index)
                    return -1;
                else if (x.Index > y.Index)
                    return 1;
            }

            return 0;
        }
        //------------------------------------------------------------------------------------
        public SynergyBreakthroughCostData GetSynergyLevelUpLimitData(Enum_SynergyType Enum_GambleSynergyType, ObscuredInt level)
        {
            return SynergyOperator.GetSynergyLevelUpLimitData(Enum_GambleSynergyType, level + 1);
        }
        //------------------------------------------------------------------------------------
        //public Dictionary<ObscuredInt, SynergyLevelUpLimitData> GetAllSynergyGradeLevelUpLimitData(Enum_SynergyType Enum_GambleSynergyType, V2Enum_Grade v2Enum_Grade)
        //{
        //    return SynergyOperator.GetAllSynergyGradeLevelUpLimitData(Enum_GambleSynergyType, v2Enum_Grade);
        //}
        ////------------------------------------------------------------------------------------
        public SynergyDuplicationData GetSynergyDuplicationData(Enum_SynergyType Enum_GambleSynergyType, V2Enum_Grade v2Enum_Grade)
        {
            return SynergyOperator.GetSynergyDuplicationData(Enum_GambleSynergyType, v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public SynergyReinforceStatData GetSynergyReinforceStatData(ObscuredInt level)
        {
            return SynergyOperator.GetSynergyReinforceStatData(level);
        }
        //------------------------------------------------------------------------------------
        public SkillInfo GetSynergyEffectSkillInfo(int index)
        {
            return GetSynergyEffectSkillInfo(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        public SkillInfo GetSynergyEffectSkillInfo(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return null;

            if (SynergyContainer.SynergyInfo.ContainsKey(synergyEffectData.Index) == true)
                return SynergyContainer.SynergyInfo[synergyEffectData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SkillInfo AddNewSkillInfo(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return null;

            SkillInfo skillInfo = new SkillInfo();
            skillInfo.Id = synergyEffectData.Index;
            skillInfo.Level = Define.PlayerSynergyDefaultLevel;
            skillInfo.Count = 0;

            SynergyContainer.SynergyInfo.Add(skillInfo.Id, skillInfo);


            return skillInfo;
        }
        //------------------------------------------------------------------------------------
        private void DoNewSynergyNotice(SynergyEffectData synergyEffectData, bool showNewIcon = true)
        {
            Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.StackSkillCount);
            Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.GetSynergySkill);
            Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.StackSkillCount);
            Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.GetSynergySkill);
            if (showNewIcon == true)
                AddNewSynergyIcon(synergyEffectData);
        }
        //------------------------------------------------------------------------------------
        public SynergyEffectData GetEquipSynergyEffect(Enum_SynergyType Enum_SynergyType, ObscuredInt tier)
        {
            if (SynergyContainer.SynergyEquip_Dic.ContainsKey(Enum_SynergyType) == false)
                return null;

            if (SynergyContainer.SynergyEquip_Dic[Enum_SynergyType].ContainsKey(tier) == false)
                return null;

            return SynergyContainer.SynergyEquip_Dic[Enum_SynergyType][tier];
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetSynergyGrade(int index)
        {
            return GetSynergyGrade(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetSynergyGrade(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return V2Enum_Grade.Max;

            if (synergyEffectData.SynergySkillData == null)
                return V2Enum_Grade.Max;

            return synergyEffectData.V2Enum_Grade;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSynergySprite(int index)
        {
            return GetSynergySprite(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSynergySprite(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return null;

            if (synergyEffectData.SynergySkillData == null)
                return null;

            return SkillManager.Instance.GetMainSkillIcon(synergyEffectData.SynergySkillData);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyAmount(int index, int amount)
        {
            SetSynergyAmount(GetSynergyEffectData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyAmount(SynergyEffectData synergyEffectData, int amount)
        {
            if (synergyEffectData == null)
                return;

            SkillInfo playerSkillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (playerSkillInfo == null)
            {
                playerSkillInfo = AddNewSkillInfo(synergyEffectData);

                DoNewSynergyNotice(synergyEffectData);
            }

            playerSkillInfo.Count = amount;

            if (ReadySynergyEnhance(synergyEffectData) == true)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbySynergy);
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbySynergy_AllEnhance);
                //Managers.RedDotManager.Instance.ShowRedDot(ConvertRedDotEnum(synergyEffectData.SynergyType));
            }
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAmount(int index)
        {
            return GetSynergyAmount(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAmount(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return 0;

            SkillInfo playerSkillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (playerSkillInfo == null)
                return 0;
            else
                return playerSkillInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public int AddSynergyAmount(int index, int amount)
        {
            return AddSynergyAmount(GetSynergyEffectData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int AddSynergyAmount(SynergyEffectData synergyEffectData, int amount)
        {
            if (synergyEffectData == null)
                return 0;

            SkillInfo playerSkillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (playerSkillInfo == null)
            {
                playerSkillInfo = AddNewSkillInfo(synergyEffectData);

                DoNewSynergyNotice(synergyEffectData);
            }

            playerSkillInfo.Count += amount;

            if (ReadySynergyEnhance(synergyEffectData) == true)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbySynergy);
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbySynergy_AllEnhance);
            }

            return playerSkillInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public int AddSynergyAmount(RewardData rewardData)
        {
            if (rewardData == null)
                return 0;

            SynergyEffectData synergyEffectData = GetSynergyEffectData(rewardData.Index);
            if (synergyEffectData == null)
                return 0;

            int DupliCount = 0;

            SkillInfo playerSkillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (playerSkillInfo == null)
            {
                playerSkillInfo = AddNewSkillInfo(synergyEffectData);
                playerSkillInfo.Count = 1;
                DoNewSynergyNotice(synergyEffectData);
                DupliCount = (int)rewardData.Amount - 1;
            }
            else
            {
                DupliCount = (int)rewardData.Amount;
            }

            if (DupliCount > 0)
            {
                SynergyDuplicationData synergyDuplicationData = GetSynergyDuplicationData(synergyEffectData.SynergyType, synergyEffectData.V2Enum_Grade);
                if (synergyDuplicationData != null)
                {
                    rewardData.DupliIndex = synergyDuplicationData.DuplicationGoodsIndex;
                    rewardData.DupliAmount = synergyDuplicationData.DuplicationGoodsValue * DupliCount;

                    GoodsManager.Instance.AddGoodsAmount(rewardData.DupliIndex, rewardData.DupliAmount);
                }
            }

            if (ReadySynergyEnhance(synergyEffectData) == true)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbySynergy);
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbySynergy_AllEnhance);
            }

            return 1;
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyAmount(int index, int amount)
        {
            return UseSynergyAmount(GetSynergyEffectData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyAmount(SynergyEffectData synergyEffectData, int amount)
        {
            if (synergyEffectData == null)
                return 0;

            SkillInfo playerSkillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (playerSkillInfo == null)
                return 0;

            playerSkillInfo.Count -= amount;

            if (playerSkillInfo.Count < 0)
                playerSkillInfo.Count = 0;

            return playerSkillInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public string GetSynergyLocalKey(int index)
        {
            SynergyEffectData synergyEffectData = GetSynergyEffectData(index);
            if (synergyEffectData != null)
            {
                if (synergyEffectData.SynergySkillData != null)
                    return synergyEffectData.SynergySkillData.NameLocalKey;
            }

            return string.Empty;
        }
        //------------------------------------------------------------------------------------
        public void AddNewSynergyIcon(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData != null)
            {
                if (SynergyContainer.NewSynergys.ContainsKey(synergyEffectData) == false)
                    SynergyContainer.NewSynergys.Add(synergyEffectData, 1);
                else
                    SynergyContainer.NewSynergys[synergyEffectData] += 1;

                PlayerPrefs.SetString(Define.NewSynergyKey, SynergyContainer.GetNewSynergySerializeString());

                if (_newSynergyDatas.ContainsKey(synergyEffectData.SynergyType) == false)
                    _newSynergyDatas.Add(synergyEffectData.SynergyType, new List<SynergyEffectData>());

                _newSynergyDatas[synergyEffectData.SynergyType].Add(synergyEffectData);
            }

            _showNewSynergyMsg.NewSynergyEffectData = synergyEffectData;
            Message.Send(_showNewSynergyMsg);
        }
        //------------------------------------------------------------------------------------
        public void RefreshNewSynergyIcon()
        {
            _showNewSynergyMsg.NewSynergyEffectData = null;
            Message.Send(_showNewSynergyMsg);
        }
        //------------------------------------------------------------------------------------
        public int GetNewSynergyIconCount()
        {
            int count = SynergyContainer.NewSynergys.Count;
            return count;
        }
        //------------------------------------------------------------------------------------
        public bool IsNewSynergyIcon(SynergyEffectData synergyEffectData)
        {
            return SynergyContainer.NewSynergys.ContainsKey(synergyEffectData);
        }
        //------------------------------------------------------------------------------------
        public int GetNewSynergyIconCount(Enum_SynergyType Enum_SynergyType)
        {
            if (_newSynergyDatas.ContainsKey(Enum_SynergyType) == true)
                return _newSynergyDatas[Enum_SynergyType].Count;

            return 0;
        }
        //------------------------------------------------------------------------------------
        public void RemoveNewIconSynergy(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData != null)
            {
                if (SynergyContainer.NewSynergys.ContainsKey(synergyEffectData) == true)
                    SynergyContainer.NewSynergys.Remove(synergyEffectData);

                PlayerPrefs.SetString(Define.NewSynergyKey, SynergyContainer.GetNewSynergySerializeString());

                if (_newSynergyDatas.ContainsKey(synergyEffectData.SynergyType) == true)
                    _newSynergyDatas[synergyEffectData.SynergyType].Remove(synergyEffectData);
            }
        }
        //------------------------------------------------------------------------------------
        public ContentDetailList ConvertRedDotEnum(Enum_SynergyType Enum_SynergyType)
        {
            switch (Enum_SynergyType)
            {
                case Enum_SynergyType.Red:
                    {
                        return ContentDetailList.LobbySynergy_Red;
                    }
                case Enum_SynergyType.Yellow:
                    {
                        return ContentDetailList.LobbySynergy_Yellow;
                    }
                case Enum_SynergyType.Blue:
                    {
                        return ContentDetailList.LobbySynergy_Blue;
                    }
                case Enum_SynergyType.White:
                    {
                        return ContentDetailList.LobbySynergy_White;
                    }
            }

            return ContentDetailList.LobbySynergy;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region SynergyBreakthroughData
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, SynergyBreakthroughData> GetAllSynergyBreakthroughData()
        {
            return SynergyOperator.GetAllSynergyRuneData();
        }
        //------------------------------------------------------------------------------------
        public SynergyBreakthroughData GetSynergyBreakthroughData(ObscuredInt index)
        {
            return SynergyOperator.GetSynergyRuneData(index);
        }
        //------------------------------------------------------------------------------------
        public SynergyDuplicationData GetSynergyBreakthroughDuplicationData(V2Enum_Grade v2Enum_Grade)
        {
            return SynergyOperator.GetSynergyRuneDuplicationData(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------

        public bool IsGetedBreakthrough(SynergyBreakthroughData SynergyRuneData)
        {
            if (SynergyRuneData == null)
                return false;

            SynergyEffectData synergyEffectData = GetSynergyEffectData(SynergyRuneData.SynergySkillIndex);
            if (synergyEffectData == null)
                return false;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return false;

            return skillInfo.LimitCompleteLevel >= SynergyRuneData.Procedure;
        }
        //------------------------------------------------------------------------------------
        private void DoNewSynergyBreakthroughNotice(SynergyBreakthroughData SynergyRuneData, bool showNewIcon = true)
        {
            //Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.StackSkillCount);
            //Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.GetSynergySkill);
            //Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.StackSkillCount);
            //Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.GetSynergySkill);
            if (showNewIcon == true)
                AddNewSynergyBreakthroughIcon(SynergyRuneData);
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetSynergyBreakthroughGrade(int index)
        {
            return GetSynergyBreakthroughGrade(GetSynergyBreakthroughData(index));
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetSynergyBreakthroughGrade(SynergyBreakthroughData SynergyRuneData)
        {
            if (SynergyRuneData == null)
                return V2Enum_Grade.Max;

            if (SynergyRuneData.SynergySkillData == null)
                return V2Enum_Grade.Max;

            return SynergyRuneData.Grade;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSynergyBreakthroughSprite(int index)
        {
            return GetSynergyBreakthroughSprite(GetSynergyBreakthroughData(index));
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSynergyBreakthroughSprite(SynergyBreakthroughData SynergyRuneData)
        {
            Sprite sp = null;

            if (_skillIcons.ContainsKey(SynergyRuneData.IconIndex) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(Define.SynergyBreakPath, SynergyRuneData.IconIndex), o =>
                {
                    sp = o as Sprite;
                    _skillIcons.Add(SynergyRuneData.IconIndex, sp);
                });
            }
            else
                sp = _skillIcons[SynergyRuneData.IconIndex];

            return sp;
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyBreakthroughAmount(int index, int amount)
        {
            SetSynergyBreakthroughAmount(GetSynergyBreakthroughData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyBreakthroughAmount(SynergyBreakthroughData SynergyRuneData, int amount)
        {
            if (SynergyRuneData == null)
                return;

            if (SynergyContainer.Runes.Contains(SynergyRuneData.Index) == false)
            {
                SynergyContainer.Runes.Add(SynergyRuneData.Index);
                DoNewSynergyBreakthroughNotice(SynergyRuneData);
                ARRRStatManager.Instance.RefreshBattlePower();
            }
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyBreakthroughAmount(int index)
        {
            return GetSynergyBreakthroughAmount(GetSynergyBreakthroughData(index));
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyBreakthroughAmount(SynergyBreakthroughData SynergyRuneData)
        {
            if (SynergyRuneData == null)
                return 0;

            return SynergyContainer.Runes.Contains(SynergyRuneData.Index) == false ? 0 : 1;
        }
        //------------------------------------------------------------------------------------
        public int AddSynergyBreakthroughAmount(int index, int amount)
        {
            return AddSynergyBreakthroughAmount(GetSynergyBreakthroughData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int AddSynergyBreakthroughAmount(SynergyBreakthroughData SynergyRuneData, int amount)
        {
            if (SynergyRuneData == null)
                return 0;

            if (SynergyContainer.Runes.Contains(SynergyRuneData.Index) == false)
            {
                SynergyContainer.Runes.Add(SynergyRuneData.Index);
                DoNewSynergyBreakthroughNotice(SynergyRuneData);
                ARRRStatManager.Instance.RefreshBattlePower();
            }

            return 1;
        }
        //------------------------------------------------------------------------------------
        public int AddSynergyBreakthroughAmount(RewardData rewardData)
        {
            if (rewardData == null)
                return 0;

            SynergyBreakthroughData SynergyRuneData = GetSynergyBreakthroughData(rewardData.Index);
            if (SynergyRuneData == null)
                return 0;

            int DupliCount = 0;


            if (SynergyContainer.Runes.Contains(SynergyRuneData.Index) == false)
            {
                SynergyContainer.Runes.Add(SynergyRuneData.Index);
                DupliCount = (int)rewardData.Amount - 1;

                ARRRStatManager.Instance.RefreshBattlePower();
            }
            else
            {
                DupliCount = (int)rewardData.Amount;
            }

            if (DupliCount > 0)
            {
                SynergyDuplicationData synergyDuplicationData = GetSynergyBreakthroughDuplicationData(SynergyRuneData.Grade);
                if (synergyDuplicationData != null)
                {
                    rewardData.DupliIndex = synergyDuplicationData.DuplicationGoodsIndex;
                    rewardData.DupliAmount = synergyDuplicationData.DuplicationGoodsValue * DupliCount;

                    GoodsManager.Instance.AddGoodsAmount(rewardData.DupliIndex, rewardData.DupliAmount);
                }
            }

            return 1;
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyBreakthroughAmount(int index, int amount)
        {
            return 1;
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyBreakthroughAmount(SynergyBreakthroughData SynergyRuneData, int amount)
        {
            return 1;
        }
        //------------------------------------------------------------------------------------
        public string GetSynergyBreakthroughLocalKey(int index)
        {
            SynergyBreakthroughData SynergyRuneData = GetSynergyBreakthroughData(index);
            if (SynergyRuneData != null)
            {
                return SynergyRuneData.NameLocalKey;
            }

            return string.Empty;
        }
        //------------------------------------------------------------------------------------
        public void AddNewSynergyBreakthroughIcon(SynergyBreakthroughData SynergyRuneData)
        {
            if (SynergyRuneData != null)
            {
                if (SynergyContainer.NewRunes.ContainsKey(SynergyRuneData) == false)
                    SynergyContainer.NewRunes.Add(SynergyRuneData, 1);
                else
                    SynergyContainer.NewRunes[SynergyRuneData] += 1;

                PlayerPrefs.SetString(Define.NewSynergyBreakKey, SynergyContainer.GetNewRuneSerializeString());

                SynergyEffectData synergyEffectData = GetSynergyEffectData(SynergyRuneData.SynergySkillIndex);
                if (synergyEffectData != null)
                    AddNewSynergyIcon(synergyEffectData);
            }
        }
        //------------------------------------------------------------------------------------
        public int GetNewSynergyBreakthroughIconCount()
        {
            int count = SynergyContainer.NewRunes.Count;
            return count;
        }
        //------------------------------------------------------------------------------------
        public bool IsNewSynergyBreakthroughIcon(SynergyBreakthroughData SynergyRuneData)
        {
            return SynergyContainer.NewRunes.ContainsKey(SynergyRuneData);
        }
        //------------------------------------------------------------------------------------
        public void RemoveNewIconBreakthroughSynergy(SynergyBreakthroughData SynergyRuneData)
        {
            if (SynergyRuneData != null)
            {
                if (SynergyContainer.NewRunes.ContainsKey(SynergyRuneData) == true)
                    SynergyContainer.NewRunes.Remove(SynergyRuneData);

                PlayerPrefs.SetString(Define.NewSynergyBreakKey, SynergyContainer.GetNewSynergySerializeString());
            }
        }
        //------------------------------------------------------------------------------------
        public void BuySynergyBreakthrough(SynergyEffectData synergyEffectData)
        { 

        }
        //------------------------------------------------------------------------------------
        public int GetSynergyBreakthroughCount(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return 0;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return 0;
            return skillInfo.LimitCompleteLevel;
        }
        //------------------------------------------------------------------------------------
        public bool NeedLimitBreak(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return false;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return false;

            SynergyBreakthroughCostData synergyLevelUpLimitData = GetSynergyLevelUpLimitData(synergyEffectData.SynergyType, skillInfo.LimitCompleteLevel);

            if (synergyLevelUpLimitData == null)
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool ReadyCharacterLimitLevelUpCost(SynergyBreakthroughCostData characterLevelUpCost)
        {
            double amount = GoodsManager.Instance.GetGoodsAmount(characterLevelUpCost.LimitBreakCostGoodsIndex);

            double need = characterLevelUpCost.LimitBreakCostGoodsValue;

            return amount >= need;
        }
        //------------------------------------------------------------------------------------
        public bool DoARRRLimitUp(SynergyEffectData synergyEffectData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.SynergyBreakthrough) == false)
                return false;

            bool lockBreak = false;

            StageInfo stageInfo = Managers.MapManager.Instance.GetStageInfo(Define.SynergyBreakTutorialStage);

            if (stageInfo != null)
            {
                lockBreak = stageInfo.RecvClearReward < Define.SynergyBreakTutorialWave;
            }

            if (lockBreak == true)
                return false;

            if (NeedLimitBreak(synergyEffectData) == false)
                return false;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return false;


            SynergyBreakthroughCostData synergyLevelUpLimitData = GetSynergyLevelUpLimitData(synergyEffectData.SynergyType, skillInfo.LimitCompleteLevel);
            if (synergyLevelUpLimitData == null)
                return false;

            if (ReadyCharacterLimitLevelUpCost(synergyLevelUpLimitData) == false)
            {
                Contents.GlobalContent.ShowPopup_OkCancel(
                Managers.LocalStringManager.Instance.GetLocalString("common/ui/shortagegoodstitle"),
                Managers.LocalStringManager.Instance.GetLocalString("common/ui/shortagegoodsdesc"),
                () =>
                {
                    UI.IDialog.RequestDialogExit<UI.LobbySynergyContentDialog>();
                    Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.ShopDailyWeek_MonthPackage);
                },
                null);
                return false;
            }

            List<int> used_type = new List<int>();
            List<double> former_quan = new List<double>();
            List<double> used_quan = new List<double>();
            List<double> keep_quan = new List<double>();

            double needAmount = synergyLevelUpLimitData.LimitBreakCostGoodsValue;

            used_type.Add(synergyLevelUpLimitData.LimitBreakCostGoodsIndex);
            former_quan.Add(GoodsManager.Instance.GetGoodsAmount(synergyLevelUpLimitData.LimitBreakCostGoodsIndex));
            used_quan.Add(needAmount);

            GoodsManager.Instance.UseGoodsAmount(synergyLevelUpLimitData.LimitBreakCostGoodsIndex, needAmount);

            keep_quan.Add(GoodsManager.Instance.GetGoodsAmount(synergyLevelUpLimitData.LimitBreakCostGoodsIndex));

            skillInfo.LimitCompleteLevel += 1;

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_log_skill_limitup(synergyEffectData.Index, skillInfo.LimitCompleteLevel,
                used_type, former_quan, used_quan, keep_quan);


            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region LobbySynergySetting
        //------------------------------------------------------------------------------------
        public bool IsLockSynergy(SynergyEffectData synergyEffectData)
        {
            if(synergyEffectData == null)
                return true;

            SynergyConditionData synergyConditionData = synergyEffectData.SynergyConditionData;
            if (synergyConditionData == null)
                return true;

            if (synergyConditionData.RequiredTier1Reinforce <= 0
                && synergyConditionData.RequiredTier2Reinforce <= 0
                && synergyConditionData.RequiredTier3Reinforce <= 0
                && synergyConditionData.RequiredTier4Reinforce <= 0)
                return false;

            if (IsOverLevel(synergyEffectData.SynergyType, 1, synergyConditionData.RequiredTier1Reinforce) == false)
                return true;

            if (IsOverLevel(synergyEffectData.SynergyType, 2, synergyConditionData.RequiredTier2Reinforce) == false)
                return true;

            if (IsOverLevel(synergyEffectData.SynergyType, 3, synergyConditionData.RequiredTier3Reinforce) == false)
                return true;

            if (IsOverLevel(synergyEffectData.SynergyType, 4, synergyConditionData.RequiredTier4Reinforce) == false)
                return true;

            return false;
        }
        //------------------------------------------------------------------------------------
        public bool IsOverLevel(Enum_SynergyType Enum_SynergyType, int tier, int targetLevel)
        {
            if (targetLevel <= 0)
                return true;

            SynergyEffectData synergyEffectData = GetEquipSynergyEffect(Enum_SynergyType, tier);
            if (synergyEffectData == null)
                return false;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return false;

            if (skillInfo.Level < targetLevel)
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
        public void ShowNoticeSynergyUnLock(SynergyEffectData synergyEffectData)
        {
            SynergyConditionData synergyConditionData = synergyEffectData.SynergyConditionData;
            if (synergyConditionData == null)
                return;

            SynergyContainer.SerializeString.Clear();

            SynergyContainer.SerializeString.Append(LocalStringManager.Instance.GetLocalString("synergy/breakthroughlock_skill"));

            if (IsOverLevel(synergyEffectData.SynergyType, 1, synergyConditionData.RequiredTier1Reinforce) == false)
            {
                SynergyEffectData target = GetEquipSynergyEffect(synergyEffectData.SynergyType, 1);

                if (SynergyContainer.SerializeString.Length > 0)
                    SynergyContainer.SerializeString.Append("\n");

                SynergyContainer.SerializeString.Append(GetNeedLevelString(target, synergyConditionData.RequiredTier1Reinforce));
            }

            if (IsOverLevel(synergyEffectData.SynergyType, 2, synergyConditionData.RequiredTier2Reinforce) == false)
            {
                SynergyEffectData target = GetEquipSynergyEffect(synergyEffectData.SynergyType, 2);

                if (SynergyContainer.SerializeString.Length > 0)
                    SynergyContainer.SerializeString.Append("\n");

                SynergyContainer.SerializeString.Append(GetNeedLevelString(target, synergyConditionData.RequiredTier2Reinforce));
            }

            if (IsOverLevel(synergyEffectData.SynergyType, 3, synergyConditionData.RequiredTier3Reinforce) == false)
            {
                SynergyEffectData target = GetEquipSynergyEffect(synergyEffectData.SynergyType, 3);

                if (SynergyContainer.SerializeString.Length > 0)
                    SynergyContainer.SerializeString.Append("\n");

                SynergyContainer.SerializeString.Append(GetNeedLevelString(target, synergyConditionData.RequiredTier3Reinforce));
            }

            if (IsOverLevel(synergyEffectData.SynergyType, 4, synergyConditionData.RequiredTier4Reinforce) == false)
            {
                SynergyEffectData target = GetEquipSynergyEffect(synergyEffectData.SynergyType, 4);

                if (SynergyContainer.SerializeString.Length > 0)
                    SynergyContainer.SerializeString.Append("\n");

                SynergyContainer.SerializeString.Append(GetNeedLevelString(target, synergyConditionData.RequiredTier4Reinforce));
            }

            Contents.GlobalContent.ShowGlobalNotice(SynergyContainer.SerializeString.ToString());
        }
        //------------------------------------------------------------------------------------
        public string GetNeedLevelString(SynergyEffectData synergyEffectData, int level)
        {
            if (synergyEffectData != null)
            {
                return string.Format("{0} : Lv.{1}",
                    LocalStringManager.Instance.GetLocalString(GetSynergyLocalKey(synergyEffectData.Index)),
                    level);
            }

            return string.Empty;
        }
        //------------------------------------------------------------------------------------
        public void ChangeEquipSynergy(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return;

            SynergyEffectData beforeData = GetEquipSynergyEffect(synergyEffectData.SynergyType, synergyEffectData.SynergyTier);

            if (SynergyContainer.SynergyEquip_Dic.ContainsKey(synergyEffectData.SynergyType) == false)
                return;

            if (SynergyContainer.SynergyEquip_Dic[synergyEffectData.SynergyType].ContainsKey(synergyEffectData.SynergyTier) == false)
                return;

            SynergyContainer.SynergyEquip_Dic[synergyEffectData.SynergyType][synergyEffectData.SynergyTier] = synergyEffectData;

            _changeEquipSynergyMsg.BeforeEquipSynergy = beforeData;
            _changeEquipSynergyMsg.AfterEquipSynergy = synergyEffectData;

            Message.Send(_changeEquipSynergyMsg);

            GameBerry.Managers.QuestManager.Instance.AddMissionCount(GameBerry.V2Enum_QuestGoalType.SynergyChange, 1);

            StageInfo stageInfo = MapManager.Instance.GetLastChellengeInfo();
            if (stageInfo != null)
            {
                ThirdPartyLog.Instance.SendLog_SynergyChange(stageInfo.StageNumber, stageInfo.LastClearWave);
            }

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxLevelSynergy(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return false;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return false;

            SynergyLevelUpCostData synergyLevelUpCostData = GetSynergyLevelUpCostData(synergyEffectData.V2Enum_Grade);

            if (synergyLevelUpCostData == null)
                return false;

            return synergyLevelUpCostData.MaximumLevel <= skillInfo.Level;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhance_NeedCount1(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return 99999;

            SynergyLevelUpCostData synergyLevelUpCostData = GetSynergyLevelUpCostData(synergyEffectData.V2Enum_Grade);

            if (synergyLevelUpCostData == null)
                return 99999;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);

            int level = skillInfo == null ? 1 : skillInfo.Level;

            level -= 1;

            return synergyLevelUpCostData.SkillLevelUpCostGoodsParam11 + (level * synergyLevelUpCostData.SkillLevelUpCostGoodsParam12);
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhanceCostGoodsIndex1(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return -1;

            SynergyLevelUpCostData synergyLevelUpCostData = GetSynergyLevelUpCostData(synergyEffectData.V2Enum_Grade);

            if (synergyLevelUpCostData == null)
                return -1;

            return synergyLevelUpCostData.SkillLevelUpCostGoodsIndex1;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhance_NeedCount2(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return 99999;

            SynergyLevelUpCostData synergyLevelUpCostData = GetSynergyLevelUpCostData(synergyEffectData.V2Enum_Grade);

            if (synergyLevelUpCostData == null)
                return 99999;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);

            int level = skillInfo == null ? 1 : skillInfo.Level;

            level -= 1;

            return synergyLevelUpCostData.SkillLevelUpCostGoodsParam21 + (level * synergyLevelUpCostData.SkillLevelUpCostGoodsParam22);
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhanceCostGoodsIndex2(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null)
                return -1;

            SynergyLevelUpCostData synergyLevelUpCostData = GetSynergyLevelUpCostData(synergyEffectData.V2Enum_Grade);

            if (synergyLevelUpCostData == null)
                return -1;

            return synergyLevelUpCostData.SkillLevelUpCostGoodsIndex2;
        }
        //------------------------------------------------------------------------------------
        public bool ReadySynergyEnhance(SynergyEffectData synergyEffectData)
        {
            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);

            if (skillInfo == null)
                return false;

            int costIndex = GetSynergyEnhanceCostGoodsIndex1(synergyEffectData);
            int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

            if (currentCount < GetSynergyEnhance_NeedCount1(synergyEffectData))
                return false;

            costIndex = GetSynergyEnhanceCostGoodsIndex2(synergyEffectData);
            currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

            if (currentCount < GetSynergyEnhance_NeedCount2(synergyEffectData))
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool EnhanceSynergy(SynergyEffectData synergyEffectData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (IsMaxLevelSynergy(synergyEffectData) == true)
                return false;

            if (ReadySynergyEnhance(synergyEffectData) == false)
            {
                Contents.GlobalContent.ShowPopup_OkCancel(
Managers.LocalStringManager.Instance.GetLocalString("common/ui/shortagegoodstitle"),
Managers.LocalStringManager.Instance.GetLocalString("common/ui/shortagegoodsdesc"),
() =>
{
    UI.IDialog.RequestDialogExit<UI.LobbySynergyContentDialog>();
    Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.ShopSummon_Normal);
},
null);
                return false;
            }

            if (IsLockSynergy(synergyEffectData) == true)
                return false;

            List<int> used_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> used_quan = new List<double>();
            List<double> after_quan = new List<double>();

            int costIndex = GetSynergyEnhanceCostGoodsIndex1(synergyEffectData);
            used_type.Add(costIndex);

            int useCost = GetSynergyEnhance_NeedCount1(synergyEffectData);

            before_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(costIndex));
            used_quan.Add(useCost);
            Managers.GoodsManager.Instance.UseGoodsAmount(costIndex, useCost);
            after_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(costIndex));


            costIndex = GetSynergyEnhanceCostGoodsIndex2(synergyEffectData);
            used_type.Add(costIndex);

            useCost = GetSynergyEnhance_NeedCount2(synergyEffectData);

            before_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(costIndex));
            used_quan.Add(useCost);
            Managers.GoodsManager.Instance.UseGoodsAmount(costIndex, useCost);
            after_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(costIndex));


            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            skillInfo.Level += 1;

            SynergyReinforceStatData synergyReinforceStatData = GetSynergyReinforceStatData(skillInfo.Level);

            if (synergyReinforceStatData != null)
            {
                for (int stat = 0; stat < synergyReinforceStatData.EffectParam_Stat.Count; ++stat)
                {
                    CreatureBaseStatElement effectstat = synergyReinforceStatData.EffectParam_Stat[stat];

                    if (_arrrSynergyTotalStatValues.ContainsKey(effectstat.BaseStat) == false)
                        _arrrSynergyTotalStatValues.Add(effectstat.BaseStat, 0);

                    _arrrSynergyTotalStatValues[effectstat.BaseStat] += effectstat.BaseValue;
                }
            }

            SynergyContainer.SynergyEffectCurrentTotalLevel += 1;

            if (SynergyContainer.SynergyEffectAccumLevel < SynergyContainer.SynergyEffectCurrentTotalLevel)
                SynergyContainer.SynergyEffectAccumLevel = SynergyContainer.SynergyEffectCurrentTotalLevel;

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_log_skill_enforce(synergyEffectData.Index, skillInfo.Level,
                used_type, before_quan, used_quan, after_quan,
                SynergyContainer.SynergyContentExp);

            PassManager.Instance.CheckPassType(V2Enum_PassType.SkillLevel);
            Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.SynergySkillLevelStack);
            Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.Synergy4GradeSkillCount);

            BattleSceneManager.Instance.RefreshMyARRRStat();
            Message.Send(_refreshCharacterInfo_StatMsg);
            ARRRStatManager.Instance.RefreshBattlePower();

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool DoSynergyLevelReset(Enum_SynergyType Enum_SynergyType)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            bool doReset = false;

            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            List<int> idx = new List<int>();

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            string equipedSynergy = SynergyContainer.GetSynergyInfoSerializeString(Enum_SynergyType);


            List<SynergyEffectData> synergyEffectDataList = GetGambleSynergyEffectDataList(Enum_SynergyType);

            int minusLevel = 0;

            for (int i = 0; i < synergyEffectDataList.Count; ++i)
            {
                SynergyEffectData synergyEffectData = synergyEffectDataList[i];
                SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
                if (skillInfo == null)
                    continue;

                if (skillInfo.Level == Define.PlayerSynergyDefaultLevel)
                    continue;

                SynergyLevelUpCostData synergyLevelUpCostData = GetSynergyLevelUpCostData(synergyEffectData.V2Enum_Grade);
                if (synergyLevelUpCostData == null)
                    continue;


                int costIndex = GetSynergyEnhanceCostGoodsIndex1(synergyEffectData);

                RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == costIndex);
                if (rewardData == null)
                {
                    rewardData = RewardManager.Instance.GetRewardData();
                    rewardData.V2Enum_Goods = GoodsManager.Instance.GetGoodsType(costIndex);
                    rewardData.Index = costIndex;
                    rewardData.Amount = 0;
                    m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                }

                rewardData.Amount += GetTotalCostToLevel(skillInfo.Level, synergyLevelUpCostData.SkillLevelUpCostGoodsParam11, synergyLevelUpCostData.SkillLevelUpCostGoodsParam12);


                costIndex = GetSynergyEnhanceCostGoodsIndex2(synergyEffectData);

                rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == costIndex);
                if (rewardData == null)
                {
                    rewardData = RewardManager.Instance.GetRewardData();
                    rewardData.V2Enum_Goods = GoodsManager.Instance.GetGoodsType(costIndex);
                    rewardData.Index = costIndex;
                    rewardData.Amount = 0;
                    m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                }

                rewardData.Amount += GetTotalCostToLevel(skillInfo.Level, synergyLevelUpCostData.SkillLevelUpCostGoodsParam21, synergyLevelUpCostData.SkillLevelUpCostGoodsParam22);

                minusLevel += skillInfo.Level - Define.PlayerSynergyDefaultLevel;

                skillInfo.Level = Define.PlayerSynergyDefaultLevel;

                doReset = true;
            }

            if (doReset == false)
                return false;

            if (m_setInGameRewardPopupMsg.RewardDatas.Count > 0)
            {
                for (int i = 0; i < m_setInGameRewardPopupMsg.RewardDatas.Count; ++i)
                {
                    RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas[i];

                    reward_type.Add(rewardData.Index);
                    before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.Index));
                    reward_quan.Add(rewardData.Amount);

                    GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);
                    after_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.Index));
                }


                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);
                //GuideQuestManager.Instance.CheckEventType(V2Enum_EventType.DailyMissionRewardGet);


                //ThirdPartyLog.Instance.SendLog_QuestEvent(contentDetailList, idx,
                //    reward_type, before_quan, reward_quan, after_quan);

                Message.Send(m_setInGameRewardPopupMsg);
                UI.IDialog.RequestDialogEnter<UI.InGameRewardPopupDialog>();

                //RedDotManager.Instance.HideRedDot(GetRedDotEnum(contentDetailList));
            }

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_log_skill_reset(Enum_SynergyType.Enum32ToInt(), equipedSynergy,
                reward_type, reward_quan);

            SynergyContainer.SynergyEffectCurrentTotalLevel -= minusLevel;
            if (SynergyContainer.SynergyEffectCurrentTotalLevel < 0)
                SynergyContainer.SynergyEffectCurrentTotalLevel = 0;

            BattleSceneManager.Instance.RefreshMyARRRStat();
            Message.Send(_refreshCharacterInfo_StatMsg);
            ARRRStatManager.Instance.RefreshBattlePower();

            return doReset;
        }
        //------------------------------------------------------------------------------------
        public double GetTotalCostToLevel(int level, int baseAmount, int levelAmount)
        {
            int n = level - 1;

            return (n * baseAmount) + (levelAmount * n * (n - 1) / 2);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region InGameSynergy
        //------------------------------------------------------------------------------------
        public void SetInGameSynergyData()
        {
            _synergyUnlockTier.Clear();

            for (int synergy = Enum_SynergyType.Red.Enum32ToInt(); synergy < Enum_SynergyType.Max.Enum32ToInt(); ++synergy)
            {
                Enum_SynergyType v2Enum_Stat = synergy.IntToEnum32<Enum_SynergyType>();
                List<SynergyEffectData> synergyEffectDatas = GetInGameEquipSynergyEffectData(v2Enum_Stat);

                if (synergyEffectDatas == null)
                    continue;

                for (int i = 0; i < synergyEffectDatas.Count; ++i)
                {
                    SynergyEffectData synergyEffectData = synergyEffectDatas[i];
                    synergyEffectData.SynergyCount = synergyEffectData.SynergyOriginCount;

                    if (IsLockSynergy(synergyEffectData) == false)
                    {
                        if (_synergyUnlockTier.ContainsKey(synergyEffectData.SynergyType) == false)
                            _synergyUnlockTier.Add(synergyEffectData.SynergyType, 0);

                        if (_synergyUnlockTier[synergyEffectData.SynergyType] < synergyEffectData.SynergyTier)
                            _synergyUnlockTier[synergyEffectData.SynergyType] = synergyEffectData.SynergyTier;
                    }

                    SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
                    if (skillInfo == null)
                        continue;

                    int mylevel = skillInfo.Level;
                }

            }
        }
        //------------------------------------------------------------------------------------
        public void AddGambleSynergy(Enum_SynergyType Enum_SynergyType, int stack, ref int descendEnhance)
        {
            if (_synergyStack.ContainsKey(Enum_SynergyType) == false)
            {
                _synergyStack.Add(Enum_SynergyType, 0);
            }

            List<SynergyEffectData> synergyEffectDatas = GetInGameEquipSynergyEffectData(Enum_SynergyType);

            if (synergyEffectDatas == null)
                return;

            int beforeStack = GetSynergyStack(Enum_SynergyType);
            int afterStack = beforeStack + stack;

            int descend = 0;

            if (synergyEffectDatas.Count > 0)
            {
                SynergyEffectData lastdata = synergyEffectDatas[synergyEffectDatas.Count - 1];

                if (lastdata.SynergyCount <= beforeStack)
                {
                    descend = stack;

                    Managers.GoodsManager.Instance.AddGoodsAmount(DescendContainer.DescendIngameEnforceData.EnforceCostGoodsIndex, stack);
                }
                else if (lastdata.SynergyCount < afterStack)
                {
                    descend = afterStack - lastdata.SynergyCount;

                    Managers.GoodsManager.Instance.AddGoodsAmount(DescendContainer.DescendIngameEnforceData.EnforceCostGoodsIndex, afterStack - lastdata.SynergyCount);
                }
            }



            descendEnhance = descend;

            _synergyStack[Enum_SynergyType] = afterStack;

            for (int i = 0; i < synergyEffectDatas.Count; ++i)
            {
                SynergyEffectData synergyEffectData = synergyEffectDatas[i];
                if (synergyEffectData.SynergyCount > beforeStack
                    && synergyEffectData.SynergyCount <= afterStack)
                {
                    if (synergyEffectData.SynergyTier <= GetInGameSynergyUnlockTier(synergyEffectData.SynergyType))
                        AddARRRSynergySkill(synergyEffectData);
                }
            }

            if (Managers.BattleSceneManager.Instance.BattleType != Enum_Dungeon.DiamondDungeon
                && Managers.BattleSceneManager.Instance.BattleType != Enum_Dungeon.TowerDungeon)
            {
                _refreshReadyGambleSynergyCombineSkillMsg.ReadySynergyCombineSkillList = GetReadySynergyCombineSkillList();
                Message.Send(_refreshReadyGambleSynergyCombineSkillMsg);
            }

            Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.RefreshSynergy(Enum_SynergyType, afterStack);

            Managers.DescendManager.Instance.RefreshGambleSynergy();
        }
        //------------------------------------------------------------------------------------
#if UNITY_EDITOR
        public int Cheat_AddSynergySkillIdx = -1;
        [ContextMenu("Cheat_AddSynergySkill")]
        private void CheatAddSynergySkill()
        {
            SynergyEffectData synergyEffectData = GetSynergyEffectData(Cheat_AddSynergySkillIdx);
            if (synergyEffectData != null)
                AddARRRSynergySkill(synergyEffectData, false);
        }
        //------------------------------------------------------------------------------------
#endif
        public void AddARRRSynergySkill(SynergyEffectData synergyEffectData, bool saveLevel = true)
        {
            Enum_SynergyType Enum_SynergyType = synergyEffectData.SynergyType;

            if (_synergySkillList.ContainsKey(Enum_SynergyType) == false)
            {
                _synergySkillList.Add(Enum_SynergyType, new List<MainSkillData>());
            }

            if (_synergyLevel.ContainsKey(Enum_SynergyType) == false)
            {
                _synergyLevel.Add(Enum_SynergyType, 0);
            }

            if (synergyEffectData.SynergySkillData != null)
            {
                if (_synergySkillList[Enum_SynergyType].Contains(synergyEffectData.SynergySkillData) == false)
                {
                    SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
                    Managers.BattleSceneManager.Instance.AddGambleSkill(synergyEffectData.SynergySkillData, synergyEffectData.SynergyType, skillInfo);

                    if (skillInfo != null)
                    {
                        if (synergyEffectData.SynergyRuneList != null)
                        {
                            for (int j = 0; j < synergyEffectData.SynergyRuneList.Count; ++j)
                            {
                                SynergyBreakthroughData synergyRuneData = synergyEffectData.SynergyRuneList[j];

                                if (IsGetedBreakthrough(synergyRuneData) == true)
                                {
                                    Managers.BattleSceneManager.Instance.AddGambleSkill(synergyRuneData.SynergySkillData);
                                }
                            }
                        }

                        if (SynergyRuneManager.Instance._targetAfterSkill.ContainsKey(synergyEffectData.SynergySkillData.MainSkillTypeParam1) == true)
                        {
                            List<MainSkillData> runeAfterSkill = SynergyRuneManager.Instance._targetAfterSkill[synergyEffectData.SynergySkillData.MainSkillTypeParam1];

                            for (int j = 0; j < runeAfterSkill.Count; ++j)
                            {
                                MainSkillData mainSkillData = runeAfterSkill[j];
                                Managers.BattleSceneManager.Instance.AddGambleSkill(mainSkillData);
                            }
                        }
                    }

                    _synergySkillList[Enum_SynergyType].Add(synergyEffectData.SynergySkillData);
                    if (saveLevel == true)
                    {
                        if (_synergyLevel[Enum_SynergyType] < synergyEffectData.SynergyTier)
                            _synergyLevel[Enum_SynergyType] = synergyEffectData.SynergyTier;
                    }
                    
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void AddSkillSynergy(Enum_SynergyType Enum_SynergyType, int stack)
        {
            if (Enum_SynergyType == Enum_SynergyType.Max)
                return;
            
            if (stack <= 0)
                return;

            _addSkillSynergyMsg.Enum_SynergyType = Enum_SynergyType;

            _addSkillSynergyMsg.BeforeData = GetCurrentSynergyEffectData(Enum_SynergyType);
            _addSkillSynergyMsg.BeforeStack = GetSynergyStack(Enum_SynergyType);

            AddGambleSynergy(Enum_SynergyType, stack, ref _addSkillSynergyMsg.DescendEnhance);

            _addSkillSynergyMsg.AfterData = GetCurrentSynergyEffectData(Enum_SynergyType);
            _addSkillSynergyMsg.AfterStack = GetSynergyStack(Enum_SynergyType);

            Message.Send(_addSkillSynergyMsg);
        }
        //------------------------------------------------------------------------------------
        public void UseGambleSynergyStack(Enum_SynergyType Enum_GambleSynergyType, int stack)
        {
            if (_synergyStack.ContainsKey(Enum_GambleSynergyType) == false)
                return;

            _synergyStack[Enum_GambleSynergyType] -= stack;
            if (_synergyStack[Enum_GambleSynergyType] < 0)
                _synergyStack[Enum_GambleSynergyType] = 0;

            _refreshGambleSynergyMsg.Enum_GambleSynergyType = Enum_GambleSynergyType;
            Message.Send(_refreshGambleSynergyMsg);
        }
        //------------------------------------------------------------------------------------
        public List<MainSkillData> GetSynergySkillList(Enum_SynergyType Enum_GambleSynergyType)
        {
            if (_synergySkillList.ContainsKey(Enum_GambleSynergyType) == false)
                return null;

            return _synergySkillList[Enum_GambleSynergyType];
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyStack(Enum_SynergyType Enum_GambleSynergyType)
        {
            if (_synergyStack.ContainsKey(Enum_GambleSynergyType) == false)
                return 0;

            return _synergyStack[Enum_GambleSynergyType];
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyLevel(Enum_SynergyType Enum_GambleSynergyType)
        {
            SynergyEffectData currentSynergy = GetCurrentSynergyEffectData(Enum_GambleSynergyType);
            if (currentSynergy == null)
                return 0;

            return currentSynergy.SynergyTier;
        }
        //------------------------------------------------------------------------------------
        public SynergyEffectData GetCurrentSynergyEffectData(Enum_SynergyType Enum_GambleSynergyType)
        {
            int stack = GetSynergyStack(Enum_GambleSynergyType);

            if (stack == 0)
                return null;

            List<SynergyEffectData> gambleSynergyEffectDatas = GetInGameEquipSynergyEffectData(Enum_GambleSynergyType);

            if (gambleSynergyEffectDatas == null)
                return null;

            SynergyEffectData currentSynergy = null;

            // 임시 테스트로
            if (_synergyLevel.ContainsKey(Enum_GambleSynergyType) == false)
                return null;

            currentSynergy = gambleSynergyEffectDatas.Find(x => x.SynergyTier == _synergyLevel[Enum_GambleSynergyType]);

            return currentSynergy;
            // 임시 테스트로

            for (int i = 0; i < gambleSynergyEffectDatas.Count; ++i)
            {
                SynergyEffectData gambleSynergyEffectData = gambleSynergyEffectDatas[i];
                if (gambleSynergyEffectData.SynergyCount <= stack)
                    currentSynergy = gambleSynergyEffectData;
            }

            return currentSynergy;
        }
        //------------------------------------------------------------------------------------
        public SynergyEffectData GetFirstSynergy(Enum_SynergyType Enum_GambleSynergyType)
        {
            List<SynergyEffectData> gambleSynergyEffectDatas = GetInGameEquipSynergyEffectData(Enum_GambleSynergyType);

            if (gambleSynergyEffectDatas == null)
                return null;

            if (gambleSynergyEffectDatas.Count > 0)
                return gambleSynergyEffectDatas[0];

            return null;
        }
        //------------------------------------------------------------------------------------
        public void ShowSynergyDetailPopup(Enum_SynergyType Enum_GambleSynergyType, SynergyEffectData focusData = null)
        {
            _showGambleSynergyDetailMsg.FocusData = focusData;
            _showGambleSynergyDetailMsg.Enum_GambleSynergyType = Enum_GambleSynergyType;
            Message.Send(_showGambleSynergyDetailMsg);

            //Managers.GuideInteractorManager.Instance.PlayCardTutorial = false;
        }
        //------------------------------------------------------------------------------------
        public SynergyEffectData CanSynergyLevelUp(ARR_CardGambleData gambleSkillData)
        {
            if (gambleSkillData == null)
                return null;

            Enum_SynergyType Enum_GambleSynergyType = gambleSkillData.SynergyType;

            SynergyEffectData nextdata = GetNextSynergyData(Enum_GambleSynergyType);
            if (nextdata == null)
                return null;

            if (nextdata.SynergyTier > GetInGameSynergyUnlockTier(Enum_GambleSynergyType))
                return null;

            int currentstack = GetSynergyStack(Enum_GambleSynergyType);

            if (nextdata.SynergyCount <= currentstack + gambleSkillData.SynergyStack)
                return nextdata;

            return null;
        }
        //------------------------------------------------------------------------------------
        public SynergyEffectData GetNextSynergyData(Enum_SynergyType Enum_GambleSynergyType)
        {
            SynergyEffectData gambleSynergyEffectData = GetCurrentSynergyEffectData(Enum_GambleSynergyType);

            SynergyEffectData nextdata = null;
            if (gambleSynergyEffectData == null)
                nextdata = GetFirstSynergy(Enum_GambleSynergyType);
            else
                nextdata = gambleSynergyEffectData.NextEffectData;

            return nextdata;
        }
        //------------------------------------------------------------------------------------
        public void ShowInterestText(double gold)
        {
            _showInterestTextMsg.text = string.Format("+{0}", (int)gold);
            Message.Send(_showInterestTextMsg);
        }
        //------------------------------------------------------------------------------------
        public void ShowInterestText(string str)
        {
            _showInterestTextMsg.text = str;
            Message.Send(_showInterestTextMsg);
        }
        //------------------------------------------------------------------------------------
        public int GetInGameSynergyUnlockTier(Enum_SynergyType Enum_GambleSynergyType)
        {
            if (_synergyUnlockTier.ContainsKey(Enum_GambleSynergyType) == false)
                return 0;
            return _synergyUnlockTier[Enum_GambleSynergyType];
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region SynergyCombine
        //------------------------------------------------------------------------------------
        public List<SynergyCombineData> GetReadySynergyCombineSkillList()
        {
            List<SynergyCombineData> gambleSynergyCombineDatas = GetAllGambleSynergyCombineData();

            _readySynergyCombineSkillList.Clear();

            for (int i = 0; i < gambleSynergyCombineDatas.Count; ++i)
            {
                SynergyCombineData gambleSynergyCombineData = gambleSynergyCombineDatas[i];

                if (IsAlReadySynergyCombineSkill(gambleSynergyCombineData) == true)
                    continue;

                if (IsReadySynergyCombineSkill(gambleSynergyCombineData) == true)
                    _readySynergyCombineSkillList.Add(gambleSynergyCombineData);
            }

            return _readySynergyCombineSkillList;
        }
        //------------------------------------------------------------------------------------
        public bool IsReadySynergyCombineSkill(SynergyCombineData gambleSynergyCombineData)
        {
            if (gambleSynergyCombineData == null)
                return false;

            foreach (var pair in gambleSynergyCombineData.NeedSynergyCount)
            {
                if (GetSynergyStack(pair.Key) < pair.Value)
                    return false;
            }

            return true;
        }
        //------------------------------------------------------------------------------------
        public float GetSynergyCombineSkillReadyRatio(SynergyCombineData gambleSynergyCombineData)
        {
            if (gambleSynergyCombineData == null)
                return 0.0f;

            float needcount = 0;
            float readycount = 0;

            foreach (var pair in gambleSynergyCombineData.NeedSynergyCount)
            {
                needcount += pair.Value;
                readycount += Mathf.Min(GetSynergyStack(pair.Key), pair.Value);
            }

            return readycount / needcount;
        }
        //------------------------------------------------------------------------------------
        public bool IsAlReadySynergyCombineSkill(SynergyCombineData gambleSynergyCombineData)
        {
            return _equipedSynergyCombineSkillList.Contains(gambleSynergyCombineData);
        }
        //------------------------------------------------------------------------------------
        public bool AddSynergyCombineSkill(SynergyCombineData gambleSynergyCombineData)
        {

            if (Managers.BattleSceneManager.Instance.BattleType == Enum_Dungeon.DiamondDungeon
                || Managers.BattleSceneManager.Instance.BattleType == Enum_Dungeon.TowerDungeon)
            {
                return false;
            }

            if (IsReadySynergyCombineSkill(gambleSynergyCombineData) == false)
                return false;

            if (_equipedSynergyCombineSkillList.Contains(gambleSynergyCombineData) == true)
                return false;

            //foreach (var pair in gambleSynergyCombineData.NeedSynergyCount)
            //{
            //    UseGambleSynergyStack(pair.Key, pair.Value);
            //}

            Managers.BattleSceneManager.Instance.AddGambleSkill(gambleSynergyCombineData.SynergySkillData);
            _equipedSynergyCombineSkillList.Add(gambleSynergyCombineData);

            _refreshGambleSynergyCombineSkillMsg.GambleSynergyCombineData = gambleSynergyCombineData;
            Message.Send(_refreshGambleSynergyCombineSkillMsg);

            _refreshReadyGambleSynergyCombineSkillMsg.ReadySynergyCombineSkillList = GetReadySynergyCombineSkillList();
            Message.Send(_refreshReadyGambleSynergyCombineSkillMsg);

            ThirdPartyLog.Instance.SendLog_Mission(gambleSynergyCombineData.Index);

            Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.SkillActiveController.AddMissionCompleteCount(1);

            double researchgold = Managers.ResearchManager.Instance.GetResearchValues(V2Enum_ResearchType.MissionRewardIncrease);
            if (researchgold > 0)
            {
                Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt(), researchgold);

                ShowInterestText(string.Format("+{0}({1})", researchgold
                , Managers.ResearchManager.Instance.GetEffectTitle(V2Enum_ResearchType.MissionRewardIncrease)));
            }

            return true;
        }
        //------------------------------------------------------------------------------------
        public int GetCombineClearCount()
        {
            return _equipedSynergyCombineSkillList.Count;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}