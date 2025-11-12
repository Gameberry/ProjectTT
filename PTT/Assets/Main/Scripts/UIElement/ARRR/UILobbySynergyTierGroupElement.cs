using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using Gpm.Ui;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UILobbySynergyTierGroupElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _synergyLevel;

        [SerializeField]
        private Image _tierEquipEffectIcon;

        [SerializeField]
        private Transform _synergyLock;


        [Header("-------SynergyEffect-------")]
        [SerializeField]
        private Transform _synergyListRoot;

        [SerializeField]
        private HorizontalLayoutGroup _synergyHorizontalLayout;

        [SerializeField]
        private UILobbySynergyEffectElement _uIGambleSynergyViewElement;

        private Dictionary<SynergyEffectData, UILobbySynergyEffectElement> _uIGambleSynergyViewElement_dic = new Dictionary<SynergyEffectData, UILobbySynergyEffectElement>();

        private List<UILobbySynergyEffectElement> _uILobbySynergyEffectElements = new List<UILobbySynergyEffectElement>();

        private System.Action<SynergyEffectData> _callBack;

        private Enum_SynergyType _currentSynergyType = Enum_SynergyType.Max;
        private ObscuredInt _currentTier;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<SynergyEffectData> action)
        {
            _callBack = action;
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyTierGroup(Enum_SynergyType Enum_SynergyType, ObscuredInt tier, List<SynergyEffectData> effectDatas)
        {
            _uIGambleSynergyViewElement_dic.Clear();

            if (effectDatas == null)
                return;

            _currentSynergyType = Enum_SynergyType;
            _currentTier = tier;

            if (_synergyLevel != null)
                _synergyLevel.SetText("{0}", tier);

            //if (_synergyLock != null)
            //    _synergyLock.gameObject.SetActive(tier <= Managers.SynergyManager.Instance.SynergyUnLockLevel);

            for (int i = 0; i < effectDatas.Count; ++i)
            {
                UILobbySynergyEffectElement uILobbySynergyEffectElement = null;

                SynergyEffectData synergyEffectData = effectDatas[i];

                if (_uILobbySynergyEffectElements.Count > i)
                    uILobbySynergyEffectElement = _uILobbySynergyEffectElements[i];
                else
                {
                    GameObject clone = Instantiate(_uIGambleSynergyViewElement.gameObject, _synergyListRoot);

                    uILobbySynergyEffectElement = clone.GetComponent<UILobbySynergyEffectElement>();
                    uILobbySynergyEffectElement.Init(_callBack);

                    _uILobbySynergyEffectElements.Add(uILobbySynergyEffectElement);
                }

                uILobbySynergyEffectElement.SetSynergyEffectData(synergyEffectData);
                uILobbySynergyEffectElement.gameObject.SetActive(true);

                _uIGambleSynergyViewElement_dic.Add(synergyEffectData, uILobbySynergyEffectElement);
            }

            for (int i = effectDatas.Count; i < _uILobbySynergyEffectElements.Count; ++i)
            {
                _uILobbySynergyEffectElements[i].gameObject.SetActive(false);
            }

            RefreshEquipEffectIcon();
        }
        //------------------------------------------------------------------------------------
        public UILobbySynergyEffectElement GetUILobbySynergyEffectElement(SynergyEffectData synergyEffectData)
        {
            if (_uIGambleSynergyViewElement_dic.ContainsKey(synergyEffectData) == false)
                return null;

            return _uIGambleSynergyViewElement_dic[synergyEffectData];
        }
        //------------------------------------------------------------------------------------
        public void RefreshEquipEffectIcon()
        {
            if (_tierEquipEffectIcon != null)
            {
                SynergyEffectData synergyEffectData = Managers.SynergyManager.Instance.GetEquipSynergyEffect(_currentSynergyType, _currentTier);
                if (synergyEffectData == null)
                    return;

                _tierEquipEffectIcon.sprite = Managers.SynergyManager.Instance.GetSynergySprite(synergyEffectData);
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshSynergyEffect(SynergyEffectData synergyEffectData)
        {
            if (_uIGambleSynergyViewElement_dic.ContainsKey(synergyEffectData) == true)
            {
                _uIGambleSynergyViewElement_dic[synergyEffectData].SetSynergyEffectData(synergyEffectData);
            }
        }
        //------------------------------------------------------------------------------------
        public void EnableSelectEelement(SynergyEffectData synergyEffectData, bool enable)
        {
            if (_uIGambleSynergyViewElement_dic.ContainsKey(synergyEffectData) == true)
            {
                _uIGambleSynergyViewElement_dic[synergyEffectData].EnableSelectElement(enable);
            }
        }
        //------------------------------------------------------------------------------------
    }
}