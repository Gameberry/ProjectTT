using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using Spine;
using Spine.Unity;

namespace GameBerry.UI
{
    public class AppLoadingDialog : IDialog
    {
        [SerializeField]
        private TMP_Text m_clientVersion;

        [Header("----------Notice----------")]
        [SerializeField]
        private Transform m_noticeGroup;

        [SerializeField]
        private TMP_Text m_noticeText;

        [Header("----------LoginGroup----------")]
        [SerializeField]
        private Transform m_loginGroup;

        [Header("----------TermsConditions----------")]
        [SerializeField]
        private Transform m_termsConditions;

        [SerializeField]
        private Toggle m_termsToggle;

        [SerializeField]
        private Button m_termsViewBtn;

        [SerializeField]
        private Toggle m_privacyToggle;

        [SerializeField]
        private Button m_privacyViewBtn;

        [SerializeField]
        private Toggle m_pushToggle;

        [SerializeField]
        private Toggle m_pushNightToggle;

        [SerializeField]
        private Image m_TermsConditions_DisableImage;

        [SerializeField]
        private Button m_TermsConditions_AgreeBtn;

        [Header("----------PushCheck----------")]
        [SerializeField]
        private Transform m_pushCheck;

        [SerializeField]
        private TMP_Text m_pushStateCheckText;

        [SerializeField]
        private Button m_pushStateCheckOKBtn;

        [Header("----------LoginButtonGroup----------")]
        [SerializeField]
        private Transform m_loginBtnGroup;

        [SerializeField]
        private Button m_googleLogin_Btn;

        [SerializeField]
        private Button m_appleLogin_Btn;

        [SerializeField]
        private Button m_guestLogin_Btn;

        [Header("----------TouchToStart----------")]
        [SerializeField]
        private Transform m_touchToStart;

        [SerializeField]
        private Button m_touchToStartBtn;

        private bool m_pushOn = false;
        private bool m_pushNightOn = false;

        [SerializeField]
        private TMP_InputField m_getHashInput;

        [SerializeField]
        private Button m_getHashBtn;

        [Header("----------IntroSound----------")]
        [SerializeField]
        private AudioSource introAudioSource;

        [SerializeField]
        private SkeletonAnimationHandler_Graphic m_darkKnightSpine;

        //------------------------------------------------------------------------------------
        System.Action<GameBerry.TheBackEnd.LoginType> m_loginTypeCallBack = null;
        System.Action<string> m_nickNameCallBack = null;

        System.Action m_touchToStartCallBack = null;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_getHashBtn != null)
                m_getHashBtn.onClick.AddListener(() =>
                {
                    if (m_getHashInput != null)
                        m_getHashInput.text = GameBerry.TheBackEnd.TheBackEndManager.Instance.GetGoogleHash();
                });

            if (m_clientVersion != null)
                m_clientVersion.text = string.Format("ver.{0}", Project.version);

            //if (m_noticeGroup != null)
            //    m_noticeGroup.gameObject.SetActive(false);

            if (m_loginGroup != null)
                m_loginGroup.gameObject.SetActive(false);

            if (m_touchToStart != null)
                m_touchToStart.gameObject.SetActive(false);

            if (m_termsConditions != null)
                m_termsConditions.gameObject.SetActive(false);

            if (m_pushCheck != null)
                m_pushCheck.gameObject.SetActive(false);

            if (m_loginBtnGroup != null)
                m_loginBtnGroup.gameObject.SetActive(false);

            if (m_termsToggle != null)
                m_termsToggle.onValueChanged.AddListener(OnTermsToggle_ValueChange);

            if (m_termsViewBtn != null)
                m_termsViewBtn.onClick.AddListener(OnClick_TermsView);

            if (m_privacyToggle != null)
                m_privacyToggle.onValueChanged.AddListener(OnTermsToggle_ValueChange);

            if (m_privacyViewBtn != null)
                m_privacyViewBtn.onClick.AddListener(OnClick_PrivacyView);

