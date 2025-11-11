using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Managers;
using GameBerry.Event;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UIMapElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _stageNumber;

        [SerializeField]
        private Button _stageEnter;

        [SerializeField]
        private List<Image> _stageStar = new List<Image>();

        [SerializeField]
        private UIGlobalGoodsRewardIconElement _uIGlobalGoodsRewardIconElement;

        private MapData _currentMapData;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_stageEnter != null)
                _stageEnter.onClick.AddListener(OnClick_EnterMap);
        }
        //------------------------------------------------------------------------------------
        public void SetMapData(MapData mapData)
        {
            _currentMapData = mapData;

            if (mapData == null)
            {
                if (_stageNumber != null)
                    _stageNumber.SetText("-");

                for (int i = 0; i < _stageStar.Count; ++i)
                {
                    _stageStar[i].gameObject.SetActive(false);
                }

                if (_stageEnter != null)
                    _stageEnter.gameObject.SetActive(false);
                return;
            }

            if (_stageNumber != null)
                _stageNumber.SetText("{0}", mapData.StageNumber.GetDecrypted());


            bool isClearStage = Managers.MapManager.Instance.IsClearStage(mapData);

            if (_stageEnter != null)
                _stageEnter.gameObject.SetActive(isClearStage == false);

            if (_uIGlobalGoodsRewardIconElement != null)
            {
                _uIGlobalGoodsRewardIconElement.gameObject.SetActive(isClearStage == false);
            }

            for (int i = 0; i < _stageStar.Count; ++i)
            {
                _stageStar[i].gameObject.SetActive(false);
            }

            return; // 별 시스템은 나중에

            int mapStar = Managers.MapManager.Instance.GetStar(mapData);

            for (int i = 0; i < _stageStar.Count; ++i)
            {
                _stageStar[i].gameObject.SetActive(i < mapStar);
            }

            if (_stageEnter != null)
                _stageEnter.gameObject.SetActive(mapStar < 3);

            
            //if (_uIGlobalGoodsRewardIconElement != null)
            //{
            //    MapRewardData mapRewardData = Managers.MapManager.Instance.GetMapRewardData(_currentMapData);
            //    _uIGlobalGoodsRewardIconElement.SetRewardElement(mapRewardData.PerfectClearReward);
            //    _uIGlobalGoodsRewardIconElement.gameObject.SetActive(mapStar < 3);
            //}
        }
        //------------------------------------------------------------------------------------
        private void OnClick_EnterMap()
        {
            if (_currentMapData != null)
            {
                Managers.MapManager.Instance.SetMapLastEnter(_currentMapData.StageNumber);
                Managers.BattleSceneManager.Instance.ChangeBattleScene(V2Enum_Dungeon.StageScene);
            }
        }
        //------------------------------------------------------------------------------------
    }
}