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

    public enum V2Enum_Grade
    {
        D = 11,
        C = 12,
        B = 13,
        A = 14,
        S = 15,
        SS = 16,
        SR = 17,
        SU = 18,
        SSR = 19,

        Max,
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

        public Enum_AttackRangeType TargetAttackType;

        public float Cooltime = 1.0f;
        public float NextPlayTime = 0.0f;
    }

    public delegate void CallMonsterHitState(double currDamage, double currHp, double totalHp);
    public delegate void CallSendDamage(double damage);
    public delegate void RefreshOpenCondition(V2Enum_OpenConditionType v2Enum_OpenConditionType, int conditionValue);

    public delegate void OnCallBack();
    public delegate void OnCallBack_Int(int intcallback);
    public delegate void OnCallBack_String(string text);
    public delegate void OnCallBack_Double(double doublecallback);


    // 스탯
    public enum V2Enum_Stat
    {
        Attack = 11,

        HP = 12,
        Defence = 13,

        MoveSpeed = 14,
        AttackSpeed = 15,

        CritChance,

        VampiricRate,
        ResistanceStat,

        ResistancePenetration,
        CritDmgIncrease,
        DmgBoost,

        Evasion,
        Accuracy,

        HpRecovery,

        Max,
    }








    // 재화
    public enum V2Enum_Goods
    {
        Gear = 11,

        Point = 20,
        CharacterSkill = 21,
        Ally = 22,
        TimePoint = 23,
        SummonTicket = 24,
        Box = 25,
        Skin = 26,
        Synergy = 27,
        Descend = 28,
        Relic = 29,
        VipPackage = 30,
        SynergyRune = 31,
        SynergyBreak = 32,
        DescendBreak = 33,

        Max,
    }

    public enum V2Enum_GearType
    {
        Weapon = 11,
        Helmet = 12,
        Armor = 13,
        Shoes = 14,
        Ring = 15,
        Necklace = 16,

        Max,
    }

    public enum V2Enum_BoxType
    {
        PackageTypeBox = 11,
        RandomTypeBox = 12,

        Max,
    }

    public enum V2Enum_Skin
    {
        SkinWeapon = 11,
        SkinBody = 12,
        SkinAura = 13,

        Max,
    }

    // 표기
    public enum V2Enum_PrintType : int
    {
        Value = 0,
        Percent = 1,

        Max,
    }

    // 개방조건
    public enum V2Enum_OpenConditionType
    {
        None = -1,

        Stage = 11, //스테이지 진행
        CharacterLevel = 12, //캐릭터 레벨

        StackLogin = 13, //누적 접속일 수
        StackSkillCount = 14, //보유 시너지 스킬 개수
        GetGear = 15, //특정 등급의 장비 획득
        BreakthroughCount = 16, //캐릭터 돌파 횟수
        Wave = 17, //특정 웨이브 클리어 / TotalNumber 파싱

        StaminaStack = 18, //행동력 일정 개수 이하 일 때
        StaminaCount = 19, //누적 행동력 소모량
        WaveDeath = 20, //"특정 웨이브 이상에서 사망 시 다음 스테이지로 넘어가는 경우 미적용"
        RelicSummonCount = 21, //누적 유물 뽑기 횟수
        SummonCount = 22, //누적 일반 뽑기 횟수
        GearSummonCount = 23, //누적 장비 뽑기 횟수
        GetSynergySkill = 24, //특정 시너지 스킬 획득
        GetDescendSkill = 25, //특정 강림 스킬 획득

        KillMonster = 26, //몬스터 처치 수 (이벤트 발생 이후)

        SynergySkillLevelStack = 27, //누적 시너지 강화 레벨
        DescendSkillLevelStack = 28, //누적 강림 레벨

        NoAdBuff = 29, //N일간 버프제거 패키지를 구매하지 않은 경우

        RelicLevelStack = 30, //누적 유물 레벨

        WaveReward = 31, //웨이브 리워드

        RuneSummonCount = 32, //누적 룬 뽑기 횟수
        RuneCombineCount = 33, //누적 룬 합성 횟수
        FireSynergyLevelStack = 34, //  불 속성 스킬 누적 레벨  
        GoldSynergyLevelStack = 35, //  금 속성 스킬 누적 레벨  
        WaterSynergyLevelStack = 36, //  물 속성 스킬 누적 레벨  
        ThunderSynergyLevelStack = 37, //  번개 속성 스킬 누적 레벨  

        FireSynergyBreakStack = 38, //  불 속성 진화 누적 레벨  
        GoldSynergyBreakStack = 39, //  금 속성 진화 누적 레벨  
        WaterSynergyBreakStack = 40, //  물 속성 진화 누적 레벨  
        ThunderSynergyBreakStack = 41, //  번개 속성 진화 누적 레벨  

        Synergy4GradeSkillCount = 42, // 4단계 시너지 스킬 해금 갯수

        DungeonDiffClear = 43, // 4단계 시너지 스킬 해금 갯수

        Max,
    }

    public enum V2Enum_ContentType
    {
        AdBuff = 11, // 광고버프
        Pass = 12, // 패스
        MailBox = 13, // 우편함
        Quest = 14, // 퀘스트
        Synergy = 15, // 시너지
        Descend = 16, // 강림
        Relic = 17, // 유물
        Shop = 18, // 상점
        RelicSummon = 19, // 유물 소환
        DescendPass = 20, // 강림 패스
        HuntingPass = 21, // 사냥 패스
        RuneSummon = 22, // 룬 소환
        Rune = 23, // 룬
        Research = 24, // 연구소

        DescendShop = 25, // 강림 상점
        SynergyBreakthrough = 26, // 속성 스킬 돌파
        Gear = 27, // 장비
        GearSummon = 28, // 장비 소환

        UnlockGoldSynergy = 29, // 금 속성 해금
        UnlockThunderSynergy = 30, // 번개 속성 해금

        Sweep = 31, // 소탕 해금
        Ranking = 32, // 랭킹 해금

        Dungeon = 33, // 던전
        DiamondDungeon = 34, // 다이아던전

        Job = 35, // 전직

        AutoPlay = 36, // 오토

        Max,
    }

    public enum V2Enum_IntervalType
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
    public enum V2Enum_SkillEffectType
    {
        None = 0,
        Invincible = 11, // 무적      있던거
        Cleansing = 12, // 상태이상 해제      있던거
        HOT = 13, // 지속 힐      있던거
        IncreaseAtt = 14, // 공격력 증가 %
        IncreaseArmor = 15, // 방어력 증가 %



        Stun = 1001, // 스턴(이동, 평타, 스킬 모두 불능)      있던거
        Silence = 1002, // 침묵 (스킬 불능)      있던거
        Blind = 1003, // 실명 (평타 불능)      있던거
        Snare = 1004, // 속박 (이동 불능)      있던거
        Slow = 1005, // 둔화 (이속 공속 감소)      있던거
        DOT = 1006, // 지속 피해      있던거

        BurnDOT = 1007, // 화상 지속 피해 (공격력 비례) SkillEffect가 종속된 Damage 값의 일정 %를 0.5초마다 입힌다.



        Knockback = 2001, // 넉백      있던거
        Fling = 2002, // 당기기      있던거
        AdditionalDmg = 2003, // 추가데미지 %

        // 완료
        Heal = 2007, //공격력에 비례한 HP 즉시 회복
        DotHeal = 2008, //공격력에 비례한 HP를 0.5초마다 회복 (Duration 만큼 지속)
        Death = 2009, //일정 HP 이하 적 즉사(Value로 % 수치 제어)
        // 완료

        Max,
    }

    public enum V2Enum_Point
    {
        InGameGold = 199010001,
        LobbyGold = 199010002,

        Dia = 199010003,

        InGameGas = 199010004,

        Stamina = 199010005,

        SynergyEnhance = 199010006,

        SynergyLimitFire = 199010007,
        SynergyLimitGold = 199010008,
        SynergyLimitWater = 199010009,
        SynergyLimitThunder = 199010010,

        ResearchAccel = 199010011,

        DescendEnhance = 199010012,
        DescendLimit = 199010013,

        WeaponEnhance = 199010014,
        HelmetEnhance = 199010015,
        ArmorEnhance = 199010016,
        ShoesEnhance = 199010017,
        AccessoryEnhance = 199010018,


        RelicSummonTicket = 199010019,
        EquipSummonTicket = 199010020,
        GeneralSummonTicket = 199010021,
        SynergySummonTicket = 199010022,

        ResearchTicket = 199010023,
        ResearchSlotTwo = 199010024,

        InGameDescendEnforce = 199010025,

        BuyDescend = 199010026,
        JobEnhance = 199010027,
        DiaDungeonTicket = 199010028,
        SkillSummonTicket = 199010029,
        AllySummonTicket = 199010030,
        JewelrySummonTicket = 199010031,

        LuckyCoin = 199010032,

        EventDungeonTicket = 199010033,
        EventDungeonHeart = 199010033,

        ClanMissionCompass = 199010034,
        ClanMissionRefreshTicket = 199010035,

        HellDungeonTicket = 199010036,

        HellMonster_Helmet = 199010037,
        HellMonster_Heart = 199010038,
        HellMonster_Sword = 199010039,
        HellMonster_Arm = 199010040,
        HellMonster_Reg = 199010041,
        HellMonster_Tail = 199010042,

        HellFragments_Anger = 199010043,
        HellFragments_Madness = 199010044,
        HellFragments_Murderous = 199010045,
        HellFragments_Arrogance = 199010046,
        HellFragments_Obsession = 199010047,
        HellFragments_Hatred = 199010048,

        HellStone_Awake = 199010049,

        AllyArenaTicket = 199010050,
        AllyArenaHonorCoin = 199010051,

        AllyJewelrySurpassReset = 199010052,

        GoddessPick = 199010053,

        RedBullPick = 199010054,

        SkinLevelUP = 199010055,

        RotationEventTicket = 199010056,

        UrsulaPick = 199010057,

        Shovel = 199010058,

        ForgeHammer = 199010059,
        ForgeShield = 199010060,

        MathRpgCoin = 199010061,

        KingSlimePick = 199010062,

        GuildRaidTicket = 199010063,
        GuildCoin = 199010064,
        GuildMileage = 199010065,
        GuildHammer = 199010066,

        Max,
    }

    public enum V2Enum_SummonType
    {
        SummonGear = 11,
        SummonNormal = 12,
        SummonRelic = 13,
        SummonRune = 14,

        Max
    }

    public enum V2Enum_ActorEmotion
    {
        None = 11,
        StandStill = 12, // 서있는상태
        Exhausted = 13, // 지친상태
        FallDown = 14, // 쓰러진상태

        Max,
    }

    public enum V2Enum_CheckInType
    {
        Once = 11,
        Repeat = 12,

        Max,
    }

    public enum CharacterSlotState
    {
        None = 0,
        OpenSlot,
        AddSlot,
        LockSlot,
    }

    public enum NoneTargetProjectileState
    {
        None = 0,
        Shoot,
        Hit,
    }

    public enum V2Enum_ShopMenuType
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

    public enum V2Enum_DisplayType
    {
        Static = 11, // 고정상품
        Dynamic = 12, // 돌발상품

        Max,
    }

    public enum V2Enum_DefineType
    {
        PenetrationFactorMin = 11, // 관통계수 최소값
        ResistanceFactorMin = 12, // 저항계수 최소값

        InterestTimer = 13, // 1회 이자를 받기 위해 필요한 시간
        InterestMaxReward = 14, // 1회 이자 최대치 (이자로 획득하는 골드 기준)
        InterestRate = 15, // 이자 1골드를 얻기 위한 보유 골드량

        GambleMinimumRate = 16, // 짝맞추기 겜블 최소 확률 보정치
        GambleReinforcementMaxLevel = 17, // 겜블 내부 확률 보정 강화 최대치

        DefaultSkin = 18, // 상시로 켜지는 코스튬 경로
        DefenseStandard = 19, // 방어력 상수 기준값
        MaximumDecreaseAtt = 20, // 공격력 최대 감소 가능 %
        MaximumDecreaseArmor = 21, // 방어력 최대 감소 가능 %

        DefaultGainGold = 22, // 스테이지를 클리어하지 않았을 때 기본으로 제공되는 보상


        NexusMonsterIndex = 27, // 넥서스용 몬스터 인덱스
        NewCharacterSkill = 28, // 신규 캐릭터 생성 시 지급되는 캐릭터 스킬 인덱스
        NewCharacterEquipSkill = 29, // 신규 캐릭터 생성 시 자동 장착되는 스킬 인덱스
        StageStartGold = 30, // 스테이지 시작 시 지급되는 골드

        StaminaChargeTime = 31, // 행동력 1 충전에 필요한 시간 (초)
        MaxStamina = 32, // 행동력 맥스 수치
        RequiredStamina = 33, // 1회 입장 시 소모하는 행동력 수치
        StaminaIndex = 34, // 1회 입장 시 소모하는 행동력 수치

        JokerCardProb = 35, // 조커 카드 등장 확률
        JokerCardCount = 36, // 조커 카드 획득 시 얻는 시너지 게이지 스택

        LimitDailyAdStamina = 37, // 하루에 최대 광고로 회복시킬 수 있는 행동력 횟수
        StaminaChargeCount = 38, // 행동력 회복 시 몇 개의 행동력이 회복되는지
        StaminaPrice = 39, // 행동력 구매 시 최초 몇 개의 다이아가 필요한지

        GasSynergyProb = 40, // 가스 시너지 도박 시 성공률
        GasSynergyCount = 41, // 가스 시너지 도박 성공 시 얻는 시너지 게이지 스택
        GasHpRecovery = 42, // 가스 소모하여 회복되는 HP % (전체 HP%)
        GasIndex = 43, // 가스 재화 인덱스

        RandomShopAdRefreshCount = 44, //랜덤 상점 AD 초기화 최대 횟수
        RandomShopRefreshCount = 45, //랜덤 상점 다이아 초기화 최대 횟수
        RandomShopRefreshIndex = 46, //랜덤 상점 초기화 필요 재화 인덱스
        RandomShopRefreshValue = 47, //랜덤 상점 초기화 필요 재화 개수
        RandomShopSlot = 48, //랜덤 상점에 노출되는 아이템 개수

        RandomShopFreeDia = 49, //랜덤 상점 첫번째 슬롯에 상시 노출되는 다이아 개수
        RandomShopFreeDiaAD = 50, //광고 보고 추가로 지급받을 수 있는 다이아 횟수

        BuffAdDailyCount = 51, //하루에 광고 버프를 활성화 할 수 있는 횟수 제한
        BaseSellingItemOnce = 52, //계정 생성 후 최초 1회 일일 상점 고정 

        Research2SlotOpenCost = 53, // 연구 2슬롯 해금 PID
        Research3SlotOpenCost = 54, // 연구 3슬롯 해금 필요 다이아 개수

        ResearchGoodsEarn = 55, // 연구 1회 회복당 획득 개수
        ResearchChargingTime = 56, // 연구 1회 회복에 필요한 시간 (초)

        ResearchGoodsLimitCount = 57, // 연구 재료 최대 소지량
        ResearchGoodsIndex = 58, // 연구 재료 인덱스

        ResearchADTime = 59, // 연구 광고 시청 시 감소하는 시간 (초)
        ResearchADTimeCount = 60, // 연구 광고 시청 가능 횟수

        ResearchAccelTicketIndex = 61, // 가속 티켓 인덱스
        ResearchAccelTime = 62, // 가속 티켓 1개당 감소하는 연구 시간 (초)

        SkillEvolutionUnlock = 63, // 스킬 진화 해금 조건

        SynergyMAXGoods = 64, // 속성 MAX 이후 지급되는 재화 인덱스

        DailySweepCount = 65, // 하루 광고보고 소탕 할 수 있는 제한 횟수
        DailyDoubleRewardCount = 66, // 하루 광고보고 소탕 할 수 있는 제한 횟수

        Max,
    }

    public enum V2Enum_EventType
    {
        SkillSummon = 22, // 스킬 소환
        SkillEquip = 23, // 스킬 장착
        SkillLevelUp = 24, // 스킬 강화

        AllySummon = 25, // 동료 소환
        AllyEquip = 26, // 동료 장착
        AllyLevelUp = 27, // 동료 강화
        AllyCompose = 28, // 동료 합성

        JewelrySummon = 29, // 보석 소환
        JewelryEquip = 30, // 보석 장착
        JewelryCompose = 31, // 보석 합성

        FameLevelUp = 32, // 명성 달성

        ResearchLevelUp = 33, // 일반 마스터리 강화

        PassFreeRewardClaim = 40, // 패스 무료보상 획득

        BuffAddView = 41, // 버프광고 시청

        MonsterKill = 42, // 몬스터 처치

        StageClear = 43, // 스테이지 돌파

        DiamondDungeonClear = 44, // 다이아던전 클리어
        MasteryDungeonClear = 45, // 마스터리던전 클리어
        GoldDungeonClear = 46, // 골드던전 클리어
        SoulStoneDungeonClear = 47, // 영혼석던전 클리어
        RuneDungeonClear = 48, // 룬던전 클리어


        CheckInRewardGet = 58, //출석보상 획득
        DailyMissionRewardGet = 59, //일일미션 보상획득
        FreePurchase = 60, //무료상품 구매

        BossChallenge = 63, //도전하기
        MailGet = 64, //우편받기


        CheckExchange = 74, //거래소 확인
        AllyStarUp = 75, //동료 승급

        TutoGambleCard = 77, // 겜블 카드 가이드

        TutoGambleSynergy = 80, // 겜블 Synergy

        WaveReward = 81, // 웨이브 리워드

        SynergyCombine = 82, // SynergyCombine

        SynergyChange = 83, // SynergyChange

        DescendChange = 84, // DescendChange

        SpeedUp = 85, // SpeedUp

        NextStage = 86, // NextStage

        GasSynergy = 87, // NextStage

        RelicTutorial = 88, // Relic

        ResearchTutorial = 89, // Relic

        RuneTutorial = 90, // Relic

        SynergyBreak = 91, // Break

        GearTutorial = 92, // Break

        TutoGambleGas = 93, // 1스테이지 가스 튜토리얼
        TutoGambleGasSynergyPick = 94, // 1스테이지 가스 강제 튜토리얼
        TutoGambleGasSynergy = 95, // 1스테이지 가스 시너지

        GearEquipTutorial = 96, // 

        InGameDescendUpGradeTutorial = 97, // 

        SynergyOpen = 98, // SynergyChange

        SynergyInteraction = 99, // 첫 스테이지 시너지 렙업 도우미

        SynergyUnLock = 100, // SynergyChange

        Job = 101, // SynergyChange

        Dungeon = 102, // SynergyChange

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

    public enum V2Enum_QuestGoalType
    {
        MonterKillCount = 11, // 몬스터 처치 수
        StageChallenge = 12, // 스테이지 진행 횟수
        WatchingAd = 13, // 광고 시청 횟수
        CardGambleCount = 14, // 카드 뽑기 시행 횟수
        CharacterLevel = 15, // 캐릭터 레벨 달성

        SynergyChange = 16, // 시너지 교체 진행
        SynergyCombineClear = 17, // 시너지 조합 미션 달성 횟수

        DailyMissionClearCount = 18, // 일일 미션 클리어 횟수
        WeeklyMissionClearCount = 19, // 주간 미션 클리어 횟수

        LoginOnce = 20, // 로그인 하기
        LoginTime = 21, // 접속 유지 시간 (분)

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


    public enum V2Enum_DungeonDifficultyType
    {
        Normal = 11, //일반
        Advanced = 12, //지옥
        Expert = 13, //악몽
        Master = 14, //저주
        GrandMaster = 15, //심연
        Challenger = 16, //낙인

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

    public class RewardData
    {
        [System.NonSerialized]
        public bool IsPoolData = false;

        public V2Enum_Goods V2Enum_Goods = V2Enum_Goods.Max;
        public ObscuredInt Index;
        public ObscuredDouble Amount;

        public ObscuredInt DupliIndex = -1;
        public ObscuredDouble DupliAmount;
    }

    public static class Define
    {
        public static double CriticalDamageValue = 2.0f;

        public const int DisplayMapDataCount = 5;

        public const int DisplayGambleSkillCount = 2;


        public const string BoxIconPath = "Icon/box/{0}";
        public const string PetIconPath = "Icon/pet/{0}";
        public const string PointIconPath = "Icon/point/{0}";
        public const string SkillIconPath = "Icon/skill/{0}";
        public const string SynergyBreakPath = "Icon/synergybreak/{0}";
        public const string SynergyRunePath = "Icon/synergyrune/{0}";
        public const string StatIconPath = "Icon/stat/{0}";
        public const string DescendIconPath = "Icon/descend/{0}";
        public const string RelicIconPath = "Icon/relic/{0}";
        public const string AdBuffIconPath = "Icon/adbuff/{0}";
        public const string VipPackageIconPath = "Icon/vippackage/{0}";
        public const string ResearchPath = "Icon/research/{0}";
        public const string GearPath = "Icon/gear/{0}";









        public static readonly float DefaultScreenWidth = 1080.0f;
        public static readonly float DefaultScreenHeight = 1920.0f;
        public static readonly float DefaultInGameCameraSize = 4.88f;

        //카메라 관련
        public static readonly float DefaultScreenInGameWidth = 17.7777f;

        //아이템 드랍 관련
        public static readonly string GoldDungeonCustomKey = "GoldDungeonCustom";
        public static readonly string SoulDungeonCustomKey = "SoulDungeonCustom";

        // 최초 로그인인가
        public static readonly string FirstLoginKey = "FirstLogin";

        public static readonly int NickNameMinCount = 2;
        public static readonly int NickNameMaxCount = 8;

        //참조 스트링
        public static readonly string WeaponAniResourceKey = "DarkKnight_Weapon";
        public static readonly string WeaponResourceNameKey = "DarkKnight_Weapon_{0}";

        public static readonly string PhantomWeaponNameKey = "DarkKnight_Weapon_Costume_Phantom_00";

        //CharacterAniSampleSetting
        public static readonly string PlayerSpriteResourceName = "DarkKnight";
        public static readonly int PlayerSpriteVariationNumber = 3;

        public static readonly string KnightNameLocalizeKey = "characterDialogueName";




        //PlayerPrefs
        public static readonly string LoginTypeKey = "loginKey";
        public static readonly string EquipPetKey = "equipskillKey";
        public static readonly string NewSynergyKey = "newsynergyKey";
        public static readonly string NewSynergyBreakKey = "newsynergybreakKey";
        public static readonly string NewSynergyRuneKey = "newsynergyruneKey";
        public static readonly string NewDescendKey = "newdescendKey";
        public static readonly string NewRelicKey = "newrelicKey";
        public static readonly string NewGearKey = "newgearKey";
        public static readonly string AutoGambleKey = "autogambleKey";
        public static readonly string AutoMaxStopKey = "automaxstopKey";

        //CharacterInfoTableKey
        public const string PlayerInfoTable = "PlayerInfo";
        public const string PlayerAdFree = "PlayerAdFree";
        public const string PlayerCheatingCheck = "PlayerCheatingCheck";
        public const string PlayerDontSearchCheat = "PlayerDontSearchCheat";
        public const string PlayerProfile = "PlayerProfile";
        public const string PlayerRecvLaunchReward = "PlayerRecvLaunchReward";
        public const string PlayerRecvPreReward = "PlayerRecvPreReward";

        //CharacterInfoTableKey
        public const string PlayerARRRInfoTable = "ARRRInfo";
        public const string PlayerARRRLevel = "ARRRLevel";
        public const string PlayerARRRLimitCompleteLevel = "ARRRLimitCompleteLevel";
        public const string PlayerARRRDefaultGoods = "DefaultGoods";

        //StaminaInfoTableKey
        public const string PlayerStaminaInfoTable = "StaminaInfo";
        public const string PlayerStaminaInitTime = "InitTime";
        public const string PlayerStaminaAccumUse = "AccumUse";
        public const string PlayerStaminaLastChargeTime = "LastChargeTime";
        public const string PlayerToDayDigAdCount = "ToDayDigAdCount";
        public const string PlayerToDayDigDiaBuyCount = "ToDayDigDiaBuyCount";

        //StaminaInfoTableKey
        public const string PlayerJobInfoTable = "JobInfo";
        public const string PlayerJobInfo = "Job";
        public const string PlayerJobType = "JobType";
        public const string PlayerJobTier = "JobTier";

        //StaminaInfoTableKey
        public const string PlayerVipPackageInfoTable = "VipPackageInfo";
        public const string PlayerVipPackageInfo = "VipPackage";
        public const string PlayerVipPackageShopInfo = "VipPackageShop";


        //SynergyInfoTableKey
        public const string PlayerSynergyInfoTable = "SynergyInfo";
        public const string PlayerSynergyAccumLevel = "AccumLevel";
        public const string PlayerSynergyInfo = "Synergy";
        public const string PlayerSynergyEquipInfo = "SynergyEquip";
        public const string PlayerSynergyExp = "Exp";
        public const string PlayerSynergyRune = "Rune";

        //DescendTableKey
        public const string PlayerDescendInfoTable = "DescendInfo";
        public const string PlayerDescendAccumLevel = "AccumLevel";
        public const string PlayerDescendInfo = "Descend";
        public const string PlayerDescendEquipInfo = "DescendEquip";
        public const string PlayerDescendExp = "Exp";


        //SynergyRuneInfoTableKey
        public const string PlayerSynergyRuneInfoTable = "SynergyRuneInfo";
        public const string PlayerSynergyRuneInfo = "Rune";
        public const string PlayerSynergyRuneEquipInfo = "RuneEquip";
        public const string PlayerSynergyRuneAccumCombine = "CombineCount";



        //RelicTableKey
        public const string PlayerRelicInfoTable = "RelicInfo";
        public const string PlayerRelicInfo = "Relic";

        //PlayerPointTable
        public const string PlayerPointTable = "PointInfo";
        public const string PlayerDiaAmount = "PlayerDiaAmount";
        public const string PlayerAccumUseDia = "PlayerAccumUseDia";

        //PlayerSummonTicketTable
        public const string PlayerSummonTicketTable = "PlayerSummonTicket";

        //PlayerBoxTable
        public const string PlayerBoxTable = "PlayerBox";

        //PlayerTrainingTable
        public const string PlayerTrainingTable = "PlayerTraining";

        //PlayerGearInfo
        public const string PlayerGearTable = "GearInfo";
        public const string PlayerGearInfo = "Gear";
        public const string PlayerGearEquipInfo = "Equip";
        public const string PlayerGearSlotInfo = "Slot";
        public const string PlayerGearAccumCombine = "CombineCount";


        //PlayerSkinInfo
        public const string PlayerSkinTable = "PlayerSkinInfo";
        public const string PlayerSkinEquip = "PlayerSkinEquip";


        //CharacterSkillInfoKey
        public const string PlayerSkillInfoTable = "SkillInfo";
        public const string PlayerSkillInfo = "skillinfo";
        public const string PlayerSkillSlotInfo = "skillslotinfo";

        //MapInfo
        public const string PlayerMapInfoTable = "MapInfo";
        public const string PlayerMapMaxWaveClear = "MapMaxWaveClear";
        public const string PlayerMapMaxClear = "MapMaxClear";
        public const string PlayerMapMapStageInfo = "StageInfo";
        public const string PlayerMapKey = "MapKey";
        public const string PlayerEventDayInitTime = "InitTime";
        public const string PlayerToDaySweepCount = "SweepCount";
        public const string PlayerToDayDoubleRewardCount = "DoubleReward";

        //DungeonInfo
        public const string PlayerDungeonInfoTable = "DungeonInfo";

   
        public const string PlayerDiamondDungeonMaxClear = "DiamondDungeonMaxClear";
        public const string PlayerTowerDungeonMaxClear = "TowerDungeonMaxClear";

        public const string PlayerDungeonInitInfo = "DungeonInitInfo";

        //PlayerResearchInfo
        public const string PlayerResearchInfoTable = "ResearchInfo";
        public const string PlayerResearchInfo = "ResearchInfo";
        public const string PlayerResearchSlot = "Slot";
        public const string PlayerResearchAdCount = "AdCount";
        public const string PlayerResearchDailyInitTime = "InitTime";
        public const string PlayerResearchChargeCount = "chargecount";
        public const string PlayerResearchLastCharge = "lastchargetime";
        public const string PlayerResearchViewQueue = "viewqueue";

        //SummonInfo
        public const string PlayerSummonInfoTable = "SummonInfo";
        public const string PlayerSummonInfo = "SummonInfo";

        //TimeInfo
        public const string PlayerTimeInfoTable = "TimeInfo";
        public const string PlayerLastRecvStageCoolTimeReward = "LastRecvStageCoolTimeReward";
        public const string PlayerAccumLoginTime = "AccumLoginTime";
        public const string PlayerDailyInitTimeStamp = "DailyInitTimeStamp";
        public const string PlayerAccumLoginCount = "AccumLoginCount";

        //AdBuffInfo
        public const string PlayerAdBuffInfoTable = "AdBuffInfo";
        public const string PlayerAdBuffInfo = "AdBuff";
        public const string PlayerAdBuffInitTime = "InitTime";
        public const string PlayerAdBuffTodayActiveCount = "TodayCount";


        //CheckInInfo
        public const string PlayerCheckInInfoTable = "PlayerCheckinInfo";
        public const string PlayerCheckInRewardInfo = "PlayerCheckInRewardInfo";

        //ShopInfo
        public const string PlayerShopInfoTable = "ShopInfo";
        public const string PlayerShopPostInfo = "ShopPostInfo";

        //ShopRandomStoreInfo
        public const string PlayerShopRandomStoreInfoTable = "ShopRandomStoreInfo";
        public const string PlayerShopRandomStoreInitTime = "InitTime";

        public const string PlayerShopRandomStoreBuyInfo = "Buy";
        public const string PlayerShopRandomStoreDisplay = "Display";

        public const string PlayerShopRandomStoreResetAdView = "ResetAd";
        public const string PlayerShopRandomStoreResetDia = "ResetDia";
        public const string PlayerShopRandomStoreDiaFree = "DiaFree";

        //PassInfo
        public const string PlayerPassInfoTable = "PassInfo";
        public const string PlayerPassMonsterKillInfo = "MonsterKill";
        public const string PlayerPassInfo = "PassInfo";

        //QuestInfo
        public const string PlayerQuestInfoTable = "QuestInfo";
        public const string PlayerClearGuideQuestOrderInfo = "PlayerClearGuideQuestOrderInfo";
        public const string PlayerQuestInfo = "PlayerQuestInfo";
        public const string PlayerQuestGaugeInfo = "PlayerQuestGaugeInfo";
        public const string PlayerQuestDailyInit = "DailyInit";

        //QuestInfo
        public const string PlayerTimeAttackMissionInfoTable = "TimeAttackMissionInfo";
        public const string PlayerTimeAttackMissionInfo = "MissionInfo";
        public const string PlayerFocusMissionInfo = "Focus";


        //ExchangeInfo
        public const string PlayerExchangeInfoTable = "PlayerExchangeInfo";
        public const string PlayerExchangeInfo = "PlayerExchangeInfo";

        //RankInfo
        public const string PlayerRankInfoTable = "RankInfo";
        public const string PlayerRankCombatPower = "CombatPower";
        public const string PlayerStage = "Stage";
        public const string PlayerEvent = "Event";

        //ViewData
        public const string PlayerViewDataTable = "PlayerViewDataInfo";
        public const string PlayerViewDataBuyPrice = "Buy";

        //CharacterDefaultSetting
        public static readonly int PlayerDefaultSummonLevel = 1;

        public static readonly int PlayerSkillDefaultLevel = 0;
        public static readonly int PlayerSynergyDefaultLevel = 1;
        public static readonly int PlayerDescendDefaultLevel = 1;
        public static readonly int PlayerJewelryDefaultLevel = 0;
        public static readonly int PlayerJobDefaultLevel = 1;

        public static readonly int CreatureDefaultLevel = 1;

        public static double StageCoolTimeRewardTimeGab = 60.0;
        public static double StageCooltimeRewardMaxSecond = 7200;


        // DefineTable
        public static double PenetrationFactorMin = 0.2;
        public static double ResistanceFactorMin = 0.2;

        public static float InterestTimer = 10.0f;
        public static double InterestMaxReward = 50.0;
        public static double InterestRate = 10.0;

        public static double GambleMinimumRate = 0.0;
        public static double GambleReinforcementMaxLevel = 10.0;

        public static string DefaultSkin = "Body/00"; // 상시로 켜지는 코스튬 경로
        public static double DefenseStandard = 100; // 방어력 상수 기준값
        public static double MaximumDecreaseAtt = -70; // 공격력 최대 감소 가능 %
        public static double MaximumDecreaseArmor = -80; // 방어력 최대 감소 가능 %

        public static double DefaultGainGold = 100; // 스테이지를 클리어하지 않았을 때 기본으로 제공되는 보상

        public static int NexusMonsterIndex = 100; // 넥서스용 몬스터 인덱스
        public static double StageStartGold = 30; // 스테이지 시작 시 지급되는 골드

        public static double StaminaChargeTime = 60; // 행동력 1 충전에 필요한 시간 (초)
        public static int MaxStamina = 30; // 행동력 맥스 수치
        public static int RequiredStamina = 5; // 1회 입장 시 소모하는 행동력 수치
        public static int StaminaIndex = 199010005; // 행동력 인덱스

        public static double JokerCardProb = 10000; // 조커 카드 등장 확률
        public static int JokerCardCount = 10; // 조커 카드 획득 시 얻는 시너지 게이지 스택


        public static int LimitDailyAdStamina = 3; // 하루에 최대 광고로 회복시킬 수 있는 행동력 횟수
        public static double StaminaChargeCount = 15; // 행동력 회복 시 몇 개의 행동력이 회복되는지
        public static double StaminaPrice = 200; // 행동력 구매 시 최초 몇 개의 다이아가 필요한지




        public static double GasSynergyProb = 60; // 가스 시너지 도박 시 성공률
        public static int GasSynergyCount = 5; // 가스 시너지 도박 성공 시 얻는 시너지 게이지 스택
        public static double GasHpRecovery = 15; // 가스 소모하여 회복되는 HP % (전체 HP%)
        public static int GasIndex = 199010004; // 가스 재화 인덱스

        public static int RandomShopAdRefreshCount = 2; //랜덤 상점 AD 초기화 최대 횟수
        public static int RandomShopRefreshCount = 3; //랜덤 상점 다이아 초기화 최대 횟수
        public static int RandomShopRefreshIndex = 199010003; //랜덤 상점 초기화 필요 재화 인덱스
        public static double RandomShopRefreshValue = 50; //랜덤 상점 초기화 필요 재화 개수
        public static int RandomShopSlot = 5; //랜덤 상점에 노출되는 아이템 개수

        public static double RandomShopFreeDia = 20; //랜덤 상점 첫번째 슬롯에 상시 노출되는 다이아 개수
        public static int RandomShopFreeDiaAD = 2; //광고 보고 추가로 지급받을 수 있는 다이아 횟수

        public static int BuffAdDailyCount = 2; //하루에 광고 버프를 활성화 할 수 있는 횟수 제한
        public static int BaseSellingItemOnce = 150050026; //계정 생성 후 최초 1회 일일 상점 고정 



        public static int Research2SlotOpenCost = 150010001; // 연구 2슬롯 해금 PID
        public static double Research3SlotOpenCost = 3000; // 연구 3슬롯 해금 필요 다이아 개수

        public static double ResearchGoodsEarn = 3; // 연구 1회 회복당 획득 개수
        public static double ResearchChargingTime = 60; // 연구 1회 회복에 필요한 시간 (초)

        public static double ResearchGoodsLimitCount = 500; // 연구 재료 최대 소지량
        public static int ResearchGoodsIndex = 199010023; // 연구 재료 인덱스

        public static double ResearchADTime = 1800; // 연구 광고 시청 시 감소하는 시간 (초)
        public static int ResearchADTimeCount = 4; // 연구 광고 시청 가능 횟수

        public static int ResearchAccelTicketIndex = 199010011; // 가속 티켓 인덱스
        public static double ResearchAccelTime = 600; // 가속 티켓 1개당 감소하는 연구 시간 (초)

        public static int SkillEvolutionUnlock = 10; // 스킬 진화 해금 조건
        public static int SynergyMAXGoods = 199010025; // 속성 MAX 이후 지급되는 재화 인덱스

        public static int DailySweepCount = 5; // 하루 광고보고 소탕 할 수 있는 제한 횟수
        public static int DailyDoubleRewardCount = 5; // 하루 광고보고 소탕 할 수 있는 제한 횟수



        public static float SkillMinCoolTime = 0.3f; // 스킬 최소 쿨타임
        public static int SkillMinCoolCount = 1; // 스킬 최소 쿨타임카운트

        public static float SkillMaxCoolDecrease = 0.8f; // 스킬 최소 쿨타임

        public static int SynergyTutorialStage = 9999; // 시너지 튜토리얼 스테이지
        public static int SynergyTutorialWave = 1; // 시너지 튜토리얼 웨이브

        public static int SynergyOpenTutorialStage = 9999; // 시너지 오픈 튜토리얼 스테이지
        public static int SynergyOpenTutorialWave = 5; // 시너지 오픈 튜토리얼 웨이브

        public static int SynergyUnLockTutorialStage = 9999; // 시너지 오픈 튜토리얼 스테이지
        public static int SynergyUnLockTutorialWave = 5; // 시너지 오픈 튜토리얼 웨이브

        public static int ResearchTutorialStage = 9999; // 연구소 튜토리얼 스테이지
        public static int ResearchTutorialWave = 20; // 연구소 튜토리얼 웨이브

        public static int GearTutorialStage = 3; // 장비 튜토리얼 스테이지
        public static int GearTutorialWave = 5; // 장비 튜토리얼 웨이브

        public static int GearEquipTutorialStage = 3; // 장비 튜토리얼 스테이지
        public static int GearEquipTutorialWave = 10; // 장비 튜토리얼 웨이브


        public static int RelicTutorialStage = 4; // 유물 튜토리얼 스테이지
        public static int RelicTutorialWave = 20; // 유물 튜토리얼 웨이브

        public static int DescendTutorialStage = 5; // 강림 튜토리얼 스테이지
        public static int DescendTutorialWave = 5; // 강림 튜토리얼 웨이브

        public static int SynergyBreakTutorialStage = 4; // 시너지 튜토리얼 스테이지
        public static int SynergyBreakTutorialWave = 5; // 시너지 튜토리얼 웨이브

        public static int RuneTutorialStage = 5; // 룬 튜토리얼 스테이지
        public static int RuneTutorialWave = 20; // 룬 튜토리얼 웨이브


        public static int JobTutorialStage = 5; // 전직 튜토리얼 스테이지
        public static int JobTutorialWave = 40; // 전직 튜토리얼 웨이브

        public static int DungeonTutorialStage = 3; // 던전 튜토리얼 스테이지
        public static int DungeonTutorialWave = 30; // 던전 튜토리얼 웨이브

        public static V2Enum_IntervalType ExchangeInterval = V2Enum_IntervalType.Day;

        public static double NickNameChangeDiaCost = 500.0;


        public static ObscuredFloat MaxEffectiveMoveSpeedValue = 15.0f;


        //시스템챗 이름
        public static readonly string SystemChatName = "System";


        public static readonly double PerStatRecoverValue = 0.0001;
        
        public static readonly double PerSkillEffectRecoverValue = 0.000001;
        public static readonly double PerStatPrintRecoverValue = 0.01;
        public static readonly double PerStatPrintRecoverValueTemp = 0.0001;

        public static readonly double PercentageRecoverValue = 0.01; // 백분율

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

        public static readonly ObscuredInt SynergyDungeonCharge = 50;
    }
}