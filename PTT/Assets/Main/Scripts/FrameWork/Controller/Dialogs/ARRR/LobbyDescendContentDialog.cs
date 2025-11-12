using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gpm.Ui;


namespace GameBerry.UI
{
    public class LobbyDescendContentDialog : IDialog
    {
        [Header("------------Guide------------")]
        [SerializeField]
        private Transform _guideBG;

        [SerializeField]
        private Button _guide2CompleteBtn;



        [Header("------------DescendSlot------------")]
        [SerializeField]
        private LayoutGroup _skillSlotLayOut;

        [SerializeField]
        private RectTransform _skillSlotLayOut_Rect;

        [SerializeField]
        private Transform _skillSlotElementRoot;

        [SerializeField]
        private UIARRRSkillSlotElement m_uISkillSlotElement;

        private Dictionary<ObscuredInt, UIARRRSkillSlotElement> m_spawnSkillSlotElement_Dic = new Dictionary<ObscuredInt, UIARRRSkillSlotElement>();


        [Header("-------SynergyGroup-------")]
        [SerializeField]
        private Transform _synergyTierGroupListRoot;

        [SerializeField]
        private UIDescendElement _uIGambleSynergyTierGroupElement;

        private Dictionary<ObscuredInt, UIDescendElement> _uIGambleSynergyTierGroupElement_dic = new Dictionary<ObscuredInt, UIDescendElement>();


        [Header("----------Stack----------")]
        [SerializeField]
        private Transform _uIStackElement_Root;

        [SerializeField]
        private UISynergyCombineStackElement _uISynergyCombineStackElement;

        private List<UISynergyCombineStackElement> _uIARRRGambleAddSkillElementPool = new List<UISynergyCombineStackElement>();


        [Header("-------Breakthrough-------")]
        [SerializeField]
        private List<UILobbyDescendBreakthroughtElement> _uISynergyRuneElements = new List<UILobbyDescendBreakthroughtElement>();

        [SerializeField]
        private Image _getBreakthroughPriceIcon;

        [SerializeField]
        private TMP_Text _getBreakthroughtTitle;

        [SerializeField]
        private TMP_Text _getBreakthroughPrice;

        [SerializeField]
        private Button _getBreakthroughBtn;

        [SerializeField]
        private Transform _breakthroughMax;

        [SerializeField]
        private TMP_Text _breakUnLockCount;

        [SerializeField]
        private Transform _breakthroughLock;

        [SerializeField]
        private TMP_Text _breakthroughUnLockNotice;

        [SerializeField]
        private Transform _breakthroughImpossibleNotice;

        [Header("-------EffectDesc-------")]
        [SerializeField]
        private UIARRRSkillDescGroup uIARRRSkillDescGroup;

        [SerializeField]
        private UIARRRSkillDescGroup uIARRRSkillDescGroup_NextLevel;


        [SerializeField]
        private List<ContentSizeFitter> customRefreshSizeFilter = new List<ContentSizeFitter>();

        [SerializeField]
        private List<RectTransform> customRefresh;

        [SerializeField]
        private Transform _synergyEffect_Enhance_Max;


        [SerializeField]
        private TMP_Text _synergyEffectLevel;

        [SerializeField]
        private Button _synergyEffect_Equip;

        [SerializeField]
        private TMP_Text _synergyEffect_EquipText;

        [SerializeField]
        private Button _synergyEffect_Get;

        [SerializeField]
        private Image _synergyEffect_GetPriceIcon;

        [SerializeField]
        private TMP_Text _synergyEffect_GetPrice;

        [SerializeField]
        private TMP_Text _synergyEffect_GetPriceTitle;

        [SerializeField]
        private Button _synergyEffect_CannotGet;

        [SerializeField]
        private Button _synergyEffect_Enhance;

        [SerializeField]
        private TMP_Text _synergyEffect_EnhanceText;

        [SerializeField]
        private Image _synergyEffect_EnhanceCountIcon;

        [SerializeField]
        private TMP_Text _synergyEffect_EnhanceCountText;


        [SerializeField]
        private Transform _synergyEffect_Enhance2Group;

        [SerializeField]
        private Image _synergyEffect_Enhance2CountIcon;

