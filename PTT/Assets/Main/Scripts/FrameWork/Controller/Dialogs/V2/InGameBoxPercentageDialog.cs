using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class InGameBoxPercentageDialog : IDialog
    {
        [SerializeField]
        private List<Button> m_exitBtn;

        [SerializeField]
        private TMP_Text m_boxTitle;

        [Header("------------Element------------")]
        [SerializeField]
        private Transform m_elementRoot;


        [SerializeField]
        private UICollectionDescElement m_uIDescElement;

        private Queue<UICollectionDescElement> m_uICollectionDescElements_Pool = new Queue<UICollectionDescElement>();

        private Queue<UICollectionDescElement> m_uICollectionDescElements_Use = new Queue<UICollectionDescElement>();


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

            Message.AddListener<GameBerry.Event.SetRandomBoxPercentageMsg>(SetRandomBoxPercentage);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetRandomBoxPercentageMsg>(SetRandomBoxPercentage);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            RequestDialogExit<InGameBoxPercentageDialog>();
        }
        //------------------------------------------------------------------------------------
        private void SetRandomBoxPercentage(GameBerry.Event.SetRandomBoxPercentageMsg msg)
        {
            SetSummonPercentUI(msg.boxData);
        }
        //------------------------------------------------------------------------------------
        private void SetSummonPercentUI(BoxData boxData)
        {
            if (boxData == null)
                return;

            if (m_boxTitle != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_boxTitle, boxData.NameLocalStringKey);

            if (boxData.BoxType != V2Enum_BoxType.RandomTypeBox)
                return;

            while (m_uICollectionDescElements_Use.Count > 0)
                PoolDescElement(m_uICollectionDescElements_Use.Dequeue());

            double totalweight = boxData.weightedRandomPicker_RandomTypeBox.GetTotalWeight();

            for (int i = 0; i < boxData.RandomTypeBoxReward.Count; ++i)
            {
                BoxComponentsData randomBoxRewardData = boxData.RandomTypeBoxReward[i];
                RewardData rewardData = randomBoxRewardData.rewardData;

                double weight = randomBoxRewardData.GoodsProb;

                UICollectionDescElement uIRuneCollectionDescElement = GetDescElement();
                uIRuneCollectionDescElement.SetCollectionElement(
                    Managers.GoodsManager.Instance.GetGoodsSprite(rewardData.Index),
                    Managers.GoodsManager.Instance.GetGoodsGrade(rewardData.Index),
                    string.Empty,
                    string.Format("{0:0.0000}%", (weight / totalweight) * 100.0),
                    rewardData.Index
                    );
                uIRuneCollectionDescElement.SetCollectionName_Localize(Managers.GoodsManager.Instance.GetGoodsLocalKey(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                uIRuneCollectionDescElement.transform.SetAsLastSibling();
                uIRuneCollectionDescElement.gameObject.SetActive(true);

                m_uICollectionDescElements_Use.Enqueue(uIRuneCollectionDescElement);
            }
        }
        //-----------------------------------------------------------------------------------
        private UICollectionDescElement GetDescElement()
        {
            UICollectionDescElement uICollectionDescElement = null;

            if (m_uICollectionDescElements_Pool.Count <= 0)
            {
                GameObject titleClone = Instantiate(m_uIDescElement.gameObject, m_elementRoot);
                uICollectionDescElement = titleClone.GetComponent<UICollectionDescElement>();
            }
            else
                uICollectionDescElement = m_uICollectionDescElements_Pool.Dequeue();

            uICollectionDescElement.gameObject.SetActive(true);

            return uICollectionDescElement;
        }
        //------------------------------------------------------------------------------------
        private void PoolDescElement(UICollectionDescElement uICollectionDescElement)
        {
            uICollectionDescElement.gameObject.SetActive(false);
            m_uICollectionDescElements_Pool.Enqueue(uICollectionDescElement);
        }
        //------------------------------------------------------------------------------------
    }
}