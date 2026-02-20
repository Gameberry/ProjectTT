using UnityEngine;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public enum IFFType
    {
        IFF_None = 0,
        IFF_Friend, // 우리팀
        IFF_Foe, // 상대팀
    }

    public enum Enum_Dungeon
    {
        None,
        StageScene,
    }

    public enum CharacterState : byte
    {
        None = 0,
        Idle,
        Run,
        Attack,
        Hit,
        Dead,

        Skill,

        Max,
    }


    public enum Enum_LookDirection
    {
        None = 0,
        Left,
        Right,
    }

    // 트리거
    public enum Enum_TriggerType
    {
        Active = 11,
        Passive = 12,
        Default = 13,

        Max,
    }

    public enum Enum_AttackRangeType
    {
        None = 0,
        Circle = 11,
        Line = 12,
        Sector = 13, // 부채꼴
        Max,
    }

    // 대미지
    public enum Enum_DamageType
    {
        Direct = 11,
        Projectile = 12,
        Sunken = 13,
        Pierce = 14,
        Void = 15,
        DirectVisioning = 16,
        RepeatAttack = 17,

        Max,
    }

    public enum Enum_Rarity
    {
        Common = 1,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic,
        Special,

        Max,
    }

    public enum Enum_Tier
    {
        Low = 1,    // 하급
        Normal,    // 중급
        High,      // 상급
        Top,       // 최상급

        Max,
    }

    public enum Enum_EquipType
    {
        Bow = 1,
        Helmet = 2,
        Armor = 3,
        Glove = 4,
        Shoes = 5,
        Cape = 6,

        Max,
    }

    public enum Enum_PointType
    {
        Dia,
        Gold,
        MagicShard,
        BlackEssence,
        RoyalCoin,
        Pact,
        Starforce,
        WeaponSummon,
        LanternSummon,
        Mileage,

        Max,
    }

    // 아이템의 출처/도메인 타입
    public enum Enum_ItemType
    {
        Potion,
        Material,
        Equip,
        Skin,
        Point,

        Weapon,

        Max
    }

    // Item이 어디에 저장되고 어떻게 취급되는지에 대한 단일 분류
    public enum Enum_ItemStorageType
    {
        Inventory,   // 일반 아이템/장비 인벤토리
        Point,       // 재화(지갑)
        Skin,         // 스킨 컬렉션
        Weapon       // 무기
    }

    public enum Enum_SummonType
    {
        Weapon = 11,
        Lantern = 12,
        Max,
    }

    public enum Enum_StarforceResult
    {
        Success,
        Stay,
        Down,
        Destroy,

        Max
    }


    [System.Serializable]
    public class AttackData
    {
        public int ResourceIndex;
        [HideInInspector]
        public CharacterControllerBase Hitter;
        public double DamageRate;
        public float AttackRange;
        public float HitRange;
        public int TargetCount;

        public Enum_AttackRangeType AttackRangeType;
        public float AttackAngle;

        public float Cooltime = 1.0f;
        public float NextPlayTime = 0.0f;

        [HideInInspector]
        public string CustomParam = string.Empty;
        public string CustomAni = string.Empty;
        public float AttackDuration = 0.5f;

        public List<int> EnemyConditionDatas = new List<int>();
        public List<int> MyConditionDatas = new List<int>();
    }

    public struct AttackStruct
    {
        public CharacterControllerBase Hitter;
        public int AttackLevel;
        public Chart.SkillInfo SkillInfo;
    }

    public delegate void CallMonsterHitState(double currDamage, double currHp, double totalHp);
    public delegate void CallSendDamage(double damage);

    public delegate void OnCallBack();
    public delegate void OnCallBack_Int(int intcallback);
    public delegate void OnCallBack_String(string text);
    public delegate void OnCallBack_Double(double doublecallback);

    // 스탯
    public enum Enum_Stat
    {
        Attack = 11,

        HP,
        Defence,

        MoveSpeed,
        AttackSpeed,

        Attack_Inc,
        Hp_Inc,
        Defence_Inc,
        MoveSpeed_Inc,
        AttackSpeed_Inc,

        CritChance,
        CritDmg_Inc,

        Evasion, // 회피
        Accuracy, // 명중

        HpRecovery,

        MinDamagePer,
        MaxDamagePer,

        FinalDamage,

        Max,
    }

    public enum Enum_StatMode
    {
        Int,
        Double,
    }

    /// <summary>
    /// 장비/코디용 슬롯 타입
    /// </summary>
    public enum Enum_SkinSlotType
    {
        Body = 0,
        Hair,
        Weapon,
        Face,
        Back,

        Max,
    }

    public enum Enum_IntervalType
    {
        None = -1,

        Quarter = 11, // 15분		
        Hour = 12, // 1시간		
        Day = 13, // 1일		
        Week = 14, // 1주일		
        Month = 15, // 1달		
        Account = 16, // 계정당

        Custom = 17, // 커스텀 (이벤트 등 버전 관리 목적용)
    }

    // 군중제어
    public enum Enum_ConditionType
    {
        None = 0,
        Invincible = 11, // 무적

        Stun = 14, // 스턴(이동, 평타, 스킬 모두 불능)
        Snare = 15, // 속박 (이동 불능)
        Slow = 16, // 둔화 (이속 공속 감소)

        Knockback = 17, // 넉백
        Fling = 18, // 당기기
        Dash = 19, // 대쉬

        // --- Buffs ---
        AttackUp = 101,      // 공격력 증가
        HpUp = 102,          // 체력 증가
        DefenseUp = 103,     // 방어력 증가
        MoveSpeedUp = 104,   // 이속 증가
        AttackSpeedUp = 105, // 공속 증가

        // --- ContentBuffs ---
        ComboBuff_AttackSpeedUp = 201, // 공속 버프
        ComboBuff_AttackUp = 202, // 공격력 버프
        ComboBuff_CriticalChangeUp = 203, // 크리티컬찬스 버프

        Max,
    }

    public enum ConditionCategory
    {
        None,
        Buff,
        Debuff,
        CrowdControl, // Stun, Snare, Slow, Knockback 등
        Utility       // Invincible, Pull, Push 등
    }

    public enum ConditionStackPolicy
    {
        MultipleInstances,   // 공속/이속/공격력 버프처럼 여러 개 각각 타이머
        Refresh,     // 스턴처럼 하나만 존재, 듀레이션만 최신으로 갱신
        MergyValue,     // 넉백처럼 값이 누적
    }

    public enum NoneTargetProjectileState
    {
        None = 0,
        Shoot,
        Hit,
    }

    public enum Enum_ShopMenuType
    {
        LimitPackage = 11, // 	기간 한정 패키지
        Daily = 12, // 	일간 패키지
        Weekly = 13, // 	주간 패키지
        Monthly = 14, // 	월간 패키지
        Descend = 15, // 	강림 강화 재화
        DayofWeek = 16, // 	요일마다 달라지는 상점
        DiaCharging = 17, // 	다이아 구매
        GoldCharging = 18, // 	골드 구매
        Research = 19, // 연구 상점
        Synergy = 20, // 속성 상점

        Max,
    }

    public enum V2Enum_QuestType
    { 
        Daily = 11,
        Weekly = 12,
        Monthly = 13,
        Achievement = 14,

        Max,
    }

    public enum V2Enum_PassType
    {
        Wave = 11, //웨이브 패스
        CharacterLevel = 12, //캐릭터 레벨
        SkillLevel = 13, //스킬 레벨업 (누적)
        DescendLevel = 14, //강림 레벨
        MonsterKill = 15, //몬스터 처치

        Max,
    }

    public enum V2Enum_RankType
    {
        Stage = 11, //스테이지
        Power = 12, //전투력

        GuildRaid = 101,
        GuildDona = 102,

        Max,
    }

    public enum ContentDetailList
    { 
        None = 0,

        CharacterProfile,

        LobbySynergy = 100,
        LobbySynergy_AllEnhance = 101,
        LobbySynergy_Red = 110,
        LobbySynergy_Yellow = 120,
        LobbySynergy_Blue = 130,
        LobbySynergy_White = 140,

        LobbyDescend = 200,

        LobbyRelic = 300,

        TimeAttackMission = 400,

        LobbySynergyRune = 500,
        LobbySynergyRune_Slot = 510,
        LobbySynergyRune_Combine = 520,

        LobbyGear = 600,
        LobbyGear_Slot = 610,
        LobbyGear_Combine = 620,

        LobbyCharacterJob = 700,
        LobbyCharacterJob_Upgrade = 710,
        LobbyCharacterJob_LevelUp = 710,

        CharacterSkin = 1600,
        CharacterSkin_Weapon = 1610,
        CharacterSkin_Body = 1620,

        LobbyResearch = 2000,
        LobbyResearch_Shop = 2010,
        LobbyResearch_Charge = 2020,

        Dungeon = 3000,
        DungeonDiamond,
        DungeonTower,


        Guild = 4500,

        Shop = 5000,

        ShopGeneral = 5100,
        ShopGeneralPackage = 5110,
        ShopSummon_Normal = 5120,
        ShopSummon_Relic,
        ShopSummon_Rune,
        ShopSummon_Gear,
        ShopInGameStore = 5130,
        ShopInGameStore_Descend,
        ShopInGameStore_Gold,
        ShopInGameStore_Synergy,
        ShopDiamondStore = 5140,

        ShopDescend = 5200,
        ShopDescendStore = 5210,

        ShopPackage = 5300,
        ShopRandomStore = 5110,
        ShopDailyWeek_DiaPackage = 5310,
        ShopDailyWeek_WeekPackage = 5320,
        ShopDailyWeek_DayPackage = 5330,
        ShopDailyWeek_MonthPackage = 5340,

        ShopDailyWeek,

        ShopRelayPackage = 5390,

        ShopVip = 5400,
        ShopVipStore_AD,
        ShopVipStore_Dia,


        ShopCharge = 5500,
        ShopCharge_Gold = 5510,
        ShopCharge_Dia = 5520,

        Post = 6000,
        PostGeneral,
        PostShop,

        Pass = 7000,
        PassWave, //웨이브 패스
        PassCharacterLevel, //캐릭터 레벨
        PassSkillLevel, //스킬 레벨업 (누적)
        PassDescendLevel, //강림 레벨
        PassMonsterKill, //몬스터 처치

        AdBuff = 8000,

        CheckIn = 10000,

        GameOption = 11000,

        Quest = 12000,
        Quest_Daily = 12100,
        Quest_Weekly = 12200,
        Quest_Monthly = 12300,
        Quest_Achievement = 12400,

        Exchange = 13000,

        Rank = 14000,
        Rank_Stage = 14100,
        Rank_CombatPower = 14200,




        StageMap = 17000,

        Inventory = 19000,
        Inventory_Item = 19100,
        Inventory_Point = 19200,

        EventDig_Shop = 25200,


        Notice = 99000,
    }

    public enum V2Enum_ReportType
    {
        inappropriate = 0, //비속어 및 성적 발언
        conflict, //갈등 조장 및 허위 사실 유포
        spam, //채팅창 도배 및 광고

        Max,
    }

    public static class Define
    {
        public static readonly float DefaultScreenWidth = 1080.0f;
        public static readonly float DefaultScreenHeight = 1920.0f;
        public static readonly float DefaultInGameCameraSize = 4.88f;

        //카메라 관련
        public static readonly float DefaultScreenInGameWidth = 17.7777f;

        // 최초 로그인인가
        public static readonly string FirstLoginKey = "FirstLogin";

        public static readonly int NickNameMinCount = 2;
        public static readonly int NickNameMaxCount = 8;

        //PlayerPrefs
        public static readonly string LoginTypeKey = "loginKey";

        public static Enum_IntervalType ExchangeInterval = Enum_IntervalType.Day;

        public static double NickNameChangeDiaCost = 500.0;

        //시스템챗 이름
        public static readonly string SystemChatName = "System";

        public const string DayLocalKey = "time/day";
        public const string HourLocalKey = "time/hour";
        public const string MinuteLocalKey = "time/minute";
        public const string SecondLocalKey = "time/second";

        // 셋팅 관련

        public static readonly string SoundBGOnKey = "SoundBGOnKey";
        public static bool SoundBGOn = true;

        public static readonly string SoundFXOnKey = "SoundFXOnKey";
        public static bool SoundBGOff = true;

        public static readonly string VisibleDamageFontKey = "VisibleDamageFontKey";
        public static bool VisibleDamageFont = true;

        public static readonly string LowSpecModeKey = "LowSpecModeKey";
        public static bool LowSpecMode = true;

        public static readonly string DeviceLocalizeKey = "DeviceLocalizeKey";
        public static readonly string ChatLocalizeKey = "ChatLocalizeKey";
        public static readonly string ChatBenUserKey = "ChatBenUserKey";

        public static readonly int ChatBenMaxCount = 20;

        // 셋팅 관련

        public static readonly string RedDotSaveKey = "RedDot_";

        public static ObscuredBool IsAdFree = false;
        public static ObscuredBool IsSpeedUpMode = false;
        public static ObscuredInt InCreaseAdBuffCount = 0;
        public static ObscuredBool OpenResearchSlot = false;
        public static ObscuredBool IsAdBuffAlways = false;
        public static ObscuredBool IsSweepUnlimited = false;


        //DefineChart에서 파싱해서 가져오기
        public static int StarforceRestoration1_Key = 1001;
        public static long StarforceRestoration1_Price = 5000;
        public static int StarforceRestoration2_Key = 1002;
        public static long StarforceRestoration2_Price = 1000000;

        public static int WeaponAwakeAddLevel = 20;
        public static int WeaponCombineCount = 5;
        public static int WeaponBaseMaxLevel = 100;
        public static int WeaponLevelUpCostKey = 1003;
        public static int SummonAdDrawCount = 10;
    }
}
