using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    [System.Serializable]
    public class LobbyQuestTabBtn
    {
        public V2Enum_QuestType v2Enum_QuestType = V2Enum_QuestType.Max;

        public Button Btn;
        public TMP_Text Text;
        public Image ExtensionImage;

        System.Action<V2Enum_QuestType> _action;

        public void Init(System.Action<V2Enum_QuestType> action)
        {
            if (Btn != null)
                Btn.onClick.AddListener(OnClick);

            _action = action;
        }

        private void OnClick()
        {
            _action?.Invoke(v2Enum_QuestType);
        }
    }

    public class LobbyQuestContentDialog : IDialog
    {
        [SerializeField]
        private Button m_recvAll;

        [SerializeField]
        private Transform _recvAllReddot;

        [SerializeField]
        private List<Transform> _achievementHideUI = new List<Transform>();

        [SerializeField]
        private TMP_Text _initTime;

        [Header("------------MissionTab------------")]
        [SerializeField]
        private List<LobbyQuestTabBtn> _tabBtn = new List<LobbyQuestTabBtn>();

        [Header("------------QuestGauge------------")]
        [SerializeField]
        private Transform _questGaugeGroup;

        [SerializeField]
        private RectTransform _questGaugeRewardRoot;

        [SerializeField]
        private Image _questGaugeRewardGauge;

        [SerializeField]
        private Button _questGaugeRewardBtn;

        [SerializeField]
        private TMP_Text _questGaugeRewardText;

        [SerializeField]
        private UIQuestGaugeRewardElement _questGaugeRewardElement;

        private List<UIQuestGaugeRewardElement> uIQuestGaugeRewardElementList = new List<UIQuestGaugeRewardElement>();



        [Header("------------ElementGroup------------")]
        [SerializeField]
        private RectTransform _elementScrollGroup;

        [SerializeField]
        private float _elementScrollGroup_origin = 720.0f;

        [SerializeField]
        private float _elementScrollGroup_extension = 880.0f;

        [SerializeField]
        private ScrollRect m_elementScrollRect;

        [SerializeField]
        private UIMissionElement m_uIMissionElement;

        [SerializeField]
        private RectTransform m_scrollRectContent;

        private V2Enum_QuestType m_currentQuestTab = V2Enum_QuestType.Max;

        private List<UIMissionElement> m_dailyElementList = new List<UIMissionElement>();

        [SerializeField]
        private UIGuideInteractor m_missionGuideActiveTabInteractor;

        private UIGuideInteractor m_missionGuideActiveInteractor;

        protected override void OnLoad()
        {
            if (m_recvAll != null)
                m_recvAll.onClick.AddListener(OnClick_RecvAll);

            if (_questGaugeRewardBtn != null)
                _questGaugeRewardBtn.onClick.AddListener(() =>
                {
                    Managers.QuestManager.Instance.GetRecvQuestGauge(m_currentQuestTab);
                });

            for (int i = 0; i < _tabBtn.Count; ++i)
            {
                LobbyQuestTabBtn lobbyQuestTabBtn = _tabBtn[i];
                if (lobbyQuestTabBtn != null)
                {
                    lobbyQuestTabBtn.Init(SetMasteryElement);
                }
            }

                Managers.TimeManager.Instance.RemainInitDailyContent_Text += SetInitInterval_Daily;
                Managers.TimeManager.Instance.RemainInitWeekContent_Text += SetInitInterval_Weekly;
                Managers.TimeManager.Instance.RemainInitMonthContent_Text += SetInitInterval_Monthly;

            SetMasteryElement(V2Enum_QuestType.Daily);

            Message.AddListener<GameBerry.Event.RefreshQuestDataMsg>(RefreshQuestData);
            Message.AddListener<GameBerry.Event.RefreshQuestGaugeDataMsg>(RefreshQuestGaugeData);
            Message.AddListener<GameBerry.Event.SetGuideInteractorDialogMsg>(SetGuideInteractorDialog);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshQuestDataMsg>(RefreshQuestData);
            Message.RemoveListener<GameBerry.Event.RefreshQuestGaugeDataMsg>(RefreshQuestGaugeData);
            Message.RemoveListener<GameBerry.Event.SetGuideInteractorDialogMsg>(SetGuideInteractorDialog);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (m_currentQuestTab == V2Enum_QuestType.Daily)
            {
                if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.DailyMissionRewardGet)
                {
                    Managers.GuideInteractorManager.Instance.SetGuideStep(m_missionGuideActiveInteractor);
                }
            }

            V2Enum_QuestType temptype = m_currentQuestTab;
            m_currentQuestTab = V2Enum_QuestType.Max;
            SetMasteryElement(temptype);

            Managers.RedDotManager.Instance.HideRedDot(Managers.QuestManager.Instance.GetRedDotEnum(m_currentQuestTab));
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.isAlive == false)
                return;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.DailyMissionRewardGet)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(m_missionGuideActiveTabInteractor);
                Managers.GuideInteractorManager.Instance.SetPrevGuideInteractor();
            }

            if (Managers.RedDotManager.isAlive == false)
                return;

            Managers.RedDotManager.Instance.HideRedDot(Managers.QuestManager.Instance.GetRedDotEnum(m_currentQuestTab));
            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.Quest);
        }
        //------------------------------------------------------------------------------------
        private void RefreshQuestData(GameBerry.Event.RefreshQuestDataMsg msg)
        {
            if (isEnter == false)
                return;

            bool needRefreshGauge = false;
            bool needRefreshElement = false;

            for (int i = 0; i < msg.missionDatas.Count; ++i)
            {
                QuestData missionData = Managers.QuestManager.Instance.GetMissionData(msg.missionDatas[i]);
                if (missionData.QuestType != m_currentQuestTab)
                    continue;

                UIMissionElement uIMissionElement = m_dailyElementList.Find(x => x.GetQuestData() == missionData);
                if (uIMissionElement != null)
                    uIMissionElement.SetMissionElement(missionData);

                needRefreshElement = Managers.QuestManager.Instance.IsAlReadyRecvRewardMission(missionData);
                needRefreshGauge = true;
            }

            if (needRefreshGauge == true)
                SetQuestGauge(m_currentQuestTab);

            if (needRefreshElement == true)
                SetQuestElement(m_currentQuestTab);

            int readycount = Managers.QuestManager.Instance.ReadyCount(m_currentQuestTab);

            if (m_recvAll != null)
                m_recvAll.interactable = readycount > 0;

            if (_recvAllReddot != null)
                _recvAllReddot.gameObject.SetActive(readycount > 0);
        }
        //------------------------------------------------------------------------------------
        private void RefreshQuestGaugeData(GameBerry.Event.RefreshQuestGaugeDataMsg msg)
        {
            if (isEnter == false)
                return;

            if (msg.v2Enum_QuestType != m_currentQuestTab)
                return;

            SetQuestGauge(m_currentQuestTab);
        }
        //------------------------------------------------------------------------------------
        private void SetMasteryElement(V2Enum_QuestType v2Enum_QuestType)
        {
            if (m_currentQuestTab == v2Enum_QuestType)
                return;

            for (int i = 0; i < _tabBtn.Count; ++i)
            {
                LobbyQuestTabBtn lobbyQuestTabBtn = _tabBtn[i];
                if (lobbyQuestTabBtn != null)
                {
                    if (lobbyQuestTabBtn.Btn != null)
                        lobbyQuestTabBtn.Btn.interactable = lobbyQuestTabBtn.v2Enum_QuestType != v2Enum_QuestType;

                    if (lobbyQuestTabBtn.ExtensionImage != null)
                        lobbyQuestTabBtn.ExtensionImage.gameObject.SetActive(lobbyQuestTabBtn.v2Enum_QuestType == v2Enum_QuestType);
                }
            }

            m_currentQuestTab = v2Enum_QuestType;

            if (m_currentQuestTab == V2Enum_QuestType.Daily)
                SetInitInterval(Managers.TimeManager.Instance.RemainInitDailyContent_String);
            else if (m_currentQuestTab == V2Enum_QuestType.Weekly)
                SetInitInterval(Managers.TimeManager.Instance.RemainInitWeekContent_String);
            else if (m_currentQuestTab == V2Enum_QuestType.Monthly)
                SetInitInterval(Managers.TimeManager.Instance.RemainInitMonthContent_String);


            for (int i = 0; i < _achievementHideUI.Count; ++i)
            {
                if (_achievementHideUI[i] != null)
                    _achievementHideUI[i].gameObject.SetActive(m_currentQuestTab != V2Enum_QuestType.Achievement);
            }

            if (_elementScrollGroup != null)
            {
                Vector2 size = _elementScrollGroup.sizeDelta;
                size.y = m_currentQuestTab == V2Enum_QuestType.Achievement ? _elementScrollGroup_extension : _elementScrollGroup_origin;
                _elementScrollGroup.sizeDelta = size;
            }

            SetQuestElement(m_currentQuestTab);
            SetQuestGauge(m_currentQuestTab);

            int readycount = Managers.QuestManager.Instance.ReadyCount(v2Enum_QuestType);

            if (m_recvAll != null)
                m_recvAll.interactable = readycount > 0;

            if (_recvAllReddot != null)
                _recvAllReddot.gameObject.SetActive(readycount > 0);

            if (m_elementScrollRect != null)
                m_elementScrollRect.normalizedPosition = Vector2.one;
        }
        //------------------------------------------------------------------------------------
        private void SetQuestGauge(V2Enum_QuestType v2Enum_QuestType)
        {
            List<QuestGaugeData> questGaugeDatas = Managers.QuestManager.Instance.GetQuestGaugeDatas(v2Enum_QuestType);

            if (questGaugeDatas == null)
            {
                if (_questGaugeGroup != null)
                    _questGaugeGroup.gameObject.SetActive(false);
                return;
            }

            if (_questGaugeGroup != null)
                _questGaugeGroup.gameObject.SetActive(true);

            float weight = 0.0f;
            if (_questGaugeRewardRoot != null)
            {
                weight = _questGaugeRewardRoot.sizeDelta.x;
            }

            int maxCheckInCount = Managers.QuestManager.Instance.GetQuestGaugeMaxCount(v2Enum_QuestType);

            for (int i = 0; i < questGaugeDatas.Count; ++i)
            {
                QuestGaugeData guildCheckInRewardData = questGaugeDatas[i];
                if (guildCheckInRewardData == null)
                    continue;

                UIQuestGaugeRewardElement uIQuestGaugeRewardElement = null;

                if (uIQuestGaugeRewardElementList.Count > i)
                {
                    uIQuestGaugeRewardElement = uIQuestGaugeRewardElementList[i];
                }
                else
                {
                    GameObject clone = Instantiate(_questGaugeRewardElement.gameObject, _questGaugeRewardRoot);
                    if (clone != null)
                    {
                        uIQuestGaugeRewardElement = clone.GetComponent<UIQuestGaugeRewardElement>();

                        uIQuestGaugeRewardElementList.Add(uIQuestGaugeRewardElement);

                    }
                }

                uIQuestGaugeRewardElement.gameObject.SetActive(true);
                uIQuestGaugeRewardElement.SetQuestGaugeElement(guildCheckInRewardData);

                float ratio = (float)guildCheckInRewardData.RequiredQuestCount / (float)maxCheckInCount;

                float xpos = weight * ratio;

                Vector2 pos = Vector2.zero;
                pos.x = xpos;
                RectTransform rectTransform = uIQuestGaugeRewardElement.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = pos;

            }

            for (int i = questGaugeDatas.Count; i < uIQuestGaugeRewardElementList.Count; ++i)
            {
                uIQuestGaugeRewardElementList[i].gameObject.SetActive(false);
            }

            RefreshOnceReward(v2Enum_QuestType);
        }
        //------------------------------------------------------------------------------------
        private void RefreshOnceReward(V2Enum_QuestType v2Enum_QuestType)
        {
            List<QuestGaugeData> questGaugeDatas = Managers.QuestManager.Instance.GetQuestGaugeDatas(v2Enum_QuestType);
            
            for (int i = 0; i < questGaugeDatas.Count; ++i)
            {
                QuestGaugeData questGaugeData = questGaugeDatas[i];
                UIQuestGaugeRewardElement uIQuestGaugeRewardElement = uIQuestGaugeRewardElementList.Find(x => x.GetGaugeData() == questGaugeData);
                uIQuestGaugeRewardElement.RefreshQuestGaugeElement();
            }

            if (_questGaugeRewardBtn != null)
            {
                _questGaugeRewardBtn.gameObject.SetActive(Managers.QuestManager.Instance.ReadyRecvDrawReward(v2Enum_QuestType));
            }

            int maxCheckInCount = Managers.QuestManager.Instance.GetQuestGaugeMaxCount(v2Enum_QuestType);

            int accumCheckInCount = Managers.QuestManager.Instance.GetEventRouletteAccumCount(v2Enum_QuestType);

            float ratio = (float)accumCheckInCount / (float)maxCheckInCount;

            if (_questGaugeRewardGauge != null)
                _questGaugeRewardGauge.fillAmount = ratio;

            if (_questGaugeRewardText != null)
                _questGaugeRewardText.SetText("{0}", accumCheckInCount);
        }
        //------------------------------------------------------------------------------------
        private void SetQuestElement(V2Enum_QuestType v2Enum_QuestType)
        {
            List<QuestData> questDatas = Managers.QuestManager.Instance.GetQuestDatas(v2Enum_QuestType);

            questDatas.Sort(Managers.QuestManager.Instance.SortGambleSynergyCombineDatas);

            for (int i = 0; i < questDatas.Count; ++i)
            {
                UIMissionElement uIMissionElement = null;

                if (m_dailyElementList.Count > i)
                {
                    uIMissionElement = m_dailyElementList[i];
                }
                else
                {
                    GameObject clone = Instantiate(m_uIMissionElement.gameObject, m_scrollRectContent);

                    uIMissionElement = clone.GetComponent<UIMissionElement>();
                    m_dailyElementList.Add(uIMissionElement);
                }

                uIMissionElement.gameObject.SetActive(true);
                uIMissionElement.SetMissionElement(questDatas[i]);
            }

            for (int i = questDatas.Count; i < m_dailyElementList.Count; ++i)
            {
                UIMissionElement uIMissionElement = m_dailyElementList[i];
                uIMissionElement.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RecvAll()
        {
            if (Managers.QuestManager.Instance.DoRecvAllMission(m_currentQuestTab))
            {
                SetQuestElement(m_currentQuestTab);
                SetQuestGauge(m_currentQuestTab);

                int readycount = Managers.QuestManager.Instance.ReadyCount(m_currentQuestTab);

                if (m_recvAll != null)
                    m_recvAll.interactable = readycount > 0;

                if (_recvAllReddot != null)
                    _recvAllReddot.gameObject.SetActive(readycount > 0);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetGuideInteractorDialog(GameBerry.Event.SetGuideInteractorDialogMsg msg)
        {
            if (msg.guideQuestData == null)
                return;

            if (msg.guideQuestData.QuestType == V2Enum_EventType.DailyMissionRewardGet)
            {
                List<QuestData> missionDatas = Managers.QuestManager.Instance.GetQuestDatas(V2Enum_QuestType.Daily);
                for (int i = 0; i < missionDatas.Count; ++i)
                {
                    QuestData missionData = missionDatas[i];
                    bool IsReady = Managers.QuestManager.Instance.IsReadyRecvRewardMission(missionData);
                    if (IsReady == true)
                    {
                        UIMissionElement uIMissionElement = m_dailyElementList.Find(x => x.GetQuestData() == missionData);
                        if (uIMissionElement != null)
                            uIMissionElement.SetDailyMissionGuideBtn();
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void SetInitInterval_Daily(string remaintime)
        {
            if (m_currentQuestTab != V2Enum_QuestType.Daily)
                return;

            SetInitInterval(remaintime);
        }
        //------------------------------------------------------------------------------------
        private void SetInitInterval_Weekly(string remaintime)
        {
            if (m_currentQuestTab != V2Enum_QuestType.Weekly)
                return;

            SetInitInterval(remaintime);
        }
        //------------------------------------------------------------------------------------
        private void SetInitInterval_Monthly(string remaintime)
        {
            if (m_currentQuestTab != V2Enum_QuestType.Monthly)
                return;

            SetInitInterval(remaintime);
        }
        //------------------------------------------------------------------------------------
        private void SetInitInterval(string remaintime)
        {
            if (_initTime != null)
                _initTime.SetText(remaintime);
        }
        //------------------------------------------------------------------------------------
    }
}