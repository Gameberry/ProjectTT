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
        [Header("------------NameChangePopup------------")]
        [SerializeField]
        private TMP_InputField _nickNameInputField;

        [SerializeField]
        private TMP_Text _nameChangeInputField_Placeholder;

        [SerializeField]
        private TMP_Text _nameChangeNotice;

        [SerializeField]
        private TMP_Text _nameChangePrice;

        [SerializeField]
        private Button _nameChangeBtn;

        protected override void OnLoad()
        {
            //if (m_playerName != null)
            //    m_playerName.text = TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName();

            if (_nameChangePrice != null)
            {
                _nameChangePrice.text = string.Format("{0:#,0.##}", Define.NickNameChangeDiaCost);
            }

            if (_nickNameInputField != null)
            {
                _nickNameInputField.onValueChanged.AddListener(onValueChanged_Nickname);
                _nickNameInputField.characterLimit = Define.NickNameMaxCount;
            }

            if (_nameChangeBtn != null)
                _nameChangeBtn.onClick.AddListener(OnCilck_ChangeNickName);

            //Message.AddListener<GameBerry.Event.RefreshNickNameMsg>(RefreshNickName);
        }
        //------------------------------------------------------------------------------------
        bool isfirst = false;
        protected override void OnEnter()
        {
            isfirst = TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName() == Backend.UID;

            if (_nameChangePrice != null)
            {
                _nameChangePrice.text = isfirst == true ? Managers.LocalStringManager.Instance.GetLocalString("common/ui/free") : string.Format("{0:#,0.##}", Define.NickNameChangeDiaCost);
            }

            if (_nameChangeInputField_Placeholder != null)
                _nameChangeInputField_Placeholder.text = TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName();
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

            if (_nickNameInputField.text.Length < Define.NickNameMinCount || _nickNameInputField.text.Length > Define.NickNameMaxCount)
            {
                if (_nameChangeNotice != null)
                {
                    _nameChangeNotice.text = Managers.LocalStringManager.Instance.GetLocalString("PlayerNick_Message_CharacterCount");
                    _nameChangeNotice.gameObject.SetActive(true);
                }

                return;
            }

            string idChecker = Regex.Replace(_nickNameInputField.text, string.Format(@"{0}", Managers.SceneManager.Instance.SpacialCharRegex), string.Empty, RegexOptions.Singleline);

            if (_nickNameInputField.text != idChecker)
            {
                if (_nameChangeNotice != null)
                {
                    _nameChangeNotice.text = Managers.LocalStringManager.Instance.GetLocalString("PlayerNick_Message_SpecialCharacter");
                    _nameChangeNotice.gameObject.SetActive(true);
                }

                return;
            }

            if (ProhibitedWordChecker.Instance.CheckProhibitedWord(_nickNameInputField.text, false) == true)
            {
                beforenickname = TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName();

                TheBackEnd.TheBackEndManager.Instance.UpdateNickName(_nickNameInputField.text, RefreshNickName);
            }
            else
            {
                if (_nameChangeNotice != null)
                {
                    _nameChangeNotice.text = Managers.LocalStringManager.Instance.GetLocalString("PlayerNick_Message_ProhibitedWords");
                    _nameChangeNotice.gameObject.SetActive(true);
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
                    _nameChangeNotice.text = Managers.LocalStringManager.Instance.GetLocalString("PlayerNick_UI_Overlap");
                    _nameChangeNotice.gameObject.SetActive(true);
                }
                else
                {
                    Contents.GlobalContent.ShowGlobalNotice(backendReturnObject.GetMessage());
                }
            }
            else
            {
                if (_nickNameInputField != null)
                    _nickNameInputField.text = string.Empty;

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

                UIManager.DialogExit<InGameNickNameChangePopupDialog>();
            }
        }
        //------------------------------------------------------------------------------------
        private void onValueChanged_Nickname(string nickname)
        {
            if (_nameChangeNotice != null)
                _nameChangeNotice.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
    }
}