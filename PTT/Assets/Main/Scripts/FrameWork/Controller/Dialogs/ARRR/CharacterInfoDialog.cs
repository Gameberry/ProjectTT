using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;
using Spine;
using Spine.Unity;

namespace GameBerry.UI
{
    [System.Serializable]
    public class CharacterEnhanceMaterial
    {
        public UIGlobalGoodsRewardIconElement MaterialElement;

        public TMP_Text MaterialCount;
    }

    public class CharacterInfoDialog : IDialog
    {
        [Header("------------Stage1HideUI------------")]
        [SerializeField]
        private List<Transform> _stage1HideUI = new List<Transform>();


        [Header("------------SkillSlot------------")]
        [SerializeField]
        private LayoutGroup _skillSlotLayOut;

        [SerializeField]
        private RectTransform _skillSlotLayOut_Rect;

        [SerializeField]
        private Transform _skillSlotElementRoot;

        [SerializeField]
        private UIARRRSkillSlotElement m_uISkillSlotElement;

        private Dictionary<ObscuredInt, UIARRRSkillSlotElement> m_spawnSkillSlotElement_Dic = new Dictionary<ObscuredInt, UIARRRSkillSlotElement>();

        [Header("------------SkillElement------------")]
        [SerializeField]
        private Transform _skillElementRoot;

        [SerializeField]
        private UIARRRSkillElement _uISkillElement;

        private Dictionary<ARRRSkillData, UIARRRSkillElement> _spawnSkillElement_Dic = new Dictionary<ARRRSkillData, UIARRRSkillElement>();

        [SerializeField]
        private UIARRRSkillDescGroup _uIARRRSkillDescGroup;


        [SerializeField]
        private Button _enhanceBtn;

        [SerializeField]
        private TMP_Text _enhanceText;


        [SerializeField]
        private Button _equipSkillBtn;

        [SerializeField]
        private TMP_Text _equipSkillText;

        [Header("------------CharacterNickName------------")]
        [SerializeField]
        private TMP_Text _playerName;

        [SerializeField]
        private Button _playerNameChangeShowPopup;


        [Header("------------CharacterBattlePower------------")]
        [SerializeField]
        private TMP_Text _characterBattlePower;

        [Header("------------CharacterStat------------")]
        [SerializeField]
        private UIStatDetailViewr uIStatDetailViewr;

        [SerializeField]
        private Transform _levelUpDimmed;

        [SerializeField]
        private TMP_Text _levelText;

        [SerializeField]
        private UIPushBtn _levelUp_PushBtn;

        [SerializeField]
        private Button _levelUpBtn;

        [SerializeField]
        private Button _limitUpBtn;

        [SerializeField]
        private Transform _maxBtn;

        [SerializeField]
        private Transform _enhanceMaterialGroup;

        [SerializeField]
        private List<CharacterEnhanceMaterial> _enhanceMaterials = new List<CharacterEnhanceMaterial>();

        private List<RewardData> _enhanceMaterial_RewardDatas = new List<RewardData>();

        [SerializeField]
        private UILobbyCharLvUp _uILobbyCharLvUpEffect;

        [SerializeField]
        private Transform _uILobbyCharLvUpEffect_Root;

        [SerializeField]
        private int _uILobbyCharLvUpEffect_MaxPoolCount = 5;

        private List<UILobbyCharLvUp> _uILobbyCharLvUpEffectPool = new List<UILobbyCharLvUp>();
        [SerializeField]
        private int _uILobbyCharLvUpEffectPoolPlayIndex = 0;

        private Dictionary<V2Enum_Stat, ObscuredDouble> _arrrDefaultStat = new Dictionary<V2Enum_Stat, ObscuredDouble>();

        private ARRRSkillData _currentSelectSkill = null;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (uIStatDetailViewr != null)
            {
                uIStatDetailViewr.Init();
            }

