using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class StaminaContainer
    {
        public static ObscuredDouble EventDayInitTime = 0; // 일일초기화

        public static ObscuredDouble StaminaLastChargeTime = 0;

        public static ObscuredInt StaminaAccumUse = 0;

        public static ObscuredInt ToDayDigAdCount = 0;
        public static ObscuredInt ToDayDigDiaBuyCount = 0;

    }
}