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

        public void SetSynergyDesc(Enum_SynergyType Enum_SynergyType, int stack, string desc)
        {
            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(Enum_SynergyType);

            if (_uIGambleChoiceSkillElement != null)
            { 
                _uIGambleChoiceSkillElement.SetStack(stack, gambleCardSprite.SynergyIcon);
            }

            if (_percentDesc != null)
                _percentDesc.SetText(desc);
        }
    }
}