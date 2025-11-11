using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    [System.Serializable]
    public class ShopSummonElementResource
    {
        public V2Enum_SummonType v2Enum_SummonType;
        public UIShopSummonElement uIShopSummonElement;
    }

    [System.Serializable]
    public class ContentDetailListRectTrans
    {
        public ContentDetailList v2Enum_SummonType;
        public RectTransform ContentRect;
    }

    public class ShopGeneralDialog : IDialog
    {
        [Header("------------Summon------------")]
        [SerializeField]
        private Transform _guideGearBG;


        [SerializeField]
        private TMP_Text _shopTitle;

        [SerializeField]
        private ScrollRect m_elementScrollRect;

        [SerializeField]
        private RectTransform m_elementRoot;

        [Header("------------Summon------------")]
        [SerializeField]
        private List<ShopSummonElementResource> m_shopSummonElementResources;

        private Dictionary<V2Enum_SummonType, UIShopSummonElement> m_uIShopSummonElements_Dic = new Dictionary<V2Enum_SummonType, UIShopSummonElement>();

        [Header("------------Shop------------")]
        [SerializeField]
        private List<UIShopElement_Group> m_uIShopElement_Groups = new List<UIShopElement_Group>();

        [SerializeField]
        private List<ContentDetailListRectTrans> m_contentDetailListRectTrans = new List<ContentDetailListRectTrans>();

        [Header("------------Button------------")]
        [SerializeField]
        private List<ContentButtonData> m_contentButtonDatas = new List<ContentButtonData>();

        private ContentDetailList _currentTab = ContentDetailList.None;

        [SerializeField]
        private UIShopElement_LimitTime _uIShopElement_LimitTime;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            for (int i = 0; i < m_contentButtonDatas.Count; ++i)
            {
                if (m_contentButtonDatas[i] != null)
                {
                    m_contentButtonDatas[i].CallBack = OnClick_Tab;
                    m_contentButtonDatas[i].btn.onClick.AddListener(m_contentButtonDatas[i].OnClick);
                }
            }

            if (_uIShopElement_LimitTime != null)
            {
                ShopDataBase shopDataBase = Managers.ShopManager.Instance.GetShopData(150040014);
                if (shopDataBase == null)
                {
                    _uIShopElement_LimitTime.gameObject.SetActive(false);
                }
                else
                {
                    if (Managers.ShopManager.Instance.IsSoldOut(shopDataBase) == true)
                        _uIShopElement_LimitTime.gameObject.SetActive(false);
                    else
                    {
                        _uIShopElement_LimitTime.gameObject.SetActive(false);
                        _uIShopElement_LimitTime.Init();
                        _uIShopElement_LimitTime.SetShopElement(Managers.ShopManager.Instance.GetShopData(150040014));
                    }
                }
            }



            Dictionary<V2Enum_SummonType, SummonData> summonData_dic = Managers.SummonManager.Instance.GetSummonDatas();

            for (int i = 0; i < m_shopSummonElementResources.Count; ++i)
            {
                ShopSummonElementResource shopSummonElementResource = m_shopSummonElementResources[i];

                if (summonData_dic.ContainsKey(shopSummonElementResource.v2Enum_SummonType) == false)
                    continue;

                SummonData summonData = summonData_dic[shopSummonElementResource.v2Enum_SummonType];

                UIShopSummonElement uIShopSummonElement = shopSummonElementResource.uIShopSummonElement;
                uIShopSummonElement.Init(shopSummonElementResource.v2Enum_SummonType);
                uIShopSummonElement.SetSummonElement(shopSummonElementResource.v2Enum_SummonType);

                if (m_uIShopSummonElements_Dic.ContainsKey(shopSummonElementResource.v2Enum_SummonType) == false)
                {
                    m_uIShopSummonElements_Dic.Add(shopSummonElementResource.v2Enum_SummonType, uIShopSummonElement);
                }

                if (summonData.SummonCostParam11 > 0)
                {
                    V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(summonData.SummonCostParam11);

                    Managers.GoodsManager.Instance.AddGoodsRefreshEvent(v2Enum_Goods, summonData.SummonCostParam11, uIShopSummonElement.RefreshElement);
                }

                if (summonData.SummonCostParam13 > 0)
                {
                    V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(summonData.SummonCostParam13);

                    Managers.GoodsManager.Instance.AddGoodsRefreshEvent(v2Enum_Goods, summonData.SummonCostParam13, uIShopSummonElement.RefreshElement);
                }
            }

            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.Dia.Enum32ToInt(), RefreshDia);

            for (int i = 0; i < m_uIShopElement_Groups.Count; ++i)
            {
                m_uIShopElement_Groups[i].SetSiblingIdx(i + 1);
                m_uIShopElement_Groups[i].SetShopElement();
                //m_uIShopElement_Groups[i].SetSibling();
            }


            Message.AddListener<GameBerry.Event.RefreshSummonInfoListMsg>(RefreshSummonInfoList);
            Message.AddListener<GameBerry.Event.SetShopSummonDialogStateMsg>(SetShopSummonDialogState);
            Message.AddListener<GameBerry.Event.SetShopGerneralDialogStateMsg>(SetShopGerneralDialogState);
            Message.AddListener<GameBerry.Event.HideGearTutorialGachaFocusMsg>(HideGearTutorialGachaFocus);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            if (Managers.GoodsManager.isAlive == true)
                Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.LobbyGold.Enum32ToInt(), RefreshDia);

            Message.RemoveListener<GameBerry.Event.RefreshSummonInfoListMsg>(RefreshSummonInfoList);
            Message.RemoveListener<GameBerry.Event.SetShopSummonDialogStateMsg>(SetShopSummonDialogState);
            Message.RemoveListener<GameBerry.Event.SetShopGerneralDialogStateMsg>(SetShopGerneralDialogState);
            Message.RemoveListener<GameBerry.Event.HideGearTutorialGachaFocusMsg>(HideGearTutorialGachaFocus);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            Contents.GlobalContent.HideGlobalNotice_Guide();

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.RelicTutorial)
            {
                Change_Tab(ContentDetailList.ShopGeneral);
                SetForcus(ContentDetailList.ShopSummon_Relic).Forget();
                RelicTutorial().Forget();
            }
            else if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.RuneTutorial)
            {
                Change_Tab(ContentDetailList.ShopGeneral);
                SetForcus(ContentDetailList.ShopSummon_Rune).Forget();
                RuneTutorial().Forget();
            }
            else if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.GearTutorial)
            {
                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() < 3)
                {
                    if (_guideGearBG != null)
                        _guideGearBG.gameObject.SetActive(true);

                    Change_Tab(ContentDetailList.ShopGeneral);
                    SetForcus(ContentDetailList.ShopSummon_Gear).Forget();
                    GearTutorial().Forget();

                    ignoreExit = true;
                }
            }

            if (_currentTab == ContentDetailList.None)
                Change_Tab(ContentDetailList.ShopGeneral);
            else
                Change_Tab(_currentTab);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.isAlive == false)
                return;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.RelicTutorial
                || Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.RuneTutorial
                || Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.GearTutorial)
            {
                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 2)
                    Managers.GuideInteractorManager.Instance.SetGuideStep(1);
                else if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 99)
                    Managers.GuideInteractorManager.Instance.SetGuideStep(3);
            }

            Contents.GlobalContent.HideGlobalNotice_Guide();
        }
        //------------------------------------------------------------------------------------
        private void HideGearTutorialGachaFocus(GameBerry.Event.HideGearTutorialGachaFocusMsg msg)
        {
            if (_guideGearBG != null)
                _guideGearBG.gameObject.SetActive(false);

            Managers.GuideInteractorManager.Instance.SetGuideStep(99);

            ignoreExit = false;
        }
        //------------------------------------------------------------------------------------
        private void SetShopSummonDialogState(GameBerry.Event.SetShopSummonDialogStateMsg msg)
        {
            V2Enum_SummonType v2Enum_SummonType = V2Enum_SummonType.Max;

            switch (msg.ContentDetailList)
            {
                case ContentDetailList.ShopRandomStore:
                    {
                        v2Enum_SummonType = V2Enum_SummonType.SummonGear;
                        break;
                    }
                case ContentDetailList.ShopSummon_Normal:
                    {
                        v2Enum_SummonType = V2Enum_SummonType.SummonNormal;
                        break;
                    }
                case ContentDetailList.ShopSummon_Relic:
                    {
                        v2Enum_SummonType = V2Enum_SummonType.SummonRelic;
                        break;
                    }
                case ContentDetailList.ShopSummon_Rune:
                    {
                        v2Enum_SummonType = V2Enum_SummonType.SummonRune;
                        break;
                    }
                case ContentDetailList.ShopSummon_Gear:
                    {
                        v2Enum_SummonType = V2Enum_SummonType.SummonGear;
                        break;
                    }
            }

            if (m_uIShopSummonElements_Dic.ContainsKey(v2Enum_SummonType) == true)
            {
                UIShopSummonElement uIShopSummonElement = m_uIShopSummonElements_Dic[v2Enum_SummonType];


                RectTransform rectTransform = null;
                if (uIShopSummonElement.TryGetComponent(out rectTransform))
                {
                    if (m_elementScrollRect != null)
                        m_elementScrollRect.SetNormalizedPositionToCenter(rectTransform);


                    //Vector2 offset = Vector2.zero;
                    //offset.y = m_snapOffSet;

                    //Util.ScrollViewSnapToItem(m_elementScrollRect, m_elementRoot, rectTransform, offset);
                }

            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshDia(double amount)
        {
            foreach (KeyValuePair<V2Enum_SummonType, UIShopSummonElement> pair in m_uIShopSummonElements_Dic)
            {
                pair.Value.SetSummonBtn();
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshSummonInfoList(GameBerry.Event.RefreshSummonInfoListMsg msg)
        {
            for (int i = 0; i < msg.datas.Count; ++i)
            {
                if (m_uIShopSummonElements_Dic.ContainsKey(msg.datas[i]) == true)
                    m_uIShopSummonElements_Dic[msg.datas[i]].SetSummonElement(msg.datas[i]);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetShopGerneralDialogState(GameBerry.Event.SetShopGerneralDialogStateMsg msg)
        {
            int tab = msg.ContentDetailList.Enum32ToInt();
            int de = tab % 100;
            tab -= de;

            Change_Tab(tab.IntToEnum32<ContentDetailList>());

            SetForcus(msg.ContentDetailList).Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask RelicTutorial()
        {
            await UniTask.WaitForSeconds(0.5f);

            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/relic2"));
            Managers.GuideInteractorManager.Instance.SetGuideStep(2);
        }
        //------------------------------------------------------------------------------------
        private async UniTask RuneTutorial()
        {
            await UniTask.WaitForSeconds(0.5f);

            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/rune2"));
            Managers.GuideInteractorManager.Instance.SetGuideStep(2);
        }
        //------------------------------------------------------------------------------------
        private async UniTask GearTutorial()
        {
            await UniTask.WaitForSeconds(0.5f);

            Managers.GuideInteractorManager.Instance.SetGuideStep(2);
        }
        //------------------------------------------------------------------------------------
        private async UniTask SetForcus(ContentDetailList ContentDetailList)
        {
            await UniTask.Yield();
            await UniTask.Yield();
            await UniTask.Yield();

            ContentDetailListRectTrans contentDetailListRectTrans = m_contentDetailListRectTrans.Find(x => x.v2Enum_SummonType == ContentDetailList);
            if (contentDetailListRectTrans != null)
            {
                RectTransform rectTransform = contentDetailListRectTrans.ContentRect;

                if (m_elementScrollRect != null)
                    m_elementScrollRect.SetNormalizedPositionToCenter(rectTransform);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Tab(ContentDetailList callbackID)
        {
            if (_currentTab == callbackID)
                return;

            Change_Tab(callbackID);
        }
        //------------------------------------------------------------------------------------
        private void Change_Tab(ContentDetailList callbackID)
        {
            if (ignoreExit == true)
                return;

            _currentTab = callbackID;

            string local = string.Empty;

            for (int i = 0; i < m_contentButtonDatas.Count; ++i)
            {
                ContentButtonData contentButtonData = m_contentButtonDatas[i];
                contentButtonData.contents.AllSetActive(contentButtonData.MenuID == callbackID);

                if (contentButtonData.MenuID == callbackID)
                    local = contentButtonData.localstring;
            }

            if (_uIShopElement_LimitTime != null)
            {
                ShopDataBase shopDataBase = Managers.ShopManager.Instance.GetShopData(150040014);
                if (shopDataBase == null)
                {
                    _uIShopElement_LimitTime.gameObject.SetActive(false);
                }
                else
                {
                    if (Managers.ShopManager.Instance.IsSoldOut(shopDataBase) == true)
                        _uIShopElement_LimitTime.gameObject.SetActive(false);
                }
            }


            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.GearSummon) == false)
            {
                ContentDetailListRectTrans contentDetailListRectTrans = m_contentDetailListRectTrans.Find(x => x.v2Enum_SummonType == ContentDetailList.ShopSummon_Gear);

                if (contentDetailListRectTrans != null && contentDetailListRectTrans.ContentRect != null)
                    contentDetailListRectTrans.ContentRect.gameObject.SetActive(false);
            }

            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.RelicSummon) == false)
            {
                if (m_uIShopSummonElements_Dic.ContainsKey(V2Enum_SummonType.SummonRelic) == true)
                    m_uIShopSummonElements_Dic[V2Enum_SummonType.SummonRelic].gameObject.SetActive(false);
            }

            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.RuneSummon) == false)
            {
                if (m_uIShopSummonElements_Dic.ContainsKey(V2Enum_SummonType.SummonRune) == true)
                    m_uIShopSummonElements_Dic[V2Enum_SummonType.SummonRune].gameObject.SetActive(false);
            }

            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.DescendShop) == false)
            {
                ContentDetailListRectTrans contentDetailListRectTrans = m_contentDetailListRectTrans.Find(x => x.v2Enum_SummonType == ContentDetailList.ShopDescendStore);

                if (contentDetailListRectTrans != null && contentDetailListRectTrans.ContentRect != null)
                    contentDetailListRectTrans.ContentRect.gameObject.SetActive(false);
            }

            if (_shopTitle != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_shopTitle, local);
        }
        //------------------------------------------------------------------------------------
    }
}