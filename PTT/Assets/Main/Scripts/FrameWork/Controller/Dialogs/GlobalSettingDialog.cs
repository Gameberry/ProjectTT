using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using BackEnd;

namespace GameBerry.UI
{
    [System.Serializable]
    public class SettingToggleElementData
    {
        public Managers.GameSettingBtn GameSettingBtnEnum = Managers.GameSettingBtn.Max;
        public UISettingToggleElement SettingElement;
    }

    [System.Serializable]
    public class LanguageSelectBtn
    {
        public LocalizeType LocalizeTypeEnum;
        public Button LanguageSelect;
        public TMP_Text LanguageText;

        private System.Action<LocalizeType> Action = null;

        public void ConnectCallBack(System.Action<LocalizeType> action)
        {
            if (LanguageSelect != null)
                LanguageSelect.onClick.AddListener(OnClick_Btn);
            Action = action;
        }

        public void OnClick_Btn()
        {
            if (Action != null)
                Action(LocalizeTypeEnum);
        }
    }

    [System.Serializable]
    public class LinkAccountBtn
    {
        public TheBackEnd.LoginType LinkingType;

        public Button LanguageSelect;
        public TMP_Text LanguageText;

        private System.Action<TheBackEnd.LoginType> Action = null;

        public bool IsConnect = false;

        public void ConnectCallBack(System.Action<TheBackEnd.LoginType> action)
        {
            if (LanguageSelect != null)
                LanguageSelect.onClick.AddListener(OnClick_Btn);
            Action = action;
        }

        public void OnClick_Btn()
        {
            if (IsConnect == true)
                return;

            if (Action != null)
                Action(LinkingType);
        }
    }

    public class GlobalSettingDialog : IDialog
    {
        [SerializeField]
        private Button m_closeSettingDialog;

        [Header("------------GameSetGroup------------")]
        [SerializeField]
        private List<UISettingToggleElement> m_settingToggleElementDatas = new List<UISettingToggleElement>();

        [SerializeField]
        private Button m_powerSavingModeBtn;

        [SerializeField]
        private IDialog m_DialogLanguage;

        [SerializeField]
        private Button m_changeLanguage;

        [SerializeField]
        private Button m_exitChangeLanguage;

        [SerializeField]
        private Transform m_changeLanguage_Popup;

        [SerializeField]
        private List<LanguageSelectBtn> m_languageSelectBtns = new List<LanguageSelectBtn>();

        [SerializeField]
        private Color m_languageEnableBtnColor;

        [SerializeField]
        private Color m_languageDisableBtnColor;

        [SerializeField]
        private Color m_languageEnableTextColor;

        [SerializeField]
        private Color m_languageDisableTextColor;

        [Header("------------EtcGroup------------")]
        [SerializeField]
        private Button m_termsOfServiceBtnView;

        [SerializeField]
        private Button m_privacyPolicyBtnView;

        [SerializeField]
        private Button m_contactUsBtnView;

        [SerializeField]
        private Button m_discordBtnView;

        [SerializeField]
        private Button m_loungeBtnView;

        [SerializeField]
        private Button m_couponBtnView;

        [Header("------------AccountGroup------------")]
        [SerializeField]
        private TMP_Text m_playerName;

        [SerializeField]
        private Button m_playerNameChangeShowPopup;

        [SerializeField]
        private TMP_Text m_playerUid;

        [SerializeField]
        private Button m_capyPlayerUid;

        [SerializeField]
        private List<LinkAccountBtn> m_linkAccountBtns = new List<LinkAccountBtn>();
        
        [SerializeField]
        private Button m_logOut;

        [SerializeField]
        private Button m_withdrawal;

        [Header("------------Version------------")]
        [SerializeField]
        private TMP_Text m_clientVersion;

        [SerializeField]
        private TMP_Text m_serverVersion;

        [Header("------------CheckWithdrawal------------")]
        [SerializeField]
        private IDialog m_checkLogoutPopup_IDialog;

        [SerializeField]
        private Transform m_checkLogoutPopup;

        [SerializeField]
        private Button m_doLogout;

