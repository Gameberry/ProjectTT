using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gpm.Ui;
using TMPro;
using System.Text.RegularExpressions;
using BackEnd;

namespace GameBerry.UI
{
    public class InGameNickNameChangePopupDialog : IDialog
    {
        [SerializeField]
        private List<Button> exitBtn;

        [Header("------------NameChangePopup------------")]
        [SerializeField]
        private TMP_InputField m_nickNameInputField;

        [SerializeField]
        private TMP_Text m_nameChangeInputField_Placeholder;

        [SerializeField]
        private TMP_Text m_nameChangeNotice;

        [SerializeField]
        private TMP_Text m_nameChangePrice;

        [SerializeField]
        private Button m_nameChangeBtn;

        protected override void OnLoad()
        {
            if (exitBtn != null)
            {
                for (int i = 0; i < exitBtn.Count; ++i)
                {
                    if (exitBtn[i] != null)
                        exitBtn[i].onClick.AddListener(OnClick_ExitBtn);
                }
            }

            //if (m_playerName != null)
            //    m_playerName.text = TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName();

            if (m_nameChangePrice != null)
            {
                m_nameChangePrice.text = string.Format("{0:#,0.##}", Define.NickNameChangeDiaCost);
            }

            if (m_nickNameInputField != null)
                m_nickNameInputField.onValueChanged.AddListener(onValueChanged_Nickname);

            if (m_nickNameInputField != null)
                m_nickNameInputField.characterLimit = Define.NickNameMaxCount;

            if (m_nameChangeBtn != null)
                m_nameChangeBtn.onClick.AddListener(OnCilck_ChangeNickName);

            //Message.AddListener<GameBerry.Event.RefreshNickNameMsg>(RefreshNickName);
        }
        //------------------------------------------------------------------------------------
        public void OnClick_ExitBtn()
        {
            UIManager.DialogExit<InGameNickNameChangePopupDialog>();
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            //Message.RemoveListener<GameBerry.Event.RefreshNickNameMsg>(RefreshNickName);
        }
        //------------------------------------------------------------------------------------
        //private void RefreshNickName(GameBerry.Event.RefreshNickNameMsg msg)
        //{
        //    if (m_playerName != null)
        //        m_playerName.text = Managers.PlayerDataManager.Instance.GetPlayerName();
        //}
        ////------------------------------------------------------------------------------------
        bool isfirst = false;
        protected override void OnEnter()
        {
            isfirst = TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName() == Backend.UID;

            if (m_nameChangePrice != null)
            {
                m_nameChangePrice.text = isfirst == true ? Managers.LocalStringManager.Instance.GetLocalString("common/ui/free") : string.Format("{0:#,0.##}", Define.NickNameChangeDiaCost);
            }

            if (m_nameChangeInputField_Placeholder != null)
                m_nameChangeInputField_Placeholder.text = TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName();
        }
        //------------------------------------------------------------------------------------
        private string beforenickname = string.Empty;
        //------------------------------------------------------------------------------------
        private void OnCilck_ChangeNickName()
        {
            if (isfirst == false)
            {
                if (Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.Dia.Enum32ToInt()) < Define.NickNameChangeDiaCost)
                {
                    Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("PlayerNick_UI_ShortOfGoods"));
                    return;
                }
            }

            if (m_nickNameInputField.text.Length < Define.NickNameMinCount || m_nickNameInputField.text.Length > Define.NickNameMaxCount)
            {
                if (m_nameChangeNotice != null)
                {
                    m_nameChangeNotice.text = Managers.LocalStringManager.Instance.GetLocalString("PlayerNick_Message_CharacterCount");
                    m_nameChangeNotice.gameObject.SetActive(true);
                }

                return;
            }

            string idChecker = Regex.Replace(m_nickNameInputField.text, string.Format(@"{0}", Managers.SceneManager.Instance.SpacialCharRegex), string.Empty, RegexOptions.Singleline);

            if (m_nickNameInputField.text != idChecker)
            {
                if (m_nameChangeNotice != null)
                {
                    m_nameChangeNotice.text = Managers.LocalStringManager.Instance.GetLocalString("PlayerNick_Message_SpecialCharacter");
                    m_nameChangeNotice.gameObject.SetActive(true);
                }

                return;
            }

            if (ProhibitedWordChecker.Instance.CheckProhibitedWord(m_nickNameInputField.text, false) == true)
            {
                beforenickname = TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName();

                TheBackEnd.TheBackEndManager.Instance.UpdateNickName(m_nickNameInputField.text, RefreshNickName);
            }
            else
            {
                if (m_nameChangeNotice != null)
                {
                    m_nameChangeNotice.text = Managers.LocalStringManager.Instance.GetLocalString("PlayerNick_Message_ProhibitedWords");
                    m_nameChangeNotice.gameObject.SetActive(true);
                }
                return;
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshNickName(BackendReturnObject backendReturnObject)
        {
            if (backendReturnObject.IsSuccess() == false)
            {
                if (backendReturnObject.GetStatusCode() == "409")
                {
                    m_nameChangeNotice.text = Managers.LocalStringManager.Instance.GetLocalString("PlayerNick_UI_Overlap");
                    m_nameChangeNotice.gameObject.SetActive(true);
                }
                else
                {
                    Contents.GlobalContent.ShowGlobalNotice(backendReturnObject.GetMessage());
                }
            }
            else
            {
                if (m_nickNameInputField != null)
                    m_nickNameInputField.text = string.Empty;

                int reward_type = 0;
                double before_quan = 0, reward_quan = 0, after_quan = 0;

                if (isfirst == false)
                {
                    reward_type = V2Enum_Point.LobbyGold.Enum32ToInt();
                    before_quan = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.Dia.Enum32ToInt());
                    reward_quan = Define.NickNameChangeDiaCost;

                    Managers.GoodsManager.Instance.UseGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.Dia.Enum32ToInt(), Define.NickNameChangeDiaCost);

                    after_quan = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.Dia.Enum32ToInt());

                    List<string> goodstable = new List<string>();
                    goodstable.Add(Define.PlayerPointTable);
                    TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(goodstable, null);
                }

                ThirdPartyLog.Instance.SendLog_NickEvent(beforenickname, TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName(),
                    reward_type, before_quan, reward_quan, after_quan);

                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("PlayerNick_UI_Complete"));

                OnClick_ExitBtn();
            }
        }
        //------------------------------------------------------------------------------------
        private void onValueChanged_Nickname(string nickname)
        {
            if (m_nameChangeNotice != null)
                m_nameChangeNotice.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
    }
}