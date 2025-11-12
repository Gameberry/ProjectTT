using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class LobbyRelicContentDialog : IDialog
    {
        [Header("------------Guide------------")]
        [SerializeField]
        private Transform _guideBG;

        [SerializeField]
        private Button _guide2CompleteBtn;

        [Header("-------SynergyGroup-------")]
        [SerializeField]
        private Transform _synergyTierGroupListRoot;

        [SerializeField]
        private UIRelicElement _uIGambleSynergyTierGroupElement;

        private Dictionary<ObscuredInt, UIRelicElement> _uIGambleSynergyTierGroupElement_dic = new Dictionary<ObscuredInt, UIRelicElement>();


        [Header("-------EffectDesc-------")]
        [SerializeField]
        private UIARRRSkillDescGroup uIARRRSkillDescGroup;

        [SerializeField]
        private RectTransform customRefresh;

        [SerializeField]
        private List<ContentSizeFitter> customRefreshSizeFilter = new List<ContentSizeFitter>();

        [SerializeField]
        private ScrollRect _descScrollRect;


        [SerializeField]
        private TMP_Text _synergyEffectLevel;

        [SerializeField]
        private Button _synergyEffect_Get;

        [SerializeField]
        private Button _synergyEffect_Enhance;

        [SerializeField]
        private TMP_Text _synergyEffect_EnhanceText;

        [SerializeField]
        private Image _synergyEffect_EnhanceCountIcon;

        [SerializeField]
        private TMP_Text _synergyEffect_EnhanceCountText;

        [SerializeField]
        private Color _buttonTextEnableColor;

        [SerializeField]
        private Material _buttonTextEnableMaterial;

        [SerializeField]
        private Color _buttonTextDisaEnableColor;

        [SerializeField]
        private Material _buttonTextDisableMaterial;


        private RelicData _currentSynergyEffectData;


        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_guide2CompleteBtn != null)
                _guide2CompleteBtn.onClick.AddListener(() =>
                {
                    if (_guideBG != null)
                        _guideBG.gameObject.SetActive(false);
                });

            foreach (var pair in Managers.RelicManager.Instance.GetAllRelicData())
            {
                GameObject clone = Instantiate(_uIGambleSynergyTierGroupElement.gameObject, _synergyTierGroupListRoot);

                UIRelicElement uIGambleSynergyListElement = clone.GetComponent<UIRelicElement>();
                uIGambleSynergyListElement.Init(OnClick_DescendElement);
                uIGambleSynergyListElement.SetDescendData(pair.Value);
                _uIGambleSynergyTierGroupElement_dic.Add(pair.Key, uIGambleSynergyListElement);
                if (_currentSynergyEffectData == null && pair.Value != null)
                    _currentSynergyEffectData = pair.Value;
            }

            if (_synergyEffect_Enhance != null)
                _synergyEffect_Enhance.onClick.AddListener(OnClick_EnhanceSynergy);

            if (_synergyEffect_Get != null)
                _synergyEffect_Get.onClick.AddListener(() =>
                {
                    UIManager.DialogExit<LobbyRelicContentDialog>();
                    Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.ShopSummon_Relic);
                });


            Message.AddListener<GameBerry.Event.ShowNewRelicMsg>(ShowNewRelicMsg);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.ShowNewRelicMsg>(ShowNewRelicMsg);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            RefreshAllElement();

            if (_currentSynergyEffectData == null)
            {
                RelicData relicData = null;

                foreach (var pair in Managers.RelicManager.Instance.GetAllRelicData())
                {
                    if (relicData == null)
                        relicData = pair.Value;


                    if (Managers.RelicManager.Instance.GetSynergyEffectSkillInfo(pair.Value) != null)
                        _currentSynergyEffectData = pair.Value;
                }

                if (_currentSynergyEffectData == null)
                    _currentSynergyEffectData = relicData;
            }

            if (Managers.GuideInteractorManager.Instance.PlayRelicTutorial == true)
            {
                //if (_currentSynergyType == Enum_SynergyType.White)
                //{
                //    SynergyEffectData tuto = Managers.SynergyManager.Instance.GetSynergyEffectData(_tutorialIndex);
                //    if (tuto != null)
                //        SetSynergyEffectDetail(_currentSynergyEffectData);
                //}

                PlayNextWaveDelay().Forget();
            }


            SetSynergyEffectDetail(_currentSynergyEffectData);
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayNextWaveDelay()
        {
            await UniTask.NextFrame();
            if (Managers.GuideInteractorManager.Instance.PlayRelicTutorial == true)
            {
                Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/relic3"));

                if (_guideBG != null)
                    _guideBG.gameObject.SetActive(true);
            }

            Managers.GuideInteractorManager.Instance.SetGuideStep(4);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.RelicTutorial)
            {
                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 4)
                {
                    if (_guideBG != null)
                        _guideBG.gameObject.SetActive(false);

                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                }
            }

            //Managers.RelicManager.Instance.RefreshNewSynergyIcon();
            Managers.RelicManager.Instance.RemoveAllNewIconSynergy();

            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyRelic);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_EnhanceSynergy()
        {
            if (_currentSynergyEffectData == null)
                return;

            {
                if (Managers.RelicManager.Instance.EnhanceSynergy(_currentSynergyEffectData) == true)
                {
                    SetSynergyEffectDetail(_currentSynergyEffectData);
                    RefreshAllElement();

                    if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(_currentSynergyEffectData.Index) == true)
                    {
                        UIRelicElement uIRelicElement = _uIGambleSynergyTierGroupElement_dic[_currentSynergyEffectData.Index];
                        if (uIRelicElement != null)
                            uIRelicElement.PlayLevelUpEffect();
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshAllElement()
        {
            foreach (var pair in Managers.RelicManager.Instance.GetAllRelicData())
            {
                if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(pair.Key) == true)
                {
                    _uIGambleSynergyTierGroupElement_dic[pair.Key].SetDescendData(pair.Value);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_DescendElement(RelicData descendData)
        {
            SetSynergyEffectDetail(descendData);
        }
        //------------------------------------------------------------------------------------
        private void SetSynergyEffectDetail(RelicData synergyEffectData)
        {
            if (synergyEffectData == null)
                return;

            if (_currentSynergyEffectData != null)
            {
                if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(_currentSynergyEffectData.Index) == true)
                {
                    UIRelicElement uILobbySynergyTierGroupElement = _uIGambleSynergyTierGroupElement_dic[_currentSynergyEffectData.Index];
                    uILobbySynergyTierGroupElement.EnableSelectElement(false);
                }
            }

            _currentSynergyEffectData = synergyEffectData;

            if (_currentSynergyEffectData != null)
            {
                if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(_currentSynergyEffectData.Index) == true)
                {
                    UIRelicElement uILobbySynergyTierGroupElement = _uIGambleSynergyTierGroupElement_dic[_currentSynergyEffectData.Index];
                    uILobbySynergyTierGroupElement.EnableSelectElement(true);
                }
            }


            SkillInfo skillInfo = Managers.RelicManager.Instance.GetSynergyEffectSkillInfo(synergyEffectData);

            if (skillInfo == null)
            {
                if (uIARRRSkillDescGroup != null)
                    uIARRRSkillDescGroup.SetSkillData(synergyEffectData);

                if (_synergyEffectLevel != null)
                    _synergyEffectLevel.gameObject.SetActive(false);

                //if (_synergyEffect_Equip != null)
                //    _synergyEffect_Equip.interactable = false;

                if (_synergyEffect_Get != null)
                    _synergyEffect_Get.gameObject.SetActive(true);


                if (_synergyEffect_Enhance != null)
                    _synergyEffect_Enhance.gameObject.SetActive(false);
            }
            else
            {
                if (_synergyEffect_Get != null)
                    _synergyEffect_Get.gameObject.SetActive(false);

                if (uIARRRSkillDescGroup != null)
                    uIARRRSkillDescGroup.SetSkillData(synergyEffectData, skillInfo.Level);

                if (_synergyEffectLevel != null)
                {
                    _synergyEffectLevel.gameObject.SetActive(skillInfo.Level > 0);
                    _synergyEffectLevel.SetText("+{0}", skillInfo.Level);
                }

                bool isMax = Managers.RelicManager.Instance.IsMaxLevelSynergy(synergyEffectData);

                if (isMax == true)
                {
                    if (_synergyEffect_Enhance != null)
                    {
                        _synergyEffect_Enhance.gameObject.SetActive(true);
                        _synergyEffect_Enhance.interactable = false;
                    }

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
                    int costIndex = Managers.RelicManager.Instance.GetSynergyEnhanceCostGoodsIndex(synergyEffectData);

                    int currentCount = skillInfo.Count;

                    int needCount = Managers.RelicManager.Instance.GetSynergyEnhance_NeedCount(synergyEffectData);

                    bool readyEnhance = Managers.RelicManager.Instance.ReadySynergyEnhance(synergyEffectData);

                    Sprite pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);


                    if (_synergyEffect_Enhance != null)
                    {
                        _synergyEffect_Enhance.gameObject.SetActive(true);
                        _synergyEffect_Enhance.interactable = readyEnhance;
                    }

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
                        _synergyEffect_EnhanceCountText.SetText("{0}/{1}", currentCount, needCount);
                    }

                    if (_synergyEffect_EnhanceCountIcon != null)
                    {
                        _synergyEffect_EnhanceCountIcon.sprite = pointsprite;
                        _synergyEffect_EnhanceCountIcon.gameObject.SetActive(true);
                    }
                }

            }

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
                LayoutRebuilder.ForceRebuildLayoutImmediate(customRefresh);
            }

            if (_descScrollRect != null)
                _descScrollRect.normalizedPosition = Vector2.one;

        }
        //------------------------------------------------------------------------------------
        private void ShowNewRelicMsg(GameBerry.Event.ShowNewRelicMsg msg)
        {
            RelicData synergyEffectData = null;
            if (msg != null)
                synergyEffectData = msg.NewSynergyEffectData;
        }
        //------------------------------------------------------------------------------------
    }
}