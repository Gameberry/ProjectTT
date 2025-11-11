using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIAdBuffPopupElement : MonoBehaviour
    {
        [SerializeField]
        private Image m_buffIcon;

        [SerializeField]
        private Image m_buffEnableBG;

        [SerializeField]
        private TMP_Text m_buffStatDesc;

        [SerializeField]
        private CButton m_buffActiveBtn;

        [SerializeField]
        private Transform m_buffInActive;

        [SerializeField]
        private TMP_Text m_buffActiveText;

        [SerializeField]
        private TMP_Text m_adRemainTime;

        private AdBuffActiveData _adBuffActiveData;

        private UIGuideInteractor uIGuideInteractor;

        private System.Action<AdBuffActiveData> _action;

        //------------------------------------------------------------------------------------
        public void InitBuffElement(AdBuffActiveData adbuffstatedata, System.Action<AdBuffActiveData> action)
        {
            _adBuffActiveData = adbuffstatedata;

            if (m_buffIcon != null)
                m_buffIcon.sprite = Managers.AdBuffManager.Instance.GetSprite(adbuffstatedata);

            if (m_buffActiveBtn != null)
                m_buffActiveBtn.onClick.AddListener(OnClick_BuffActiveBtn);

            if (Define.IsAdBuffAlways == false)
                Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += RefreshAdBuffState;

            _action = action;

            SetRefreshBuffState();
        }
        //------------------------------------------------------------------------------------
        public void SetRefreshBuffState()
        {
            if (_adBuffActiveData == null)
                return;

            bool isEnableBuff = Managers.AdBuffManager.Instance.IsEnableActiveBuff(_adBuffActiveData);

            if (m_buffEnableBG != null)
                m_buffEnableBG.gameObject.SetActive(isEnableBuff);

            if (m_buffIcon != null)
                m_buffIcon.color = isEnableBuff == true ? Color.white : Color.gray;

            if (m_buffActiveBtn != null)
            {
                m_buffActiveBtn.SetInteractable(isEnableBuff == false);
            }

            if (m_buffStatDesc != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_buffStatDesc, string.Format("addBuff/{0}/desc", _adBuffActiveData.ResourceIndex));

            if (isEnableBuff == true)
            {
                if (m_buffActiveBtn != null)
                    m_buffActiveBtn.SetInteractable(false);

                if (m_buffActiveText != null)
                    m_buffActiveText.gameObject.SetActive(true);


                if (m_buffInActive != null)
                    m_buffInActive.gameObject.SetActive(false);

                RefreshBuffRemainTime();
            }
            else
            {
                if (m_buffActiveBtn != null)
                    m_buffActiveBtn.SetInteractable(true);

                if (m_buffActiveText != null)
                    m_buffActiveText.gameObject.SetActive(false);

                if (m_buffInActive != null)
                    m_buffInActive.gameObject.SetActive(true);
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshAdBuffState()
        {
            RefreshBuffRemainTime();
        }
        //------------------------------------------------------------------------------------
        private void RefreshBuffRemainTime()
        {
            if (_adBuffActiveData == null)
                return;

            if (Define.IsAdBuffAlways == true)
            {
                if (m_adRemainTime != null)
                { 
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_adRemainTime, "adbuff/always");
                    m_adRemainTime.gameObject.SetActive(true);
                }
                return;
            }

            bool isEnableBuff = Managers.AdBuffManager.Instance.IsEnableActiveBuff(_adBuffActiveData);

            if (m_adRemainTime != null)
            {
                if (isEnableBuff == false)
                {
                    m_adRemainTime.gameObject.SetActive(false);
                }
                else
                {
                    m_adRemainTime.gameObject.SetActive(true);

                    int remainSecond = (int)(Managers.AdBuffManager.Instance.GetBuffEndTime(_adBuffActiveData) - Managers.TimeManager.Instance.Current_TimeStamp);
                    int remainMinute = (int)remainSecond / 60;

                    if (remainMinute < 1)
                        m_adRemainTime.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), remainSecond);
                    else
                        m_adRemainTime.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), remainMinute);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_BuffActiveBtn()
        {
            _action?.Invoke(_adBuffActiveData);
        }
        //------------------------------------------------------------------------------------
        public UIGuideInteractor SetAdBuffGuideBtn()
        {
            if (m_buffActiveBtn != null)
            {
                uIGuideInteractor = m_buffActiveBtn.gameObject.AddComponent<UIGuideInteractor>();
                uIGuideInteractor.MyGuideType = V2Enum_EventType.BuffAddView;
                uIGuideInteractor.MyStepID = 2;
                uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.End;
                uIGuideInteractor.IsAutoSetting = false;
                uIGuideInteractor.ConnectInteractor();

                return uIGuideInteractor;
            }

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}