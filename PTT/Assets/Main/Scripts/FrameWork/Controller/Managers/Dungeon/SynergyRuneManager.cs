using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class SynergyRuneManager : MonoSingleton<SynergyRuneManager>
    {

        private Event.ChangeEquipStateSynergyRuneMsg _changeEquipStateSynergyRuneMsg = new Event.ChangeEquipStateSynergyRuneMsg();
        private Event.ShowNewSynergyRuneMsg _showNewSynergyMsg = new Event.ShowNewSynergyRuneMsg();

        private Dictionary<int, Sprite> _skillIcons = new Dictionary<int, Sprite>();

        public Dictionary<ObscuredInt, List<MainSkillData>> _targetAfterSkill = new Dictionary<ObscuredInt, List<MainSkillData>>();

        private Dictionary<SynergyRuneData, ObscuredInt> _materials = new Dictionary<SynergyRuneData, ObscuredInt>();

        private Dictionary<SynergyRuneData, int> _materialcount = new Dictionary<SynergyRuneData, int>();

        private ObscuredInt _synergyRuneSlotCount = 4;

        private List<string> m_changeInfoUpdate = new List<string>();

        private SkillInfo fakeRuneLevelInfo = new SkillInfo();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerSynergyRuneInfoTable);

            SynergyRuneOperator.Init();
        }
        //------------------------------------------------------------------------------------

        public void InitSynergyRuneContent()
        {
            string newSynergyIndex = PlayerPrefs.GetString(Define.NewSynergyRuneKey);
            if (string.IsNullOrEmpty(newSynergyIndex) == false)
            {
                string[] arr = newSynergyIndex.Split(',');

                for (int i = 0; i < arr.Length; ++i)
                {
                    int index = arr[i].ToInt() + 118010000;
                    SynergyRuneData synergyEffectData = GetSynergyEffectData(index);
                    if (synergyEffectData == null)
                        continue;

                    AddNewSynergyIcon(synergyEffectData);
                }
            }

            if (SynergyRuneContainer.SynergyEquip_Dic.Count == 0)
            {
                //for (int synergy = V2Enum_ARR_SynergyType.Red.Enum32ToInt(); synergy < V2Enum_ARR_SynergyType.Max.Enum32ToInt(); ++synergy)
                //{
                //    V2Enum_ARR_SynergyType v2Enum_Stat = synergy.IntToEnum32<V2Enum_ARR_SynergyType>();
                //    SynergyRuneContainer.SynergyEquip_Dic.Add(v2Enum_Stat, new Dictionary<ObscuredInt, ObscuredInt>());

                //    for (int slot = 1; slot <= 8; ++slot)
                //    { 
                //        SynergyRuneContainer.SynergyEquip_Dic[v2Enum_Stat].Add(slot, -1);
                //    }
                //}

                for (int slot = 1; slot <= 8; ++slot)
                {
                    SynergyRuneContainer.SynergyEquip_Dic.Add(slot, -1);
                }
            }

            foreach (var pair in GetAllSynergyRuneData_Dic())
            {
                SynergyRuneData synergyEffectData = pair.Value;

                synergyEffectData.SynergySkillData = SkillManager.Instance.GetMainSkillData(synergyEffectData.MainSkillIndex);
            }

            Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.StackSkillCount);
        }
        //------------------------------------------------------------------------------------
        #region SynergyData
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, SynergyRuneData> GetAllSynergyRuneData_Dic()
        {
            return SynergyRuneOperator.GetAllSynergyRuneData_Dic();
        }
        //------------------------------------------------------------------------------------
        public List<SynergyRuneData> GetAllSynergyRuneData()
        {
            return SynergyRuneOperator.GetAllSynergyRuneData();
        }
        //------------------------------------------------------------------------------------
        public List<SynergyRuneData> GetAllSynergyRuneData(V2Enum_Grade v2Enum_Grade)
        {
            return SynergyRuneOperator.GetAllSynergyRuneData(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public int SortRuneData(SynergyRuneData x, SynergyRuneData y)
        {
            if (x.Grade > y.Grade)
                return -1;
            else if (x.Grade < y.Grade)
                return 1;
            else
            {

                if (x.SynergyType < y.SynergyType)
                    return -1;
                else if (x.SynergyType > y.SynergyType)
                    return 1;

                if (x.Index > y.Index)
                    return -1;
                else if (x.Index < y.Index)
                    return 1;
            }

            return 0;
        }
        //------------------------------------------------------------------------------------
        public SynergyRuneData GetSynergyEffectData(ObscuredInt index)
        {
            return SynergyRuneOperator.GetSynergyRuneData(index);
        }
        //------------------------------------------------------------------------------------
        public SynergyRuneOpenconditionData GetSynergyRuneOpenconditionData(ObscuredInt slotNumber)
        {
            return SynergyRuneOperator.GetSynergyRuneOpenconditionData(slotNumber);
        }
        //------------------------------------------------------------------------------------
        public SynergyRuneCombineData GetSynergyRuneCombineData(V2Enum_Grade grade)
        {
            return SynergyRuneOperator.GetSynergyRuneCombineData(grade);
        }
        //------------------------------------------------------------------------------------
        public SkillInfo GetSynergyEffectSkillInfo(ObscuredInt index)
        {
            return GetSynergyEffectSkillInfo(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        public SkillInfo GetSynergyEffectSkillInfo(SynergyRuneData synergyEffectData)
        {
            if (synergyEffectData == null)
                return null;

            if (SynergyRuneContainer.SynergyInfo.ContainsKey(synergyEffectData.Index) == true)
                return SynergyRuneContainer.SynergyInfo[synergyEffectData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SkillInfo AddNewSkillInfo(SynergyRuneData synergyEffectData)
        {
            if (synergyEffectData == null)
                return null;

            SkillInfo skillInfo = new SkillInfo();
            skillInfo.Id = synergyEffectData.Index;
            skillInfo.Level = Define.PlayerSkillDefaultLevel;
            skillInfo.Count = 0;

            SynergyRuneContainer.SynergyInfo.Add(skillInfo.Id, skillInfo);


            return skillInfo;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, ObscuredInt> GetEquipSynergyEffect()
        {
            return SynergyRuneContainer.SynergyEquip_Dic;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetDescendIcon(SynergyRuneData skillBaseData)
        {
            if (skillBaseData == null)
                return null;

            return GetIcon(skillBaseData.ResourceIndex);
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
                ResourceLoader.Instance.Load<Sprite>(string.Format(Define.SynergyRunePath, iconIndex), o =>
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
        public void SetSynergyAmount(SynergyRuneData synergyEffectData, int amount)
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
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbySynergyRune);
            }
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAmount(int index)
        {
            return GetSynergyAmount(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAmount(SynergyRuneData synergyEffectData)
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
        public int AddSynergyAmount(SynergyRuneData synergyEffectData, int amount)
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
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbySynergyRune);
            }

            return playerSkillInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyAmount(int index, int amount)
        {
            return UseSynergyAmount(GetSynergyEffectData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyAmount(SynergyRuneData synergyEffectData, int amount)
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
            SynergyRuneData synergyEffectData = GetSynergyEffectData(index);
            if (synergyEffectData != null)
            {
                return synergyEffectData.NameLocalKey;
            }

            return string.Empty;
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetSynergyGrade(int index)
        {
            return GetSynergyGrade(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetSynergyGrade(SynergyRuneData synergyEffectData)
        {
            if (synergyEffectData == null)
                return V2Enum_Grade.Max;

            if (synergyEffectData.SynergySkillData == null)
                return V2Enum_Grade.Max;

            return synergyEffectData.Grade;
        }
        //------------------------------------------------------------------------------------
        public void AddNewSynergyIcon(SynergyRuneData synergyEffectData)
        {
            if (synergyEffectData != null)
            {
                if (SynergyRuneContainer.NewSynergys.ContainsKey(synergyEffectData) == false)
                    SynergyRuneContainer.NewSynergys.Add(synergyEffectData, 1);
                else
                    SynergyRuneContainer.NewSynergys[synergyEffectData] += 1;

                PlayerPrefs.SetString(Define.NewSynergyRuneKey, SynergyRuneContainer.GetNewSynergySerializeString());

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
            int count = SynergyRuneContainer.NewSynergys.Count;
            return count;
        }
        //------------------------------------------------------------------------------------
        public bool IsNewSynergyIcon(SynergyRuneData synergyEffectData)
        {
            return SynergyRuneContainer.NewSynergys.ContainsKey(synergyEffectData);
        }
        //------------------------------------------------------------------------------------
        public void RemoveNewIconSynergy(SynergyRuneData synergyEffectData)
        {
            if (synergyEffectData != null)
            {
                if (SynergyRuneContainer.NewSynergys.ContainsKey(synergyEffectData) == true)
                    SynergyRuneContainer.NewSynergys.Remove(synergyEffectData);

                PlayerPrefs.SetString(Define.NewSynergyRuneKey, SynergyRuneContainer.GetNewSynergySerializeString());
            }
        }
        //------------------------------------------------------------------------------------
        public void RemoveAllNewIconSynergy()
        {
            SynergyRuneContainer.NewSynergys.Clear();

            PlayerPrefs.SetString(Define.NewSynergyRuneKey, SynergyRuneContainer.GetNewSynergySerializeString());
        }
        //------------------------------------------------------------------------------------
        public int GetDisplayEquipRuneCount(SynergyRuneData synergyEffectData)
        {
            // 장착한거, 합성중인 카운트 빼고
            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return 0;

            int displaycount = skillInfo.Count;

            int equipcount = 0;

            foreach (var pair in SynergyRuneContainer.SynergyEquip_Dic)
            {
                if (pair.Value == synergyEffectData.Index)
                {
                    equipcount++;
                }
            }

            displaycount -= equipcount;


            if (_materials.ContainsKey(synergyEffectData) == true)
                displaycount -= _materials[synergyEffectData];

            return displaycount;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region SynergyRuneCombine
        //------------------------------------------------------------------------------------
        public bool ReadySynergyEnhance(SynergyRuneData synergyEffectData)
        {
            if (synergyEffectData == null)
                return false;

            SynergyRuneCombineData synergyRuneCombineData = GetSynergyRuneCombineData(synergyEffectData.Grade);
            if (synergyRuneCombineData == null)
                return false;

            List<SynergyRuneData> synergyRuneDatas = GetAllSynergyRuneData(synergyEffectData.Grade);
            if (synergyRuneDatas == null)
                return false;
            
            int count = 0;

            for (int i = 0; i < synergyRuneDatas.Count; ++i)
            {
                count += GetDisplayEquipRuneCount(synergyRuneDatas[i]);
            }

            return count >= synergyRuneCombineData.RequiredCount;
        }
        //------------------------------------------------------------------------------------
        public int NeedMaterialCount(V2Enum_Grade v2Enum_Grade)
        {
            SynergyRuneCombineData synergyRuneCombineData = GetSynergyRuneCombineData(v2Enum_Grade);
            if (synergyRuneCombineData == null)
                return 99999;

            return synergyRuneCombineData.RequiredCount;
        }
        //------------------------------------------------------------------------------------
        public void AddMaterial(SynergyRuneData synergyRuneData)
        {
            if (_materials.ContainsKey(synergyRuneData) == false)
                _materials.Add(synergyRuneData, 0);

            _materials[synergyRuneData] += 1;
        }
        //------------------------------------------------------------------------------------
        public void RemoveMaterial(SynergyRuneData synergyRuneData)
        {
            if (_materials.ContainsKey(synergyRuneData) == false)
                return;

            if (_materials[synergyRuneData] <= 0)
                return;

            _materials[synergyRuneData] -= 1;
        }
        //------------------------------------------------------------------------------------
        public int GetCurrentMaterialCount()
        {
            int materialcount = 0;

            foreach (var pair in _materials)
            {
                materialcount += pair.Value;
            }

            return materialcount;
        }
        //------------------------------------------------------------------------------------
        public bool CanCombine()
        {
            for (int i = 0; i < V2Enum_Grade.Max.Enum32ToInt(); ++i)
            {
                V2Enum_Grade grade = i.IntToEnum32<V2Enum_Grade>();

                SynergyRuneCombineData synergyRuneCombineData = GetSynergyRuneCombineData(grade);
                if (synergyRuneCombineData == null)
                {
                    continue;
                }

                List<SynergyRuneData> mat = GetAllSynergyRuneData().FindAll(
                        x => GetDisplayEquipRuneCount(x) > 0
                        && x.Grade == grade);

                if (mat.Count >= synergyRuneCombineData.RequiredCount)
                    return true;
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public SynergyRuneData DoRuneCombine(SynergyRuneCombineData synergyRuneCombineData, List<SynergyRuneData> materialRunes)
        {
            if (synergyRuneCombineData == null)
                return null;

            if (materialRunes == null)
                return null;

            if (materialRunes.Count < synergyRuneCombineData.RequiredCount)
                return null;

            _materialcount.Clear();

            List<int> materialidx = new List<int>();


            for (int i = 0; i < synergyRuneCombineData.RequiredCount; ++i)
            {
                if (materialRunes[i] == null)
                    return null;

                if (_materialcount.ContainsKey(materialRunes[i]) == false)
                    _materialcount.Add(materialRunes[i], 0);

                _materialcount[materialRunes[i]] += 1;

                materialidx.Add(materialRunes[i].Index);

            }

            foreach (var pair in _materialcount)
            {
                if (_materials.ContainsKey(pair.Key) == false)
                    return null;

                if (_materials[pair.Key] < pair.Value)
                    return null;

                SkillInfo matinfo = GetSynergyEffectSkillInfo(pair.Key);
                if (matinfo == null)
                    return null;

                if (matinfo.Count < pair.Value)
                    return null;
            }

            int randomrange = UnityEngine.Random.Range(1, 101);

            bool success = synergyRuneCombineData.SuccessProb >= randomrange;

            V2Enum_Grade pickGrade = synergyRuneCombineData.Grade;
            if (success == true)
            {
                int enumnum = pickGrade.Enum32ToInt();
                enumnum++;
                pickGrade = enumnum.IntToEnum32<V2Enum_Grade>();
            }

            List<SynergyRuneData> getrunelist = GetAllSynergyRuneData(pickGrade);

            if (getrunelist == null || getrunelist.Count <= 0)
                return null;

            SynergyRuneData resultdata = getrunelist[UnityEngine.Random.Range(0, getrunelist.Count)];


            foreach (var pair in _materialcount)
            {
                if (_materials.ContainsKey(pair.Key) == false)
                    return null;

                _materials[pair.Key] -= pair.Value;

                SkillInfo matinfo = GetSynergyEffectSkillInfo(pair.Key);
                if (matinfo == null)
                    return null;


                matinfo.Count -= pair.Value;
            }

            AddSynergyAmount(resultdata.Index, 1);

            SynergyRuneContainer.AccumCombineCount += 1;

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate);

            ThirdPartyLog.Instance.SendLog_log_rune_synthesis(resultdata.Index, resultdata.Grade.Enum32ToInt(), materialidx, System.Environment.TickCount);

            return resultdata;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region SynergyRuneEquip
        //------------------------------------------------------------------------------------
        public int GetCanEquipSkillSlotIdx()
        {
            int slotidx = -1;

            foreach (var pair in SynergyRuneContainer.SynergyEquip_Dic)
            {
                if (pair.Value == -1)
                {
                    if (IsOpenDescendSlot(pair.Key))
                    {
                        slotidx = pair.Key;
                        break;
                    }
                }
            }

            return slotidx;
        }
        //------------------------------------------------------------------------------------
        public bool IsOpenDescendSlot(int slot)
        {
            SynergyRuneOpenconditionData synergyRuneOpenconditionData = GetSynergyRuneOpenconditionData(slot);
            if (synergyRuneOpenconditionData == null)
                return false;

            return ContentOpenConditionManager.Instance.IsOpen(synergyRuneOpenconditionData.OpenConditionType, synergyRuneOpenconditionData.OpenConditionValue);
        }
        //------------------------------------------------------------------------------------
        public void ShowNoticeDescendSlotUnLockExp(int slot)
        {
            if (IsOpenDescendSlot(slot) == true)
                return;

            SynergyRuneOpenconditionData synergyRuneOpenconditionData = GetSynergyRuneOpenconditionData(slot);
            if (synergyRuneOpenconditionData == null)
                return;

            ContentOpenConditionManager.Instance.ShowOpenConditionNotice(synergyRuneOpenconditionData.OpenConditionType, synergyRuneOpenconditionData.OpenConditionValue);
        }
        //------------------------------------------------------------------------------------
        public bool IsEquipSkill(SynergyRuneData synergyEffectData)
        {
            return SynergyRuneContainer.SynergyEquip_Dic.ContainsValue(synergyEffectData.Index);
        }
        //------------------------------------------------------------------------------------
        public SynergyRuneData EquipedRuneData(int slotIndex)
        {
            if (SynergyRuneContainer.SynergyEquip_Dic.ContainsKey(slotIndex) == false)
                return null;

            return GetSynergyEffectData(SynergyRuneContainer.SynergyEquip_Dic[slotIndex]);
        }
        //------------------------------------------------------------------------------------
        public bool EquipSkill(SynergyRuneData aRRRSkillData)
        {
            if (aRRRSkillData == null)
                return false;

            int slotidx = GetCanEquipSkillSlotIdx();
            if (slotidx == -1)
                return false;

            if (slotidx == -1)
                return false;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(aRRRSkillData);

            SynergyRuneContainer.SynergyEquip_Dic[slotidx] = aRRRSkillData.Index;

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyRuneInfoTable);

            _changeEquipStateSynergyRuneMsg.slotid = slotidx;
            _changeEquipStateSynergyRuneMsg.synergyRuneData = aRRRSkillData;
            _changeEquipStateSynergyRuneMsg.IsEquipResult = true;
            Message.Send(_changeEquipStateSynergyRuneMsg);

            return true;
        }
        //------------------------------------------------------------------------------------
        public SynergyRuneData UnEquipSkillSlot(int slotIndex)
        {
            if (SynergyRuneContainer.SynergyEquip_Dic.ContainsKey(slotIndex) == false)
                return null;

            if (SynergyRuneContainer.SynergyEquip_Dic[slotIndex] == -1)
                return null;

            SynergyRuneData aRRRSkillData = GetSynergyEffectData(SynergyRuneContainer.SynergyEquip_Dic[slotIndex]);

            SynergyRuneContainer.SynergyEquip_Dic[slotIndex] = -1;

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyRuneInfoTable);

            _changeEquipStateSynergyRuneMsg.slotid = slotIndex;
            _changeEquipStateSynergyRuneMsg.synergyRuneData = aRRRSkillData;
            _changeEquipStateSynergyRuneMsg.IsEquipResult = false;
            Message.Send(_changeEquipStateSynergyRuneMsg);

            return aRRRSkillData;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region InGameRune
        //------------------------------------------------------------------------------------
        public void SetInGameSynergyRuneData()
        {
            _targetAfterSkill.Clear();

            foreach (var pair in SynergyRuneContainer.SynergyEquip_Dic)
            {
                if (pair.Value == -1)
                    continue;

                SynergyRuneData descendData = GetSynergyEffectData(pair.Value);
                if (descendData == null)
                    continue;

                if (descendData.SynergySkillData == null)
                    continue;

                MainSkillData mainSkillData = descendData.SynergySkillData;

                fakeRuneLevelInfo.Level = Managers.JobManager.Instance.GetJobAddBuff_Rune(descendData.SynergyType);

                if (mainSkillData.MainSkillType == V2Enum_ARR_MainSkillType.SkillEnforge
                    || mainSkillData.MainSkillType == V2Enum_ARR_MainSkillType.AfterSkill)
                {
                    if (_targetAfterSkill.ContainsKey(mainSkillData.MainSkillTypeParam2) == false)
                        _targetAfterSkill.Add(mainSkillData.MainSkillTypeParam2, new List<MainSkillData>());
                    _targetAfterSkill[mainSkillData.MainSkillTypeParam2].Add(mainSkillData);
                }
                else
                    Managers.BattleSceneManager.Instance.AddGambleSkill(descendData.SynergySkillData, descendData.SynergyType, fakeRuneLevelInfo);
            }
        }
        //------------------------------------------------------------------------------------
        public void ResetSynergyRuneState()
        {
            _targetAfterSkill.Clear();
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}