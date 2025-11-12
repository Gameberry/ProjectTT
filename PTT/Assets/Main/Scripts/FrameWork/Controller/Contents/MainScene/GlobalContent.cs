using System.Collections.Generic;
using UnityEngine;
using GameBerry.UI;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Contents
{
    [System.Serializable]
    public class V2SkillTriggerColorData
    {
        public Enum_TriggerType SkillTriggerType = Enum_TriggerType.Max;
        public Color BGColor;
        public Color TextColor;
        public Material TextMaterial;
    }


    [System.Serializable]
    public class BerserkerModeBGColor
    {
        public int Min;
        public int Max;
        public Color BGColor = Color.white;
    }

    [System.Serializable]
    public class PromoStarSprite
    {
        public int Promo = 0;
        public List<Sprite> StarImage = new List<Sprite>();
    }

    public class GlobalContent : IContent
    {
        [SerializeField]
        private List<V2SkillTriggerColorData> m_v2SkillTriggerColorDatas = new List<V2SkillTriggerColorData>();
        private static Dictionary<Enum_TriggerType, V2SkillTriggerColorData> m_v2SkillTriggerColorDatas_Dic = new Dictionary<Enum_TriggerType, V2SkillTriggerColorData>();


        [Header("----------------BerserkerModeBG----------------")]
        [SerializeField]
        private List<BerserkerModeBGColor> m_berserkerModeBGColor_List = null;
        private static List<BerserkerModeBGColor> m_berserkerModeBGColors = null;

        [Header("----------------BerserkerModeBG----------------")]
        [SerializeField]
        private List<PromoStarSprite> m_promoStarSprite_List = null;
        private static Dictionary<int, PromoStarSprite> m_promoStarSprites = new Dictionary<int, PromoStarSprite>();


        private static GameBerry.Event.ShowPopup_OkMsg m_showPopup_OkMsg = new GameBerry.Event.ShowPopup_OkMsg();
        private static GameBerry.Event.ShowPopup_OkCancelMsg m_showPopup_OkCancelMsg = new GameBerry.Event.ShowPopup_OkCancelMsg();
        private static GameBerry.Event.ShowPopup_InputMsg m_showPopup_InputMsg = new GameBerry.Event.ShowPopup_InputMsg();

        private static GameBerry.Event.GlobalNoticeMsg m_globalNoticeMsg = new GameBerry.Event.GlobalNoticeMsg();
        private static GameBerry.Event.GlobalNoticeBattlePowerMsg m_globalNoticeBattlePowerMsg = new GameBerry.Event.GlobalNoticeBattlePowerMsg();
        private static GameBerry.Event.GlobalNoticeSynergyCountMsg m_globalNoticeSynergyCountMsg = new GameBerry.Event.GlobalNoticeSynergyCountMsg();
        private static GameBerry.Event.GlobalUnLockContentNoticeMsg m_globalUnLockContentNoticeMsg = new GameBerry.Event.GlobalUnLockContentNoticeMsg();
        private static GameBerry.Event.GlobalGuideNoticeMsg _globalGuideNoticeMsg = new GameBerry.Event.GlobalGuideNoticeMsg();

        private static GameBerry.Event.DoFadeMsg m_fadeMsg = new GameBerry.Event.DoFadeMsg();
        private static GameBerry.Event.DoDungeonFadeMsg m_dungeonFadeMsg = new GameBerry.Event.DoDungeonFadeMsg();

        private static GameBerry.Event.SetBuffStringMsg m_setBuffStringMsg = new GameBerry.Event.SetBuffStringMsg();

        private static GameBerry.Event.SetSelectGoodsPopupMsg m_setSelectGoodsPopupMsg = new GameBerry.Event.SetSelectGoodsPopupMsg();
        private static GameBerry.Event.SetGoodsDescPopupMsg m_setGoodsDescPopupMsg = new GameBerry.Event.SetGoodsDescPopupMsg();

        //------------------------------------------------------------------------------------
        protected override void OnLoadStart()
        {
            for (int i = 0; i < m_v2SkillTriggerColorDatas.Count; ++i)
            {
                if (m_v2SkillTriggerColorDatas_Dic.ContainsKey(m_v2SkillTriggerColorDatas[i].SkillTriggerType) == false)
                {
                    m_v2SkillTriggerColorDatas_Dic.Add(m_v2SkillTriggerColorDatas[i].SkillTriggerType, m_v2SkillTriggerColorDatas[i]);
                }
            }


            m_berserkerModeBGColors = m_berserkerModeBGColor_List;

            for (int i = 0; i < m_promoStarSprite_List.Count; ++i)
            {
                if (m_promoStarSprites.ContainsKey(m_promoStarSprite_List[i].Promo) == false)
                {
                    m_promoStarSprites.Add(m_promoStarSprite_List[i].Promo, m_promoStarSprite_List[i]);
                }
            }

#if DEV_DEFINE
_uiLoader._uiList.Add("GlobalCheatDialog");
#endif

            SetLoadComplete();
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            IDialog.RequestDialogEnter<GlobalPopupDialog>();
            IDialog.RequestDialogEnter<GlobalDungeonFadeDialog>();
            IDialog.RequestDialogEnter<GlobalFadeDialog>();
            IDialog.RequestDialogEnter<GlobalNoticeDialog>();
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            IDialog.RequestDialogExit<GlobalPopupDialog>();
            IDialog.RequestDialogExit<GlobalBufferingDialog>();
            IDialog.RequestDialogExit<GlobalButtonLockDialog>();
            IDialog.RequestDialogExit<GlobalDungeonFadeDialog>();
            IDialog.RequestDialogExit<GlobalFadeDialog>();
            IDialog.RequestDialogExit<GlobalNoticeDialog>();
        }
        //------------------------------------------------------------------------------------
        public static V2SkillTriggerColorData GetV2SkillTriggerColorData(Enum_TriggerType v2Enum_TriggerType)
        {
            V2SkillTriggerColorData v2SkillTriggerColorData = null;

            if (m_v2SkillTriggerColorDatas_Dic.TryGetValue(v2Enum_TriggerType, out v2SkillTriggerColorData) == true)
                return v2SkillTriggerColorData;

            return null;
        }
        //------------------------------------------------------------------------------------
        public static void DoFade(bool visible, float duration = 1.0f)
        {
            // visible : false 투명 -> 검은색으로

            m_fadeMsg.duration = duration;
            m_fadeMsg.visible = visible;

            Message.Send(m_fadeMsg);
        }
        //------------------------------------------------------------------------------------
        public static void DoDungeonFade(bool visible, float duration = 1.0f)
        {
            // visible : false 투명 -> 검은색으로

            m_dungeonFadeMsg.duration = duration;
            m_dungeonFadeMsg.visible = visible;

            Message.Send(m_dungeonFadeMsg);
        }
        //------------------------------------------------------------------------------------
        private static bool showBufferingUI = false;
        private static bool showNetWorkBufferingUI = false;
        public static void VisibleBufferingUI(bool visible)
        {
            if (visible == true)
                IDialog.RequestDialogEnter<GlobalBufferingDialog>();
            else
            {
                if (showNetWorkBufferingUI == false)
                    IDialog.RequestDialogExit<GlobalBufferingDialog>();
            }

            showBufferingUI = visible;
        }
        //------------------------------------------------------------------------------------
        public static void VisibleBufferingUI(bool visible, string buffstring)
        {
            if (visible == true)
                IDialog.RequestDialogEnter<GlobalBufferingDialog>();
            else
            {
                if (showNetWorkBufferingUI == false)
                    IDialog.RequestDialogExit<GlobalBufferingDialog>();
            }

            showBufferingUI = visible;

            m_setBuffStringMsg.buffstr = buffstring;
            Message.Send(m_setBuffStringMsg);
        }
        //------------------------------------------------------------------------------------
        public static void VisibleNetWorkBufferingUI(bool visible)
        {
            if (visible == true)
                IDialog.RequestDialogEnter<GlobalBufferingDialog>();
            else
            {
                if (showBufferingUI == false)
                    IDialog.RequestDialogExit<GlobalBufferingDialog>();
            }

            showNetWorkBufferingUI = visible;
        }
        //------------------------------------------------------------------------------------
        public static void SetButtonLock(bool btnlock)
        {
            if (btnlock == true)
                IDialog.RequestDialogEnter<GlobalButtonLockDialog>();
            else
                IDialog.RequestDialogExit<GlobalButtonLockDialog>();
        }
        //------------------------------------------------------------------------------------
        public static void ShowPopup_Ok(string titletext, string contenttext, System.Action okAction = null, System.Action<bool> toDayHide = null)
        {
            m_showPopup_OkMsg.titletext = titletext;
            m_showPopup_OkMsg.contenttext = contenttext;
            m_showPopup_OkMsg.okAction = okAction;
            m_showPopup_OkMsg.toDayHide = toDayHide;

            Message.Send(m_showPopup_OkMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowPopup_OkCancel(string titletext, string contenttext, System.Action okAction = null, System.Action cancelAction = null, System.Action<bool> toDayHide = null)
        {
            m_showPopup_OkCancelMsg.titletext = titletext;
            m_showPopup_OkCancelMsg.contenttext = contenttext;

            m_showPopup_OkCancelMsg.useCustomOKBtn = false;

            m_showPopup_OkCancelMsg.okAction = okAction;
            m_showPopup_OkCancelMsg.cancelAction = cancelAction;
            m_showPopup_OkCancelMsg.toDayHide = toDayHide;

            Message.Send(m_showPopup_OkCancelMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowPopup_CustomOkCancel(
            string titletext, 
            string contenttext, 

            string okbtntitle,
            Sprite okbtnicon,
            string okbtncontent,

            System.Action okAction = null, 
            System.Action cancelAction = null,
            System.Action<bool> toDayHide = null)
        {
            m_showPopup_OkCancelMsg.titletext = titletext;
            m_showPopup_OkCancelMsg.contenttext = contenttext;

            m_showPopup_OkCancelMsg.useCustomOKBtn = true;
            m_showPopup_OkCancelMsg.OKBtnTitle = okbtntitle;
            m_showPopup_OkCancelMsg.OKBtnIcon = okbtnicon;
            m_showPopup_OkCancelMsg.OKBtnContent = okbtncontent;

            m_showPopup_OkCancelMsg.okAction = okAction;
            m_showPopup_OkCancelMsg.cancelAction = cancelAction;
            m_showPopup_OkCancelMsg.toDayHide = toDayHide;

            Message.Send(m_showPopup_OkCancelMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowPopup_Input(string titletext, string defaultstr, string placeholder, System.Action<string> okAction, System.Action cancelAction = null, System.Action<bool> toDayHide = null)
        {
            m_showPopup_InputMsg.titletext = titletext;
            m_showPopup_InputMsg.defaultstr = defaultstr;
            m_showPopup_InputMsg.placeholder = placeholder;
            m_showPopup_InputMsg.okAction = okAction;
            m_showPopup_InputMsg.cancelAction = cancelAction;
            m_showPopup_InputMsg.toDayHide = toDayHide;

            Message.Send(m_showPopup_InputMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowGlobalNotice(string notice, float duration = 1.0f)
        {
            m_globalNoticeMsg.duration = duration;
            m_globalNoticeMsg.NoticeString = notice;

            Message.Send(m_globalNoticeMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowGlobalNotice_LocalString(string localkey, float duration = 1.0f)
        {
            m_globalNoticeMsg.duration = duration;
            m_globalNoticeMsg.NoticeString = Managers.LocalStringManager.Instance.GetLocalString(localkey); ;

            Message.Send(m_globalNoticeMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowGlobalNotice_BattlePower(double battlePower, double changeValue)
        {
            m_globalNoticeBattlePowerMsg.battlePower = battlePower;
            m_globalNoticeBattlePowerMsg.changeValue = changeValue;

            Message.Send(m_globalNoticeBattlePowerMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowGlobalNotice_SynergyCount(int before, int after)
        {
            m_globalNoticeSynergyCountMsg.before = before;
            m_globalNoticeSynergyCountMsg.after = after;

            Message.Send(m_globalNoticeSynergyCountMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowGlobalNotice_Guide(string notice, float duration = 4.0f, float height = -180.0f)
        {
            _globalGuideNoticeMsg.duration = duration;
            _globalGuideNoticeMsg.NoticeString = notice;
            _globalGuideNoticeMsg.height = height;

            Message.Send(_globalGuideNoticeMsg);
        }
        //------------------------------------------------------------------------------------
        public static void HideGlobalNotice_Guide()
        {
            _globalGuideNoticeMsg.duration = 0;
            _globalGuideNoticeMsg.NoticeString = string.Empty;
            _globalGuideNoticeMsg.height = 0;

            Message.Send(_globalGuideNoticeMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowUnLockContentNotice(string notice)
        {
            m_globalUnLockContentNoticeMsg.titletext = notice;

            Message.Send(m_globalUnLockContentNoticeMsg);
        }
        //------------------------------------------------------------------------------------
        public static void ShowSelectGoodsPopup(V2Enum_Goods v2Enum_Goods, List<ObscuredInt> SelectIndexList, System.Action<int> SelectedCallBack)
        {
            m_setSelectGoodsPopupMsg.v2Enum_Goods = v2Enum_Goods;
            m_setSelectGoodsPopupMsg.SelectIndexList = SelectIndexList;
            m_setSelectGoodsPopupMsg.SelectedCallBack = SelectedCallBack;

            Message.Send(m_setSelectGoodsPopupMsg);

            UI.IDialog.RequestDialogEnter<InGameSelectGoodsPopupDialog>();
        }
        //------------------------------------------------------------------------------------
        public static void ShowGoodsDescPopup(V2Enum_Goods v2Enum_Goods, int index, double timegoodstime = 0.0)
        {
            m_setGoodsDescPopupMsg.v2Enum_Goods = v2Enum_Goods;

            if (m_setGoodsDescPopupMsg.v2Enum_Goods == V2Enum_Goods.Max)
                m_setGoodsDescPopupMsg.v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(index);

            m_setGoodsDescPopupMsg.index = index;
            m_setGoodsDescPopupMsg.timeGoodsTime = timegoodstime;

            Message.Send(m_setGoodsDescPopupMsg);

            UI.IDialog.RequestDialogEnter<InGameGoodsDescPopupDialog>();
        }
        //------------------------------------------------------------------------------------
        public static Color GetBerserkerMoveColor(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
                return Color.white;

            if(m_berserkerModeBGColors == null)
                return spriteRenderer.color;

            for (int i = 0; i < m_berserkerModeBGColors.Count; ++i)
            {
                if (m_berserkerModeBGColors[i].Min < spriteRenderer.sortingOrder
                    && m_berserkerModeBGColors[i].Max >= spriteRenderer.sortingOrder)
                    return m_berserkerModeBGColors[i].BGColor;
            }

            return spriteRenderer.color;
        }
        //------------------------------------------------------------------------------------
        public static PromoStarSprite GePromoStarSprite(int promo)
        {
            if (m_promoStarSprites.ContainsKey(promo) == false)
                return null;

            return m_promoStarSprites[promo];
        }
        //------------------------------------------------------------------------------------
    }
}