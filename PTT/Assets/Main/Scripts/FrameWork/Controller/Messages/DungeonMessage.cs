using System.Collections.Generic;
// 대체로 로직(Dungeon_Base)과 다이얼로그의 디펜던시 방지를 위해 만듬

namespace GameBerry.Event
{
    /// <summary>
    /// QuickLink
    /// </summary>

    public class SetCharacterContentNavBarStateMsg : Message
    {
        public ContentDetailList ContentDetailList;
    }

    public class SetAllyContentNavBarStateMsg : Message
    {
        public ContentDetailList ContentDetailList;
    }

    public class ShowShortCutAllyJewelryViewMsg : Message
    {
        
    }

    public class SetShopContentNavBarStateMsg : Message
    {
        public ContentDetailList ContentDetailList;
    }

    public class SetDungeonContentDialogStateMsg : Message
    {
        public ContentDetailList ContentDetailList;
    }

    public class SetShopSummonDialogStateMsg : Message
    {
        public ContentDetailList ContentDetailList;
    }

    public class SetShopGerneralDialogStateMsg : Message
    {
        public ContentDetailList ContentDetailList;
    }

    public class SetPassDialogStateMsg : Message
    {
        public ContentDetailList ContentDetailList;
    }

    public class VisibleDungeonExitPopupMsg : Message
    {
        public bool Visible = false;
    }
}