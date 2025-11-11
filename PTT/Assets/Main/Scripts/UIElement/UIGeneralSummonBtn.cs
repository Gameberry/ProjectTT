using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;
using Spine;
using Spine.Unity;

namespace GameBerry.UI
{
    public class UIGeneralSummonBtn : MonoBehaviour
    {
        [Header("---------GeneralSummon---------")]
        [SerializeField]
        private TMP_Text _generalSummonTicketAmount;

        [SerializeField]
        private Image _generalSummonTicketGauge;

        [SerializeField]
        private Color _generalSummonTicketGaugeColor_None = Color.white;

        [SerializeField]
        private Color _generalSummonTicketGaugeColor_Full = Color.white;

        [SerializeField]
        private Transform _reddot;

        void Awake()
        {
            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.GeneralSummonTicket.Enum32ToInt(), RefreshGeneralSummonTicket);

            RefreshGeneralSummonTicket(Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.GeneralSummonTicket.Enum32ToInt()));

        }
        //------------------------------------------------------------------------------------
        private void RefreshGeneralSummonTicket(double amount)
        {
            SummonData summonData = Managers.SummonManager.Instance.GetSummonData(V2Enum_SummonType.SummonNormal);

            if (summonData == null || summonData.SummonCostDatas == null || summonData.SummonCostDatas.Count <= 0)
                return;

            float targetCount = (float)(summonData.SummonCostParam12 * summonData.SummonCostDatas[0].summonCount);

            float ratio = (float)amount / targetCount;

            if (_generalSummonTicketAmount != null)
                _generalSummonTicketAmount.text = string.Format("{0}/{1}",
                    amount,
                    targetCount);

            bool canEnter = amount >= targetCount;

            if (_generalSummonTicketGauge != null)
            {
                _generalSummonTicketGauge.fillAmount = ratio;
                _generalSummonTicketGauge.color = canEnter == true ? _generalSummonTicketGaugeColor_Full : _generalSummonTicketGaugeColor_None;
            }

            if (_reddot != null)
                _reddot.gameObject.SetActive(canEnter);
        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.GeneralSummonTicket.Enum32ToInt(), RefreshGeneralSummonTicket);

        }
        //------------------------------------------------------------------------------------
    }
}