using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GameBerry.UI
{
    public class UILocalStringConvert : MonoBehaviour
    {
        [SerializeField]
        private string LocalizeID = string.Empty;

        private TMP_Text _TextTarget;

        private void Start()
        {
            if (_TextTarget == null)
                _TextTarget = GetComponent<TMP_Text>();

            if (_TextTarget != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_TextTarget, LocalizeID);
        }

        public void SetLocalizeID(string localizeID)
        {
            LocalizeID = localizeID;

            if (_TextTarget == null)
                _TextTarget = GetComponent<TMP_Text>();

            if (_TextTarget != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_TextTarget, LocalizeID);
        }
    }
}