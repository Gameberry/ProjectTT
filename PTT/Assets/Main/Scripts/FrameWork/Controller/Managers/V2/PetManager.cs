using System.Collections.Generic;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using GameBerry.Common;

namespace GameBerry.Managers
{
    public class PetManager : MonoSingleton<PetManager>
    {
        private PetController _petControllerObject;

        private ObjectPool<PetController> _petControllerPool = new ObjectPool<PetController>();

        private PetLocalTable _petLocalTable = null;

        private List<PetInfo> _equipPetList = new List<PetInfo>(); // 임시 펫 장착 리스트

        private PetInfo _equipPetInfo = null;

        private Dictionary<int, Sprite> _skillIcons = new Dictionary<int, Sprite>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _petLocalTable = TableManager.Instance.GetTableClass<PetLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public void InitCreatureContent()
        {
            ResourceLoader.Instance.Load<GameObject>("BattleScene/PetController", o =>
            {
                GameObject obj = o as GameObject;
                _petControllerObject = obj.GetComponent<PetController>();
            });

            foreach (var pair in GetPetAllData_Dic())
            {
                PetData petData = pair.Value;
                petData.ActiveSkillData = SkillManager.Instance.GetSkillBaseData(petData.ActiveSkill);

                SetPassiveSkillData(petData, SkillManager.Instance.GetSkillBaseData(petData.PassiveSkill1));
                SetPassiveSkillData(petData, SkillManager.Instance.GetSkillBaseData(petData.PassiveSkill2));
                SetPassiveSkillData(petData, SkillManager.Instance.GetSkillBaseData(petData.PassiveSkill3));

                AddNewPlayerGearInfo(petData);
            }

            int equipPetIdx = PlayerPrefs.GetInt(Define.EquipPetKey, -1);
            if (equipPetIdx != -1)
            {
                PetData petData = GetPetData(equipPetIdx);
                if (petData != null)
                {
                    DoEquipPet(petData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void SetPassiveSkillData(PetData petData, SkillBaseData skillBaseData)
        {
            if (skillBaseData == null)
                return;

            if (petData.PassiveDatas == null)
                petData.PassiveDatas = new List<SkillBaseData>();

            petData.PassiveDatas.Add(skillBaseData);
        }
        //------------------------------------------------------------------------------------
        public List<PetData> GetPetAllDatas()
        {
            return _petLocalTable.GetPetAllDatas();
        }
        //------------------------------------------------------------------------------------
        public int SortPetInfo(PetData x, PetData y)
        {
            PetInfo xinfo = GetPetInfo(x);
            PetInfo yinfo = GetPetInfo(y);

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
                if (x.Index > y.Index)
                    return -1;
                else if (x.Index < y.Index)
                    return 1;
                else
                {
                    if (xinfo.Level > yinfo.Level)
                        return -1;
                    else if (xinfo.Level < yinfo.Level)
                        return 1;
                }
            }

            return 0;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, PetData> GetPetAllData_Dic()
        {
            return _petLocalTable.GetPetAllData();
        }
        //------------------------------------------------------------------------------------
        public PetData GetPetData(ObscuredInt index)
        {
            return _petLocalTable.GetPetData(index);
        }
        //------------------------------------------------------------------------------------
        public int AddPetAmount(int index, int amount)
        {
            return AddPetAmount(GetPetData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int AddPetAmount(PetData characterGearData, int amount)
        {
            if (characterGearData == null)
                return 0;

            PetInfo playerGearInfo = GetPetInfo(characterGearData);
            if (playerGearInfo == null)
                playerGearInfo = AddNewPlayerGearInfo(characterGearData);

            playerGearInfo.Count += amount;

            //CheckEnableAllEnhanceOrCombineState(characterGearData);

            //m_refreshEquipmentInfoListResponseMsg.datas.Clear();
            //m_refreshEquipmentInfoListResponseMsg.datas.Add(characterGearData);
            //Message.Send(m_refreshEquipmentInfoListResponseMsg);

            //if (CheckIsReadyEnhance(characterGearData) == true
            //            || CheckIsReadyCombine(characterGearData) == true)
            //    ShowGearRedDot(characterGearData.GearType);

            //ClanManager.Instance.CheckClanRedDot();

            return playerGearInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public PetController GetPetController()
        {
            PetController petController = _petControllerPool.GetObject() ?? CreatePetController();

            return petController;
        }
        //------------------------------------------------------------------------------------
        public void PoolCreature(PetController petController)
        {
            petController.gameObject.SetActive(false);
            _petControllerPool.PoolObject(petController);
        }
        //------------------------------------------------------------------------------------
        private PetController CreatePetController()
        {
            GameObject clone = Instantiate(_petControllerObject.gameObject, transform);

            PetController petController = clone.GetComponent<PetController>();
            petController.Init();

            return petController;
        }
        //------------------------------------------------------------------------------------
        public PetInfo GetPetInfo(PetData petData)
        {
            if (petData == null)
                return null;

            if (PetContainer.petInfos.ContainsKey(petData.Index) == true)
                return PetContainer.petInfos[petData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        private PetInfo AddNewPlayerGearInfo(PetData characterGearData)
        {
            PetInfo playerGearInfo = PetOperator.AddNewPlayerPetInfo(characterGearData);

            if (playerGearInfo == null)
                return null;

            //ShowGearRedDot(characterGearData.GearType);

            //m_setNewGearMsg.newData = characterGearData;
            //Message.Send(m_setNewGearMsg);

            //ClanManager.Instance.CheckClanRedDot();

            return playerGearInfo;
        }
        //------------------------------------------------------------------------------------
        public string GetPetLocalKey(PetData petData)
        {
            return string.Empty;
        }
        //------------------------------------------------------------------------------------
        public int GetPetLevel(PetData petData)
        {
            PetInfo petInfo = GetPetInfo(petData);
            if (petInfo != null)
                return petInfo.Level;

            return 1;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetPetIcon(PetData petData)
        {
            int iconIndex = petData.ResourceIndex;

            Sprite sp = null;

            if (_skillIcons.ContainsKey(iconIndex) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(Define.PetIconPath, iconIndex), o =>
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
        public bool IsEquipPet(PetData petData)
        {
            if (_equipPetInfo == null)
                return false;

            // 지금 단순 인덱스 검사지만 추후 펫을 여러마리 장착할 수 있어서 그때 알아서 수정
            return _equipPetInfo.Id == petData.Index;
        }
        //------------------------------------------------------------------------------------
        public PetInfo GetEquipPet()
        {
            return _equipPetInfo;
        }
        //------------------------------------------------------------------------------------
        public List<PetInfo> GetEquipPets()
        {
            _equipPetList.Clear();
            if (_equipPetInfo != null)
                _equipPetList.Add(_equipPetInfo);

            return _equipPetList;
        }
        //------------------------------------------------------------------------------------
        public bool DoEquipPet(PetData petData)
        {
            PetInfo petInfo = GetPetInfo(petData);
            if (petInfo == null)
                return false;

            _equipPetInfo = petInfo;

            PlayerPrefs.SetInt(Define.EquipPetKey, petData.Index.GetDecrypted());
            PlayerPrefs.Save();

            return true;
        }
        //------------------------------------------------------------------------------------
        public int GetLevelUpCost(PetData petData)
        {
            if (petData == null)
                return 9999999;

            LevelUpCostData petLevelUpCostData = _petLocalTable.GetPetLevelUpCostData(petData.Grade);
            if (petLevelUpCostData == null)
                return 9999999;

            return petLevelUpCostData.LevelUpCostCount;
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxLevel(PetData petData)
        {
            PetInfo petInfo = GetPetInfo(petData);
            if (petInfo == null)
                return false;

            LevelUpCostData petLevelUpCostData = _petLocalTable.GetPetLevelUpCostData(petData.Grade);
            if (petLevelUpCostData == null)
                return false;

            return petLevelUpCostData.MaximumLevel <= petInfo.Level;
        }
        //------------------------------------------------------------------------------------
        public bool CanLevelUp(PetData petData)
        {
            if (IsMaxLevel(petData) == true)
                return false;

            PetInfo petInfo = GetPetInfo(petData);
            if (petInfo == null)
                return false;

            LevelUpCostData petLevelUpCostData = _petLocalTable.GetPetLevelUpCostData(petData.Grade);
            if (petLevelUpCostData == null)
                return false;

            return petLevelUpCostData.LevelUpCostCount <= petInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public bool DoLevelUp(PetData petData)
        {
            if (CanLevelUp(petData) == false)
                return false;

            if (IsMaxLevel(petData) == true)
                return false;

            PetInfo petInfo = GetPetInfo(petData);
            if (petInfo == null)
                return false;

            LevelUpCostData petLevelUpCostData = _petLocalTable.GetPetLevelUpCostData(petData.Grade);
            if (petLevelUpCostData == null)
                return false;

            petInfo.Count -= petLevelUpCostData.LevelUpCostCount;
            petInfo.Level += 1;

            return true;
        }
        //------------------------------------------------------------------------------------
    }
}