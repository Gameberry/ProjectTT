using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BackEnd;
using GameBerry.Contents;

namespace GameBerry.UI
{
    public class AuctionDialog : IDialog
    {
        private enum AuctionTab
        {
            Buy = 0,
            Sell = 1,
            History = 2
        }

        private enum HistoryTab
        {
            BuyHistory = 0,
            SellHistory = 1
        }

        [Header("Main Tabs")]
        [SerializeField] private UINumberBtn _buyTabButton;
        [SerializeField] private UINumberBtn _sellTabButton;
        [SerializeField] private UINumberBtn _historyTabButton;
        [SerializeField] private GameObject _buyRoot;
        [SerializeField] private GameObject _sellRoot;
        [SerializeField] private GameObject _historyRoot;

        [Header("Buy Tab")]
        [SerializeField] private TMP_InputField _searchItemIdInput;
        [SerializeField] private Button _searchButton;
        [SerializeField] private Button _searchRefreshButton;
        [SerializeField] private Transform _searchContentRoot;
        [SerializeField] private UIAuctionListItemElement _searchRowPrefab;

        [Header("Sell Tab")]
        [SerializeField] private TMP_InputField _sellItemIdInput;
        [SerializeField] private TMP_InputField _sellPriceInput;
        [SerializeField] private TMP_InputField _sellAmountInput;
        [SerializeField] private Button _addAuctionButton;
        [SerializeField] private Button _refreshMyAuctionButton;
        [SerializeField] private Button _recvSoldItemsButton;
        [SerializeField] private Transform _myAuctionContentRoot;
        [SerializeField] private UIAuctionListItemElement _myAuctionRowPrefab;

        [Header("History Tab")]
        [SerializeField] private UINumberBtn _buyHistoryTabButton;
        [SerializeField] private UINumberBtn _sellHistoryTabButton;
        [SerializeField] private GameObject _buyHistoryRoot;
        [SerializeField] private GameObject _sellHistoryRoot;
        [SerializeField] private Button _refreshHistoryButton;
        [SerializeField] private Transform _buyHistoryContentRoot;
        [SerializeField] private Transform _sellHistoryContentRoot;
        [SerializeField] private UIAuctionListItemElement _buyHistoryRowPrefab;
        [SerializeField] private UIAuctionListItemElement _sellHistoryRowPrefab;

        [Header("Status")]
        [SerializeField] private TMP_Text _statusText;

        private readonly List<UIAuctionListItemElement> _searchRows = new List<UIAuctionListItemElement>();
        private readonly List<UIAuctionListItemElement> _myRows = new List<UIAuctionListItemElement>();
        private readonly List<UIAuctionListItemElement> _buyHistoryRows = new List<UIAuctionListItemElement>();
        private readonly List<UIAuctionListItemElement> _sellHistoryRows = new List<UIAuctionListItemElement>();

        private AuctionTab _selectedTab = AuctionTab.Buy;
        private HistoryTab _selectedHistoryTab = HistoryTab.BuyHistory;
        private bool _isBusy = false;

        protected override void OnLoad()
        {
            BindTabButtons();
            BindInputContentTypes();
            BindActionButtons();
        }

        protected override void OnEnter()
        {
            SelectMainTab(AuctionTab.Buy);
            SelectHistoryTab(HistoryTab.BuyHistory);
            RefreshMyAuctionItemsAsync().Forget();
            RefreshHistoryAsync().Forget();
        }

        private void BindTabButtons()
        {
            if (_buyTabButton != null)
            {
                _buyTabButton.Num = (int)AuctionTab.Buy;
                _buyTabButton.AddListener += OnClickMainTab;
            }

            if (_sellTabButton != null)
            {
                _sellTabButton.Num = (int)AuctionTab.Sell;
                _sellTabButton.AddListener += OnClickMainTab;
            }

            if (_historyTabButton != null)
            {
                _historyTabButton.Num = (int)AuctionTab.History;
                _historyTabButton.AddListener += OnClickMainTab;
            }

            if (_buyHistoryTabButton != null)
            {
                _buyHistoryTabButton.Num = (int)HistoryTab.BuyHistory;
                _buyHistoryTabButton.AddListener += OnClickHistoryTab;
            }

            if (_sellHistoryTabButton != null)
            {
                _sellHistoryTabButton.Num = (int)HistoryTab.SellHistory;
                _sellHistoryTabButton.AddListener += OnClickHistoryTab;
            }
        }

