using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.UI;

namespace GameBerry.UI
{
    /// <summary>
    /// 인벤 아이템 클릭 시 뜨는 설명/행동 다이얼로그.
    /// - Stack/Instance를 ItemHandle로 통일해서 받는다.
    /// - 실제 강화/장착/판매 UI는 프로젝트에 맞춰 버튼을 연결/확장하면 됨.
    /// </summary>
    public class ItemDescDialog : IDialog
    {
        [Header("UI")]
        [SerializeField] private UIItemElement _uIItemElement;
        [SerializeField] private TMP_Text _itemName;
        [SerializeField] private TMP_Text _itemType;
        [SerializeField] private TMP_Text _itemRarity;
        [SerializeField] private TMP_Text _itemDesc;

        [Header("EquipView")]
        [SerializeField] private Transform _equipStatViewGroup;
        [SerializeField] private TMP_Text _equipType;

        [SerializeField] private ScrollRect _equipStatScroll;  // optional
        [SerializeField] private Transform _equipStatContent;  // ScrollRect Content
        [SerializeField] private UIStatElement _equipStatViewPrefab; // (TMP_Text name, TMP_Text value) 프리팹

        [SerializeField] private Transform _equipStatLine;

        [SerializeField] private TMP_Text _equipMetaAddStatCount;

        private List<UIStatElement> _spawnStatElement = new List<UIStatElement>();

        [Header("Buttons")]
        [SerializeField] private Button consumeButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button sellButton;

        private ItemHandle _handle;

        // --------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (consumeButton != null) 
                consumeButton.onClick.AddListener(OnConsume);

            if (equipButton != null) 
                equipButton.onClick.AddListener(OnEquip);