            if (m_TermsConditions_AgreeBtn != null)
                m_TermsConditions_AgreeBtn.onClick.AddListener(OnClick_TermsConditions_AgreeBtn);

            if (m_pushToggle != null)
                m_pushToggle.onValueChanged.AddListener(OnTValueChange_PushToggle);

            if (m_pushNightToggle != null)
                m_pushNightToggle.onValueChanged.AddListener(OnTValueChange_PushNightToggle);

            if (m_pushStateCheckOKBtn != null)
                m_pushStateCheckOKBtn.onClick.AddListener(OnClick_PushStateCheckOKBtn);

            if (m_googleLogin_Btn != null)
                m_googleLogin_Btn.onClick.AddListener(OnClick_GoogleBtn);

            if (m_guestLogin_Btn != null)
                m_guestLogin_Btn.onClick.AddListener(OnClick_GuestBtn);

            if (m_appleLogin_Btn != null)
                m_appleLogin_Btn.onClick.AddListener(OnClick_AppleBtn);

            if (m_touchToStartBtn != null)
                m_touchToStartBtn.onClick.AddListener(OnClilck_TouchToStartBtn);

            if (m_darkKnightSpine != null)
                m_darkKnightSpine.PlayAnimation("intro");

            Message.AddListener<GameBerry.Event.SetNoticeMsg>(SetNotice);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetNoticeMsg>(SetNotice);
        }
        //------------------------------------------------------------------------------------
        private void SetNotice(GameBerry.Event.SetNoticeMsg msg)
        {
            SetNoticeText(msg.NoticeStr);
        }
        //------------------------------------------------------------------------------------
        public void SetNoticeText(string noticetext)
        {
            if (m_noticeText != null)
                m_noticeText.text = noticetext;
        }
        ////------------------------------------------------------------------------------------
        public void VisibleLoginProcess(bool visible)
        {
            if (visible == true)
            {
#if UNITY_EDITOR
                if (m_googleLogin_Btn != null)
                    m_googleLogin_Btn.gameObject.SetActive(false);

                if (m_appleLogin_Btn != null)
                    m_appleLogin_Btn.gameObject.SetActive(false);
#else
            if (m_googleLogin_Btn != null)
                m_googleLogin_Btn.gameObject.SetActive(true);

#if UNITY_IOS
            if (m_appleLogin_Btn != null)
                m_appleLogin_Btn.gameObject.SetActive(true);
#else
            if (m_appleLogin_Btn != null)
                m_appleLogin_Btn.gameObject.SetActive(false);
#endif

            
#endif

                // D+1테스트에서는 싹다 비활성화
                if (m_appleLogin_Btn != null)
                    m_appleLogin_Btn.gameObject.SetActive(false);
                // D+1테스트에서는 싹다 비활성화


                if (m_guestLogin_Btn != null)
                    m_guestLogin_Btn.gameObject.SetActive(true);
            }

            if (m_loginGroup != null)
                m_loginGroup.gameObject.SetActive(visible);
        }
        //------------------------------------------------------------------------------------
        public void VisibleTermsConditions(bool visible)
        {
            if (m_termsConditions != null)
                m_termsConditions.gameObject.SetActive(visible);
        }
        //------------------------------------------------------------------------------------
        private void OnTermsToggle_ValueChange(bool value)
        {
            bool termsToggle = false;
            bool privacyToggle = false;

            if (m_termsToggle != null)
                termsToggle = m_termsToggle.isOn;

            if (m_privacyToggle != null)
                privacyToggle = m_privacyToggle.isOn;

            if (m_TermsConditions_DisableImage != null)
                m_TermsConditions_DisableImage.gameObject.SetActive(termsToggle == false || privacyToggle == false);

            if (m_TermsConditions_AgreeBtn != null)
                m_TermsConditions_AgreeBtn.gameObject.SetActive(termsToggle == true && privacyToggle == true);
        }
        //------------------------------------------------------------------------------------
        private void OnTValueChange_PushToggle(bool value)
        {
            if (value == false)
            {
                if (m_pushNightToggle != null)
                    m_pushNightToggle.isOn = false;
            }
        }
        //------------------------------------------------------------------------------------
        private void OnTValueChange_PushNightToggle(bool value)
        {
            if (value == true)
            {
                if (m_pushToggle != null)
                    m_pushToggle.isOn = true;
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_TermsView()
        {
            ProjectNoticeContent.Instance.Show_TermsView();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PrivacyView()
        {
            ProjectNoticeContent.Instance.Show_PrivacyView();
        }
        //------------------------------------------------------------------------------------
        public void OnClick_TermsConditions_AgreeBtn()
        {
            if (m_pushToggle != null)
                m_pushOn = m_pushToggle.isOn;

            if (m_pushNightToggle != null)
                m_pushNightOn = m_pushNightToggle.isOn;

            TheBackEnd.TheBackEndManager.Instance.SavePushState(m_pushOn, m_pushNightOn);

            //GamePotManager.Instance.SetPushInfo(enablePush, enablePushNight, enablePushAd);

            VisibleTermsConditions(false);
            //VisibleLoginButtonGroup(true);

            if (m_pushCheck != null)
                m_pushCheck.gameObject.SetActive(true);

            if (m_pushStateCheckText != null)
            {
                string nowtimestr = System.DateTime.Now.ToString("yyyy-MM-dd");
                string gametitle = string.Format("[{0}]", Managers.LocalStringManager.Instance.GetLocalString("Common_UI_GameTitle"));
                string enablePushstr = m_pushOn == true ? Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_Push_OK") : Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_Push_NO");
                string enablePushNightstr = m_pushNightOn == true ? Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_PushNight_OK") : Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_PushNight_NO");

                m_pushStateCheckText.text = string.Format("{0}\n{1}\n{2}\n{3}", nowtimestr, gametitle, enablePushstr, enablePushNightstr);
            }
        }
        //------------------------------------------------------------------------------------
        public void OnClick_PushStateCheckOKBtn()
        {
            if (m_pushCheck != null)
                m_pushCheck.gameObject.SetActive(false);

            VisibleLoginButtonGroup(true);
        }
        //------------------------------------------------------------------------------------
        public void VisibleLoginButtonGroup(bool visible)
        {
            if (m_loginBtnGroup != null)
                m_loginBtnGroup.gameObject.SetActive(visible);
        }
        //------------------------------------------------------------------------------------
        public void SetLoginCallBack(System.Action<GameBerry.TheBackEnd.LoginType> callback)
        {
            m_loginTypeCallBack = callback;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_FaceBookBtn()
        {
            if (m_loginTypeCallBack != null)
                m_loginTypeCallBack(GameBerry.TheBackEnd.LoginType.Facebook);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_GoogleBtn()
        {
            if (m_loginTypeCallBack != null)
                m_loginTypeCallBack(GameBerry.TheBackEnd.LoginType.Google);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_AppleBtn()
        {
            if (m_loginTypeCallBack != null)
                m_loginTypeCallBack(GameBerry.TheBackEnd.LoginType.Apple);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_GuestBtn()
        {
            if (m_loginTypeCallBack != null)
                m_loginTypeCallBack(GameBerry.TheBackEnd.LoginType.CustomLogin);
        }
        //------------------------------------------------------------------------------------
        public void VisibleTouchToStart(bool visible)
        {
            if (m_touchToStart != null)
                m_touchToStart.gameObject.SetActive(visible);
        }
        //------------------------------------------------------------------------------------
        public void SetCallBack_TouchToStart(System.Action action)
        {
            m_touchToStartCallBack = action;
        }
        //------------------------------------------------------------------------------------
        private void OnClilck_TouchToStartBtn()
        {
            if (m_touchToStartCallBack != null)
                m_touchToStartCallBack();
        }
        //------------------------------------------------------------------------------------
    }
}
