using System.Collections.Generic;
// 대체로 로직(Dungeon_Base)과 다이얼로그의 디펜던시 방지를 위해 만듬

namespace GameBerry.Event
{
    public class RefreshBattleSceneUIMsg : Message
    {

    }

    public class VisibleDungeonExitPopupMsg : Message
    {
        public bool Visible = false;
    }
}