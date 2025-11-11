using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIARRRGambleAddSkillGroup : MonoBehaviour
    {
        [SerializeField]
        private Transform _gambleAddskillIconElementRoot;

        [SerializeField]
        private UIARRRGambleAddSkillElement _uIARRRGambleAddSkillElementObj;

        private SkillActiveController _skillActiveController;

        private Dictionary<MainSkillData, int> _gambleSkillData_Count = new Dictionary<MainSkillData, int>();

        private Dictionary<MainSkillData, UIARRRGambleAddSkillElement> _uIARRRGambleAddSkillElements_CoolTimeRefresh = new Dictionary<MainSkillData, UIARRRGambleAddSkillElement>();

        private Queue<UIARRRGambleAddSkillElement> _uIARRRGambleAddSkillElementPool = new Queue<UIARRRGambleAddSkillElement>();


        //------------------------------------------------------------------------------------
        public void SetSkillActiveController(SkillActiveController skillActiveController)
        {
            if (_skillActiveController != null)
            {
                _skillActiveController.AddGambleSkillAction -= AddGambleSkill;
            }

            foreach (var pair in _uIARRRGambleAddSkillElements_CoolTimeRefresh)
            {
                _uIARRRGambleAddSkillElementPool.Enqueue(pair.Value);
                pair.Value.gameObject.SetActive(false);
            }

            _uIARRRGambleAddSkillElements_CoolTimeRefresh.Clear();
            _gambleSkillData_Count.Clear();

            _skillActiveController = skillActiveController;

            if (skillActiveController != null)
            {
                skillActiveController.AddGambleSkillAction += AddGambleSkill;

                var gambleNode = skillActiveController.MyAddGambleSkill.First;

                while (gambleNode != null)
                {
                    AddGambleSkillElement(gambleNode.Value);
                    gambleNode = gambleNode.Next;
                }

            }
        }
        //------------------------------------------------------------------------------------
        private UIARRRGambleAddSkillElement GetUIARRRSkillCoolTimeElement()
        {
            if (_uIARRRGambleAddSkillElementPool.Count > 0)
                return _uIARRRGambleAddSkillElementPool.Dequeue();

            GameObject clone = Instantiate(_uIARRRGambleAddSkillElementObj.gameObject, _gambleAddskillIconElementRoot);

            UIARRRGambleAddSkillElement uIARRRSkillCoolTimeElement = clone.GetComponent<UIARRRGambleAddSkillElement>();

            return uIARRRSkillCoolTimeElement;
        }
        //------------------------------------------------------------------------------------
        private void AddGambleSkillElement(MainSkillData skillManageInfo)
        {
            UIARRRGambleAddSkillElement uISkillIconElement = null;

            if (_gambleSkillData_Count.ContainsKey(skillManageInfo) == false)
            {
                _gambleSkillData_Count.Add(skillManageInfo, 1);

                uISkillIconElement = GetUIARRRSkillCoolTimeElement();
                uISkillIconElement.gameObject.SetActive(true);
                uISkillIconElement.SetSkillGambleSkill(skillManageInfo);

                if (_uIARRRGambleAddSkillElements_CoolTimeRefresh.ContainsKey(skillManageInfo) == false)
                    _uIARRRGambleAddSkillElements_CoolTimeRefresh.Add(skillManageInfo, uISkillIconElement);
            }
            else
            { 
                _gambleSkillData_Count[skillManageInfo] += 1;

                if (_uIARRRGambleAddSkillElements_CoolTimeRefresh.ContainsKey(skillManageInfo) == true)
                    uISkillIconElement = _uIARRRGambleAddSkillElements_CoolTimeRefresh[skillManageInfo];
            }

            if (uISkillIconElement == null)
                return;

            uISkillIconElement.SetSkillAccumCount(_gambleSkillData_Count[skillManageInfo]);
        }
        //------------------------------------------------------------------------------------
        public void AddGambleSkill(MainSkillData gambleSkillData)
        {
            AddGambleSkillElement(gambleSkillData);
        }
        //------------------------------------------------------------------------------------
    }
}