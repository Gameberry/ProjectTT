using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIARRRSkillEffectDescElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _uISkillEffect_Desc;

        public void SetEffectDesc(SkillEffectData skillEffectData, int level = 0)
        {
            if (_uISkillEffect_Desc == null)
                return;

            _uISkillEffect_Desc.SetText(Managers.SkillManager.Instance.GetEffectLocalString(skillEffectData, level));
        }
    }
}