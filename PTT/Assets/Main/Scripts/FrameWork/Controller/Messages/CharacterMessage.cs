using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Event
{
    public class RefreshNickNameMsg : Message
    {
    }

    public class RefreshPlayerSkinMsg : Message
    { 

    }

    public class RefreshComboUIMsg : Message
    {
        public int Combo = 0;
    }
}