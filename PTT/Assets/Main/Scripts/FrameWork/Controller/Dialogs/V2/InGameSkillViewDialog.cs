using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
    public class InGameSkillViewDialog : IDialog
    {
        [Header("----------CoolTimeGroup----------")]
        [SerializeField]
        private UIARRRSkillCoolTimeGroup _uIARRRSkillCoolTimeGroup;

        [Header("----------GambleGet----------")]
        [SerializeField]
        private Button _showGambleSkill;

        [SerializeField]
        private Image _showGambleSkill_Check;

        [SerializeField]
        private UIARRRGambleAddSkillGroup _uIARRRGambleAddSkillGroup;

        private bool _enableGambleSkill = false;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_showGambleSkill != null)
                _showGambleSkill.onClick.AddListener(OnClick_EnableGambleSkill);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            ARRRController aRRRController = Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers;
            if (_uIARRRSkillCoolTimeGroup != null)
                _uIARRRSkillCoolTimeGroup.SetSkillActiveController(aRRRController.SkillActiveController);

            if (_uIARRRGambleAddSkillGroup != null)
                _uIARRRGambleAddSkillGroup.SetSkillActiveController(aRRRController.SkillActiveController);

            _enableGambleSkill = false;
            RefreshShowGambleSkill();
        }
        //------------------------------------------------------------------------------------
        private void RefreshShowGambleSkill()
        {
            if (_showGambleSkill_Check != null)
                _showGambleSkill_Check.gameObject.SetActive(_enableGambleSkill);

            if (_uIARRRGambleAddSkillGroup != null)
                _uIARRRGambleAddSkillGroup.gameObject.SetActive(_enableGambleSkill);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_EnableGambleSkill()
        {
            _enableGambleSkill = !_enableGambleSkill;
            RefreshShowGambleSkill();
        }
        //------------------------------------------------------------------------------------
    }
}