using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UILobbySynergyUnLockNoticeElement : MonoBehaviour
    {
        [SerializeField]
        private UILobbySynergyEffectElement uILobbySynergyEffectElement;

        [SerializeField]
        private TMP_Text desc;

        public bool JustShowLevel = false;

        public void SetElement(SynergyEffectData target, int level)
        {
            if (uILobbySynergyEffectElement != null)
            { 
                uILobbySynergyEffectElement.SetSynergyEffectData(target);
                uILobbySynergyEffectElement.EnableSelectElement(false);
                uILobbySynergyEffectElement.EnableLockImage(false);
            }

            if (desc != null)
            {
                if (JustShowLevel == false)
                    desc.SetText(Managers.SynergyManager.Instance.GetNeedLevelString(target, level));
                else
                    desc.SetText("Lv.{0}", level);
            }
        }
    }
}
