using System.Collections.Generic;
using UnityEngine;
using GameBerry.UI;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Contents
{
    
    public class GlobalContent : IContent
    {
        private static GameBerry.Event.ShowPopup_OkMsg _showPopup_OkMsg = new GameBerry.Event.ShowPopup_OkMsg();
        private static GameBerry.Event.ShowPopup_OkCancelMsg _showPopup_OkCancelMsg = new GameBerry.Event.ShowPopup_OkCancelMsg();
        private static GameBerry.Event.ShowPopup_InputMsg _showPopup_InputMsg = new GameBerry.Event.ShowPopup_InputMsg();

        private static GameBerry.Event.GlobalNoticeMsg _globalNoticeMsg = new GameBerry.Event.GlobalNoticeMsg();
        private static GameBerry.Event.GlobalNoticeBattlePowerMsg _globalNoticeBattlePowerMsg = new GameBerry.Event.GlobalNoticeBattlePowerMsg();
        private static GameBerry.Event.GlobalNoticeSynergyCountMsg _globalNoticeSynergyCountMsg = new GameBerry.Event.GlobalNoticeSynergyCountMsg();
        private static GameBerry.Event.GlobalUnLockContentNoticeMsg _globalUnLockContentNoticeMsg = new GameBerry.Event.GlobalUnLockContentNoticeMsg();
        private static GameBerry.Event.GlobalGuideNoticeMsg _globalGuideNoticeMsg = new GameBerry.Event.GlobalGuideNoticeMsg();

        private static GameBerry.Event.DoFadeMsg _fadeMsg = new GameBerry.Event.DoFadeMsg();
        private static GameBerry.Event.DoDungeonFadeMsg _dungeonFadeMsg = new GameBerry.Event.DoDungeonFadeMsg();

        private static GameBerry.Event.SetBuffStringMsg _setBuffStringMsg = new GameBerry.Event.SetBuffStringMsg();

        private static GameBerry.Event.SetSelectGoodsPopupMsg _setSelectGoodsPopupMsg = new GameBerry.Event.SetSelectGoodsPopupMsg();
        private static GameBerry.Event.SetGoodsDescPopupMsg _setGoodsDescPopupMsg = new GameBerry.Event.SetGoodsDescPopupMsg();

        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            UIManager.DialogEnter<GlobalPopupDialog>();
            UIManager.DialogEnter<GlobalDungeonFadeDialog>();
            UIManager.DialogEnter<GlobalFadeDialog>();
            UIManager.DialogEnter<GlobalNoticeDialog>();
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            UIManager.DialogExit<GlobalPopupDialog>();
            UIManager.DialogExit<GlobalBufferingDialog>();
            UIManager.DialogExit<GlobalButtonLockDialog>();
            UIManager.DialogExit<GlobalDungeonFadeDialog>();
            UIManager.DialogExit<GlobalFadeDialog>();
            UIManager.DialogExit<GlobalNoticeDialog>();
        }
        //------------------------------------------------------------------------------------
        public static void ShowMaintenanceError()
        {
            ProjectNoticeContent.Instance.ShowCheckDialog(Managers.LocalStringManager.Instance.GetLocalString("Notice_Check"));

            Managers.AOSBackBtnManager.Instance.QuickExitGame = true;

            BackEnd.Backend.Notice.GetTempNotice((callback) => {

                string coment = callback.Substring(26, callback.Length - 26 - 2).Replace("\\n", "\n");
                string desc = coment.Split(',')[0];


                string[] arr = coment.Split(',');
                if (arr.Length >= 3)
                {
                    System.DateTime startTime = System.DateTime.Parse(arr[1]).ToLocalTime();
                    System.DateTime endTime = System.DateTime.Parse(arr[2]).ToLocalTime();

                    string notice = string.Format(Managers.LocalStringManager.Instance.GetLocalString(arr[0])
                        , startTime.ToString("yyyy-MM-dd HH:mm:ss")
                        , endTime.ToString("yyyy-MM-dd HH:mm:ss"));

                    ProjectNoticeContent.Instance.ShowCheckDialog(notice);
                }

                Debug.Log(callback);
            });
        }
        //------------------------------------------------------------------------------------
        public static void DoFade(bool visible, float duration = 1.0f)
        {
            // visible : false 투명 -> 검은색으로

            _fadeMsg.duration = duration;
            _fadeMsg.visible = visible;

            Message.Send(_fadeMsg);
        }
        //------------------------------------------------------------------------------------
        public static void DoDungeonFade(bool visible, float duration = 1.0f)
        {
            // visible : false 투명 -> 검은색으로

            _dungeonFadeMsg.duration = duration;
            _dungeonFadeMsg.visible = visible;

            Message.Send(_dungeonFadeMsg);
        }
        //------------------------------------------------------------------------------------
        private static bool showBufferingUI = false;
        private static bool showNetWorkBufferingUI = false;
        public static void VisibleBufferingUI(bool visible)
        {
            if (visible == true)
                UIManager.DialogEnter<GlobalBufferingDialog>();
            else
            {
                if (showNetWorkBufferingUI == false)
                    UIManager.DialogExit<GlobalBufferingDialog>();
            }

            showBufferingUI = visible;
        }
        //------------------------------------------------------------------------------------
        public static void VisibleBufferingUI(bool visible, string buffstring)
        {
            if (visible == true)
                UIManager.DialogEnter<GlobalBufferingDialog>();
            else
            {
                if (showNetWorkBufferingUI == false)
                    UIManager.DialogExit<GlobalBufferingDialog>();
            }

            showBufferingUI = visible;

            _setBuffStringMsg.buffstr = buffstring;
            Message.Send(_setBuffStringMsg);
        }
        //------------------------------------------------------------------------------------
        public static void VisibleNetWorkBufferingUI(bool visible)
        {
            if (visible == true)
                UIManager.DialogEnter<GlobalBufferingDialog>();
            else
            {
                if (showBufferingUI == false)
                    UIManager.DialogExit<GlobalBufferingDialog>();
            }

            showNetWorkBufferingUI = visible;
        }
        //------------------------------------------------------------------------------------
        public static void SetButtonLock(bool btnlock)
        {
            if (btnlock == true)
                UIManager.DialogEnter<GlobalButtonLockDialog>();
            else
                UIManager.DialogExit<GlobalButtonLockDialog>();
        }
        //------------------------------------------------------------------------------------
        public static void ShowPopup_Ok(string titletext, string contenttext, System.Action okAction = null, System.Action<bool> toDayHide = null)
        {
            _showPopup_OkMsg.titletext = titletext;
            _showPopup_OkMsg.contenttext = contenttext;
            _showPopup_OkMsg.okAction = okAction;
            _showPopup_OkMsg.toDayHide = toDayHide;

            Message.Send(_showPopup_OkMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowPopup_OkCancel(string titletext, string contenttext, System.Action okAction = null, System.Action cancelAction = null, System.Action<bool> toDayHide = null)
        {
            _showPopup_OkCancelMsg.titletext = titletext;
            _showPopup_OkCancelMsg.contenttext = contenttext;

            _showPopup_OkCancelMsg.useCustomOKBtn = false;

            _showPopup_OkCancelMsg.okAction = okAction;
            _showPopup_OkCancelMsg.cancelAction = cancelAction;
            _showPopup_OkCancelMsg.toDayHide = toDayHide;

            Message.Send(_showPopup_OkCancelMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowPopup_CustomOkCancel(
            string titletext, 
            string contenttext, 

            string okbtntitle,
            Sprite okbtnicon,
            string okbtncontent,

            System.Action okAction = null, 
            System.Action cancelAction = null,
            System.Action<bool> toDayHide = null)
        {
            _showPopup_OkCancelMsg.titletext = titletext;
            _showPopup_OkCancelMsg.contenttext = contenttext;

            _showPopup_OkCancelMsg.useCustomOKBtn = true;
            _showPopup_OkCancelMsg.OKBtnTitle = okbtntitle;
            _showPopup_OkCancelMsg.OKBtnIcon = okbtnicon;
            _showPopup_OkCancelMsg.OKBtnContent = okbtncontent;

            _showPopup_OkCancelMsg.okAction = okAction;
            _showPopup_OkCancelMsg.cancelAction = cancelAction;
            _showPopup_OkCancelMsg.toDayHide = toDayHide;

            Message.Send(_showPopup_OkCancelMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowPopup_Input(string titletext, string defaultstr, string placeholder, System.Action<string> okAction, System.Action cancelAction = null, System.Action<bool> toDayHide = null)
        {
            _showPopup_InputMsg.titletext = titletext;
            _showPopup_InputMsg.defaultstr = defaultstr;
            _showPopup_InputMsg.placeholder = placeholder;
            _showPopup_InputMsg.okAction = okAction;
            _showPopup_InputMsg.cancelAction = cancelAction;
            _showPopup_InputMsg.toDayHide = toDayHide;

            Message.Send(_showPopup_InputMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowGlobalNotice(string notice, float duration = 1.0f)
        {
            _globalNoticeMsg.duration = duration;
            _globalNoticeMsg.NoticeString = notice;

            Message.Send(_globalNoticeMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowGlobalNotice_LocalString(string localkey, float duration = 1.0f)
        {
            _globalNoticeMsg.duration = duration;
            _globalNoticeMsg.NoticeString = Managers.LocalStringManager.Instance.GetLocalString(localkey); ;

            Message.Send(_globalNoticeMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowGlobalNotice_BattlePower(double battlePower, double changeValue)
        {
            _globalNoticeBattlePowerMsg.battlePower = battlePower;
            _globalNoticeBattlePowerMsg.changeValue = changeValue;

            Message.Send(_globalNoticeBattlePowerMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowGlobalNotice_SynergyCount(int before, int after)
        {
            _globalNoticeSynergyCountMsg.before = before;
            _globalNoticeSynergyCountMsg.after = after;

            Message.Send(_globalNoticeSynergyCountMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowGlobalNotice_Guide(string notice, float duration = 4.0f, float height = -180.0f)
        {
            _globalGuideNoticeMsg.duration = duration;
            _globalGuideNoticeMsg.NoticeString = notice;
            _globalGuideNoticeMsg.height = height;

            Message.Send(_globalGuideNoticeMsg);
        }
        //------------------------------------------------------------------------------------
        public static void HideGlobalNotice_Guide()
        {
            _globalGuideNoticeMsg.duration = 0;
            _globalGuideNoticeMsg.NoticeString = string.Empty;
            _globalGuideNoticeMsg.height = 0;

            Message.Send(_globalGuideNoticeMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowUnLockContentNotice(string notice)
        {
            _globalUnLockContentNoticeMsg.titletext = notice;

            Message.Send(_globalUnLockContentNoticeMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowSelectGoodsPopup(V2Enum_Goods v2Enum_Goods, List<ObscuredInt> SelectIndexList, System.Action<int> SelectedCallBack)
        {
            UI.UIManager.DialogEnter<InGameSelectGoodsPopupDialog>();

            _setSelectGoodsPopupMsg.v2Enum_Goods = v2Enum_Goods;
            _setSelectGoodsPopupMsg.SelectIndexList = SelectIndexList;
            _setSelectGoodsPopupMsg.SelectedCallBack = SelectedCallBack;

            Message.Send(_setSelectGoodsPopupMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowGoodsDescPopup(V2Enum_Goods v2Enu_Goods, int index, double timegoodstime = 0.0)
        {
            UI.UIManager.DialogEnter<InGameGoodsDescPopupDialog>();

            _setGoodsDescPopupMsg.v2Enum_Goods = v2Enu_Goods;

            if (_setGoodsDescPopupMsg.v2Enum_Goods == V2Enum_Goods.Max)
                _setGoodsDescPopupMsg.v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(index);

            _setGoodsDescPopupMsg.index = index;
            _setGoodsDescPopupMsg.timeGoodsTime = timegoodstime;

            Message.Send(_setGoodsDescPopupMsg);
        }
        //------------------------------------------------------------------------------------
    }
}