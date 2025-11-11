using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
    public class UIQuestGaugeRewardElement : MonoBehaviour
    {
        [SerializeField]
        private Image rewardIcon;

        [SerializeField]
        private TMP_Text rewardAmount;
        
        [SerializeField]
        private TMP_Text drawCount;

        [SerializeField]
        private Color shortColor = Color.white;

        [SerializeField]
        private Color readyColor = Color.white;

        [SerializeField]
        private Color alreadyColor = Color.white;

        [SerializeField]
        private Transform readyReward;

        [SerializeField]
        private Transform lockReward;

        [SerializeField]
        private Transform alReadyReward;

        [SerializeField]
        private Transform _redDot;

        [SerializeField]
        private Button showitemInfo;

        private QuestGaugeData currentQuestGaugeData;

        private RewardData currentRewardData = null;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (showitemInfo != null)
                showitemInfo.onClick.AddListener(OnClick_ShowInfo);
        }
        //------------------------------------------------------------------------------------
        

        public void SetQuestGaugeElement(QuestGaugeData questGaugeData)
        {
            if (questGaugeData == null && questGaugeData.RewardData == null)
                return;

            currentQuestGaugeData = questGaugeData;

            if (rewardIcon != null)
                rewardIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(questGaugeData.RewardData.V2Enum_Goods.Enum32ToInt(), questGaugeData.RewardData.Index);

            if (rewardAmount != null)
                rewardAmount.SetText(Util.GetAlphabetNumber(questGaugeData.RewardData.Amount));

            currentRewardData = questGaugeData.RewardData;

            RefreshQuestGaugeElement();
        }
        //------------------------------------------------------------------------------------
        public void RefreshQuestGaugeElement()
        {
            if (currentQuestGaugeData == null)
                return;

            bool isRecved = Managers.QuestManager.Instance.GetRecvedOnceReward(currentQuestGaugeData.QuestType) >= currentQuestGaugeData.RequiredQuestCount;

            int currentAccum = Managers.QuestManager.Instance.GetEventRouletteAccumCount(currentQuestGaugeData.QuestType);

            if (currentAccum > currentQuestGaugeData.RequiredQuestCount)
                currentAccum = currentQuestGaugeData.RequiredQuestCount;

            if (drawCount != null)
                drawCount.SetText("{0}", currentQuestGaugeData.RequiredQuestCount);

            if (readyReward != null)
                readyReward.gameObject.SetActive(false);

            if (alReadyReward != null)
                alReadyReward.gameObject.SetActive(isRecved);

            if (rewardIcon != null)
                rewardIcon.color = isRecved == true ? Color.gray : Color.white;

            if (isRecved == true)
            {
                if (drawCount != null)
                    drawCount.color = alreadyColor;

                if (lockReward != null)
                    lockReward.gameObject.SetActive(false);

                if (_redDot != null)
                    _redDot.gameObject.SetActive(false);
            }
            else
            {
                bool ready = currentAccum >= currentQuestGaugeData.RequiredQuestCount;

                if (lockReward != null)
                    lockReward.gameObject.SetActive(ready == false);

                if (_redDot != null)
                    _redDot.gameObject.SetActive(ready);

                if (ready == true)
                {
                    if (drawCount != null)
                        drawCount.color = readyColor;

                    if (readyReward != null)
                        readyReward.gameObject.SetActive(true);

                }
                else
                {
                    if (drawCount != null)
                        drawCount.color = shortColor;
                }
            }
        }
        //------------------------------------------------------------------------------------
        public QuestGaugeData GetGaugeData()
        {
            return currentQuestGaugeData;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowInfo()
        {
            if (currentRewardData == null)
                return;

            Contents.GlobalContent.ShowGoodsDescPopup(currentRewardData.V2Enum_Goods, currentRewardData.Index, currentRewardData.Amount);
        }
        //------------------------------------------------------------------------------------
    }
}