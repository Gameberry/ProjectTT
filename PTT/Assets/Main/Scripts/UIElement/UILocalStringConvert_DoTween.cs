using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UILocalStringConvert_DoTween : MonoBehaviour
    {
        [SerializeField]
        private string LocalizeID = string.Empty;

        private TMP_Text TextTarget;

        [SerializeField]
        private float duration = 1.0f;

        private void Start()
        {
            if (TextTarget == null)
                TextTarget = GetComponent<TMP_Text>();

            if (TextTarget != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(TextTarget, LocalizeID);
        }

        private void OnEnable()
        {
            if (TextTarget == null)
                TextTarget = GetComponent<TMP_Text>();

            if (TextTarget != null)
            {
                TextTarget.DOKill();
                TextTarget.text = "";
                TextTarget.DOText(Managers.LocalStringManager.Instance.GetLocalString(LocalizeID), duration).SetEase(Ease.Linear);
            }
        }
    }
}