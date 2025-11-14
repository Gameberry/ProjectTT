using System.Collections.Generic;
using UnityEngine;
using GameBerry.UI;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections;

using System.Linq;

namespace GameBerry.Contents
{
    public class InGameContent : IContent
    {
        private GameBerry.Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new GameBerry.Event.SetInGameRewardPopupMsg();
        private GameBerry.Event.SetInGameRewardPopup_TitleMsg m_SetInGameRewardPopup_TitleMsg = new GameBerry.Event.SetInGameRewardPopup_TitleMsg();

        //------------------------------------------------------------------------------------
        protected override void OnLoadStart()
        {
            Managers.BattleSceneManager.Instance.InitBattleScene();

            StartCoroutine(CompleteFade());
        }
        //------------------------------------------------------------------------------------
        private IEnumerator CompleteFade()
        {
            //yield return new WaitForSeconds(0.3f);
            GlobalContent.DoFade(false);
            yield return new WaitForSeconds(0.5f);

            SetLoadComplete();
        }
        //------------------------------------------------------------------------------------
        protected override void OnLoadComplete()
        {
            //GlobalContent.DoFade(true);

            TheBackEnd.TheBackEndManager.Instance.EnableSendRecvUpdateCheck();
            TheBackEnd.TheBackEndManager.Instance.OnAutoSave();

            ThirdPartyLog.Instance.SendLog_Game_Connect();
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            Managers.AOSBackBtnManager.Instance.InitManager();
            Managers.BattleSceneManager.Instance.ChangeBattleScene(Enum_Dungeon.StageScene);
        }
        //------------------------------------------------------------------------------------
    }
}