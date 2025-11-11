using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;
using Spine;
using Spine.Unity;

namespace GameBerry.UI
{
    public class BattleScenePositionSetDialog : IDialog
    {

        [SerializeField]
        private Button _exitDungeonBtn;

        [SerializeField]
        private Button _playBtn;

        [Header("------------AllyElementGroup------------")]
        [SerializeField]
        private InfiniteScroll _allyElementInfinityScroll;



        private BattleSceneBase _battleSceneBase;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_exitDungeonBtn != null)
                _exitDungeonBtn.onClick.AddListener(OnClick_ExitDungeonBtn);

            if (_playBtn != null)
                _playBtn.onClick.AddListener(OnClick_PlayBtn);

            if (_allyElementInfinityScroll != null)
                _allyElementInfinityScroll.AddSelectCallback(OnClick_AllyElement);

        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        { 

        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            //_battleSceneBase = Managers.BattleSceneManager.Instance.CurrentBattleScene;

            //List<PlayerCharacterInfo> playerV3AllyInfos = Managers.CharacterManager.Instance.GetPlayerCharacterInfos().FindAll(x => x.Enable == 1);
            //if (playerV3AllyInfos == null)
            //    return;

            //playerV3AllyInfos.Sort(Managers.CharacterManager.Instance.SortAllyInfo);

            //_allyElementInfinityScroll.Clear();

            //for (int i = 0; i < playerV3AllyInfos.Count; ++i)
            //{
            //    _allyElementInfinityScroll.InsertData(playerV3AllyInfos[i]);
            //}
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitDungeonBtn()
        {
            Managers.BattleSceneManager.Instance.ChangeBattleScene(V2Enum_Dungeon.LobbyScene);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PlayBtn()
        {
            if (_battleSceneBase.IsReadyBattle() == true)
                _battleSceneBase.PlayBattleScene();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_AllyElement(InfiniteScrollData infiniteScrollData)
        {
            PlayerCharacterInfo playerV3AllyInfo = infiniteScrollData as PlayerCharacterInfo;

            if (playerV3AllyInfo == null)
                return;

            //if (_battleSceneBase.IsContainCreature(playerV3AllyInfo) == true)
            //{
            //    _battleSceneBase.RemoveFriendCreature(playerV3AllyInfo);

            //    if (_allyElementInfinityScroll != null)
            //        _allyElementInfinityScroll.UpdateData(playerV3AllyInfo);
            //}
            //else
            //{
            //    if (_battleSceneBase.CanAddFriendCreature() == true)
            //    {
            //        _battleSceneBase.AddFriendCreature(playerV3AllyInfo);

            //        if (_allyElementInfinityScroll != null)
            //            _allyElementInfinityScroll.UpdateData(playerV3AllyInfo);
            //    }
            //}
        }
        //------------------------------------------------------------------------------------
    }
}