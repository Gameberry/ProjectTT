using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.UI;
using System.Linq;

namespace GameBerry.Contents
{
    [System.Serializable]
    public class ShopElementCustomResource
    {
        public ContentDetailList ContentDetailList;
        public int ResoueceIndex;

        public Color PackageBG_Color = Color.white;
        public Color PackageGraBG_Color = Color.white;
        public Color PackageFrame1_Color = Color.white;
        public Color PackageFrame2_Color = Color.white;
        public Sprite PackageTitleIcon_Sprite;

        public GameObject PackageIcon;
    }


    [System.Serializable]
    public class EventDialog
    {
        public V2Enum_EventKindType V2Enum_EventKindType;
        public List<string> dialogLists = new List<string>();
    }

    [System.Serializable]
    public class V2ForgeColorData
    {
        public int index;
        public Color ForgeColor = Color.white;
        public Color ForgeGradationColor = Color.white;
    }

    public class InGameContent : IContent
    {
        [Header("----------------Dig----------------")]
        public List<EventDialog> eventDialogList = new List<EventDialog>();

        [Header("----------------Dungeon----------------")]
        [SerializeField]
        private List<VarianceColor> m_varianceColor_List = new List<VarianceColor>();

        private static Dictionary<CreatureStatController, double> m_noticeBattlePower = new Dictionary<CreatureStatController, double>();

        [Header("----------------ShopElementCustomResource----------------")]
        [SerializeField]
        private List<ShopElementCustomResource> m_shopElementCustomResource_List = new List<ShopElementCustomResource>();
        private static Dictionary<ContentDetailList, List<ShopElementCustomResource>> m_shopElementCustomResource_Dic = new Dictionary<ContentDetailList, List<ShopElementCustomResource>>();

        [Header("----------------CharacterProfileSprite----------------")]
        [SerializeField]
        private List<Sprite> characterProfileSprite_List = new List<Sprite>();

        [Header("----------------Forge----------------")]
        [SerializeField]
        private List<V2ForgeColorData> m_v2ForgeColorDatas = new List<V2ForgeColorData>();
        private static Dictionary<int, V2ForgeColorData> m_v2ForgeColorDatas_Dic = new Dictionary<int, V2ForgeColorData>();


        public int saveCount = 0;

        private static bool m_readyNoticeBattlePower = false;

        public static GameBerry.Event.SetNoticeMsg m_setNoticeMsg = new GameBerry.Event.SetNoticeMsg();

        private static GameBerry.Event.SetSimpleDescPopupMsg setSimpleDescPopupMsg = new GameBerry.Event.SetSimpleDescPopupMsg();

