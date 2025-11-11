using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIPassElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_passName;

        [SerializeField]
        private Transform _passSelected;

        [SerializeField]
        private Button m_enterPass;

        [SerializeField]
        private UIRedDotElement m_uiRedDotElement = null;

        [SerializeField]
        private UIContentUnlockElement _uIContentUnlockElement = null;
        

        private PassData m_focusData = null;
        private V2Enum_PassType m_currentPassType;

        private System.Action<V2Enum_PassType> m_action = null;

        private bool m_isInitReddot = false;
        private bool m_isInitContentUnLock = false;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<V2Enum_PassType> action)
        {
            if (m_enterPass != null)
                m_enterPass.onClick.AddListener(OnClick_EnterPass);

            m_action = action;
        }
        //------------------------------------------------------------------------------------
        public void SetPassElement(V2Enum_PassType v2Enum_PassType)
        {
            if (m_isInitReddot == false)
            {
                if (m_uiRedDotElement != null)
                {
                    m_uiRedDotElement.AddRecvRedDotType(Managers.PassManager.Instance.ConvertPassTypeToContentDetail(v2Enum_PassType));
                    m_uiRedDotElement.Init();

                }

                m_isInitReddot = true;
            }

            m_currentPassType = v2Enum_PassType;

            m_focusData = Managers.PassManager.Instance.GetPassFocusData(v2Enum_PassType);

            if (m_focusData == null)
                return;

            if (m_passName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_passName, m_focusData.TitleLocalStringKey);

            if (m_isInitContentUnLock == false)
            {
                if (_uIContentUnlockElement != null)
                {
                    if (v2Enum_PassType == V2Enum_PassType.DescendLevel)
                        SetContentUnLock(V2Enum_ContentType.DescendPass);
                    else if (v2Enum_PassType == V2Enum_PassType.MonsterKill)
                        SetContentUnLock(V2Enum_ContentType.HuntingPass);
                    else
                        _uIContentUnlockElement.gameObject.SetActive(false);
                }

                m_isInitContentUnLock = true;
            }

            

            

            //RefreshPassElement();
        }
        //------------------------------------------------------------------------------------
        public void SetSelected(bool selected)
        {
            if (_passSelected != null)
                _passSelected.gameObject.SetActive(selected);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_EnterPass()
        {
            m_action?.Invoke(m_currentPassType);
        }
        //------------------------------------------------------------------------------------
        public void SetContentUnLock(V2Enum_ContentType v2Enum_ContentType)
        {
            if (_uIContentUnlockElement != null)
            {
                _uIContentUnlockElement.m_v2Enum_ContentType = v2Enum_ContentType;
                _uIContentUnlockElement.Init();
            }
        }
        //------------------------------------------------------------------------------------
        public UIGuideInteractor SetPassGuideBtn()
        {
            if (m_enterPass != null)
            {
                UIGuideInteractor uIGuideInteractor = m_enterPass.gameObject.AddComponent<UIGuideInteractor>();
                uIGuideInteractor.MyGuideType = V2Enum_EventType.PassFreeRewardClaim;
                uIGuideInteractor.MyStepID = 2;
                uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.Next;
                uIGuideInteractor.IsAutoSetting = false;
                uIGuideInteractor.ConnectInteractor();

                return uIGuideInteractor;
            }

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}