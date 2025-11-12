using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class DescendManager : MonoSingleton<DescendManager>
    {
        private List<DescendData> _inActiveDescendDatas = new List<DescendData>();
        private Dictionary<DescendData, int> _activeDescendDatas = new Dictionary<DescendData, int>();


        private Event.ChangeEquipDescendMsg _changeEquipSynergyMsg = new Event.ChangeEquipDescendMsg();
        private Event.ShowNewDescendMsg _showNewSynergyMsg = new Event.ShowNewDescendMsg();
        private Event.RefreshInGameReadyDescendMsg _refreshInGameReadyDescendMsg = new Event.RefreshInGameReadyDescendMsg();
        private Event.ChangeInGameActiveDescendMsg _changeInGameActiveDescendMsg = new Event.ChangeInGameActiveDescendMsg();

        private GameBerry.Event.RefreshCharacterInfo_StatMsg _refreshCharacterInfo_StatMsg = new Event.RefreshCharacterInfo_StatMsg();

        private ObscuredInt _skillSlotCount = 3;

        private List<string> m_changeInfoUpdate = new List<string>();

        private Dictionary<int, Sprite> _skillIcons = new Dictionary<int, Sprite>();

        private Dictionary<V2Enum_Stat, ObscuredDouble> _arrrSynergyTotalStatValues = new Dictionary<V2Enum_Stat, ObscuredDouble>();
        public Dictionary<V2Enum_Stat, ObscuredDouble> ArrrSynergyTotalStatValues { get { return _arrrSynergyTotalStatValues; } }

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerDescendInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);

            DescendOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitDescendContent()
        {
            string newSynergyIndex = PlayerPrefs.GetString(Define.NewDescendKey);
            if (string.IsNullOrEmpty(newSynergyIndex) == false)
            {
                string[] arr = newSynergyIndex.Split(',');

                for (int i = 0; i < arr.Length; ++i)
                {
                    int index = arr[i].ToInt() + 110010000;
                    DescendData synergyEffectData = GetSynergyEffectData(index);
                    if (synergyEffectData == null)
                        continue;

                    AddNewSynergyIcon(synergyEffectData);
                }
            }

            foreach (var pair in GetAllSynergyEffectDatas())
            {
                DescendData synergyEffectData = pair.Value;

                if (synergyEffectData.DescendType == Enum_DescendType.DescendSkill)
                    synergyEffectData.SynergySkillData = SkillManager.Instance.GetMainSkillData(synergyEffectData.DescendParam1);
                else if (synergyEffectData.DescendType == Enum_DescendType.DescendPassive)
                    synergyEffectData.PetData = PetManager.Instance.GetPetData(synergyEffectData.DescendParam1);

                if (synergyEffectData.SynergyRuneList != null)
                {
                    for (int r = 0; r < synergyEffectData.SynergyRuneList.Count; ++r)
                    {
                        DescendBreakthroughData synergyRuneData = synergyEffectData.SynergyRuneList[r];
                        synergyRuneData.SynergySkillData = SkillManager.Instance.GetMainSkillData(synergyRuneData.MainSkillIndex);
                    }
                }

                SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
                if (skillInfo != null)
                {
                    DescendLevelUpStatData synergyReinforceStatData = GetSynergyReinforceStatData(skillInfo.Level);

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

            foreach (var pair in GetAllDescendSlotConditionDatas())
            {
                if (DescendContainer.SynergyEquip_Dic.ContainsKey(pair.Key) == false)
                    DescendContainer.SynergyEquip_Dic.Add(pair.Key, -1);
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshSynergyRedDot()
        {
            foreach (var pair in GetAllSynergyEffectDatas())
            {
                DescendData synergyEffectData = pair.Value;

                SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);

                if (skillInfo == null)
                    continue;

                if (ReadySynergyEnhance(synergyEffectData) == true)
                { 
                    Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyDescend);
                    break;
                }
            }

            RefreshNewSynergyIcon();
        }
        //------------------------------------------------------------------------------------
        #region Data
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, DescendData> GetAllSynergyEffectDatas()
        {
            return DescendOperator.GetAllSynergyEffectDatas();
        }
        //------------------------------------------------------------------------------------
        public DescendData GetSynergyEffectData(ObscuredInt index)
        {
            return DescendOperator.GetSynergyEffectData(index);
        }
        //------------------------------------------------------------------------------------
        public DescendLevelUpCostData GetSynergyLevelUpCostData(DescendData synergyEffectData)
        {
            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);

            if (skillInfo == null)
                return null;

            return DescendOperator.GetSynergyLevelUpCostData(skillInfo.Level - 1);
        }
        //------------------------------------------------------------------------------------
        public DescendBreakthroughCostData GetSynergyLevelUpLimitData(ObscuredInt level)
        {
            return DescendOperator.GetSynergyLevelUpLimitData(level);
        }
        //------------------------------------------------------------------------------------
        public DescendDuplicationData GetSynergyDuplicationData(ObscuredInt index)
        {
            return DescendOperator.GetSynergyDuplicationData(index);
        }
        //------------------------------------------------------------------------------------
        public DescendLevelUpStatData GetSynergyReinforceStatData(ObscuredInt level)
        {
            return DescendOperator.GetSynergyReinforceStatData(level);
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, DescendSlotConditionData> GetAllDescendSlotConditionDatas()
        {
            return DescendOperator.GetAllDescendSlotConditionDatas();
        }
        //------------------------------------------------------------------------------------
        public DescendSlotConditionData GetDescendSlotConditionData(ObscuredInt slot)
        {
            return DescendOperator.GetDescendSlotConditionData(slot);
        }
        //------------------------------------------------------------------------------------
        public SkillInfo GetSynergyEffectSkillInfo(ObscuredInt index)
        {
            return GetSynergyEffectSkillInfo(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        public SkillInfo GetSynergyEffectSkillInfo(DescendData synergyEffectData)
        {
            if (synergyEffectData == null)
                return null;

            if (DescendContainer.SynergyInfo.ContainsKey(synergyEffectData.Index) == true)
                return DescendContainer.SynergyInfo[synergyEffectData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SkillInfo AddNewSkillInfo(DescendData synergyEffectData)
        {
            if (synergyEffectData == null)
                return null;

            SkillInfo skillInfo = new SkillInfo();
            skillInfo.Id = synergyEffectData.Index;
            skillInfo.Level = Define.PlayerDescendDefaultLevel;
            skillInfo.Count = 0;

            DescendContainer.SynergyInfo.Add(skillInfo.Id, skillInfo);


            return skillInfo;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, ObscuredInt> GetEquipSynergyEffect()
        {
            return DescendContainer.SynergyEquip_Dic;
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetSynergyGrade(int index)
        {
            return GetSynergyGrade(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetSynergyGrade(DescendData synergyEffectData)
        {
            if (synergyEffectData == null)
                return V2Enum_Grade.Max;

            if (synergyEffectData.SynergySkillData == null)
                return V2Enum_Grade.Max;

            return synergyEffectData.Grade;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetDescendIcon(DescendData skillBaseData)
        {
            if (skillBaseData == null)
                return null;

            return GetIcon(skillBaseData.DescendIconIndex);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetDescendIcon(int index)
        {
            return GetDescendIcon(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        private Sprite GetIcon(int iconIndex)
        {
            Sprite sp = null;

            if (_skillIcons.ContainsKey(iconIndex) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(Define.DescendIconPath, iconIndex), o =>
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
        public void SetSynergyAmount(int index, int amount)
        {
            SetSynergyAmount(GetSynergyEffectData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyAmount(DescendData synergyEffectData, int amount)
        {
            if (synergyEffectData == null)
                return;

            SkillInfo playerSkillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (playerSkillInfo == null)
            {
                playerSkillInfo = AddNewSkillInfo(synergyEffectData);

                AddNewSynergyIcon(synergyEffectData);

                ARRRStatManager.Instance.RefreshBattlePower();
            }

            playerSkillInfo.Count = amount;

            if (ReadySynergyEnhance(synergyEffectData) == true)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyDescend);
            }
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAmount(int index)
        {
            return GetSynergyAmount(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAmount(DescendData synergyEffectData)
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
        public int AddSynergyAmount(DescendData synergyEffectData, int amount)
        {
            if (synergyEffectData == null)
                return 0;

            SkillInfo playerSkillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (playerSkillInfo == null)
            {
                playerSkillInfo = AddNewSkillInfo(synergyEffectData);

                AddNewSynergyIcon(synergyEffectData);

                ARRRStatManager.Instance.RefreshBattlePower();
            }

            playerSkillInfo.Count += amount;

            if (ReadySynergyEnhance(synergyEffectData) == true)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyDescend);
            }

            return playerSkillInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public int AddSynergyAmount(RewardData rewardData)
        {
            if (rewardData == null)
                return 0;

            DescendData synergyEffectData = GetSynergyEffectData(rewardData.Index);
            if (synergyEffectData == null)
                return 0;

            int DupliCount = 0;

            SkillInfo playerSkillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (playerSkillInfo == null)
            {
                playerSkillInfo = AddNewSkillInfo(synergyEffectData);
                playerSkillInfo.Count = 1;
                DupliCount = (int)rewardData.Amount - 1;

                AddNewSynergyIcon(synergyEffectData);

                ARRRStatManager.Instance.RefreshBattlePower();
            }
            else
            {
                DupliCount = (int)rewardData.Amount;
            }

            if (DupliCount > 0)
            {
                DescendDuplicationData synergyDuplicationData = GetSynergyDuplicationData(synergyEffectData.Index);
                if (synergyDuplicationData != null)
                {
                    rewardData.DupliIndex = synergyDuplicationData.DuplicationGoodsIndex;
                    rewardData.DupliAmount = synergyDuplicationData.DuplicationGoodsValue * DupliCount;

                    GoodsManager.Instance.AddGoodsAmount(rewardData.DupliIndex, rewardData.DupliAmount);
                }
            }

            if (ReadySynergyEnhance(synergyEffectData) == true)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyDescend);
            }

            return 1;
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyAmount(int index, int amount)
        {
            return UseSynergyAmount(GetSynergyEffectData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyAmount(DescendData synergyEffectData, int amount)
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
            DescendData synergyEffectData = GetSynergyEffectData(index);
            if (synergyEffectData != null)
            {
                return synergyEffectData.NameLocalKey;
            }

            return string.Empty;
        }
        //------------------------------------------------------------------------------------
        public void AddNewSynergyIcon(DescendData synergyEffectData)
        {
            if (synergyEffectData != null)
            {
                if (DescendContainer.NewSynergys.ContainsKey(synergyEffectData) == false)
                    DescendContainer.NewSynergys.Add(synergyEffectData, 1);
                else
                    DescendContainer.NewSynergys[synergyEffectData] += 1;

                PlayerPrefs.SetString(Define.NewDescendKey, DescendContainer.GetNewSynergySerializeString());

            }

            Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.GetDescendSkill);

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
            int count = DescendContainer.NewSynergys.Count;
            return count;
        }
        //------------------------------------------------------------------------------------
        public bool IsNewSynergyIcon(DescendData synergyEffectData)
        {
            return DescendContainer.NewSynergys.ContainsKey(synergyEffectData);
        }
        //------------------------------------------------------------------------------------
        public void RemoveNewIconSynergy(DescendData synergyEffectData)
        {
            if (synergyEffectData != null)
            {
                if (DescendContainer.NewSynergys.ContainsKey(synergyEffectData) == true)
                    DescendContainer.NewSynergys.Remove(synergyEffectData);

                PlayerPrefs.SetString(Define.NewDescendKey, DescendContainer.GetNewSynergySerializeString());
            }
        }
        //------------------------------------------------------------------------------------
        public void RemoveAllNewIconSynergy()
        {
            DescendContainer.NewSynergys.Clear();

            PlayerPrefs.SetString(Define.NewDescendKey, DescendContainer.GetNewSynergySerializeString());
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region SynergyBreakthroughData
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        public DescendBreakthroughData GetDescendBreakthroughData(ObscuredInt index)
        {
            return DescendOperator.GetSynergyRuneData(index);
        }
        //------------------------------------------------------------------------------------
        public bool IsGetedBreakthrough(DescendBreakthroughData SynergyRuneData)
        {
            if (SynergyRuneData == null)
                return false;

            DescendData synergyEffectData = GetSynergyEffectData(SynergyRuneData.DecendSkillIndex);
            if (synergyEffectData == null)
                return false;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return false;

            return skillInfo.LimitCompleteLevel >= SynergyRuneData.Procedure;
        }
        //------------------------------------------------------------------------------------
        private void DoNewSynergyBreakthroughNotice(DescendBreakthroughData SynergyRuneData, bool showNewIcon = true)
        {
            //Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.StackSkillCount);
            //Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.GetSynergySkill);
            //Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.StackSkillCount);
            //Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.GetSynergySkill);
            if (showNewIcon == true)
                AddNewSynergyBreakthroughIcon(SynergyRuneData);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSynergyBreakthroughSprite(int index)
        {
            return GetSynergyBreakthroughSprite(GetDescendBreakthroughData(index));
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSynergyBreakthroughSprite(DescendBreakthroughData SynergyRuneData)
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
            SetSynergyBreakthroughAmount(GetDescendBreakthroughData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyBreakthroughAmount(DescendBreakthroughData SynergyRuneData, int amount)
        {
            if (SynergyRuneData == null)
                return;

            if (DescendContainer.Runes.Contains(SynergyRuneData.Index) == false)
            {
                DescendContainer.Runes.Add(SynergyRuneData.Index);
                DoNewSynergyBreakthroughNotice(SynergyRuneData);
                ARRRStatManager.Instance.RefreshBattlePower();
            }
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyBreakthroughAmount(int index)
        {
            return GetSynergyBreakthroughAmount(GetDescendBreakthroughData(index));
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyBreakthroughAmount(DescendBreakthroughData SynergyRuneData)
        {
            if (SynergyRuneData == null)
                return 0;

            return DescendContainer.Runes.Contains(SynergyRuneData.Index) == false ? 0 : 1;
        }
        //------------------------------------------------------------------------------------
        public int AddSynergyBreakthroughAmount(int index, int amount)
        {
            return AddSynergyBreakthroughAmount(GetDescendBreakthroughData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int AddSynergyBreakthroughAmount(DescendBreakthroughData SynergyRuneData, int amount)
        {
            if (SynergyRuneData == null)
                return 0;

            if (DescendContainer.Runes.Contains(SynergyRuneData.Index) == false)
            {
                DescendContainer.Runes.Add(SynergyRuneData.Index);
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

            DescendBreakthroughData SynergyRuneData = GetDescendBreakthroughData(rewardData.Index);
            if (SynergyRuneData == null)
                return 0;

            int DupliCount = 0;


            if (DescendContainer.Runes.Contains(SynergyRuneData.Index) == false)
            {
                DescendContainer.Runes.Add(SynergyRuneData.Index);
                DupliCount = (int)rewardData.Amount - 1;

                ARRRStatManager.Instance.RefreshBattlePower();
            }
            else
            {
                DupliCount = (int)rewardData.Amount;
            }

            return 1;
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyBreakthroughAmount(int index, int amount)
        {
            return 1;
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyBreakthroughAmount(DescendBreakthroughData SynergyRuneData, int amount)
        {
            return 1;
        }
        //------------------------------------------------------------------------------------
        public string GetSynergyBreakthroughLocalKey(int index)
        {
            DescendBreakthroughData SynergyRuneData = GetDescendBreakthroughData(index);
            if (SynergyRuneData != null)
            {
                return SynergyRuneData.NameLocalKey;
            }

            return string.Empty;
        }
        //------------------------------------------------------------------------------------
        public void AddNewSynergyBreakthroughIcon(DescendBreakthroughData SynergyRuneData)
        {
            if (SynergyRuneData != null)
            {
                if (DescendContainer.NewRunes.ContainsKey(SynergyRuneData) == false)
                    DescendContainer.NewRunes.Add(SynergyRuneData, 1);
                else
                    DescendContainer.NewRunes[SynergyRuneData] += 1;

                PlayerPrefs.SetString(Define.NewSynergyBreakKey, DescendContainer.GetNewRuneSerializeString());

                DescendData synergyEffectData = GetSynergyEffectData(SynergyRuneData.DecendSkillIndex);
                if (synergyEffectData != null)
                    AddNewSynergyIcon(synergyEffectData);
            }
        }
        //------------------------------------------------------------------------------------
        public int GetNewSynergyBreakthroughIconCount()
        {
            int count = DescendContainer.NewRunes.Count;
            return count;
        }
        //------------------------------------------------------------------------------------
        public bool IsNewSynergyBreakthroughIcon(DescendBreakthroughData SynergyRuneData)
        {
            return DescendContainer.NewRunes.ContainsKey(SynergyRuneData);
        }
        //------------------------------------------------------------------------------------
        public void RemoveNewIconBreakthroughSynergy(DescendBreakthroughData SynergyRuneData)
        {
            if (SynergyRuneData != null)
            {
                if (DescendContainer.NewRunes.ContainsKey(SynergyRuneData) == true)
                    DescendContainer.NewRunes.Remove(SynergyRuneData);

                PlayerPrefs.SetString(Define.NewSynergyBreakKey, DescendContainer.GetNewSynergySerializeString());
            }
        }
        //------------------------------------------------------------------------------------
        public void BuySynergyBreakthrough(DescendData synergyEffectData)
        {

        }
        //------------------------------------------------------------------------------------
        public int GetSynergyBreakthroughCount(DescendData synergyEffectData)
        {
            if (synergyEffectData == null)
                return 0;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return 0;
            return skillInfo.LimitCompleteLevel;
        }
        //------------------------------------------------------------------------------------
        public bool NeedLimitBreak(DescendData synergyEffectData)
        {
            if (synergyEffectData == null)
                return false;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return false;

            DescendBreakthroughCostData synergyLevelUpLimitData = GetSynergyLevelUpLimitData(skillInfo.LimitCompleteLevel);

            if (synergyLevelUpLimitData == null)
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool ReadyCharacterLimitLevelUpCost(DescendBreakthroughCostData characterLevelUpCost)
        {
            double amount = GoodsManager.Instance.GetGoodsAmount(characterLevelUpCost.LimitBreakCostGoodsIndex);

            double need = characterLevelUpCost.LimitBreakCostGoodsValue;

            return amount >= need;
        }
        //------------------------------------------------------------------------------------
        public bool DoARRRLimitUp(DescendData synergyEffectData)
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


            DescendBreakthroughCostData synergyLevelUpLimitData = GetSynergyLevelUpLimitData(skillInfo.LimitCompleteLevel);
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
                    Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.ShopDescendStore);
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

            ThirdPartyLog.Instance.SendLog_log_descend_limitup(synergyEffectData.Index, skillInfo.LimitCompleteLevel,
                used_type, former_quan, used_quan, keep_quan);


            return true;
        }
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region LobbyDescendSetting
        //------------------------------------------------------------------------------------
        public bool IsFullEquipSkillSlot()
        {
            int slotidx = -1;

            foreach (var pair in DescendContainer.SynergyEquip_Dic)
            {
                if (pair.Value == -1)
                {
                    slotidx = pair.Key;
                    break;
                }
            }

            return slotidx == -1;
        }
        //------------------------------------------------------------------------------------
        public bool IsEquipSkill(DescendData synergyEffectData)
        {
            return DescendContainer.SynergyEquip_Dic.ContainsValue(synergyEffectData.Index);
        }
        //------------------------------------------------------------------------------------
        public bool EquipSkill(DescendData aRRRSkillData)
        {
            if (aRRRSkillData == null)
                return false;

            if (IsEquipSkill(aRRRSkillData) == true)
                return false;

            int slotidx = -1;

            foreach (var pair in DescendContainer.SynergyEquip_Dic)
            {
                if (pair.Value == -1)
                {
                    slotidx = pair.Key;
                    break;
                }
            }

            if (IsOpenDescendSlot(slotidx) == false)
            {
                Contents.GlobalContent.ShowGlobalNotice_LocalString("ui/descend/slotfull");

                //ShowNoticeDescendSlotUnLockExp(slotidx);
                return false;
            }

            if (slotidx == -1)
                return false;

            DescendContainer.SynergyEquip_Dic[slotidx] = aRRRSkillData.Index;

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerDescendInfoTable);

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool IsOpenDescendSlot(int idx)
        {
            DescendSlotConditionData descendSlotConditionData = GetDescendSlotConditionData(idx);

            if (descendSlotConditionData == null)
                return false;

            return Managers.ContentOpenConditionManager.Instance.IsOpen(descendSlotConditionData.OpenConditionType, descendSlotConditionData.OpenConditionValue);
        }
        //------------------------------------------------------------------------------------
        public void ShowNoticeDescendSlotUnLockExp(int idx)
        {

            DescendSlotConditionData descendSlotConditionData = GetDescendSlotConditionData(idx);

            if (descendSlotConditionData == null)
                return;

            Contents.GlobalContent.ShowGlobalNotice(Managers.ContentOpenConditionManager.Instance.GetOpenContitionLocalString(descendSlotConditionData.OpenConditionType, descendSlotConditionData.OpenConditionValue));
        }
        //------------------------------------------------------------------------------------
        public void SweepSkillSlot(int from, int to)
        {
            int fromeSkill = -1;
            int toSkill = -1;

            if (IsOpenDescendSlot(from) == false)
            {
                ShowNoticeDescendSlotUnLockExp(from);
                return;
            }


            if (IsOpenDescendSlot(to) == false)
            {
                ShowNoticeDescendSlotUnLockExp(to);
                return;
            }

            if (DescendContainer.SynergyEquip_Dic.ContainsKey(from) == false
                || DescendContainer.SynergyEquip_Dic.ContainsKey(to) == false)
                return;

            fromeSkill = DescendContainer.SynergyEquip_Dic[from];
            toSkill = DescendContainer.SynergyEquip_Dic[to];

            DescendContainer.SynergyEquip_Dic[from] = toSkill;
            DescendContainer.SynergyEquip_Dic[to] = fromeSkill;

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerDescendInfoTable);
        }
        //------------------------------------------------------------------------------------
        public DescendData UnEquipSkillSlot(int slotIndex)
        {
            if (DescendContainer.SynergyEquip_Dic.ContainsKey(slotIndex) == false)
                return null;

            DescendData aRRRSkillData = GetSynergyEffectData(DescendContainer.SynergyEquip_Dic[slotIndex]);

            DescendContainer.SynergyEquip_Dic[slotIndex] = -1;

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerDescendInfoTable);
            return aRRRSkillData;
        }
        //------------------------------------------------------------------------------------
        public bool UnEquipSkillSlot(DescendData aRRRSkillData)
        {
            if (aRRRSkillData == null)
                return false;

            int slotIndex = -1;

            foreach (var pair in DescendContainer.SynergyEquip_Dic)
            {
                if (pair.Value == aRRRSkillData.Index)
                {
                    slotIndex = pair.Key;
                }
            }

            if (slotIndex != -1)
            {
                DescendContainer.SynergyEquip_Dic[slotIndex] = -1;
                TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerDescendInfoTable);
                return true;
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public void ChangeEquipSynergy(DescendData synergyEffectData, int slot)
        {
            if (synergyEffectData == null)
                return;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return;

            int index = -1;
            if (DescendContainer.SynergyEquip_Dic.ContainsKey(slot) == true)
                index = DescendContainer.SynergyEquip_Dic[slot];
            else
                DescendContainer.SynergyEquip_Dic.Add(slot, -1);

            DescendData beforeData = GetSynergyEffectData(index);

            DescendContainer.SynergyEquip_Dic[slot] = synergyEffectData.Index;

            _changeEquipSynergyMsg.BeforeEquipSynergy = beforeData;
            _changeEquipSynergyMsg.AfterEquipSynergy = synergyEffectData;

            Message.Send(_changeEquipSynergyMsg);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxLevelSynergy(DescendData synergyEffectData)
        {
            if (synergyEffectData == null)
                return false;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return false;

            DescendLevelUpCostData synergyLevelUpCostData = GetSynergyLevelUpCostData(synergyEffectData);

            if (synergyLevelUpCostData == null)
                return false;

            return synergyLevelUpCostData.DescendLevelUpCostGoodParam1 < 1;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhance_NeedCount1(DescendData synergyEffectData)
        {
            if (synergyEffectData == null)
                return 99999;

            DescendLevelUpCostData synergyLevelUpCostData = GetSynergyLevelUpCostData(synergyEffectData);

            if (synergyLevelUpCostData == null)
                return 99999;

            return synergyLevelUpCostData.DescendLevelUpCostGoodParam1;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhanceCostGoodsIndex1(DescendData synergyEffectData)
        {
            if (synergyEffectData == null)
                return -1;

            DescendLevelUpCostData synergyLevelUpCostData = GetSynergyLevelUpCostData(synergyEffectData);

            if (synergyLevelUpCostData == null)
                return -1;

            return synergyLevelUpCostData.DescendLevelUpCostGoodsIndex;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhance_NeedCount2(DescendData synergyEffectData)
        {
            if (synergyEffectData == null)
                return 99999;

            DescendLevelUpCostData synergyLevelUpCostData = GetSynergyLevelUpCostData(synergyEffectData);

            if (synergyLevelUpCostData == null)
                return 99999;

            return synergyLevelUpCostData.DescendLevelUpCostGoodParam2;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhanceCostGoodsIndex2(DescendData synergyEffectData)
        {
            if (synergyEffectData == null)
                return -1;

            DescendLevelUpCostData synergyLevelUpCostData = GetSynergyLevelUpCostData(synergyEffectData);

            if (synergyLevelUpCostData == null)
                return -1;

            return synergyLevelUpCostData.DescendLevelUpCostGoodsIndex2;
        }
        //------------------------------------------------------------------------------------
        public bool ReadySynergyEnhance(DescendData synergyEffectData)
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
        public bool EnhanceSynergy(DescendData synergyEffectData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (IsMaxLevelSynergy(synergyEffectData) == true)
                return false;

            if (ReadySynergyEnhance(synergyEffectData) == false)
                return false;

            SynergyTotalLevelCostData synergyTotalLevelCostData = DescendContainer.SynergyTotalLevelCostData;

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
            //skillInfo.Count -= GetSynergyEnhance_NeedCount(synergyEffectData);
            skillInfo.Level += 1;

            DescendLevelUpStatData synergyReinforceStatData = GetSynergyReinforceStatData(skillInfo.Level);

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

            DescendContainer.SynergyAccumLevel += 1;

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_log_descend_enforce(synergyEffectData.Index, skillInfo.Level,
                 used_type, before_quan, used_quan, after_quan,
                 DescendContainer.SynergyContentExp);

            PassManager.Instance.CheckPassType(V2Enum_PassType.DescendLevel);
            Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.DescendSkillLevelStack);

            ARRRStatManager.Instance.RefreshBattlePower();

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool GetSynergy(DescendData synergyEffectData)
        {
            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo != null)
                return false;

            DescendOpenCostData synergyConditionData = synergyEffectData.DescendOpenCostData;
            if (synergyConditionData == null)
                return false;

            if (synergyConditionData.OpenCostGoodsIndex == -1)
                return false;

            int used_type;
            double before_quan, used_quan, after_quan;

            int costIndex = synergyConditionData.OpenCostGoodsIndex;
            used_type = costIndex;

            if (Managers.GoodsManager.Instance.GetGoodsAmount(costIndex) < synergyConditionData.OpenCostGoodsValue)
            {
                Contents.GlobalContent.ShowPopup_OkCancel(
                Managers.LocalStringManager.Instance.GetLocalString("common/ui/shortagegoodstitle"),
                Managers.LocalStringManager.Instance.GetLocalString("common/ui/shortagegoodsdesc"),
                () =>
                {
                    UI.IDialog.RequestDialogExit<UI.LobbyDescendContentDialog>();
                    Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.ShopDescendStore);
                },
                null);
                return false;
            }

            before_quan = Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);
            used_quan = synergyConditionData.OpenCostGoodsValue;
            Managers.GoodsManager.Instance.UseGoodsAmount(costIndex, synergyConditionData.OpenCostGoodsValue);

            after_quan = Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

            skillInfo = AddNewSkillInfo(synergyEffectData);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_log_descend_acquire(synergyEffectData.Index,
    used_type, before_quan, used_quan, after_quan);

            ARRRStatManager.Instance.RefreshBattlePower();

            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region InGameDescend
        //------------------------------------------------------------------------------------
        public void SetInGameDescendData()
        {
            foreach (var pair in GetEquipSynergyEffect())
            {
                DescendData descendData = GetSynergyEffectData(pair.Value);

                if (descendData == null)
                    continue;

                _inActiveDescendDatas.Add(descendData);

                descendData.NeedRequiredExp = GetFinalInGameExp(descendData);

            }
        }
        //------------------------------------------------------------------------------------
        public double GetFinalInGameExp(DescendData descendData)
        {
            if(descendData == null)
            return 9999;

            double exp = descendData.OriginNeedRequiredExp;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(descendData);
            if (skillInfo == null)
                return exp;

            int mylevel = skillInfo.Level;

            if (skillInfo != null && descendData.SynergyRuneList != null)
            {
                for (int j = 0; j < descendData.SynergyRuneList.Count; ++j)
                {
                    if (descendData.SynergyRuneList[j].Procedure <= skillInfo.Level)
                    {
                        MainSkillData mainSkillData = descendData.SynergyRuneList[j].SynergySkillData;
                        if (mainSkillData != null)
                        {
                            SkillBaseData AfterSkill = Managers.SkillManager.Instance.GetSkillBaseData(mainSkillData.MainSkillTypeParam1);
                            if (AfterSkill == null)
                                continue;

                            if (AfterSkill.SkillEffect != null)
                            {
                                for (int e = 0; e < AfterSkill.SkillEffect.Count; ++e)
                                {
                                    SkillEffectData skillEffectData = AfterSkill.SkillEffect[e];

                                    if (skillEffectData.SkillEffectType == V2Enum_SkillEffectType.ReduceDescendExp)
                                    {
                                        double value = skillEffectData.SkillEffectValue + (skillEffectData.SkillEffectIncreasePerLevel * mylevel);
                                        exp -= value;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return exp;
        }
        //------------------------------------------------------------------------------------
        public void EquipDescendContent()
        {
            _changeInGameActiveDescendMsg.ActiveDescend.Clear();

            for (int i = 0; i < _inActiveDescendDatas.Count; ++i)
            {
                DescendData descendData = _inActiveDescendDatas[i];

                SkillInfo skillInfo = GetSynergyEffectSkillInfo(descendData);
                if (skillInfo != null)
                    skillInfo.descend = descendData;

                if (descendData.DescendType == Enum_DescendType.DescendSkill)
                {
                    Managers.BattleSceneManager.Instance.AddGambleSkill(descendData.SynergySkillData, Enum_SynergyType.Max, skillInfo);
                }
                else if (descendData.DescendType == Enum_DescendType.DescendPassive)
                { 
                    Managers.BattleSceneManager.Instance.AddPet(descendData.PetData, skillInfo);
                }


                int mylevel = skillInfo.Level;

                if (skillInfo != null && descendData.SynergyRuneList != null)
                {
                    for (int j = 0; j < descendData.SynergyRuneList.Count; ++j)
                    {
                        if (descendData.SynergyRuneList[j].Procedure <= skillInfo.Level)
                        {
                            MainSkillData mainSkillData = descendData.SynergyRuneList[j].SynergySkillData;
                            if (mainSkillData != null)
                            {
                                if (descendData.DescendType == Enum_DescendType.DescendSkill)
                                    Managers.BattleSceneManager.Instance.AddGambleSkill(mainSkillData, Enum_SynergyType.Max, skillInfo);
                                else if (descendData.DescendType == Enum_DescendType.DescendPassive)
                                    Managers.BattleSceneManager.Instance.AddPetAfterSkill(descendData.PetData, mainSkillData, skillInfo);
                            }
                        }
                    }
                }


                _changeInGameActiveDescendMsg.ActiveDescend.Add(descendData);

                if (_activeDescendDatas.ContainsKey(descendData) == false)
                    _activeDescendDatas.Add(descendData, 0);
            }

            if (_changeInGameActiveDescendMsg.ActiveDescend.Count > 0)
            {
                for (int i = 0; i < _changeInGameActiveDescendMsg.ActiveDescend.Count; ++i)
                {
                    _inActiveDescendDatas.Remove(_changeInGameActiveDescendMsg.ActiveDescend[i]);
                }

                Message.Send(_changeInGameActiveDescendMsg);
            }

            Message.Send(_refreshInGameReadyDescendMsg);
        }
        //------------------------------------------------------------------------------------
        public void ResetDescendState()
        {
            _inActiveDescendDatas.Clear();
            _activeDescendDatas.Clear();
        }
        //------------------------------------------------------------------------------------
        public List<DescendData> GetInActiveDescendDatas()
        {
            return _inActiveDescendDatas;
        }
        //------------------------------------------------------------------------------------
        public void RefreshGambleSynergy()
        {
            //foreach (var pair in GetEquipSynergyEffect())
            //{
            //    DescendData descendData = GetSynergyEffectData(pair.Value);

            //    if (descendData == null)
            //        continue;

            //    if (_activeDescendDatas.ContainsKey(descendData) == false)
            //        continue;

            //    if (IsInGameDescendMaxLevel(descendData) == true)
            //        continue;

            //    int currLevel = GetInGameDescendLevel(descendData);

            //    int synergyCount = GetMySynergyStackCount(descendData);
            //    int level = (int)(synergyCount / descendData.NeedRequiredExp);

            //    if (currLevel < level)
            //    {
            //        for (int i = currLevel + 1; i <= level; ++i)
            //        {
            //            if (descendData.DescendPhaseIndexList.ContainsKey(i) == true)
            //            {
            //                _activeDescendDatas[descendData] = i;
            //                int skillidx = descendData.DescendPhaseIndexList[i];
            //                MainSkillData mainSkillData = SkillManager.Instance.GetMainSkillData(skillidx);
            //                if (mainSkillData == null)
            //                    continue;

            //                if (descendData.DescendType == Enum_DescendType.DescendSkill)
            //                {
            //                    Managers.BattleSceneManager.Instance.AddGambleSkill(mainSkillData);
            //                }
            //                else if (descendData.DescendType == Enum_DescendType.DescendPassive)
            //                    Managers.BattleSceneManager.Instance.AddPetAfterSkill(descendData.PetData, mainSkillData);
            //            }
            //        }
            //    }
            //}

            Message.Send(_refreshInGameReadyDescendMsg);
        }
        //------------------------------------------------------------------------------------
        public bool IsReadySynergyCombineSkill(DescendData gambleSynergyCombineData)
        {
            if (gambleSynergyCombineData == null)
                return false;

            //foreach (var pair in gambleSynergyCombineData.NeedSynergyCount)
            //{
            //    if (Managers.SynergyManager.Instance.GetSynergyStack(pair.Key) < pair.Value)
            //        return false;
            //}

            return true;
        }
        //------------------------------------------------------------------------------------
        public int GetMySynergyStackCount(DescendData gambleSynergyCombineData)
        {
            if (gambleSynergyCombineData == null)
                return 0;

            int count = 0;

            foreach (var pair in gambleSynergyCombineData.SynergyType)
            {
                count += Managers.SynergyManager.Instance.GetSynergyStack(pair);
            }

            return count;
        }
        //------------------------------------------------------------------------------------
        public float GetSynergyCombineSkillReadyRatio(DescendData descendData)
        {
            if (_activeDescendDatas.ContainsKey(descendData) == false)
                return 0.0f;

            if (descendData == null)
                return 0.0f;

            int synergyCount = GetMySynergyStackCount(descendData);
            float curr = (float)(synergyCount % descendData.NeedRequiredExp);

            return curr / (float)descendData.NeedRequiredExp;
        }
        //------------------------------------------------------------------------------------
        public int GetInGameDescendMaxLevel(DescendData descendData)
        {
            if (descendData == null)
                return 0;

            return descendData.DescendPhaseIndexList.Count;
        }
        //------------------------------------------------------------------------------------
        public bool IsInGameDescendMaxLevel(DescendData descendData)
        {
            return GetInGameDescendLevel(descendData) >= GetInGameDescendMaxLevel(descendData);
        }
        //------------------------------------------------------------------------------------
        public int GetInGameDescendLevel(DescendData descendData)
        {
            if (_activeDescendDatas.ContainsKey(descendData) == false)
                return 0;

            return _activeDescendDatas[descendData];
        }
        //------------------------------------------------------------------------------------
        public double GetInGameDescendDamageRatio(DescendData descendData)
        {
            int level = 0;

            if (_activeDescendDatas.ContainsKey(descendData) == true)
                level = _activeDescendDatas[descendData];

            if (DescendContainer.DescendIngameEnforceData == null)
                return 0;

            return DescendContainer.DescendIngameEnforceData.DmgBoost * level;
        }
        //------------------------------------------------------------------------------------
        public double GetInGameDescendEnhanceCost(DescendData descendData)
        {
            int level = 0;

            if (_activeDescendDatas.ContainsKey(descendData) == true)
                level = _activeDescendDatas[descendData];

            if (DescendContainer.DescendIngameEnforceData == null)
                return 99999;

            return DescendContainer.DescendIngameEnforceData.EnforceCostBaseValue 
                + (DescendContainer.DescendIngameEnforceData.EnforceCostLevelValue * level);
        }
        //------------------------------------------------------------------------------------
        public bool CanLevelUp(DescendData descendData)
        {
            double cost = Managers.GoodsManager.Instance.GetGoodsAmount(DescendContainer.DescendIngameEnforceData.EnforceCostGoodsIndex);

            return GetInGameDescendEnhanceCost(descendData) <= cost;
        }
        //------------------------------------------------------------------------------------
        public bool DoInGameLevelUp(DescendData descendData)
        {
            if (CanLevelUp(descendData) == false)
                return false;

            _activeDescendDatas[descendData]++;

            foreach (var pair in descendData.DescendPhaseIndexList)
            {
                int skillidx = pair.Value;
                MainSkillData mainSkillData = SkillManager.Instance.GetMainSkillData(skillidx);
                if (mainSkillData == null)
                    continue;

                SkillInfo skillInfo = GetSynergyEffectSkillInfo(descendData);
                if (skillInfo != null)
                    skillInfo.descend = descendData;

                if (descendData.DescendType == Enum_DescendType.DescendSkill)
                {
                    Managers.BattleSceneManager.Instance.AddGambleSkill(mainSkillData, Enum_SynergyType.Max, skillInfo);
                }
                else if (descendData.DescendType == Enum_DescendType.DescendPassive)
                    Managers.BattleSceneManager.Instance.AddPetAfterSkill(descendData.PetData, mainSkillData, skillInfo);
            }

            Managers.GoodsManager.Instance.UseGoodsAmount(DescendContainer.DescendIngameEnforceData.EnforceCostGoodsIndex, GetInGameDescendEnhanceCost(descendData));

            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}