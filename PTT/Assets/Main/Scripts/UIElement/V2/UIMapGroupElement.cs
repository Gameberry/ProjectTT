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
    public class UIMapGroupElement : MonoBehaviour
    {
        [SerializeField] private List<UIMapElement> _uIMapElements = new List<UIMapElement>();

        public void SetMapGroup(MapData[] mapDatas)
        {
            if (mapDatas == null)
                return;

            for (int i = 0; i < _uIMapElements.Count; ++i)
            {
                if (i < mapDatas.Length)
                    _uIMapElements[i].SetMapData(mapDatas[i]);
                else
                    _uIMapElements[i].SetMapData(null);
            }
        }
    }
}