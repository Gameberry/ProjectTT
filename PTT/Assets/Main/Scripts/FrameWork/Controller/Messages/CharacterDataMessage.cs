using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Event
{
    public class RefreshNickNameMsg : Message
    {
    }

    public class SetInGameRewardPopupMsg : Message
    {
        public V2Enum_Goods GoodsType = V2Enum_Goods.Max;
        public List<RewardData> RewardDatas = new List<RewardData>();
    }

    public class SetInGameRewardPopup_TitleMsg : Message
    {
        public string title;
    }

    public class SetSelectGoodsPopupMsg : Message
    {
        public V2Enum_Goods v2Enum_Goods;

        public List<ObscuredInt> SelectIndexList;

        public System.Action<int> SelectedCallBack = null;
    }

    public class SetGoodsDescPopupMsg : Message
    {
        public V2Enum_Goods v2Enum_Goods;
        public int index;
        public double timeGoodsTime;
    }
}