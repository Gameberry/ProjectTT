using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    [System.Serializable]
    public class RewardIconUIs
    {
        public V2Enum_Goods V2Enum_Goods;

        public Image Icon;
        public Image GradeColorBG;
        public Image Grade_Text_BG;
        public TMP_Text Grade_Text;
        public TMP_Text Amount_Text;

        public Transform LightCircle;
    }

    public class UIGlobalGoodsRewardIconElement : MonoBehaviour
    {
        [SerializeField]
        private Button m_button;

        [SerializeField]
        private Image m_icon;

        [SerializeField]
        private Transform _spotlight;

        [SerializeField]
        private Image m_gradeColorBG;

        [SerializeField]
        private TMP_Text m_grade_Text;

        [SerializeField]
        private Image m_grade_ImageText;

        [SerializeField]
        private TMP_Text m_amount_Text;

        [SerializeField]
        public Transform m_lightCircle_Group;

        [SerializeField]
        public Transform m_lightCircle_SS;

        [SerializeField]
        public Transform m_lightCircle_S;

        [SerializeField]
        public Transform m_lightCircle_Order;

        [SerializeField]
        private Transform _duplicationRoot;

        [SerializeField]
        private Image _duplicationIcon;

        [SerializeField]
        private TMP_Text _duplicationAmount;

        private System.Action<UIGlobalGoodsRewardIconElement> m_action = null;

        private RewardData m_currentRewardData;

        private V2Enum_Goods m_currentV2Enum_Goods = V2Enum_Goods.Max;
        private int m_currentIndex = -1;
        private double m_currentAmount = 0.0;

        private V2Enum_Grade _goodsGrade = V2Enum_Grade.Max;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (m_button != null)
                m_button.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        public void AddClickListener(System.Action<UIGlobalGoodsRewardIconElement> uIGlobalGoodsRewardIconElement)
        {
            m_action = uIGlobalGoodsRewardIconElement;
        }
        //------------------------------------------------------------------------------------
        public void SetRewardElement(RewardData rewardData)
        {
            m_currentRewardData = rewardData;

            m_currentV2Enum_Goods = V2Enum_Goods.Max;
            m_currentIndex = -1;

            if (rewardData.V2Enum_Goods == V2Enum_Goods.Max)
                rewardData.V2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(rewardData.Index);

            V2Enum_Grade v2Enum_Grade = Managers.GoodsManager.Instance.GetGoodsGrade(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index);
            double amount = rewardData.Amount;
            Sprite icon = null;

            
            icon = Managers.GoodsManager.Instance.GetGoodsSprite(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index);

            SetRewardElement(rewardData.V2Enum_Goods, rewardData.Index, icon, v2Enum_Grade, amount, rewardData);

            SetDuplication(rewardData.DupliIndex, rewardData.DupliAmount);
        }
        //------------------------------------------------------------------------------------
        public void SetRewardElement(V2Enum_Goods v2Enum_Goods, int index, Sprite icon, V2Enum_Grade v2Enum_Grade, double amount, RewardData rewardData = null)
        {
            if (icon == null)
                Debug.LogError(string.Format("EmptyIcon {0} {1}", v2Enum_Goods, index));

            if (_spotlight != null)
                _spotlight.gameObject.SetActive(false);

            m_currentRewardData = rewardData;

            m_currentV2Enum_Goods = v2Enum_Goods;
            m_currentIndex = index;
            m_currentAmount = amount;

            if (m_currentV2Enum_Goods == V2Enum_Goods.Max)
                m_currentV2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(m_currentIndex);

            if (m_lightCircle_Group != null)
                m_lightCircle_Group.gameObject.SetActive(false);

            if (m_currentRewardData != null)
            {
                V2Enum_Grade backLightGrade = Managers.GoodsManager.Instance.GetBackLightGrade(m_currentRewardData.Index);
                if (backLightGrade == V2Enum_Grade.SS || backLightGrade == V2Enum_Grade.S)
                    ShowLightCircle();
            }

            if (m_icon != null)
                m_icon.sprite = icon;

            V2Enum_Grade myV2Enum_Grade = v2Enum_Grade;

            if (v2Enum_Goods == V2Enum_Goods.Point
                || v2Enum_Goods == V2Enum_Goods.TimePoint)
            {
                SetGradeColor(V2Enum_Grade.Max);

                //Contents.V2PointColorData v2PointColorData = Contents.GlobalContent.GetV2PointColorData(index.IntToEnum32<V2Enum_Point>());

                //if (v2PointColorData == null)
                //    SetGradeColor(V2Enum_Grade.Max);
                //else
                //{
                //    m_grade_Text.gameObject.SetActive(false);

                //    if (m_gradeColorBG != null)
                //        m_gradeColorBG.color = v2PointColorData.GradeColor;
                //}
            }
            else
            {
                SetGradeColor(myV2Enum_Grade);
            }

            if (m_amount_Text != null)
            {
                if (amount <= 0)
                    m_amount_Text.gameObject.SetActive(false);
                else
                {
                    m_amount_Text.gameObject.SetActive(true);
                    if (v2Enum_Goods == V2Enum_Goods.TimePoint)
                    {
                        m_amount_Text.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), amount);
                    }
                    else
                    {
                        if (v2Enum_Goods == V2Enum_Goods.Point
                            && index.IntToEnum32<V2Enum_Point>() == V2Enum_Point.LobbyGold)
                        {
                            m_amount_Text.text = string.Format("{0:#,0.##}", amount);
                        }
                        else
                            m_amount_Text.SetText(Util.GetAlphabetNumber(amount));
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void EnableSpotLight(bool enable)
        {
            if (_spotlight != null)
                _spotlight.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public void SetDuplication(int index, double amount)
        {
            if (index == -1)
            {
                if (_duplicationRoot != null)
                    _duplicationRoot.gameObject.SetActive(false);
            }
            else
            {
                if (_duplicationRoot != null)
                    _duplicationRoot.gameObject.SetActive(true);

                if (_duplicationIcon != null)
                    _duplicationIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(index);

                if(_duplicationAmount != null)
                    _duplicationAmount.text = string.Format("{0:#,0.##}", amount);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetGradeColor(V2Enum_Grade v2Enum_Grade)
        {
            V2Enum_Grade myV2Enum_Grade = v2Enum_Grade;

            if (m_gradeColorBG != null)
                m_gradeColorBG.SetGradeBGImage(v2Enum_Grade);

            //if (m_grade_Text != null)
            //    m_grade_Text.gameObject.SetActive(myV2Enum_Grade != V2Enum_Grade.Max);

            if (m_grade_ImageText != null)
                m_grade_ImageText.gameObject.SetActive(myV2Enum_Grade != V2Enum_Grade.Max);

            _goodsGrade = myV2Enum_Grade;

            if (myV2Enum_Grade == V2Enum_Grade.Max)
                return;

            //if (m_gradeColorBG != null)
            //    m_gradeColorBG.color = StaticResource.Instance.GetV2GradeColor(myV2Enum_Grade);

            //if (m_grade_Text != null)
            //{
            //    m_grade_Text.gameObject.SetActive(true);
            //    m_grade_Text.text = myV2Enum_Grade.ToString();
            //    m_grade_Text.color = StaticResource.Instance.GetV2GradeTextColor(myV2Enum_Grade);

            //    V2GradeColorData v2GradeColorData = StaticResource.Instance.GetV2GradeColorData(myV2Enum_Grade);
            //    m_grade_Text.enableVertexGradient = v2GradeColorData.UseGradeTextGradation;
            //    if (v2GradeColorData.UseGradeTextGradation == true)
            //    {
            //        m_grade_Text.colorGradient = v2GradeColorData.GradeTextColorGradient;
            //    }
            //}

            if (m_grade_ImageText != null)
                m_grade_ImageText.SetGradeTextImage(myV2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetGrade()
        {
            return _goodsGrade;
        }
        //------------------------------------------------------------------------------------
        public void ShowLightCircle()
        {
            if (m_lightCircle_Group != null)
            { 
                m_lightCircle_Group.gameObject.SetActive(true);

                if (m_currentRewardData == null)
                {
                    if (m_lightCircle_SS != null)
                        m_lightCircle_SS.gameObject.SetActive(false);

                    if (m_lightCircle_S != null)
                        m_lightCircle_S.gameObject.SetActive(false);

                    if (m_lightCircle_Order != null)
                        m_lightCircle_Order.gameObject.SetActive(true);
                }
                else
                {
                    V2Enum_Grade v2Enum_Grade = Managers.GoodsManager.Instance.GetBackLightGrade(m_currentRewardData.Index);

                    if (m_lightCircle_SS != null)
                        m_lightCircle_SS.gameObject.SetActive(v2Enum_Grade == V2Enum_Grade.SS);

                    if (m_lightCircle_S != null)
                        m_lightCircle_S.gameObject.SetActive(v2Enum_Grade == V2Enum_Grade.S);

                    if (m_lightCircle_Order != null)
                        m_lightCircle_Order.gameObject.SetActive(v2Enum_Grade != V2Enum_Grade.SS && v2Enum_Grade != V2Enum_Grade.S);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void HideLightCircle()
        {
            if (m_lightCircle_Group != null)
                m_lightCircle_Group.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        public void ForceShowAmount()
        {
            if (m_amount_Text != null)
            {
                m_amount_Text.gameObject.SetActive(true);
                if (m_currentV2Enum_Goods == V2Enum_Goods.TimePoint)
                {
                    m_amount_Text.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), m_currentAmount);
                }
                else
                {
                    if (m_currentV2Enum_Goods == V2Enum_Goods.Point
                        && m_currentIndex.IntToEnum32<V2Enum_Point>() == V2Enum_Point.LobbyGold)
                    {
                        m_amount_Text.text = string.Format("{0:#,0.##}", m_currentAmount);
                    }
                    else
                        m_amount_Text.SetText(Util.GetAlphabetNumber(m_currentAmount));
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void HideAmount()
        {
            if (m_amount_Text != null)
                m_amount_Text.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        public void ShowOpenGacha()
        {
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            if (m_action != null)
            { 
                m_action(this);
                m_action = null;
                return;
            }

            if (m_currentV2Enum_Goods != V2Enum_Goods.Max
                && m_currentIndex != -1)
            {
                Contents.GlobalContent.ShowGoodsDescPopup(m_currentV2Enum_Goods, m_currentIndex, m_currentAmount);
            }
        }
        //------------------------------------------------------------------------------------
        public RewardData GetRewardData()
        {
            return m_currentRewardData;
        }
        //------------------------------------------------------------------------------------
    }
}