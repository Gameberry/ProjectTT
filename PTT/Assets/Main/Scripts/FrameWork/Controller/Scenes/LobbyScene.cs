using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Scene
{
    public class LobbyScene : IScene
    {
        protected override void OnLoadComplete()
        {// 여기서 각 매니저들을 초기화하고 다 끝났으면 여기서 처음으로 스타트 해준다.

            //Managers.SceneManager.Instance.DeleteAppInitProcess();

            //Managers.DungeonManager.Instance.InitPlayDungeon();

            //Managers.GuideQuestManager.Instance.PlayGuideQuest();

            //if (Managers.CheckInManager.Instance.canCheckInType != V2Enum_CheckInType.Max)
            //{
            //    if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.CheckIn) == true)
            //    {
            //        UI.IDialog.RequestDialogEnter<UI.InGameCheckInDialog>();
            //    }
            //}
        }
    }
}