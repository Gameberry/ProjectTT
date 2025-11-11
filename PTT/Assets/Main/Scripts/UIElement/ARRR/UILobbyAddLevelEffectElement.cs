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
    public class UILobbyAddLevelEffectElement : MonoBehaviour
    {
        [SerializeField]
        private List<UnityEngine.UI.Extensions.Gradient> _grageBG = new List<UnityEngine.UI.Extensions.Gradient>();

        [SerializeField]
        private TMP_Text _grage;

        [SerializeField]
        private Image _gradeTextImage;

        [SerializeField]
        private TMP_Text _level;

        [SerializeField]
        private TMP_Text _exp;

        [SerializeField]
        private Color _levelEnableColor;


        [SerializeField]
        private TMP_Text _desc;

        [SerializeField]
        private Color _descEnableColor;

        [SerializeField]
        private Color _descDisaEnableColor;

        [SerializeField]
        private Image _dimmed;

        public void SetSynergyLevelEffectData(SynergyLevelEffectData synergyLevelEffectData, int synergyLevel)
        {
            if (synergyLevelEffectData == null)
                return;

            if (_level != null)
            {
                _level.SetText("+{0}", synergyLevelEffectData.RequiredLevel);
                _level.color = synergyLevelEffectData.RequiredLevel <= synergyLevel ? _levelEnableColor : Color.gray;
            }

            if (_desc != null)
            { 
                Managers.LocalStringManager.Instance.SetLocalizeText(_desc, synergyLevelEffectData.DescLocalKey);
                _desc.color = synergyLevelEffectData.RequiredLevel <= synergyLevel ? _descEnableColor : Color.gray;
            }
        }

        public void SetSynergyLevelEffectData(DescendBreakthroughData synergyLevelEffectData, int synergyLevel)
        {
            if (synergyLevelEffectData == null)
                return;

            if (_level != null)
            {
                _level.SetText("+{0}", synergyLevelEffectData.Procedure);
                _level.color = synergyLevelEffectData.Procedure <= synergyLevel ? _levelEnableColor : Color.gray;
            }

            if (_desc != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(_desc, synergyLevelEffectData.DescLocalKey);
                _desc.color = synergyLevelEffectData.Procedure <= synergyLevel ? _descEnableColor : Color.gray;
            }
        }

        public void SetInGameLobbyDesc(string local, int synergyLevel)
        {
            if (_level != null)
            {
                _level.SetText("Lv.{0}", synergyLevel);
                _level.color = _levelEnableColor;
            }

            if (_desc != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(_desc, local);
                _desc.color = _descEnableColor;
            }
        }

        public void SetGearDesc(string local, V2Enum_Grade v2Enum_Grade)
        {
            SetGradeColor(v2Enum_Grade);

            if (_desc != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(_desc, local);
            }


        }

        public void SetEnableDimmedImage(bool enable)
        {
            //if (_desc != null)
            //    _desc.color = enable == true ? _descEnableColor : _descDisaEnableColor;

            if (_dimmed != null)
                _dimmed.gameObject.SetActive(enable);
        }

        public void SetGradeColor(V2Enum_Grade v2Enum_Grade)
        {
            V2Enum_Grade myV2Enum_Grade = v2Enum_Grade;

            for (int i = 0; i < _grageBG.Count; ++i)
            {
                _grageBG[i].SetGrade(v2Enum_Grade);
            }

            if (_grage != null)
            {
                _grage.gameObject.SetActive(true);
                _grage.SetGrade(v2Enum_Grade);
            }

            if (_gradeTextImage != null)
                _gradeTextImage.SetGradeTextImage(v2Enum_Grade);
        }
    }
}