using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
    public class LobbyResearchDialog : IDialog
    {
        [Header("------------Guide------------")]
        [SerializeField]
        private Transform _guideBG2;

        [SerializeField]
        private Button _guide2CompleteBtn;

        [SerializeField]
        private Transform _guideBG3;

        [SerializeField]
        private Transform _guideBG4;

        [SerializeField]
        private Button _guide4CompleteBtn;

        [SerializeField]
        private UIGuideInteractor _researchGuideInteractor4 = null;

        private UIGuideInteractor _researchGuideInteractor5 = null;

        [SerializeField]
        private List<Button> _guideIgnoreBtn = new List<Button>();

        [Header("---------ResearchAccumContent---------")]
        [SerializeField]
        private Button _showResearchAccumGoods;

        [SerializeField]
        private TMP_Text _researchAmount;

        [SerializeField]
        private TMP_Text _researchTimer;

        [SerializeField]
        private LobbyResearchChargeDialog _lobbyResearchChargeDialog;

        [Header("---------ResearchTicket---------")]
        [SerializeField]
        private Button _showResearchTicketShop;

        [SerializeField]
        private LobbyResearchTicketShopDialog _lobbyResearchTicketShopDialog;

        [Header("---------AccelTicket---------")]

        [SerializeField]
        private LobbyResearchUseAccelDialog _lobbyResearchUseAccelDialog;


        [Header("------------ElementGroup------------")]
        [SerializeField]
        private float m_topPadding = 85.0f;

        [SerializeField]
        private float m_bottomPadding = 85.0f;

        [SerializeField]
        private Vector2 m_spacing = 70.0f.ToVector2();

        [SerializeField]
        private Vector2 m_cellSize = 150.0f.ToVector2();

        [SerializeField]
        private ScrollRect m_elementScrollRect;

        [SerializeField]
        private UIResearchElement _researchElement;

        [SerializeField]
        private RectTransform m_scrollRectContent;

        private List<UIResearchElement> m_uIResearchElements = new List<UIResearchElement>();
        private Dictionary<int, UIResearchElement> m_uIResearchElements_Dic = new Dictionary<int, UIResearchElement>();


        [SerializeField]
        private UIResearchLineElement m_uIresearchLineElement;
        private List<UIResearchLineElement> m_uIResearchLineElements = new List<UIResearchLineElement>();
        private Dictionary<int, List<UIResearchLineElement>> m_uIResearchLineElements_Dic = new Dictionary<int, List<UIResearchLineElement>>();

        [Header("------------Detail------------")]
        [SerializeField]
        private UIResearchElement m_detailResearchElement;

        [SerializeField]
        private TMP_Text m_researchName;

        [SerializeField]
        private TMP_Text m_researchTime;

        [Header("------------EffectValue------------")]
        [SerializeField]
        private Transform m_notMaxDescGroup;

        [SerializeField]
        private TMP_Text m_statName;

        [SerializeField]
        private TMP_Text m_notMax_CurrentValue;

        [SerializeField]
        private TMP_Text m_notMax_NextValue;

        [SerializeField]
        private UIResearchSynergyDescViewElement m_notMax_CurrentSynergyValue;

        [SerializeField]
        private UIResearchSynergyDescViewElement m_notMax_NextSynergyValue;

        [SerializeField]
        private Transform m_maxDescGroup;

        [SerializeField]
        private TMP_Text m_max_CurrentValue;

        [SerializeField]
        private UIResearchSynergyDescViewElement m_max_CurrentSynergyValue;

        [Header("------------Button------------")]
        [SerializeField]
        private Transform _maxBtn;

        [SerializeField]
        private Transform _researchingBtn;

        [SerializeField]
        private Button m_enhance;

        [SerializeField]
        private Color m_detailEnhanceBtn_Disable;

        [SerializeField]
        private Color m_detailEnhanceBtn_Enable;


        [SerializeField]
        private TMP_Text m_detailEnhanceTitleText;

        [SerializeField]
        private TMP_Text m_detailEnhancePriceTitleText;

        [SerializeField]
        private Image m_detailEnhanceIconTitleText;

        [SerializeField]
        private Color m_detailEnhanceTitleText_Disable;

        [SerializeField]
        private Color m_detailEnhanceTitleText_Enable;


        [Header("------------Slot------------")]
        [SerializeField]
        private Transform _slotRoot;

        [Header("------------CompletePopup------------")]
        [SerializeField]
        private IDialog _completePopup;

        [SerializeField]
        private UIResearchElement _uIResearchElement;

        [SerializeField]
        private List<UIResearchSlotElement> _uIResearchSlotElement = new List<UIResearchSlotElement>();

        private Dictionary<ObscuredInt, UIResearchSlotElement> _uIResearchSlotDic = new Dictionary<ObscuredInt, UIResearchSlotElement>();

        private ResearchData _currentResearchData = null;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_guide2CompleteBtn != null)
                _guide2CompleteBtn.onClick.AddListener(() =>
                {
                    _guide2CompleteBtn.enabled = false;

                    if (Managers.GuideInteractorManager.Instance.PlayResearchTutorial == true)
                    {
                        if (_guideBG2 != null)
                            _guideBG2.gameObject.SetActive(false);

                        if (_guideBG3 != null)
                            _guideBG3.gameObject.SetActive(true);
                    }
                });

            if (_guide4CompleteBtn != null)
                _guide4CompleteBtn.onClick.AddListener(() =>
                {
                    _guide4CompleteBtn.enabled = false;

                    if (_guideBG4 != null)
                        _guideBG4.gameObject.SetActive(false);
                });

            if (_lobbyResearchChargeDialog != null)
                _lobbyResearchChargeDialog.Load_Element();

            if(_showResearchAccumGoods != null)
                _showResearchAccumGoods.onClick.AddListener(() =>
                {
                    bool ismax = Managers.ResearchManager.Instance.IsMaxResearchCharge();
                    if (ismax == true)
                        Managers.ResearchManager.Instance.GetResearchChargeAmount();
                    else
                    {
                        if (_lobbyResearchChargeDialog != null)
                            _lobbyResearchChargeDialog.ElementEnter();
                    }
                });

            if (_lobbyResearchTicketShopDialog != null)
                _lobbyResearchTicketShopDialog.Load_Element();

            if (_lobbyResearchUseAccelDialog != null)
                _lobbyResearchUseAccelDialog.Load_Element();

            if (_showResearchTicketShop != null)
                _showResearchTicketShop.onClick.AddListener(() =>
                {
                    if (_lobbyResearchTicketShopDialog != null)
                        _lobbyResearchTicketShopDialog.ElementEnter();
                });

            if (m_enhance != null)
                m_enhance.onClick.AddListener(OnClick_EnhanceBtn);

            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.ResearchTicket.Enum32ToInt(), RefreshSP_NormalPoint);

            foreach (var pair in ResearchContainer.ResearchSlot)
            {
                UIResearchSlotElement uIResearchLineElement = null;

                int idx = pair.Key - 1;

                if (idx < _uIResearchSlotElement.Count)
                    uIResearchLineElement = _uIResearchSlotElement[idx];

                uIResearchLineElement.SetSlotIdx(pair.Key, OnClick_ResearchSlotAccel);
                ResearchData researchData = Managers.ResearchManager.Instance.GetSlotResearchData(pair.Key);
                uIResearchLineElement.SetResearchData(researchData);

                _uIResearchSlotDic.Add(pair.Key, uIResearchLineElement);
            }

            if (_completePopup != null)
            { 
                _completePopup.Load_Element();
                _completePopup.exitCallBack += HideCompletePopup;
            }

            Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_HalfSec += CheckResearchTimer;

            Managers.ResearchManager.Instance.RechargeTime += SetResearchChargeRemainTime;

            SetResearchElement();

            Message.AddListener<GameBerry.Event.RefreshResearchInfoListMsg>(RefreshResearchInfoList);
            Message.AddListener<GameBerry.Event.DrawNextResearchLineMsg>(DrawNextResearchLine);
            Message.AddListener<GameBerry.Event.RefreshResearchSlotMsg>(RefreshResearchSlot);
            Message.AddListener<GameBerry.Event.NoticeResearchCompleteMsg>(NoticeResearchComplete);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            if (Managers.GoodsManager.isAlive == true)
            {
                Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.ResearchTicket.Enum32ToInt(), RefreshSP_NormalPoint);
            }

            Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_HalfSec -= CheckResearchTimer;

            Message.RemoveListener<GameBerry.Event.RefreshResearchInfoListMsg>(RefreshResearchInfoList);
            Message.RemoveListener<GameBerry.Event.DrawNextResearchLineMsg>(DrawNextResearchLine);
            Message.RemoveListener<GameBerry.Event.RefreshResearchSlotMsg>(RefreshResearchSlot);
            Message.RemoveListener<GameBerry.Event.NoticeResearchCompleteMsg>(NoticeResearchComplete);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.ResearchTutorial)
            {
                ignoreExit = true;

                if (_researchGuideInteractor5 == null)
                {
                    int targetidx = 119010002;

                    if (m_uIResearchElements_Dic.ContainsKey(targetidx) == true)
                    {
                        UIResearchElement uIResearchElement = m_uIResearchElements_Dic[targetidx];
                        if (uIResearchElement != null)
                        {
                            _researchGuideInteractor5 = uIResearchElement.SetGuideInteractor();
                        }
                    }
                }

                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() < 3)
                {
                    SetResearchDetail(Managers.ResearchManager.Instance.GetResearchData(119010001));
                    PlayNextWaveDelay().Forget();
                }

                for (int i = 0; i < _guideIgnoreBtn.Count; ++i)
                {
                    _guideIgnoreBtn[i].enabled = false;
                }

            }
            else
                ignoreExit = false;

            SetResearchDetail(_currentResearchData);

            ScrollViewSnapToItem();

            RefreshAllSlot();

            ShowCompletePopup();
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.isAlive == false)
                return;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.ResearchTutorial)
            {
                if (_guideBG2 != null)
                    _guideBG2.gameObject.SetActive(false);

                if (_guideBG3 != null)
                    _guideBG3.gameObject.SetActive(false);

                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(false);

                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 4)
                {
                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                }
                else
                    Managers.GuideInteractorManager.Instance.SetGuideStep(1);
            }

            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyResearch);
        }
        //------------------------------------------------------------------------------------
        public void EndTutorial()
        {
            for (int i = 0; i < _guideIgnoreBtn.Count; ++i)
            {
                _guideIgnoreBtn[i].enabled = true;
            }

            ignoreExit = false;

            if (m_elementScrollRect != null)
                m_elementScrollRect.enabled = true;
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayNextWaveDelay()
        {
            await UniTask.NextFrame();


            if (_guide2CompleteBtn != null)
                _guide2CompleteBtn.enabled = true;

            if (_guide4CompleteBtn != null)
                _guide4CompleteBtn.enabled = false;

            if (_guideBG2 != null)
                _guideBG2.gameObject.SetActive(true);


            if (_guideBG3 != null)
                _guideBG3.gameObject.SetActive(false);


            if (_guideBG4 != null)
                _guideBG4.gameObject.SetActive(false);

            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/research2"));
            Managers.GuideInteractorManager.Instance.SetGuideStep(2);

            await UniTask.Delay(500);

            if (m_elementScrollRect != null)
                m_elementScrollRect.enabled = false;

        }
        //------------------------------------------------------------------------------------
        private void SetResearchChargeRemainTime(string timestamp)
        {
            if (_researchAmount != null)
                _researchAmount.SetText(string.Format("{0:0}", ResearchContainer.ChargeResearchCount));

            if (_researchTimer != null)
                _researchTimer.SetText(timestamp);
        }
        //------------------------------------------------------------------------------------
        private void CheckResearchTimer()
        {
            if (Managers.GuideInteractorManager.Instance.PlayResearchTutorial == true)
                return;

            foreach (var pair in _uIResearchSlotDic)
            {
                pair.Value.RefreshResearchData();
            }
        }
        //------------------------------------------------------------------------------------
        private void NoticeResearchComplete(GameBerry.Event.NoticeResearchCompleteMsg msg)
        {
            if (_isEnter == true)
                ShowCompletePopup();
        }
        //------------------------------------------------------------------------------------
        private void HideCompletePopup()
        {
            ShowCompletePopup();
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.ResearchTutorial)
            {
                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 4)
                {
                    Managers.GuideInteractorManager.Instance.SetGuideStep(5);
                    //Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/research3"));
                }

            }
        }
        //------------------------------------------------------------------------------------
        private void ShowCompletePopup()
        {
            int index = 0;

            if (ResearchContainer.ResearchEndViewQueue.Count > 0)
            {
                index = ResearchContainer.ResearchEndViewQueue[0];
                ResearchContainer.ResearchEndViewQueue.RemoveAt(0);
            }
            else
                return;

            ResearchData researchData = Managers.ResearchManager.Instance.GetResearchData(index);
            if (researchData == null)
                return;

            if (_uIResearchElement != null)
            { 
                _uIResearchElement.SetResearchElement(researchData);
                _uIResearchElement.SetSelectState(false);
            }

            if (_completePopup != null)
                _completePopup.ElementEnter();
        }
        //------------------------------------------------------------------------------------
        private void SetResearchElement()
        {
            SetResearchDetail(null);

            ResearchData focusResearchData = null;

            m_uIResearchElements_Dic.Clear();

            m_uIResearchLineElements_Dic.Clear();

            List<List<ResearchData>> researchDatas = Managers.ResearchManager.Instance.GetResearchDatas();

            Vector2 contentSize = m_scrollRectContent.sizeDelta;
            contentSize.y = m_topPadding + m_bottomPadding + ((m_cellSize.y + m_spacing.y) * (researchDatas.Count)) - m_cellSize.y + m_spacing.y;
            m_scrollRectContent.sizeDelta = contentSize;

            float ypos = 0;
            ypos -= m_topPadding + m_spacing.y;


            int researchIndex = 0;
            int lineIndex = 0;

            int centerxlocate = 4;

            for (int i = 0; i < researchDatas.Count; ++i)
            {
                List<ResearchData> researchxdata = researchDatas[i];

                Vector3 pos = Vector3.zero;
                pos.y = ypos;

                bool isaddnum = researchxdata.Count % 2 == 0 ? false : true;

                int halfnum = researchxdata.Count / 2;

                float halfspacing = (m_cellSize.x + m_spacing.x) * 0.5f;

                for (int j = 0; j < researchxdata.Count; ++j)
                {
                    //if (isaddnum == true)
                    //{
                    //    pos.x = (m_cellSize.x + m_spacing.x) * (j - halfnum);
                    //}
                    //else
                    //{
                    //    float tempxpos = (m_cellSize.x + m_spacing.x) * (j - halfnum);
                    //    pos.x = tempxpos + halfspacing;
                    //}

                    ResearchData currresearchxdata = researchxdata[j];

                    pos.x = m_spacing.x * (currresearchxdata.LocationX - centerxlocate);

                    UIResearchElement uIResearchElement;

                    if (researchIndex < m_uIResearchElements.Count)
                    {
                        uIResearchElement = m_uIResearchElements[researchIndex];
                        uIResearchElement.gameObject.SetActive(true);
                    }
                    else
                    {
                        GameObject clone = Instantiate(_researchElement.gameObject, m_scrollRectContent);

                        uIResearchElement = clone.GetComponent<UIResearchElement>();
                        uIResearchElement.Init(OnClick_ResearchElement);
                        m_uIResearchElements.Add(uIResearchElement);

                    }

                    uIResearchElement.SetResearchElement(researchxdata[j]);

                    researchIndex++;

                    uIResearchElement.transform.localPosition = pos;

                    RectTransform rectTransform = uIResearchElement.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = m_cellSize;

                    m_uIResearchElements_Dic.Add(researchxdata[j].Index, uIResearchElement);

                    for (int rootidx = 0; rootidx < researchxdata[j].MyRootData.Count; ++rootidx)
                    {
                        if (m_uIResearchElements_Dic.ContainsKey(researchxdata[j].MyRootData[rootidx].Index) == false)
                            continue;

                        ResearchData rootData = Managers.ResearchManager.Instance.GetResearchData(researchxdata[j].MyRootData[rootidx].Index);
                        if (rootData == null)
                            continue;

                        if (focusResearchData == null)
                        {
                            if (Managers.ResearchManager.Instance.IsMaxLevel(rootData) == false
                            && Managers.ResearchManager.Instance.IsOpenCondition(rootData) == true)
                                focusResearchData = rootData;
                        }

                        UIResearchLineElement uIResearchLineElement;

                        if (lineIndex < m_uIResearchLineElements.Count)
                        {
                            uIResearchLineElement = m_uIResearchLineElements[lineIndex];
                            uIResearchLineElement.gameObject.SetActive(true);
                        }
                        else
                        {
                            GameObject clone = Instantiate(m_uIresearchLineElement.gameObject, m_scrollRectContent);

                            uIResearchLineElement = clone.GetComponent<UIResearchLineElement>();
                            m_uIResearchLineElements.Add(uIResearchLineElement);
                        }

                        uIResearchLineElement.SetLineTrans(m_uIResearchElements_Dic[rootData.Index], uIResearchElement);
                        uIResearchLineElement.SetStartEndData(rootData, researchxdata[j]);
                        uIResearchLineElement.RefreshEnableLine();
                        uIResearchLineElement.transform.SetAsFirstSibling();

                        lineIndex++;

                        if (m_uIResearchLineElements_Dic.ContainsKey(rootData.Index) == false)
                            m_uIResearchLineElements_Dic.Add(rootData.Index, new List<UIResearchLineElement>());

                        m_uIResearchLineElements_Dic[rootData.Index].Add(uIResearchLineElement);
                    }
                }

                ypos -= m_cellSize.y + m_spacing.y;
            }

            for (int i = researchIndex; i < m_uIResearchElements.Count; ++i)
            {
                m_uIResearchElements[i].gameObject.SetActive(false);
            }

            for (int i = lineIndex; i < m_uIResearchLineElements.Count; ++i)
            {
                m_uIResearchLineElements[i].gameObject.SetActive(false);
            }

            if (focusResearchData == null)
            {
                if (researchDatas != null && researchDatas.Count > 0)
                {
                    List<ResearchData> endresearchList = researchDatas[researchDatas.Count - 1];

                    if (endresearchList != null && endresearchList.Count > 0)
                        focusResearchData = endresearchList[endresearchList.Count - 1];
                }
            }

            SetResearchDetail(focusResearchData);

            ScrollViewSnapToItem();
        }
        //------------------------------------------------------------------------------------
        private void ScrollViewSnapToItem()
        {
            UIResearchElement uIResearchElement = null;

            if (_currentResearchData == null)
            {
                if (m_uIResearchElements.Count > 0)
                {
                    uIResearchElement = m_uIResearchElements[0];
                }
            }
            else
            {
                uIResearchElement = m_uIResearchElements_Dic[_currentResearchData.Index];
            }

            if (uIResearchElement != null)
            {
                //m_elementScrollRect.SetNormalizedPositionToCenter(uIResearchElement.transform);

                RectTransform rectTransform = null;
                if (uIResearchElement.TryGetComponent(out rectTransform))
                {
                    Vector2 offset = Vector2.zero;
                    offset.y = m_spacing.y + m_cellSize.y;

                    Util.ScrollViewSnapToItem(m_elementScrollRect, m_scrollRectContent, rectTransform, offset);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void SetResearchDetail(ResearchData researchData)
        {
            if (_currentResearchData != null)
            {
                if (m_uIResearchElements_Dic.ContainsKey(_currentResearchData.Index) == true)
                {
                    UIResearchElement uIResearchElement = m_uIResearchElements_Dic[_currentResearchData.Index];
                    uIResearchElement.SetSelectState(false);
                }
            }

            _currentResearchData = researchData;

            if (researchData == null)
                return;


            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.ResearchTutorial)
            {
                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() < 3)
                {
                    if (researchData.Index != 119010001)
                        return;
                }
                else if (Managers.GuideInteractorManager.Instance.GetCurrentStep() >= 3)
                {
                    if (researchData.Index != 119010002)
                        return;
                }

                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() <= 5)
                {
                    if (researchData.Index == 119010002)
                    {
                        if (_guideBG3 != null)
                            _guideBG3.gameObject.SetActive(true);
                    }
                }
            }

            if (m_detailResearchElement != null)
                m_detailResearchElement.SetResearchElement(researchData);

            bool isMax = Managers.ResearchManager.Instance.IsMaxLevel(researchData);

            if (m_notMaxDescGroup != null)
                m_notMaxDescGroup.gameObject.SetActive(isMax == false);

            if (m_maxDescGroup != null)
                m_maxDescGroup.gameObject.SetActive(isMax == true);

            if (m_enhance != null)
                m_enhance.gameObject.SetActive(isMax == false);

            if (_maxBtn != null)
                _maxBtn.gameObject.SetActive(isMax == true);
            

            if (m_researchName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_researchName, Managers.ResearchManager.Instance.GetResearchName(_currentResearchData));

            if (m_researchTime != null)
            {
                //if (Managers.ResearchManager.Instance.IsDoingResearch(researchData) == true)
                //    m_researchTime.SetText("In Research");
                //else
                if (isMax == true)
                    m_researchTime.SetText("-");
                else
                    m_researchTime.SetText(Managers.TimeManager.Instance.GetSecendToDayString_HMS(Managers.ResearchManager.Instance.GetResearchTime(researchData).ToInt()));
            }

            if (m_statName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_statName, Managers.ResearchManager.Instance.GetEffectTitle(_currentResearchData.ResearchEffectType));

            bool issynergyresearch = Managers.ResearchManager.Instance.IsSynergyResearchData(_currentResearchData);


            if (isMax == true)
            {
                if (issynergyresearch == true)
                {
                    if (m_max_CurrentValue != null)
                    {
                        m_max_CurrentValue.gameObject.SetActive(false);
                    }

                    if (m_max_CurrentSynergyValue != null)
                    {
                        m_max_CurrentSynergyValue.gameObject.SetActive(true);
                        m_max_CurrentSynergyValue.SetResearchData(_currentResearchData, _currentResearchData.ResearchEffectTypeLevelUpValue.GetDecrypted().ToInt());
                    }
                }
                else
                {
                    if (m_max_CurrentValue != null)
                    {
                        m_max_CurrentValue.gameObject.SetActive(true);
                        m_max_CurrentValue.SetText(Managers.ResearchManager.Instance.GetDisplayCurrentValue(_currentResearchData));
                    }

                    if (m_max_CurrentSynergyValue != null)
                    {
                        m_max_CurrentSynergyValue.gameObject.SetActive(false);
                    }
                }

                if (_researchingBtn != null)
                    _researchingBtn.gameObject.SetActive(false);

            }
            else
            {
                

                if (issynergyresearch == true)
                {
                    if (m_notMax_CurrentValue != null)
                    {
                        m_notMax_CurrentValue.gameObject.SetActive(false);
                    }

                    if (m_notMax_NextValue != null)
                    {
                        m_notMax_NextValue.gameObject.SetActive(false);
                    }

                    if (m_notMax_CurrentSynergyValue != null)
                    {
                        m_notMax_CurrentSynergyValue.gameObject.SetActive(true);
                        int desclevel = Managers.ResearchManager.Instance.GetSynergyGambleLevel(
                            Managers.ResearchManager.Instance.ConvertResearchTypeToSynergyType(researchData.ResearchEffectType));
                        m_notMax_CurrentSynergyValue.SetResearchData(_currentResearchData, desclevel);
                    }

                    if (m_notMax_NextSynergyValue != null)
                    {
                        m_notMax_NextSynergyValue.gameObject.SetActive(true);
                        m_notMax_NextSynergyValue.SetResearchData(_currentResearchData, _currentResearchData.ResearchEffectTypeLevelUpValue.GetDecrypted().ToInt());
                    }

                    
                }
                else
                {
                    if (m_notMax_CurrentValue != null)
                    {
                        m_notMax_CurrentValue.gameObject.SetActive(true);
                        m_notMax_CurrentValue.SetText(Managers.ResearchManager.Instance.GetDisplayCurrentValue(_currentResearchData));
                    }

                    if (m_notMax_NextValue != null)
                    {
                        m_notMax_NextValue.gameObject.SetActive(true);
                        m_notMax_NextValue.SetText(Managers.ResearchManager.Instance.GetDisplayNextValue(_currentResearchData));
                    }

                    if (m_notMax_CurrentSynergyValue != null)
                    {
                        m_notMax_CurrentSynergyValue.gameObject.SetActive(false);
                    }

                    if (m_notMax_NextSynergyValue != null)
                    {
                        m_notMax_NextSynergyValue.gameObject.SetActive(false);
                    }
                }
            }

            SetEnhanceBtn();

            if (_currentResearchData != null)
            {
                if (m_uIResearchElements_Dic.ContainsKey(_currentResearchData.Index) == true)
                {
                    UIResearchElement uIResearchElement = m_uIResearchElements_Dic[_currentResearchData.Index];
                    uIResearchElement.SetSelectState(true);
                }
            }


            foreach (var pair in _uIResearchSlotDic)
            {
                ResearchData slotdata = Managers.ResearchManager.Instance.GetSlotResearchData(pair.Key);
                _uIResearchSlotDic[pair.Key].EnableSelectEffect(slotdata == _currentResearchData);
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshSP_NormalPoint(double amount)
        {
            SetEnhanceBtn();

            //if (isEnter == false)
            //{
            //    if (amount >= 1.0)
            //        Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyResearch);
            //}
        }
        //------------------------------------------------------------------------------------
        private void SetEnhanceBtn()
        {
            if (_currentResearchData == null)
                return;

            bool isMax = Managers.ResearchManager.Instance.IsMaxLevel(_currentResearchData);

            if (isMax == false)
            {
                if (Managers.ResearchManager.Instance.IsDoingResearch(_currentResearchData) == true)
                {
                    if (_researchingBtn != null)
                        _researchingBtn.gameObject.SetActive(true);

                    if (m_enhance != null)
                        m_enhance.gameObject.SetActive(false);
                }
                else
                {
                    if (_researchingBtn != null)
                        _researchingBtn.gameObject.SetActive(false);

                    if (m_enhance != null)
                        m_enhance.gameObject.SetActive(true);
                }

                bool isLevelUp = Managers.ResearchManager.Instance.CheckReadyLevelUp(_currentResearchData);

                if (m_enhance != null)
                {
                    m_enhance.image.color = isLevelUp == true ? m_detailEnhanceBtn_Enable : m_detailEnhanceBtn_Disable;
                    //m_enhance.interactable = isLevelUp;
                }

                if (m_detailEnhanceTitleText != null)
                    m_detailEnhanceTitleText.color = isLevelUp == true ? m_detailEnhanceTitleText_Enable : m_detailEnhanceTitleText_Disable;

                if (m_detailEnhancePriceTitleText != null)
                {
                    m_detailEnhancePriceTitleText.text = Util.GetAlphabetNumber(Managers.ResearchManager.Instance.GetNeedLevelUPSpPoint(_currentResearchData));
                    m_detailEnhancePriceTitleText.color = isLevelUp == true ? m_detailEnhanceTitleText_Enable : m_detailEnhanceTitleText_Disable;
                }

                if (m_detailEnhanceIconTitleText != null)
                {
                    m_detailEnhanceIconTitleText.gameObject.SetActive(true);
                    m_detailEnhanceIconTitleText.sprite = Managers.ResearchManager.Instance.GetMastaryLevelUPGoodsSprite(_currentResearchData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshResearchInfoList(GameBerry.Event.RefreshResearchInfoListMsg msg)
        {
            for (int i = 0; i < msg.datas.Count; ++i)
            {
                if (m_uIResearchElements_Dic.ContainsKey(msg.datas[i].Index) == true)
                {
                    UIResearchElement uIResearchElement = m_uIResearchElements_Dic[msg.datas[i].Index];
                    uIResearchElement.SetResearchElement(msg.datas[i]);

                    if (msg.datas[i] == _currentResearchData)
                    {
                        SetResearchDetail(_currentResearchData);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void DrawNextResearchLine(GameBerry.Event.DrawNextResearchLineMsg msg)
        {
            if (m_uIResearchLineElements_Dic.ContainsKey(msg.researchData.Index) == true)
            {
                List<UIResearchLineElement> uIResearchLineElements = m_uIResearchLineElements_Dic[msg.researchData.Index];

                for (int i = 0; i < uIResearchLineElements.Count; ++i)
                {
                    uIResearchLineElements[i].RefreshEnableLine();
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshAllSlot()
        {
            foreach (var pair in _uIResearchSlotDic)
            {
                _uIResearchSlotDic[pair.Key].RefreshSlotData();
                ResearchData researchData = Managers.ResearchManager.Instance.GetSlotResearchData(pair.Key);
                _uIResearchSlotDic[pair.Key].SetResearchData(researchData);
                _uIResearchSlotDic[pair.Key].EnableSelectEffect(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshResearchSlot(GameBerry.Event.RefreshResearchSlotMsg msg)
        {
            if (_uIResearchSlotDic.ContainsKey(msg.SlotIdx) == true)
            { 
                _uIResearchSlotDic[msg.SlotIdx].RefreshSlotData();
                ResearchData researchData = Managers.ResearchManager.Instance.GetSlotResearchData(msg.SlotIdx);
                _uIResearchSlotDic[msg.SlotIdx].SetResearchData(researchData);

                if (_lobbyResearchUseAccelDialog != null)
                {
                    if (_lobbyResearchUseAccelDialog.isEnter == true)
                        _lobbyResearchUseAccelDialog.RefreshRemainTime();
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ResearchSlotAccel(int slot)
        {
            if (Managers.GuideInteractorManager.Instance.PlayResearchTutorial == true)
            {
                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(false);


                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() < 5)
                {
                    return;
                }
            }

            if (_lobbyResearchUseAccelDialog != null)
            {
                _lobbyResearchUseAccelDialog.SetSlotIdx(slot);
                _lobbyResearchUseAccelDialog.ElementEnter();
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ResearchElement(ResearchData characterGearData)
        {
            SetResearchDetail(characterGearData);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_EnhanceBtn()
        {
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.ResearchTutorial)
            {
                if (_currentResearchData.Index != 119010001 && _currentResearchData.Index != 119010002)
                    return;
            }

            if (Managers.ResearchManager.Instance.SetPlayerResearchInfo_LevelUp(_currentResearchData) == true)
            { 
                SetResearchDetail(_currentResearchData);

                if (Managers.GuideInteractorManager.Instance.PlayResearchTutorial == true)
                {
                    if (_guideBG3 != null)
                        _guideBG3.gameObject.SetActive(false);

                    if (_currentResearchData.Index == 119010001)
                    {
                        if (_researchGuideInteractor4 != null)
                        {
                            _researchGuideInteractor4.ConnectInteractor();
                            _researchGuideInteractor4 = null;
                        }
                        Managers.GuideInteractorManager.Instance.SetGuideStep(4);
                    }
                    else if (_currentResearchData.Index == 119010002)
                    {
                        if (_guideBG4 != null)
                            _guideBG4.gameObject.SetActive(true);
                        Managers.GuideInteractorManager.Instance.SetGuideStep(7);
                        Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/research4"), 4.0f, 0);
                    }

                    //if (Managers.GuideInteractorManager.Instance.GetCurrentStep() > 5)
                    //{
                    //    if (_guideBG4 != null)
                    //        _guideBG4.gameObject.SetActive(true);

                    //    Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/research4"));
                    //}


                    //if (_guide4CompleteBtn != null)
                    //    _guide4CompleteBtn.enabled = false;
                }

            }
        }
        //------------------------------------------------------------------------------------
    }
}