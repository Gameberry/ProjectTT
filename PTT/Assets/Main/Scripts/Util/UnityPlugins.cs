using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;
using AppsFlyerSDK;
using Firebase;
using Firebase.Auth;
using Google;
//using AppleAuth;
using CodeStage.AntiCheat.Storage;

#if UNITY_ANDROID
//using Google.Play.Review;
#endif

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
using AudienceNetwork;
#endif

public class UnityPlugins : MonoBehaviour
{
    static UnityPlugins _instance;
    public static UnityPlugins instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UnityPlugins>(true);

                if (_instance == null)
                {
                    _instance = new GameObject("[ PLUGINS ]").AddComponent<UnityPlugins>();
                }
            }
            return _instance;
        }
    }

    [SerializeField]
    bool _initializeOnAwake = false;

    [SerializeField]
    UnityIAP _iap = new UnityIAP();
    public static UnityIAP iap => instance._iap;

    [SerializeField]
    UnityAppLovin _appLovin = new UnityAppLovin();
    public static UnityAppLovin appLovin => instance._appLovin;

    [SerializeField]
    UnityAppsFlyer _appsFlyer = new UnityAppsFlyer();
    public static UnityAppsFlyer appsFlyer => instance._appsFlyer;

    [SerializeField]
    UnityFacebook _facebook = new UnityFacebook();
    public static UnityFacebook facebook => instance._facebook;

    //[SerializeField]
    //UnityGoogleLogin _googleLogin = new UnityGoogleLogin();
    //public static UnityGoogleLogin googleLogin => instance._googleLogin;

    //[SerializeField]
    //UnityInAppReview _inAppReview = new UnityInAppReview();
    //public static UnityInAppReview inAppReview => instance._inAppReview;

    //[SerializeField]
    //UnityAppleLogin _appleLogin = new UnityAppleLogin();
    //public static UnityAppleLogin appleLogin => instance._appleLogin;

    //[SerializeField]
    //UnityAppTrackingTransparency _att = new UnityAppTrackingTransparency();
    //public static UnityAppTrackingTransparency att => instance._att;

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (_initializeOnAwake)
        {
            appsFlyer.Initialize();
            facebook.Initialize();
            //googleLogin.Initialize();


            iap.Initialize();
        }
    }

    //void Update()
    //{
    //    appleLogin.Update();
    //}

    public static void SetTimer(float delay, Action onAction)
    {
        instance.StartCoroutine(instance.TimerCoroutine(delay, onAction));
    }

    IEnumerator TimerCoroutine(float delay, Action onAction)
    {
        yield return new WaitForSeconds(delay);

        onAction?.Invoke();
    }
}

#region InAppPurchase

[Serializable]
public class UnityIAP : IDetailedStoreListener
{
    [Serializable]
    public class Item
    {
        public string productId;
        public ProductType productType;
        public string localizedPriceString;
        public string localizedTitle;
        public string localizedDescription;
        public string isoCurrencyCode;
        public decimal localizedPrice;
    }

    public bool debugLog;
    public List<Item> items = new List<Item>();

    public string AutoInsertStr = string.Empty;

    public void AutoInsertFunc()
    {
        string[] arr = AutoInsertStr.Split(',');
        for (int i = 0; i < arr.Length; ++i)
        {
            Item item = new Item();
            item.productId = arr[i];
            item.productType = ProductType.Consumable;

            items.Add(item);
        }
    }

    IStoreController _controller;
    IExtensionProvider _extensions;
    Action<Product> _onPurchaseSuccess;
    Action<Product, string> _onPurchaseFail;
    Product _purchasedProduct;
    ConfigurationBuilder builder;
    public void Initialize()
    {
        if (debugLog) Debug.Log("[InAppPurchase] Initialize()");

        builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        items.ForEach(item => builder.AddProduct(item.productId, item.productType));

        UnityPurchasing.Initialize(this, builder);
    }

    public void AddItem(string productId, ProductType productType = ProductType.Consumable)
    {
        if (!string.IsNullOrEmpty(productId))
        {
            var item = items.Find(x => x.productId.Equals(productId));

            if (item == null)
            {
                if (debugLog) Debug.LogFormat("[InAppPurchase] AddItem() : {0}({1})", productId, productType);

                items.Add(new Item()
                {
                    productId = productId,
                    productType = productType,
                });
            }
        }
    }

    public Item GetItem(string productId)
    {
        return items.Find(x => x.productId.Equals(productId));
    }

    public bool IsInitialized()
    {
        return _controller != null && _extensions != null;
    }

