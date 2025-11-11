using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIBattleWaveIconElement : MonoBehaviour
    {
        [SerializeField]
        private List<Transform> _iconGroup = new List<Transform>();

        [SerializeField]
        private Image _seashell;

        public void SetIcon(int index)
        {
            if (_iconGroup.Count <= index)
                return;

            for (int i = 0; i < _iconGroup.Count; ++i)
            {
                _iconGroup[i].gameObject.SetActive(i == index);
            }
        }

        public void EnableSeashell(bool enable)
        {
            if (_seashell != null)
                _seashell.gameObject.SetActive(enable);
        }
    }
}