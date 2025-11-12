using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class GearManager : MonoSingleton<GearManager>
    {

        private Event.ChangeEquipStateGearMsg _changeEquipStateSynergyRuneMsg = new Event.ChangeEquipStateGearMsg();
        private Event.ShowNewGearMsg _showNewSynergyMsg = new Event.ShowNewGearMsg();
        private Event.RefreshGearAllSlotMsg _refreshGearAllSlotMsg = new Event.RefreshGearAllSlotMsg();
        private GameBerry.Event.RefreshCharacterInfo_StatMsg _refreshCharacterInfo_StatMsg = new Event.RefreshCharacterInfo_StatMsg();
        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();

        private Dictionary<int, Sprite> _skillIcons = new Dictionary<int, Sprite>();

        private Dictionary<GearData, ObscuredInt> _materials = new Dictionary<GearData, ObscuredInt>();

        private Dictionary<GearData, int> _materialcount = new Dictionary<GearData, int>();

        private ObscuredInt _synergyRuneSlotCount = 4;

        private List<string> m_changeInfoUpdate = new List<string>();

        // totalLevel
        private Dictionary<V2Enum_Stat, ObscuredDouble> _arrrSynergyTotalStatValues = new Dictionary<V2Enum_Stat, ObscuredDouble>();
        public Dictionary<V2Enum_Stat, ObscuredDouble> ArrrSynergyTotalStatValues { get { return _arrrSynergyTotalStatValues; } }

        public int SynergyInCreaseCount = 0;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerGearTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);

            CharacterGearOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitSynergyRuneContent()
        {
            string newSynergyIndex = PlayerPrefs.GetString(Define.NewGearKey);
            if (string.IsNullOrEmpty(newSynergyIndex) == false)
            {
                string[] arr = newSynergyIndex.Split(',');

                for (int i = 0; i < arr.Length; ++i)
                {
                    int index = arr[i].ToInt() + 111010000;
                    GearData synergyEffectData = GetSynergyEffectData(index);
                    if (synergyEffectData == null)
                        continue;

                    AddNewSynergyIcon(synergyEffectData);
                }
            }

            if (GearContainer.SynergyEquip_Dic.Count == 0)
            {
                for (int synergy = V2Enum_GearType.Weapon.Enum32ToInt(); synergy < V2Enum_GearType.Max.Enum32ToInt(); ++synergy)
                {
                    V2Enum_GearType v2Enum_Stat = synergy.IntToEnum32<V2Enum_GearType>();
                    GearContainer.SynergyEquip_Dic.Add(v2Enum_Stat, -1);
                }
            }

            foreach (var pair in GearContainer.SynergyEquip_Dic)
            { 
                IncreaseStat(EquipedRuneData(pair.Key));
            }

            Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.StackSkillCount);
        }
        //------------------------------------------------------------------------------------
        #region SynergyData
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, GearData> GetAllGearData_Dic()
        {
            return CharacterGearOperator.GetAllGearData_Dic();
        }
        //------------------------------------------------------------------------------------
        public List<GearData> GetAllGearData()
        {
            return CharacterGearOperator.GetAllGearData();
        }
        //------------------------------------------------------------------------------------
        public List<GearData> GetAllGearData(V2Enum_Grade v2Enum_Grade)
        {
            return CharacterGearOperator.GetAllGearData(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public List<GearData> GetAllGearData(V2Enum_GearType v2Enum_Grade)
        {
            return CharacterGearOperator.GetAllGearData(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public double GetStatValue(GearData gearData, OperatorOverrideStat operatorOverrideStat)
        {
            int level = GetSlotLevel(gearData.GearType);

            return GetStatValue(level, operatorOverrideStat);
        }
        //------------------------------------------------------------------------------------
        public double GetStatValue(int level, OperatorOverrideStat operatorOverrideStat)
        {
            return operatorOverrideStat.OverrideStatBaseValue + (level * operatorOverrideStat.OverrideStatAddValue);
        }
        //------------------------------------------------------------------------------------
        public double GetJobBuffValue(GearData gearData, OperatorOverrideStat operatorOverrideStat)
        {
            if (gearData == null)
                return 0;

            if (Managers.JobManager.Instance.GetCurrentJobType() != gearData.SynergyType)
                return 0;

            double buffValue = Managers.JobManager.Instance.GetJobAddBuff_Gear(gearData.SynergyType);
            buffValue *= 0.01;

            return GetStatValue(gearData, operatorOverrideStat) * buffValue;
        }
        //------------------------------------------------------------------------------------
        public void IncreaseStat(GearData gearData)
        {
            if (gearData == null)
                return;

            for (int i = 0; i < gearData.OwnEffect.Count; ++i)
            {
                OperatorOverrideStat operatorOverrideStat = gearData.OwnEffect[i];

                double statvalue = GetStatValue(gearData, operatorOverrideStat);

                if (_arrrSynergyTotalStatValues.ContainsKey(operatorOverrideStat.BaseStat) == false)
                    _arrrSynergyTotalStatValues.Add(operatorOverrideStat.BaseStat, 0);

                _arrrSynergyTotalStatValues[operatorOverrideStat.BaseStat] += statvalue + GetJobBuffValue(gearData, operatorOverrideStat);
            }

            GearOptionData gearOptionData = GetGearOptionData(gearData.GearType, gearData.SynergyType);

            foreach (var pair in gearOptionData.GearSkills)
            {
                if (gearData.Grade >= pair.Key)
                {
                    MainSkillData mainSkillData = SkillManager.Instance.GetMainSkillData(pair.Value);
                    if (mainSkillData != null && mainSkillData.MainSkillType == Enum_MainSkillType.AddSkill)
                    {
                        SkillBaseData skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(mainSkillData.MainSkillTypeParam1);
                        if (skillBaseData != null && skillBaseData.SkillEffect != null)
                        {
                            for (int effect = 0; effect < skillBaseData.SkillEffect.Count; ++effect)
                            {
                                SkillEffectData skillEffectData = skillBaseData.SkillEffect[effect];
                                if (skillEffectData.SkillEffectType == V2Enum_SkillEffectType.IncreaseSynergyCount)
                                {
                                    SynergyInCreaseCount += skillEffectData.SkillEffectValue.ToInt();
                                }
                            }
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void DecreaseStat(GearData gearData)
        {
            if (gearData == null)
                return;

            for (int i = 0; i < gearData.OwnEffect.Count; ++i)
            {
                OperatorOverrideStat operatorOverrideStat = gearData.OwnEffect[i];

                if (_arrrSynergyTotalStatValues.ContainsKey(operatorOverrideStat.BaseStat) == false)
                    continue;

                double statvalue = GetStatValue(gearData, operatorOverrideStat);

                _arrrSynergyTotalStatValues[operatorOverrideStat.BaseStat] -= statvalue + GetJobBuffValue(gearData, operatorOverrideStat);

                if (_arrrSynergyTotalStatValues[operatorOverrideStat.BaseStat] < 0)
                    _arrrSynergyTotalStatValues[operatorOverrideStat.BaseStat] = 0;
            }

            GearOptionData gearOptionData = GetGearOptionData(gearData.GearType, gearData.SynergyType);

            foreach (var pair in gearOptionData.GearSkills)
            {
                if (gearData.Grade >= pair.Key)
                {
                    MainSkillData mainSkillData = SkillManager.Instance.GetMainSkillData(pair.Value);
                    if (mainSkillData != null && mainSkillData.MainSkillType == Enum_MainSkillType.AddSkill)
                    {
                        SkillBaseData skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(mainSkillData.MainSkillTypeParam1);
                        if (skillBaseData != null && skillBaseData.SkillEffect != null)
                        {
                            for (int effect = 0; effect < skillBaseData.SkillEffect.Count; ++effect)
                            {
                                SkillEffectData skillEffectData = skillBaseData.SkillEffect[effect];
                                if (skillEffectData.SkillEffectType == V2Enum_SkillEffectType.IncreaseSynergyCount)
                                {
                                    SynergyInCreaseCount -= skillEffectData.SkillEffectValue.ToInt();
                                }
                            }
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public int SortRuneData(GearData x, GearData y)
        {
            if (x.Grade > y.Grade)
                return -1;
            else if (x.Grade < y.Grade)
                return 1;
            else
            {

                if (x.GearType < y.GearType)
                    return -1;
                else if (x.GearType > y.GearType)
                    return 1;

                if (x.Index > y.Index)
                    return -1;
                else if (x.Index < y.Index)
                    return 1;
            }

            return 0;
        }
        //------------------------------------------------------------------------------------
        public GearData GetSynergyEffectData(ObscuredInt index)
        {
            return CharacterGearOperator.GetGearData(index);
        }
        //------------------------------------------------------------------------------------
        public GearLevelUpCostData GetGearLevelUpCostData(V2Enum_GearType v2Enum_Grade)
        {
            return CharacterGearOperator.GetGearLevelUpCostData(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public GearCombineData GetGearCombineData(V2Enum_Grade grade)
        {
            return CharacterGearOperator.GetGearCombineData(grade);
        }
        //------------------------------------------------------------------------------------
        public GearOptionData GetGearOptionData(V2Enum_GearType v2Enum_Grade, Enum_SynergyType gearNumber)
        {
            return CharacterGearOperator.GetGearOptionData(v2Enum_Grade, gearNumber);
        }
        //------------------------------------------------------------------------------------
        public SkillInfo GetSynergyEffectSkillInfo(ObscuredInt index)
        {
            return GetSynergyEffectSkillInfo(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        public SkillInfo GetSynergyEffectSkillInfo(GearData synergyEffectData)
        {
            if (synergyEffectData == null)
                return null;

            if (GearContainer.SynergyInfo.ContainsKey(synergyEffectData.Index) == true)
                return GearContainer.SynergyInfo[synergyEffectData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SkillInfo AddNewSkillInfo(GearData synergyEffectData)
        {
            if (synergyEffectData == null)
                return null;

            SkillInfo skillInfo = new SkillInfo();
            skillInfo.Id = synergyEffectData.Index;
            skillInfo.Level = Define.PlayerSkillDefaultLevel;
            skillInfo.Count = 0;

            GearContainer.SynergyInfo.Add(skillInfo.Id, skillInfo);


            return skillInfo;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetDescendIcon(GearData skillBaseData)
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
                ResourceLoader.Instance.Load<Sprite>(string.Format(Define.GearPath, iconIndex), o =>
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
        public void RefreshSynergyRedDot()
        {
            int count = 0;
            foreach (var pair in GearContainer.SynergyEquip_Dic)
            {
                if (pair.Value == -1)
                    continue;

                if (ReadySynergyEnhance(pair.Key))
                    count++;
            }
            if (count > 0)
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyGear);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyAmount(int index, int amount)
        {
            SetSynergyAmount(GetSynergyEffectData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyAmount(GearData synergyEffectData, int amount)
        {
            if (synergyEffectData == null)
                return;

            SkillInfo playerSkillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (playerSkillInfo == null)
            {
                playerSkillInfo = AddNewSkillInfo(synergyEffectData);

                AddNewSynergyIcon(synergyEffectData);

                //ARRRStatManager.Instance.RefreshBattlePower();
            }

            playerSkillInfo.Count = amount;

            if (ReadySynergyEnhance(synergyEffectData) == true)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyGear);
            }
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAmount(int index)
        {
            return GetSynergyAmount(GetSynergyEffectData(index));
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAmount(GearData synergyEffectData)
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
        public int AddSynergyAmount(GearData synergyEffectData, int amount)
        {
            if (synergyEffectData == null)
                return 0;

            SkillInfo playerSkillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (playerSkillInfo == null)
            {
                playerSkillInfo = AddNewSkillInfo(synergyEffectData);

                AddNewSynergyIcon(synergyEffectData);

                //ARRRStatManager.Instance.RefreshBattlePower();
            }

            playerSkillInfo.Count += amount;

            if (ReadySynergyEnhance(synergyEffectData) == true)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyGear);
            }

            return playerSkillInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyAmount(int index, int amount)
        {
            return UseSynergyAmount(GetSynergyEffectData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyAmount(GearData synergyEffectData, int amount)
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
            GearData synergyEffectData = GetSynergyEffectData(index);
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
        public V2Enum_Grade GetSynergyGrade(GearData synergyEffectData)
        {
            if (synergyEffectData == null)
                return V2Enum_Grade.Max;

            return synergyEffectData.Grade;
        }
        //------------------------------------------------------------------------------------
        public void AddNewSynergyIcon(GearData synergyEffectData)
        {
            if (synergyEffectData != null)
            {
                if (GearContainer.NewSynergys.ContainsKey(synergyEffectData) == false)
                    GearContainer.NewSynergys.Add(synergyEffectData, 1);
                else
                    GearContainer.NewSynergys[synergyEffectData] += 1;

                PlayerPrefs.SetString(Define.NewGearKey, GearContainer.GetNewSynergySerializeString());

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
            int count = GearContainer.NewSynergys.Count;
            return count;
        }
        //------------------------------------------------------------------------------------
        public bool IsNewSynergyIcon(GearData synergyEffectData)
        {
            return GearContainer.NewSynergys.ContainsKey(synergyEffectData);
        }
        //------------------------------------------------------------------------------------
        public void RemoveNewIconSynergy(GearData synergyEffectData)
        {
            if (synergyEffectData != null)
            {
                if (GearContainer.NewSynergys.ContainsKey(synergyEffectData) == true)
                    GearContainer.NewSynergys.Remove(synergyEffectData);

                PlayerPrefs.SetString(Define.NewGearKey, GearContainer.GetNewSynergySerializeString());
            }
        }
        //------------------------------------------------------------------------------------
        public void RemoveAllNewIconSynergy()
        {
            GearContainer.NewSynergys.Clear();

            PlayerPrefs.SetString(Define.NewGearKey, GearContainer.GetNewSynergySerializeString());
        }
        //------------------------------------------------------------------------------------
        public int GetDisplayEquipRuneCount(GearData synergyEffectData)
        {
            // 장착한거, 합성중인 카운트 빼고
            SkillInfo skillInfo = GetSynergyEffectSkillInfo(synergyEffectData);
            if (skillInfo == null)
                return 0;

            int displaycount = skillInfo.Count;

            if (GearContainer.SynergyEquip_Dic.ContainsKey(synergyEffectData.GearType) == true)
            {
                if (GearContainer.SynergyEquip_Dic[synergyEffectData.GearType] == synergyEffectData.Index)
                    displaycount -= 1;
            }

            if (_materials.ContainsKey(synergyEffectData) == true)
                displaycount -= _materials[synergyEffectData];


            return displaycount;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region SynergyRuneCombine
        //------------------------------------------------------------------------------------
        public bool ReadySynergyEnhance(GearData synergyEffectData)
        {
            if (synergyEffectData == null)
                return false;

            GearCombineData synergyRuneCombineData = GetGearCombineData(synergyEffectData.Grade);
            if (synergyRuneCombineData == null)
                return false;

            List<GearData> synergyRuneDatas = GetAllGearData(synergyEffectData.Grade);
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
            GearCombineData synergyRuneCombineData = GetGearCombineData(v2Enum_Grade);
            if (synergyRuneCombineData == null)
                return 99999;

            return synergyRuneCombineData.RequiredCount;
        }
        //------------------------------------------------------------------------------------
        public void AddMaterial(GearData synergyRuneData)
        {
            if (_materials.ContainsKey(synergyRuneData) == false)
                _materials.Add(synergyRuneData, 0);

            _materials[synergyRuneData] += 1;
        }
        //------------------------------------------------------------------------------------
        public void RemoveMaterial(GearData synergyRuneData)
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
                
                GearCombineData synergyRuneCombineData = Managers.GearManager.Instance.GetGearCombineData(grade);
                if (synergyRuneCombineData == null)
                {
                    continue;
                }

                for (int synergy = V2Enum_GearType.Weapon.Enum32ToInt(); synergy < V2Enum_GearType.Max.Enum32ToInt(); ++synergy)
                {
                    V2Enum_GearType v2Enum_Stat = synergy.IntToEnum32<V2Enum_GearType>();


                    List<GearData> mat = GetAllGearData(v2Enum_Stat).FindAll(
                        x => GetDisplayEquipRuneCount(x) > 0
                        && x.Grade == grade);

                    if (mat.Count >= synergyRuneCombineData.RequiredCount)
                        return true;
                }
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public GearData DoRuneCombine(GearCombineData synergyRuneCombineData, List<GearData> materialRunes)
        {
            if (synergyRuneCombineData == null)
                return null;

            if (materialRunes == null)
                return null;

            if (materialRunes.Count < synergyRuneCombineData.RequiredCount)
                return null;

            _materialcount.Clear();

            List<int> materialidx = new List<int>();

            V2Enum_GearType v2Enum_GearType = V2Enum_GearType.Max;

            for (int i = 0; i < synergyRuneCombineData.RequiredCount; ++i)
            {
                if (materialRunes[i] == null)
                    return null;

                if (_materialcount.ContainsKey(materialRunes[i]) == false)
                    _materialcount.Add(materialRunes[i], 0);

                _materialcount[materialRunes[i]] += 1;

                materialidx.Add(materialRunes[i].Index);

                if (materialRunes[i].GearType != V2Enum_GearType.Max)
                    v2Enum_GearType = materialRunes[i].GearType;
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

            List<GearData> getrunelist = GetAllGearData(pickGrade);

            if (getrunelist == null || getrunelist.Count <= 0)
                return null;

            getrunelist = getrunelist.FindAll(x => x.GearType == v2Enum_GearType);

            if (getrunelist.Count <= 0)
                return null;

            GearData resultdata = getrunelist[UnityEngine.Random.Range(0, getrunelist.Count)];


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

            GearContainer.AccumCombineCount += 1;

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate);

            ThirdPartyLog.Instance.SendLog_log_equip_synthesis(resultdata.Index, resultdata.Grade.Enum32ToInt(), materialidx, System.Environment.TickCount);

            return resultdata;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region GearSlot
        //------------------------------------------------------------------------------------
        public int GetCanEquipSkillSlotIdx(V2Enum_GearType Enum_SynergyType)
        {
            if (GearContainer.SynergyEquip_Dic.ContainsKey(Enum_SynergyType) == false)
                return -1;

            return 0;
        }
        //------------------------------------------------------------------------------------
        public bool IsEquipSkill(GearData synergyEffectData)
        {
            if (GearContainer.SynergyEquip_Dic.ContainsKey(synergyEffectData.GearType) == false)
                return false;

            return GearContainer.SynergyEquip_Dic[synergyEffectData.GearType] == synergyEffectData.Index;
        }
        //------------------------------------------------------------------------------------
        public GearData EquipedRuneData(V2Enum_GearType Enum_SynergyType)
        {
            if (GearContainer.SynergyEquip_Dic.ContainsKey(Enum_SynergyType) == false)
                return null;

            return GetSynergyEffectData(GearContainer.SynergyEquip_Dic[Enum_SynergyType]);
        }
        //------------------------------------------------------------------------------------
        public Dictionary<V2Enum_GearType, GearData> GetHigherGradeEquipments()
        {
            Dictionary<V2Enum_GearType, GearData> changeGear = new Dictionary<V2Enum_GearType, GearData>();

            foreach (var pair in GearContainer.SynergyInfo)
            {
                if (pair.Value.Count <= 0)
                    continue;

                GearData gearData = GetSynergyEffectData(pair.Key);
                if (gearData == null)
                    continue;

                GearData equipedData = EquipedRuneData(gearData.GearType);
                if (equipedData != null)
                {
                    if (equipedData.Grade >= gearData.Grade)
                        continue;
                }

                if (changeGear.ContainsKey(gearData.GearType) == true)
                {
                    if (changeGear[gearData.GearType].Grade < gearData.Grade)
                        changeGear[gearData.GearType] = gearData;
                }
                else
                {
                    changeGear.Add(gearData.GearType, gearData);
                }
            }

            return changeGear;
        }
        //------------------------------------------------------------------------------------
        public bool CanGearEquip()
        {
            Dictionary<V2Enum_GearType, GearData> changeGear = new Dictionary<V2Enum_GearType, GearData>();

            return GetHigherGradeEquipments().Count > 0;
        }
        //------------------------------------------------------------------------------------
        public bool AllEquipGear()
        {
            Dictionary<V2Enum_GearType, GearData> changeGear = GetHigherGradeEquipments();

            foreach (var pair in changeGear)
            {
                EquipSkill(pair.Value);
            }

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool EquipSkill(GearData aRRRSkillData)
        {
            if (aRRRSkillData == null)
                return false;

            GearData beforeGearData = EquipedRuneData(aRRRSkillData.GearType);
            DecreaseStat(beforeGearData);


            GearContainer.SynergyEquip_Dic[aRRRSkillData.GearType] = aRRRSkillData.Index;
            IncreaseStat(aRRRSkillData);

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerGearTable);

            _changeEquipStateSynergyRuneMsg.Enum_SynergyType = aRRRSkillData.GearType;
            _changeEquipStateSynergyRuneMsg.synergyRuneData = aRRRSkillData;
            _changeEquipStateSynergyRuneMsg.IsEquipResult = true;
            Message.Send(_changeEquipStateSynergyRuneMsg);

            BattleSceneManager.Instance.RefreshMyARRRStat();
            Message.Send(_refreshCharacterInfo_StatMsg);
            ARRRStatManager.Instance.RefreshBattlePower();

            ThirdPartyLog.Instance.SendLog_log_equip_gear(aRRRSkillData.Index);

            return true;
        }
        //------------------------------------------------------------------------------------
        public GearData UnEquipSkillSlot(V2Enum_GearType Enum_SynergyType)
        {
            if (GearContainer.SynergyEquip_Dic.ContainsKey(Enum_SynergyType) == false)
                return null;

            GearData aRRRSkillData = GetSynergyEffectData(GearContainer.SynergyEquip_Dic[Enum_SynergyType]);

            DecreaseStat(aRRRSkillData);

            GearContainer.SynergyEquip_Dic[Enum_SynergyType] = -1;

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerGearTable);

            _changeEquipStateSynergyRuneMsg.Enum_SynergyType = Enum_SynergyType;
            _changeEquipStateSynergyRuneMsg.synergyRuneData = aRRRSkillData;
            _changeEquipStateSynergyRuneMsg.IsEquipResult = false;
            Message.Send(_changeEquipStateSynergyRuneMsg);

            BattleSceneManager.Instance.RefreshMyARRRStat();
            Message.Send(_refreshCharacterInfo_StatMsg);
            ARRRStatManager.Instance.RefreshBattlePower();

            return aRRRSkillData;
        }
        //------------------------------------------------------------------------------------
        public int GetSlotLevel(V2Enum_GearType Enum_SynergyType)
        {
            if (GearContainer.SlotLevel_Dic.ContainsKey(Enum_SynergyType) == true)
                return GearContainer.SlotLevel_Dic[Enum_SynergyType];

            return 0;
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxLevelSynergy(V2Enum_GearType Enum_SynergyType)
        {
            GearLevelUpCostData synergyLevelUpCostData = GetGearLevelUpCostData(Enum_SynergyType);

            if (synergyLevelUpCostData == null)
                return true;

            int level = GetSlotLevel(Enum_SynergyType);

            return level >= synergyLevelUpCostData.MaxLevel;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhance_NeedCount1(V2Enum_GearType Enum_SynergyType)
        {
            GearLevelUpCostData synergyLevelUpCostData = GetGearLevelUpCostData(Enum_SynergyType);

            if (synergyLevelUpCostData == null)
                return 99999;

            int level = GetSlotLevel(Enum_SynergyType);

            return synergyLevelUpCostData.LevelUpCostGoodsParam12 + (level * synergyLevelUpCostData.LevelUpCostGoodsParam13);
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhanceCostGoodsIndex1(V2Enum_GearType Enum_SynergyType)
        {
            GearLevelUpCostData synergyLevelUpCostData = GetGearLevelUpCostData(Enum_SynergyType);

            if (synergyLevelUpCostData == null)
                return -1;

            return synergyLevelUpCostData.LevelUpCostGoodsParam11;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhance_NeedCount2(V2Enum_GearType Enum_SynergyType)
        {
            GearLevelUpCostData synergyLevelUpCostData = GetGearLevelUpCostData(Enum_SynergyType);

            if (synergyLevelUpCostData == null)
                return 99999;

            int level = GetSlotLevel(Enum_SynergyType);

            return synergyLevelUpCostData.LevelUpCostGoodsParam22 + (level * synergyLevelUpCostData.LevelUpCostGoodsParam23);
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhanceCostGoodsIndex2(V2Enum_GearType Enum_SynergyType)
        {
            GearLevelUpCostData synergyLevelUpCostData = GetGearLevelUpCostData(Enum_SynergyType);

            if (synergyLevelUpCostData == null)
                return -1;

            return synergyLevelUpCostData.LevelUpCostGoodsParam21;
        }
        //------------------------------------------------------------------------------------
        public bool ReadySynergyEnhance(V2Enum_GearType Enum_SynergyType)
        {
            int costIndex = GetSynergyEnhanceCostGoodsIndex1(Enum_SynergyType);
            int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

            if (currentCount < GetSynergyEnhance_NeedCount1(Enum_SynergyType))
                return false;

            costIndex = GetSynergyEnhanceCostGoodsIndex2(Enum_SynergyType);
            currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

            if (currentCount < GetSynergyEnhance_NeedCount2(Enum_SynergyType))
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool EnhanceSynergy(V2Enum_GearType Enum_SynergyType)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (IsMaxLevelSynergy(Enum_SynergyType) == true)
                return false;

            if (ReadySynergyEnhance(Enum_SynergyType) == false)
            {
                Contents.GlobalContent.ShowPopup_OkCancel(
Managers.LocalStringManager.Instance.GetLocalString("common/ui/shortagegoodstitle"),
Managers.LocalStringManager.Instance.GetLocalString("common/ui/shortagegoodsdesc"),
() =>
{
    UI.UIManager.DialogExit<UI.LobbySynergyContentDialog>();
    Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.ShopInGameStore_Synergy);
},
null);
                return false;
            }

            List<int> used_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> used_quan = new List<double>();
            List<double> after_quan = new List<double>();

            int costIndex = GetSynergyEnhanceCostGoodsIndex1(Enum_SynergyType);
            used_type.Add(costIndex);

            int useCost = GetSynergyEnhance_NeedCount1(Enum_SynergyType);

            before_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(costIndex));
            used_quan.Add(useCost);
            Managers.GoodsManager.Instance.UseGoodsAmount(costIndex, useCost);
            after_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(costIndex));


            costIndex = GetSynergyEnhanceCostGoodsIndex2(Enum_SynergyType);
            used_type.Add(costIndex);

            useCost = GetSynergyEnhance_NeedCount2(Enum_SynergyType);

            before_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(costIndex));
            used_quan.Add(useCost);
            Managers.GoodsManager.Instance.UseGoodsAmount(costIndex, useCost);
            after_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(costIndex));

            GearData gearData = EquipedRuneData(Enum_SynergyType);
            if (gearData != null)
                DecreaseStat(gearData);

            if (GearContainer.SlotLevel_Dic.ContainsKey(Enum_SynergyType) == false)
                GearContainer.SlotLevel_Dic.Add(Enum_SynergyType, 0);

            GearContainer.SlotLevel_Dic[Enum_SynergyType] += 1;

            if (gearData != null)
                IncreaseStat(gearData);


            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_log_gearslot_enforce(Enum_SynergyType.Enum32ToInt(), GearContainer.SlotLevel_Dic[Enum_SynergyType],
                used_type, before_quan, used_quan, after_quan);

            //PassManager.Instance.CheckPassType(V2Enum_PassType.SkillLevel);
            //Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.SynergySkillLevelStack);

            Message.Send(_refreshGearAllSlotMsg);

            BattleSceneManager.Instance.RefreshMyARRRStat();
            Message.Send(_refreshCharacterInfo_StatMsg);
            ARRRStatManager.Instance.RefreshBattlePower();

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool ResetSlot(V2Enum_GearType Enum_SynergyType)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (GearContainer.SlotLevel_Dic.ContainsKey(Enum_SynergyType) == false)
                return false;

            if (GearContainer.SlotLevel_Dic[Enum_SynergyType] == 0)
                return false;

            List<int> idx = new List<int>();

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            {
                GearLevelUpCostData synergyLevelUpCostData = GetGearLevelUpCostData(Enum_SynergyType);
                if (synergyLevelUpCostData == null)
                    return false;

                int slotlevel = GearContainer.SlotLevel_Dic[Enum_SynergyType];

                int costIndex = GetSynergyEnhanceCostGoodsIndex1(Enum_SynergyType);

                RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == costIndex);
                if (rewardData == null)
                {
                    rewardData = RewardManager.Instance.GetRewardData();
                    rewardData.V2Enum_Goods = GoodsManager.Instance.GetGoodsType(costIndex);
                    rewardData.Index = costIndex;
                    rewardData.Amount = 0;
                    m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                }

                rewardData.Amount += GetTotalCost(slotlevel, synergyLevelUpCostData.LevelUpCostGoodsParam12, synergyLevelUpCostData.LevelUpCostGoodsParam13);


                costIndex = GetSynergyEnhanceCostGoodsIndex2(Enum_SynergyType);

                rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == costIndex);
                if (rewardData == null)
                {
                    rewardData = RewardManager.Instance.GetRewardData();
                    rewardData.V2Enum_Goods = GoodsManager.Instance.GetGoodsType(costIndex);
                    rewardData.Index = costIndex;
                    rewardData.Amount = 0;
                    m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                }

                rewardData.Amount += GetTotalCost(slotlevel, synergyLevelUpCostData.LevelUpCostGoodsParam22, synergyLevelUpCostData.LevelUpCostGoodsParam23);

            }

            int slotbeforelevel = GearContainer.SlotLevel_Dic[Enum_SynergyType];

            GearData gearData = EquipedRuneData(Enum_SynergyType);
            if (gearData != null)
                DecreaseStat(gearData);

            GearContainer.SlotLevel_Dic[Enum_SynergyType] = 0;

            if (gearData != null)
                IncreaseStat(gearData);

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
                UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();

                //RedDotManager.Instance.HideRedDot(GetRedDotEnum(contentDetailList));
            }

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_log_gearslot_reset(Enum_SynergyType.Enum32ToInt(), slotbeforelevel,
                reward_type, reward_quan);


            Message.Send(_refreshGearAllSlotMsg);

            BattleSceneManager.Instance.RefreshMyARRRStat();
            Message.Send(_refreshCharacterInfo_StatMsg);
            ARRRStatManager.Instance.RefreshBattlePower();

            return true;
        }
        //------------------------------------------------------------------------------------
        public int GetTotalCost(int level, int baseAmount, int levelAmount)
        {
            int n = level;

            return (n * baseAmount) + (levelAmount * n * (n - 1) / 2);
        }
        //------------------------------------------------------------------------------------
        public void RefreshJobUpGrade()
        {
            SynergyInCreaseCount = 0;

            _arrrSynergyTotalStatValues.Clear();

            foreach (var pair in GearContainer.SynergyEquip_Dic)
            {
                IncreaseStat(EquipedRuneData(pair.Key));
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region InGameRune
        //------------------------------------------------------------------------------------
        public void SetInGameGearData()
        {
            foreach (var equippair in GearContainer.SynergyEquip_Dic)
            {
                GearData descendData = EquipedRuneData(equippair.Key);
                if (descendData == null)
                    continue;

                GearOptionData gearOptionData = GetGearOptionData(descendData.GearType, descendData.SynergyType);

                foreach (var pair in gearOptionData.GearSkills)
                {
                    if (descendData.Grade >= pair.Key)
                    { 
                        MainSkillData mainSkillData = SkillManager.Instance.GetMainSkillData(pair.Value);
                        Managers.BattleSceneManager.Instance.AddGambleSkill(mainSkillData);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}