        [SerializeField]
        private TMP_Text _synergyEffect_Enhance2CountText;


        [SerializeField]
        private Color _buttonTextEnableColor;

        [SerializeField]
        private Material _buttonTextEnableMaterial;

        [SerializeField]
        private Color _buttonTextDisaEnableColor;

        [SerializeField]
        private Material _buttonTextDisableMaterial;


        [Header("-------ReinforceStat-------")]
        [SerializeField]
        private TMP_Text _reinforceStat_Attack;

        [SerializeField]
        private TMP_Text _reinforceStat_Defence;

        [SerializeField]
        private TMP_Text _reinforceStat_HP;


        private DescendData _currentSynergyEffectData;

        public int ingameLevel = 0;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            foreach (var pair in Managers.DescendManager.Instance.GetEquipSynergyEffect())
            {
                UIARRRSkillSlotElement element = CreateSlot();
                element.SetSlotID(pair.Key);
                DescendData descendData = Managers.DescendManager.Instance.GetSynergyEffectData(pair.Value);

                element.SetSkill(descendData);
                element.VisibleLock(Managers.DescendManager.Instance.IsOpenDescendSlot(pair.Key) == false);
                m_spawnSkillSlotElement_Dic.Add(pair.Key, element);
            }

            List<DescendData> newdescend = new List<DescendData>();

            foreach (var pair in Managers.DescendManager.Instance.GetAllSynergyEffectDatas())
            {
                newdescend.Add(pair.Value);
            }

            newdescend.Sort((x, y) =>
            {
                if (x.Grade > y.Grade)
                    return 1;
                else if (x.Grade < y.Grade)
                    return -1;
                else
                {
                    if (x.Index > y.Index)
                        return -1;
                    else if (x.Index < y.Index)
                        return 1;
                }

                return 0;
            });

            foreach (var pair in newdescend)
            {
                GameObject clone = Instantiate(_uIGambleSynergyTierGroupElement.gameObject, _synergyTierGroupListRoot);

                UIDescendElement uIGambleSynergyListElement = clone.GetComponent<UIDescendElement>();
                uIGambleSynergyListElement.Init(OnClick_DescendElement);
                uIGambleSynergyListElement.SetDescendData(pair);
                _uIGambleSynergyTierGroupElement_dic.Add(pair.Index, uIGambleSynergyListElement);
            }

            if (_synergyEffect_Equip != null)
                _synergyEffect_Equip.onClick.AddListener(OnClick_ChangeEquipSynergy);

            if (_synergyEffect_Enhance != null)
                _synergyEffect_Enhance.onClick.AddListener(OnClick_EnhanceSynergy);

            if (_synergyEffect_Get != null)
                _synergyEffect_Get.onClick.AddListener(OnClick_SynergyGet);

            if (_getBreakthroughBtn != null)
                _getBreakthroughBtn.onClick.AddListener(OnClick_GetBreakthrough);

            if (_guide2CompleteBtn != null)
                _guide2CompleteBtn.onClick.AddListener(() =>
                {
                    Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/descendskill3"));
                    if (_guideBG != null)
                        _guideBG.gameObject.SetActive(false);
                });

            Message.AddListener<GameBerry.Event.ChangeEquipDescendMsg>(ChangeEquipSynergy);
            Message.AddListener<GameBerry.Event.ShowNewDescendMsg>(ShowNewSynergy);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.ChangeEquipDescendMsg>(ChangeEquipSynergy);
            Message.RemoveListener<GameBerry.Event.ShowNewDescendMsg>(ShowNewSynergy);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            RefreshAllElement();

            if (_currentSynergyEffectData == null)
            {
                DescendData descendData = null;

                foreach (var pair in Managers.DescendManager.Instance.GetAllSynergyEffectDatas())
                {
                    if (descendData == null)
                        descendData = pair.Value;

                    if (Managers.DescendManager.Instance.GetSynergyEffectSkillInfo(pair.Value) != null)
                        _currentSynergyEffectData = pair.Value;
                }

                if (_currentSynergyEffectData == null)
                    _currentSynergyEffectData = descendData;
            }

            SetSynergyEffectDetail(_currentSynergyEffectData);

