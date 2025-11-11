using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class UIGambleSynergySkillListElement : MonoBehaviour
    {
        [SerializeField]
        private Image _gambleSynergyIcon;

        [SerializeField]
        private Transform _gambleAddskillIconElementRoot;

        [SerializeField]
        private UIARRRGambleAddSkillElement _uIARRRGambleAddSkillElementObj;

        private Dictionary<MainSkillData, int> _gambleSkillData_Count = new Dictionary<MainSkillData, int>();

        private Dictionary<MainSkillData, UIARRRGambleAddSkillElement> _uIARRRGambleAddSkillElements = new Dictionary<MainSkillData, UIARRRGambleAddSkillElement>();

        private Queue<UIARRRGambleAddSkillElement> _uIARRRGambleAddSkillElementPool = new Queue<UIARRRGambleAddSkillElement>();

        private SkillActiveController _skillActiveController;

        private LinkedList<UIARRRGambleAddSkillElement> _uIARRRSkillCoolTimeElements_CoolTimeRefresh = new LinkedList<UIARRRGambleAddSkillElement>();

        //------------------------------------------------------------------------------------
        public void SetSynergyListData(SynergyData gambleSynergyData)
        { 

        }
        //------------------------------------------------------------------------------------
        public void SetSkillActiveController(SkillActiveController skillActiveController)
        {
            if (_skillActiveController != null)
            {
                _skillActiveController.RefreshCoolTimeAction -= RefreshCoolTime;
            }

            foreach (var pair in _uIARRRGambleAddSkillElements)
            {
                _uIARRRGambleAddSkillElementPool.Enqueue(pair.Value);
                pair.Value.gameObject.SetActive(false);
            }

            _uIARRRSkillCoolTimeElements_CoolTimeRefresh.Clear();
            _uIARRRGambleAddSkillElements.Clear();
            _gambleSkillData_Count.Clear();
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
        private void AddGambleSkillElement(MainSkillData gambleSkillData)
        {
            UIARRRGambleAddSkillElement uISkillIconElement = null;

            if (_gambleSkillData_Count.ContainsKey(gambleSkillData) == false)
            {
                _gambleSkillData_Count.Add(gambleSkillData, 1);

                uISkillIconElement = GetUIARRRSkillCoolTimeElement();
                uISkillIconElement.gameObject.SetActive(true);
                uISkillIconElement.SetSkillGambleSkill(gambleSkillData);

                if (_uIARRRGambleAddSkillElements.ContainsKey(gambleSkillData) == false)
                    _uIARRRGambleAddSkillElements.Add(gambleSkillData, uISkillIconElement);

                if (_skillActiveController != null)
                {
                    SkillBaseData skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(gambleSkillData.MainSkillTypeParam1);
                    if (skillBaseData != null)
                    {
                        if (skillBaseData.IsUseCoolTime == true)
                        {
                            SkillManageInfo skillManageInfo = _skillActiveController.GetSkillManagerInfo(skillBaseData);
                            if (skillManageInfo != null)
                            {
                                // 쿨타임이 있는 애면
                                uISkillIconElement.SetSkillManageInfo(skillManageInfo);
                                _uIARRRSkillCoolTimeElements_CoolTimeRefresh.AddLast(uISkillIconElement);
                            }
                        }
                    }
                }
            }
            else
            {
                _gambleSkillData_Count[gambleSkillData] += 1;

                if (_uIARRRGambleAddSkillElements.ContainsKey(gambleSkillData) == true)
                    uISkillIconElement = _uIARRRGambleAddSkillElements[gambleSkillData];
            }

            if (uISkillIconElement == null)
                return;

            uISkillIconElement.SetSkillAccumCount(_gambleSkillData_Count[gambleSkillData]);
        }
        //------------------------------------------------------------------------------------
        public void AddGambleSkill(MainSkillData gambleSkillData)
        {
            AddGambleSkillElement(gambleSkillData);
        }
        //------------------------------------------------------------------------------------
        public void RefreshCoolTime()
        {
            var node = _uIARRRSkillCoolTimeElements_CoolTimeRefresh.First;

            while (node != null)
            {
                node.Value?.RefreshSkillCoolTime();
                node = node.Next;
            }
        }
        //------------------------------------------------------------------------------------
    }
}