            //foreach (KeyValuePair<ObscuredInt, ARRRSkillData> pair in Managers.ARRRSkillManager.Instance.GetEquipARRRSkillSlot())
            //{
            //    UIARRRSkillSlotElement element = CreateSlot();
            //    element.SetSlotID(pair.Key);
            //    element.SetSkill(pair.Value);
            //    m_spawnSkillSlotElement_Dic.Add(pair.Key, element);

            //    if (_currentSelectSkill == null)
            //        _currentSelectSkill = pair.Value;
            //}

            foreach (KeyValuePair<ObscuredInt, ARRRSkillData> pair in Managers.ARRRSkillManager.Instance.GetARRRSkillLinkDatas())
            {
                GameObject clone = Instantiate(_uISkillElement.gameObject, _skillElementRoot);
                UIARRRSkillElement slot = clone.GetComponent<UIARRRSkillElement>();
                slot.SetCallBack(Click_SkillElement);
                slot.SetSkillElement(pair.Value);

                _spawnSkillElement_Dic.Add(pair.Value, slot);

                if (_currentSelectSkill == null)
                    _currentSelectSkill = pair.Value;
            }

            if (_equipSkillBtn != null)
                _equipSkillBtn.onClick.AddListener(OnClick_EquipSkillBtn);

            if (_enhanceBtn != null)
                _enhanceBtn.onClick.AddListener(OnClick_SkillLevelUpBtn);

            if (_levelUp_PushBtn != null)
            {
                _levelUp_PushBtn.SetOnClick(OnClick_LevelUpBtn);
                _levelUp_PushBtn.SetOnPush(OnClick_LevelUpBtn);
                //uIPushBtn.SetOnPushEnd(UpGrade_PushEnd);
            }

            if (_playerNameChangeShowPopup != null)
                _playerNameChangeShowPopup.onClick.AddListener(() =>
                {
                    UIManager.DialogEnter<InGameNickNameChangePopupDialog>();
                });


            //if (_levelUpBtn != null)
            //    _levelUpBtn.onClick.AddListener(OnClick_LevelUpBtn);

            if (_limitUpBtn != null)
                _limitUpBtn.onClick.AddListener(OnClick_LimitUpBtn);

            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.LobbyGold.Enum32ToInt(), RefreshEnhanceBtn);