    public void Purchase(string productId, Action<Product> onSuccess, Action<Product, string> onFail = null)
    {
        if (IsInitialized())
        {
            Product product = _controller.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                if (debugLog) Debug.Log("[InAppPurchase] Purchase Product : " + product.definition.id);

                _onPurchaseSuccess = onSuccess;
                _onPurchaseFail = onFail;
                _controller.InitiatePurchase(productId);
            }
        }
        else
        {
            throw new Exception("[InAppPurchase] NOT INITIALIZED!");
        }
    }

    public void ConfirmPendingPurchase()
    {
        if (_purchasedProduct != null)
        {
            _controller.ConfirmPendingPurchase(_purchasedProduct);
            _purchasedProduct = null;
        }
    }

    public string GetOrderId(Product product)
    {
        string orderid = string.Empty;

        try
        {
            if (product.receipt == null)
                return null;

            var receiptWrapper = MiniJson.JsonDecode(product.receipt) as Dictionary<string, object>;
            var payload = MiniJson.JsonDecode(receiptWrapper["Payload"] as string) as Dictionary<string, object>;
            var json = MiniJson.JsonDecode(payload["json"] as string) as Dictionary<string, object>;

            orderid = json.ContainsKey("orderId") ? json["orderId"].ToString() : null;
        }
        catch (Exception ex)
        {
        }

        return orderid;
    }

    #region IStoreListener

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        if (debugLog) Debug.Log("[InAppPurchase] OnInitialized");

        _controller = controller;
        _extensions = extensions;

        foreach (var item in items)
        {
            var product = _controller.products.WithID(item.productId);

            item.localizedPriceString = product.metadata.localizedPriceString;
            item.localizedTitle = product.metadata.localizedTitle;
            item.localizedDescription = product.metadata.localizedDescription;
            item.isoCurrencyCode = product.metadata.isoCurrencyCode;
            item.localizedPrice = product.metadata.localizedPrice;
        }

        //if (GameBerry.Managers.ShopManager.isAlive == true)
        //    GameBerry.Managers.ShopManager.Instance.RefreshPrice();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        if (debugLog) Debug.LogFormat("[InAppPurchase] OnInitializeFailed : {0}", error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        if (debugLog) Debug.LogFormat("[InAppPurchase] OnInitializeFailed : {0}\n{1}", error, message);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        _purchasedProduct = e.purchasedProduct;

        bool isLocalValidated = true;

        //#if  !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        //        if (localValidation)
        //        {
        //            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
        //
        //            try
        //            {
        //                IPurchaseReceipt[] receipts = validator.Validate(e.purchasedProduct.receipt);
        //
        //                for (int i = 0; i < receipts.Length; i++)
        //                {
        //                    Analytics.Transaction(receipts[i].productID, e.purchasedProduct.metadata.localizedPrice, e.purchasedProduct.metadata.isoCurrencyCode, receipts[i].transactionID, null);
        //                }
        //            }
        //            catch (IAPSecurityException)
        //            {
        //                isLocalValidated = false;
        //            }
        //        }
        //#endif

        ObscuredPrefs.Set("transactionID", e.purchasedProduct.transactionID);
        ObscuredPrefs.Set("receipt", e.purchasedProduct.receipt);
        ObscuredPrefs.Set("pid", e.purchasedProduct.definition.id);
        ObscuredPrefs.Set("iapPrice", e.purchasedProduct.metadata.localizedPrice);
        ObscuredPrefs.Set("iapCurrency", e.purchasedProduct.metadata.isoCurrencyCode);
        ObscuredPrefs.Set("orderid", GetOrderId(e.purchasedProduct));
        
        ObscuredPrefs.Save();

        if (isLocalValidated && e.purchasedProduct.hasReceipt)
        {
            if (items.Any(item => item.productId.Equals(e.purchasedProduct.definition.id)))
            {
                if (debugLog) Debug.Log("[InAppPurchase] PURCHASE SUCCESS\n" + e.purchasedProduct.receipt);

                _onPurchaseSuccess?.Invoke(e.purchasedProduct);
            }
            else
            {
                if (debugLog) Debug.LogError("[InAppPurchase] NOT FOUND PRODUCT ID : " + e.purchasedProduct.definition.id);

                _onPurchaseFail?.Invoke(e.purchasedProduct, PurchaseFailureReason.ProductUnavailable.ToString());
            }
        }
        else
        {
            if (debugLog) Debug.Log("[InAppPurchase] PURCHASE FAIL : " + e.purchasedProduct.definition.id);

            _onPurchaseFail?.Invoke(e.purchasedProduct, PurchaseFailureReason.SignatureInvalid.ToString());
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        if (debugLog) Debug.Log("[InAppPurchase] OnPurchaseFailed : " + failureReason);

        _onPurchaseFail?.Invoke(product, failureReason.ToString());
    }

    public void OnPurchaseFailed(Product product, UnityEngine.Purchasing.Extension.PurchaseFailureDescription failureDescription)
    {
        if (debugLog) Debug.Log("[InAppPurchase] OnPurchaseFailed : " + failureDescription.message);

        _onPurchaseFail?.Invoke(product, failureDescription.message);
    }

    #endregion
}

