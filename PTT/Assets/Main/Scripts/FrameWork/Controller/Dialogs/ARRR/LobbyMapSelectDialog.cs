using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Managers;
using GameBerry.Event;
using DG.Tweening;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class LobbyMapSelectDialog : IDialog
    {
        [SerializeField]
        private List<UIMapGroupElement> _uIMapGroupElements = new List<UIMapGroupElement>();

        [SerializeField]
        private Button _prevBtn;

        [SerializeField]
        private Button _nextBtn;

        private ObscuredInt _minStage = 0;
        private ObscuredInt _maxStage = 0;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_prevBtn != null)
                _prevBtn.onClick.AddListener(OnClick_PrevBtn);

            if (_nextBtn != null)
                _nextBtn.onClick.AddListener(OnClick_NextBtn);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            //SetMap(MapContainer.MapLastEnter);
        }
        //------------------------------------------------------------------------------------
        private void SetMap(int lastEnter)
        {
            int convertselect = lastEnter - 1;

            int index = convertselect / Define.DisplayMapDataCount; // 5의 범위로 정렬

            index = index % _uIMapGroupElements.Count;

            if (_uIMapGroupElements.Count <= index)
                return;

            UIMapGroupElement uIMapGroupElement = _uIMapGroupElements[index];

            for (int i = 0; i < _uIMapGroupElements.Count; ++i)
            {
                if (i == index)
                {
                    uIMapGroupElement = _uIMapGroupElements[i];
                    uIMapGroupElement.gameObject.SetActive(true);
                }
                else
                    _uIMapGroupElements[i].gameObject.SetActive(false);
            }

            MapData[] mapDatas = Managers.MapManager.Instance.GetDisPlayMapDatas(lastEnter);

            if (mapDatas == null || mapDatas.Length <= 0)
                return;

            if (uIMapGroupElement != null)
                uIMapGroupElement.SetMapGroup(mapDatas);

            _minStage = mapDatas[0].StageNumber;
            _maxStage = mapDatas[mapDatas.Length - 1].StageNumber;

            if (_prevBtn != null)
                _prevBtn.gameObject.SetActive(_minStage > MapOperator.GetMinMapNumber());

            if (_nextBtn != null)
                _nextBtn.gameObject.SetActive(_maxStage < MapOperator.GetMaxMapNumber());
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PrevBtn()
        {
            SetMap(_minStage - 1);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_NextBtn()
        {
            SetMap(_maxStage + 1);
        }
        //------------------------------------------------------------------------------------
    }
}