using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry.Managers
{
    public class BattleSceneManager : MonoSingleton<BattleSceneManager>
    {
        public Dictionary<Enum_Dungeon, BattleSceneBase> _battleScene_Dic = new Dictionary<Enum_Dungeon, BattleSceneBase>();

        private BattleSceneBase _currentBattleScene = null;
        public BattleSceneBase CurrentBattleScene { get { return _currentBattleScene; } }
        private Enum_Dungeon _currentBattleType = Enum_Dungeon.None;
        public Enum_Dungeon BattleType { get { return _currentBattleType; } }

        public Enum_Dungeon _prevBattleType = Enum_Dungeon.None;

        private string m_lastBGName = string.Empty;

        public bool ShowTestBG = false;

        public string TestBGResMapPath = string.Empty;

        public string TestBGResMapName = string.Empty;

        private BattleSceneCamera _battleSceneCamera;

        private BattleSceneMap _battleSceneMap;

        public float releaseOperXPos = 2.0f;
        public float releaseWaitPosTime = 0.1f;
        public float releasePosTime = 0.5f;

        public float releaseCraeturePosTime = 1.0f;

        public GameBerry.Event.RefreshBattleSceneUIMsg _refreshBattleSceneUIMsg = new Event.RefreshBattleSceneUIMsg();
        private Event.VisibleDungeonExitPopupMsg m_visibleDungeonExitPopupMsg = new Event.VisibleDungeonExitPopupMsg();

        protected override void Init()
        {
            ResourceLoader.Instance.Load<GameObject>("BattleScene/BattleSceneCamera", o =>
            {
                GameObject clone = Instantiate(o, transform) as GameObject;
                if (clone != null)
                    _battleSceneCamera = clone.GetComponent<BattleSceneCamera>();
            });

            ResourceLoader.Instance.Load<GameObject>("BattleScene/BattleSceneMap", o =>
            {
                GameObject clone = Instantiate(o, transform) as GameObject;
                if (clone != null)
                    _battleSceneMap = clone.GetComponent<BattleSceneMap>();
            });

            SetBattleMap(0);
        }
        //------------------------------------------------------------------------------------
        public void InitBattleScene()
        {
            AddBattleScene(Enum_Dungeon.StageScene, new BattleScene_Stage());

            foreach (var pair in _battleScene_Dic)
            {
                pair.Value.Init();
            }
        }
        //------------------------------------------------------------------------------------
        private void AddBattleScene(Enum_Dungeon Enum_BattleType, BattleSceneBase battleSceneBase)
        {
            _battleScene_Dic.Add(Enum_BattleType, battleSceneBase);
            _battleScene_Dic[Enum_BattleType].MyEnum_BattleType = Enum_BattleType;
        }
        //------------------------------------------------------------------------------------
        public BattleSceneBase GetBattleSceneLogic(Enum_Dungeon enum_BattleType)
        {
            if (_battleScene_Dic.ContainsKey(enum_BattleType) == true)
                return _battleScene_Dic[enum_BattleType];

            return null;
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            _currentBattleScene?.Updated();
        }
        //------------------------------------------------------------------------------------
        private void LateUpdate()
        {
            _currentBattleScene?.LateUpdated();
        }
        //------------------------------------------------------------------------------------
        private bool doChanged = false;
        //------------------------------------------------------------------------------------
        public void ChangeBattleScene(Enum_Dungeon enum_BattleType)
        {
            if (_currentBattleType == enum_BattleType)
                return;

            if (doChanged == true)
                return;

            doChanged = true;

            _prevBattleType = _currentBattleType;

            _currentBattleType = enum_BattleType;

            GoRootPos().Forget();
        }
        //------------------------------------------------------------------------------------
        public void ReloadCurrentBattleScene()
        {
            if (_currentBattleType == Enum_Dungeon.None || doChanged == true)
                return;

            doChanged = true;
            GoRootPos().Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask GoRootPos()
        {
            Contents.GlobalContent.DoFade(false, 0.5f);
            await UniTask.Delay(500);

            _currentBattleScene?.ReleaseBattleScene();
            _currentBattleScene = _battleScene_Dic[_currentBattleType];

            await UniTask.Delay(500);

            //await UniTask.Delay(1000);

            ResetCameraPos();

            _currentBattleScene?.SetBattleScene();
            
            Message.Send(_refreshBattleSceneUIMsg);

            Contents.GlobalContent.DoFade(true, 0.5f);

            doChanged = false;
        }
        //------------------------------------------------------------------------------------
        public void VisibleDungeonExitPopup(bool visible)
        {
            if (doChanged == true)
                return;

            m_visibleDungeonExitPopupMsg.Visible = visible;

            Message.Send(m_visibleDungeonExitPopupMsg);
        }
        //------------------------------------------------------------------------------------
        private void ResetCameraPos()
        {
            Vector3 pos = _battleSceneCamera.transform.position;
            pos.x = 0.0f;
            _battleSceneCamera.transform.position = pos;
        }
        //------------------------------------------------------------------------------------
        public BattleSceneCamera GetBattleSceneCamera()
        {
            return _battleSceneCamera;
        }
        //------------------------------------------------------------------------------------
        public CharacterControllerBase GetPlayer()
        {
            if (_currentBattleScene == null)
                return null;

            return _currentBattleScene.PlayerController;
        }
        //------------------------------------------------------------------------------------
        public void PlayCameraShake(
            float strengthOverride = -1f,
            float durationOverride = -1f)
        {
            _battleSceneCamera?.PlayShake(strengthOverride, durationOverride);
        }
        //------------------------------------------------------------------------------------
        public void SetBattleMap(int index)
        {
            _battleSceneMap?.SetMap(index);
        }
        //------------------------------------------------------------------------------------
        public void SpawnMonster(int count)
        {
            _battleSceneMap?.SpawnMonster(count);
        }
        //------------------------------------------------------------------------------------
        public void ReleaseAllMonsters()
        {
            _battleSceneMap?.ReleaseAllMonsters();
        }
        //------------------------------------------------------------------------------------
        public void DeadPlayer(PlayerController playerController)
        {
            _currentBattleScene?.DeadPlayer(playerController);
        }
        //------------------------------------------------------------------------------------
        public void DeadMonster(MonsterController monsterController)
        {
            _currentBattleScene?.DeadMonster(monsterController);
        }
        //------------------------------------------------------------------------------------
    }
}
