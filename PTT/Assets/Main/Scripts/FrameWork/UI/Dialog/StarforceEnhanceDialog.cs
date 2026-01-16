using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Chart;
using GameBerry.Table;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class StarforceEnhanceDialog : IDialog
    {
        [Header("Left Panel - Equip Slots")]
        [SerializeField] private List<UIEquipmentSlotElement> _slotElements = new List<UIEquipmentSlotElement>();

        [Header("Center Panel - Selected Equipment Info")]
        [SerializeField] private UIEquipmentSlotElement _selectedItemElement;
        [SerializeField] private TMP_Text _selectedItemName;

        [Header("Center Panel - Stat View")]
        [SerializeField] private Transform _statContent;
        [SerializeField] private UIStarforceStatElement _statElementPrefab;
        private readonly List<UIStarforceStatElement> _spawnedStatElements = new List<UIStarforceStatElement>();
        [SerializeField] private Transform _equipStatLine;

        [Header("Right Panel - Starforce Info")]
        [SerializeField] private Sprite _starFilled;
        [SerializeField] private Sprite _starEmpty;
        [SerializeField] private List<Image> _starImages = new List<Image>();

        [SerializeField] private TMP_Text _currentLevelText;
        [SerializeField] private TMP_Text _nextLevelText;

        [SerializeField] private TMP_Text _mainStatPerCurrentText;
        [SerializeField] private TMP_Text _mainStatPerNextText;
        [SerializeField] private Image _mainStatPerArrow;

        [SerializeField] private TMP_Text _subStatPerText;
        [SerializeField] private TMP_Text _subStatPerNextText;
        [SerializeField] private Image _subStatPerArrow;

        [Header("Right Panel - Probability")]
        [SerializeField] private TMP_Text _successRateText;
        [SerializeField] private TMP_Text _stayRateText;
        [SerializeField] private TMP_Text _downRateText;
        [SerializeField] private TMP_Text _destroyRateText;

        [Header("Right Panel - Options")]
        [SerializeField] private Toggle _downAidToggle;
        [SerializeField] private Toggle _destroyAidToggle;
        [SerializeField] private Transform _downAidGroup;
        [SerializeField] private Transform _destroyAidGroup;

        [Header("Right Panel - Price")]
        [SerializeField] private UIItemElement _mainPriceElement;
        [SerializeField] private TMP_Text _mainPriceText;
        [SerializeField] private UIItemElement _subPriceElement;
        [SerializeField] private TMP_Text _subPriceText;

        [Header("Right Panel - Buttons")]
        [SerializeField] private Button _enhanceButton;
        [SerializeField] private TMP_Text _enhanceButtonText;

        [Header("Destroy State")]
        [SerializeField] private Transform _destroyStateGroup;
        [SerializeField] private TMP_Text _destroyStateMessage;
        [SerializeField] private UIItemElement _restorationItem1Element;
        [SerializeField] private TMP_Text _restorationItem1Text;
        [SerializeField] private UIItemElement _restorationItem2Element;
        [SerializeField] private TMP_Text _restorationItem2Text;
        [SerializeField] private Button _restorationButton;

        [Header("Result Popup")]
        [SerializeField] private Transform _resultPopupRoot;
        [SerializeField] private UIItemElement _resultItemElement;
        [SerializeField] private TMP_Text _resultTitleText;
        [SerializeField] private Image _resultEffectImage;
        [SerializeField] private Transform _resultStatContent;
        [SerializeField] private UIStarforceResultStatElement _resultStatPrefab;
        private readonly List<UIStarforceResultStatElement> _spawnedResultStats = new List<UIStarforceResultStatElement>();

        [Header("Result Popup Colors")]
        [SerializeField] private Color _successColor = Color.green;
        [SerializeField] private Color _stayColor = Color.yellow;
        [SerializeField] private Color _downColor = Color.gray;
        [SerializeField] private Color _destroyColor = Color.red;

        // State
        private Enum_EquipType _selectedSlot = Enum_EquipType.Max;
        private EquipSlotEnhanceChart _enhanceChart;
        private bool _isProcessing = false;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            _enhanceChart = GameChart.Get<EquipSlotEnhanceChart>();

            if (_enhanceButton != null)
                _enhanceButton.onClick.AddListener(OnClickEnhance);

            if (_restorationButton != null)
                _restorationButton.onClick.AddListener(OnClickRestoration);

            if (_downAidToggle != null)
                _downAidToggle.onValueChanged.AddListener(_ => RefreshProbabilityAndPrice());

            if (_destroyAidToggle != null)
                _destroyAidToggle.onValueChanged.AddListener(_ => RefreshProbabilityAndPrice());

            for (int i = 0; i < _slotElements.Count; i++)
            {
                var slot = _slotElements[i];
                slot.Init();
                slot.OnSlotClicked += OnSlotSelected;
            }

            if (_resultPopupRoot != null)
                _resultPopupRoot.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            EquipmentManager.Instance.OnEquipSlotChanged += RefreshAllSlots;

            RefreshAllSlots();

            // 첫 번째 장착된 슬롯 자동 선택
            SelectFirstEquippedSlot();
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (EquipmentManager.Instance != null)
                EquipmentManager.Instance.OnEquipSlotChanged -= RefreshAllSlots;
        }
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        private void RefreshAllSlots()
        {
            for (int i = 0; i < _slotElements.Count; i++)
            {
                _slotElements[i].RefreshSlot();
            }

            if (_selectedSlot != Enum_EquipType.Max)
                RefreshSelectedSlot();
        }
        //------------------------------------------------------------------------------------
        private void SelectFirstEquippedSlot()
        {
            for (int i = 0; i < _slotElements.Count; i++)
            {
                if (_slotElements[i].HasEquipment)
                {
                    OnSlotSelected(_slotElements[i]._enum_EquipType);
                    return;
                }
            }

            _selectedSlot = Enum_EquipType.Max;
            ClearSelectedView();
        }
        //------------------------------------------------------------------------------------
        private void OnSlotSelected(Enum_EquipType equipType)
        {
            _selectedSlot = equipType;

            for (int i = 0; i < _slotElements.Count; i++)
            {
                _slotElements[i].SetSelected(_slotElements[i]._enum_EquipType == equipType);
            }

            RefreshSelectedSlot();
        }
        //------------------------------------------------------------------------------------
        private void RefreshSelectedSlot()
        {
            if (_selectedSlot == Enum_EquipType.Max)
            {
                ClearSelectedView();
                return;
            }

            if (!EquipmentManager.Instance.TryGetEquippedHandle(_selectedSlot, out ItemHandle handle))
            {
                ClearSelectedView();
                return;
            }

            // 장비 정보 표시
            if (_selectedItemElement != null)
            {
                _selectedItemElement._enum_EquipType = _selectedSlot;
                _selectedItemElement.Init();
                _selectedItemElement.RefreshSlot();
            }

            if (_selectedItemName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_selectedItemName, ItemManager.Instance.GetItemNameLocalKey(handle.itemId));

            int currentLevel = EquipmentManager.Instance.GetStarforceLevel(_selectedSlot);
            bool isDestroyed = EquipmentManager.Instance.IsDestroyStarforce(_selectedSlot);

            if (isDestroyed)
            {
                ShowDestroyState();
            }
            else
            {
                ShowEnhanceState(handle, currentLevel);
            }
        }
        //------------------------------------------------------------------------------------
        private void ShowEnhanceState(ItemHandle handle, int currentLevel)
        {
            if (_destroyStateGroup != null)
                _destroyStateGroup.gameObject.SetActive(false);

            RefreshStars(currentLevel);
            RefreshStatView(handle, currentLevel);
            RefreshEnhanceInfo(currentLevel);
            RefreshProbabilityAndPrice();

            if (_enhanceButton != null)
            {
                bool canEnhance = _enhanceChart.TryGetEquipSlotEnhanceInfo(currentLevel + 1, out _);
                _enhanceButton.interactable = canEnhance && !_isProcessing;

                if (_enhanceButtonText != null)
                    _enhanceButtonText.SetText(canEnhance ? "강화하기" : "MAX");
            }
        }
        //------------------------------------------------------------------------------------
        private void ShowDestroyState()
        {
            if (_destroyStateGroup != null)
                _destroyStateGroup.gameObject.SetActive(true);

            if (_destroyStateMessage != null)
                _destroyStateMessage.SetText("장비 슬롯이 파괴되어 강화 효과가 적용되지 않습니다.");

            // 복구 재료 표시
            long item1Amount = ItemManager.Instance.GetItemAmount(Define.StarforceRestoration1_Key);
            long item2Amount = ItemManager.Instance.GetItemAmount(Define.StarforceRestoration2_Key);

            if (_restorationItem1Element != null)
                _restorationItem1Element.Bind(ItemHandle.ForStack(Define.StarforceRestoration1_Key));

            if (_restorationItem1Text != null)
                _restorationItem1Text.SetText("{0}/{1}", item1Amount, Define.StarforceRestoration1_Price);

            if (_restorationItem2Element != null)
                _restorationItem2Element.Bind(ItemHandle.ForStack(Define.StarforceRestoration2_Key));

            if (_restorationItem2Text != null)
                _restorationItem2Text.SetText("{0}/{1}", item2Amount, Define.StarforceRestoration2_Price);

            if (_restorationButton != null)
            {
                bool canRestore = item1Amount >= Define.StarforceRestoration1_Price
                               && item2Amount >= Define.StarforceRestoration2_Price;
                _restorationButton.interactable = canRestore && !_isProcessing;
            }

            // 강화 UI 숨김
            RefreshStars(0);

            if (_enhanceButton != null)
                _enhanceButton.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void RefreshStars(int level)
        {
            for (int i = 0; i < _starImages.Count; i++)
            {
                if (i < level)
                    _starImages[i].sprite = _starFilled;
                else
                    _starImages[i].sprite = _starEmpty;
            }

            if (_currentLevelText != null)
                _currentLevelText.SetText("{0}", level);

            if (_nextLevelText != null)
                _nextLevelText.SetText("{0}", level + 1);
        }
        //------------------------------------------------------------------------------------
        private void RefreshStatView(ItemHandle handle, int currentLevel)
        {
            EquipInfo equipInfo = GameChart.Get<EquipChart>()?.Get(handle.itemId);
            if (equipInfo == null)
                return;

            EquipmentData equipData = EquipmentManager.Instance.GetEquipmentData(handle);

            _enhanceChart.TryGetEquipSlotEnhanceInfo(currentLevel, out EquipSlotEnhanceInfo currentInfo);
            _enhanceChart.TryGetEquipSlotEnhanceInfo(currentLevel + 1, out EquipSlotEnhanceInfo nextInfo);

            int idx = 0;

            // 기본 스탯
            foreach (var kvp in equipInfo.GetBaseStats())
            {
                double currentValue = kvp.Value * (1.0 + currentInfo.MainStatPer);
                double nextValue = kvp.Value * (1.0 + nextInfo.MainStatPer);
                double diff = nextValue - currentValue;

                AddStatLine(kvp.Key, currentValue, kvp.Value, kvp.Value * currentInfo.MainStatPer, diff, idx);
                idx++;
            }

            if (_equipStatLine != null)
                _equipStatLine.SetAsLastSibling();

            // 추가 스탯
            if (equipData?.addStatList != null)
            {
                foreach (var addStat in equipData.addStatList)
                {
                    double currentValue = addStat.value * (1.0 + currentInfo.SubStatPer);
                    double nextValue = addStat.value * (1.0 + nextInfo.SubStatPer);
                    double diff = nextValue - currentValue;

                    AddStatLine(addStat.stat, currentValue, addStat.value, addStat.value * currentInfo.MainStatPer, diff, idx);
                    idx++;
                }
            }

            // 나머지 숨김
            for (int i = idx; i < _spawnedStatElements.Count; i++)
            {
                _spawnedStatElements[i].gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void AddStatLine(Enum_Stat stat, double currentValue, double equipValue, double addValue, double diff, int index)
        {
            UIStarforceStatElement element;

            if (index < _spawnedStatElements.Count)
            {
                element = _spawnedStatElements[index];
            }
            else
            {
                element = Instantiate(_statElementPrefab, _statContent);
                _spawnedStatElements.Add(element);
            }

            element.SetStat(stat, currentValue, equipValue, addValue, diff);
            element.transform.SetAsLastSibling();
            element.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        private void RefreshEnhanceInfo(int currentLevel)
        {
            if (!_enhanceChart.TryGetEquipSlotEnhanceInfo(currentLevel, out EquipSlotEnhanceInfo currentInfo))
                return;

            _enhanceChart.TryGetEquipSlotEnhanceInfo(currentLevel + 1, out EquipSlotEnhanceInfo nextInfo);

            // 주 옵션 증폭
            if (_mainStatPerCurrentText != null)
                _mainStatPerCurrentText.SetText(string.Format("{0:P0}", currentInfo.MainStatPer));

            bool sameMainPer = currentInfo.MainStatPer == nextInfo.MainStatPer;

            if (_mainStatPerArrow != null)
                _mainStatPerArrow.gameObject.SetActive(sameMainPer == false);

            if (_mainStatPerNextText != null)
            {
                if (sameMainPer)
                    _mainStatPerNextText.gameObject.SetActive(false);
                else
                {
                    _mainStatPerNextText.gameObject.SetActive(true);
                    _mainStatPerNextText.SetText(string.Format("{0:P0}", nextInfo.MainStatPer));
                }
            }

            if (_subStatPerText != null)
                _subStatPerText.SetText(string.Format("{0:P0}", nextInfo.SubStatPer));

            bool sameSubPer = currentInfo.SubStatPer == nextInfo.SubStatPer;

            if (_subStatPerArrow != null)
                _subStatPerArrow.gameObject.SetActive(sameSubPer == false);

            if (_subStatPerNextText != null)
            {
                if (sameSubPer)
                    _subStatPerNextText.gameObject.SetActive(false);
                else
                {
                    _subStatPerNextText.gameObject.SetActive(true);
                    _subStatPerNextText.SetText(string.Format("{0:P0}", nextInfo.SubStatPer));
                }
            }

            // 하락/파괴 완화 옵션 표시 여부
            if (_downAidGroup != null)
                _downAidGroup.gameObject.SetActive(currentInfo.Down > 0);

            if (_destroyAidGroup != null)
                _destroyAidGroup.gameObject.SetActive(currentInfo.Destroy > 0);

            if (_downAidToggle != null && currentInfo.Down <= 0)
                _downAidToggle.isOn = false;

            if (_destroyAidToggle != null && currentInfo.Destroy <= 0)
                _destroyAidToggle.isOn = false;
        }
        //------------------------------------------------------------------------------------
        private void RefreshProbabilityAndPrice()
        {
            int currentLevel = EquipmentManager.Instance.GetStarforceLevel(_selectedSlot);

            if (!_enhanceChart.TryGetEquipSlotEnhanceInfo(currentLevel, out EquipSlotEnhanceInfo info))
                return;

            bool downAid = _downAidToggle != null && _downAidToggle.isOn;
            bool destroyAid = _destroyAidToggle != null && _destroyAidToggle.isOn;

            float success = info.Success;
            float stay = info.Stay;
            float down = info.Down;
            float destroy = info.Destroy;

            if (downAid)
            {
                stay += down;
                down = 0;
            }

            if (destroyAid)
            {
                stay += destroy * 0.5f;
                destroy *= 0.5f;
            }

            if (_successRateText != null)
                _successRateText.SetText("{0:P0}", success);

            if (_stayRateText != null)
                _stayRateText.SetText("{0:P0}", stay);

            if (_downRateText != null)
                _downRateText.SetText("{0:P0}", down);

            if (_destroyRateText != null)
            {
                _destroyRateText.SetText("{0:P0}", destroy);
                _destroyRateText.color = destroy > 0 ? _destroyColor : Color.white;
            }

            // 가격
            long subPrice = info.SubPrice;
            if (downAid) subPrice += info.SubPrice;
            if (destroyAid) subPrice += info.SubPrice;

            if (_mainPriceElement != null)
                _mainPriceElement.Bind(ItemHandle.ForStack(info.MainPriceKey));

            if (_mainPriceText != null)
                _mainPriceText.SetText("{0}", info.MainPrice);

            if (_subPriceElement != null)
                _subPriceElement.Bind(ItemHandle.ForStack(info.SubPriceKey));

            if (_subPriceText != null)
                Util.SetCommaInteger(_subPriceText, subPrice);

            // 버튼 활성화 체크
            RefreshEnhanceButtonState(info, subPrice);
        }
        //------------------------------------------------------------------------------------
        private void RefreshEnhanceButtonState(EquipSlotEnhanceInfo info, long subPrice)
        {
            if (_enhanceButton == null)
                return;

            long mainAmount = ItemManager.Instance.GetItemAmount(info.MainPriceKey);
            long subAmount = ItemManager.Instance.GetItemAmount(info.SubPriceKey);

            bool canAfford = mainAmount >= info.MainPrice && subAmount >= subPrice;
            bool hasNext = _enhanceChart.TryGetEquipSlotEnhanceInfo(EquipmentManager.Instance.GetStarforceLevel(_selectedSlot) + 1, out _);

            _enhanceButton.interactable = canAfford && hasNext && !_isProcessing;
        }
        //------------------------------------------------------------------------------------
        private void ClearSelectedView()
        {
            if (_selectedItemElement != null)
                _selectedItemElement.gameObject.SetActive(false);

            if (_selectedItemName != null)
                _selectedItemName.SetText("");

            for (int i = 0; i < _spawnedStatElements.Count; i++)
            {
                _spawnedStatElements[i].gameObject.SetActive(false);
            }

            RefreshStars(0);

            if (_enhanceButton != null)
                _enhanceButton.interactable = false;

            if (_destroyStateGroup != null)
                _destroyStateGroup.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void OnClickEnhance()
        {
            if (_isProcessing || _selectedSlot == Enum_EquipType.Max)
                return;

            DoEnhanceAsync().Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask DoEnhanceAsync()
        {
            _isProcessing = true;

            if (_enhanceButton != null)
                _enhanceButton.interactable = false;

            bool downAid = _downAidToggle != null && _downAidToggle.isOn;
            bool destroyAid = _destroyAidToggle != null && _destroyAidToggle.isOn;

            // 강화 전 스탯 저장 (결과 표시용)
            int beforeLevel = EquipmentManager.Instance.GetStarforceLevel(_selectedSlot);

            Enum_StarforceResult result = EquipmentManager.Instance.DoStarforceUp(_selectedSlot, downAid, destroyAid);

            if (result == Enum_StarforceResult.Max)
            {
                _isProcessing = false;
                RefreshSelectedSlot();
                return;
            }

            int afterLevel = EquipmentManager.Instance.GetStarforceLevel(_selectedSlot);

            // 결과 팝업 표시
            await ShowResultPopup(result, beforeLevel, afterLevel);

            _isProcessing = false;
            RefreshSelectedSlot();
        }
        //------------------------------------------------------------------------------------
        private async UniTask ShowResultPopup(Enum_StarforceResult result, int beforeLevel, int afterLevel)
        {
            if (_resultPopupRoot == null)
                return;

            _resultPopupRoot.gameObject.SetActive(true);

            // 아이템 표시
            if (EquipmentManager.Instance.TryGetEquippedHandle(_selectedSlot, out ItemHandle handle))
            {
                if (_resultItemElement != null)
                    _resultItemElement.Bind(handle);
            }

            // 결과 타이틀
            string titleText = "";
            Color titleColor = Color.white;

            switch (result)
            {
                case Enum_StarforceResult.Success:
                    titleText = "강화 성공";
                    titleColor = _successColor;
                    break;
                case Enum_StarforceResult.Stay:
                    titleText = "강화 유지";
                    titleColor = _stayColor;
                    break;
                case Enum_StarforceResult.Down:
                    titleText = "강화 하락";
                    titleColor = _downColor;
                    break;
                case Enum_StarforceResult.Destroy:
                    titleText = "장비 파괴";
                    titleColor = _destroyColor;
                    break;
            }

            if (_resultTitleText != null)
            {
                _resultTitleText.SetText(titleText);
                _resultTitleText.color = titleColor;
            }

            // 결과 이펙트 색상
            if (_resultEffectImage != null)
                _resultEffectImage.color = titleColor;

            // 성공 시 스탯 변화 표시
            if (result == Enum_StarforceResult.Success)
            {
                ShowResultStats(handle, beforeLevel, afterLevel);
            }
            else
            {
                // 스탯 표시 숨김
                for (int i = 0; i < _spawnedResultStats.Count; i++)
                {
                    _spawnedResultStats[i].gameObject.SetActive(false);
                }
            }

            // 팝업 애니메이션 (DOTween)
            _resultPopupRoot.localScale = Vector3.zero;
            await _resultPopupRoot.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).AsyncWaitForCompletion();

            // 터치 대기
            await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0) || Input.touchCount > 0);

            await _resultPopupRoot.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).AsyncWaitForCompletion();

            _resultPopupRoot.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void ShowResultStats(ItemHandle handle, int beforeLevel, int afterLevel)
        {
            EquipInfo equipInfo = GameChart.Get<EquipChart>()?.Get(handle.itemId);
            if (equipInfo == null)
                return;

            EquipmentData equipData = EquipmentManager.Instance.GetEquipmentData(handle);

            _enhanceChart.TryGetEquipSlotEnhanceInfo(beforeLevel, out EquipSlotEnhanceInfo beforeInfo);
            _enhanceChart.TryGetEquipSlotEnhanceInfo(afterLevel, out EquipSlotEnhanceInfo afterInfo);

            int idx = 0;

            // 기본 스탯
            foreach (var kvp in equipInfo.GetBaseStats())
            {
                double beforeValue = kvp.Value * (1.0 + beforeInfo.MainStatPer);
                double afterValue = kvp.Value * (1.0 + afterInfo.MainStatPer);

                AddResultStatLine(kvp.Key, beforeValue, afterValue, idx);
                idx++;
            }

            // 추가 스탯
            if (equipData?.addStatList != null)
            {
                foreach (var addStat in equipData.addStatList)
                {
                    double beforeValue = addStat.value * (1.0 + beforeInfo.SubStatPer);
                    double afterValue = addStat.value * (1.0 + afterInfo.SubStatPer);

                    AddResultStatLine(addStat.stat, beforeValue, afterValue, idx);
                    idx++;
                }
            }

            for (int i = idx; i < _spawnedResultStats.Count; i++)
            {
                _spawnedResultStats[i].gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void AddResultStatLine(Enum_Stat stat, double beforeValue, double afterValue, int index)
        {
            UIStarforceResultStatElement element;

            if (index < _spawnedResultStats.Count)
            {
                element = _spawnedResultStats[index];
            }
            else
            {
                element = Instantiate(_resultStatPrefab, _resultStatContent);
                _spawnedResultStats.Add(element);
            }

            element.SetStat(stat, beforeValue, afterValue);
            element.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        private void OnClickRestoration()
        {
            if (_isProcessing || _selectedSlot == Enum_EquipType.Max)
                return;

            if (EquipmentManager.Instance.DoStarforceRestoration(_selectedSlot))
            {
                RefreshSelectedSlot();
            }
        }
        //------------------------------------------------------------------------------------
    }
}
