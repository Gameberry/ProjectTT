using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UISettingToggleElement : MonoBehaviour
    {
        [SerializeField]
        private Image _onImage;

        [SerializeField]
        private Image _offImage;

        [SerializeField]
        private Button m_settingToggleBtn;

        [SerializeField]
        private Managers.GameSettingBtn m_gameSettingBtnID = Managers.GameSettingBtn.Max;

        [SerializeField]
        private bool m_needCallListener = false;

        public void Init()
        {
            if (m_settingToggleBtn != null)
                m_settingToggleBtn.onClick.AddListener(OnClick_SettingToggleBtn);

            RefreshElement();

            if (m_needCallListener == true)
            {
                Managers.GameSettingManager.Instance.AddListener(m_gameSettingBtnID, RefreshElement);
            }
        }

        private void OnDestroy()
        {
            if (Managers.GameSettingManager.isAlive == true)
            {
                Managers.GameSettingManager.Instance.RemoveListener(m_gameSettingBtnID, RefreshElement);
            }
        }

        public void RefreshElement()
        {
            bool isOn = Managers.GameSettingManager.Instance.IsOn(m_gameSettingBtnID);

            if (_onImage != null)
                _onImage.gameObject.SetActive(isOn);

            if (_offImage != null)
                _offImage.gameObject.SetActive(isOn == false);
        }

        private void OnClick_SettingToggleBtn()
        {
            Managers.GameSettingManager.Instance.ChangeGameOption(m_gameSettingBtnID);
            RefreshElement();
        }
    }
}