            if (sellButton != null) 
                sellButton.onClick.AddListener(OnSell);
        }
        // --------------------------------------------------------------------
        public void Bind(ItemHandle handle)
        {
            _handle = handle;

            if (_uIItemElement != null)
                _uIItemElement.Bind(_handle);

            // 최소 정보 표시(메타 차트 연동은 프로젝트 확장 포인트)
            if (_itemName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_itemName, ItemManager.Instance.GetItemNameLocalKey(handle.itemId));

            if (_itemType != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_itemType, ItemManager.Instance.GetItemType(handle.itemId).ToString());

            if (_itemRarity != null)
            {
                Enum_Rarity enum_Rarity = ItemManager.Instance.GetItemRarity(handle.itemId);
                Managers.LocalStringManager.Instance.SetLocalizeText(_itemRarity, enum_Rarity.ToString());
                _itemRarity.color = StaticResource.Instance.GetRarityTextColor(enum_Rarity);
            }

            Enum_ItemType enum_ItemType = ItemManager.Instance.GetItemType(handle.itemId);
            if (enum_ItemType == Enum_ItemType.Equip)
            {
                if (_itemDesc != null)
                    _itemDesc.gameObject.SetActive(false);

                ShowEquipStat(handle);
            }
            else
            {
                if (_equipStatViewGroup != null)
                    _equipStatViewGroup.gameObject.SetActive(false);

                if (_itemDesc != null)
                {
                    _itemDesc.gameObject.SetActive(true);
                    Managers.LocalStringManager.Instance.SetLocalizeText(_itemDesc, ItemManager.Instance.GetItemDescLocalKey(handle.itemId));
                }
                // 버튼 노출 제어(기본)
                if (equipButton != null) equipButton.gameObject.SetActive(false);
            }


            //// consume/sell은 프로젝트 룰에 따라 조절 가능
            if (consumeButton != null)
            {
                if (handle.isMeta == false)
                    consumeButton.gameObject.SetActive(true);
                else
                    consumeButton.gameObject.SetActive(false);
            }
            //if (sellButton != null) sellButton.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        private void ShowEquipStat(ItemHandle handle)
        {
            Chart.EquipInfo equipInfo = Chart.GameChart.Get<Chart.EquipChart>()?.Get(handle.itemId);

            if (_equipStatViewGroup != null)
                _equipStatViewGroup.gameObject.SetActive(true);

            if (equipInfo == null)
            {
                if (equipButton != null)
                    equipButton.gameObject.SetActive(false);
                if (_equipType != null) 
                    _equipType.gameObject.SetActive(false);
                return;
            }

            if (_equipType != null)
                _equipType.SetText(equipInfo.EquipType.ToString());

            int idx = 0;

            if (equipInfo != null)
            {
                foreach (var pair in equipInfo.GetBaseStats())
                { // baseStat UI에 표시
                    AddStatLine(pair.Key, pair.Value, idx);
                    idx++;
                }
            }

            if (_equipStatLine != null)
                _equipStatLine.SetAsLastSibling();

            if (handle.isMeta == true)
            {
                Enum_Rarity enum_Rarity = ItemManager.Instance.GetItemRarity(handle.itemId);
                if (Chart.GameChart.Get<Chart.EquipRarityRuleChart>().TryGetRandomRule(enum_Rarity, out var rule))
                {
                    if (_equipMetaAddStatCount != null)
                    {
                        _equipMetaAddStatCount.SetText("AddStat{0}~{1}", rule.RandomStatMin, rule.RandomStatMax);
                        _equipMetaAddStatCount.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (_equipMetaAddStatCount != null)
                        _equipMetaAddStatCount.gameObject.SetActive(false);
                }

                if (equipButton != null) 
                    equipButton.gameObject.SetActive(false);
            }
            else
            {
                if (_equipMetaAddStatCount != null)
                    _equipMetaAddStatCount.gameObject.SetActive(false);

                Table.EquipmentData data = EquipmentManager.Instance.GetEquipmentData(handle);

                if (data != null)
                {
                    foreach (var pair in data.addStatList)
                    { // addStat UI에 표시
                        AddStatLine(pair.stat, pair.value, idx);
                        idx++;
                    }
                }

                if (equipButton != null) 
                    equipButton.gameObject.SetActive(!EquipmentManager.Instance.IsEquip(handle));
            }

            for (int i = idx; i < _spawnStatElement.Count; ++i)
            {
                _spawnStatElement[i].gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void AddStatLine(Enum_Stat stat, double value, int lineIdx)
        {
            UIStatElement uIStatElement = null;

            if (lineIdx < _spawnStatElement.Count)
            {
                uIStatElement = _spawnStatElement[lineIdx];
            }
            else
            {
                var go = Instantiate(_equipStatViewPrefab, _equipStatContent);
                uIStatElement = go.GetComponent<UIStatElement>();
                if (uIStatElement == null)
                    return;

                _spawnStatElement.Add(uIStatElement);
            }

            uIStatElement.SetStatView(stat, value);
            uIStatElement.transform.SetAsLastSibling();
            uIStatElement.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        private void OnConsume()
        {
            // 기본: 1개 소모(스택은 1, 인스턴스는 해당 1개)
            var res = ItemManager.Instance.Consume(_handle, 1, true);
            Debug.Log($"[ItemDescDialog] Consume {_handle} => {res.Success} ({res.Reason})");

            // 소모 후 닫기 (원하면 유지)
            Exit();
        }
        //------------------------------------------------------------------------------------
        private void OnEquip()
        {
            // 장착 시스템은 프로젝트별이므로 연결만 해두고 확장
            if (EquipmentManager.Instance.SetEquip(_handle))
                ShowEquipStat(_handle);
        }
        //------------------------------------------------------------------------------------
        private void OnSell()
        {
            Debug.Log($"[ItemDescDialog] Sell requested: {_handle}");
            // 보통 판매는 인스턴스는 1개, 스택은 입력 수량 기반
            // 일단 1개 판매=1개 소모로 가정
            var res = ItemManager.Instance.Consume(_handle, 1, true);
            Debug.Log($"[ItemDescDialog] Sell(Consume) {_handle} => {res.Success} ({res.Reason})");
            Exit();
        }
        //------------------------------------------------------------------------------------
    }
}
