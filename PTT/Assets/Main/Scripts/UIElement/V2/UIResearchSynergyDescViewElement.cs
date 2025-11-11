using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class UIResearchSynergyDescViewElement : MonoBehaviour
    {
        [SerializeField]
        private List<UIResearchSynergyDescLine> _uIResearchSynergyDescLines = new List<UIResearchSynergyDescLine>();

        public void SetResearchData(ResearchData researchData, int level)
        {
            List<GambleGradeProbData> gambleProbDatas = GambleOperator.GetGambleProbDatas(level);

            V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Max;
            if (researchData.ResearchEffectType == V2Enum_ResearchType.FireGambleLevel)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Red;
            else if (researchData.ResearchEffectType == V2Enum_ResearchType.GoldGambleLevel)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Yellow;
            else if (researchData.ResearchEffectType == V2Enum_ResearchType.WaterGambleLevel)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Blue;
            else if (researchData.ResearchEffectType == V2Enum_ResearchType.ThunderGambleLevel)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.White;

            double totalProb = 0;


            for (int i = 0; i < gambleProbDatas.Count; ++i)
            {
                totalProb += gambleProbDatas[i].GambleProb;
            }

            for (int i = 0; i < gambleProbDatas.Count; ++i)
            {
                UIResearchSynergyDescLine uIResearchSynergyDescLine = null;
                if (_uIResearchSynergyDescLines.Count > i)
                {
                    uIResearchSynergyDescLine = _uIResearchSynergyDescLines[i];
                }
                else
                    break;

                GambleGradeProbData gambleGradeProbData = gambleProbDatas[i];

                string desc = string.Format("{0:0.##}%", (gambleGradeProbData.GambleProb / totalProb) * 100);

                uIResearchSynergyDescLine.gameObject.SetActive(true);
                uIResearchSynergyDescLine.SetSynergyDesc(v2Enum_ARR_SynergyType, gambleGradeProbData.CardGambleGrade.Enum32ToInt() - 10, desc);
            }

            for (int i = gambleProbDatas.Count; i < _uIResearchSynergyDescLines.Count; ++i)
            {
                _uIResearchSynergyDescLines[i].gameObject.SetActive(false);
            }
        }
    }
}