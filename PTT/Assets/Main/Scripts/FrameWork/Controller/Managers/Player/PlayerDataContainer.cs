using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Gpm.Ui;
using UnityEngine;

namespace GameBerry
{
    public enum HpMpVarianceType
    { 
        None = 0,
        NomalDamage = V2Enum_Stat.Attack,
        CriticalChance = V2Enum_Stat.CritChance,

        Miss = 10000,
        RecoveryHP,
        RecoveryMP,
        Block,

        Hit,

        Dead,
        Revive,
        KillingMoney,
        DoubleInterest,

        GoldGainTimer,

        VampiricDmg,

        Monster,
    }

    public class CharacterProfileData : InfiniteScrollData
    {
        public bool isEnable = false;
        public Sprite sprite;
        public int profileIndex;
        
        public bool isAlly = false;
    }

    public static class PlayerDataContainer
    {
        public static string PlayerCombetPower = string.Empty;
        public static ObscuredDouble SaveHightestBattlePower = 0.0;
        public static string PlayerName;

        public static int Profile = 0;

        public static string PlayerServerKind = string.Empty;
        public static int PlayerServerNum = 0;

        public static string LogServerName = string.Empty;

        public static string DisplayServerName = string.Empty;
        public static string RankWindowDisplayServerName = string.Empty;
        public static string DisplayChatServerName = string.Empty;

        public static ObscuredInt PlayerCheatingCount = 0;
        public static ObscuredInt PlayerDontSearchCheat = 0;

        public static ObscuredInt PlayerRecvLaunchReward = 0;
        public static ObscuredInt PlayerRecvPreReward = 0;

        public static List<CharacterProfileData> characterProfileDatas = new List<CharacterProfileData>();

        public static Dictionary<int, CharacterProfileData> characterProfileDatas_Dic = new Dictionary<int, CharacterProfileData>();

        public static ObscuredDouble LastRouletteInitTimeStemp = 0.0;
        public static ObscuredDouble LastRouletteActionTime = 0.0;
        public static ObscuredInt TodayRouletteActionCount = 0;
    }
}