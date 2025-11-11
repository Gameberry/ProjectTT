using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using BackEnd;

namespace GameBerry.UI
{
    public class CharacterSkinDialog : IDialog
    {
        [SerializeField]
        private List<Button> m_exitBtn;

        [Header("------------GearSlot------------")]
        // 반지를 뺀 나머지
        [SerializeField]
        private List<UISkinSlotElement> m_uiSkinSlotElements_List = new List<UISkinSlotElement>();


        [SerializeField]
        private TMP_Text m_SkinName;

        [SerializeField]
        private TMP_Text m_grade_Text;

        [SerializeField]
        private TMP_Text m_SkinLv;


        [Header("----------SkinDetail_PossEffect----------")]
        [SerializeField]
        private TMP_Text m_SkinPossEffect_Title;

        [SerializeField]
        private TMP_Text m_SkinPossEffect_Value;



        [Header("------------SkinDetail_ButtonGroup------------")]
        [SerializeField]
        private Button m_detailSkinEquipBtn;

        [SerializeField]
        private Color m_detailSkinEquipBtn_Disable;

        [SerializeField]
        private Color m_detailSkinEquipBtn_Enable;

        [SerializeField]
        private Color m_detailSkinEquipBtn_Already;


        [SerializeField]
        private TMP_Text m_detailSkinEquipText;

        [SerializeField]
        private Color m_detailSkinEquipText_Disable;

        [SerializeField]
        private Color m_detailSkinEquipText_Enable;

        [SerializeField]
        private Color m_detailSkinEquipText_Already;




        [SerializeField]
        private Button m_detailSkinEnhanceBtn;

        [SerializeField]
        private Color m_detailSkinEnhanceBtn_Disable;

        [SerializeField]
        private Color m_detailSkinEnhanceBtn_Enable;

        [SerializeField]
        private Image m_detailSkinEnhancePriceIcon;

        [SerializeField]
        private TMP_Text m_detailSkinEnhancePrice;

        [SerializeField]
        private TMP_Text m_detailSkinEnhanceTitleText;

        [SerializeField]
        private Color m_detailSkinEnhanceTitleText_Disable;

        [SerializeField]
        private Color m_detailSkinEnhanceTitleText_Enable;


        [Header("------------SkinElementGroup------------")]
        [SerializeField]
        private ScrollRect m_SkinElementScrollRect;

        [SerializeField]
        private RectTransform m_SkinElementGroup_Root;

        [SerializeField]
        private UISkinElement m_SkinElement;

        private List<UISkinElement> m_uiSkinElements_List = new List<UISkinElement>();
        private Dictionary<CharacterSkinData, UISkinElement> m_uiSkinElements_Dic = new Dictionary<CharacterSkinData, UISkinElement>();

        private CharacterSkinData m_currentSkinData = null;