#endregion

#region AppLovin

[Serializable]
public class UnityAppLovin
{
    public bool debugLog;

    public string maxSdkKey;

    public bool attState = false;

    public string googleBannerAdUnitId;
    public string googleRewardedAdUnitId;

    public string iosBannerAdUnitId;
    public string iosRewardedAdUnitId;

    bool _isInitialized;
    bool _isBannerShowing;

    int _interstitialRetryAttempt;
    int _rewardedRetryAttempt;
    int _rewardedInterstitialRetryAttempt;

    string _bannerAdUnitId;
    string _rewardedAdUnitId;

    public void Initialize(Action onDone = null)
    {
        if (string.IsNullOrEmpty(maxSdkKey))
        {
            new System.Exception("[AppLovin] SDK Key is empty!");
        }

        if (!_isInitialized)
        {
            if (debugLog) Debug.Log("[AppLovin] Initialize()");

            _bannerAdUnitId = (Application.platform == RuntimePlatform.Android) ? googleBannerAdUnitId : iosBannerAdUnitId;
            _rewardedAdUnitId = (Application.platform == RuntimePlatform.Android) ? googleRewardedAdUnitId : iosRewardedAdUnitId;

#if UNITY_EDITOR
            if (string.IsNullOrEmpty(_rewardedAdUnitId) == true)
                _rewardedAdUnitId = googleRewardedAdUnitId;
#endif

#if UNITY_IOS && !UNITY_EDITOR
        AdSettings.SetDataProcessingOptions(new string[] { });
#endif

            Debug.Log("AdRewardID : " + _rewardedAdUnitId);


            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
            {
                _isInitialized = true;

                if (debugLog) Debug.Log("[AppLovin] Initialized!");

#if UNITY_IOS || UNITY_IPHONE //|| UNITY_EDITOR
            if (MaxSdkUtils.CompareVersions(UnityEngine.iOS.Device.systemVersion, "14.5") != MaxSdkUtils.VersionComparisonResult.Lesser)
            {
                // Note that App transparency tracking authorization can be checked via `sdkConfiguration.AppTrackingStatus` for Unity Editor and iOS targets
                // 1. Set Meta ATE flag here, THEN
                
                AdSettings.SetAdvertiserTrackingEnabled(attState);

                //FB.Mobile.SetAdvertiserTrackingEnabled(isATTState);

                if (debugLog) Debug.Log("sdkConfiguration.AppTrackingStatus " + sdkConfiguration.AppTrackingStatus);
            }

#endif
                if (!string.IsNullOrEmpty(_bannerAdUnitId))
                {
                    InitializeBannerAds();
                }
                if (!string.IsNullOrEmpty(_rewardedAdUnitId))
                {
                    InitializeRewardedAds();
                }

                onDone?.Invoke();
            };

            Debug.Log("SetSdkKey : " + maxSdkKey);

            MaxSdk.SetSdkKey(maxSdkKey);
            MaxSdk.InitializeSdk();
        }
    }

    public void SetUserId(string userId)
    {
        MaxSdk.SetUserId(userId);
    }

    #region Banner Ad Methods

    void InitializeBannerAds()
    {
        // Attach Callbacks
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;

        // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
        // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
        MaxSdk.CreateBanner(_bannerAdUnitId, MaxSdkBase.BannerPosition.TopCenter);

        // Set background or background color for banners to be fully functional.
        MaxSdk.SetBannerBackgroundColor(_bannerAdUnitId, Color.black);
    }

