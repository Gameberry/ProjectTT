using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UISynergyCombineSkillElement : MonoBehaviour
    {
        [SerializeField]
        private UISkillIconElement _uISkillIconElement;

        [SerializeField]
        private TMP_Text _percent;

        [SerializeField]
        private Image _alReadySkill;

        [SerializeField]
        private Image _clickFrame;

        [SerializeField]
        private Button _uIElementClick;

        private System.Action<SynergyCombineData> _callback = null;
        private SynergyCombineData _currentGambleSynergyCombineData = null;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_uIElementClick != null)
                _uIElementClick.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        public void Init(System.Action<SynergyCombineData> action)
        {
            _callback = action;
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyDetailView(SynergyCombineData gambleSynergyCombineData)
        {
            _currentGambleSynergyCombineData = gambleSynergyCombineData;
            SetSkillGambleSkill(gambleSynergyCombineData.SynergySkillData);
        }
        //------------------------------------------------------------------------------------
        private void SetSkillGambleSkill(MainSkillData gambleSkillData)
        {
            if (gambleSkillData == null)
                return;

            if (_uISkillIconElement != null)
                _uISkillIconElement.SetSkillElement(gambleSkillData);
        }
        //------------------------------------------------------------------------------------
        public void RefreshElement()
        {
            if (_currentGambleSynergyCombineData == null)
                return;

            bool isAlReady = Managers.SynergyManager.Instance.IsAlReadySynergyCombineSkill(_currentGambleSynergyCombineData);

            if (_percent != null)
            {
                if (isAlReady == true)
                    _percent.SetText("-");
                else
                    _percent.SetText("{0:0}%", Managers.SynergyManager.Instance.GetSynergyCombineSkillReadyRatio(_currentGambleSynergyCombineData) * 100.0f);
            }

            if (_alReadySkill != null)
                _alReadySkill.gameObject.SetActive(isAlReady);
        }
        //------------------------------------------------------------------------------------
        public void SetClickEnable(bool enable)
        {
            if (_clickFrame != null)
                _clickFrame.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            _callback?.Invoke(_currentGambleSynergyCombineData);
        }
        //------------------------------------------------------------------------------------
    }
}