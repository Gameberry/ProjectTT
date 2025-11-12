using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class BattleScene_StageDialog : IDialog
    {
        [SerializeField]
        private Transform _gaugeGroup;

        [SerializeField]
        private RectTransform _gauge_RectTrans;

        [SerializeField]
        private float _gauge_MaxWeight;

        [SerializeField]
        private Image _gauge_PositionBar_Start;

        [SerializeField]
        private Image _gauge_PositionBar_End;

        [SerializeField]
        private Transform _iconFriend_Icon;

        [SerializeField]
        private UIBattleWaveIconElement _iconFoe_Obj;

        private List<UIBattleWaveIconElement> _iconFoeIcons = new List<UIBattleWaveIconElement>();

        private Vector3 _gaugeStartPos = Vector3.zero;
        private float m_farmPositionGaugeGap = 0.0f;


        [SerializeField]
        private TMP_Text _waveNumbe;

        [SerializeField]
        private TMP_Text _monsterCount;


        [Header("----------Interest----------")]
        [SerializeField]
        private TMP_Text _interestTimer;

        [SerializeField]
        private UIFarmingDirectionController _uIFarmingDirectionController;

        private Battle_StageScene _battle_StageScene;

        public float gaugeSpeed = 2.0f;
        private float originGauge = 0.0f;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_gauge_PositionBar_Start != null
                && _gauge_PositionBar_End != null)
            {
                _gaugeStartPos = _gauge_PositionBar_Start.transform.localPosition;
                m_farmPositionGaugeGap = _gauge_PositionBar_End.transform.localPosition.x - _gauge_PositionBar_Start.transform.localPosition.x;
            }

            _battle_StageScene = Managers.BattleSceneManager.Instance.GetBattleSceneLogic(Enum_Dungeon.StageScene) as Battle_StageScene;

            _battle_StageScene.InterestText += AddInterestValue;

#if DEV_DEFINE
            if (_monsterCount != null)
                _monsterCount.gameObject.SetActive(true);
#endif

            Message.AddListener<GameBerry.Event.ChangeCurrentWaveStateMsg>(ChangeCurrentWaveState);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.ChangeCurrentWaveStateMsg>(ChangeCurrentWaveState);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (_waveNumbe != null)
                _waveNumbe.gameObject.SetActive(false);

            originGauge = 0;

            SetGaugePosIcon(_iconFriend_Icon, _battle_StageScene.ArrrMapPosition);

            List<FoeWaveData> foePos = _battle_StageScene.FoeSpawnedList;

            for (int i = 0; i < foePos.Count; ++i)
            {
                UIBattleWaveIconElement icon = null;
                if (_iconFoeIcons.Count > i)
                {
                    icon = _iconFoeIcons[i];
                }
                else
                {
                    GameObject clone = Instantiate(_iconFoe_Obj.gameObject, _iconFoe_Obj.transform.parent);
                    icon = clone.GetComponent<UIBattleWaveIconElement>();
                    _iconFoeIcons.Add(icon);
                }

                icon.gameObject.SetActive(_battle_StageScene.ArrrMapPosition < foePos[i].CurrentPos);

                icon.SetIcon(foePos[i].MapWaveData.MonsterSetIcon);
                icon.EnableSeashell(foePos[i].MapWaveData.GasReward > 0);
                SetGaugePosIcon(icon.transform, foePos[i].CurrentPos);
            }

            for (int i = foePos.Count; i < _iconFoeIcons.Count; ++i)
            {
                _iconFoeIcons[i].gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (_uIFarmingDirectionController != null)
                _uIFarmingDirectionController.ForceRelease();
        }
        //------------------------------------------------------------------------------------
        private void ChangeCurrentWaveState(GameBerry.Event.ChangeCurrentWaveStateMsg msg)
        {
            if (msg.MapWaveData != null)
            {
                if (_waveNumbe != null)
                {
                    _waveNumbe.gameObject.SetActive(true);
                    _waveNumbe.SetText("{0}/{1}", msg.MapWaveData.WaveNumber, msg.MaxWave);
                }
            }
        }

        //------------------------------------------------------------------------------------
        //private float GetGaugeRatio(float xpos)
        //{
        //    MapData currentMapData = null;
        //    if (_battle_StageScene == null)
        //        return 0.0f;

        //    currentMapData = _battle_StageScene.CurrentMapData;
        //    if (currentMapData == null)
        //        return 0.0f;

        //    float startpos = StaticResource.Instance.GetBattleModeStaticData().Stage_StartXPos;
        //    float endpos = _battle_StageScene.StageEndPos;
        //    float fullRange = endpos - startpos;
        //    return (xpos - startpos) / fullRange;
        //}
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (isEnter == false)
                return;

            if (_battle_StageScene == null)
                return;

            if (_monsterCount != null)
                _monsterCount.SetText("MonsterCount : {0}", _battle_StageScene.RemainFoeCount);

            originGauge += (_battle_StageScene.ArrrMapPosition - originGauge) * Time.deltaTime * gaugeSpeed;
            float directionGauge = originGauge;

            SetGaugePosIcon(_iconFriend_Icon, directionGauge);

            List<FoeWaveData> foePos = _battle_StageScene.FoeSpawnedList;

            for (int i = 0; i < foePos.Count; ++i)
            {
                UIBattleWaveIconElement icon = null;
                if (_iconFoeIcons.Count > i)
                {
                    icon = _iconFoeIcons[i];
                    icon.gameObject.SetActive(_battle_StageScene.ArrrMapPosition < foePos[i].CurrentPos);
                }
            }

            if (_gauge_RectTrans != null)
            {
                float ratio = directionGauge;

                if (ratio > 1.0f)
                    ratio = 1.0f;

                Vector2 gaugeSize = _gauge_RectTrans.sizeDelta;
                gaugeSize.x = _gauge_MaxWeight * ratio;
                _gauge_RectTrans.sizeDelta = gaugeSize;
            }

            //if (_battle_StageScene.IsPlay == true)
            //{
            //    if (_interestTimer != null)
            //        _interestTimer.SetText(string.Format("{0:0.00}", _battle_StageScene.NextInterestTime - Time.time));
            //}
        }
        //------------------------------------------------------------------------------------
        private void AddInterestValue(string text)
        {
            Sprite sprite = Managers.GoodsManager.Instance.GetGoodsSprite(V2Enum_Goods.Point, V2Enum_Point.InGameGold.Enum32ToInt());
            if (_uIFarmingDirectionController != null)
                _uIFarmingDirectionController.ShowFarmingReward(sprite, text);
        }
        //------------------------------------------------------------------------------------
        private void SetGaugePosIcon(Transform icon, float position)
        {
            float ratio = position;

            if (ratio > 1.0f)
                ratio = 1.0f;

            Vector3 pos = _gaugeStartPos;
            pos.x += ratio * m_farmPositionGaugeGap;
            icon.transform.localPosition = pos;
        }
        //------------------------------------------------------------------------------------
    }
}