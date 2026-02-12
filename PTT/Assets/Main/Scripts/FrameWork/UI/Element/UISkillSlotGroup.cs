using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry.UI
{
    public class UISkillSlotGroup : MonoBehaviour
    {
        [Header("Skill Slots")]
        [SerializeField] private List<UISkillSlotElement> _skillSlotElements = new List<UISkillSlotElement>();

        private System.Action<int> _onClickCallback;

        //------------------------------------------------------------------------------------
        void Start()
        {
            for (int i = 0; i < _skillSlotElements.Count; i++)
            {
                _skillSlotElements[i].Init(i);
                _skillSlotElements[i].OnSlotClicked += OnSlotClicked;
            }

            SkillManager.Instance.OnSkillSlotChanged += RefreshSlots;
        }
        //------------------------------------------------------------------------------------
        private void OnEnable()
        {
            RefreshSlots();
        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.OnSkillSlotChanged -= RefreshSlots;
            }
        }
        //------------------------------------------------------------------------------------
        public void OnConnect_SlotClicked(System.Action<int> onClickCallback)
        {
            _onClickCallback = onClickCallback;
        }
        //------------------------------------------------------------------------------------
        private void RefreshSlots()
        {
            for (int i = 0; i < _skillSlotElements.Count; i++)
            {
                _skillSlotElements[i].RefreshSlot();
            }
        }
        //------------------------------------------------------------------------------------
        private void OnSlotClicked(int slotIndex)
        {
            _onClickCallback?.Invoke(slotIndex);
        }
        //------------------------------------------------------------------------------------
    }
}