        [SerializeField]
        private Button m_cancelLogout;

        [Header("------------CheckWithdrawal------------")]
        [SerializeField]
        private IDialog m_checkWithdrawalPopup_IDialog;

        [SerializeField]
        private Transform m_checkWithdrawalPopup;

        [SerializeField]
        private Button m_doWithdrawal;

        [SerializeField]
        private Button m_cancelWithdrawal;

        [SerializeField]
        private TMP_InputField m_deleteAcountWithdrawal;

        [Header("------------Coupon------------")]
        [SerializeField]
        private IDialog m_couponPopup_IDialog;

        [SerializeField]
        private Transform m_couponPopup;

        [SerializeField]
        private Button m_doCoupon;

        [SerializeField]
        private Button m_cancelCoupon;

        [SerializeField]
        private TMP_InputField m_couponInputField;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            Message.AddListener<GameBerry.Event.RefreshNickNameMsg>(RefreshNickName);
            

            if (m_closeSettingDialog != null)
                m_closeSettingDialog.onClick.AddListener(() =>
                {
                    RequestDialogExit<GlobalSettingDialog>();
                });

            for (int i = 0; i < m_settingToggleElementDatas.Count; ++i)
            {
                m_settingToggleElementDatas[i].Init();
            }

            if(m_powerSavingModeBtn != null)
                m_powerSavingModeBtn.onClick.AddListener(() =>
                {
                    RequestDialogEnter<GlobalPowerSavingDialog>();
                });

            if (m_changeLanguage != null)
                m_changeLanguage.onClick.AddListener(() =>
                {
                    if (m_DialogLanguage != null)
                        m_DialogLanguage.ElementEnter();
                    //if (m_changeLanguage_Popup != null)
                    //    m_changeLanguage_Popup.gameObject.SetActive(true);
                });

            if (m_exitChangeLanguage != null)
                m_exitChangeLanguage.onClick.AddListener(() =>
                {
                    if (m_DialogLanguage != null)
                        m_DialogLanguage.ElementExit();

                    //if (m_changeLanguage_Popup != null)
                    //    m_changeLanguage_Popup.gameObject.SetActive(false);
                });

            for (int i = 0; i < m_languageSelectBtns.Count; ++i)
            {
                m_languageSelectBtns[i].ConnectCallBack(ChangeLanguage);
            }

            RefreshLanguageUI();

            if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop
                || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.QA)
            {
                if (m_clientVersion != null)
                    m_clientVersion.text = string.Format("CVer : {0}", Project.version);
            }
            else
            {
                if (m_clientVersion != null)
                    m_clientVersion.text = string.Format("{0}", Project.version);
            }
            

