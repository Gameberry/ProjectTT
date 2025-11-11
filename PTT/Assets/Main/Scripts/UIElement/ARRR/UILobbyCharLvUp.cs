using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UILobbyCharLvUp : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _setLevelText;

        [SerializeField]
        private Animation _uiLobbyCharAnimation;

        public void SetText(string text)
        {
            if (_setLevelText != null)
                _setLevelText.SetText(text);
        }

        public void PlayEffect()
        {
            if (_uiLobbyCharAnimation != null)
            {
                _uiLobbyCharAnimation.Stop();
                _uiLobbyCharAnimation.Play();
            }
        }
    }
}