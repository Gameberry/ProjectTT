using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry.UI
{
    public class LanternDetailDialog : IDialog
    {
        [Header("Basic")]
        [SerializeField] private Image _detailIcon;
        [SerializeField] private Image _detailFrame;
        [SerializeField] private TMP_Text _detailName;
        [SerializeField] private TMP_Text _detailRarity;
        [SerializeField] private TMP_Text _detailTier;
        [SerializeField] private TMP_Text _detailLevel;

        [Header("Skill")]
        [SerializeField] private Image _skillIcon;
        [SerializeField] private TMP_Text _skillName;
        [SerializeField] private TMP_Text _skillType;
        [SerializeField] private TMP_Text _selectedSkillConditionDataDescription;
        [SerializeField] private TMP_Text _selectedSkillDescription;
        [SerializeField] private Transform _activeSkillCoolTimeGroup;
        [SerializeField] private TMP_Text _cooldownTypeText;
        [SerializeField] private TMP_Text _cooldownValueText;

        [Header("Stats")]
        [SerializeField] private TMP_Text _detailEquipStats;
        [SerializeField] private TMP_Text _detailOwnStats;

        [Header("Buttons")]
        [SerializeField] private Button _levelUpButton;
        [SerializeField] private TMP_Text _levelUpButtonText;
        [SerializeField] private Button _equipButton;
        [SerializeField] private TMP_Text _equipButtonText;

        private readonly StringBuilder _sb = new StringBuilder(128);
        private int _selectedItemId;
        private System.Action<int> _onClickEquipRequest;

        protected override void OnLoad()
        {
            if (_levelUpButton != null)
                _levelUpButton.onClick.AddListener(OnClickLevelUp);

            if (_equipButton != null)
                _equipButton.onClick.AddListener(OnClickEquipRequest);
        }

        protected override void OnEnter()
        {
            if (LanternManager.isAlive)
            {
                LanternManager.Instance.OnLanternDataChanged += RefreshDetail;
                LanternManager.Instance.OnLanternEquipChanged += RefreshDetail;
            }

            if (ItemManager.isAlive)
                ItemManager.Instance.OnLanternStorageChanged += RefreshDetail;

            RefreshDetail();
        }

        protected override void OnExit()
        {
            if (LanternManager.isAlive)
            {
                LanternManager.Instance.OnLanternDataChanged -= RefreshDetail;
                LanternManager.Instance.OnLanternEquipChanged -= RefreshDetail;
            }

            if (ItemManager.isAlive)
                ItemManager.Instance.OnLanternStorageChanged -= RefreshDetail;
        }

        public void Open(int itemId, System.Action<int> onClickEquipRequest)
        {
            _selectedItemId = itemId;
            _onClickEquipRequest = onClickEquipRequest;
            RefreshDetail();
            ElementEnter();
        }

        public void RefreshDetail()
        {
            if (_selectedItemId <= 0 || LanternManager.isAlive == false || ItemManager.isAlive == false)
                return;

            LanternManager lm = LanternManager.Instance;
            ItemManager im = ItemManager.Instance;

            ItemInfo itemInfo = im.GetItemMeta(_selectedItemId);
            LanternInfo lanternInfo = lm.GetLanternInfo(_selectedItemId);
            LanternData lanternData = lm.GetLanternData(_selectedItemId);
            if (itemInfo == null || lanternInfo == null)
                return;

            int level = lm.GetLanternLevel(_selectedItemId);
            int maxLevel = lm.GetMaxLevel(_selectedItemId);
            bool neverObtained = lanternData == null;

            if (_detailIcon != null) _detailIcon.sprite = im.GetIcon(_selectedItemId);
            if (_detailFrame != null) _detailFrame.sprite = StaticResource.Instance.GetRarityFrame(itemInfo.Rarity);
            if (_detailName != null) Managers.LocalStringManager.Instance.SetLocalizeText(_detailName, im.GetItemNameLocalKey(_selectedItemId));
            if (_detailRarity != null)
            {
                _detailRarity.SetText(itemInfo.Rarity.ToString());
                _detailRarity.color = StaticResource.Instance.GetRarityTextColor(itemInfo.Rarity);
            }
            if (_detailTier != null) _detailTier.SetText(itemInfo.Tier.ToString());
            if (_detailLevel != null) _detailLevel.SetText($"Lv.{level}/{maxLevel}");

            SkillInfo skillInfo = null;
            if (lanternInfo.Skill > 0)
                skillInfo = GameChart.Get<SkillChart>()?.GetActive(lanternInfo.Skill, Enum_SkillActorType.Lantern);

            if (_skillIcon != null)
                _skillIcon.sprite = skillInfo != null ? SkillManager.Instance.GetIcon(skillInfo.SkillId) : null;

            if (_skillType != null)
                _skillType.SetText(skillInfo != null ? skillInfo.SkillType.ToString() : "-");

            if (_skillName != null)
                _skillName.SetText(skillInfo != null ? SkillManager.Instance.GetSkillNameText(skillInfo.SkillId) : "-");

            if (_selectedSkillConditionDataDescription != null)
                _selectedSkillConditionDataDescription.SetText(skillInfo != null ? SkillManager.Instance.GetSkillConditionDescription(skillInfo) : "-");

            if (_selectedSkillDescription != null)
            {
                if (skillInfo == null)
                    _selectedSkillDescription.SetText("-");
                else
                    _selectedSkillDescription.SetText(SkillManager.Instance.GetSkillDescriptionText(skillInfo, level));
            }

            if (_activeSkillCoolTimeGroup != null)
                _activeSkillCoolTimeGroup.gameObject.SetActive(skillInfo != null);

            if (_cooldownTypeText != null)
                _cooldownTypeText.SetText(skillInfo != null ? SkillManager.Instance.GetCooldownTypeText(skillInfo) : "-");

            if (_cooldownValueText != null)
                _cooldownValueText.SetText(skillInfo != null ? SkillManager.Instance.GetCooldownValueText(skillInfo) : "-");

            if (_detailEquipStats != null)
                _detailEquipStats.SetText(BuildStatText(lanternInfo.GetEquipStats(), level));

            if (_detailOwnStats != null)
                _detailOwnStats.SetText(BuildStatText(lanternInfo.GetOwnStats(), level));

            bool canLevelUp = lm.CanLevelUp(_selectedItemId);
            bool canCombine = lm.CanCombine(_selectedItemId);

            if (_levelUpButton != null)
                _levelUpButton.interactable = neverObtained == false && (canLevelUp || canCombine);

            if (_levelUpButtonText != null)
                _levelUpButtonText.SetText(canCombine ? "Combine" : "Level Up");

            if (_equipButton != null)
                _equipButton.interactable = neverObtained == false;

            if (_equipButtonText != null)
                _equipButtonText.SetText("Equip");
        }

        private string BuildStatText(IReadOnlyDictionary<Enum_Stat, double> stats, int level)
        {
            _sb.Clear();

            if (stats == null || stats.Count <= 0)
                return "-";

            double multiplier = 1.0 + (Mathf.Max(1, level) - 1) * 0.1;
            List<KeyValuePair<Enum_Stat, double>> ordered = new List<KeyValuePair<Enum_Stat, double>>(stats);
            ordered.Sort((a, b) => a.Key.CompareTo(b.Key));

            for (int i = 0; i < ordered.Count; ++i)
            {
                KeyValuePair<Enum_Stat, double> kv = ordered[i];
                _sb.Append(StatHelper.GetStatDisplayName(kv.Key));
                _sb.Append(' ');
                _sb.Append(StatHelper.FormatStatDisplayValue(kv.Key, kv.Value * multiplier));
                if (i < ordered.Count - 1)
                    _sb.Append('\n');
            }

            return _sb.ToString();
        }

        private void OnClickLevelUp()
        {
            if (_selectedItemId <= 0 || LanternManager.isAlive == false)
                return;

            if (LanternManager.Instance.CanLevelUp(_selectedItemId))
                LanternManager.Instance.DoLevelUp(_selectedItemId);
            else if (LanternManager.Instance.CanCombine(_selectedItemId))
                LanternManager.Instance.DoCombine(_selectedItemId);
        }

        private void OnClickEquipRequest()
        {
            if (_selectedItemId <= 0)
                return;

            _onClickEquipRequest?.Invoke(_selectedItemId);
        }
    }
}
