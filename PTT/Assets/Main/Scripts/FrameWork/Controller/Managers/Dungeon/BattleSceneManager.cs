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

        private BGManager _bgManager = null;
        public BGManager CurrentBgManager { get { return _bgManager; } }

        private string m_lastBGName = string.Empty;

        public bool ShowTestBG = false;

        public string TestBGResMapPath = string.Empty;

        public string TestBGResMapName = string.Empty;

        private BattleSceneCamera _battleSceneCamera;

        public float releaseOperXPos = 2.0f;
        public float releaseWaitPosTime = 0.1f;
        public float releasePosTime = 0.5f;

        public float releaseCraeturePosTime = 1.0f;

        public GameBerry.Event.RefreshBattleSceneUIMsg _refreshBattleSceneUIMsg = new Event.RefreshBattleSceneUIMsg();
        public GameBerry.Event.RefreshBattleSpeedMsg _refreshBattleSpeedMsg = new Event.RefreshBattleSpeedMsg();
        private Event.VisibleDungeonExitPopupMsg m_visibleDungeonExitPopupMsg = new Event.VisibleDungeonExitPopupMsg();

        protected override void Init()
        {
            ResourceLoader.Instance.Load<GameObject>("BattleScene/BattleSceneCamera", o =>
            {
                GameObject clone = Instantiate(o, transform) as GameObject;
                if (clone != null)
                    _battleSceneCamera = clone.GetComponent<BattleSceneCamera>();
            });

            //SetBGIndex(0);
        }
        //------------------------------------------------------------------------------------
        public void InitBattleScene()
        {
            AddBattleScene(Enum_Dungeon.LobbyScene, new Battle_LobbyScene());
            AddBattleScene(Enum_Dungeon.StageScene, new Battle_StageScene());
            AddBattleScene(Enum_Dungeon.DiamondDungeon, new Battle_DiaDungeonScene());
            AddBattleScene(Enum_Dungeon.TowerDungeon, new Battle_DiaDungeonScene());

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
            if (_currentBattleScene != null)
                _currentBattleScene.Updated();

#if DEV_DEFINE
            if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop)
            {
                if (Input.GetKeyUp(KeyCode.Minus))
                    ChangeBattleScene(Enum_Dungeon.LobbyScene);

                if (Input.GetKeyUp(KeyCode.Plus))
                    ChangeBattleScene(Enum_Dungeon.StageScene);

                if (Input.GetKeyUp(KeyCode.KeypadPlus))
                {
                    Time.timeScale += 1.0f;
                }

                if (Input.GetKeyUp(KeyCode.KeypadMinus))
                {
                    Time.timeScale -= 0.5f;
                }

                if (Input.GetKeyUp(KeyCode.KeypadEnter))
                {
                    Time.timeScale = 1.0f;
                }
            }