    void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Banner ad is ready to be shown.
        // If you have already called MaxSdk.ShowBanner(BannerAdUnitId) it will automatically be shown on the next ad refresh.
        if (debugLog) Debug.Log("[AppLovin] Banner ad loaded");
    }

    void OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Banner ad failed to load. MAX will automatically try loading a new ad internally.
        if (debugLog) Debug.Log("[AppLovin] Banner ad failed to load with error code: " + errorInfo.Code);
    }

    void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        if (debugLog) Debug.Log("[AppLovin] Banner ad clicked");
    }

    void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Banner ad revenue paid. Use this callback to track user revenue.
        if (debugLog) Debug.Log("[AppLovin] Banner ad revenue paid");

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD" in most cases!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to
    }

    #endregion

    #region Rewarded Ad Methods

    Action _onRewardedAdComplete;
    Action<string> _onRewardedAdFail;

    void InitializeRewardedAds()
    {
        // Attach callbacks
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;

        Debug.Log("InitializeRewardedAds");

        // Load the first RewardedAd
        LoadRewardedAd();
    }

    void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(_rewardedAdUnitId);

        Debug.Log("LoadRewardedAd");
    }

    public void ShowRewardedAd(Action onComplete, Action<string> onFail = null)
    {
        if (GameBerry.Define.IsAdFree == true)
        {
            //GameBerry.Managers.QuestManager.Instance.AddMissionCount(GameBerry.V2Enum_QuestGoalType.WatchingAd, 1);

            if (onComplete != null)
                onComplete();

            return;
        }

        if (MaxSdk.IsRewardedAdReady(_rewardedAdUnitId))
        {
            _onRewardedAdComplete = onComplete;
            _onRewardedAdFail = onFail;

            MaxSdk.ShowRewardedAd(_rewardedAdUnitId);

            UnityPlugins.SetTimer(1f, LoadRewardedAd);
        }
        else
        {
            if (debugLog) Debug.Log("[AppLovin] Rewarded Ad not ready");
            GameBerry.Contents.GlobalContent.ShowGlobalNotice(GameBerry.Managers.LocalStringManager.Instance.GetLocalString("Common_Message_No_Ad"));
            LoadRewardedAd();
        }
    }

    void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
        if (debugLog) Debug.Log("[AppLovin] OnRewardedAdLoadedEvent: " + adUnitId);

        _rewardedAdUnitId = adUnitId;
        _rewardedRetryAttempt = 0;
    }

    void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
        _rewardedRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, _rewardedRetryAttempt));

        UnityPlugins.SetTimer((float)retryDelay, LoadRewardedAd);
    }

    void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        _onRewardedAdFail?.Invoke("Rewarded ad failed to display with error code: " + errorInfo.Code);

        LoadRewardedAd();
    }

    void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LoadRewardedAd();
    }

    void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        //GameBerry.Managers.TimeManager.Instance.RefreshServerTime();
        //GameBerry.Managers.QuestManager.Instance.AddMissionCount(GameBerry.V2Enum_QuestGoalType.WatchingAd, 1);
        _onRewardedAdComplete?.Invoke();
    }

    void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad revenue paid. Use this callback to track user revenue.
        if (debugLog) Debug.Log("[AppLovin] OnRewardedAdRevenuePaidEvent: " + adUnitId);

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD" in most cases!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to
    }

    #endregion
}

#endregion

#region AppsFlyer

[Serializable]
public class UnityAppsFlyer
{
    public bool debugLog;
    public string devKey;
    public string iosAppId;
    public string playstoreLicenseKey;

    Dictionary<string, string> _logParamDic = new Dictionary<string, string>();

    public void Initialize()
    {
        if (debugLog) Debug.LogFormat("[AppsFlyer] Initialize()\n{0}", devKey);

        if (string.IsNullOrEmpty(devKey))
        {
            throw new System.Exception("[AppsFlyer] DEVKEY IS EMPTY!");
        }

        if (Application.platform == RuntimePlatform.Android && string.IsNullOrEmpty(playstoreLicenseKey))
        {
            throw new System.Exception("[AppsFlyer] Playstore LicenseKey Empty!");
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer && string.IsNullOrEmpty(iosAppId))
        {
            throw new System.Exception("[AppsFlyer] iOS AppID Empty!");
        }

        AppsFlyer.setIsDebug(debugLog);
        //AppsFlyer.setIsDebug(false);
        AppsFlyer.initSDK(devKey, iosAppId);

#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
        SetIDFA();
#endif

        AppsFlyer.OnRequestResponse += AppsFlyerOnRequestResponse;

        AppsFlyer.enableTCFDataCollection(true);
        AppsFlyer.startSDK();

        UnityPlugins.appLovin.Initialize();
        //UnityPlugins.appleLogin.Initialize();
    }

    void AppsFlyerOnRequestResponse(object sender, EventArgs e)
    {
        var args = e as AppsFlyerRequestEventArgs;
        AppsFlyer.AFLog("AppsFlyerOnRequestResponse", " status code " + args.statusCode);
        if (debugLog) Debug.Log("[AppsFlyer] AppsFlyerOnRequestResponse : " + args.statusCode);
    }

    public void CheckATTState()
    {
#if UNITY_IOS
        Debug.LogFormat("ATT : " + ATTrackingStatusBinding.GetAuthorizationTrackingStatus());
#endif
    }

#if UNITY_IOS
    private void SetIDFA()
    {
        if (debugLog) Debug.LogFormat("SetIDFA");

        if (debugLog) Debug.LogFormat("ATT : " + ATTrackingStatusBinding.GetAuthorizationTrackingStatus());

        if (PlayerPrefs.GetInt("initattt", 0) == 0)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();

            PlayerPrefs.SetInt("initattt", 1);
        }
        else
        {
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus()
      == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
            else if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED)
            {
                UnityPlugins.appLovin.attState = true;
                if (debugLog) Debug.LogFormat("ATT : true");
            }
            else
            {
                UnityPlugins.appLovin.attState = false;
                if (debugLog) Debug.LogFormat("ATT : false");
            }
        }

        
    }