            Message.AddListener<GameBerry.Event.RefreshCharacterInfo_StatMsg>(RefreshCharacterInfo_Stat);
            Message.AddListener<GameBerry.Event.RefreshBattlePowerUIMsg>(RefreshBattlePowerUI);
            Message.AddListener<GameBerry.Event.RefreshNickNameMsg>(RefreshNickName);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshCharacterInfo_StatMsg>(RefreshCharacterInfo_Stat);
            Message.RemoveListener<GameBerry.Event.RefreshBattlePowerUIMsg>(RefreshBattlePowerUI);
            Message.RemoveListener<GameBerry.Event.RefreshNickNameMsg>(RefreshNickName);

            Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.LobbyGold.Enum32ToInt(), RefreshEnhanceBtn);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (_playerName != null)
                _playerName.text = TheBackEnd.TheBackEndManager.Instance.GetFakeNickPlayerName();

            for (int i = 0; i < _stage1HideUI.Count; ++i)
            {
                if (_stage1HideUI[i] != null)
                    _stage1HideUI[i].gameObject.SetActive(Managers.MapManager.Instance.NeedTutotial1() == false);
            }

            if (uIStatDetailViewr != null)
                uIStatDetailViewr.SetMiniWindow();

            RefreshStatViewer();

            SetARRRLevel();
            SetEnhanceBtn();

            RefreshSkillSlot();

            RefreshAllSkillElement();

            SetDetailSkillElement(_currentSelectSkill);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_skillSlotLayOut_Rect);

            if (_skillSlotLayOut != null)
            {
                _skillSlotLayOut.enabled = false;
            }

            for (int i = 0; i < _uILobbyCharLvUpEffectPool.Count; ++i)
            {
                _uILobbyCharLvUpEffectPool[i].gameObject.SetActive(false);
            }

            RefreshBattlePowerUI(null);

        }
        //------------------------------------------------------------------------------------
        private void RefreshNickName(GameBerry.Event.RefreshNickNameMsg msg)
        {
            if (_playerName != null)
                _playerName.text = Managers.PlayerDataManager.Instance.GetPlayerName();
        }
        //------------------------------------------------------------------------------------
        private void RefreshEnhanceBtn(double amount)
        {
            SetEnhanceBtn();
        }
        //------------------------------------------------------------------------------------
        private void RefreshCharacterInfo_Stat(GameBerry.Event.RefreshCharacterInfo_StatMsg msg)
        {
            RefreshStatViewer();
        }
        //------------------------------------------------------------------------------------
        private void RefreshStatViewer()
        {
            if (uIStatDetailViewr != null)
            {
                Managers.ARRRStatManager.Instance.GetPlayerARRRDefaultStat(ref _arrrDefaultStat);
                uIStatDetailViewr.SetDetailViewr(_arrrDefaultStat, null);
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshBattlePowerUI(GameBerry.Event.RefreshBattlePowerUIMsg msg)
        {
            if (_characterBattlePower != null)
                _characterBattlePower.SetText(string.Format("{0:0,0}", Managers.ARRRStatManager.Instance.GetBattlePower()));
        }
        //------------------------------------------------------------------------------------
        #region Skill
        //------------------------------------------------------------------------------------
        private void RefreshSkillSlot()
        {
            foreach (KeyValuePair<ObscuredInt, ARRRSkillData> pair in Managers.ARRRSkillManager.Instance.GetEquipARRRSkillSlot())
            {
                if (m_spawnSkillSlotElement_Dic.ContainsKey(pair.Key) == false)
                    continue;

                UIARRRSkillSlotElement element = m_spawnSkillSlotElement_Dic[pair.Key];
                if (element != null)
                    element.SetSkill(pair.Value);
            }
        }
        //------------------------------------------------------------------------------------
        private UIARRRSkillSlotElement CreateSlot()
        {
            GameObject clone = Instantiate(m_uISkillSlotElement.gameObject, _skillSlotElementRoot);
            UIARRRSkillSlotElement slot = clone.GetComponent<UIARRRSkillSlotElement>();
            slot.Init(OnClick_SlotBtn,
                OnDrag_Slot,
                OnEndDrag_Slot,
                OnDrop_Slot);

            return slot;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SlotBtn(int slotid)
        {
            //Managers.CharacterSkillSlotManager.Instance.UnEquipSkill(slotid);
            Debug.Log(string.Format("OnClick : {0}", slotid));

            ARRRSkillData aRRRSkillData = Managers.ARRRSkillManager.Instance.UnEquipSkillSlot(slotid);

            if (aRRRSkillData != null)
            {
                if (_spawnSkillElement_Dic.ContainsKey(aRRRSkillData) == true)
                {
                    UIARRRSkillElement uIARRRSkillElement = _spawnSkillElement_Dic[aRRRSkillData];
                    uIARRRSkillElement.SetEquipElement(false);
                }

                //if (aRRRSkillData == _currentSelectSkill)
                SetDetailSkillElement(aRRRSkillData);
            }

            RefreshSkillSlot();
        }
        //------------------------------------------------------------------------------------
        private int _dragSlotIdx = -1;
        private int _overSlotIdx = -1;
        private void OnDrag_Slot(int slotid)
        {
            _dragSlotIdx = slotid;
        }
        //------------------------------------------------------------------------------------
        private void OnEndDrag_Slot(int slotid)
        {
            //Debug.Log(string.Format("drag : {0}, drop : {1}", _dragSlotIdx, _overSlotIdx));
        }
        //------------------------------------------------------------------------------------
        private void OnDrop_Slot(int slotid)
        {
            _overSlotIdx = slotid;

            Debug.Log(string.Format("drag : {0}, drop : {1}", _dragSlotIdx, _overSlotIdx));

            SweepSkillSlot(_dragSlotIdx, _overSlotIdx);
        }
        //------------------------------------------------------------------------------------
        private void SweepSkillSlot(int dragSlotIdx, int overSlotIdx)
        {
            if (_dragSlotIdx == -1 || _overSlotIdx == -1)
            {
                _dragSlotIdx = -1;
                _overSlotIdx = -1;
                return;
            }

            Managers.ARRRSkillManager.Instance.SweepSkillSlot(dragSlotIdx, overSlotIdx);

            _dragSlotIdx = -1;
            _overSlotIdx = -1;

            RefreshSkillSlot();
        }
        //------------------------------------------------------------------------------------
        private void RefreshAllSkillElement()
        {
            List<ARRRSkillData> skilllist = Managers.ARRRSkillManager.Instance.GetARRRSkillDatas();
            skilllist.Sort(Managers.ARRRSkillManager.Instance.SortARRRSkillData);
            //private Dictionary<ARRRSkillData, UIARRRSkillElement> m_spawnSkillElement_Dic = new Dictionary<ARRRSkillData, UIARRRSkillElement>();

            for (int i = 0; i < skilllist.Count; ++i)
            {
                ARRRSkillData aRRRSkillData = skilllist[i];
                if (_spawnSkillElement_Dic.ContainsKey(aRRRSkillData) == false)
                    continue;

                UIARRRSkillElement uIARRRSkillElement = _spawnSkillElement_Dic[aRRRSkillData];
                uIARRRSkillElement.SetSkillElement(aRRRSkillData);
                uIARRRSkillElement.transform.SetAsLastSibling();
            }
        }
        //------------------------------------------------------------------------------------
        private void Click_SkillElement(ARRRSkillData skillBaseData, SkillInfo skillInfo)
        {
            SetDetailSkillElement(skillBaseData);
        }
        //------------------------------------------------------------------------------------
        private void SetDetailSkillElement(ARRRSkillData aRRRSkillData)
        {
            if (_currentSelectSkill != null)
            {
                if (_spawnSkillElement_Dic.ContainsKey(_currentSelectSkill) == true)
                {
                    UIARRRSkillElement uIARRRSkillElement = _spawnSkillElement_Dic[_currentSelectSkill];
                    uIARRRSkillElement.SetSelect(false);
                }
            }

            _currentSelectSkill = aRRRSkillData;

            if (_currentSelectSkill != null)
            {
                if (_spawnSkillElement_Dic.ContainsKey(_currentSelectSkill) == true)
                {
                    UIARRRSkillElement uIARRRSkillElement = _spawnSkillElement_Dic[_currentSelectSkill];
                    uIARRRSkillElement.SetSelect(true);
                }
            }

            //if (_uISkillDetailElement != null)
            //    _uISkillDetailElement.SetSkillElement(_currentSelectSkill);

            SkillBaseData skillBaseData = Managers.ARRRSkillManager.Instance.GetSkillBaseData(_currentSelectSkill);
            //if (skillBaseData != null)
            //{
            //    if (_uISkillDetail_Name != null)
            //        Managers.LocalStringManager.Instance.SetLocalizeText(_uISkillDetail_Name, skillBaseData.NameLocalKey);

            //    if (_uISkillDetail_CoolTime != null)
            //    {
            //        if (skillBaseData.CoolTimeType == Enum_CoolTimeType.Default
            //            || skillBaseData.CoolTimeType == Enum_CoolTimeType.GamebleCoolTime)
            //            _uISkillDetail_CoolTime.SetText("{0}s");
            //        else
            //            _uISkillDetail_CoolTime.SetText("-");
            //    }
            //}

            //if (_uIARRRSkillDescGroup != null)
            //    _uIARRRSkillDescGroup.SetSkillData(skillBaseData, Managers.ARRRSkillManager.Instance.GetSkillLevel(skillBaseData));

            if (Managers.ARRRSkillManager.Instance.IsFullEquipSkillSlot() == true
                && Managers.ARRRSkillManager.Instance.IsEquipSkill(_currentSelectSkill) == false)
            {
                if (_equipSkillBtn != null)
                    _equipSkillBtn.interactable = false;

                if (_equipSkillText != null)
                { 
                    Managers.LocalStringManager.Instance.SetLocalizeText(_equipSkillText, "common/ui/slotfull");
                    _equipSkillText.color = Color.red;
                }
            }
            else
            {
                if (_equipSkillBtn != null)
                    _equipSkillBtn.interactable = true;

                if (_equipSkillText != null)
                {
                    if (Managers.ARRRSkillManager.Instance.IsEquipSkill(_currentSelectSkill))
                        Managers.LocalStringManager.Instance.SetLocalizeText(_equipSkillText, "common/ui/unequip");
                    else
                        Managers.LocalStringManager.Instance.SetLocalizeText(_equipSkillText, "common/ui/equip");

                    _equipSkillText.color = Color.white;
                }
            }

            if (Managers.ARRRSkillManager.Instance.CanLevelUp(aRRRSkillData) == true)
            {
                if (_enhanceBtn != null)
                    _enhanceBtn.interactable = true;

                if (_enhanceText != null)
                {
                    _enhanceText.SetText(Managers.LocalStringManager.Instance.GetLocalString("common/ui/levelUp"));
                    _enhanceText.color = Color.white;
                }
            }
            else
            {
                bool isMaxLevel = Managers.ARRRSkillManager.Instance.IsMaxLevel(aRRRSkillData);

                if (_enhanceBtn != null)
                    _enhanceBtn.interactable = false;

                if (_enhanceText != null)
                {
                    _enhanceText.SetText(isMaxLevel == true ? "MaxLevel" : Managers.LocalStringManager.Instance.GetLocalString("common/ui/levelUp"));
                    _enhanceText.color = isMaxLevel == true ? Color.gray : Color.red;
                }
            }

        }
        //------------------------------------------------------------------------------------
        private void OnClick_EquipSkillBtn()
        {
            if (Managers.ARRRSkillManager.Instance.IsEquipSkill(_currentSelectSkill) == false)
            {
                if (Managers.ARRRSkillManager.Instance.EquipSkill(_currentSelectSkill) == true)
                {
                    if (_spawnSkillElement_Dic.ContainsKey(_currentSelectSkill) == true)
                    {
                        UIARRRSkillElement uIARRRSkillElement = _spawnSkillElement_Dic[_currentSelectSkill];
                        uIARRRSkillElement.SetEquipElement(true);
                    }

                    RefreshSkillSlot();

                    SetDetailSkillElement(_currentSelectSkill);
                }
            }
            else
            {
                if (Managers.ARRRSkillManager.Instance.UnEquipSkillSlot(_currentSelectSkill) == true)
                {
                    if (_spawnSkillElement_Dic.ContainsKey(_currentSelectSkill) == true)
                    {
                        UIARRRSkillElement uIARRRSkillElement = _spawnSkillElement_Dic[_currentSelectSkill];
                        uIARRRSkillElement.SetEquipElement(false);
                    }

                    RefreshSkillSlot();

                    SetDetailSkillElement(_currentSelectSkill);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SkillLevelUpBtn()
        {
            if (Managers.ARRRSkillManager.Instance.DoLevelUp(_currentSelectSkill))
            {
                if (_spawnSkillElement_Dic.ContainsKey(_currentSelectSkill) == true)
                {
                    UIARRRSkillElement uIARRRSkillElement = _spawnSkillElement_Dic[_currentSelectSkill];
                    uIARRRSkillElement.SetSkillElement(_currentSelectSkill);
                }

                SetDetailSkillElement(_currentSelectSkill);
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Level
        //------------------------------------------------------------------------------------
        private void SetARRRLevel()
        {
            if (_levelText != null)
                _levelText.SetText("Lv.{0}", Managers.ARRRStatManager.Instance.GetCharacterLevel().GetDecrypted());
        }
        //------------------------------------------------------------------------------------
        private void SetEnhanceBtn()
        {
            ObscuredInt level = Managers.ARRRStatManager.Instance.GetCharacterLevel();

            if (Managers.ARRRStatManager.Instance.NeedLimitBreak(level) == true)
            {
                NeedLimitBreak(level);
                return;
            }

            if (Managers.ARRRStatManager.Instance.IsMaxLevel(level))
            {
                SetMaxLevel();
                return;
            }

            SetLevelUp(level);
        }
        //------------------------------------------------------------------------------------
        private void NeedLimitBreak(ObscuredInt level)
        {
            bool readyCost = true;

            if (_levelUpBtn != null)
                _levelUpBtn.gameObject.SetActive(false);

            if (_limitUpBtn != null)
                _limitUpBtn.gameObject.SetActive(true);

            if (_maxBtn != null)
                _maxBtn.gameObject.SetActive(false);

            if (_enhanceMaterialGroup != null)
                _enhanceMaterialGroup.gameObject.SetActive(true);

            CharacterLevelUpLimitData characterLevelUpLimitData = Managers.ARRRStatManager.Instance.GetCharacterLevelUpLimitData(level);
            if (characterLevelUpLimitData == null)
                return;

            for (int i = 0; i < characterLevelUpLimitData.LimitCostGoods.Count; ++i)
            {
                CharacterLevelUpLimitCost characterLevelUpLimitCost = characterLevelUpLimitData.LimitCostGoods[i];
                SetMaterialElement(characterLevelUpLimitCost.LimitBreakCostGoodsType, characterLevelUpLimitCost.LimitBreakCostGoodsParam1, characterLevelUpLimitCost.LimitBreakCostGoodsParam2, i);

                if (Managers.ARRRStatManager.Instance.ReadyCharacterLimitLevelUpCost(characterLevelUpLimitCost) == false)
                    readyCost = false;
            }

            for (int i = characterLevelUpLimitData.LimitCostGoods.Count; i < _enhanceMaterials.Count; ++i)
            {
                if (_enhanceMaterials[i].MaterialElement != null)
                    _enhanceMaterials[i].MaterialElement.gameObject.SetActive(false);
            }

            if (_limitUpBtn != null)
                _limitUpBtn.interactable = readyCost;

            if (_levelUpDimmed != null)
                _levelUpDimmed.gameObject.SetActive(readyCost == false);
        }
        //------------------------------------------------------------------------------------
        private void SetMaxLevel()
        {
            if (_levelUpBtn != null)
                _levelUpBtn.gameObject.SetActive(false);

            if (_limitUpBtn != null)
                _limitUpBtn.gameObject.SetActive(false);

            if (_maxBtn != null)
                _maxBtn.gameObject.SetActive(true);

            if (_enhanceMaterialGroup != null)
                _enhanceMaterialGroup.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void SetLevelUp(ObscuredInt level)
        {
            bool readyCost = true;

            if (_levelUpBtn != null)
                _levelUpBtn.gameObject.SetActive(true);

            if (_limitUpBtn != null)
                _limitUpBtn.gameObject.SetActive(false);

            if (_maxBtn != null)
                _maxBtn.gameObject.SetActive(false);

            if (_enhanceMaterialGroup != null)
                _enhanceMaterialGroup.gameObject.SetActive(true);

            CharacterLevelUpCostData characterLevelUpCostData = Managers.ARRRStatManager.Instance.GetCharacterLevelUpCostData(level);
            if (characterLevelUpCostData == null)
                return;

            for (int i = 0; i < characterLevelUpCostData.LevelUpCostGoods.Count; ++i)
            {
                CharacterLevelUpCost characterLevelUpCost = characterLevelUpCostData.LevelUpCostGoods[i];
                SetMaterialElement(
                    characterLevelUpCost.LevelUpCostGoodsType, 
                    characterLevelUpCost.LevelUpCostGoodsParam1,
                    Managers.ARRRStatManager.Instance.GetLevelUpCostAmount(characterLevelUpCost, level), 
                    i);

                if (Managers.ARRRStatManager.Instance.ReadyCharacterLevelUpCost(characterLevelUpCost, level) == false)
                    readyCost = false;
            }

            for (int i = characterLevelUpCostData.LevelUpCostGoods.Count; i < _enhanceMaterials.Count; ++i)
            {
                if (_enhanceMaterials[i].MaterialElement != null)
                    _enhanceMaterials[i].MaterialElement.gameObject.SetActive(false);
            }

            if (_levelUp_PushBtn != null)
                _levelUp_PushBtn.enabled = readyCost;

            if (_levelUpBtn != null)
                _levelUpBtn.interactable = readyCost;

            if (_levelUpDimmed != null)
                _levelUpDimmed.gameObject.SetActive(readyCost == false);
        }
        //------------------------------------------------------------------------------------
        private void SetMaterialElement(V2Enum_Goods v2Enum_Goods, ObscuredInt index, ObscuredDouble amount, int i)
        {
            CharacterEnhanceMaterial characterEnhanceMaterial = null;
            RewardData rewardData = null;

            if (i < _enhanceMaterials.Count)
                characterEnhanceMaterial = _enhanceMaterials[i];
            else
                return;

            if (i < _enhanceMaterial_RewardDatas.Count)
                rewardData = _enhanceMaterial_RewardDatas[i];
            else
            {
                rewardData = new RewardData();
                _enhanceMaterial_RewardDatas.Add(rewardData);
            }

            rewardData.V2Enum_Goods = v2Enum_Goods;
            rewardData.Index = index;
            rewardData.Amount = 0;

            characterEnhanceMaterial.MaterialElement.SetRewardElement(rewardData);
            characterEnhanceMaterial.MaterialElement.gameObject.SetActive(true);

            double CurrentAmount = Managers.GoodsManager.Instance.GetGoodsAmount(v2Enum_Goods.Enum32ToInt(), index);

            double LevelUpAmount = amount;


            if (characterEnhanceMaterial.MaterialCount != null)
            {
                characterEnhanceMaterial.MaterialCount.text = string.Format("{0}/{1}", CurrentAmount, LevelUpAmount);
                characterEnhanceMaterial.MaterialCount.color = CurrentAmount >= LevelUpAmount ? Color.white : Color.red;
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_LevelUpBtn()
        {
            if (Managers.ARRRStatManager.Instance.DoARRRLevelUp() == true)
            {
                RefreshStatViewer();

                SetARRRLevel();
                SetEnhanceBtn();
                PlayLevelUpEffect();
            }
        }
        //------------------------------------------------------------------------------------
        private void PlayLevelUpEffect()
        {
            int level = Managers.ARRRStatManager.Instance.GetCharacterLevel().GetDecrypted();

            if (_uILobbyCharLvUpEffectPoolPlayIndex >= _uILobbyCharLvUpEffect_MaxPoolCount)
                _uILobbyCharLvUpEffectPoolPlayIndex = 0;

            UILobbyCharLvUp uILobbyCharLvUp = null;

            if (_uILobbyCharLvUpEffectPool.Count > _uILobbyCharLvUpEffectPoolPlayIndex)
            {
                uILobbyCharLvUp = _uILobbyCharLvUpEffectPool[_uILobbyCharLvUpEffectPoolPlayIndex];
            }
            else
            {
                GameObject clone = Instantiate(_uILobbyCharLvUpEffect.gameObject, _uILobbyCharLvUpEffect_Root);
                clone.transform.ResetLocal();
                uILobbyCharLvUp = clone.GetComponent<UILobbyCharLvUp>();
                _uILobbyCharLvUpEffectPool.Add(uILobbyCharLvUp);
            }

            uILobbyCharLvUp.gameObject.SetActive(true);
            uILobbyCharLvUp.transform.SetAsLastSibling();
            uILobbyCharLvUp.SetText(string.Format("Lv.{0}", level));
            uILobbyCharLvUp.PlayEffect();

            _uILobbyCharLvUpEffectPoolPlayIndex++;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_LimitUpBtn()
        {
            if (Managers.ARRRStatManager.Instance.DoARRRLimitUp() == true)
            {
                RefreshStatViewer();

                SetARRRLevel();
                SetEnhanceBtn();
            }
        }
        //------------------------------------------------------------------------------------
        #endregion

    }
}