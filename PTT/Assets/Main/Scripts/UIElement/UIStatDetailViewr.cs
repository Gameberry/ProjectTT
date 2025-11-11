using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class UIStatDetailViewr : MonoBehaviour
    {
        [SerializeField]
        private RectTransform m_uIStatDetailViewrRectTrans;

        [SerializeField]
        private UIStatDetailElement m_statDetailElement;

        [SerializeField]
        private Button m_statViewChange;

        [SerializeField]
        private Image m_closeDetailViewImg;

        [SerializeField]
        private Transform m_elementRoot;

        private bool m_isMiniWindow = true;

        [SerializeField]
        private float m_miniWindowHeight = 100.0f;

        [SerializeField]
        private float m_extensionWindowHeight = 500.0f;

        private CreatureStatController m_creatureStatController;

        private Dictionary<V2Enum_OnlyStatDetailView, UIStatDetailElement> m_uIStatFinalDetailElement = new Dictionary<V2Enum_OnlyStatDetailView, UIStatDetailElement>();

        private Dictionary<V2Enum_Stat, UIStatDetailElement> m_uIStatDetailElement = new Dictionary<V2Enum_Stat, UIStatDetailElement>();
        //------------------------------------------------------------------------------------
        public void Init()
        {
            if (m_statViewChange != null)
                m_statViewChange.onClick.AddListener(OnClick_StatViewChange);
        }
        //------------------------------------------------------------------------------------
        public void SetDetailViewr(CreatureStatController creatureStatController)
        {
            if (creatureStatController == null)
                return;

            m_creatureStatController = creatureStatController;

            for(int i = 0; i < V2Enum_OnlyStatDetailView.Max.Enum32ToInt(); ++i)
            {
                UIStatDetailElement uIStatDetailElement = null;

                V2Enum_OnlyStatDetailView v2Enum_OnlyStatDetailView = i.IntToEnum32<V2Enum_OnlyStatDetailView>();

                if (m_uIStatFinalDetailElement.TryGetValue(v2Enum_OnlyStatDetailView, out uIStatDetailElement) == false)
                {
                    GameObject clone = Instantiate(m_statDetailElement.gameObject, m_elementRoot);
                    uIStatDetailElement = clone.GetComponent<UIStatDetailElement>();
                    m_uIStatFinalDetailElement.Add(v2Enum_OnlyStatDetailView, uIStatDetailElement);
                }

                uIStatDetailElement.transform.SetAsLastSibling();
                uIStatDetailElement.SetOnlyStatDetailView(m_creatureStatController, v2Enum_OnlyStatDetailView);
            }

            foreach (KeyValuePair<V2Enum_Stat, ObscuredDouble> pair in creatureStatController.m_defaultStatValues)
            {
                UIStatDetailElement uIStatDetailElement = null;
                if (m_uIStatDetailElement.TryGetValue(pair.Key, out uIStatDetailElement) == false)
                {
                    GameObject clone = Instantiate(m_statDetailElement.gameObject, m_elementRoot);
                    uIStatDetailElement = clone.GetComponent<UIStatDetailElement>();
                    m_uIStatDetailElement.Add(pair.Key, uIStatDetailElement);
                }
                else
                {
                    creatureStatController.RemoveStatRefrashEvent(pair.Key, uIStatDetailElement.RefrashValue);
                }

                uIStatDetailElement.transform.SetAsLastSibling();
                uIStatDetailElement.SetStatData(Managers.ARRRStatManager.Instance.GetCharacterBaseStatData(pair.Key));
                uIStatDetailElement.RefrashValue(creatureStatController.GetOutPutStatValue(pair.Key));
                creatureStatController.AddStatRefrashEvent(pair.Key, uIStatDetailElement.RefrashValue);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetDetailViewr(Dictionary<V2Enum_Stat, ObscuredDouble> stat, System.Action onCloseCallBack)
        {
            foreach (KeyValuePair<V2Enum_Stat, ObscuredDouble> pair in stat)
            {
                UIStatDetailElement uIStatDetailElement = null;
                if (m_uIStatDetailElement.TryGetValue(pair.Key, out uIStatDetailElement) == false)
                {
                    GameObject clone = Instantiate(m_statDetailElement.gameObject, m_elementRoot);
                    uIStatDetailElement = clone.GetComponent<UIStatDetailElement>();
                    m_uIStatDetailElement.Add(pair.Key, uIStatDetailElement);
                }
                else
                {
                }

                uIStatDetailElement.transform.SetAsLastSibling();
                uIStatDetailElement.SetStatData(Managers.ARRRStatManager.Instance.GetCharacterBaseStatData(pair.Key));
                uIStatDetailElement.RefrashValue(pair.Value);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_StatViewChange()
        {
            if (m_uIStatDetailViewrRectTrans != null)
            {
                Vector2 sizedelta = m_uIStatDetailViewrRectTrans.sizeDelta;
                sizedelta.y = m_isMiniWindow == true ? m_extensionWindowHeight : m_miniWindowHeight;

                m_uIStatDetailViewrRectTrans.sizeDelta = sizedelta;

                //Vector3 rotate = m_statViewChange.transform.localEulerAngles;
                //rotate.z = m_isMiniWindow == true ? 45.0f : 0.0f;
                //m_statViewChange.transform.localEulerAngles = rotate;

                m_isMiniWindow = !m_isMiniWindow;
            }

            if (m_closeDetailViewImg != null)
                m_closeDetailViewImg.gameObject.SetActive(m_isMiniWindow == false);
        }
        //------------------------------------------------------------------------------------
        public void SetMiniWindow()
        {
            if (m_isMiniWindow == false)
                OnClick_StatViewChange();
        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            if (m_creatureStatController == null)
                return;

            foreach (KeyValuePair<V2Enum_Stat, ObscuredDouble> pair in m_creatureStatController.m_defaultStatValues)
            {
                UIStatDetailElement uIStatDetailElement = null;
                if (m_uIStatDetailElement.TryGetValue(pair.Key, out uIStatDetailElement) == true)
                {
                    m_creatureStatController.RemoveStatRefrashEvent(pair.Key, uIStatDetailElement.RefrashValue);
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}