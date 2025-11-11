using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIARRRSkillCoolTimeGroup : MonoBehaviour
    {
        [SerializeField]
        private Transform _coolTimeElementRoot;

        [SerializeField]
        private UIARRRSkillCoolTimeElement _uIARRRSkillCoolTimeElementObj;

        private SkillActiveController _skillActiveController;

        private LinkedList<UIARRRSkillCoolTimeElement> _uIARRRSkillCoolTimeElements_CoolTimeRefresh = new LinkedList<UIARRRSkillCoolTimeElement>();

        private Queue<UIARRRSkillCoolTimeElement> _uIARRRSkillCoolTimeElementPool = new Queue<UIARRRSkillCoolTimeElement>();

        //------------------------------------------------------------------------------------
        public void SetSkillActiveController(SkillActiveController skillActiveController)
        {
            if (_skillActiveController != null)
            {
                _skillActiveController.RefreshCoolTimeAction -= RefreshCoolTime;
            }

            var node = _uIARRRSkillCoolTimeElements_CoolTimeRefresh.First;

            while (node != null)
            {
                _uIARRRSkillCoolTimeElementPool.Enqueue(node.Value);
                node.Value.gameObject.SetActive(false);
                node = node.Next;
            }

            _uIARRRSkillCoolTimeElements_CoolTimeRefresh.Clear();

            _skillActiveController = skillActiveController;

            if (skillActiveController != null)
            { 
                skillActiveController.RefreshCoolTimeAction += RefreshCoolTime;

                for (int i = 0; i < skillActiveController.OriginSkillList.Count; ++i)
                {
                    SkillManageInfo skillManageInfo = skillActiveController.OriginSkillList[i];

                    UIARRRSkillCoolTimeElement uIARRRSkillCoolTimeElement = GetUIARRRSkillCoolTimeElement();
                    uIARRRSkillCoolTimeElement.gameObject.SetActive(true);
                    uIARRRSkillCoolTimeElement.SetSkillManageInfo(skillManageInfo);
                    uIARRRSkillCoolTimeElement.RefreshSkillCoolTime();
                    _uIARRRSkillCoolTimeElements_CoolTimeRefresh.AddLast(uIARRRSkillCoolTimeElement);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private UIARRRSkillCoolTimeElement GetUIARRRSkillCoolTimeElement()
        {
            if (_uIARRRSkillCoolTimeElementPool.Count > 0)
                return _uIARRRSkillCoolTimeElementPool.Dequeue();

            GameObject clone = Instantiate(_uIARRRSkillCoolTimeElementObj.gameObject, _coolTimeElementRoot);

            UIARRRSkillCoolTimeElement uIARRRSkillCoolTimeElement = clone.GetComponent<UIARRRSkillCoolTimeElement>();

            return uIARRRSkillCoolTimeElement;
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