#endif

    public void SetUserId(string userId)
    {
        if (debugLog) Debug.Log("[AppsFlyer] SetCustomerUserId() : " + userId);

        AppsFlyer.setCustomerUserId(userId);
    }

    public void LogEvent(string eventName, Action<Dictionary<string, string>> setParams = null)
    {
        if (debugLog) Debug.Log("[AppsFlyer] LogEvent() : " + eventName);

        _logParamDic.Clear();

        setParams?.Invoke(_logParamDic);

        AppsFlyer.sendEvent(eventName, _logParamDic);
    }

    public void ValidateIAP(Product product)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            var receiptJson = (Dictionary<string, object>)AFMiniJSON.Json.Deserialize(product.receipt);
            var receiptPayload = (Dictionary<string, object>)AFMiniJSON.Json.Deserialize((string)receiptJson["Payload"]);
            if (receiptPayload != null)
            {
                string purchaseData = (string)receiptPayload["json"];
                string signature = (string)receiptPayload["signature"];

                AppsFlyer.validateAndSendInAppPurchase(
                    playstoreLicenseKey,
                    signature,
                    purchaseData,
                    product.metadata.localizedPrice.ToString(),
                    product.metadata.isoCurrencyCode,
                    null, null);
            }
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            LogEvent("af_purchase", dic =>
            {
                dic.Add("af_revenue", product.metadata.localizedPrice.ToString());
                dic.Add("af_currency", product.metadata.isoCurrencyCode);
                dic.Add("af_quantity", "1");
                dic.Add("af_platform", Application.platform == RuntimePlatform.Android ? "1" : "2");
                dic.Add("af_content_id", product.definition.id);
                dic.Add("af_order_id", product.receipt ?? "");
            });

            //AppsFlyer.validateAndSendInAppPurchase(
            //    product.definition.id,
            //    product.metadata.localizedPrice.ToString(),
            //    product.metadata.isoCurrencyCode,
            //    product.transactionID,
            //    null, null);
        }
    }
}

#endregion

#region Facebook

[Serializable]
public class UnityFacebook
{
//    public static class AdSettings
//    {
//#if UNITY_IOS
//        [DllImport("__Internal")]
//        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);
//#endif
//        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
//        {
//#if UNITY_IOS
//        FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
//#endif
//        }
//    }

    public bool debugLog;
    public List<string> permissions = new List<string>() { "public_profile", "email" };
    public string accessToken { get; private set; }

    public bool isLoggedIn => !string.IsNullOrEmpty(accessToken);

    public void Initialize(Action onDone = null)
    {
        if (!Facebook.Unity.FB.IsInitialized)
        {
            Facebook.Unity.FB.Init(() =>
            {
                Facebook.Unity.FB.ActivateApp();

#if UNITY_IOS
            Facebook.Unity.FB.Mobile.SetAdvertiserTrackingEnabled(true);
#endif

                onDone?.Invoke();
            });
        }
        else
        {
            Facebook.Unity.FB.ActivateApp();
        }
    }

    public void LoginWithReadPermissions(Action<string> onSuccess, Action<string> onFail)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            if (debugLog) Debug.Log("[Facebook] LoginWithReadPermissions()");

            Facebook.Unity.FB.LogInWithReadPermissions(permissions, res =>
            {
                if (Facebook.Unity.FB.IsLoggedIn && res.AccessToken != null)
                {
                    if (debugLog) Debug.Log("[Facebook] LoginWithReadPermissions() SUCCESS");

                    onSuccess?.Invoke(accessToken = res.AccessToken.TokenString);
                }
                else
                {
                    if (debugLog) Debug.Log("[Facebook] LoginWithReadPermissions() ERROR : " + res.Error);

                    onFail?.Invoke(string.IsNullOrEmpty(res.Error) ? "Login Canceled" : res.Error);
                }
            });
        }
    }

    public void RetrieveLoginStatus(Action<string> onSuccess, Action<string> onFail)
    {
        if (debugLog) Debug.Log("[Facebook] RetrieveLoginStatus()");

        Facebook.Unity.FB.Android.RetrieveLoginStatus(res =>
        {
            if (!string.IsNullOrEmpty(res.Error))
            {
                if (debugLog) Debug.Log("[Facebook] RetrieveLoginStatus() ERROR : " + res.Error);

                onFail?.Invoke(res.Error);
            }
            else if (res.Failed)
            {
                if (debugLog) Debug.Log("[Facebook] RetrieveLoginStatus() FAIL");

                onFail?.Invoke("Login Failed");
            }
            else
            {
                if (debugLog) Debug.Log("[Facebook] RetrieveLoginStatus() SUCCESS");

                onSuccess?.Invoke(accessToken = res.AccessToken.TokenString);
            }
        });
    }

    public void Logout()
    {
        if (!string.IsNullOrEmpty(accessToken))
        {
            if (debugLog) Debug.Log("[Facebook] Logout()");

            accessToken = "";

            //Facebook.Unity.FB.LogOut();
        }
    }
}

