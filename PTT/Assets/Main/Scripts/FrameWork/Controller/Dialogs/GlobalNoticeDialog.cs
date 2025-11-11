using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Event;
using TMPro;

namespace GameBerry.UI
{
    public class GlobalNoticeDialog : IDialog
    {
        [SerializeField]
        private UIGlobalNoticeElement m_uIGlobalNoticeElement;

        [SerializeField]
        private Transform m_uIGlobalNoticeRoot;

        [SerializeField]
        private Transform m_uIContentOpenNoticeGroup;

        [SerializeField]
        private TMPro.TMP_Text m_uIContentOpenTextGroup;

        [SerializeField]
        private Button m_uIContentOpenExitBtn;

        [SerializeField]
        private UIGlobalNoticeElement_BattlePower m_uIGlobalNoticeElement_BattlePower;

        [SerializeField]
        private UIGlobalNoticeElement_SynergyCount m_uIGlobalNoticeElement_SynergyGauge;

        [SerializeField]
        private UIGlobalNoticeElement _uIGlobalNoticeElement_Guide;

        [SerializeField]
        private Transform m_uIGlobalNoticeElement_Guide_UpPos;

        [SerializeField]
        private Transform m_uIGlobalNoticeElement_Guide_DownPos;

        private Queue<string> m_reservationMsg = new Queue<string>();
        private Queue<UIGlobalNoticeElement> m_uIGlobalNoticeElement_Pool = new Queue<UIGlobalNoticeElement>();

        private Queue<string> m_contentOpenNoticeQueue = new Queue<string>();

        [Header("-------------DungeonExitPopup----------------")]
        [SerializeField]
        private IDialog m_dungeonExitPopupIDialog;

        [SerializeField]
        private Transform m_dungeonExitPopUpGroup;

        [SerializeField]
        private TMP_Text m_dungeonExitPopUpText;

        [SerializeField]
        private Button m_closedungeonExitPopup;

        [SerializeField]
        private Button m_dungeonExit;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_uIGlobalNoticeElement_BattlePower != null)
                m_uIGlobalNoticeElement_BattlePower.Init(EndNoticeElementBattlePower);

            if (m_uIGlobalNoticeElement_SynergyGauge != null)
                m_uIGlobalNoticeElement_SynergyGauge.Init(EndNoticeElementBattlePower);
            
            if (_uIGlobalNoticeElement_Guide != null)
                _uIGlobalNoticeElement_Guide.Init(EndNoticeElement_Guide);
            
            if (m_uIContentOpenExitBtn != null)
                m_uIContentOpenExitBtn.onClick.AddListener(OnClick_ContentOpenNotice);

            if (m_closedungeonExitPopup != null)
                m_closedungeonExitPopup.onClick.AddListener(OnClick_CloseDungeonExitPopup);

            if (m_dungeonExit != null)
                m_dungeonExit.onClick.AddListener(OnClick_ForceToExitDungeonMsg);

            if (m_dungeonExitPopupIDialog != null)
            {
                m_dungeonExitPopupIDialog.exitCallBack += Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed;
            }

