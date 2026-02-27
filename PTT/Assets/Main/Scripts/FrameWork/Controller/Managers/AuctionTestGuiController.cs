#if DEV_DEFINE || UNITY_EDITOR
using System;
using System.Collections.Generic;
using BACKND.Database;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameBerry
{
    public class AuctionTestGuiController : MonoBehaviour
    {
        private const float HoldToShowSeconds = 5.0f;

        private bool _isTouchBegin = false;
        private float _touchStartTime = 0.0f;
        private bool _isVisible = false;

        private Rect _windowRect = new Rect(0, 0, 0, 0);
        private Rect _safeAreaRect = new Rect(0, 0, 0, 0);
        private Vector2 _mainScrollPos = Vector2.zero;
        private Vector2 _scrollPos = Vector2.zero;
        private Vector2 _searchScrollPos = Vector2.zero;

        private string _itemIdInput = "1";
        private string _searchItemIdInput = "1";
        private string _priceInput = "100";
        private string _amountInput = "1";
        private string _statusText = string.Empty;

        private List<Auction> _myAuctions = new List<Auction>();
        private List<Auction> _searchAuctions = new List<Auction>();
        private bool _styleApplied = false;
        private EventSystem _blockedEventSystem = null;
        private bool _blockedEventSystemPrevEnabled = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindObjectOfType<AuctionTestGuiController>() != null)
                return;

            var go = new GameObject("[DEV]AuctionTestGuiController");
            DontDestroyOnLoad(go);
            go.AddComponent<AuctionTestGuiController>();
        }

        private void Start()
        {
            Show();
        }

        public void Show()
        {
            _isVisible = true;
            _statusText = "Auction Test Window Opened";
            Debug.Log("[AuctionTestGuiController] Show");
            ApplyUiBlockState();
        }

        private void ApplyGuiStyleIfNeeded()
        {
            if (_styleApplied)
                return;

            GUI.skin.label.fontSize = 22;
            GUI.skin.button.fontSize = 22;
            GUI.skin.textField.fontSize = 22;
            GUI.skin.window.fontSize = 24;
            _styleApplied = true;
        }

        private void Hide()
        {
            _isVisible = false;
            ApplyUiBlockState();
        }

        private void Update()
        {
            ApplyUiBlockState();

            if (Input.GetMouseButton(0))
            {
                if (_isTouchBegin == false)
                {
                    _isTouchBegin = true;
                    _touchStartTime = Time.unscaledTime;
                }
                else
                {
                    if (_isVisible == false && Time.unscaledTime - _touchStartTime >= HoldToShowSeconds)
                    {
                        _isTouchBegin = false;
                        _touchStartTime = 0.0f;
                        Show();
                    }
                }
            }
            else
            {
                _isTouchBegin = false;
                _touchStartTime = 0.0f;
            }
        }

        private void OnGUI()
        {
            if (_isVisible == false)
                return;

            ApplyGuiStyleIfNeeded();

            Rect safeArea = Screen.safeArea;
            _safeAreaRect.x = safeArea.x;
            _safeAreaRect.y = Screen.height - safeArea.y - safeArea.height;
            _safeAreaRect.width = safeArea.width;
            _safeAreaRect.height = safeArea.height;

            _windowRect.x = _safeAreaRect.x;
            _windowRect.y = _safeAreaRect.y;
            _windowRect.width = _safeAreaRect.width;
            _windowRect.height = _safeAreaRect.height;

            _windowRect = GUI.Window(GetInstanceID(), _windowRect, DrawWindow, "Auction Test");
        }

        private void OnDisable()
        {
            RestoreBlockedEventSystem();
        }

        private void OnDestroy()
        {
            RestoreBlockedEventSystem();
        }

        private void ApplyUiBlockState()
        {
            if (_isVisible == false)
            {
                RestoreBlockedEventSystem();
                return;
            }

            EventSystem current = EventSystem.current;
            if (current == null)
                return;

            if (_blockedEventSystem != current)
            {
                RestoreBlockedEventSystem();
                _blockedEventSystem = current;
                _blockedEventSystemPrevEnabled = current.enabled;
            }

            if (_blockedEventSystem != null && _blockedEventSystem.enabled)
                _blockedEventSystem.enabled = false;
        }

        private void RestoreBlockedEventSystem()
        {
            if (_blockedEventSystem == null)
                return;

            _blockedEventSystem.enabled = _blockedEventSystemPrevEnabled;
            _blockedEventSystem = null;
            _blockedEventSystemPrevEnabled = true;
        }

        private void DrawWindow(int windowId)
        {
            float padding = 16.0f;
            float closeWidth = 140.0f;
            float closeHeight = 46.0f;
            float topBarHeight = closeHeight + 8.0f;
            float innerWidth = Mathf.Max(100.0f, _windowRect.width - (padding * 2.0f));
            float innerHeight = Mathf.Max(100.0f, _windowRect.height - (padding * 2.0f));
            float bodyHeight = Mathf.Max(40.0f, innerHeight - topBarHeight);

            float myListHeight = Mathf.Clamp(bodyHeight * 0.38f, 140.0f, 320.0f);
            float searchListHeight = Mathf.Clamp(bodyHeight * 0.32f, 120.0f, 280.0f);

            GUILayout.BeginArea(new Rect(padding, padding, innerWidth, innerHeight));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", GUILayout.Width(closeWidth), GUILayout.Height(closeHeight)))
            {
                Hide();
            }
            GUILayout.EndHorizontal();

            _mainScrollPos = GUILayout.BeginScrollView(_mainScrollPos, GUILayout.Height(bodyHeight));
            GUILayout.BeginVertical();

            GUILayout.Label("Hold 5 seconds anywhere to open this test window.");
            GUILayout.Space(6);

            GUILayout.Label("Item Id");
            _itemIdInput = GUILayout.TextField(_itemIdInput);

            GUILayout.Label("Price");
            _priceInput = GUILayout.TextField(_priceInput);

            GUILayout.Label("Amount");
            _amountInput = GUILayout.TextField(_amountInput);

            GUILayout.Space(8);

            if (GUILayout.Button("Add Auction"))
            {
                AddAuctionAsync().Forget();
            }

            if (GUILayout.Button("Refresh My Auctions"))
            {
                RefreshMyAuctionsAsync().Forget();
            }

            GUILayout.Space(8);
            GUILayout.Label("Search Item Id");
            _searchItemIdInput = GUILayout.TextField(_searchItemIdInput);
            if (GUILayout.Button("Search Auctions By Item Id"))
            {
                SearchAuctionItemsAsync().Forget();
            }

            if (GUILayout.Button("Receive Sold Auction Items"))
            {
                ReceiveSoldItemsAsync().Forget();
            }

            GUILayout.Space(8);
            GUILayout.Label("Status: " + _statusText);
            GUILayout.Space(8);

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(myListHeight));
            for (int i = 0; i < _myAuctions.Count; ++i)
            {
                Auction auction = _myAuctions[i];
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(
                    string.Format(
                        "Id:{0} Item:{1} Amount:{2} Price:{3} Sold:{4}",
                        auction.Id,
                        auction.Itemid,
                        auction.Amount,
                        auction.Price,
                        auction.Issold),
                    GUILayout.Width(340));

                if (auction.Issold == false && GUILayout.Button("Cancel", GUILayout.Width(80)))
                {
                    CancelAuctionAsync(auction).Forget();
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            GUILayout.Space(8);
            GUILayout.Label("Search Result");
            _searchScrollPos = GUILayout.BeginScrollView(_searchScrollPos, GUILayout.Height(searchListHeight));
            for (int i = 0; i < _searchAuctions.Count; ++i)
            {
                Auction auction = _searchAuctions[i];
                GUILayout.BeginVertical("box");
                GUILayout.Label(
                    string.Format(
                        "Id:{0} Owner:{1} Item:{2} Amount:{3} Price:{4} Sold:{5}",
                        auction.Id,
                        auction.Owner,
                        auction.Itemid,
                        auction.Amount,
                        auction.Price,
                        auction.Issold));

                bool canBuy = auction.Issold == false;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.enabled = canBuy;
                if (GUILayout.Button("Buy", GUILayout.Width(120), GUILayout.Height(32)))
                {
                    BuyAuctionAsync(auction).Forget();
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();

            GUILayout.Space(8);
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndArea();

        }

        private async UniTaskVoid AddAuctionAsync()
        {
            if (TryParseInputs(out int itemId, out int price, out int amount) == false)
                return;

            try
            {
                await AuctionManager.Instance.AddAuction(itemId, price, amount);
                _statusText = "AddAuction success";
                await RefreshMyAuctionsInternalAsync();
            }
            catch (Exception e)
            {
                _statusText = "AddAuction failed: " + e.Message;
                Debug.LogError("[AuctionTestGuiController] " + e);
            }
        }

        private async UniTaskVoid RefreshMyAuctionsAsync()
        {
            await RefreshMyAuctionsInternalAsync();
        }

        private async UniTask RefreshMyAuctionsInternalAsync()
        {
            try
            {
                _myAuctions = await AuctionManager.Instance.GetAllMyAuctionItems();
                if (_myAuctions == null)
                    _myAuctions = new List<Auction>();

                _statusText = "Refresh success. Count: " + _myAuctions.Count;
            }
            catch (Exception e)
            {
                _statusText = "Refresh failed: " + e.Message;
                Debug.LogError("[AuctionTestGuiController] " + e);
            }
        }

        private async UniTaskVoid CancelAuctionAsync(Auction auction)
        {
            try
            {
                await AuctionManager.Instance.CancelAuctionItem(auction);
                _statusText = "Cancel success. AuctionId: " + auction.Id;
                await RefreshMyAuctionsInternalAsync();
            }
            catch (Exception e)
            {
                _statusText = "Cancel failed: " + e.Message;
                Debug.LogError("[AuctionTestGuiController] " + e);
            }
        }

        private async UniTaskVoid ReceiveSoldItemsAsync()
        {
            try
            {
                await AuctionManager.Instance.RecvMySoldAuctionItems();
                _statusText = "Receive sold items success";
                await RefreshMyAuctionsInternalAsync();
            }
            catch (Exception e)
            {
                _statusText = "Receive sold items failed: " + e.Message;
                Debug.LogError("[AuctionTestGuiController] " + e);
            }
        }

        private async UniTaskVoid SearchAuctionItemsAsync()
        {
            if (int.TryParse(_searchItemIdInput, out int searchItemId) == false)
            {
                _statusText = "Invalid search item id.";
                return;
            }

            try
            {
                _searchAuctions = await AuctionManager.Instance.GetAuctionItems(searchItemId);
                if (_searchAuctions == null)
                    _searchAuctions = new List<Auction>();

                _statusText = "Search success. Count: " + _searchAuctions.Count;
            }
            catch (Exception e)
            {
                _statusText = "Search failed: " + e.Message;
                Debug.LogError("[AuctionTestGuiController] " + e);
            }
        }

        private async UniTaskVoid BuyAuctionAsync(Auction auction)
        {
            try
            {
                await AuctionManager.Instance.BuyAuctionItem(auction);
                _statusText = "Buy processed. AuctionId: " + auction.Id;
                await RefreshMyAuctionsInternalAsync();

                if (int.TryParse(_searchItemIdInput, out int searchItemId))
                {
                    _searchAuctions = await AuctionManager.Instance.GetAuctionItems(searchItemId);
                    if (_searchAuctions == null)
                        _searchAuctions = new List<Auction>();
                }
            }
            catch (Exception e)
            {
                _statusText = "Buy failed: " + e.Message;
                Debug.LogError("[AuctionTestGuiController] " + e);
            }
        }

        private bool TryParseInputs(out int itemId, out int price, out int amount)
        {
            itemId = 0;
            price = 0;
            amount = 0;

            if (int.TryParse(_itemIdInput, out itemId) == false
                || int.TryParse(_priceInput, out price) == false
                || int.TryParse(_amountInput, out amount) == false)
            {
                _statusText = "Invalid input. ItemId/Price/Amount must be int.";
                return false;
            }

            if (price < 0 || amount <= 0)
            {
                _statusText = "Invalid range. Price >= 0, Amount > 0";
                return false;
            }

            return true;
        }
    }
}
#endif
