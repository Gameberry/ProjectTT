using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class UIGlobalNoticeElement_SynergyCount : UIGlobalNoticeElement
    {
        [SerializeField]
        protected TMP_Text _beforeStack;

        [SerializeField]
        private List<UIGambleChoiceSkillElement> _uIBeforeSkills = new List<UIGambleChoiceSkillElement>();

        [SerializeField]
        protected TMP_Text _afterStack;

        [SerializeField]
        private List<UIGambleChoiceSkillElement> _uIAfterSkills = new List<UIGambleChoiceSkillElement>();

        [SerializeField]
        private Color _increaseColor;

        [SerializeField]
        private Color _decreaseColor;

        [SerializeField]
        private WaitForSeconds m_turm = new WaitForSeconds(2.0f);

        //------------------------------------------------------------------------------------
        public void ShowNoticeElement_SynergyCount(int before, int after)
        {
            int index = 0;

            if (_beforeStack != null)
                _beforeStack.SetText("+{0}", before);

            if (_afterStack != null)
            { 
                _afterStack.SetText("+{0}", after);
                _afterStack.color = before < after ? _increaseColor : _decreaseColor;
            }

            if (m_messageCanvasGroup != null)
                m_messageCanvasGroup.alpha = 1.0f;

            gameObject.SetActive(true);

            for (int i = V2Enum_ARR_SynergyType.Red.Enum32ToInt(); i < V2Enum_ARR_SynergyType.Max.Enum32ToInt(); ++i)
            {
                V2Enum_ARR_SynergyType v2Enum_Stat = i.IntToEnum32<V2Enum_ARR_SynergyType>();

                GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(v2Enum_Stat);

                if (_uIBeforeSkills.Count > index)
                {
                    _uIBeforeSkills[index].SetStack(before, gambleCardSprite.SynergyIcon);
                }

                if (_uIAfterSkills.Count > index)
                {
                    _uIAfterSkills[index].SetStack(after, gambleCardSprite.SynergyIcon);
                }

                index++;
            }

            if (HideDirectionCoroutine != null)
                StopCoroutine(HideDirectionCoroutine);

            HideDirectionCoroutine = StartCoroutine(HideDirection(m_turm));
        }
    }
}