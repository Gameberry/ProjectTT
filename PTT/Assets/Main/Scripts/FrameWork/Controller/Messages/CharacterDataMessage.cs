using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Event
{
    public class ReadyStageCooltimeRewardMsg : Message
    { 

    }

    public class ReadyLuckyRouletteMsg : Message
    {

    }

    public class SetRemainLuckyRouletteMsg : Message
    {

    }

    public class RefreshNickNameMsg : Message
    {
    }

    public class SetLoginStateMsg : Message
    {
        public bool IsSuccess = false;
        public string ShowNoticeMsg = string.Empty;
    }

    public class ShowGoodsDropMsg : Message
    {
        public UnityEngine.Sprite sprite;
        public UnityEngine.Vector3 from;
        public UnityEngine.Vector3 to;
        public int amount = -1;
    }

    public class RefreshProfileMsg : Message
    {
        public CharacterProfileData characterProfileData;
    }

    public class ChangeEquipStateGearMsg : Message
    {
        public V2Enum_GearType v2Enum_ARR_SynergyType;
        public GearData synergyRuneData;

        public bool IsEquipResult = false;
    }


    public class ShowNewGearMsg : Message
    {
        public GearData NewSynergyEffectData;
    }

    public class RefreshGearAllSlotMsg : Message
    {

    }

    public class RefreshCharacterSkinInfoListMsg : Message
    {
        public List<CharacterSkinData> datas = new List<CharacterSkinData>();
    }

    public class ChangeCharacterSkinInfoMsg : Message
    {
        public V2Enum_Skin V2Enum_Skin;
        public CharacterSkinData BeforeGear;
        public CharacterSkinData AfterGear;
    }

    public class RefreshResearchInfoListMsg : Message
    {
        public List<ResearchData> datas = new List<ResearchData>();
    }

    public class NoticeResearchCompleteMsg : Message
    {
    }

    public class RefreshResearchSlotMsg : Message
    {
        public int SlotIdx;
    }


    public class RefreshBerserkerModeStatListMsg : Message
    {
    }

    public class RefreshBerserkerSlotMsg : Message
    {
        public int BerserkerSlotIndex;
    }

    public class SetBerserkerSlotDetailViewMsgMsg : Message
    {
        public int BerserkerSlotIndex;
    }

    public class DrawNextResearchLineMsg : Message
    {
        public ResearchData researchData;
    }

    public class RefreshAutoGachaStateMsg : Message
    {
        public bool AutoGachaState = false;
    }

    public class RefreshSummonInfoListMsg : Message
    {
        public List<V2Enum_SummonType> datas = new List<V2Enum_SummonType>();
    }

    public class SetShopSummonPercentageDialogMsg : Message
    {
        public SummonGroupData summonGroupData;
    }

    public class SetRandomBoxPercentageMsg : Message
    {
        public BoxData boxData;
    }

    public class RefreshTrainingMsg : Message
    {
        public int TrainingId;
    }

    public class SetDungeonSelectMsg : Message
    {
        public V2Enum_Dungeon v2Enum_Dungeon;
    }

    public class RefreshDungeonAdInfoListMsg : Message
    {
        public List<V2Enum_Dungeon> datas = new List<V2Enum_Dungeon>();
    }

    public class RefreshAdBuffStateMsg : Message
    {
    }

    public class RefreshDynamicBuffProgressMsg : Message
    {
        public int Index;
        public UnityEngine.Sprite Icon;
        public float RamainTime;
        public float ApplyTime;

        public int BuffStack;
    }

    public class EndDynamicBuffProgressMsg : Message
    {
        public int Index;
    }

    public class RefreshPassListMsg : Message
    {
        public List<PassData> passDatas = new List<PassData>();
    }

    public class RefreshPassUIMsg : Message
    {
    }

    public class SetPassRewardDialogMsg : Message
    {
        public V2Enum_PassType v2Enum_PassType;
    }

    public class RefreshPostListMsg : Message
    { 

    }

    public class RefreshShopPostListMsg : Message
    {

    }

    public class RefreshCheckInRewardMsg : Message
    {
        public V2Enum_CheckInType v2Enum_CheckInType = V2Enum_CheckInType.Max;
    }

    public class RefreshGuideQuestMsg : Message
    {
        public GuideQuestData guideQuestData = null;
    }

    public class SetGuideDialogMsg : Message
    {
        public GuideTutorialData guideTutorialData = null;
    }

    public class SetGuideInteractorDialogMsg : Message
    {
        public GuideQuestData guideQuestData = null;
    }

    public class VisibleGuideInteractorFocusMsg : Message
    {
        public bool visible;
        public UI.UIGuideInteractor uIGuideInteractor = null;
    }

    public class SetSodialDialogMsg : Message
    {
        public string nickName;
        public bool needData;
        public RankDetailInfo detailinfo;
    }

    public class RefreshQuestDataMsg : Message
    {
        public List<int> missionDatas = new List<int>();
    }

    public class RefreshQuestGaugeDataMsg : Message
    {
        public V2Enum_QuestType v2Enum_QuestType = V2Enum_QuestType.Max;
    }

    public class RefreshTimeAttackMissionMsg : Message
    {
    }

    public class HideTimeAttackMissionIconMsg : Message
    {
    }

    public class SetStartRotationEventMsg : Message
    {

    }

    public class RefreshEventPassSchedulerMsg : Message
    {
        public EventPassGroupData eventPassGroupData = null;
    }

    public class EndEventPassDisplayTimeMsg : Message
    {
        public EventPassGroupData eventPassGroupData = null;
    }

    public class SetEventPassDialogMsg : Message
    {
        public EventPassGroupData eventPassGroupData = null;
    }

    public class RefreshEventPassMsg : Message
    {
        public EventPassGroupData eventPassGroupData = null;
    }

    public class RefreshEventPassMissionDataMsg : Message
    {
        public List<EventPassMissionData> missionDatas = new List<EventPassMissionData>();
    }


    public class RefreshExchangeDataMsg : Message
    {
        public List<ExchangeData> exchangeDatas = new List<ExchangeData>();
    }

    public class ChangeAdFreeStateUIMsg : Message
    { 

    }

    public class SetStorageExtensionPopupDialogMsg : Message
    {
        public ContentDetailList contentDetailList;
    }

    public class RefreshBlockUserMsg : Message
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

    public class SetSummonPopupMsg : Message
    {
        public V2Enum_Goods GoodsType = V2Enum_Goods.Max;
        public List<RewardData> RewardDatas = new List<RewardData>();
    }

    public class SetSummonPopup_SummonBtnMsg : Message
    {
        public V2Enum_SummonType v2Enum_SummonType;
    }

    public class RefreshRelayGroupMsg : Message
    {
        public ShopPackageRelayGroupData RefreshData;
    }

    public class SetRelayPackageGroupPopupMsg : Message
    {
        public ShopPackageRelayGroupData RefreshData;
    }

    public class RefreshShopSpecialMsg : Message
    {
        public ShopPackageSpecialData shopPackageSpecialData;
    }

    public class SetShopSpecialPackagePopupMsg : Message
    {
        public ShopPackageSpecialData shopPackageSpecialData;
    }


    public class ShowHudSpecialPackageRemainTimeMsg : Message
    {
        public int index;
    }

    public class RefreshShopEventMsg : Message
    {
        public ShopPackageEventData shopPackageEventData;
    }

    public class SetShopEventPackagePopupMsg : Message
    {
        public ShopPackageEventData shopPackageEventData;
    }

    public class RefreshShopRotateMsg : Message
    {
        public ShopPackageRotateGroupData shopPackageRotateGroupData;
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

    public class SetGradeSelectPopupMsg : Message
    {
        public System.Action<List<V2Enum_Grade>> SelectedCallBack = null;
    }

    public class SetGradeSelectTraitPopupMsg : Message
    {
        public System.Action<List<V2Enum_Grade>> SelectedCallBack = null;
    }

    public class SetSimpleDescPopupMsg : Message
    {
        public string title = string.Empty;
        public string desc = string.Empty;
    }


    public class RefreshCastleGuildInfoMsg : Message
    { 
    }

    public class RefreshCastleGuildUIMsg : Message
    {

    }
}