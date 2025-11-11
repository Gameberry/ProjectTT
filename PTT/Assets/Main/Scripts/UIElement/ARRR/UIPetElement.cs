using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class UIPetElement : InfiniteScrollItem
    {
        [SerializeField]
        protected Button m_elementbtn;

        [SerializeField]
        protected Image m_allyIcon;

        [SerializeField]
        protected TMP_Text _petName;

        [SerializeField]
        protected Image m_allyFrame;

        [SerializeField]
        public Transform Frame_Light_SS;

        [SerializeField]
        public Transform Frame_Light_S;


        [SerializeField]
        protected Image m_gradeColorBG;

        [SerializeField]
        protected TMP_Text m_allyRarityText;


        [SerializeField]
        protected Transform m_canLevelUPIcon;

        [SerializeField]
        protected TMP_Text m_allyLevelText;

        [SerializeField]
        protected Transform m_equipMark;


        [SerializeField]
        protected Transform m_selectMark;

        [SerializeField]
        protected Transform m_disableImage;


        [SerializeField]
        private Image m_amountCountImage;

        [SerializeField]
        private TMP_Text m_amountCountText;

        private PetData m_currentV3AllyInfo;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (m_elementbtn != null)
                m_elementbtn.onClick.AddListener(OnSelect);
        }
        //------------------------------------------------------------------------------------
        protected override void OnInitalize()
        {
            scroll?.AddSelectCallback(OnSelectCallBack);
        }
        //------------------------------------------------------------------------------------
        public override void UpdateData(InfiniteScrollData scrollData)
        {
            PetData playerV3AllyInfo = scrollData as PetData;
            if (playerV3AllyInfo == null)
                return;

            SetAllyElement(playerV3AllyInfo);
        }
        //------------------------------------------------------------------------------------
        public void SetAllyElement(PetData petData)
        {
            m_currentV3AllyInfo = petData;

            PetInfo petInfo = Managers.PetManager.Instance.GetPetInfo(petData);

            if (petData == null)
                return;

            if (_petName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_petName, petData.NameLocalStringKey);

            SetAllyBaseState(petData);

            SetAmount(petData);

            if (m_disableImage != null)
                m_disableImage.gameObject.SetActive(petInfo == null);

            if (m_allyLevelText != null)
            {
                //m_allyLevelText.gameObject.SetActive(true);
                m_allyLevelText.text = string.Format("Lv.{0}", Managers.PetManager.Instance.GetPetLevel(petData));
            }

            SetSelect(GetDataIndex() == scroll.GetSelectDataIndex());

            SetEquip(Managers.PetManager.Instance.IsEquipPet(petData));
        }
        //------------------------------------------------------------------------------------
        protected void SetAllyBaseState(PetData petData)
        {
            SetGrade(petData.Grade);

            if (m_allyIcon != null)
                m_allyIcon.sprite = Managers.PetManager.Instance.GetPetIcon(petData);
        }
        //------------------------------------------------------------------------------------
        public void SetGrade(V2Enum_Grade v2Enum_Grade)
        {
            //if (m_gradeColorBG != null)
            //    m_gradeColorBG.color = StaticResource.Instance.GetV2GradeColor(v2Enum_Grade);

            if (m_allyFrame != null)
                StaticResource.Instance.SetAllyFrame(v2Enum_Grade, m_allyFrame);

            if (m_allyRarityText != null)
            {
                m_allyRarityText.text = v2Enum_Grade.ToString();
                m_allyRarityText.color = StaticResource.Instance.GetV2GradeTextColor(v2Enum_Grade);

                V2GradeColorData v2GradeColorData = StaticResource.Instance.GetV2GradeColorData(v2Enum_Grade);
                m_allyRarityText.enableVertexGradient = v2GradeColorData.UseGradeTextGradation;
                if (v2GradeColorData.UseGradeTextGradation == true)
                {
                    m_allyRarityText.colorGradient = v2GradeColorData.GradeTextColorGradient;
                }
            }

            if (v2Enum_Grade == V2Enum_Grade.SS || v2Enum_Grade == V2Enum_Grade.SR)
            {
                if (Frame_Light_SS != null)
                    Frame_Light_SS.gameObject.SetActive(true);

                if (Frame_Light_S != null)
                    Frame_Light_S.gameObject.SetActive(false);
            }
            else if (v2Enum_Grade == V2Enum_Grade.S)
            {
                if (Frame_Light_SS != null)
                    Frame_Light_SS.gameObject.SetActive(false);

                if (Frame_Light_S != null)
                    Frame_Light_S.gameObject.SetActive(true);
            }
            else
            {
                if (Frame_Light_SS != null)
                    Frame_Light_SS.gameObject.SetActive(false);

                if (Frame_Light_S != null)
                    Frame_Light_S.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        protected void SetAmount(PetData petData)
        {
            if (petData == null)
                return;

            PetInfo petInfo = Managers.PetManager.Instance.GetPetInfo(petData);

            if (petInfo == null)
            {
                if (m_amountCountImage != null)
                    m_amountCountImage.fillAmount = 0.0f;

                if (m_amountCountText != null)
                    m_amountCountText.SetText("-");

                if (m_canLevelUPIcon != null)
                    m_canLevelUPIcon.gameObject.SetActive(false);

                return;
            }

            int needCount = Managers.PetManager.Instance.GetLevelUpCost(petData);

            float amountratio = (float)petInfo.Count / (float)(needCount);

            if (m_amountCountImage != null)
                m_amountCountImage.fillAmount = amountratio;

            if (m_amountCountText != null)
                m_amountCountText.SetText("{0}/{1}", petInfo.Count, needCount);

            if (m_canLevelUPIcon != null)
                m_canLevelUPIcon.gameObject.SetActive(petInfo.Count >= needCount);
        }
        //------------------------------------------------------------------------------------
        public void SetSelect(bool select)
        {
            if (m_selectMark != null)
            {
                m_selectMark.gameObject.SetActive(select);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetEquip(bool select)
        {
            if (m_equipMark != null)
            {
                m_equipMark.gameObject.SetActive(select);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnSelectCallBack(InfiniteScrollData infiniteScrollData)
        {
            SetSelect(infiniteScrollData == scrollData);
        }
        //------------------------------------------------------------------------------------
    }
}