            Message.AddListener<GameBerry.Event.GlobalNoticeMsg>(GlobalNotice);
            Message.AddListener<GameBerry.Event.GlobalUnLockContentNoticeMsg>(GlobalUnLockContentNotice);
            Message.AddListener<GameBerry.Event.GlobalGuideNoticeMsg>(GlobalGuideNotice);
            Message.AddListener<GameBerry.Event.GlobalNoticeBattlePowerMsg>(GlobalNoticeBattlePower);
            Message.AddListener<GameBerry.Event.GlobalNoticeSynergyCountMsg>(GlobalNoticeSynergyCount);
            Message.AddListener<VisibleDungeonExitPopupMsg>(VisibleDungeonExitPopup);

            
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            while (m_reservationMsg.Count > 0)
            {
                ShowNoticeMessage(m_reservationMsg.Dequeue(), 0.5f);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.GlobalNoticeMsg>(GlobalNotice);
            Message.RemoveListener<GameBerry.Event.GlobalUnLockContentNoticeMsg>(GlobalUnLockContentNotice);
            Message.RemoveListener<GameBerry.Event.GlobalGuideNoticeMsg>(GlobalGuideNotice);
            Message.RemoveListener<GameBerry.Event.GlobalNoticeBattlePowerMsg>(GlobalNoticeBattlePower);
            Message.RemoveListener<GameBerry.Event.GlobalNoticeSynergyCountMsg>(GlobalNoticeSynergyCount);
            Message.RemoveListener<VisibleDungeonExitPopupMsg>(VisibleDungeonExitPopup);
        }
        //------------------------------------------------------------------------------------
        private void GlobalNotice(GameBerry.Event.GlobalNoticeMsg msg)
        {
            if (isEnter == false)
            {
                m_reservationMsg.Enqueue(msg.NoticeString);
                return;
            }

            ShowNoticeMessage(msg.NoticeString, msg.duration);
        }
        //------------------------------------------------------------------------------------
        private void ShowNoticeMessage(string msg, float duration)
        {
            UIGlobalNoticeElement element = GetNoticeElement();
            if (element == null)
                return;

            element.transform.SetAsLastSibling();
            element.ShowNoticeElement(msg, duration);
        }
        //------------------------------------------------------------------------------------
        private UIGlobalNoticeElement GetNoticeElement()
        {
            if (m_uIGlobalNoticeElement_Pool.Count > 0)
                return m_uIGlobalNoticeElement_Pool.Dequeue();

            GameObject clone = Instantiate(m_uIGlobalNoticeElement.gameObject, m_uIGlobalNoticeRoot);

            if (clone != null)
            {
                UIGlobalNoticeElement element = clone.GetComponent<UIGlobalNoticeElement>();

                element.Init(PoolNoticeElement);

                return element;
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        private void PoolNoticeElement(UIGlobalNoticeElement uIGlobalNoticeElement)
        {
            uIGlobalNoticeElement.gameObject.SetActive(false);

            m_uIGlobalNoticeElement_Pool.Enqueue(uIGlobalNoticeElement);
        }
        //------------------------------------------------------------------------------------
        private void GlobalUnLockContentNotice(GameBerry.Event.GlobalUnLockContentNoticeMsg msg)
        {
            if (m_uIContentOpenNoticeGroup == null || m_uIContentOpenTextGroup == null)
                return;

            if (m_uIContentOpenNoticeGroup.gameObject.activeSelf == true)
                m_contentOpenNoticeQueue.Enqueue(msg.titletext);
            else
            {
                m_uIContentOpenNoticeGroup.gameObject.SetActive(true);
                m_uIContentOpenTextGroup.SetText(msg.titletext);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ContentOpenNotice()
        {
            if (m_contentOpenNoticeQueue.Count > 0)
                m_uIContentOpenTextGroup.SetText(m_contentOpenNoticeQueue.Dequeue());
            else
                m_uIContentOpenNoticeGroup.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void GlobalGuideNotice(GameBerry.Event.GlobalGuideNoticeMsg msg)
        {
            if (_uIGlobalNoticeElement_Guide != null)
            {
                if (msg.duration <= 0)
                {
                    _uIGlobalNoticeElement_Guide.ForceReleaseHide();
                    _uIGlobalNoticeElement_Guide.gameObject.SetActive(false);
                }
                else
                {
                    _uIGlobalNoticeElement_Guide.transform.SetAsLastSibling();
                    _uIGlobalNoticeElement_Guide.ShowNoticeElement_Guide(msg.NoticeString, msg.duration);
                    Vector3 pos = _uIGlobalNoticeElement_Guide.transform.localPosition;
                    pos.y = msg.height;
                    _uIGlobalNoticeElement_Guide.transform.localPosition = pos;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void GlobalNoticeBattlePower(GameBerry.Event.GlobalNoticeBattlePowerMsg msg)
        {
            if (m_uIGlobalNoticeElement_BattlePower != null)
            {
                m_uIGlobalNoticeElement_BattlePower.transform.SetAsLastSibling();
                m_uIGlobalNoticeElement_BattlePower.ShowNoticeElement_BattlePower(msg.battlePower, msg.changeValue);
            }
        }
        //------------------------------------------------------------------------------------
        private void GlobalNoticeSynergyCount(GameBerry.Event.GlobalNoticeSynergyCountMsg msg)
        {
            if (m_uIGlobalNoticeElement_SynergyGauge != null)
            {
                m_uIGlobalNoticeElement_SynergyGauge.transform.SetAsLastSibling();
                m_uIGlobalNoticeElement_SynergyGauge.ShowNoticeElement_SynergyCount(msg.before, msg.after);
            }
        }
        
        //------------------------------------------------------------------------------------
        private void EndNoticeElementBattlePower(UIGlobalNoticeElement uIGlobalNoticeElement)
        {
            uIGlobalNoticeElement.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void EndNoticeElement_Guide(UIGlobalNoticeElement uIGlobalNoticeElement)
        {
            uIGlobalNoticeElement.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void VisibleDungeonExitPopup(VisibleDungeonExitPopupMsg msg)
        {
            if (m_dungeonExitPopupIDialog != null)
            {
                if (msg.Visible == true)
                {
                    if (m_dungeonExitPopUpText != null)
                    {
                        //if (Managers.DungeonManager.Instance.CurrentDungeonKinds == V2Enum_Dungeon.DevilCastleDungeon)
                        //    Managers.LocalStringManager.Instance.SetLocalizeText(m_dungeonExitPopUpText, "devilCastle/exitDesc");
                        //else if (Managers.DungeonManager.Instance.CurrentDungeonKinds == V2Enum_Dungeon.GoddessDungeon
                        //    || Managers.DungeonManager.Instance.CurrentDungeonKinds == V2Enum_Dungeon.RedBullDungeon
                        //    || Managers.DungeonManager.Instance.CurrentDungeonKinds == V2Enum_Dungeon.UrsulaDungeon
                        //    || Managers.DungeonManager.Instance.CurrentDungeonKinds == V2Enum_Dungeon.KingSlimeDungeon)
                        //    Managers.LocalStringManager.Instance.SetLocalizeText(m_dungeonExitPopUpText, "goddessDungeon/exitComment");
                        if (Managers.BattleSceneManager.Instance.BattleType == V2Enum_Dungeon.StageScene)
                            Managers.LocalStringManager.Instance.SetLocalizeText(m_dungeonExitPopUpText, "stage/exitComment");
                        else
                            Managers.LocalStringManager.Instance.SetLocalizeText(m_dungeonExitPopUpText, "dungeon/exitComment");
                    }
                    m_dungeonExitPopupIDialog.ElementEnter();

                    Managers.BattleSceneManager.Instance.ChangeTimeScale(V2Enum_ARR_BattleSpeed.Pause);
                }
                else
                {
                    Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();
                    m_dungeonExitPopupIDialog.ElementExit();
                }
            }

            //if (m_dungeonExitPopUpGroup != null)
            //    m_dungeonExitPopUpGroup.gameObject.SetActive(msg.Visible);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_CloseDungeonExitPopup()
        {
            Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();
            if (m_dungeonExitPopupIDialog != null)
            {
                m_dungeonExitPopupIDialog.ElementExit();
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ForceToExitDungeonMsg()
        {
            OnClick_CloseDungeonExitPopup();

            ThirdPartyLog.Instance.SendLog_log_dungeon_end(3, MapContainer.GetLogStage() + MapContainer.GetLogWave());
            Managers.MapManager.Instance.RemoveEnterKey();
            Managers.BattleSceneManager.Instance.ChangeBattleScene(V2Enum_Dungeon.LobbyScene);
        }
        //------------------------------------------------------------------------------------
    }
}