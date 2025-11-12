using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace GameBerry.Event
{
    public class RefreshCharacterInfoListMsg : Message
    {
        public List<PlayerCharacterInfo> datas = new List<PlayerCharacterInfo>();
    }

    public class RefreshCharacterInfo_EnhanceMsg : Message
    {
        public PlayerCharacterInfo playerV3AllyInfo = null;
    }

    public class RefreshCharacterSkin_StatMsg : Message
    {
    }

    public class RefreshCharacterInfo_StatMsg : Message
    {
    }

    public class RefreshBattlePowerUIMsg : Message
    {
    }


    public class RefreshBattleSceneUIMsg : Message
    {

    }

    public class RefreshBattleSpeedMsg : Message
    {

    }


    public class SetHpBarMsg : Message
    {
        public double currHp;
        public double TotalHp;
    }

    public class PlayGambleCardMsg : Message
    {
        public ARR_CardGambleResultData aRR_GambleResultData;
    }

    public class PlayForceEndGambleCardMsg : Message
    {

    }

    public class PlayGasSynergyMsg : Message
    {
    }

    public class PlayMinorJokerSynergyMsg : Message
    {
        public ObscuredInt SynergyStack = 0;
    }

    public class PlayFreeGambleSlotMsg : Message
    {
    }

    public class AddGambleSynergySlotMsg : Message
    {
        public Vector3 UIStartPos;
        public ARR_CardGambleData GambleSkillData;

        public SynergyEffectData BeforeData;
        public int BeforeStack;

        public SynergyEffectData AfterData;
        public int AfterStack;

        public int DescendEnhance = 0;

    }

    public class AddSkillSynergyMsg : Message
    {
        public Enum_SynergyType Enum_SynergyType = Enum_SynergyType.Max;

        public SynergyEffectData BeforeData;
        public int BeforeStack;

        public SynergyEffectData AfterData;
        public int AfterStack;

        public int DescendEnhance = 0;
    }

    public class ShowNewSynergyMsg : Message
    {
        public SynergyEffectData NewSynergyEffectData;
    }

    public class RefreshGambleSynergyMsg : Message
    {
        public Enum_SynergyType Enum_GambleSynergyType = Enum_SynergyType.Max;
    }

    public class RefreshGambleSynergyCombineSkillMsg : Message
    {
        public SynergyCombineData GambleSynergyCombineData;
    }

    public class RefreshReadyGambleSynergyCombineSkillMsg : Message
    {
        public List<SynergyCombineData> ReadySynergyCombineSkillList;
    }

    public class NoticeNewLobbySynergyElementMsg : Message
    {
        public SynergyEffectData SynergyEffectData;
    }

    public class ShowGambleSynergyDetailMsg : Message
    {
        public SynergyEffectData FocusData;
        public Enum_SynergyType Enum_GambleSynergyType = Enum_SynergyType.Max;
    }

    public class ChangeEquipSynergyMsg : Message
    {
        public SynergyEffectData BeforeEquipSynergy;
        public SynergyEffectData AfterEquipSynergy;
    }

    public class ChangeEquipDescendMsg : Message
    {
        public DescendData BeforeEquipSynergy;
        public DescendData AfterEquipSynergy;
    }

    public class ShowNewDescendMsg : Message
    {
        public DescendData NewSynergyEffectData;
    }

    public class RefreshInGameReadyDescendMsg : Message
    {
    }

    public class ChangeInGameActiveDescendMsg : Message
    {
        public List<DescendData> ActiveDescend = new List<DescendData>();
    }

    //public class ShowTotalExpDetailMsg : Message
    //{
    //    public ContentDetailList ContentDetailList;
    //    public SynergyTotalLevelEffectData SynergyTotalLevelEffectData;
    //}



    public class ShowNewRelicMsg : Message
    {
        public RelicData NewSynergyEffectData;
    }



    public class ChangeEquipStateSynergyRuneMsg : Message
    {
        public int slotid;
        public SynergyRuneData synergyRuneData;

        public bool IsEquipResult = false;
    }


    public class ShowNewSynergyRuneMsg : Message
    {
        public SynergyRuneData NewSynergyEffectData;
    }








    public class ResultBattleStageMsg : Message
    {
        public Enum_Dungeon EnumDungeon;
        public bool Win = false;
        public float PlayTime = 0.0f;
        public List<RewardData> WaveRewardList = new List<RewardData>();
        public bool ApplyAdIncreaseRewardMode = false;
        public double ApplyAdIncreaseRewardValue = 0.0;
        public int currentRecord = 0;
        public int prevRecord = 0;
    }

    public class RefreshResultBattleStage_DoubleRewardMsg : Message
    {
        public Enum_Dungeon EnumDungeon;
        public List<RewardData> WaveRewardList = new List<RewardData>();
    }

    public class SetWaveRewardDialogMsg : Message
    {
        public int StageNumber;
    }

    public class ChangeNewChellengeMapMsg : Message
    {
        
    }

    public class RefreshLobbyMapSelecterMapMsg : Message
    {

    }

    public class RefreshStageSweepMsg : Message
    {

    }

    public class PlayARRRTutorialMsg : Message
    {
        public V2Enum_EventType Enum_GambleType = V2Enum_EventType.Max;
    }

    public class PlaySlotTutorialMsg : Message
    {

    }

    public class PlayGasTutorialMsg : Message
    {

    }

    public class PlaySynergyCombineTutorialMsg : Message
    {
    }

    public class PlaySynergyChangeTutorialMsg : Message
    {
        public int Index;
    }

    public class PlaySynergyOpenTutorialMsg : Message
    {
    }

    public class PlaySynergyUnLockTutorialMsg : Message
    {
    }

    public class PlayResearchChangeTutorialMsg : Message
    {
    }

    public class PlayRelicTutorialMsg : Message
    {
    }

    public class PlayGearTutorialMsg : Message
    {
    }

    public class HideGearTutorialGachaFocusMsg : Message
    {
    }


    public class PlayGearEquipTutorialMsg : Message
    {
        public int Index;
    }

    public class PlaySynergyBreakTutorialMsg : Message
    {
    }

    public class PlayRuneTutorialMsg : Message
    {
    }

    public class PlayDescendChangeTutorialMsg : Message
    {
    }

    public class PlayJobTutorialMsg : Message
    {
    }

    public class PlayDungeonTutorialMsg : Message
    {
    }

    public class RefreshStaminaMsg : Message
    {
    }

    public class RefreshAddBuffMsg : Message
    {
        public V2Enum_Stat v2Enum_Stat;
    }

    public class ShowInterestTextMsg : Message
    {
        public string text;
    }

    public class RefreshGambleAutoTriggerMsg : Message
    {
    }

    public class ChangeCurrentWaveStateMsg : Message
    {
        public MapWaveData MapWaveData;
        public int MaxWave;
    }

    public class SetSkillDetailViewMsg : Message
    {
        public SkillBaseData skillBaseData;
    }

    public class ChangeEquipSkillMsg : Message
    {
        public List<int> EquipSkillList = new List<int>();
    }

    public class RefreshShopRandomStoreMsg : Message
    {
    }

    public class RefreshShopVipPackageMsg : Message
    {
    }
}