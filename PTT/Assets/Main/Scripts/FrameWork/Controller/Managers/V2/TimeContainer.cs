using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class TimeContainer
    {
        public static ObscuredDouble AccumLoginTime = 0.0;
        public static ObscuredDouble DailyInitTimeStamp = 0.0;
        public static ObscuredInt AccumLoginCount = 0;
    }
}