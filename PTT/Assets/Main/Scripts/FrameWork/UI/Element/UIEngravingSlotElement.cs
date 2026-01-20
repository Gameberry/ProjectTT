using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Table;

namespace GameBerry.UI
{
    /// <summary>
    /// 각인 스테이지의 3개 슬롯을 표시하는 Element
    /// </summary>
    public class UIEngravingSlotElement : MonoBehaviour
    {
        [Header("Slot UI")]
        [SerializeField] private UIStatElement[] _statElements = new UIStatElement[3];
        [SerializeField] private Image[] _gradeBackgrounds = new Image[3];

        [Header("Matching Display")]
        [SerializeField] private GameObject _matchingIndicator;
        [SerializeField] private Image _borderImage;
        [SerializeField] private Color _matchingBorderColor = Color.green;
        [SerializeField] private Color _normalBorderColor = Color.gray;

        //------------------------------------------------------------------------------------
        public void UpdateSlots(EngravingStageData stageData)
        {
            if (stageData == null)
                return;

            for (int i = 0; i < EngravingStageData.SlotCount; i++)
            {
                if (i < stageData.slots.Count)
                    UpdateSlot(i, stageData.slots[i]);
                else
                    ClearSlot(i);
            }

            bool isMatching = stageData.HasMatchingStats();

            if (_matchingIndicator != null)
                _matchingIndicator.SetActive(isMatching);

            if (_borderImage != null)
                _borderImage.color = isMatching ? _matchingBorderColor : _normalBorderColor;
        }
        //------------------------------------------------------------------------------------
        private void UpdateSlot(int index, EngravingSlot slot)
        {
            if (index < 0 || index >= _statElements.Length)
                return;

            if (slot.IsEmpty)
            {
                ClearSlot(index);
                return;
            }

            if (_statElements[index] != null)
            {
                _statElements[index].gameObject.SetActive(true);
                _statElements[index].SetStatView(slot.statType, slot.value);
            }

            if (_gradeBackgrounds[index] != null)
            {
                _gradeBackgrounds[index].color = StaticResource.Instance.GetRarityTextColor(slot.grade);
            }
        }
        //------------------------------------------------------------------------------------
        private void ClearSlot(int index)
        {
            if (index < 0 || index >= _statElements.Length)
                return;

            if (_statElements[index] != null)
                _statElements[index].gameObject.SetActive(false);

            if (_gradeBackgrounds[index] != null)
                _gradeBackgrounds[index].color = Color.white;
        }
        //------------------------------------------------------------------------------------
    }
}
