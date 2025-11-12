using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class InGameGambleAutoDialog : IDialog
    {
        [SerializeField]
        private List<Image> _autoSettingOrder;

        [SerializeField]
        private List<UICallBackBtnElement> m_elementFilter;
        private List<Enum_SynergyType> m_allyElementFilter = new List<Enum_SynergyType>();

        [SerializeField]
        private Toggle _gambleAutoToggle;

        Dictionary<int, Enum_SynergyType> autoOrder = new Dictionary<int, Enum_SynergyType>();

        [SerializeField]
        private TMP_Text _playAutoCanCount;

        [SerializeField]
        private Button _playAutoGamble;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_gambleAutoToggle != null)
            {
                _gambleAutoToggle.isOn = Managers.GambleManager.Instance.SetStopAllMax;
                _gambleAutoToggle.onValueChanged.AddListener(OnValueChange_AutoGamble);
            }

            if (_playAutoGamble != null)
                _playAutoGamble.onClick.AddListener(OnClick_PlayAuto);

            for (int i = 0; i < Managers.GambleManager.Instance.AutoGambleOrder.Count; ++i)
            {
                Enum_SynergyType Enum_SynergyType = Managers.GambleManager.Instance.AutoGambleOrder[i];

                autoOrder.Add(i, Enum_SynergyType);

                GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(Enum_SynergyType);
                if (gambleCardSprite == null)
                    continue;

                if (_autoSettingOrder.Count > i)
                    _autoSettingOrder[i].sprite = gambleCardSprite.SynergyIcon;
            }


            for (int i = 0; i < m_elementFilter.Count; ++i)
            {
                m_elementFilter[i].SetCallBack(OnClick_ElementFilter);
                m_elementFilter[i].SetEnable(true);
            }

        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {

        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            RefreshEnableBtn();
            Managers.BattleSceneManager.Instance.ChangeTimeScale(Enum_BattleSpeed.Pause);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();
        }
        //------------------------------------------------------------------------------------
        private void RefreshEnableBtn()
        {
            bool canAuto = true;

            int notmaxcount = 0;

            foreach (var pair in autoOrder)
            {
                if (pair.Value == Enum_SynergyType.Max)
                {
                    canAuto = false;
                }
                else
                    notmaxcount++;
            }

            if (_playAutoCanCount != null)
                _playAutoCanCount.SetText("{0}/{1}", notmaxcount, autoOrder.Count);

            if (_playAutoGamble != null)
                _playAutoGamble.interactable = canAuto;
        }
        //------------------------------------------------------------------------------------
        private void OnValueChange_AutoGamble(bool value)
        {
            PlayerPrefs.SetInt(Define.AutoMaxStopKey, value == true ? 1 : 0);
            Managers.GambleManager.Instance.SetStopAllMax = value;
        }
        //------------------------------------------------------------------------------------
        public System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        private void OnClick_PlayAuto()
        {
            bool canAuto = true;

            foreach (var pair in autoOrder)
            {
                if (pair.Value == Enum_SynergyType.Max)
                {
                    canAuto = false;
                    break;
                }
            }

            if (canAuto == false)
                return;

            SerializeString.Clear();

            foreach (var pair in autoOrder)
            {
                if (Managers.GambleManager.Instance.AutoGambleOrder.Count > pair.Key)
                    Managers.GambleManager.Instance.AutoGambleOrder[pair.Key] = pair.Value;

                SerializeString.Append(pair.Value.Enum32ToInt());
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            PlayerPrefs.SetString(Define.AutoGambleKey, SerializeString.ToString());

            Managers.GambleManager.Instance.SetAutoGamble(true);
            ElementExit();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ElementFilter(int element)
        {
            SetElementFilter(element);
        }
        //------------------------------------------------------------------------------------
        private void SetElementFilter(int element)
        {
            Enum_SynergyType v2Enum_ElementType = element.IntToEnum32<Enum_SynergyType>();

            UICallBackBtnElement uICallBackBtnElement = m_elementFilter.Find(x => x.m_myID == element);

            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(v2Enum_ElementType);

            if (autoOrder.ContainsValue(v2Enum_ElementType) == false)
            {
                if (uICallBackBtnElement != null)
                    uICallBackBtnElement.SetEnable(true);

                int idx = -1;

                foreach (var pair in autoOrder)
                {
                    if (pair.Value == Enum_SynergyType.Max)
                    { 
                        idx = pair.Key;
                        break;
                    }
                }

                if (idx != -1 && gambleCardSprite != null)
                {
                    autoOrder[idx] = v2Enum_ElementType;
                    if (_autoSettingOrder.Count > idx)
                    {
                        _autoSettingOrder[idx].gameObject.SetActive(true);
                        _autoSettingOrder[idx].sprite = gambleCardSprite.SynergyIcon;
                    }
                }
            }
            else
            {
                if (uICallBackBtnElement != null)
                    uICallBackBtnElement.SetEnable(false);

                int idx = -1;

                foreach (var pair in autoOrder)
                {
                    if (pair.Value == v2Enum_ElementType)
                        idx = pair.Key;
                }

                if (idx != -1)
                {
                    autoOrder[idx] = Enum_SynergyType.Max;
                    if (_autoSettingOrder.Count > idx)
                    {
                        _autoSettingOrder[idx].gameObject.SetActive(false);
                    }
                }
            }

            RefreshEnableBtn();
        }
        //------------------------------------------------------------------------------------
    }
}