#endregion

//#region Google Login

//[Serializable]
//public class UnityGoogleLogin
//{
//    public bool debugLog;
//    public string webClientId = "401847770295-64hg99fvp8ttctrejbm8ifq52miv23b2.apps.googleusercontent.com";
//    public string idToken => _idToken;
//    public string authCode => _authCode;

//    public bool isLoggedIn => !string.IsNullOrEmpty(idToken);

//    Action<string, string> _onSuccess;
//    Action<string> _onFail;
//    string _idToken;
//    string _authCode;
//    string _errorMessage;

//    private FirebaseAuth auth;
//    private GoogleSignInConfiguration configuration;
//    public void Initialize()
//    {
//        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true, RequestAuthCode = true };
//        CheckFirebaseDependencies();
//    }

//    private void CheckFirebaseDependencies()
//    {
//        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
//        {
//            if (task.IsCompleted)
//            {
//                if (task.Result == DependencyStatus.Available)
//                    auth = FirebaseAuth.DefaultInstance;
//                else
//                    Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result.ToString());
//            }
//            else
//            {
//                Debug.LogError("Dependency check was not completed. Error : " + task.Exception.Message);
//            }
//        });
//    }

//    /// <summary>
//    /// 로그인 요청
//    /// onSuccess : authCode, idToken
//    /// onFail : errMessage
//    /// </summary>
//    public void Request(Action<string, string> onSuccess, Action<string> onFail)
//    {
//        _onSuccess = onSuccess;
//        _onFail = onFail;

//        GoogleSignIn.Configuration = configuration;
//        GoogleSignIn.Configuration.UseGameSignIn = false;
//        GoogleSignIn.Configuration.RequestIdToken = true;
//        Debug.Log("Calling SignIn");

//        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
//    }

//    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
//    {
//        if (task.IsFaulted)
//        {
//            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
//            {
//                if (enumerator.MoveNext())
//                {
//                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
//                    Debug.LogError("Got Error: " + error.Status + " " + error.Message);

//                    _errorMessage = error.Status + " " + error.Message;
//                    _onFail(_errorMessage);
//                }
//                else
//                {
//                    Debug.LogError("Got Unexpected Exception?!?" + task.Exception);

//                    _errorMessage = task.Exception.ToString();
//                    _onFail(_errorMessage);
//                }
//            }
//        }
//        else if (task.IsCanceled)
//        {
//            Debug.LogError("Canceled");

//            _errorMessage = "task.Canceled";
//            _onFail(_errorMessage);
//        }
//        else
//        {
//            Debug.Log("Welcome: " + task.Result.DisplayName + "!");
//            Debug.Log("Email = " + task.Result.Email);
//            Debug.Log("Google ID Token = " + task.Result.IdToken);
//            Debug.Log("AuthCode = " + task.Result.AuthCode);

//            _idToken = task.Result.IdToken;
//            _authCode = task.Result.AuthCode;

//            SignInWithGoogleOnFirebase(task.Result.IdToken);
//        }
//    }

//    private void SignInWithGoogleOnFirebase(string idToken)
//    {
//        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

//        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
//        {
//            AggregateException ex = task.Exception;
//            if (ex != null)
//            {
//                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
//                { 
//                    Debug.LogError("\nError code = " + inner.ErrorCode + " Message = " + inner.Message);
//                    _errorMessage = string.Format("\nError code = " + inner.ErrorCode + " Message = " + inner.Message);
//                    _onFail(_errorMessage);
//                }
//            }
//            else
//            {
//                Debug.Log("Sign In Successful.");

//                _onSuccess(authCode, idToken);
//            }
//        });
//    }

//    public void Logout()
//    {
//        if (!string.IsNullOrEmpty(_idToken))
//        {
//            if (debugLog) Debug.Log("[Google Login] Logout()");

//            _idToken = "";

//            GoogleSignIn.DefaultInstance.SignOut();
//        }
//    }
//}

//#endregion

//#region Apple Login

//[Serializable]
//public class UnityAppleLogin
//{
//    public bool debugLog;

//    [HideInInspector]
//    public string userId;

//    [HideInInspector]
//    public string userEmail;

