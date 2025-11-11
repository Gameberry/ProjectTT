namespace GameBerry.UI
{
    public enum UISibling
    {
        // Screen UI 1 ~ 499
        SceneUI = 0,

        // Login
        LoginDialog,
        // Login

        // InGame

        GlobalDungeonFadeDialog,

        LobbyWaveRewardDialog,
        InGamePlayContentDialog,

        LobbyEtcMenuDialog,

        InGameGambleDialog,

        InGameGoodsDropDirectionDialog,

        CharacterInfoDialog,
        CharacterContentDialog,
        PetContentDialog,

        LobbyRelicContentDialog,
        LobbyTimeAttackMissionDialog,
        LobbyDescendContentDialog,
        LobbyQuestContentDialog,
        LobbySynergyContentDialog,
        LobbySynergyRuneContentDialog,
        LobbyResearchDialog,

        LobbyCharacterContentDialog,

        LobbyGearContentDialog,

        LobbyTotalExpDetailViewDialog,

        DungeonContentDialog,
        DungeonSelectDialog,

        LobbyStaminaShopDialog,
        AdBuffDialog,

        LobbyPassDialog,
        LobbyPassConditionDialog,

        ShopPackageLimitTimePopupDialog,

        ShopGeneralDialog,
        ShopSummonPercentageDialog,

        StageCooltimeRewardDialog,

        InGameRankDialog,

        InGameSkillViewDialog,

        InGameGamble_CardDialog,
        InGameGambleCardHandDialog,

        BattleSceneDialog,

        InGameGambleSynergyDialog,
        InGameDescendContentDialog,
        InGameGamble_SlotDialog,

        InGameGambleSynergyDetailDialog,
        InGameGambleSynergyCombineDetailDialog,

        InGameGambleAutoDialog,

        InGamePostPopupDialog,

        InGameSimpleDescPopupDialog,

        GuildAlertPopupDialog,
        GuildDungeonRewardPopupDialog,
        InGameRewardPopupDialog,
        EventRouletteResetRewardPopupDialog,


        ShopSummonResultPopupDialog,

        InGameGoodsDescPopupDialog,

        InGameBoxPercentageDialog,

        InGameNoticeDialog,

        // Global UI 501 ~ 799
        GlobalUI = 500,

        GlobalFadeDialog,
        GlobalSettingDialog,

        ChatBlockListDialog,
        ChatUserReportDialog,

        InGameNickNameChangePopupDialog,

        GlobalPopupDialog,
        GlobalButtonLockDialog,
        GlobalBufferingDialog,
        GlobalNoticeDialog,
        GlobalEmergencyNoticeDialog,

        GlobalCheatDialog,

        // PowerSaving UI 801 ~ 999
        PowerSavingUI = 800,
        GlobalPowerSavingDialog,

    }

    //public enum UISibling
    //{
    //       // Screen UI 1 ~ 499
    //       SceneUI = 0,

    //       // Login
    //       LoginDialog,
    //       // Login

    //       // InGame

    //       AllyStatePortraitDialog,
    //       InGameBerserkerModeDialog,
    //       DungeonProcessDialog,
    //       AllyArenaProcessDialog,

    //       SkillSlotDialog,

    //       GlobalDungeonFadeDialog,

    //       DynamicBuffProgressDialog,

    //       InGameGuideQuestDialog,

    //       InGamePlayMenuDialog,

    //       AdBuffBackGroundDialog,

    //       InGameGoodsDropDirectionDialog,

    //       ChatViewDialog,
    //       ChatGuildDialog,
    //       InGameTrainingDialog,

    //       DungeonNoticeDialog,

    //       CharacterGearDialog,
    //       CharacterProfileDialog,

    //       CharacterSkillDialog,
    //       CharacterFameDialog,
    //       CharacterMasteryDialog,
    //       CharacterTraitDialog,

    //       AllyContentDialog,
    //       AllyComposeDialog,
    //       AllyPromoAllDialog,
    //       AllyCollectionDialog,
    //       CharacterRuneDialog,
    //       DungeonContentDialog,

    //       ClanCastleDialog,

    //       ShopSummonDialog,
    //       ShopSummon_AllyDialog,

    //       ShopPackageDialog,
    //       ShopFixedTermDialog,
    //       ShopSpecialDialog,

    //       InGamePlayContentDialog,
    //       CharacterContentNavBarDialog,
    //       AllyContentNavBarDialog,
    //       ShopContentNavBarDialog,

    //       PlayerInfoDialog,

    //       InGameQuestDialog,

    //       InGameGuideNPCNoticeDialog,

    //       AdBuffDialog,

    //       InGameEventListDialog,

    //       EventDungeonGoddessDialog,
    //       EventDungeonGoddessSummonChanceDialog,
    //       EventDungeonGoddessMissionDialog,
    //       EventDungeonGoddessShopDialog,
    //       EventDungeonGoddessPassConditionDialog,
    //       EventDungeonGoddessRankDialog,
    //       EventDungeonGoddessRankRewardDialog,

    //       EventRedBullDialog,
    //       EventRedBullSummonChanceDialog,
    //       EventRedBullMissionDialog,
    //       EventRedBullShopDialog,
    //       EventRedBullPassConditionDialog,
    //       EventRedBullRankDialog,
    //       EventRedBullRankRewardDialog,

    //       EventUrsulaDialog,
    //       EventUrsulaSummonChanceDialog,
    //       EventUrsulaMissionDialog,
    //       EventUrsulaShopDialog,
    //       EventUrsulaPassConditionDialog,
    //       EventUrsulaRankDialog,
    //       EventUrsulaRankRewardDialog,

    //       EventDigDialog,
    //       EventDigShopDialog,
    //       EventDigPassConditionDialog,


    //       EventMathRpgDialog,
    //       EventMathRpgPassConditionDialog,
    //       EventMathRpgRankDialog,
    //       EventMathRpgRankRewardDialog,
    //       EventMathRpgStageDialog,
    //       EventMathRpgRouletteDialog,
    //       EventMathRpgShopDialog,
    //       EventMathRpgRewardPopupDialog,

    //       EventDungeonKingSlimeDialog,
    //       EventDungeonKingSlimeSummonChanceDialog,
    //       EventDungeonKingSlimeMissionDialog,
    //       EventDungeonKingSlimeShopDialog,
    //       EventDungeonKingSlimePassConditionDialog,
    //       EventDungeonKingSlimeRankDialog,
    //       EventDungeonKingSlimeRankRewardDialog,

    //       InGameEventPassDialog,
    //       InGameEventPassConditionDialog,

    //       ClanContentDialog,
    //       ClanMissionDialog,
    //       ClanMissionShopDialog,
    //       ClanMissionAllySelectDialog,
    //       ClanMissionDiapatchListDialog,


    //       GuildListDialog,
    //       GuildDialog,

    //       GuildMissionDialog,
    //       GuildCoinShopDialog,
    //       GuildCheckInDialog,
    //       GuildPurchaseDialog,
    //       GuildRaidShopDialog,
    //       GuildRaidEntranceDialog,

    //       GuildMemberListDialog,
    //       GuildApplicationAcceptListDialog,
    //       GuildRaidRankDialog,
    //       GuildRaidRankRewardDialog,
    //       GuildNoticeDialog,

    //       GuildMemberInfoDialog,
    //       GuildInfoDialog,
    //       GuildFoundateDialog,



    //       CharacterGearDetailDialog,
    //       CharacterSkinDialog,

    //       CharacterSkillDetailDialog,
    //       AllyDetailDialog,
    //       AllyFormationDetailDialog,
    //       AllyPromoMaterialDialog,

    //       AllyJewelryDetailDialog,
    //       CharacterRuneCollectionDialog,
    //       CharacterRuneDetailDialog,
    //       CharacterRuneDiscoverDialog,

    //       CharacterTraitPercentageDialog,
    //       CharacterTraitSynergyDetailDialog,

    //       DungeonSelectDialog,


    //       CharacterBerserkerEquipDialog,
    //       CharacterBerserkerEquipDetailDialog,

    //       AllyArenaContentDialog,
    //       AllyArenaFormationDetailDialog,
    //       AllyArenaRuneDetailDialog,
    //       AllyArenaMissionDialog,
    //       AllyArenaHonorShopDialog,
    //       AllyArenaBattleShopDialog,
    //       AllyArenaRankDialog,
    //       AllyArenaRankRewardDialog,




    //       ShopSummonPercentageDialog,
    //       ShopRelayPackageGroupPopupDialog,
    //       ShopSpecialPackagePopupDialog,
    //       ShopPackagePopupDialog,
    //       ShopPackageEventPopupDialog,
    //       ShopPackageRotatePopupDialog,


    //       AllyPromoCompletePopupDialog,
    //       AllyAllPromoResultPopupDialog,

    //       InGamePassDialog,

    //       StageContentMapDialog,
    //       StageChangeDialog,
    //       StageDevilCastleEnterDialog,
    //       StageTrialTowerEnterDialog,

    //       InGamePassConditionDialog,

    //       DungeonRewardDialog,

    //       StageCooltimeRewardDialog,
    //       InGameLuckyRouletteDialog,
    //       InGamePostPopupDialog,
    //       InGameNoticeDialog,

    //       InGameCheckInDialog,

    //       InGameInventoryDialog,

    //       InGameMissionDialog,

    //       InGameExchangeDialog,

    //       InGameRankDialog,

    //       SevenDayDialog,

    //       InGameForgeShopDialog,

    //       EventRouletteDialog,
    //       EventRouletteShopDialog,
    //       EventRouletteRankDialog,
    //       EventRouletteMissionDialog,
    //       EventRouletteRankRewardDialog,
    //       EventRouletteSummonChanceDialog,

    //       EventDungeonDialog,
    //       EventDungeonMissionDialog,
    //       EventDungeonShopDialog,
    //       EventDungeonRankDialog,
    //       EventDungeonRankRewardDialog,



    //       RotationEventDialog,
    //       RotationEventMissionDialog,
    //       RotationEventRankDialog,
    //       RotationEventRankRewardDialog,
    //       RotationEventShopDialog,

    //       AllyStorageExtensionPopupDialog,
    //       InGameGradeSelectDialog,
    //       InGameGradeSelect_AllyDialog,
    //       InGameGradeSelect_JewelryDialog,
    //       InGameGradeSelect_TraitDialog,
    //       InGameSelectGoodsPopupDialog,

    //       InGameGuideInteractorDialog,

    //       DungeonElementDescPopupDialog,



    //       InGameRankDetailViewDialog,

    //       InGameSimpleDescPopupDialog,

    //       GuildAlertPopupDialog,
    //       GuildDungeonRewardPopupDialog,
    //       InGameRewardPopupDialog,
    //       EventRouletteResetRewardPopupDialog,

    //       ShopSummonResultPopupDialog,

    //       InGameGoodsDescPopupDialog,

    //       InGameBoxPercentageDialog,

    //       // Global UI 501 ~ 799
    //       GlobalUI = 500,

    //       GlobalFadeDialog,
    //       GlobalSettingDialog,

    //       ChatBlockListDialog,
    //       ChatUserReportDialog,

    //       InGameNickNameChangePopupDialog,

    //       GlobalPopupDialog,
    //       GlobalButtonLockDialog,
    //       GlobalBufferingDialog,
    //       GlobalNoticeDialog,
    //       GlobalEmergencyNoticeDialog,

    //       GlobalCheatDialog,

    //       // PowerSaving UI 801 ~ 999
    //       PowerSavingUI = 800,
    //       GlobalPowerSavingDialog,

    //   }
}