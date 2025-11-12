using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using System.Collections.Concurrent;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class ARRRSkillManager : MonoSingleton<ARRRSkillManager>
    {
        private CharacterLocalTable _characterLocalTable;
        private ObscuredInt _skillSlotCount = 4;

        private SkillBaseData _characterSkillData_BasicAttack;

        private List<string> _changeInfoUpdate = new List<string>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _changeInfoUpdate.Add(Define.PlayerSkillInfoTable);
            _changeInfoUpdate.Add(Define.PlayerPointTable);


            _characterLocalTable = TableManager.Instance.GetTableClass<CharacterLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public void InitARRRSkillContent()
        {
            Dictionary<ObscuredInt, ARRRSkillData> ARRRSkillDatas = _characterLocalTable.GetARRRSkillLinkDatas();

            List<ObscuredInt> defaultSkillList = new List<ObscuredInt>();

            if (ARRRSkillContainer._skillInfo.Count == 0)
            {
                //for (int i = 0; i < Define.NewCharacterSkill.Count; ++i)
                //{
                //    ARRRSkillData aRRRSkillData = GetARRRSkillData(Define.NewCharacterSkill[i]);
                //    if (aRRRSkillData == null)
                //        continue;

                //    SkillInfo skillInfo = AddNewPlayerSkillInfo(aRRRSkillData);
                //    skillInfo.Count = 1;

                //    if (ARRRSkillContainer._skillTempSlotData.Count < _skillSlotCount)
                //    {
                //        if (Define.NewCharacterEquipSkill.Contains(Define.NewCharacterSkill[i]))
                //            ARRRSkillContainer._skillTempSlotData.Add(Define.NewCharacterSkill[i]);
                //    }
                //}
            }



            foreach (var pair in ARRRSkillDatas)
            {
                ARRRSkillData aRRRSkillData = pair.Value;

                SkillBaseData skillBaseData = GetSkillBaseData(aRRRSkillData);

                if (skillBaseData != null && skillBaseData.TriggerType == Enum_TriggerType.Default)
                {
                    _characterSkillData_BasicAttack = skillBaseData;
                    defaultSkillList.Add(aRRRSkillData.Index);
                }

                continue;
            }

            //foreach (var pair in ARRRSkillDatas)
            //{
            //    ARRRSkillData aRRRSkillData = pair.Value;

            //    SkillBaseData skillBaseData = GetSkillBaseData(aRRRSkillData);

            //    if (skillBaseData != null && skillBaseData.TriggerType == Enum_TriggerType.Default)
            //    {
            //        _characterSkillData_BasicAttack = skillBaseData;
            //        defaultSkillList.Add(aRRRSkillData.Index);
            //        continue;
            //    }


            //    if (ARRRSkillContainer._skillInfo.ContainsKey(aRRRSkillData.Index) == false)
            //    {
            //        SkillInfo skillInfo = AddNewPlayerSkillInfo(aRRRSkillData);
            //        skillInfo.Count = 1;
            //    }
            //}

            List<ARRRSkillData> skilllist = GetARRRSkillDatas();

            for (int i = 0; i < defaultSkillList.Count; ++i)
            {
                if (ARRRSkillDatas.ContainsKey(defaultSkillList[i]))
                {
                    ARRRSkillData aRRRSkillData = ARRRSkillDatas[defaultSkillList[i]];
                    ARRRSkillDatas.Remove(defaultSkillList[i]);
                    skilllist.Remove(aRRRSkillData);
                }
            }


            //for (int i = 0; i < ARRRSkillContainer._skillTempSlotData.Count; ++i)
            //{
            //    ARRRSkillData aRRRSkillData = null;

            //    if (ARRRSkillContainer._skillTempSlotData[i] != -1)
            //    {
            //        aRRRSkillData = GetARRRSkillData(ARRRSkillContainer._skillTempSlotData[i]);
            //    }

            //    ARRRSkillContainer._skillSlotData.Add(i, aRRRSkillData);
            //}

            for (int i = 0; i < _skillSlotCount; ++i)
            {
                if (ARRRSkillContainer._skillSlotData.ContainsKey(i) == false)
                    ARRRSkillContainer._skillSlotData.Add(i, null);
            }

            // 스킬 장착된거 없게 수정
            //ARRRSkillContainer._skillSlotData.Clear();

        }
        //------------------------------------------------------------------------------------
        public int GetSkillSlotMaxCount()
        {
            return _skillSlotCount;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, ARRRSkillData> GetEquipARRRSkillSlot()
        {
            return ARRRSkillContainer._skillSlotData;
        }
        //------------------------------------------------------------------------------------
        public void SweepSkillSlot(int from, int to)
        {
            ARRRSkillData fromeSkill = null;
            ARRRSkillData toSkill = null;

            if (ARRRSkillContainer._skillSlotData.ContainsKey(from) == false
                || ARRRSkillContainer._skillSlotData.ContainsKey(to) == false)
                return;

            fromeSkill = ARRRSkillContainer._skillSlotData[from];
            toSkill = ARRRSkillContainer._skillSlotData[to];

            ARRRSkillContainer._skillSlotData[from] = toSkill;
            ARRRSkillContainer._skillSlotData[to] = fromeSkill;
        }
        //------------------------------------------------------------------------------------
        public ARRRSkillData UnEquipSkillSlot(int slotIndex)
        {
            if (ARRRSkillContainer._skillSlotData.ContainsKey(slotIndex) == false)
                return null;

            ARRRSkillData aRRRSkillData = ARRRSkillContainer._skillSlotData[slotIndex];

            ARRRSkillContainer._skillSlotData[slotIndex] = null;

            return aRRRSkillData;
        }
        //------------------------------------------------------------------------------------
        public bool UnEquipSkillSlot(ARRRSkillData aRRRSkillData)
        {
            int slotIndex = -1;

            foreach (var pair in ARRRSkillContainer._skillSlotData)
            {
                if (pair.Value == aRRRSkillData)
                {
                    slotIndex = pair.Key;
                }
            }

            if (slotIndex != -1)
            {
                ARRRSkillContainer._skillSlotData[slotIndex] = null;
                return true;
            }
                
            return false;
        }
        //------------------------------------------------------------------------------------
        private SkillInfo AddNewPlayerSkillInfo(ARRRSkillData characterSkillData)
        {
            if (ARRRSkillContainer._skillInfo.ContainsKey(characterSkillData.Index) == true)
                return ARRRSkillContainer._skillInfo[characterSkillData.Index];
            else
            {
                SkillInfo playerSkillInfo = new SkillInfo();
                playerSkillInfo.Id = characterSkillData.Index;
                playerSkillInfo.Count = 0;
                playerSkillInfo.Level = Define.PlayerSkillDefaultLevel;
                ARRRSkillContainer._skillInfo.Add(playerSkillInfo.Id, playerSkillInfo);

                return playerSkillInfo;
            }
        }
        //------------------------------------------------------------------------------------
        public List<ARRRSkillData> GetARRRSkillDatas()
        {
            return _characterLocalTable.GetARRRSkillDatas();
        }
        //------------------------------------------------------------------------------------
        public int SortARRRSkillData(ARRRSkillData x, ARRRSkillData y)
        {
            SkillInfo xinfo = GetARRRSkillInfo(x);
            SkillInfo yinfo = GetARRRSkillInfo(y);

            if (xinfo != null && yinfo == null)
                return -1;
            else if (xinfo == null && yinfo != null)
                return 1;

            if (x.Grade > y.Grade)
                return -1;
            else if (x.Grade < y.Grade)
                return 1;
            else
            {
                if (x.Index < y.Index)
                    return -1;
                else if (x.Index > y.Index)
                    return 1;
                //else
                //{
                //    if (xinfo.Level > yinfo.Level)
                //        return -1;
                //    else if (xinfo.Level < yinfo.Level)
                //        return 1;
                //}
            }

            return 0;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, ARRRSkillData> GetARRRSkillLinkDatas()
        {
            return _characterLocalTable.GetARRRSkillLinkDatas();
        }
        //------------------------------------------------------------------------------------
        public bool IsFullEquipSkillSlot()
        {
            int slotidx = -1;

            foreach (var pair in ARRRSkillContainer._skillSlotData)
            {
                if (pair.Value == null)
                {
                    slotidx = pair.Key;
                    break;
                }
            }

            return slotidx == -1;
        }
        //------------------------------------------------------------------------------------
        public bool IsEquipSkill(ARRRSkillData aRRRSkillData)
        {
            if (aRRRSkillData == null)
                return false;

            return ARRRSkillContainer._skillSlotData.ContainsValue(aRRRSkillData);
        }
        //------------------------------------------------------------------------------------
        public bool EquipSkill(ARRRSkillData aRRRSkillData)
        {
            if (aRRRSkillData == null)
                return false;

            if (IsEquipSkill(aRRRSkillData) == true)
                return false;

            int slotidx = -1;

            foreach (var pair in ARRRSkillContainer._skillSlotData)
            {
                if (pair.Value == null)
                { 
                    slotidx = pair.Key;
                    break;
                }
            }

            if (slotidx == -1)
                return false;

            ARRRSkillContainer._skillSlotData[slotidx] = aRRRSkillData;

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSkillInfoTable);

            return true;
        }
        //------------------------------------------------------------------------------------
        public SkillBaseData GetBasicAttackSkillData()
        {
            return _characterSkillData_BasicAttack;
        }
        //------------------------------------------------------------------------------------
        public ARRRSkillData GetARRRSkillData(ObscuredInt index)
        {
            return _characterLocalTable.GetARRRSkillData(index);
        }
        //------------------------------------------------------------------------------------
        public ARRRSkillData GetARRRSkillData_SkillBaseDataIndex(ObscuredInt index)
        {
            return _characterLocalTable.GetARRRSkillData_SkillBaseDataIndex(index);
        }
        //------------------------------------------------------------------------------------
        public SkillBaseData GetSkillBaseData(ARRRSkillData aRRRSkillData)
        {
            if (aRRRSkillData == null)
                return null;

            return SkillManager.Instance.GetSkillBaseData(aRRRSkillData.SkillIndex);
        }
        //------------------------------------------------------------------------------------
        public SkillBaseData ConvertSkillInfoToSkillBaseData(SkillInfo skillInfo)
        {
            ARRRSkillData aRRRSkillData = GetARRRSkillData(skillInfo.Id);
            if (aRRRSkillData == null)
                return null;

            SkillBaseData skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(aRRRSkillData.SkillIndex);

            return skillBaseData;
        }
        //------------------------------------------------------------------------------------
        public void GetARRREquipSkill(ref List<SkillInfo> skillInfos)
        {
            skillInfos.Clear();
            
            foreach (var pair in ARRRSkillContainer._skillSlotData)
            {
                if (pair.Value == null)
                    continue;

                SkillInfo skillInfo = null;

                if (ARRRSkillContainer._skillInfo.ContainsKey(pair.Value.Index) == true)
                    skillInfo = ARRRSkillContainer._skillInfo[pair.Value.Index];

                skillInfos.Add(skillInfo);
            }
        }
        //------------------------------------------------------------------------------------
        public Sprite GetARRRSKillSprite(int index)
        {
            ARRRSkillData aRRRSkillData = GetARRRSkillData(index);

            if (aRRRSkillData == null)
                return null;

            SkillBaseData skillBaseData = GetSkillBaseData(aRRRSkillData);
            if (skillBaseData == null)
                return null;

            return SkillManager.Instance.GetSkillIcon(skillBaseData);
        }
        //------------------------------------------------------------------------------------
        public string GetARRRSkillLocalKey(int index)
        {
            ARRRSkillData aRRRSkillData = GetARRRSkillData(index);

            if (aRRRSkillData == null)
                return string.Empty;

            SkillBaseData skillBaseData = GetSkillBaseData(aRRRSkillData);
            if (skillBaseData == null)
                return string.Empty;

            return skillBaseData.NameLocalKey;
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetARRRSkillGrade(int index)
        {
            ARRRSkillData aRRRSkillData = GetARRRSkillData(index);

            if (aRRRSkillData == null)
                return V2Enum_Grade.D;

            return aRRRSkillData.Grade;
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetARRRSkillGrade(SkillBaseData skillBaseData)
        {
            if (skillBaseData == null)
                return V2Enum_Grade.Max;

            ARRRSkillData aRRRSkillData = GetARRRSkillData_SkillBaseDataIndex(skillBaseData.Index);
            if(aRRRSkillData == null)
                return V2Enum_Grade.Max;

            return aRRRSkillData.Grade;
        }
        //------------------------------------------------------------------------------------
        public SkillInfo GetARRRSkillInfo(SkillBaseData skillBaseData)
        {
            if (skillBaseData == null)
                return null;

            ARRRSkillData aRRRSkillData = GetARRRSkillData_SkillBaseDataIndex(skillBaseData.Index);

            if (ARRRSkillContainer._skillInfo.ContainsKey(aRRRSkillData.Index) == true)
                return ARRRSkillContainer._skillInfo[aRRRSkillData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SkillInfo GetARRRSkillInfo(ARRRSkillData aRRRSkillData)
        {
            if (aRRRSkillData == null)
                return null;

            if (ARRRSkillContainer._skillInfo.ContainsKey(aRRRSkillData.Index) == true)
                return ARRRSkillContainer._skillInfo[aRRRSkillData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public int GetSkillLevel(SkillBaseData skillBaseData)
        {
            if (skillBaseData == null)
                return 1;

            SkillInfo skillInfo = GetARRRSkillInfo(skillBaseData);

            if (skillInfo == null)
                return 1;

            return skillInfo.Level;
        }
        //------------------------------------------------------------------------------------
        public void SetSkillAmount(int index, int amount)
        {
            SetSkillAmount(GetARRRSkillData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public void SetSkillAmount(ARRRSkillData aRRRSkillData, int amount)
        {
            if (aRRRSkillData == null)
                return;

            SkillInfo playerSkillInfo = GetARRRSkillInfo(aRRRSkillData);
            if (playerSkillInfo == null)
                playerSkillInfo = AddNewPlayerSkillInfo(aRRRSkillData);

            playerSkillInfo.Count = amount;

            //if (CheckIsAlreadyEnhance(characterSkillData) == true
            //            || CheckIsAlreadyCombine(characterSkillData) == true)
            //    ShowGearRedDot(characterSkillData.TriggerType);
        }
        //------------------------------------------------------------------------------------
        public int GetSkillAmount(int index)
        {
            return GetSkillAmount(GetARRRSkillData(index));
        }
        //------------------------------------------------------------------------------------
        public int GetSkillAmount(ARRRSkillData aRRRSkillData)
        {
            if (aRRRSkillData == null)
                return 0;

            SkillInfo playerSkillInfo = GetARRRSkillInfo(aRRRSkillData);
            if (playerSkillInfo == null)
                return 0;
            else
                return playerSkillInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public int AddSkillAmount(int index, int amount)
        {
            return AddSkillAmount(GetARRRSkillData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int AddSkillAmount(ARRRSkillData aRRRSkillData, int amount)
        {
            if (aRRRSkillData == null)
                return 0;

            SkillInfo playerSkillInfo = GetARRRSkillInfo(aRRRSkillData);
            if (playerSkillInfo == null)
                playerSkillInfo = AddNewPlayerSkillInfo(aRRRSkillData);

            playerSkillInfo.Count += amount;

            return playerSkillInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public int UseSkillAmount(int index, int amount)
        {
            return UseSkillAmount(GetARRRSkillData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int UseSkillAmount(ARRRSkillData aRRRSkillData, int amount)
        {
            if (aRRRSkillData == null)
                return 0;

            SkillInfo playerSkillInfo = GetARRRSkillInfo(aRRRSkillData);
            if (playerSkillInfo == null)
                return 0;

            playerSkillInfo.Count -= amount;

            if (playerSkillInfo.Count < 0)
                playerSkillInfo.Count = 0;

            return playerSkillInfo.Count;
        }
        //------------------------------------------------------------------------------------
        #region LevelUp
        //------------------------------------------------------------------------------------
        public int GetLevelUpCost(ARRRSkillData aRRRSkillData)
        {
            if (aRRRSkillData == null)
                return 9999999;

            LevelUpCostData levelUpCostData = _characterLocalTable.GetPetLevelUpCostData(aRRRSkillData.Grade);
            if (levelUpCostData == null)
                return 9999999;

            return levelUpCostData.LevelUpCostCount;
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxLevel(ARRRSkillData aRRRSkillData)
        {
            SkillInfo skillInfo = GetARRRSkillInfo(aRRRSkillData);
            if (skillInfo == null)
                return false;

            LevelUpCostData levelUpCostData = _characterLocalTable.GetPetLevelUpCostData(aRRRSkillData.Grade);
            if (levelUpCostData == null)
                return false;

            return levelUpCostData.MaximumLevel <= skillInfo.Level;
        }
        //------------------------------------------------------------------------------------
        public bool CanLevelUp(ARRRSkillData aRRRSkillData)
        {
            if (IsMaxLevel(aRRRSkillData) == true)
                return false;

            SkillInfo skillInfo = GetARRRSkillInfo(aRRRSkillData);
            if (skillInfo == null)
                return false;

            LevelUpCostData levelUpCostData = _characterLocalTable.GetPetLevelUpCostData(aRRRSkillData.Grade);
            if (levelUpCostData == null)
                return false;

            return levelUpCostData.LevelUpCostCount <= skillInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public bool DoLevelUp(ARRRSkillData aRRRSkillData)
        {
            if (CanLevelUp(aRRRSkillData) == false)
                return false;

            if (IsMaxLevel(aRRRSkillData) == true)
                return false;

            SkillInfo skillInfo = GetARRRSkillInfo(aRRRSkillData);
            if (skillInfo == null)
                return false;

            LevelUpCostData levelUpCostData = _characterLocalTable.GetPetLevelUpCostData(aRRRSkillData.Grade);
            if (levelUpCostData == null)
                return false;

            skillInfo.Count -= levelUpCostData.LevelUpCostCount;
            skillInfo.Level += 1;

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(_changeInfoUpdate, null);

            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------





    }
}