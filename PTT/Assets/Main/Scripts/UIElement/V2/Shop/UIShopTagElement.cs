using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopTagElement : MonoBehaviour
    {
        [SerializeField]
        private Transform _tagGroup;

        [SerializeField]
        private TMP_Text _tagString;
        //------------------------------------------------------------------------------------
        public void SetTag(string tag)
        {
            if (_tagGroup != null)
            {
                if (tag == "-1")
                {
                    _tagGroup.gameObject.SetActive(false);
                }
                else
                {
                    _tagGroup.gameObject.SetActive(true);

                    if (_tagString != null)
                        Managers.LocalStringManager.Instance.SetLocalizeText(_tagString, tag);
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}