#endif
        }
        //------------------------------------------------------------------------------------
        private void LateUpdate()
        {
            if (_currentBattleScene != null)
                _currentBattleScene.LateUpdated();
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
        private async UniTask GoRootPos()
        {
            Contents.GlobalContent.DoFade(false, 0.5f);
            await UniTask.Delay(500);

            if (_currentBattleScene != null)
                _currentBattleScene.ReleaseBattleScene();

            ChangeTimeScale(Enum_BattleSpeed.x1);

            _currentBattleScene = _battleScene_Dic[_currentBattleType];
            if (Define.IsSpeedUpMode == false)
            {
                if (_currentBattleScene.BattleSpeedType == Enum_BattleSpeed.x2
                    || _currentBattleScene.BattleSpeedType == Enum_BattleSpeed.x3)
                    _currentBattleScene.ChangeBattleSpeed(Enum_BattleSpeed.x1);
            }

            await UniTask.Delay(500);

            //await UniTask.Delay(1000);

            ResetCameraPos();

            _currentBattleScene.SetBattleScene();
            
            Message.Send(_refreshBattleSceneUIMsg);

            Contents.GlobalContent.DoFade(true, 0.5f);

            doChanged = false;
        }
        //------------------------------------------------------------------------------------
        public void VisibleDungeonExitPopup(bool visible)
        {
            if (doChanged == true)
                return;

            if (BattleType == Enum_Dungeon.StageScene && Managers.MapManager.Instance.NeedTutotial1() == true)
                return;

            //if (BattleType == Enum_BattleType.StageScene && Managers.MapManager.Instance.NeedTutotial2() == true)
            //    return;

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
        public void SelectMap(string mapPath, string mapName)
        {
            if (ShowTestBG == true)
            {
                mapPath = TestBGResMapPath;
                mapName = TestBGResMapName;
            }

            if (m_lastBGName == mapName)
                return;

            GameObject clone = GetMapObject(mapPath, mapName, transform);

            if (clone != null)
            {
                clone.SetLayerInChildren(LayerMask.NameToLayer("InGame"));

                if (_bgManager != null)
                    Destroy(_bgManager.gameObject);

                _bgManager = clone.GetComponent<BGManager>();
                if (_bgManager != null)
                {
                    _bgManager.SetFocusTransform(_battleSceneCamera.transform);
                }

                m_lastBGName = mapName;
            }
        }
        //------------------------------------------------------------------------------------
        public GameObject GetMapObject(string mapKey, string mapName, Transform parent)
        {
            string bundlePath = string.Format("{0}/{1}", mapKey, mapName);
            GameObject clone = null;

            ResourceLoader.Instance.Load<GameObject>(bundlePath, o =>
            {
                if (o == null)
                    return;
                GameObject obj = o as GameObject;
                clone = Instantiate(obj, parent);
            });

            return clone;
        }
        //------------------------------------------------------------------------------------
        public void SetBGIndex(int index)
        {
            if(_bgManager == null)
                SelectMap("BattleScene/MapResources/Map_Coast", "Map_Coast_Aurora_1");

            if (_bgManager != null)
                _bgManager.ShowBGIndex(index);
        }
        //------------------------------------------------------------------------------------
        public float GetCreatureLimitLine()
        {
            if (_currentBattleScene == null)
                return -1;

            return _currentBattleScene.CreatureLimitLine;
        }
        //------------------------------------------------------------------------------------
        public Enum_BattleSpeed CurrentBattleSpeed()
        {
            if (_currentBattleScene != null)
                return _currentBattleScene.BattleSpeedType;

            return Enum_BattleSpeed.x1;
        }
        //------------------------------------------------------------------------------------
        public void ChangeTimeScale(Enum_BattleSpeed Enum_BattleSpeed)
        {
            switch (Enum_BattleSpeed)
            {
                case Enum_BattleSpeed.x1:
                    Time.timeScale = 1.0f;
                    break;
                case Enum_BattleSpeed.x1Dot5:
                    Time.timeScale = 1.5f;
                    break;
                case Enum_BattleSpeed.x2:
                    Time.timeScale = 2.0f;
                    break;
                case Enum_BattleSpeed.x3:
                    Time.timeScale = 3.0f;
                    break;
                case Enum_BattleSpeed.xHalf:
                    Time.timeScale = 0.5f;
                    break;
                case Enum_BattleSpeed.Pause:
                    Time.timeScale = 0.0f;
                    break;
                default:
                    Time.timeScale = 1.0f;
                    break;
            }
        }
        //------------------------------------------------------------------------------------
        public void ChangeBattleSpeed_UI()
        { // UI 에서 바꾸는건 토글 방식으로만 바꿔준다.
            Enum_BattleSpeed Enum_BattleSpeed = Enum_BattleSpeed.x1;

            if (_currentBattleScene != null)
            {
                switch (_currentBattleScene.BattleSpeedType)
                {
                    case Enum_BattleSpeed.x1:
                        Enum_BattleSpeed = Enum_BattleSpeed.x1Dot5;
                        break;
                    case Enum_BattleSpeed.x1Dot5:
                        if (Define.IsSpeedUpMode == true)
                            Enum_BattleSpeed = Enum_BattleSpeed.x2;
                        else
                            Enum_BattleSpeed = Enum_BattleSpeed.x1;
                        break;
                    case Enum_BattleSpeed.x2:
#if DEV_DEFINE
                        Enum_BattleSpeed = Enum_BattleSpeed.x3;
#else
                        Enum_BattleSpeed = Enum_BattleSpeed.x1;
#endif
                        break;
                    case Enum_BattleSpeed.x3:
                        Enum_BattleSpeed = Enum_BattleSpeed.x1;
                        break;
                }
            }

            _currentBattleScene.ChangeBattleSpeed(Enum_BattleSpeed);

            ThirdPartyLog.Instance.SendLog_log_playmode(Enum_BattleSpeed);

            Message.Send(_refreshBattleSpeedMsg);
        }
        //------------------------------------------------------------------------------------
        public void ChangeOriginBattleSpeed()
        {
            if (_currentBattleScene != null)
            {
                ChangeTimeScale(_currentBattleScene.BattleSpeedType);
            }
        }
        //------------------------------------------------------------------------------------
        public void CallDeadCreature(CreatureController creatureController)
        {
            if (_currentBattleScene != null)
                _currentBattleScene.CallDeadCreature(creatureController);
        }
        //------------------------------------------------------------------------------------
        public void CallReleaseCreature(CreatureController creatureController)
        {
            if (_currentBattleScene != null)
                _currentBattleScene.CallReleaseCreature(creatureController);
        }
        //------------------------------------------------------------------------------------
        public void CallDeadARRR(ARRRController creatureController)
        {
            if (_currentBattleScene != null)
                _currentBattleScene.CallDeadARRR(creatureController);
        }
        //------------------------------------------------------------------------------------
        public void CallReleaseARRR(ARRRController creatureController)
        {
            if (_currentBattleScene != null)
                _currentBattleScene.CallReleaseARRR(creatureController);
        }
        //------------------------------------------------------------------------------------
        public void RefreshMyARRRStat()
        {
            if (_currentBattleScene != null)
                _currentBattleScene.RefreshMyARRRControllerStat();
        }
        //------------------------------------------------------------------------------------
        public void AddMyARRRBuff(V2Enum_Stat v2Enum_Stat, ObscuredDouble buffValue)
        {
            if (_currentBattleScene != null)
                _currentBattleScene.AddMyARRRBuff(v2Enum_Stat, buffValue);
        }
        //------------------------------------------------------------------------------------
        public void SetBuff(SkillManageInfo skillManageInfo, IFFType iFFType)
        {
            if (_currentBattleScene != null)
                _currentBattleScene.SetBuff(skillManageInfo, iFFType, true);
        }
        //------------------------------------------------------------------------------------
        public void ReleaseBuff(SkillManageInfo skillManageInfo, IFFType iFFType)
        {
            if (_currentBattleScene != null)
                _currentBattleScene.SetBuff(skillManageInfo, iFFType, false);
        }
        //------------------------------------------------------------------------------------
        public void AddGambleSkill(MainSkillData gambleSkillData, Enum_SynergyType Enum_SynergyType = Enum_SynergyType.Max, SkillInfo skillInfo = null)
        {
            if (_currentBattleScene != null)
                _currentBattleScene.AddGambleSkill(gambleSkillData, Enum_SynergyType, skillInfo);
        }
        //------------------------------------------------------------------------------------
        public void AddPet(PetData petData, SkillInfo skillInfo = null)
        {
            if (_currentBattleScene != null)
                _currentBattleScene.AddPet(petData, skillInfo);
        }
        //------------------------------------------------------------------------------------
        public void AddPetAfterSkill(PetData petData, MainSkillData gambleSkillData, SkillInfo skillInfo = null)
        {
            if (_currentBattleScene != null)
                _currentBattleScene.AddPetAfterSkill(petData, gambleSkillData, skillInfo);
        }
        //------------------------------------------------------------------------------------
    }
}