        //------------------------------------------------------------------------------------
        protected override void OnLoadStart()
        {
            if (Define.EventRouletteDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                AddEventDialog(V2Enum_EventKindType.EventRoulette);
            if (Define.EventDungeonDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                AddEventDialog(V2Enum_EventKindType.EventDungeon);
            if (Define.EventHealDungeonDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                AddEventDialog(V2Enum_EventKindType.EventHealDungeon);
            if (Define.EventRedBullDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                AddEventDialog(V2Enum_EventKindType.EventRedBullDungeon);
            if (Define.EventUrsulaDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                AddEventDialog(V2Enum_EventKindType.EventUrsulaDungeon);
            if (Define.EventDigDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                AddEventDialog(V2Enum_EventKindType.EventDig);
            if (Define.EventMathRpgDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                AddEventDialog(V2Enum_EventKindType.EventMathRpg);
            if (Define.EventKingSlimeDungeonDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                AddEventDialog(V2Enum_EventKindType.EventKingSlime);

            Managers.CharacterStatManager.Instance.SetBattlePower();

            for (int i = 0; i < m_shopElementCustomResource_List.Count; ++i)
            {
                ShopElementCustomResource shopElementCustomResource = m_shopElementCustomResource_List[i];

                if (m_shopElementCustomResource_Dic.ContainsKey(shopElementCustomResource.ContentDetailList) == false)
                    m_shopElementCustomResource_Dic.Add(shopElementCustomResource.ContentDetailList, new List<ShopElementCustomResource>());

                m_shopElementCustomResource_Dic[shopElementCustomResource.ContentDetailList].Add(shopElementCustomResource);
            }

            //m_setNoticeMsg.NoticeStr = string.Format("·ÎµùÁß({0}/{1}");

            //Message.Send(m_setNoticeMsg);


            for (int i = 0; i < m_v2ForgeColorDatas.Count; ++i)
            {
                if (m_v2ForgeColorDatas_Dic.ContainsKey(m_v2ForgeColorDatas[i].index) == false)
                {
                    m_v2ForgeColorDatas_Dic.Add(m_v2ForgeColorDatas[i].index, m_v2ForgeColorDatas[i]);
                }
            }

            Managers.PlayerDataManager.Instance.SetProfileData(characterProfileSprite_List);
        }
        //------------------------------------------------------------------------------------
        private void AddEventDialog(V2Enum_EventKindType v2Enum_EventKindType)
        {
            EventDialog eventDialog = eventDialogList.Find(x => x.V2Enum_EventKindType == v2Enum_EventKindType);

            if (eventDialog == null)
                return;

            _uiLoader._uiList.AddRange(eventDialog.dialogLists);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUILoadComplete()
        {
            StartCoroutine(CompleteFade());
        }
        //------------------------------------------------------------------------------------
        private IEnumerator CompleteFade()
        {
            yield return new WaitForSeconds(0.3f);
            GlobalContent.DoFade(false);
            yield return new WaitForSeconds(1.0f);
            Managers.SceneManager.Instance.DeleteAppInitProcess();

            SetLoadComplete();
        }
        //------------------------------------------------------------------------------------
        protected override void OnLoadComplete()
        {
            GlobalContent.DoFade(true);

            Managers.TimeManager.Instance.PlayCheckStageCoolTimeReward();

            Managers.HPMPVarianceManager.Instance.InitVariance(m_varianceColor_List);

            Managers.PassManager.Instance.SetPassContent();
            Managers.RankManager.Instance.InitRankContent();

            TheBackEnd.TheBackEndManager.Instance.EnableSendRecvUpdateCheck();
            TheBackEnd.TheBackEndManager.Instance.OnAutoSave();
            TheBackEnd.TheBackEndManager.Instance.GetPlayerViewDataTableData();

            ThirdPartyLog.Instance.SendLog_Game_Connect();
            Managers.LocalNoticeManager.Instance.isReady = true;
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            //Managers.GuideManager.Instance.PlayGuide();

            IDialog.RequestDialogEnter<InGameGoodsDropDirectionDialog>();
            IDialog.RequestDialogEnter<InGamePlayContentDialog>();
            IDialog.RequestDialogEnter<LobbyEtcMenuDialog>();


            IDialog.RequestDialogEnter<DynamicBuffProgressDialog>();


            IDialog.RequestDialogEnter<InGameGuideInteractorDialog>();
            IDialog.RequestDialogEnter<ChatViewDialog>();

            if (SummonTicketContainer.needReddot == true || BoxContainer.needReddot == true)
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.Inventory_Item);

            double amount = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.ForgeHammer.Enum32ToInt());

#if !UNITY_EDITOR
            Managers.ShopManager.Instance.CheckUnPaid();
#endif
            m_readyNoticeBattlePower = true;
        }
        //------------------------------------------------------------------------------------
        public static void CheckNoticeBattlePowerChange(CreatureStatController creatureStatController)
        {
            if (m_readyNoticeBattlePower == false)
                return;

            if (m_noticeBattlePower.ContainsKey(creatureStatController) == true)
                return;

            m_noticeBattlePower.Add(creatureStatController, creatureStatController.GetBattlePower());
        }
        //------------------------------------------------------------------------------------
        //public string testGuildName;
        //public string testGuildintroduce;
        //public string testGuildmark;
        //public int testGuildflag;
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (m_noticeBattlePower.Count > 0)
            {
                foreach (var key in m_noticeBattlePower.Keys.ToList())
                {
                    if (key == null)
                    {
                        m_noticeBattlePower.Remove(key);
                        continue;
                    }

                    double battlepower = key.GetBattlePower();
                    double beforebattlepower = m_noticeBattlePower[key];

                    if (battlepower != beforebattlepower)
                    { 
                        GlobalContent.ShowGlobalNotice_BattlePower(battlepower, battlepower - beforebattlepower);
                        m_noticeBattlePower.Remove(key);
                    }
                }
            }

            //if (Input.GetKeyUp(KeyCode.F))
            //{
            //    Managers.GuildManager.Instance.CreateGuild(testGuildName, testGuildintroduce, testGuildmark, testGuildflag, true, true, null);
            //}

            //if (Input.GetKeyUp(KeyCode.G))
            //{
            //    Managers.GuildManager.Instance.GetMyGuildInfo(null);

            //}

            //if (Input.GetKeyUp(KeyCode.H))
            //{
            //    Managers.GuildManager.Instance.GetMyGuildGoods(null);
            //}


            //if (Input.GetKeyUp(KeyCode.J))
            //{
            //    Managers.GuildManager.Instance.WithdrawGuild(null);
            //}

            //if (Input.GetKeyUp(KeyCode.K))
            //{
            //    Managers.GuildManager.Instance.GetGuildList(callback =>
            //    {
            //        if (callback == null)
            //            return;

            //        for (int i = 0; i < callback.Count; ++i)
            //        {
            //            GuildListInfo guildListInfo = callback[i];

            //            Debug.Log(guildListInfo.ToString());
            //        }
            //    });
            //}

        }
        //------------------------------------------------------------------------------------
        public static ShopElementCustomResource GetShopElementCustomResource(ContentDetailList contentDetailList, int resoueceIndex)
        {
            if (m_shopElementCustomResource_Dic.ContainsKey(contentDetailList) == false)
                return null;

            List<ShopElementCustomResource> shopElementCustomResources = m_shopElementCustomResource_Dic[contentDetailList];

            return shopElementCustomResources.Find(x => x.ResoueceIndex == resoueceIndex);
        }
        //------------------------------------------------------------------------------------
        [ContextMenu("SortShopElementCustomResource")]
        private void SortShopElementCustomResource()
        {
            m_shopElementCustomResource_List.Sort((x, y) =>
            {
                if (x.ContentDetailList.Enum32ToInt() > y.ContentDetailList.Enum32ToInt())
                    return 1;
                else if (x.ContentDetailList.Enum32ToInt() < y.ContentDetailList.Enum32ToInt())
                    return -1;
                else
                {
                    if (x.ResoueceIndex > y.ResoueceIndex)
                        return 1;
                    else if (x.ResoueceIndex < y.ResoueceIndex)
                        return -1;
                }
                return 0;
            });
        }
        //------------------------------------------------------------------------------------
        public void AddShopElementCustomResource(ShopElementCustomResource shopElementCustomResource)
        {
            m_shopElementCustomResource_List.Add(shopElementCustomResource);
        }
        //------------------------------------------------------------------------------------
        public static void ShowSimpleDescPopup(string title, string desc)
        {
            setSimpleDescPopupMsg.title = title;
            setSimpleDescPopupMsg.desc = desc;

            Message.Send(setSimpleDescPopupMsg);

            GameBerry.UI.IDialog.RequestDialogEnter<InGameSimpleDescPopupDialog>();
        }
        //------------------------------------------------------------------------------------
        public static V2ForgeColorData GetV2ForgeColorData(int index)
        {
            if (m_v2ForgeColorDatas_Dic.ContainsKey(index) == true)
                return m_v2ForgeColorDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}