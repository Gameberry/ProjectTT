using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UISummonLevelRewardElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _descText;
        [SerializeField] private UIItemElement _rewardItem;
        [SerializeField] private TMP_Text _stateText;
        [SerializeField] private Image _stateBg;
        [SerializeField] private Color _claimableColor = Color.cyan;
        [SerializeField] private Color _claimedColor = Color.gray;
        [SerializeField] private Color _lockedColor = Color.white;

        public void Bind(Chart.SummonLevelInfo info, bool claimable, bool claimed)
        {
            if (_levelText != null)
                _levelText.SetText($"Summon Lv.{info.SummonLevel}");

            if (_descText != null)
                _descText.SetText($"Reach total summon count {info.Exp} at Lv.{Mathf.Max(1, info.SummonLevel - 1)}");

            if (_rewardItem != null)
                _rewardItem.Bind(info._RewardItemHandle);

            if (_stateText != null)
            {
                if (claimed) _stateText.SetText("Claimed");
                else if (claimable) _stateText.SetText("Claimable");
                else _stateText.SetText("Locked");
            }

            if (_stateBg != null)
            {
                if (claimed) _stateBg.color = _claimedColor;
                else if (claimable) _stateBg.color = _claimableColor;
                else _stateBg.color = _lockedColor;
            }
        }
    }
}
