using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    [System.Serializable]
    public class CheckInKindData
    {
        public V2Enum_CheckInType V2Enum_CheckInType;
        public string TitleLocalKey = string.Empty;
        public float BGHeight = 620.0f;
    }

    public class InGameCheckInDialog : IDialog
    {
        public static bool IsEnterState = false;

        [SerializeField]
        private List<Button> m_exitBtn;

        [SerializeField]
        private List<CheckInKindData> m_checkInKindDatas = new List<CheckInKindData>();

        [SerializeField]
        private TMP_Text m_title;

        [SerializeField]
        private RectTransform m_bgTrans;

        [Header("------------MasteryTab------------")]
        [SerializeField]
        private Button m_onceBtn;

        [SerializeField]
        private TMP_Text m_once_Text;


        [SerializeField]
        private Button m_repeatBtn;

        [SerializeField]
        private TMP_Text m_repeat_Text;


        [SerializeField]
        private Sprite m_selectTabBG;

        [SerializeField]
        private Sprite m_noneTabBG;


        [SerializeField]
        private Color m_selectTab_Text;

        [SerializeField]
        private Color m_noneTab_Text;

        [Header("------------Element------------")]

        [SerializeField]
        private ScrollRect m_elementScrollRect;

        [SerializeField]
        private RectTransform m_scrollRectContent;

        [SerializeField]
        private RectTransform m_uICheckInElementRoot;

        [SerializeField]
        private UICheckInElement m_uICheckInElement;

        [SerializeField]
        private Button m_checkInButton;

        [SerializeField]
        private Image m_checkInLight;

        [SerializeField]
        private Color m_uiCheckIn_Btn_Disable;

        [SerializeField]
        private Color m_uiCheckIn_Btn_Enable;

        [SerializeField]
        private TMP_Text m_checkInText;

        [SerializeField]
        private Color m_uiCheckInPrice_TitleText_Disable;

        [SerializeField]
        private Color m_uiCheckInPrice_TitleText_Enable;

        [SerializeField]
        private Image m_adReward;

        [SerializeField]
        private UIGuideInteractor uIGuideInteractor;

        private CheckInRewardData m_currentFocusReward = null;

        private List<UICheckInElement> uICheckInElements = new List<UICheckInElement>();

        private V2Enum_CheckInType m_v2Enum_CheckInType = V2Enum_CheckInType.Max;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_checkInButton != null)
                m_checkInButton.onClick.AddListener(OnClick_CheckInButton);

            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(() =>
                        {
                            if (m_v2Enum_CheckInType == V2Enum_CheckInType.Once)
                            {
                                SetCheckInElement(V2Enum_CheckInType.Repeat);
                            }
                            else
                                RequestDialogExit<InGameCheckInDialog>();
                        });
                }
            }

            if (m_onceBtn != null)
                m_onceBtn.onClick.AddListener(OnClick_OnceTab);

            if (m_repeatBtn != null)
                m_repeatBtn.onClick.AddListener(OnClick_RepeatTab);

            Message.AddListener<GameBerry.Event.RefreshCheckInRewardMsg>(RefreshCheckInReward);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshCheckInRewardMsg>(RefreshCheckInReward);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            //if (Managers.CheckInManager.Instance.canCheckInType != V2Enum_CheckInType.Max)
            //{
            //    SetCheckInElement(Managers.CheckInManager.Instance.canCheckInType);

            //    Managers.CheckInManager.Instance.canCheckInType = V2Enum_CheckInType.Max;
            //}

            Managers.CheckInManager.Instance.canCheckInType = V2Enum_CheckInType.Max;

            if (Managers.CheckInManager.Instance.ReadyAdView(V2Enum_CheckInType.Once))
                SetCheckInElement(V2Enum_CheckInType.Once);
            else
                SetCheckInElement(V2Enum_CheckInType.Repeat);

            //ScrollViewSnapToItem();

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.CheckInRewardGet)
                Managers.GuideInteractorManager.Instance.SetGuideStep(uIGuideInteractor);

            IsEnterState = true;
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.isAlive == false)
                return;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.CheckInRewardGet)
                Managers.GuideInteractorManager.Instance.SetGuideStep(1);

            IsEnterState = false;
        }
        //------------------------------------------------------------------------------------
        private void RefreshCheckInReward(GameBerry.Event.RefreshCheckInRewardMsg msg)
        {
            m_currentFocusReward = Managers.CheckInManager.Instance.GetFocusCheckInReward(m_v2Enum_CheckInType);

            for (int i = 0; i < uICheckInElements.Count; ++i)
            {
                uICheckInElements[i].RefreshCheckInElement();
            }

            SetFocusRewardState(m_v2Enum_CheckInType);
        }
        //------------------------------------------------------------------------------------
        private void SetFocusRewardState(V2Enum_CheckInType v2Enum_CheckInType)
        {
            m_currentFocusReward = Managers.CheckInManager.Instance.GetFocusCheckInReward(v2Enum_CheckInType);

            bool isReady = Managers.CheckInManager.Instance.IsReadyCheckInReward(m_currentFocusReward);
            bool isAdReady = Managers.CheckInManager.Instance.IsReadyAdCheckInReward(m_currentFocusReward);

            if (m_adReward != null)
                m_adReward.gameObject.SetActive(isReady == false && isAdReady == true);

            bool enableBtn = isReady == true || isAdReady == true;

            if (m_checkInButton != null)
            {
                m_checkInButton.interactable = enableBtn;
                m_checkInButton.image.color = enableBtn == true ? m_uiCheckIn_Btn_Enable : m_uiCheckIn_Btn_Disable;
            }

            if (m_checkInLight != null)
                m_checkInLight.gameObject.SetActive(enableBtn);

            if (m_checkInText != null)
            {
                m_checkInText.color = enableBtn == true ? m_uiCheckInPrice_TitleText_Enable : m_uiCheckInPrice_TitleText_Disable;
            }
        }
        //------------------------------------------------------------------------------------
        private void SetCheckInElement(V2Enum_CheckInType v2Enum_CheckInType)
        {
            if (m_v2Enum_CheckInType == v2Enum_CheckInType)
                return;

            CheckInKindData checkInKindData = m_checkInKindDatas.Find(x => x.V2Enum_CheckInType == v2Enum_CheckInType);

            if (checkInKindData != null)
            {
                if (m_title != null)
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_title, checkInKindData.TitleLocalKey);

                if (m_bgTrans != null)
                {
                    Vector2 sizedelta = m_bgTrans.sizeDelta;
                    sizedelta.y = checkInKindData.BGHeight;
                    m_bgTrans.sizeDelta = sizedelta;
                }
            }


            m_v2Enum_CheckInType = v2Enum_CheckInType;

            SetFocusRewardState(m_v2Enum_CheckInType);

            List<CheckInRewardData> checkInRewardDatas = Managers.CheckInManager.Instance.GetCheckInRewardDatas(m_v2Enum_CheckInType);

            int selectIndex = 0;

            for (int i = 0; i < checkInRewardDatas.Count; ++i)
            {
                UICheckInElement uICheckInElement = null;

                if (uICheckInElements.Count <= i)
                {
                    GameObject clone = Instantiate(m_uICheckInElement.gameObject, m_uICheckInElementRoot);
                    if (clone == null)
                        break;

                    uICheckInElement = clone.GetComponent<UICheckInElement>();
                    if (uICheckInElement == null)
                        break;

                    uICheckInElements.Add(uICheckInElement);
                }
                else
                {
                    uICheckInElement = uICheckInElements[i];
                }

                if (uICheckInElement == null)
                    continue;

                uICheckInElement.gameObject.SetActive(true);
                uICheckInElement.SetCheckInElement(checkInRewardDatas[i]);
                uICheckInElement.RefreshCheckInElement();

                selectIndex = i;
            }

            for (int i = selectIndex + 1; i < uICheckInElements.Count; ++i)
            {
                uICheckInElements[i].gameObject.SetActive(false);
            }

            int elementGroupFrameSibling = m_elementScrollRect.transform.GetSiblingIndex();

            if (m_onceBtn != null)
            {
                m_onceBtn.image.sprite = m_v2Enum_CheckInType == V2Enum_CheckInType.Repeat ? m_noneTabBG : m_selectTabBG;
                int newsibling = elementGroupFrameSibling + (m_v2Enum_CheckInType == V2Enum_CheckInType.Repeat ? -1 : 1);
                m_onceBtn.transform.SetSiblingIndex(newsibling);
            }

            elementGroupFrameSibling = m_elementScrollRect.transform.GetSiblingIndex();

            if (m_repeatBtn != null)
            {
                m_repeatBtn.image.sprite = m_v2Enum_CheckInType == V2Enum_CheckInType.Repeat ? m_selectTabBG : m_noneTabBG;
                int newsibling = elementGroupFrameSibling + (m_v2Enum_CheckInType == V2Enum_CheckInType.Repeat ? 1 : -1);
                m_repeatBtn.transform.SetSiblingIndex(newsibling);
            }


            if (m_once_Text != null)
                m_once_Text.color = m_v2Enum_CheckInType == V2Enum_CheckInType.Repeat ? m_noneTab_Text : m_selectTab_Text;

            if (m_repeat_Text != null)
                m_repeat_Text.color = m_v2Enum_CheckInType == V2Enum_CheckInType.Repeat ? m_selectTab_Text : m_noneTab_Text;

            //ScrollViewSnapToItem();
        }
        //------------------------------------------------------------------------------------
        private void ScrollViewSnapToItem()
        {
            UICheckInElement uIMasteryElement = null;

            if (m_currentFocusReward == null)
            {
                if (uICheckInElements.Count > 0)
                {
                    uIMasteryElement = uICheckInElements[uICheckInElements.Count - 1];
                }
            }
            else
            {
                if (uICheckInElements != null)
                {
                    if (m_currentFocusReward.CheckInCount.GetDecrypted() <= uICheckInElements.Count)
                    {
                        uIMasteryElement = uICheckInElements[m_currentFocusReward.CheckInCount.GetDecrypted() - 1];
                    }
                }
            }

            if (uIMasteryElement != null)
            {
                RectTransform rectTransform = null;
                if (uIMasteryElement.TryGetComponent(out rectTransform))
                {
                    Vector2 offset = Vector2.zero;
                    Util.ScrollViewSnapToItem(m_elementScrollRect, m_scrollRectContent, rectTransform, offset);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_OnceTab()
        {
            SetCheckInElement(V2Enum_CheckInType.Once);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RepeatTab()
        {
            SetCheckInElement(V2Enum_CheckInType.Repeat);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_CheckInButton()
        {
            Managers.CheckInManager.Instance.DoCheckInReward(m_currentFocusReward);
        }
        //------------------------------------------------------------------------------------
    }
}