        private void BindInputContentTypes()
        {
            if (_searchItemIdInput != null)
                _searchItemIdInput.contentType = TMP_InputField.ContentType.IntegerNumber;

            if (_sellItemIdInput != null)
                _sellItemIdInput.contentType = TMP_InputField.ContentType.IntegerNumber;

            if (_sellPriceInput != null)
                _sellPriceInput.contentType = TMP_InputField.ContentType.IntegerNumber;

            if (_sellAmountInput != null)
                _sellAmountInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        }

        private void BindActionButtons()
        {
            if (_searchButton != null)
                _searchButton.onClick.AddListener(OnClickSearchButton);

            if (_searchRefreshButton != null)
                _searchRefreshButton.onClick.AddListener(OnClickSearchButton);

            if (_addAuctionButton != null)
                _addAuctionButton.onClick.AddListener(OnClickAddAuctionButton);

            if (_refreshMyAuctionButton != null)
                _refreshMyAuctionButton.onClick.AddListener(OnClickRefreshMyAuctionButton);

            if (_recvSoldItemsButton != null)
                _recvSoldItemsButton.onClick.AddListener(OnClickRecvSoldItemsButton);

            if (_refreshHistoryButton != null)
                _refreshHistoryButton.onClick.AddListener(OnClickRefreshHistoryButton);
        }

        private void OnClickMainTab(int tabNum)
        {
            AuctionTab tab = AuctionTab.Buy;
            if (tabNum == (int)AuctionTab.Sell)
                tab = AuctionTab.Sell;
            else if (tabNum == (int)AuctionTab.History)
                tab = AuctionTab.History;

            SelectMainTab(tab);

            if (tab == AuctionTab.Sell)
                RefreshMyAuctionItemsAsync().Forget();
            else if (tab == AuctionTab.History)
                RefreshHistoryAsync().Forget();
        }

        private void OnClickHistoryTab(int tabNum)
        {
            HistoryTab tab = tabNum == (int)HistoryTab.SellHistory ? HistoryTab.SellHistory : HistoryTab.BuyHistory;
            SelectHistoryTab(tab);
        }

        private void SelectMainTab(AuctionTab tab)
        {
            _selectedTab = tab;

            if (_buyTabButton != null)
                _buyTabButton.SetSelected(tab == AuctionTab.Buy);

            if (_sellTabButton != null)
                _sellTabButton.SetSelected(tab == AuctionTab.Sell);

            if (_historyTabButton != null)
                _historyTabButton.SetSelected(tab == AuctionTab.History);

            if (_buyRoot != null)
                _buyRoot.SetActive(tab == AuctionTab.Buy);

            if (_sellRoot != null)
                _sellRoot.SetActive(tab == AuctionTab.Sell);

            if (_historyRoot != null)
                _historyRoot.SetActive(tab == AuctionTab.History);
        }

        private void SelectHistoryTab(HistoryTab tab)
        {
            _selectedHistoryTab = tab;

            if (_buyHistoryTabButton != null)
                _buyHistoryTabButton.SetSelected(tab == HistoryTab.BuyHistory);

            if (_sellHistoryTabButton != null)
                _sellHistoryTabButton.SetSelected(tab == HistoryTab.SellHistory);

            if (_buyHistoryRoot != null)
                _buyHistoryRoot.SetActive(tab == HistoryTab.BuyHistory);

            if (_sellHistoryRoot != null)
                _sellHistoryRoot.SetActive(tab == HistoryTab.SellHistory);
        }

        private void OnClickSearchButton()
        {
            SearchAuctionItemsAsync().Forget();
        }

        private void OnClickAddAuctionButton()
        {
            AddAuctionAsync().Forget();
        }

        private void OnClickRefreshMyAuctionButton()
        {
            RefreshMyAuctionItemsAsync().Forget();
        }

        private void OnClickRecvSoldItemsButton()
        {
            RecvSoldItemsAsync().Forget();
        }

        private void OnClickRefreshHistoryButton()
        {
            RefreshHistoryAsync().Forget();
        }

