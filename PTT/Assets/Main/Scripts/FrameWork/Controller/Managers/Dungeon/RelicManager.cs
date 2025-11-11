using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class RelicManager : MonoSingleton<RelicManager>
    {
        private Event.ShowNewRelicMsg _showNewSynergyMsg = new Event.ShowNewRelicMsg();

        private List<string> m_changeInfoUpdate = new List<string>();

        private Dictionary<int, Sprite> _skillIcons = new Dictionary<int, Sprite>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerRelicInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);

            RelicOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitRelicContent()
        {
            string newSynergyIndex = PlayerPrefs.GetString(Define.NewRelicKey);
            if (string.IsNullOrEmpty(newSynergyIndex) == false)
            {
                string[] arr = newSynergyIndex.Split(',');

                for (int i = 0; i < arr.Length; ++i)
                {
                    int index = arr[i].ToInt() + 112010000;
                    RelicData synergyEffectData = GetRelicData(index);
                    if (synergyEffectData == null)
                        continue;

                    AddNewSynergyIcon(synergyEffectData);
                }
            }

            RelicContainer.SynergyAccumLevel = 0;

            foreach (var pair in GetAllRelicData())
            {
                RelicData relicData = pair.Value;
                relicData.SynergySkillData = SkillManager.Instance.GetMainSkillData(relicData.MainSkill);

                SkillInfo skillInfo = GetSynergyEffectSkillInfo(relicData);
                if (skillInfo != null)
                    RelicContainer.SynergyAccumLevel += skillInfo.Level;
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshSynergyRedDot()
        {
            foreach (var pair in GetAllRelicData())
            {
                RelicData synergyEffectData = pair.Value;

                if (ReadySynergyEnhance(synergyEffectData) == true)
                {
                    Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyRelic);
                    break;
                }
            }

            RefreshNewSynergyIcon();
        }
        //------------------------------------------------------------------------------------
        #region Data
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, RelicData> GetAllRelicData()
        {
            return RelicOperator.GetAllRelicData();
        }
        //------------------------------------------------------------------------------------
        public RelicData GetRelicData(ObscuredInt index)
        {
            return RelicOperator.GetRelicData(index);
        }
        //------------------------------------------------------------------------------------
        public RelicLevelUpCostData GetRelicLevelUpCostData(ObscuredInt level)
        {
            return RelicOperator.GetRelicLevelUpCostData(level);
        }
        //------------------------------------------------------------------------------------
        public SkillInfo GetSynergyEffectSkillInfo(RelicData synergyEffectData)
        {
            if (synergyEffectData == null)
                return null;

            if (RelicContainer.SynergyInfo.ContainsKey(synergyEffectData.Index) == true)
                return RelicContainer.SynergyInfo[synergyEffectData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SkillInfo AddNewSkillInfo(RelicData synergyEffectData)
        {
            if (synergyEffectData == null)
                return null;

            SkillInfo skillInfo = new SkillInfo();
            skillInfo.Id = synergyEffectData.Index;
            skillInfo.Level = Define.PlayerSkillDefaultLevel;
            skillInfo.Count = 0;

            RelicContainer.SynergyInfo.Add(skillInfo.Id, skillInfo);


            return skillInfo;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetRelicIcon(RelicData skillBaseData)
        {
            if (skillBaseData == null)
                return null;

            return GetIcon(skillBaseData.ResourceIndex);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetRelicIcon(int index)
        {
            return GetRelicIcon(GetRelicData(index));
        }
        //------------------------------------------------------------------------------------
        private Sprite GetIcon(int iconIndex)
        {
            Sprite sp = null;

            if (_skillIcons.ContainsKey(iconIndex) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(Define.RelicIconPath, iconIndex), o =>
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
            SetSynergyAmount(GetRelicData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyAmount(RelicData synergyEffectData, int amount)
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
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyRelic);
            }
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAmount(int index)
        {
            return GetSynergyAmount(GetRelicData(index));
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAmount(RelicData synergyEffectData)
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
            return AddSynergyAmount(GetRelicData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int AddSynergyAmount(RelicData synergyEffectData, int amount)
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
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyRelic);
            }

            return playerSkillInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyAmount(int index, int amount)
        {
            return UseSynergyAmount(GetRelicData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyAmount(RelicData synergyEffectData, int amount)
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
            RelicData synergyEffectData = GetRelicData(index);
            if (synergyEffectData != null)
            {
                return synergyEffectData.NameLocalKey;
            }

            return string.Empty;
        }
        //------------------------------------------------------------------------------------

        public void AddNewSynergyIcon(RelicData synergyEffectData)
        {
            if (synergyEffectData != null)
            {
                if (RelicContainer.NewSynergys.ContainsKey(synergyEffectData) == false)
                    RelicContainer.NewSynergys.Add(synergyEffectData, 1);
                else
                    RelicContainer.NewSynergys[synergyEffectData] += 1;

                PlayerPrefs.SetString(Define.NewRelicKey, RelicContainer.GetNewSynergySerializeString());
            }

            Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyRelic);

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
            int count = RelicContainer.NewSynergys.Count;
            return count;
        }
        //------------------------------------------------------------------------------------
        public bool IsNewSynergyIcon(RelicData synergyEffectData)
        {
            return RelicContainer.NewSynergys.ContainsKey(synergyEffectData);
        }
        //------------------------------------------------------------------------------------
        public void RemoveNewIconSynergy(RelicData synergyEffectData)
        {
            if (synergyEffectData != null)
            {
                if (RelicContainer.NewSynergys.ContainsKey(synergyEffectData) == true)
                    RelicContainer.NewSynergys.Remove(synergyEffectData);

                PlayerPrefs.SetString(Define.NewRelicKey, RelicContainer.GetNewSynergySerializeString());
            }
        }
        //------------------------------------------------------------------------------------
        public void RemoveAllNewIconSynergy()
        {
            RelicContainer.NewSynergys.Clear();

            PlayerPrefs.SetString(Define.NewRelicKey, RelicContainer.GetNewSynergySerializeString());
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region LobbyRelic
        //------------------------------------------------------------------------------------
        public bool IsMaxLevelSynergy(RelicData synergyEffectData)
        {
            if (synergyEffectData == null)
                return false;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return false;

            RelicLevelUpCostData synergyLevelUpCostData = GetRelicLevelUpCostData(skillInfo.Level);

            if (synergyLevelUpCostData == null)
                return false;

            return synergyLevelUpCostData.LevelUpCostGoodsParam1 <= -1;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhance_NeedCount(RelicData synergyEffectData)
        {
            if (synergyEffectData == null)
                return 99999;


            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);

            int level = skillInfo == null ? 0 : skillInfo.Level;

            RelicLevelUpCostData synergyLevelUpCostData = GetRelicLevelUpCostData(skillInfo.Level);

            if (synergyLevelUpCostData == null)
                return 99999;


            return synergyLevelUpCostData.LevelUpCostGoodsParam1;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhanceCostGoodsIndex(RelicData synergyEffectData)
        {
            if (synergyEffectData == null)
                return -1;

            return synergyEffectData.Index;
        }
        //------------------------------------------------------------------------------------
        public bool ReadySynergyEnhance(RelicData synergyEffectData)
        {
            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);

            if (skillInfo == null)
                return false;

            return skillInfo.Count >= GetSynergyEnhance_NeedCount(synergyEffectData);
        }
        //------------------------------------------------------------------------------------
        public bool EnhanceSynergy(RelicData synergyEffectData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (IsMaxLevelSynergy(synergyEffectData) == true)
                return false;

            if (ReadySynergyEnhance(synergyEffectData) == false)
                return false;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            skillInfo.Count -= GetSynergyEnhance_NeedCount(synergyEffectData);
            skillInfo.Level += 1;

            RelicContainer.SynergyAccumLevel += 1;

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_log_artifact_enforce(synergyEffectData.Index, skillInfo.Level);

            ARRRStatManager.Instance.RefreshBattlePower();

            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region InGameRelic
        //------------------------------------------------------------------------------------
        public void SetInGameRelicData()
        {
            foreach (var pair in RelicContainer.SynergyInfo)
            {
                RelicData descendData = GetRelicData(pair.Key);

                if (descendData != null)
                {
                    SkillInfo skillInfo = pair.Value;
                    if (skillInfo == null)
                        continue;

                    Managers.BattleSceneManager.Instance.AddGambleSkill(descendData.SynergySkillData, V2Enum_ARR_SynergyType.Max, skillInfo);
                }
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}