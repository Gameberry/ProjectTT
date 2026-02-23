using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;
using GameBerry.Common;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry.UI
{
    public class LanternInventoryDialog : IDialog
    {
        [Header("Right List")]
        [SerializeField] private UILanternElement _lanternElementPrefab;
        [SerializeField] private Transform _listRoot;

        [Header("Lantern Slots")]
        [SerializeField] private UILanternSlotElement _mainSlotElement;
        [SerializeField] private UILanternSlotElement _subSlotPrefab;
        [SerializeField] private Transform _subSlotRoot;

        [Header("Center Info")]
        [SerializeField] private TMP_Text _detailName;
        [SerializeField] private Image _skillIcon;
        [SerializeField] private TMP_Text _skillName;
        [SerializeField] private TMP_Text _skillType;
        [SerializeField] private TMP_Text _selectedSkillConditionDataDescription;
        [SerializeField] private TMP_Text _selectedSkillDescription;
        [Header("Active CoolTime Info")]
        [SerializeField] private Transform _activeSkillCoolTimeGroup;
        [SerializeField] private TMP_Text _cooldownTypeText;
        [SerializeField] private TMP_Text _cooldownValueText;

        [Header("Bottom Buttons")]
        [SerializeField] private Button _autoEquipButton;
        [SerializeField] private Button _allLevelUpButton;
        [SerializeField] private Button _showAppliedEffectButton;
        [SerializeField] private LanternAppliedEffectDialog _appliedEffectDialog;

        [Header("Page Buttons (Visual Only)")]
        [SerializeField] private List<Button> _pageButtons = new List<Button>();
        [SerializeField] private List<Image> _pageSelectedImages = new List<Image>();

        private readonly List<UILanternElement> _spawned = new List<UILanternElement>();
        private readonly ObjectPool<UILanternElement> _pool = new ObjectPool<UILanternElement>();
        private readonly List<int> _lanternItemIds = new List<int>();
        private readonly List<UILanternSlotElement> _subSlotElements = new List<UILanternSlotElement>();
        private readonly StringBuilder _sb = new StringBuilder(128);

        private int _selectedItemId;
        private int _selectedPageIndex;
        private int _lastMainLanternId = -1;

        protected override void OnLoad()
        {
            if (_autoEquipButton != null) _autoEquipButton.onClick.AddListener(OnClickAutoEquip);
            if (_allLevelUpButton != null) _allLevelUpButton.onClick.AddListener(OnClickAllLevelUp);
            if (_showAppliedEffectButton != null) _showAppliedEffectButton.onClick.AddListener(OnClickShowAppliedEffects);

            for (int i = 0; i < _pageButtons.Count; ++i)
            {
                int page = i;
                if (_pageButtons[page] != null)
                    _pageButtons[page].onClick.AddListener(() => SetPage(page));
            }

            BuildSlotElements();

            if (_appliedEffectDialog != null)
                _appliedEffectDialog.Load_Element();

            Refresh();
        }

        protected override void OnEnter()
        {
            if (LanternManager.isAlive)
            {
                LanternManager.Instance.OnLanternDataChanged += Refresh;
                LanternManager.Instance.OnLanternEquipChanged += Refresh;
            }

            if (ItemManager.isAlive)
                ItemManager.Instance.OnLanternStorageChanged += Refresh;

            Refresh();
        }

        protected override void OnExit()
        {
            if (LanternManager.isAlive)
            {
                LanternManager.Instance.OnLanternDataChanged -= Refresh;
                LanternManager.Instance.OnLanternEquipChanged -= Refresh;
            }

            if (ItemManager.isAlive)
                ItemManager.Instance.OnLanternStorageChanged -= Refresh;
        }

        protected override void OnUnload()
        {
            ReleaseAll();
            _pool.ClearAll();
        }

        private void Refresh()
        {
            BuildLanternIdList();
            RebuildList();
            EnsureSelectedLantern();

            int currentMainLanternId = LanternManager.isAlive ? LanternManager.Instance.GetMainLanternId() : 0;
            if (_lastMainLanternId != currentMainLanternId)
            {
                _lastMainLanternId = currentMainLanternId;
                _selectedItemId = currentMainLanternId > 0 ? currentMainLanternId : _selectedItemId;
            }

            RefreshSelectionVisual();
            RefreshSlotElements();
            RefreshCenterInfo();
            RefreshPageVisual();
        }

        private void BuildLanternIdList()
        {
            _lanternItemIds.Clear();

            LanternChart chart = GameChart.Get<LanternChart>();
            if (chart?.rows == null)
                return;

            for (int i = 0; i < chart.rows.Length; ++i)
            {
                LanternInfo info = chart.rows[i];
                if (info == null)
                    continue;

                _lanternItemIds.Add(info.ItemId);
            }
        }

        private void RebuildList()
        {
            ReleaseAll();

            if (_lanternElementPrefab == null || _listRoot == null)
                return;

            for (int i = 0; i < _lanternItemIds.Count; ++i)
            {
                UILanternElement element = GetOrCreate();
                if (element == null)
                    continue;

                element.transform.SetParent(_listRoot, false);
                element.gameObject.SetActive(true);
                element.SetOnSelect(OnSelectLantern);
                element.Bind(_lanternItemIds[i]);
                _spawned.Add(element);
            }
        }

        private UILanternElement GetOrCreate()
        {
            UILanternElement element = _pool.GetObject();
            if (element != null)
                return element;

            if (_lanternElementPrefab == null || _listRoot == null)
                return null;

            return Instantiate(_lanternElementPrefab, _listRoot);
        }

        private void ReleaseAll()
        {
            for (int i = 0; i < _spawned.Count; ++i)
            {
                UILanternElement e = _spawned[i];
                if (e == null)
                    continue;

                e.gameObject.SetActive(false);
                _pool.PoolObject(e);
            }

            _spawned.Clear();
        }

        private void BuildSlotElements()
        {
            if (_mainSlotElement != null)
                _mainSlotElement.Init(Enum_LanternSlotType.Main, OnClickSlot);

            LanternSlotChart slotChart = GameChart.Get<LanternSlotChart>();
            if (_subSlotPrefab == null || _subSlotRoot == null || slotChart?.rows == null)
                return;

            List<Enum_LanternSlotType> slotTypes = new List<Enum_LanternSlotType>();
            for (int i = 0; i < slotChart.rows.Length; ++i)
            {
                LanternSlotInfo slotInfo = slotChart.rows[i];
                if (slotInfo == null || slotInfo.SlotType == Enum_LanternSlotType.Main)
                    continue;

                slotTypes.Add(slotInfo.SlotType);
            }

            slotTypes.Sort((a, b) => a.CompareTo(b));

            while (_subSlotElements.Count < slotTypes.Count)
            {
                UILanternSlotElement element = Instantiate(_subSlotPrefab, _subSlotRoot);
                _subSlotElements.Add(element);
            }

            for (int i = 0; i < _subSlotElements.Count; ++i)
            {
                UILanternSlotElement element = _subSlotElements[i];
                if (element == null)
                    continue;

                bool active = i < slotTypes.Count;
                element.gameObject.SetActive(active);
                if (active)
                    element.Init(slotTypes[i], OnClickSlot);
            }
        }

        private void RefreshSlotElements()
        {
            if (LanternManager.isAlive == false)
                return;

            int mainId = LanternManager.Instance.GetMainLanternId();
            if (_mainSlotElement != null)
            {
                bool unlocked = LanternManager.Instance.IsSlotUnlocked(Enum_LanternSlotType.Main);
                _mainSlotElement.Bind(mainId, unlocked, BuildSlotLockText(Enum_LanternSlotType.Main));
            }

            for (int i = 0; i < _subSlotElements.Count; ++i)
            {
                UILanternSlotElement element = _subSlotElements[i];
                if (element == null || element.gameObject.activeSelf == false)
                    continue;

                Enum_LanternSlotType slotType = element.SlotType;
                bool unlocked = LanternManager.Instance.IsSlotUnlocked(slotType);
                int itemId = unlocked ? LanternManager.Instance.GetEquippedLanternId(slotType) : 0;
                element.Bind(itemId, unlocked, BuildSlotLockText(slotType));
            }
        }

        private string BuildSlotLockText(Enum_LanternSlotType slotType)
        {
            LanternSlotInfo slotInfo = LanternManager.Instance.GetSlotInfo(slotType);
            int need = slotInfo != null ? Mathf.Max(1, slotInfo.UnLockSummonLevel) : 1;
            return $"Unlock at summon lv.{need}";
        }

        private void OnSelectLantern(int itemId)
        {
            _selectedItemId = itemId;
            RefreshSelectionVisual();
        }

        private void EnsureSelectedLantern()
        {
            if (_lanternItemIds.Count <= 0)
            {
                _selectedItemId = 0;
                return;
            }

            if (_selectedItemId > 0 && _lanternItemIds.Contains(_selectedItemId))
                return;

            int equippedMain = LanternManager.Instance.GetMainLanternId();
            if (equippedMain > 0 && _lanternItemIds.Contains(equippedMain))
            {
                _selectedItemId = equippedMain;
                return;
            }

            _selectedItemId = _lanternItemIds[0];
        }

        private void RefreshSelectionVisual()
        {
            for (int i = 0; i < _spawned.Count; ++i)
            {
                UILanternElement element = _spawned[i];
                if (element == null)
                    continue;

                bool selected = _lanternItemIds.Count > i && _lanternItemIds[i] == _selectedItemId;
                element.SetSelected(selected);
                element.Refresh();
            }
        }

        private void RefreshCenterInfo()
        {
            int targetItemId = LanternManager.Instance.GetMainLanternId();
            LanternInfo lanternInfo = LanternManager.Instance.GetLanternInfo(targetItemId);
            LanternData lanternData = LanternManager.Instance.GetLanternData(targetItemId);
            ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(targetItemId);
            int level = lanternData?.level ?? 1;

            if (lanternInfo == null)
            {
                if (_detailName != null) _detailName.SetText("-");
                if (_skillIcon != null) _skillIcon.sprite = null;
                if (_skillType != null) _skillType.SetText("-");
                if (_skillName != null) _skillName.SetText("-");
                if (_selectedSkillConditionDataDescription != null) _selectedSkillConditionDataDescription.SetText("-");
                if (_selectedSkillDescription != null) _selectedSkillDescription.SetText("-");
                if (_cooldownTypeText != null) _cooldownTypeText.SetText("-");
                if (_cooldownValueText != null) _cooldownValueText.SetText("-");
                if (_activeSkillCoolTimeGroup != null) _activeSkillCoolTimeGroup.gameObject.SetActive(false);
                return;
            }

            if (_detailName != null) Managers.LocalStringManager.Instance.SetLocalizeText(_detailName, ItemManager.Instance.GetItemNameLocalKey(targetItemId));
            
            SkillInfo skillInfo = null;
            if (lanternInfo.Skill > 0)
                skillInfo = GameChart.Get<SkillChart>()?.GetActive(lanternInfo.Skill, Enum_SkillActorType.Lantern);

            if (_skillIcon != null)
                _skillIcon.sprite = skillInfo != null ? SkillManager.Instance.GetIcon(skillInfo.SkillId) : null;

            if (_skillType != null)
                _skillType.SetText(skillInfo != null ? skillInfo.SkillType.ToString() : "-");

            if (_skillName != null)
                _skillName.SetText(skillInfo != null ? $"Skill {skillInfo.SkillId}" : "-");

            if (_selectedSkillConditionDataDescription != null)
                _selectedSkillConditionDataDescription.SetText(skillInfo != null ? $"{(skillInfo.GetFinalAttackMultiplier(level) * 100.0):0.#}%" : "-");

            

            if (_selectedSkillDescription != null)
            {
                if (skillInfo == null)
                    _selectedSkillDescription.SetText("-");
                else
                    _selectedSkillDescription.SetText($"Hits {skillInfo.TargetCount} targets for {skillInfo.HitCount} times.");
            }

            if (_activeSkillCoolTimeGroup != null)
                _activeSkillCoolTimeGroup.gameObject.SetActive(skillInfo != null);

            if (_cooldownTypeText != null)
                _cooldownTypeText.SetText(skillInfo != null ? skillInfo.CooldownType.ToString() : "-");

            if (_cooldownValueText != null)
                _cooldownValueText.SetText(skillInfo != null ? $"{skillInfo.CooldownValue:0.#}s" : "-");

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

        private void OnClickSlot(Enum_LanternSlotType slotType)
        {
            if (LanternManager.Instance.IsSlotUnlocked(slotType) == false)
                return;
            if (_selectedItemId <= 0)
                return;

            if (LanternManager.Instance.SetEquip(slotType, _selectedItemId) == false)
                return;

            if (slotType == Enum_LanternSlotType.Main)
            {
                // Main equip should immediately drive center info.
                _lastMainLanternId = _selectedItemId;
                RefreshSelectionVisual();
                RefreshSlotElements();
                RefreshCenterInfo();
            }
        }

        private void OnClickAutoEquip()
        {
            LanternManager.Instance.AutoEquip();
        }

        private void OnClickAllLevelUp()
        {
            LanternManager.Instance.DoAllLevelUp();
        }

        private void OnClickShowAppliedEffects()
        {
            if (_appliedEffectDialog == null)
                return;

            _appliedEffectDialog.RefreshRows();
            _appliedEffectDialog.ElementEnter();
        }

        private void SetPage(int index)
        {
            _selectedPageIndex = Mathf.Clamp(index, 0, Mathf.Max(0, _pageButtons.Count - 1));
            RefreshPageVisual();
        }

        private void RefreshPageVisual()
        {
            for (int i = 0; i < _pageSelectedImages.Count; ++i)
            {
                if (_pageSelectedImages[i] == null)
                    continue;
                _pageSelectedImages[i].gameObject.SetActive(i == _selectedPageIndex);
            }
        }
    }
}
