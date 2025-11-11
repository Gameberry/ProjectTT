using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIStatDetailElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_statDetailText;

        [SerializeField]
        private TMP_Text m_statDetailValue;

        [SerializeField]
        private TMP_Text m_statBunusValue;

        private CharacterBaseStatData m_myStatData;

        private CreatureStatController m_creatureStatController;
        private V2Enum_OnlyStatDetailView m_v2Enum_OnlyStatDetailView = V2Enum_OnlyStatDetailView.Max;
        private bool m_isOnlyStatDetailView = false;

        //------------------------------------------------------------------------------------
        public void SetOnlyStatDetailView(CreatureStatController creatureStatController, V2Enum_OnlyStatDetailView v2Enum_OnlyStatDetailView)
        {
            switch (m_v2Enum_OnlyStatDetailView)
            {
                case V2Enum_OnlyStatDetailView.FinalAttack:
                    {
                        creatureStatController.RemoveStatRefrashEvent(V2Enum_Stat.Attack, SetFinalViewValue);
                        break;
                    }
                case V2Enum_OnlyStatDetailView.FinalHP:
                    {
                        creatureStatController.RemoveStatRefrashEvent(V2Enum_Stat.HP, SetFinalViewValue);
                        break;
                    }
                case V2Enum_OnlyStatDetailView.FinalDefence:
                    {
                        creatureStatController.RemoveStatRefrashEvent(V2Enum_Stat.Defence, SetFinalViewValue);
                        break;
                    }
                case V2Enum_OnlyStatDetailView.FinalCriticalDamage:
                    {
                        creatureStatController.RemoveStatRefrashEvent(V2Enum_Stat.Attack, SetFinalViewValue);
                        break;
                    }
                case V2Enum_OnlyStatDetailView.FinalSuperCriticalDamage:
                    {
                        creatureStatController.RemoveStatRefrashEvent(V2Enum_Stat.Attack, SetFinalViewValue);
                        break;
                    }
                case V2Enum_OnlyStatDetailView.FinalHyperCriticalDamage:
                    {
                        creatureStatController.RemoveStatRefrashEvent(V2Enum_Stat.Attack, SetFinalViewValue);
                        break;
                    }
            }


            m_creatureStatController = creatureStatController;
            m_isOnlyStatDetailView = true;
            m_v2Enum_OnlyStatDetailView = v2Enum_OnlyStatDetailView;



            switch (m_v2Enum_OnlyStatDetailView)
            {
                case V2Enum_OnlyStatDetailView.FinalAttack:
                    {
                        if (m_statDetailText != null)
                        {
                            Managers.LocalStringManager.Instance.SetLocalizeText(m_statDetailText, "characterStatSummary/1/name");
                        }

                        creatureStatController.AddStatRefrashEvent(V2Enum_Stat.Attack, SetFinalViewValue);
                        break;
                    }
                case V2Enum_OnlyStatDetailView.FinalHP:
                    {
                        if (m_statDetailText != null)
                        {
                            Managers.LocalStringManager.Instance.SetLocalizeText(m_statDetailText, "characterStatSummary/2/name");
                        }

                        creatureStatController.AddStatRefrashEvent(V2Enum_Stat.HP, SetFinalViewValue);
                        break;
                    }
                case V2Enum_OnlyStatDetailView.FinalDefence:
                    {
                        if (m_statDetailText != null)
                        {
                            Managers.LocalStringManager.Instance.SetLocalizeText(m_statDetailText, "characterStatSummary/3/name");
                        }

                        creatureStatController.AddStatRefrashEvent(V2Enum_Stat.Defence, SetFinalViewValue);
                        break;
                    }
                case V2Enum_OnlyStatDetailView.FinalCriticalDamage:
                    {
                        if (m_statDetailText != null)
                        {
                            Managers.LocalStringManager.Instance.SetLocalizeText(m_statDetailText, "characterStatSummary/4/name");
                        }

                        creatureStatController.AddStatRefrashEvent(V2Enum_Stat.Attack, SetFinalViewValue);
                        break;
                    }
                case V2Enum_OnlyStatDetailView.FinalSuperCriticalDamage:
                    {
                        if (m_statDetailText != null)
                        {
                            Managers.LocalStringManager.Instance.SetLocalizeText(m_statDetailText, "characterStatSummary/5/name");
                        }

                        creatureStatController.AddStatRefrashEvent(V2Enum_Stat.Attack, SetFinalViewValue);
                        break;
                    }
                case V2Enum_OnlyStatDetailView.FinalHyperCriticalDamage:
                    {
                        if (m_statDetailText != null)
                        {
                            Managers.LocalStringManager.Instance.SetLocalizeText(m_statDetailText, "characterStatSummary/6/name");
                        }

                        creatureStatController.AddStatRefrashEvent(V2Enum_Stat.Attack, SetFinalViewValue);
                        break;
                    }
            }

            SetFinalViewValue(0.0);
        }
        //------------------------------------------------------------------------------------
        public void SetFinalViewValue(double value)
        {
            if (m_statDetailValue != null)
            {
                double statvalue = 0.0;

                switch (m_v2Enum_OnlyStatDetailView)
                {
                    case V2Enum_OnlyStatDetailView.FinalAttack:
                        {
                            statvalue = m_creatureStatController.GetFinalAttack();
                            break;
                        }
                    case V2Enum_OnlyStatDetailView.FinalHP:
                        {
                            statvalue = m_creatureStatController.GetFinalHP();
                            break;
                        }
                    case V2Enum_OnlyStatDetailView.FinalDefence:
                        {
                            statvalue = m_creatureStatController.GetFinalDefence();
                            break;
                        }
                    case V2Enum_OnlyStatDetailView.FinalCriticalDamage:
                        {
                            statvalue = m_creatureStatController.GetFinalCriticalDamage();
                            break;
                        }
                }

                m_statDetailValue.text = System.Math.Round(statvalue).ToString("N0");
            }
        }
        //------------------------------------------------------------------------------------
        public void SetStatData(CharacterBaseStatData statdata)
        {
            m_myStatData = statdata;
            m_isOnlyStatDetailView = false;
            if (m_statDetailText != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(m_statDetailText, m_myStatData.NameLocalStringKey);
            }
        }
        //------------------------------------------------------------------------------------
        public void RefrashValue(double value)
        {
            if (m_statDetailValue != null)
            {
                if (m_myStatData.VisibleType == V2Enum_PrintType.Percent)
                {
                    m_statDetailValue.text = string.Format("{0:0.0}%", value * Define.PerStatPrintRecoverValueTemp);
                }
                else
                {
                    m_statDetailValue.text = System.Math.Round(value).ToString("N0");
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void ShowBunusValue(double value)
        {
            if (m_statBunusValue != null)
            {
                if (m_myStatData.VisibleType == V2Enum_PrintType.Percent)
                {
                    m_statBunusValue.text = string.Format("+{0:0.0}%", value * Define.PerStatPrintRecoverValueTemp);
                }
                else
                {
                    m_statBunusValue.text = string.Format("+{0}", System.Math.Round(value).ToString("N0"));
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void EnableBonusValue(bool enable)
        {
            if (m_statBunusValue != null)
            { 
                m_statBunusValue.enabled = enable;
            }
        }
        //------------------------------------------------------------------------------------
    }
}