//    [HideInInspector]
//    public string identityToken;

//    [HideInInspector]
//    public string authorizationCode;

//    [HideInInspector]
//    public string rawNonce;

//    public AppleAuth.Interfaces.IPersonName fulleName;

//    IAppleAuthManager _authManager;

//    public bool isInitialized => _authManager != null;

//    public void Initialize()
//    {
//        if (AppleAuthManager.IsCurrentPlatformSupported)
//        {
//            _authManager = new AppleAuthManager(new AppleAuth.Native.PayloadDeserializer());
//        }
//    }

//    public void Update()
//    {
//        if (_authManager != null)
//        {
//            _authManager.Update();
//        }
//    }

//    string GenerateNonceSHA256(string rawNonce)
//    {
//        var sha = new SHA256Managed();
//        var rawNonceUtf8 = Encoding.UTF8.GetBytes(rawNonce);
//        var hash = sha.ComputeHash(rawNonceUtf8);

//        string result = string.Empty;
//        for (int i = 0; i < hash.Length; i++)
//        {
//            result += hash[i].ToString("x2");
//        }
//        return result;
//    }

//    /// <summary>
//    /// 일반 로그인 요청: onSuccess, onError(reason)
//    /// </summary>
//    public void Request(Action<string> onSuccess, Action<string> onError)
//    {
//        if (_authManager != null)
//        {
//            if (debugLog) Debug.Log("[Apple Login] Request()");

//            rawNonce = System.Guid.NewGuid().ToString();

//            var nonce = GenerateNonceSHA256(rawNonce);
//            var loginArgs = new AppleAuthLoginArgs(AppleAuth.Enums.LoginOptions.IncludeEmail | AppleAuth.Enums.LoginOptions.IncludeFullName, nonce);

//            _authManager.LoginWithAppleId(loginArgs, credential =>
//            {
//                if (debugLog) Debug.Log("[Apple Login] Request(): SUCCESS");

//                var appleIdCredential = credential as AppleAuth.Interfaces.IAppleIDCredential;
//                if (appleIdCredential != null)
//                {
//                    userId = appleIdCredential.User;
//                    userEmail = appleIdCredential.Email;
//                    fulleName = appleIdCredential.FullName;
//                    identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0, appleIdCredential.IdentityToken.Length);
//                    authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode, 0, appleIdCredential.AuthorizationCode.Length);

//                    onSuccess?.Invoke(identityToken);
//                }

//            }, error =>
//            {
//                if (debugLog) Debug.Log("[Apple Login] Request(): ERROR " + error.LocalizedFailureReason);

//                onError?.Invoke(error.LocalizedFailureReason);
//            });
//        }
//        else
//        {
//            onError?.Invoke("NOT INITIALIZED");
//        }
//    }

//    /// <summary>
//    /// 퀵 로그인 요청: onSuccess, onFail()
//    /// </summary>
//    public void RequestQuick(Action<string> onSuccess, Action<string> onError)
//    {
//        if (_authManager != null)
//        {
//            if (debugLog) Debug.Log("[Apple Login] RequestQuick()");

//            rawNonce = System.Guid.NewGuid().ToString();

//            var nonce = GenerateNonceSHA256(rawNonce);
//            var quickLoginArgs = new AppleAuthQuickLoginArgs(nonce);

//            _authManager.QuickLogin(quickLoginArgs, credential =>
//            {
//                if (debugLog) Debug.Log("[Apple Login] RequestQuick(): SUCCESS");

//                var appleIdCredential = credential as AppleAuth.Interfaces.IAppleIDCredential;
//                if (appleIdCredential != null)
//                {
//                    userId = appleIdCredential.User;
//                    identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0, appleIdCredential.IdentityToken.Length);
//                    authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode, 0, appleIdCredential.AuthorizationCode.Length);

//                    onSuccess?.Invoke(identityToken);
//                }

//            }, error =>
//            {
//                if (debugLog) Debug.Log("[Apple Login] RequestQuick(): FAIL");

//                onError?.Invoke(error.LocalizedFailureReason);
//            });
//        }
//        else
//        {
//            onError?.Invoke("NOT INITIALIZED");
//        }
//    }

//    /// <summary>
//    /// 유저 삭제시 토큰 취소
//    /// https://developer.apple.com/documentation/sign_in_with_apple/revoke_tokens
//    /// </summary>
//    public void RevokeToken(MonoBehaviour context, Action onSuccess, Action<string> onFail)
//    {
//        // AuthCode 유효시간이 5분이라 재로그인해야함.
//        Request(idToken =>
//        {
//            context.StartCoroutine(RevokeTokenCoroutine(onSuccess, onFail));

//        }, onFail);
//    }

