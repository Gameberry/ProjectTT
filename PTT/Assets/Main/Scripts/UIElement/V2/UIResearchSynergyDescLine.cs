using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class UIResearchSynergyDescLine : MonoBehaviour
    {
        [SerializeField]
        private UIGambleChoiceSkillElement _uIGambleChoiceSkillElement;

        [SerializeField]
        private TMP_Text _percentDesc;

        public void SetSynergyDesc(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType, int stack, string desc)
        {
            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(v2Enum_ARR_SynergyType);

            if (_uIGambleChoiceSkillElement != null)
            { 
                _uIGambleChoiceSkillElement.SetStack(stack, gambleCardSprite.SynergyIcon);
            }

            if (_percentDesc != null)
                _percentDesc.SetText(desc);
        }
    }
}