        private async UniTaskVoid SearchAuctionItemsAsync()
        {
            if (_isBusy)
                return;

            if (TryGetInt(_searchItemIdInput, out int itemId) == false)
            {
                SetStatus("Enter item id to search.");
                return;
            }

            _isBusy = true;
            try
            {
                List<Auction> items = await AuctionManager.Instance.GetAuctionItems(itemId);
                BindSearchRows(items);
                SetStatus($"Search done: {items.Count}");
            }
            catch (Exception e)
            {
                SetStatus($"Search failed: {e.Message}");
                GlobalContent.ShowPopup_Ok("Auction Error", e.Message);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async UniTaskVoid AddAuctionAsync()
        {
            if (_isBusy)
                return;

            if (TryGetInt(_sellItemIdInput, out int itemId) == false
                || TryGetInt(_sellPriceInput, out int price) == false
                || TryGetInt(_sellAmountInput, out int amount) == false)
            {
                SetStatus("Invalid input for add auction.");
                return;
            }

            if (price < 0 || amount <= 0)
            {
                SetStatus("Price >= 0 and Amount > 0 required.");
                return;
            }

            _isBusy = true;
            try
            {
                await AuctionManager.Instance.AddAuction(itemId, price, amount);
                SetStatus("Add auction done.");
                await RefreshMyAuctionItemsInternalAsync();
            }
            catch (Exception e)
            {
                SetStatus($"Add auction failed: {e.Message}");
                GlobalContent.ShowPopup_Ok("Auction Error", e.Message);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async UniTaskVoid RefreshMyAuctionItemsAsync()
        {
            if (_isBusy)
                return;

            _isBusy = true;
            try
            {
                await RefreshMyAuctionItemsInternalAsync();
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async UniTask RefreshMyAuctionItemsInternalAsync()
        {
            try
            {
                List<Auction> items = await AuctionManager.Instance.GetAllMyAuctionItems();
                items.Sort((a, b) => b.Id.CompareTo(a.Id));
                BindMyRows(items);
                SetStatus($"My list refreshed: {items.Count}");
            }
            catch (Exception e)
            {
                SetStatus($"My list refresh failed: {e.Message}");
                GlobalContent.ShowPopup_Ok("Auction Error", e.Message);
            }
        }

        private async UniTaskVoid RecvSoldItemsAsync()
        {
            if (_isBusy)
                return;

            _isBusy = true;
            try
            {
                await AuctionManager.Instance.RecvMySoldAuctionItems();
                await RefreshMyAuctionItemsInternalAsync();
                await RefreshHistoryInternalAsync();
            }
            catch (Exception e)
            {
                SetStatus($"Receive sold failed: {e.Message}");
                GlobalContent.ShowPopup_Ok("Auction Error", e.Message);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async UniTaskVoid RefreshHistoryAsync()
        {
            if (_isBusy)
                return;

            _isBusy = true;
            try
            {
                await RefreshHistoryInternalAsync();
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async UniTask RefreshHistoryInternalAsync()
        {
            try
            {
                List<Auction> myItems = await AuctionManager.Instance.GetAllMyAuctionItems();
                List<Auction> buyHistory = new List<Auction>();
                List<Auction> sellHistory = myItems.FindAll(x => x.Issold);
                sellHistory.Sort((a, b) => b.Id.CompareTo(a.Id));

                BindBuyHistoryRows(buyHistory);
                BindSellHistoryRows(sellHistory);
                SetStatus($"History refreshed: Buy {buyHistory.Count}, Sell {sellHistory.Count}");
            }
            catch (Exception e)
            {
                SetStatus($"History refresh failed: {e.Message}");
                GlobalContent.ShowPopup_Ok("Auction Error", e.Message);
            }
        }

        private async UniTaskVoid BuyAuctionAsync(Auction auction)
        {
            if (_isBusy || auction == null)
                return;

            _isBusy = true;
            try
            {
                await AuctionManager.Instance.BuyAuctionItem(auction);
                SetStatus($"Buy done: AuctionId {auction.Id}");

                await RefreshMyAuctionItemsInternalAsync();
                await RefreshHistoryInternalAsync();

                if (TryGetInt(_searchItemIdInput, out int itemId))
                {
                    List<Auction> items = await AuctionManager.Instance.GetAuctionItems(itemId);
                    BindSearchRows(items);
                }
            }
            catch (Exception e)
            {
                SetStatus($"Buy failed: {e.Message}");
                GlobalContent.ShowPopup_Ok("Auction Error", e.Message);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async UniTaskVoid CancelAuctionAsync(Auction auction)
        {
            if (_isBusy || auction == null)
                return;

            _isBusy = true;
            try
            {
                await AuctionManager.Instance.CancelAuctionItem(auction);
                SetStatus($"Cancel done: AuctionId {auction.Id}");
                await RefreshMyAuctionItemsInternalAsync();
            }
            catch (Exception e)
            {
                SetStatus($"Cancel failed: {e.Message}");
                GlobalContent.ShowPopup_Ok("Auction Error", e.Message);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private void BindSearchRows(List<Auction> items)
        {
            int count = items != null ? items.Count : 0;
            EnsureRows(_searchRows, _searchRowPrefab, _searchContentRoot, count);

            for (int i = 0; i < _searchRows.Count; ++i)
            {
                bool active = i < count;
                _searchRows[i].gameObject.SetActive(active);
                if (active == false)
                    continue;

                Auction auction = items[i];
                bool interactable = auction != null
                    && auction.Issold == false
                    && auction.Owner != Backend.UserInDate;

                string info = BuildAuctionInfo(auction);
                _searchRows[i].Bind(auction, info, "Buy", interactable, OnClickBuyRow);
            }
        }

        private void BindMyRows(List<Auction> items)
        {
            int count = items != null ? items.Count : 0;
            EnsureRows(_myRows, _myAuctionRowPrefab, _myAuctionContentRoot, count);

            for (int i = 0; i < _myRows.Count; ++i)
            {
                bool active = i < count;
                _myRows[i].gameObject.SetActive(active);
                if (active == false)
                    continue;

                Auction auction = items[i];
                bool canCancel = auction != null && auction.Issold == false;
                string action = canCancel ? "Cancel" : "Sold";

                string state = auction.Issold == false
                    ? "OPEN"
                    : "SOLD";

                string info = BuildAuctionInfo(auction) + $"  STATE:{state}";
                _myRows[i].Bind(auction, info, action, canCancel, OnClickCancelRow);
            }
        }

        private void BindBuyHistoryRows(List<Auction> items)
        {
            int count = items != null ? items.Count : 0;
            EnsureRows(_buyHistoryRows, _buyHistoryRowPrefab, _buyHistoryContentRoot, count);

            for (int i = 0; i < _buyHistoryRows.Count; ++i)
            {
                bool active = i < count;
                _buyHistoryRows[i].gameObject.SetActive(active);
                if (active == false)
                    continue;

                Auction auction = items[i];
                string info = $"ITEM:{auction.Itemid}  AMT:{auction.Amount}  PAID:{auction.Price}";
                _buyHistoryRows[i].Bind(auction, info, string.Empty, false, null);
            }
        }

        private void BindSellHistoryRows(List<Auction> items)
        {
            int count = items != null ? items.Count : 0;
            EnsureRows(_sellHistoryRows, _sellHistoryRowPrefab, _sellHistoryContentRoot, count);

            for (int i = 0; i < _sellHistoryRows.Count; ++i)
            {
                bool active = i < count;
                _sellHistoryRows[i].gameObject.SetActive(active);
                if (active == false)
                    continue;

                Auction auction = items[i];
                int fee = CalculateFee(auction.Price);
                int settlement = Mathf.Max(0, auction.Price - fee);
                string info = $"ITEM:{auction.Itemid}  AMT:{auction.Amount}  FEE:{fee}  NET:{settlement}";
                _sellHistoryRows[i].Bind(auction, info, string.Empty, false, null);
            }
        }

        private int CalculateFee(int price)
        {
            return Mathf.FloorToInt(price * 0.1f);
        }

        private void EnsureRows(List<UIAuctionListItemElement> rows, UIAuctionListItemElement prefab, Transform root, int count)
        {
            if (prefab == null || root == null)
                return;

            while (rows.Count < count)
            {
                UIAuctionListItemElement row = Instantiate(prefab, root);
                rows.Add(row);
            }
        }

        private string BuildAuctionInfo(Auction auction)
        {
            if (auction == null)
                return string.Empty;

            return $"ID:{auction.Id}  ITEM:{auction.Itemid}  AMT:{auction.Amount}  PRICE:{auction.Price}";
        }

        private void OnClickBuyRow(Auction auction)
        {
            BuyAuctionAsync(auction).Forget();
        }

        private void OnClickCancelRow(Auction auction)
        {
            CancelAuctionAsync(auction).Forget();
        }

        private bool TryGetInt(TMP_InputField input, out int value)
        {
            value = 0;
            if (input == null)
                return false;

            return int.TryParse(input.text, out value);
        }

        private void SetStatus(string message)
        {
            if (_statusText != null)
                _statusText.SetText(message ?? string.Empty);
        }
    }
}