//    IEnumerator RevokeTokenCoroutine(Action onSuccess, Action<string> onFail)
//    {
//        if (debugLog) Debug.LogFormat("[AppleLogin] RevokeTokenCoroutine()\n idToken: {0}, authCode: {1}", identityToken, authorizationCode);

//        var form = new WWWForm();
//        {
//            form.AddField("client_id", Application.identifier);
//            form.AddField("client_secret", identityToken);
//            form.AddField("token", authorizationCode);
//        }

//        using (UnityWebRequest request = UnityWebRequest.Post("https://appleid.apple.com/auth/revoke", form))
//        {
//            yield return request.SendWebRequest();

//            if (request.isDone && request.result == UnityWebRequest.Result.Success)
//            {
//                if (debugLog) Debug.LogFormat("[AppleLogin] RevokeTokenCoroutine() SUCCESS");

//                onSuccess?.Invoke();
//            }
//            else
//            {
//                if (debugLog) Debug.LogFormat("[AppleLogin] RevokeTokenCoroutine() FAIL\n {0}", request.error);

//                onFail?.Invoke(request.error);
//            }
//        }
//    }
//}

//#endregion

//#region InAppReview

//[Serializable]
//public class UnityInAppReview
//{
//    public bool debugLog;

//    public void Request(Action onComplete = null, Action<string> onError = null)
//    {
//        if (debugLog) Debug.Log("[InAppReview] Request()");

//#if UNITY_ANDROID
//        var reviewManager = new ReviewManager();

//        var requestReviewOperation = reviewManager.RequestReviewFlow();

//        requestReviewOperation.Completed += requestOperation =>
//        {
//            if (requestOperation.Error == ReviewErrorCode.NoError)
//            {
//                var launchReviewOperation = reviewManager.LaunchReviewFlow(requestOperation.GetResult());

//                launchReviewOperation.Completed += launchOperation =>
//                {
//                    if (launchOperation.Error == ReviewErrorCode.NoError)
//                    {
//                        if (debugLog) Debug.Log("[InAppReview] Request() Completed");

//                        onComplete?.Invoke();
//                    }
//                    else
//                    {
//                        if (debugLog) Debug.LogError("[InAppReview] Error: " + requestOperation.Error.ToString());

//                        onError?.Invoke(requestOperation.Error.ToString());
//                    }
//                };
//            }
//            else
//            {
//                if (debugLog) Debug.LogError("[InAppReview] Error: " + requestOperation.Error.ToString());

//                onError?.Invoke(requestOperation.Error.ToString());
//            }
//        };
//#endif

//#if UNITY_IOS
//        if (!UnityEngine.iOS.Device.RequestStoreReview())
//        {
//            if (debugLog) Debug.LogError("[InAppReview] the iOS version isn't recent enough or that the StoreKit framework is not linked with the app");
//        }
//#endif
//    }
//}

//#endregion

//#region AppTrackingTransparency

//public class UnityAppTrackingTransparency
//{
//    public bool debugLog;

//    public void Request(Action<int> onComplete = null)
//    {
//        if (debugLog) Debug.Log("[UnityAppTrackingTransparency] Request()");


//#if UNITY_IOS
//        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
//        {
//            //ATTrackingStatusBinding.RequestAuthorizationTracking(status =>
//            //{
//            //    onComplete?.Invoke(status);
//            //});
//            ATTrackingStatusBinding.RequestAuthorizationTracking();
//            onComplete?.Invoke((int)ATTrackingStatusBinding.GetAuthorizationTrackingStatus());
//        }
//        else
//        {
//            onComplete?.Invoke(0);
//        }
//#else
//        onComplete?.Invoke(0);
//#endif
//    }
//}

//#endregion


namespace AudienceNetwork
{
    public static class AdSettings
    {
        public static void SetDataProcessingOptions(string[] dataProcessingOptions)
        {
            //#if UNITY_ANDROID
            //            AndroidJavaClass adSettings = new AndroidJavaClass("com.facebook.ads.AdSettings");
            //            adSettings.CallStatic("setDataProcessingOptions", (object)dataProcessingOptions);
            //#endif

#if UNITY_IOS
            FBAdSettingsBridgeSetDataProcessingOptions(dataProcessingOptions, dataProcessingOptions.Length);
#endif
        }

        public static void SetDataProcessingOptions(string[] dataProcessingOptions, int country, int state)
        {
#if UNITY_IOS
            FBAdSettingsBridgeSetDetailedDataProcessingOptions(dataProcessingOptions, dataProcessingOptions.Length, country, state);
#endif
        }

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetDataProcessingOptions(string[] dataProcessingOptions, int length);

        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetDetailedDataProcessingOptions(string[] dataProcessingOptions, int length, int country, int state);

        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
        {
            FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
        }
#endif
    }
}
