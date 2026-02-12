using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Chart;

namespace GameBerry.UI
{
    public class UISkillJobGroup : MonoBehaviour
    {
        [SerializeField] private TMP_Text _jobName;
        [SerializeField] private UISkillElement _uISkillElement;
        [SerializeField] private Transform _uIPassiveSkillRoot;
        [SerializeField] private Transform _uIActiveSkillRoot;

        [SerializeField]
        private Vector2 _passiveSize = 140.0f.ToVector2();

        [SerializeField]
        private Vector2 _activeSize = 120.0f.ToVector2();

        private List<UISkillElement> _spawnElements = new List<UISkillElement>();

        //------------------------------------------------------------------------------------
        public void SetJobSkill(int job, List<SkillInfo> skillInfos, System.Action<int> onClickCallback = null)
        {
            if (_jobName != null)
                _jobName.SetText("{0} Job", job);

            for (int i = 0; i < skillInfos.Count; ++i)
            {
                SkillInfo skillInfo = skillInfos[i];

                var element = Instantiate(_uISkillElement, skillInfo.SkillType == Enum_SkillType.Passive ? _uIPassiveSkillRoot : _uIActiveSkillRoot);
                RectTransform rectTransform = element.GetComponent<RectTransform>();
                if (rectTransform != null)
                    rectTransform.sizeDelta = skillInfo.SkillType == Enum_SkillType.Passive ? _passiveSize : _activeSize;
                element.ConnectCallBack(onClickCallback);
                element.Bind(skillInfo.SkillId);

                _spawnElements.Add(element);
            }
        }
        //------------------------------------------------------------------------------------
        public void Refresh()
        {
            for (int i = 0; i < _spawnElements.Count; ++i)
            {
                _spawnElements[i].Refresh();
            }
        }
        //------------------------------------------------------------------------------------
    }
}