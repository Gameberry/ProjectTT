using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class Battle_LobbyScene : BattleSceneBase
    {

        //------------------------------------------------------------------------------------
        protected override void OnSetBattleScene()
        {
            UI.UIManager.DialogEnter<UI.LobbyWaveRewardDialog>();
            UI.UIManager.DialogEnter<UI.InGamePlayContentDialog>();
            UI.UIManager.DialogEnter<UI.LobbyEtcMenuDialog>();
            UI.UIManager.DialogEnter<UI.CharacterInfoDialog>();

            List<ShopFreeGoodsData> datas = Managers.ShopRandomStoreManager.Instance.GetShopFreeGoodsDatas();

            for (int i = 0; i < datas.Count; ++i)
            {
                
                if (Managers.ShopRandomStoreManager.Instance.CanRecvFreeDia(datas[i]) == true)
                {
                    if (datas[i].MenuType == V2Enum_ShopMenuType.Research)
                    {
                        if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Research) == true)
                            Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyResearch_Shop);
                    }
                    else
                        Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.ShopRandomStore);
                }
            }

            Managers.SynergyManager.Instance.RefreshSynergyRedDot();

            //Managers.BattleSceneManager.Instance.SelectMap("BattleScene/MapResources/Map_Coast", "Map_Coast_Aurora_1");

            Managers.SoundManager.Instance.PlayBGM("bgm_stage_normal");

            Managers.GuideInteractorManager.Instance.lobbyinterctordelay = Time.time + 1.0f;

            CheatSpawnFriend();

            PlayBattleScene();
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlayBattleScene()
        {
            Managers.TimeManager.Instance.RefreshServerTime();
            PlayDelay().Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayDelay()
        {
            if (MapContainer.MapMaxClear >= 0)
            {
                if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.StageClear)
                {
                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                }

                Managers.GuideInteractorManager.Instance.SetLastGuidePlayTime(V2Enum_EventType.StageClear, Time.time + 120f);
            }

            if (MapContainer.LastFailWave > 0)
                Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.WaveDeath);

            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Research) == true)
            {
                if (Managers.ResearchManager.Instance.GetCanResearchSlot() != -1)
                    Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyResearch);
            }

            if (Managers.BattleSceneManager.Instance._prevBattleType == Enum_Dungeon.DiamondDungeon
                || Managers.BattleSceneManager.Instance._prevBattleType == Enum_Dungeon.TowerDungeon)
                UI.UIManager.DialogEnter<UI.DungeonContentDialog>();

            Managers.GearManager.Instance.RefreshSynergyRedDot();
            Managers.DescendManager.Instance.RefreshSynergyRedDot();
            Managers.QuestManager.Instance.RefreshSynergyRedDot();

            Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.DungeonDiffClear);

            await UniTask.Delay(1500);

            Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.StaminaStack);
            Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.StaminaCount);

            if (Managers.GuideInteractorManager.Instance.ShowNeedGearNotice == true)
            {
                //Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("notice/gotoequipdraw"));

                Managers.GuideInteractorManager.Instance.ShowNeedGearNotice = false;
            }

            if (_isPlay == false)
                return;

            CheatSpawnFoe();
        }
        //------------------------------------------------------------------------------------
        private void CheatSpawnFriend()
        {
            
        }
        //------------------------------------------------------------------------------------
        private void CheatSpawnFoe()
        {
            //for (int i = 0; i < 9; ++i)
            //{
            //    Transform pos = InGamePositionContainer.Instance.GetBattleFoeSpawnPos(i + 1);

            //    CreatureController creatureController = SpawnCreature(Managers.CreatureManager.Instance.GetCreatureData(Random.Range(109010031, 109010035)), 1, pos, IFFType.IFF_Foe, Enum_LookDirection.Left);
            //    creatureController.ReadyCreature();
            //    creatureController.PlayCreature();
            //}

            _remainFoeCount = 9;

            //if (_myARRRControllers != null)
            //{
            //    _myARRRControllers.PlayCreature();
            //    _myARRRControllers.HPRecoverPer(100);
            //}
        }
        //------------------------------------------------------------------------------------
        public override void CallDeadCreature(CreatureController creatureController)
        {
            base.CallDeadCreature(creatureController);

            if (_remainFoeCount == 0)
                CheatSpawnFoe();
        }
        //------------------------------------------------------------------------------------
        protected override void OnReleaseBattleScene()
        {
            UI.UIManager.DialogExit<UI.LobbyWaveRewardDialog>();
            UI.UIManager.DialogExit<UI.InGamePlayContentDialog>();
            UI.UIManager.DialogExit<UI.LobbyEtcMenuDialog>();
            UI.UIManager.DialogExit<UI.CharacterInfoDialog>();
        }
        //------------------------------------------------------------------------------------
    }
}