        private V2Enum_Skin m_currentGoods = V2Enum_Skin.Max;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(OnClick_ExitBtn);
                }
            }

            if (m_detailSkinEquipBtn != null)
                m_detailSkinEquipBtn.onClick.AddListener(OnClick_EquipBtn);

            if (m_detailSkinEnhanceBtn != null)
            { 
                m_detailSkinEnhanceBtn.onClick.AddListener(OnClick_EnhanceBtn);
            }

            for (int i = 0; i < m_uiSkinSlotElements_List.Count; ++i)
            {
                UISkinSlotElement uIGearSlotElement = m_uiSkinSlotElements_List[i];
                V2Enum_Skin v2Enum_Goods = uIGearSlotElement.GetSkinType();

                if (v2Enum_Goods == V2Enum_Skin.Max)
                    continue;

                uIGearSlotElement.Init(OnClick_GearSlot);
                uIGearSlotElement.SetSkinData(Managers.CharacterSkinManager.Instance.GetCurrentSlotSkin(v2Enum_Goods));
            }

            SetSkinDetailView(V2Enum_Skin.SkinWeapon);

            Message.AddListener<GameBerry.Event.RefreshCharacterSkinInfoListMsg>(RefreshCharacterSkinInfoList);
            Message.AddListener<GameBerry.Event.ChangeCharacterSkinInfoMsg>(ChangeCharacterSkinInfo);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshCharacterSkinInfoListMsg>(RefreshCharacterSkinInfoList);
            Message.RemoveListener<GameBerry.Event.ChangeCharacterSkinInfoMsg>(ChangeCharacterSkinInfo);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            CharacterSkinData characterSkinData = null;

            characterSkinData = Managers.CharacterSkinManager.Instance.GetCurrentSlotSkin(m_currentGoods);

            if (characterSkinData == null)
            {
                List<CharacterSkinData> characterSkinDatas = Managers.CharacterSkinManager.Instance.GetCharacterSkinAllData(m_currentGoods);
                if (characterSkinDatas != null)
                {
                    if (characterSkinDatas.Count > 0)
                        characterSkinData = characterSkinDatas[0];
                }
            }

            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.CharacterSkin);

            if (m_currentGoods == V2Enum_Skin.SkinWeapon)
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.CharacterSkin_Weapon);
            else if (m_currentGoods == V2Enum_Skin.SkinBody)
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.CharacterSkin_Body);

            SetSkinDetail(characterSkinData);

            ScrollViewSnapToItem();
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.CharacterSkinManager.isAlive == false)
                return;

            if (Managers.RedDotManager.isAlive == false)
                return;

            Managers.CharacterSkinManager.Instance.ReleaseFakeWeaponSkin();

            //UICharacterAniManager.Instance.SetAniResourceName(
            //    Define.PlayerSpriteResourceName
            //    , Managers.CharacterSkinManager.Instance.GetSkinBodyNumber()
            //    , ActorType.Knight);
            //UICharacterAniManager.Instance.PlayIdleAni();

            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.CharacterSkin);
        }
        //------------------------------------------------------------------------------------
        private void SetSkinDetailView(V2Enum_Skin v2Enum_Skin)
        {
            if (m_currentGoods == v2Enum_Skin)
                return;

            m_currentGoods = v2Enum_Skin;

            if (m_currentSkinData != null)
            {
                if (m_uiSkinElements_Dic.ContainsKey(m_currentSkinData) == true)
                {
                    UISkinElement uISkinElement = m_uiSkinElements_Dic[m_currentSkinData];
                    uISkinElement.SetSelectState(false);
                }
            }

            if (m_currentGoods == V2Enum_Skin.SkinWeapon)
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.CharacterSkin_Weapon);
            else if (m_currentGoods == V2Enum_Skin.SkinBody)
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.CharacterSkin_Body);

            m_currentSkinData = null;

            List<CharacterSkinData> characterSkinDatas = Managers.CharacterSkinManager.Instance.GetCharacterSkinAllData(m_currentGoods);

            m_uiSkinElements_Dic.Clear();

            int selectindex = 0;

            for (int i = 0; i < characterSkinDatas.Count; ++i)
            {
                if (m_uiSkinElements_List.Count <= i)
                    CreateSkinElement();

                m_uiSkinElements_List[i].SetSkinElement(characterSkinDatas[i]);
                m_uiSkinElements_List[i].SetEquipState(Managers.CharacterSkinManager.Instance.IsEquipSkin(characterSkinDatas[i]));

                m_uiSkinElements_List[i].gameObject.SetActive(true);

                m_uiSkinElements_Dic.Add(characterSkinDatas[i], m_uiSkinElements_List[i]);

                selectindex = i;
            }

            for (int i = selectindex + 1; i < m_uiSkinElements_List.Count; ++i)
            {
                m_uiSkinElements_List[i].gameObject.SetActive(false);
            }

        }
        //------------------------------------------------------------------------------------
        private void CreateSkinElement()
        {
            GameObject clone = Instantiate(m_SkinElement.gameObject, m_SkinElementGroup_Root);
            UISkinElement element = clone.GetComponent<UISkinElement>();
            element.Init(OnClick_SkinElement);

            m_uiSkinElements_List.Add(element);
        }
        //------------------------------------------------------------------------------------
        private void RefreshCharacterSkinInfoList(GameBerry.Event.RefreshCharacterSkinInfoListMsg msg)
        {
            for (int i = 0; i < msg.datas.Count; ++i)
            {
                if (m_uiSkinElements_Dic.ContainsKey(msg.datas[i]) == true)
                {
                    UISkinElement element = m_uiSkinElements_Dic[msg.datas[i]];
                    element.SetSkinElement(msg.datas[i]);
                    m_uiSkinElements_List[i].SetEquipState(Managers.CharacterSkinManager.Instance.IsEquipSkin(msg.datas[i]));
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void ChangeCharacterSkinInfo(GameBerry.Event.ChangeCharacterSkinInfoMsg msg)
        {
            UISkinElement element = null;

            if (msg.BeforeGear != null)
            {
                if (m_uiSkinElements_Dic.TryGetValue(msg.BeforeGear, out element) == true)
                    element.SetEquipState(false);
            }

            if (msg.AfterGear != null)
            {
                if (m_uiSkinElements_Dic.TryGetValue(msg.AfterGear, out element) == true)
                    element.SetEquipState(true);
            }

            UISkinSlotElement uISkinSlotElement = m_uiSkinSlotElements_List.Find(x => x.GetSkinType() == msg.V2Enum_Skin);
            if (uISkinSlotElement != null)
                uISkinSlotElement.SetSkinData(msg.AfterGear);

            if (msg.V2Enum_Skin == V2Enum_Skin.SkinBody)
            {
                //UICharacterAniManager.Instance.SetAniResourceName(
                //Define.PlayerSpriteResourceName
                //, Managers.CharacterSkinManager.Instance.GetSkinBodyNumber()
                //, ActorType.Knight);
                //UICharacterAniManager.Instance.PlayIdleAni();
            }
        }
        //------------------------------------------------------------------------------------
        private void SetSkinDetail(CharacterSkinData characterSkinData)
        {
            if (m_currentSkinData != null)
            {
                if (m_uiSkinElements_Dic.ContainsKey(m_currentSkinData) == true)
                {
                    UISkinElement uISkinElement = m_uiSkinElements_Dic[m_currentSkinData];
                    uISkinElement.SetSelectState(false);
                }
            }

            if (characterSkinData == null)
            {
                if (m_SkinName != null)
                {
                    m_SkinName.gameObject.SetActive(false);
                }


                if (m_grade_Text != null)
                    m_grade_Text.gameObject.SetActive(false);



                if (m_SkinLv != null)
                    m_SkinLv.text = string.Format("Lv.0");


                if (m_detailSkinEquipBtn != null)
                {
                    m_detailSkinEquipBtn.interactable = false;
                    m_detailSkinEquipBtn.image.color = m_detailSkinEquipBtn_Disable;
                }

                if (m_detailSkinEquipText != null)
                {
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_detailSkinEquipText, "common/ui/equip");
                    m_detailSkinEquipText.color = m_detailSkinEquipText_Disable;
                }

                if (m_detailSkinEnhanceBtn != null)
                {
                    m_detailSkinEnhanceBtn.image.color = m_detailSkinEnhanceBtn_Disable;
                    m_detailSkinEnhanceBtn.interactable = false;
                }

                if (m_detailSkinEnhanceTitleText != null)
                {
                    m_detailSkinEnhanceTitleText.color = m_detailSkinEnhanceTitleText_Disable;
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_detailSkinEnhanceTitleText, "common/ui/levelUp");
                }

                if (m_detailSkinEnhancePriceIcon != null)
                    m_detailSkinEnhancePriceIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.SkinLevelUP.Enum32ToInt());

                if (m_detailSkinEnhancePrice != null)
                {
                    m_detailSkinEnhancePrice.text = "-";
                    m_detailSkinEnhancePrice.color = m_detailSkinEnhanceTitleText_Disable;
                }

                m_currentSkinData = null;

                return;
            }

            if (characterSkinData.SkinType == V2Enum_Skin.SkinBody)
            {
                //UICharacterAniManager.Instance.SetAniResourceName(
                //Define.PlayerSpriteResourceName
                //, Managers.CharacterSkinManager.Instance.GetSkinBodyNumber()
                //, ActorType.Knight);
                //UICharacterAniManager.Instance.PlayIdleAni();
            }

            PlayerSkinInfo playerSkinInfo = Managers.CharacterSkinManager.Instance.GetPlayerSkinInfo(characterSkinData);

            if (m_SkinName != null)
            {
                m_SkinName.gameObject.SetActive(true);
                Managers.LocalStringManager.Instance.SetLocalizeText(m_SkinName, characterSkinData.NameLocalStringKey);
            }

            if (m_grade_Text != null)
            {
                m_grade_Text.SetText(characterSkinData.SkinGrade.ToString());
                m_grade_Text.color = StaticResource.Instance.GetV2GradeTextColor(characterSkinData.SkinGrade);

                V2GradeColorData v2GradeColorData = StaticResource.Instance.GetV2GradeColorData(characterSkinData.SkinGrade);
                m_grade_Text.enableVertexGradient = v2GradeColorData.UseGradeTextGradation;
                if (v2GradeColorData.UseGradeTextGradation == true)
                {
                    m_grade_Text.colorGradient = v2GradeColorData.GradeTextColorGradient;
                }

                m_grade_Text.gameObject.SetActive(true);
            }


            if (m_SkinPossEffect_Title != null)
            {
                CharacterBaseStatData statdata = Managers.ARRRStatManager.Instance.GetCharacterBaseStatData(characterSkinData.OwnEffectType);
                Managers.LocalStringManager.Instance.SetLocalizeText(m_SkinPossEffect_Title, statdata.NameLocalStringKey);

                if (m_SkinPossEffect_Value != null)
                {
                    double ownvalue = Managers.CharacterSkinManager.Instance.GetSkinOwnEffectValue(characterSkinData);

                    m_SkinPossEffect_Value.text = Util.GetAddStatPrintValue(ownvalue, statdata);
                }
            }


            if (m_SkinLv != null)
                m_SkinLv.SetText("Lv.{0}", Managers.CharacterSkinManager.Instance.GetSkinLevel(characterSkinData));

            if (playerSkinInfo == null)
            {
                if (m_detailSkinEquipBtn != null)
                {
                    m_detailSkinEquipBtn.interactable = false;
                    m_detailSkinEquipBtn.image.color = m_detailSkinEquipBtn_Disable;
                }

                if (m_detailSkinEquipText != null)
                {
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_detailSkinEquipText, "common/ui/equip");
                    m_detailSkinEquipText.color = m_detailSkinEquipText_Disable;
                }

                if (m_detailSkinEnhanceBtn != null)
                {
                    m_detailSkinEnhanceBtn.image.color = m_detailSkinEnhanceBtn_Disable;
                    m_detailSkinEnhanceBtn.interactable = false;
                }

                if (m_detailSkinEnhanceTitleText != null)
                {
                    m_detailSkinEnhanceTitleText.color = m_detailSkinEnhanceTitleText_Disable;
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_detailSkinEnhanceTitleText, "common/ui/levelUp");
                }

                if (m_detailSkinEnhancePriceIcon != null)
                    m_detailSkinEnhancePriceIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.SkinLevelUP.Enum32ToInt());

                if (m_detailSkinEnhancePrice != null)
                {
                    m_detailSkinEnhancePrice.text = "-";
                    m_detailSkinEnhancePrice.color = m_detailSkinEnhanceTitleText_Disable;
                }
            }
            else
            {
                bool isEquipState = Managers.CharacterSkinManager.Instance.IsEquipSkin(characterSkinData);

                if (isEquipState == true)
                {
                    if (m_detailSkinEquipBtn != null)
                    {
                        m_detailSkinEquipBtn.interactable = true;
                        m_detailSkinEquipBtn.image.color = m_detailSkinEquipBtn_Already;
                    }

                    if (m_detailSkinEquipText != null)
                    {
                        Managers.LocalStringManager.Instance.SetLocalizeText(m_detailSkinEquipText, "common/ui/unEquip");
                        m_detailSkinEquipText.color = m_detailSkinEquipText_Already;
                    }
                }
                else
                {
                    if (m_detailSkinEquipBtn != null)
                    {
                        m_detailSkinEquipBtn.interactable = true;
                        m_detailSkinEquipBtn.image.color = m_detailSkinEquipBtn_Enable;
                    }

                    if (m_detailSkinEquipText != null)
                    {
                        Managers.LocalStringManager.Instance.SetLocalizeText(m_detailSkinEquipText, "common/ui/equip");
                        m_detailSkinEquipText.color = m_detailSkinEquipText_Enable;
                    }
                }

                if (Managers.CharacterSkinManager.Instance.CheckIsReadyStarUp(characterSkinData))
                {
                    if (m_detailSkinEnhanceBtn != null)
                    {
                        m_detailSkinEnhanceBtn.image.color = m_detailSkinEnhanceBtn_Enable;
                        m_detailSkinEnhanceBtn.interactable = true;
                    }

                    if (m_detailSkinEnhancePriceIcon != null)
                        m_detailSkinEnhancePriceIcon.sprite = Managers.CharacterSkinManager.Instance.GetSkinSprite(characterSkinData);

                    if (m_detailSkinEnhanceTitleText != null)
                    {
                        m_detailSkinEnhanceTitleText.color = m_detailSkinEnhanceTitleText_Enable;
                        Managers.LocalStringManager.Instance.SetLocalizeText(m_detailSkinEnhanceTitleText, "common/ui/surpass");
                    }

                    if (m_detailSkinEnhancePrice != null)
                    {
                        m_detailSkinEnhancePrice.SetText("{0}", Managers.CharacterSkinManager.Instance.GetNeedStarUpSkinPrice(characterSkinData));
                        m_detailSkinEnhancePrice.color = m_detailSkinEnhanceTitleText_Enable;
                    }
                }
                else
                {
                    if (m_detailSkinEnhancePriceIcon != null)
                        m_detailSkinEnhancePriceIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.SkinLevelUP.Enum32ToInt());

                    if (m_detailSkinEnhanceTitleText != null)
                    {
                        Managers.LocalStringManager.Instance.SetLocalizeText(m_detailSkinEnhanceTitleText, "common/ui/levelUp");
                    }

                    bool isMax = Managers.CharacterSkinManager.Instance.IsMaxLevel(characterSkinData);
                    if (isMax == true)
                    {
                        if (m_detailSkinEnhanceBtn != null)
                        {
                            m_detailSkinEnhanceBtn.image.color = m_detailSkinEnhanceBtn_Disable;
                            m_detailSkinEnhanceBtn.interactable = false;
                        }

                        if (m_detailSkinEnhanceTitleText != null)
                        {
                            m_detailSkinEnhanceTitleText.color = m_detailSkinEnhanceTitleText_Disable;
                        }

                        if (m_detailSkinEnhancePrice != null)
                        {
                            m_detailSkinEnhancePrice.text = "-";
                            m_detailSkinEnhancePrice.color = m_detailSkinEnhanceTitleText_Disable;
                        }
                    }
                    else
                    {
                        bool isReadyEnhance = Managers.CharacterSkinManager.Instance.CheckIsReadyEnhance(characterSkinData);

                        if (m_detailSkinEnhanceBtn != null)
                        {
                            m_detailSkinEnhanceBtn.image.color = isReadyEnhance == true ? m_detailSkinEnhanceBtn_Enable : m_detailSkinEnhanceBtn_Disable;
                            m_detailSkinEnhanceBtn.interactable = isReadyEnhance;
                        }

                        if (m_detailSkinEnhanceTitleText != null)
                        {
                            m_detailSkinEnhanceTitleText.color = isReadyEnhance == true ? m_detailSkinEnhanceTitleText_Enable : m_detailSkinEnhanceTitleText_Disable;
                        }

                        if (m_detailSkinEnhancePrice != null)
                        {
                            m_detailSkinEnhancePrice.text = Util.GetAlphabetNumber(Managers.CharacterSkinManager.Instance.GetNeedEnhanceSkinPrice(characterSkinData));
                            m_detailSkinEnhancePrice.color = isReadyEnhance == true ? m_detailSkinEnhanceTitleText_Enable : m_detailSkinEnhanceTitleText_Disable;
                        }
                    }
                }
            }

            m_currentSkinData = characterSkinData;

            if (m_currentSkinData != null)
            {
                if (m_uiSkinElements_Dic.ContainsKey(m_currentSkinData) == true)
                {
                    UISkinElement uISkinElement = m_uiSkinElements_Dic[m_currentSkinData];
                    uISkinElement.SetSelectState(true);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void ScrollViewSnapToItem()
        {
            UISkinElement uIMasteryElement = null;

            if (m_currentSkinData == null)
            {
                if (m_uiSkinElements_List.Count > 0)
                {
                    uIMasteryElement = m_uiSkinElements_List[0];
                }
            }
            else
            {
                uIMasteryElement = m_uiSkinElements_Dic[m_currentSkinData];
            }

            if (uIMasteryElement != null)
            {
                RectTransform rectTransform = null;
                if (uIMasteryElement.TryGetComponent(out rectTransform))
                {
                    Vector2 offset = Vector2.zero;
                    offset.y = -50.0f;

                    Util.ScrollViewSnapToItem(m_SkinElementScrollRect, m_SkinElementGroup_Root, rectTransform, offset);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            RequestDialogExit<CharacterSkinDialog>();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SkinElement(CharacterSkinData characterSkinData)
        {
            Managers.CharacterSkinManager.Instance.SetFakeWeaponSkin(characterSkinData);

            SetSkinDetail(characterSkinData);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_EquipBtn()
        {
            if (m_currentSkinData == null)
                return;

            bool isEquipState = Managers.CharacterSkinManager.Instance.IsEquipSkin(m_currentSkinData);

            if (isEquipState == true)
            {
                Managers.CharacterSkinManager.Instance.DoUnEquipSkin(m_currentSkinData.SkinType);
            }
            else
            {
                if (Managers.CharacterSkinManager.Instance.DoEquipSkin(m_currentSkinData) == true)
                {
                }
            }

            SetSkinDetail(m_currentSkinData);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_EnhanceBtn()
        {
            if (m_currentSkinData == null)
                return;

            if (Managers.CharacterSkinManager.Instance.CheckIsReadyStarUp(m_currentSkinData) == true)
            {
                if (Managers.CharacterSkinManager.Instance.DoSkinStarUp(m_currentSkinData) == true)
                    SetSkinDetail(m_currentSkinData);
            }
            else
            {
                if (Managers.CharacterSkinManager.Instance.DoSkinEnhance(m_currentSkinData) == true)
                    SetSkinDetail(m_currentSkinData);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_GearSlot(V2Enum_Skin v2Enum_Goods)
        {
            SetSkinDetailView(v2Enum_Goods);
        }
        //------------------------------------------------------------------------------------
    }
}