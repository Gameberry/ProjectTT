using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class GuideQuestContainer
    {
        public static ObscuredInt ClearGuideQuestOrder = -1;
        public static string CurrentGuideQuestOrder = "1";
        public static Dictionary<int, bool> ContainBossChallengeStep = new Dictionary<int, bool>();
    }
}