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

        private TMP_Text TextTarget;

        private void Start()
        {
            if (TextTarget == null)
                TextTarget = GetComponent<TMP_Text>();

            if (TextTarget != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(TextTarget, LocalizeID);
        }

        public void SetLocalizeID(string localizeID)
        {
            LocalizeID = localizeID;

            if (TextTarget == null)
                TextTarget = GetComponent<TMP_Text>();

            if (TextTarget != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(TextTarget, LocalizeID);
        }
    }
}