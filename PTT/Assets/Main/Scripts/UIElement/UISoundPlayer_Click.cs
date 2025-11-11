using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry
{
    public class UISoundPlayer_Click : MonoBehaviour
    {
        private string soundName1 = "Button 5";

        private void Start()
        {
            Button btn = GetComponent<Button>();

            if (btn != null)
                btn.onClick.AddListener(PlaySound);
        }

        private void PlaySound()
        {
            if (Managers.SoundManager.isAlive == true)
            {
                Managers.SoundManager.Instance.PlaySound(soundName1);
            }
        }
    }
}