            if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop)
            {
                if (m_serverVersion != null)
                {
                }
            }
            else if(Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.QA
                || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Stage)
            {
                if (m_serverVersion != null)
                {
                    m_serverVersion.text = string.Format("SVer : {0}", Managers.SceneManager.Instance.BuildElement.ToString());
                }
            }
            else
            {
                if (m_serverVersion != null)
                {
                    m_serverVersion.gameObject.SetActive(false);
                }
            }

            if (m_playerUid != null)
                m_playerUid.text = TheBackEnd.TheBackEndManager.Instance.GetUserID();

            if (m_capyPlayerUid != null)
                m_capyPlayerUid.onClick.AddListener(OnClick_CapyPlayerUid);

            ////////////////////////Account

            if (m_playerName != null)
                m_playerName.text = TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName();

            if (m_playerNameChangeShowPopup != null)
                m_playerNameChangeShowPopup.onClick.AddListener(() =>
                {
                    RequestDialogEnter<InGameNickNameChangePopupDialog>();
                });

            
            for (int i = 0; i < m_linkAccountBtns.Count; ++i)
            {
#if UNITY_IOS
                if (m_linkAccountBtns[i].LinkingType == TheBackEnd.LoginType.Apple)
                {
                    if (m_linkAccountBtns[i].LanguageSelect != null)
                    {
                        m_linkAccountBtns[i].LanguageSelect.gameObject.SetActive(true);
                    }
                }
#else
                if (m_linkAccountBtns[i].LinkingType == TheBackEnd.LoginType.Apple)
                {
                    if (m_linkAccountBtns[i].LanguageSelect != null)
                    {
                        m_linkAccountBtns[i].LanguageSelect.gameObject.SetActive(false);
                    }
                }
#endif
                m_linkAccountBtns[i].ConnectCallBack(LinkAccount);
            }

            if (m_logOut != null)
                m_logOut.onClick.AddListener(ShowLogoutPopup);

            if (m_withdrawal != null)
                m_withdrawal.onClick.AddListener(ShowCheckWithdrawalPopup);

            RefreshLinkAccountUI();

            if (m_doLogout != null)
                m_doLogout.onClick.AddListener(OnClick_DoLogout);

            if (m_cancelLogout != null)
                m_cancelLogout.onClick.AddListener(OnClick_CancelLogout);

            if (m_doWithdrawal != null)
                m_doWithdrawal.onClick.AddListener(OnClick_DoWithdrawal);

            if (m_cancelWithdrawal != null)
                m_cancelWithdrawal.onClick.AddListener(OnClick_CancelWithdrawal);

            ////////////////////////Account


            ////////////////////////Etc
            if (m_termsOfServiceBtnView != null)
                m_termsOfServiceBtnView.onClick.AddListener(ProjectNoticeContent.Instance.Show_TermsView);

            if (m_privacyPolicyBtnView != null)
                m_privacyPolicyBtnView.onClick.AddListener(ProjectNoticeContent.Instance.Show_PrivacyView);

            if (m_contactUsBtnView != null)
                m_contactUsBtnView.onClick.AddListener(Managers.SceneManager.Instance.GoOqupie);

            if (m_discordBtnView != null)
                m_discordBtnView.onClick.AddListener(Managers.SceneManager.Instance.GoDiscode);

            if (m_loungeBtnView != null)
            {
                //if (Application.systemLanguage == SystemLanguage.Korean)
                //{
                //    m_loungeBtnView.gameObject.SetActive(true);
                //    m_loungeBtnView.onClick.AddListener(Managers.SceneManager.Instance.GoLounge);
                //}
                //else
                //    m_loungeBtnView.gameObject.SetActive(false);
            }
            ////////////////////////Etc


            ////////////////////////Coupon
            if (m_couponBtnView != null)
            {
//#if UNITY_IOS
//                m_couponBtnView.gameObject.SetActive(false);
//#else
//                m_couponBtnView.gameObject.SetActive(true);
//                m_couponBtnView.onClick.AddListener(ShowCouponPopup);
//#endif
            }

            if (m_doCoupon != null)
                m_doCoupon.onClick.AddListener(OnClick_DoCoupon);

            if (m_cancelCoupon != null)
                m_cancelCoupon.onClick.AddListener(OnClick_CancelCoupon);
            ////////////////////////Coupon
            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString += RefreshLanguageUI;
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshNickNameMsg>(RefreshNickName);

            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString -= RefreshLanguageUI;
        }
        //------------------------------------------------------------------------------------
        private void RefreshNickName(GameBerry.Event.RefreshNickNameMsg msg)
        {
            if (m_playerName != null)
                m_playerName.text = Managers.PlayerDataManager.Instance.GetPlayerName();
        }
        //------------------------------------------------------------------------------------
        public void ChangeLanguage(LocalizeType localizeType)
        {
            Managers.LocalStringManager.Instance.ChangeLocalize(localizeType);
        }
        //------------------------------------------------------------------------------------
        public void RefreshLanguageUI()
        {
            LocalizeType localizeType = Managers.LocalStringManager.Instance.GetLocalizeType();

            for (int i = 0; i < m_languageSelectBtns.Count; ++i)
            {
                if (m_languageSelectBtns[i] != null)
                {
                    bool isenable = localizeType == m_languageSelectBtns[i].LocalizeTypeEnum;

                    if (m_languageSelectBtns[i].LanguageSelect != null)
                        m_languageSelectBtns[i].LanguageSelect.image.color = isenable == true ? m_languageEnableBtnColor : m_languageDisableBtnColor;

                    if (m_languageSelectBtns[i].LanguageText != null)
                        m_languageSelectBtns[i].LanguageText.color = isenable == true ? m_languageEnableTextColor : m_languageDisableTextColor;
                }
            }

        }
        //------------------------------------------------------------------------------------
        public void LinkAccount(TheBackEnd.LoginType linkingType)
        {
#if UNITY_EDITOR
            return;
#endif

            Contents.GlobalContent.VisibleBufferingUI(true);

            if (linkingType == TheBackEnd.LoginType.Google)
                TheBackEnd.TheBackEndManager.Instance.ChangeCustomToFederation_Google(LinkResult);
            else if (linkingType == TheBackEnd.LoginType.Apple)
                TheBackEnd.TheBackEndManager.Instance.ChangeCustomToFederation_Apple(LinkResult);
            //GamePotManager.Instance.createLinking(linkingType, LinkResult);
        }
        //------------------------------------------------------------------------------------
        public void LinkResult(BackendReturnObject resultLinking)
        {
            Contents.GlobalContent.VisibleBufferingUI(false);

            if (resultLinking == null)
            { 
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("Setting_Message_SNS_Fail"));
                return;
            }

            switch (resultLinking.IsSuccess())
            {
                case true:
                    {
                        Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("Setting_Message_SNS_Sucess"));
                        RefreshLinkAccountUI();
                        break;
                    }
                case false:
                    {

                        Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("Setting_Message_SNS_Fail"));

                        break;
                    }
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshLinkAccountUI()
        {
            for (int i = 0; i < m_linkAccountBtns.Count; ++i)
            {
                if (m_linkAccountBtns[i] != null)
                {
                    bool isenable = m_linkAccountBtns[i].LinkingType == TheBackEnd.TheBackEndManager.Instance.UserLoginType;

                    //if (m_linkAccountBtns[i].LanguageSelect != null)
                    //    m_linkAccountBtns[i].LanguageSelect.image.color = isenable == true ? m_languageEnableBtnColor : m_languageDisableBtnColor;

                    if (m_linkAccountBtns[i].LanguageText != null)
                    {
                        //m_linkAccountBtns[i].LanguageText.color = isenable == true ? m_languageEnableTextColor : m_languageDisableTextColor;
                        if (isenable == true)
                            Managers.LocalStringManager.Instance.SetLocalizeText(m_linkAccountBtns[i].LanguageText, "common/option/accountLinked");
                        else
                        {
                            if (m_linkAccountBtns[i].LinkingType == TheBackEnd.LoginType.Google)
                            {
                                Managers.LocalStringManager.Instance.SetLocalizeText(m_linkAccountBtns[i].LanguageText, "common/ui/accountGoogleLogin");
                            }
                            else if (m_linkAccountBtns[i].LinkingType == TheBackEnd.LoginType.Apple)
                            {
                                Managers.LocalStringManager.Instance.SetLocalizeText(m_linkAccountBtns[i].LanguageText, "common/ui/accountAppleLogin");
                            }
                            else
                                Managers.LocalStringManager.Instance.SetLocalizeText(m_linkAccountBtns[i].LanguageText, "common/option/accountLinkedNot");
                        }
                    }

                    m_linkAccountBtns[i].IsConnect = isenable;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_CapyPlayerUid()
        {
            if (m_playerUid != null)
            { 
                UniClipboard.SetText(m_playerUid.text);
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("common/ui/complete"));
            }
        }
        //------------------------------------------------------------------------------------
        private void ShowLogoutPopup()
        {
            if (m_checkLogoutPopup_IDialog != null)
                m_checkLogoutPopup_IDialog.ElementEnter();
            //if (m_checkLogoutPopup != null)
            //    m_checkLogoutPopup.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_DoLogout()
        {
            if (m_checkLogoutPopup_IDialog != null)
                m_checkLogoutPopup_IDialog.ElementExit();

            //if (m_checkLogoutPopup != null)
            //    m_checkLogoutPopup.gameObject.SetActive(false);

            TheBackEnd.TheBackEndManager.Instance.Logout();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_CancelLogout()
        {
            if (m_checkLogoutPopup_IDialog != null)
                m_checkLogoutPopup_IDialog.ElementExit();

            //if (m_checkLogoutPopup != null)
            //    m_checkLogoutPopup.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void ShowCheckWithdrawalPopup()
        {
            if (m_deleteAcountWithdrawal != null)
                m_deleteAcountWithdrawal.text = string.Empty;

            if (m_checkWithdrawalPopup_IDialog != null)
                m_checkWithdrawalPopup_IDialog.ElementEnter();

            //if (m_checkWithdrawalPopup != null)
            //    m_checkWithdrawalPopup.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_DoWithdrawal()
        {
            if (m_deleteAcountWithdrawal != null)
            {
                if (m_deleteAcountWithdrawal.text != "Delete Account")
                    return;
            }

            if (m_checkWithdrawalPopup_IDialog != null)
                m_checkWithdrawalPopup_IDialog.ElementExit();

            //if (m_checkWithdrawalPopup != null)
            //    m_checkWithdrawalPopup.gameObject.SetActive(false);

            TheBackEnd.TheBackEndManager.Instance.WithdrawAccount();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_CancelWithdrawal()
        {
            if (m_checkWithdrawalPopup_IDialog != null)
                m_checkWithdrawalPopup_IDialog.ElementExit();

            //if (m_checkWithdrawalPopup != null)
            //    m_checkWithdrawalPopup.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void ShowCouponPopup()
        {
            if (m_couponPopup_IDialog != null)
                m_couponPopup_IDialog.ElementEnter();

            //if (m_couponPopup != null)
            //    m_couponPopup.gameObject.SetActive(true);

            if (m_couponInputField != null)
                m_couponInputField.text = string.Empty;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_DoCoupon()
        {
            string couponNum = string.Empty;

            if (m_couponInputField != null)
                couponNum = m_couponInputField.text;

            TheBackEnd.TheBackEndManager.Instance.ReceiveCoupon(couponNum, RecvCoupon);

            Contents.GlobalContent.VisibleBufferingUI(true);
        }
        //------------------------------------------------------------------------------------
        public void RecvCoupon(BackendReturnObject backendReturnObject)
        {
            if (m_couponPopup_IDialog != null)
                m_couponPopup_IDialog.ElementExit();

            //if (m_couponPopup != null)
            //    m_couponPopup.gameObject.SetActive(false);

            Contents.GlobalContent.VisibleBufferingUI(false);

            if (backendReturnObject.IsSuccess() == false)
            {
                Contents.GlobalContent.ShowGlobalNotice(backendReturnObject.GetMessage());
                Debug.LogError(backendReturnObject.GetMessage());
                return;
            }

            if (backendReturnObject.GetReturnValuetoJSON().ContainsKey("itemObject") == false)
                return;

            RequestDialogExit<GlobalSettingDialog>();

            GameBerry.Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new GameBerry.Event.SetInGameRewardPopupMsg();

            LitJson.JsonData json = backendReturnObject.GetReturnValuetoJSON()["itemObject"];

            for (int i = 0; i < json.Count; i++)
            {
                try
                {
                    RewardData rewardData = Managers.RewardManager.Instance.GetRewardData();
                    rewardData.Amount = json[i]["itemCount"].ToString().ToDouble();
                    rewardData.V2Enum_Goods = json[i]["item"]["GoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                    rewardData.Index = json[i]["item"]["GoodsIndex"].ToString().ToInt();

                    Managers.GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

                    m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }

            if (m_setInGameRewardPopupMsg.RewardDatas.Count > 0)
            { 
                Message.Send(m_setInGameRewardPopupMsg);
                RequestDialogEnter<InGameRewardPopupDialog>();
            }

            TheBackEnd.TheBackEndManager.Instance.SendUpdateWaitData(true);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_CancelCoupon()
        {
            if (m_couponPopup_IDialog != null)
                m_couponPopup_IDialog.ElementExit();

            //if (m_couponPopup != null)
            //    m_couponPopup.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
    }
}