            if (Managers.GuideInteractorManager.Instance.PlayDescendChangeTutorial == true)
            {
                if (_guide2CompleteBtn != null)
                    _guide2CompleteBtn.interactable = true;

                if (_guideBG != null)
                    _guideBG.gameObject.SetActive(true);

                Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/descendskill2"));
                PlayNextWaveDelay().Forget();
            }
            else
            {
                if (_guide2CompleteBtn != null)
                    _guide2CompleteBtn.interactable = false;

                if (_guideBG != null)
                    _guideBG.gameObject.SetActive(false);
            }

            if (playedOnce == false)
                PlayOnceFrame().Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayNextWaveDelay()
        {
            await UniTask.NextFrame();
            Managers.GuideInteractorManager.Instance.SetGuideStep(2);
        }
        //------------------------------------------------------------------------------------
        bool playedOnce = false;
        private async UniTask PlayOnceFrame()
        {
            await UniTask.NextFrame();

            if (_skillSlotLayOut != null)
                _skillSlotLayOut.enabled = false;

            playedOnce = true;
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.DescendChange)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(1);
            }
            Managers.DescendManager.Instance.RemoveAllNewIconSynergy();
            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyDescend);
        }
        //------------------------------------------------------------------------------------
        //private async UniTask PlayNextWaveDelay()
        //{
        //    await UniTask.NextFrame();
        //    Managers.GuideInteractorManager.Instance.SetGuideStep(2);
        //}
        //------------------------------------------------------------------------------------
        private void RefreshSkillSlot()
        {
            foreach (var pair in Managers.DescendManager.Instance.GetEquipSynergyEffect())
            {
                if (m_spawnSkillSlotElement_Dic.ContainsKey(pair.Key) == false)
                    continue;

                DescendData descendData = Managers.DescendManager.Instance.GetSynergyEffectData(pair.Value);

                UIARRRSkillSlotElement element = m_spawnSkillSlotElement_Dic[pair.Key];
                if (element != null)
                { 
                    element.SetSkill(descendData);
                    element.VisibleLock(Managers.DescendManager.Instance.IsOpenDescendSlot(pair.Key) == false);
                }
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

            if (Managers.DescendManager.Instance.IsOpenDescendSlot(slotid) == false)
                Managers.DescendManager.Instance.ShowNoticeDescendSlotUnLockExp(slotid);

            DescendData aRRRSkillData = Managers.DescendManager.Instance.UnEquipSkillSlot(slotid);

            if (aRRRSkillData != null)
            {
                if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(aRRRSkillData.Index) == true)
                {
                    UIDescendElement uIARRRSkillElement = _uIGambleSynergyTierGroupElement_dic[aRRRSkillData.Index];
                    uIARRRSkillElement.EnableEquipElement(false);
                }
            }

            RefreshSkillSlot();

            RefreshAllElement();

            SetSynergyEffectDetail(_currentSynergyEffectData);
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

            RefreshAllElement();
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

            Managers.DescendManager.Instance.SweepSkillSlot(dragSlotIdx, overSlotIdx);

            _dragSlotIdx = -1;
            _overSlotIdx = -1;

            RefreshSkillSlot();

            RefreshAllElement();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ChangeEquipSynergy()
        {
            if (Managers.DescendManager.Instance.IsEquipSkill(_currentSynergyEffectData) == false)
            {
                if (Managers.DescendManager.Instance.EquipSkill(_currentSynergyEffectData) == true)
                {
                    if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(_currentSynergyEffectData.Index) == true)
                    {
                        UIDescendElement uIARRRSkillElement = _uIGambleSynergyTierGroupElement_dic[_currentSynergyEffectData.Index];
                        uIARRRSkillElement.EnableEquipElement(true);
                    }

                    RefreshSkillSlot();

                    SetSynergyEffectDetail(_currentSynergyEffectData);
                    RefreshAllElement();
                }
            }
            else
            {
                if (Managers.DescendManager.Instance.UnEquipSkillSlot(_currentSynergyEffectData) == true)
                {
                    if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(_currentSynergyEffectData.Index) == true)
                    {
                        UIDescendElement uIARRRSkillElement = _uIGambleSynergyTierGroupElement_dic[_currentSynergyEffectData.Index];
                        uIARRRSkillElement.EnableEquipElement(false);
                    }

                    RefreshSkillSlot();

                    SetSynergyEffectDetail(_currentSynergyEffectData);
                    RefreshAllElement();
                }
            }
        }
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        private void OnClick_EnhanceSynergy()
        {
            if (_currentSynergyEffectData == null)
                return;

            {
                if (Managers.DescendManager.Instance.EnhanceSynergy(_currentSynergyEffectData) == true)
                {
                    SetSynergyEffectDetail(_currentSynergyEffectData);
                    RefreshAllElement();

                    if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(_currentSynergyEffectData.Index) == true)
                    {
                        UIDescendElement uIRelicElement = _uIGambleSynergyTierGroupElement_dic[_currentSynergyEffectData.Index];
                        if (uIRelicElement != null)
                            uIRelicElement.PlayLevelUpEffect();
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SynergyGet()
        {
            if (_currentSynergyEffectData == null)
                return;

            if (Managers.DescendManager.Instance.GetSynergy(_currentSynergyEffectData) == true)
            {
                SetSynergyEffectDetail(_currentSynergyEffectData);
                RefreshAllElement();
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshAllElement()
        {
            foreach (var pair in Managers.DescendManager.Instance.GetAllSynergyEffectDatas())
            {
                if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(pair.Key) == true)
                {
                    _uIGambleSynergyTierGroupElement_dic[pair.Key].SetDescendData(pair.Value);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_DescendElement(DescendData descendData)
        {
            SetSynergyEffectDetail(descendData);
        }
        //------------------------------------------------------------------------------------
        private void SetSynergyRune(DescendData synergyEffectData)
        {
            if (_breakthroughImpossibleNotice != null)
            {
                if (synergyEffectData.SynergyRuneList == null || synergyEffectData.SynergyRuneList.Count == 0)
                {
                    _breakthroughImpossibleNotice.gameObject.SetActive(true);
                    return;
                }
                else
                    _breakthroughImpossibleNotice.gameObject.SetActive(false);
            }

            
            

            int count = 0;
            int unlockcount = 0;
            foreach (var pair in synergyEffectData.SynergyRuneList)
            {
                UILobbyDescendBreakthroughtElement uILobbySynergyEffectElement = null;

                if (_uISynergyRuneElements.Count > count)
                    uILobbySynergyEffectElement = _uISynergyRuneElements[count];
                else
                    break;

                uILobbySynergyEffectElement.SetSynergyEffectData(pair);
                uILobbySynergyEffectElement.gameObject.SetActive(true);

                if (Managers.DescendManager.Instance.IsGetedBreakthrough(pair) == true)
                    unlockcount++;

                count++;
            }


            if (_breakUnLockCount != null)
                _breakUnLockCount.SetText("{0}/{1}", unlockcount, synergyEffectData.SynergyRuneList.Count);

            for (int i = synergyEffectData.SynergyRuneList.Count; i < _uISynergyRuneElements.Count; ++i)
            {
                _uISynergyRuneElements[i].gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetSynergyEffectDetail(DescendData synergyEffectData)
        {
            if (synergyEffectData == null)
                return;

            if (_currentSynergyEffectData != null)
            {
                if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(_currentSynergyEffectData.Index) == true)
                {
                    UIDescendElement uILobbySynergyTierGroupElement = _uIGambleSynergyTierGroupElement_dic[_currentSynergyEffectData.Index];
                    uILobbySynergyTierGroupElement.EnableSelectElement(false);
                }
            }

            _currentSynergyEffectData = synergyEffectData;

            if (_currentSynergyEffectData != null)
            {
                if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(_currentSynergyEffectData.Index) == true)
                {
                    UIDescendElement uILobbySynergyTierGroupElement = _uIGambleSynergyTierGroupElement_dic[_currentSynergyEffectData.Index];
                    uILobbySynergyTierGroupElement.EnableSelectElement(true);
                }
            }

            if (_synergyEffect_Enhance2Group != null)
                _synergyEffect_Enhance2Group.gameObject.SetActive(false);


            SkillInfo skillInfo = Managers.DescendManager.Instance.GetSynergyEffectSkillInfo(synergyEffectData);


            int level = skillInfo == null ? Define.PlayerDescendDefaultLevel : skillInfo.Level;

            if (_breakthroughUnLockNotice != null)
            {
                _breakthroughUnLockNotice.SetText(Managers.LocalStringManager.Instance.GetLocalString("synergy/breakthroughlock"));
            }

            bool lockBreak = Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.SynergyBreakthrough) == false;

            //StageInfo stageInfo = Managers.MapManager.Instance.GetStageInfo(Define.SynergyBreakTutorialStage);

            //if (stageInfo != null)
            //{
            //    lockBreak = stageInfo.RecvClearReward < Define.SynergyBreakTutorialWave;
            //}

            if (_breakthroughLock != null)
                _breakthroughLock.gameObject.SetActive(lockBreak);

            if (skillInfo == null)
            {
                if (uIARRRSkillDescGroup != null)
                    uIARRRSkillDescGroup.SetSkillData(synergyEffectData, Define.PlayerSynergyDefaultLevel);

                if (uIARRRSkillDescGroup_NextLevel != null)
                    uIARRRSkillDescGroup_NextLevel.SetSkillData(synergyEffectData, Define.PlayerSynergyDefaultLevel + 1);

                if (_synergyEffect_Equip != null)
                    _synergyEffect_Equip.gameObject.SetActive(false);

                //if (_synergyEffect_Equip != null)
                //    _synergyEffect_Equip.interactable = false;

                if (_synergyEffect_Enhance != null)
                    _synergyEffect_Enhance.gameObject.SetActive(false);

                if (_synergyEffectLevel != null)
                {
                    _synergyEffectLevel.SetText("Lv.{0}", Define.PlayerDescendDefaultLevel);
                }

                if (_synergyEffect_Enhance != null)
                    _synergyEffect_Enhance.gameObject.SetActive(false);

                if (_synergyEffect_Enhance_Max != null)
                    _synergyEffect_Enhance_Max.gameObject.SetActive(false);


                if (_reinforceStat_Attack != null)
                    _reinforceStat_Attack.SetText("-");
                if (_reinforceStat_Defence != null)
                    _reinforceStat_Defence.SetText("-");
                if (_reinforceStat_HP != null)
                    _reinforceStat_HP.SetText("-");

                DescendOpenCostData synergyConditionData = synergyEffectData.DescendOpenCostData;
                if (synergyConditionData != null)
                {
                    if (synergyConditionData.OpenCostGoodsIndex == -1)
                    {
                        if (_synergyEffect_Get != null)
                            _synergyEffect_Get.gameObject.SetActive(false);

                        if (_synergyEffect_CannotGet != null)
                            _synergyEffect_CannotGet.gameObject.SetActive(true);
                    }
                    else
                    {
                        if (_synergyEffect_Get != null)
                            _synergyEffect_Get.gameObject.SetActive(true);

                        if (_synergyEffect_CannotGet != null)
                            _synergyEffect_CannotGet.gameObject.SetActive(false);

                        int costIndex = synergyConditionData.OpenCostGoodsIndex;

                        int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                        int needCount = (int)synergyConditionData.OpenCostGoodsValue;

                        bool readyEnhance = currentCount >= needCount;

                        Sprite pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);

                        if (_synergyEffect_GetPriceIcon != null)
                            _synergyEffect_GetPriceIcon.sprite = pointsprite;

                        if (_synergyEffect_GetPrice != null)
                        {
                            _synergyEffect_GetPrice.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                            _synergyEffect_GetPrice.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                            _synergyEffect_GetPrice.SetText("{0}", needCount);
                        }
                    }
                }
                else
                {
                    if (_synergyEffect_Get != null)
                        _synergyEffect_Get.gameObject.SetActive(false);

                    if (_synergyEffect_CannotGet != null)
                        _synergyEffect_CannotGet.gameObject.SetActive(true);
                }
            }
            else
            {
                if (_synergyEffect_Get != null)
                    _synergyEffect_Get.gameObject.SetActive(false);

                if (uIARRRSkillDescGroup != null)
                    uIARRRSkillDescGroup.SetSkillData(synergyEffectData, skillInfo.Level);

                if (_synergyEffectLevel != null)
                {
                    _synergyEffectLevel.SetText("Lv.{0}", skillInfo.Level);
                }

                if (uIARRRSkillDescGroup_NextLevel != null)
                {
                    if (Managers.DescendManager.Instance.IsMaxLevelSynergy(synergyEffectData) == false)
                    {
                        uIARRRSkillDescGroup_NextLevel.gameObject.SetActive(true);
                        uIARRRSkillDescGroup_NextLevel.SetSkillData(synergyEffectData, skillInfo.Level + 1);
                    }
                    else
                        uIARRRSkillDescGroup_NextLevel.gameObject.SetActive(false);
                }


                if (_synergyEffect_Equip != null)
                    _synergyEffect_Equip.gameObject.SetActive(true);

                if (Managers.DescendManager.Instance.IsFullEquipSkillSlot() == true
                && Managers.DescendManager.Instance.IsEquipSkill(synergyEffectData) == false)
                {
                    if (_synergyEffect_Equip != null)
                        _synergyEffect_Equip.interactable = false;

                    if (_synergyEffect_EquipText != null)
                    {
                        Managers.LocalStringManager.Instance.SetLocalizeText(_synergyEffect_EquipText, "common/ui/slotfull");
                        _synergyEffect_EquipText.color = _buttonTextDisaEnableColor;
                    }
                }
                else
                {
                    if (_synergyEffect_Equip != null)
                        _synergyEffect_Equip.interactable = true;

                    if (_synergyEffect_EquipText != null)
                    {
                        if (Managers.DescendManager.Instance.IsEquipSkill(synergyEffectData))
                            Managers.LocalStringManager.Instance.SetLocalizeText(_synergyEffect_EquipText, "common/ui/unequip");
                        else
                            Managers.LocalStringManager.Instance.SetLocalizeText(_synergyEffect_EquipText, "common/ui/equip");

                        _synergyEffect_EquipText.color = _buttonTextEnableColor;
                    }
                }



                if (Managers.DescendManager.Instance.NeedLimitBreak(synergyEffectData) == true)
                {
                    DescendBreakthroughCostData synergyLevelUpLimitData = Managers.DescendManager.Instance.GetSynergyLevelUpLimitData(skillInfo.LimitCompleteLevel);

                    int costIndex = synergyLevelUpLimitData.LimitBreakCostGoodsIndex;

                    int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                    int needCount = synergyLevelUpLimitData.LimitBreakCostGoodsValue;

                    bool readyEnhance = Managers.DescendManager.Instance.ReadyCharacterLimitLevelUpCost(synergyLevelUpLimitData);

                    Sprite pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);



                    if (_getBreakthroughBtn != null)
                    {
                        _getBreakthroughBtn.gameObject.SetActive(true);
                    }

                    if (_breakthroughMax != null)
                        _breakthroughMax.gameObject.SetActive(false);

                    if (_getBreakthroughtTitle != null)
                    {
                        _getBreakthroughtTitle.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                        _getBreakthroughtTitle.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                        Managers.LocalStringManager.Instance.SetLocalizeText(_getBreakthroughtTitle, "common/ui/promote");
                    }

                    if (_getBreakthroughPrice != null)
                    {
                        _getBreakthroughPrice.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                        _getBreakthroughPrice.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                        _getBreakthroughPrice.SetText("{0}/{1}", currentCount, needCount);
                    }

                    if (_getBreakthroughPriceIcon != null)
                    {
                        _getBreakthroughPriceIcon.sprite = pointsprite;
                        _getBreakthroughPriceIcon.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (_getBreakthroughBtn != null)
                    {
                        _getBreakthroughBtn.gameObject.SetActive(false);
                    }

                    if (_breakthroughMax != null)
                        _breakthroughMax.gameObject.SetActive(true);
                }

                {
                    bool isMax = Managers.DescendManager.Instance.IsMaxLevelSynergy(synergyEffectData);

                    if (_synergyEffect_Enhance != null)
                        _synergyEffect_Enhance.gameObject.SetActive(isMax == false);

                    if (_synergyEffect_Enhance_Max != null)
                        _synergyEffect_Enhance_Max.gameObject.SetActive(isMax == true);


                    if (isMax == true)
                    {
                        if (_synergyEffect_EnhanceText != null)
                        {
                            _synergyEffect_EnhanceText.color = _buttonTextEnableColor;
                            _synergyEffect_EnhanceText.fontMaterial = _buttonTextEnableMaterial;
                            Managers.LocalStringManager.Instance.SetLocalizeText(_synergyEffect_EnhanceText, "Max");
                        }

                        if (_synergyEffect_EnhanceCountText != null)
                        {
                            _synergyEffect_EnhanceCountText.color = Color.white;
                            _synergyEffect_EnhanceCountText.SetText("-");
                        }

                        if (_synergyEffect_EnhanceCountIcon != null)
                            _synergyEffect_EnhanceCountIcon.gameObject.SetActive(false);
                    }
                    else
                    {
                        int costIndex = Managers.DescendManager.Instance.GetSynergyEnhanceCostGoodsIndex1(synergyEffectData);

                        int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                        int needCount = Managers.DescendManager.Instance.GetSynergyEnhance_NeedCount1(synergyEffectData);

                        bool readyEnhance = currentCount >= needCount;

                        Sprite pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);

                        if (_synergyEffect_EnhanceText != null)
                        {
                            _synergyEffect_EnhanceText.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                            _synergyEffect_EnhanceText.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                            Managers.LocalStringManager.Instance.SetLocalizeText(_synergyEffect_EnhanceText, "ui/reinforce");
                        }

                        if (_synergyEffect_EnhanceCountText != null)
                        {
                            _synergyEffect_EnhanceCountText.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                            _synergyEffect_EnhanceCountText.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                            _synergyEffect_EnhanceCountText.SetText("{0}", needCount);
                        }

                        if (_synergyEffect_EnhanceCountIcon != null)
                        {
                            _synergyEffect_EnhanceCountIcon.sprite = pointsprite;
                            _synergyEffect_EnhanceCountIcon.gameObject.SetActive(true);
                        }





                        costIndex = Managers.DescendManager.Instance.GetSynergyEnhanceCostGoodsIndex2(synergyEffectData);

                        currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                        needCount = Managers.DescendManager.Instance.GetSynergyEnhance_NeedCount2(synergyEffectData);

                        readyEnhance = currentCount >= needCount;

                        pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);


                        if (costIndex != -1)
                        {
                            if (_synergyEffect_Enhance2Group != null)
                                _synergyEffect_Enhance2Group.gameObject.SetActive(true);

                            if (_synergyEffect_Enhance2CountText != null)
                            {
                                _synergyEffect_Enhance2CountText.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                                _synergyEffect_Enhance2CountText.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                                _synergyEffect_Enhance2CountText.SetText("{0}", needCount);
                            }

                            if (_synergyEffect_Enhance2CountIcon != null)
                            {
                                _synergyEffect_Enhance2CountIcon.sprite = pointsprite;
                                _synergyEffect_Enhance2CountIcon.gameObject.SetActive(true);
                            }
                        }
                    }
                }

                DescendLevelUpStatData synergyReinforceStatData = Managers.DescendManager.Instance.GetSynergyReinforceStatData(skillInfo.Level);
                if (synergyReinforceStatData != null)
                {
                    if (synergyReinforceStatData.AccEffectStat.ContainsKey(V2Enum_Stat.Attack) == true)
                    {
                        if (_reinforceStat_Attack != null)
                            _reinforceStat_Attack.SetText(string.Format("{0:0.#}", synergyReinforceStatData.AccEffectStat[V2Enum_Stat.Attack]));
                    }

                    if (synergyReinforceStatData.AccEffectStat.ContainsKey(V2Enum_Stat.Defence) == true)
                    {
                        if (_reinforceStat_Defence != null)
                            _reinforceStat_Defence.SetText(string.Format("{0:0.#}", synergyReinforceStatData.AccEffectStat[V2Enum_Stat.Defence]));
                    }

                    if (synergyReinforceStatData.AccEffectStat.ContainsKey(V2Enum_Stat.HP) == true)
                    {
                        if (_reinforceStat_HP != null)
                            _reinforceStat_HP.SetText(string.Format("{0:0.#}", synergyReinforceStatData.AccEffectStat[V2Enum_Stat.HP]));
                    }
                }

            }

            SetSynergyStack(synergyEffectData.SynergyType);

            //SetDescendInGameLevelView(ingameLevel);

            SetSynergyRune(synergyEffectData);

            RefreshSizeFilter().Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask RefreshSizeFilter()
        {
            await UniTask.NextFrame();
            for (int i = 0; i < customRefreshSizeFilter.Count; ++i)
            {
                customRefreshSizeFilter[i].SetLayoutVertical();
            }

            if (customRefresh != null)
            {
                for (int i = 0; i < customRefresh.Count; ++i)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(customRefresh[i]);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void SetSynergyStack(HashSet<Enum_SynergyType> NeedSynergyCount)
        {
            {
                int i = 0;
                foreach (var pair in NeedSynergyCount)
                {
                    UISynergyCombineStackElement uISynergyCombineStackElement = null;
                    if (_uIARRRGambleAddSkillElementPool.Count > i)
                        uISynergyCombineStackElement = _uIARRRGambleAddSkillElementPool[i];
                    else
                    {
                        GameObject clone = Instantiate(_uISynergyCombineStackElement.gameObject, _uIStackElement_Root);
                        uISynergyCombineStackElement = clone.GetComponent<UISynergyCombineStackElement>();
                        _uIARRRGambleAddSkillElementPool.Add(uISynergyCombineStackElement);
                    }

                    uISynergyCombineStackElement.gameObject.SetActive(true);
                    uISynergyCombineStackElement.SetDescendSynergyStack(pair);

                    i++;
                }
            }

            for (int i = NeedSynergyCount.Count; i < _uIARRRGambleAddSkillElementPool.Count; ++i)
            {
                UISynergyCombineStackElement uISynergyCombineStackElement = _uIARRRGambleAddSkillElementPool[i];
                uISynergyCombineStackElement.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void ChangeEquipSynergy(GameBerry.Event.ChangeEquipDescendMsg msg)
        {
            RefreshSynergyEffectEquipState(msg.BeforeEquipSynergy, false);
            RefreshSynergyEffectEquipState(msg.AfterEquipSynergy, false);

            SetSynergyEffectDetail(_currentSynergyEffectData);
        }
        //------------------------------------------------------------------------------------
        private void RefreshSynergyEffectEquipState(DescendData synergyEffectData, bool equip)
        {
            if (synergyEffectData == null)
                return;

            if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(synergyEffectData.Index) == true)
            {
                UIDescendElement uILobbySynergyTierGroupElement = _uIGambleSynergyTierGroupElement_dic[synergyEffectData.Index];
                uILobbySynergyTierGroupElement.EnableEquipElement(equip);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_GetBreakthrough()
        {
            if (_currentSynergyEffectData == null)
                return;

            int targetidx = 0;

            SkillInfo skillInfo = Managers.DescendManager.Instance.GetSynergyEffectSkillInfo(_currentSynergyEffectData);
            if (skillInfo != null)
            {
                targetidx = skillInfo.LimitCompleteLevel;
            }

            if (Managers.DescendManager.Instance.DoARRRLimitUp(_currentSynergyEffectData) == true)
            {
                SetSynergyEffectDetail(_currentSynergyEffectData);

                if (_uISynergyRuneElements.Count > targetidx)
                    _uISynergyRuneElements[targetidx].PlayGetEffect();
            }
        }
        //------------------------------------------------------------------------------------
        private void ShowNewSynergy(GameBerry.Event.ShowNewDescendMsg msg)
        {
            DescendData synergyEffectData = null;
            if (msg != null)
                synergyEffectData = msg.NewSynergyEffectData;

            //if (synergyEffectData != null)
            //{
            //    UILobbySynergyTabElement uILobbySynergyTabElement = _uIGambleSynergyViewElement_dic[synergyEffectData.SynergyType];

            //    if (uILobbySynergyTabElement != null)
            //        uILobbySynergyTabElement.EnableNewDot(true);
            //}
            //else
            //{
            //    foreach (var pair in _uIGambleSynergyViewElement_dic)
            //    {
            //        UILobbySynergyTabElement uILobbySynergyTabElement = pair.Value;

            //        if (uILobbySynergyTabElement != null)
            //            uILobbySynergyTabElement.EnableNewDot(Managers.DescendManager.Instance.GetNewSynergyIconCount(pair.Key) > 0);
            //    }
            //}
        }
        //------------------------------------------------------------------------------------
    }
}