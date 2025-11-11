using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
    public class InGameGoodsDropDirectionDialog : IDialog
    {
        [SerializeField]
        private Transform root;

        [SerializeField]
        private UIGoodsDropDirectionElement uIGoodsDropDirectionElementObj;

        [SerializeField]
        private int m_spawnDefaultCount = 4;

        [SerializeField]
        private int m_spawnLimitCount = 6;

        [SerializeField]
        private float explo_range = 0.5f;

        [SerializeField]
        private Ease FromEase = Ease.OutCubic;

        [SerializeField]
        private float FromDuration = 0.25f;

        [SerializeField]
        private Ease ToEase = Ease.InCubic;

        [SerializeField]
        private float ToDuration = 0.25f;

        private Queue<UIGoodsDropDirectionElement> m_uIGoodsDropDirectionElements = new Queue<UIGoodsDropDirectionElement>();

        [SerializeField]
        private int spawnElementLimitCount = 20;

        private int spawnElementCount = 0;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            Message.AddListener<GameBerry.Event.ShowGoodsDropMsg>(ShowGoodsDrop);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.ShowGoodsDropMsg>(ShowGoodsDrop);
        }
        //------------------------------------------------------------------------------------
        private void ShowGoodsDrop(GameBerry.Event.ShowGoodsDropMsg msg)
        {
            int spawnCount = msg.amount;
            if (spawnCount == -1)
                spawnCount = m_spawnDefaultCount;

            if (spawnCount > m_spawnLimitCount)
                spawnCount = m_spawnLimitCount;

            for (int i = 0; i < spawnCount; ++i)
            {
                UIGoodsDropDirectionElement uIGoodsDropDirectionElement = GetUIGoodsDropDirectionElement();
                if (uIGoodsDropDirectionElement == null)
                    continue;

                uIGoodsDropDirectionElement.gameObject.SetActive(true);
                uIGoodsDropDirectionElement.Explosion(msg.sprite, msg.from, msg.to, explo_range, FromEase, FromDuration, ToEase, ToDuration).Forget();
            }
        }
        //------------------------------------------------------------------------------------
        private UIGoodsDropDirectionElement GetUIGoodsDropDirectionElement()
        {
            if (m_uIGoodsDropDirectionElements.Count > 0)
                return m_uIGoodsDropDirectionElements.Dequeue();

            if (spawnElementLimitCount <= spawnElementCount)
                return null;

            GameObject clone = Instantiate(uIGoodsDropDirectionElementObj.gameObject, root);
            if (clone == null)
                return null;

            UIGoodsDropDirectionElement uIGoodsDropDirectionElement = clone.GetComponent<UIGoodsDropDirectionElement>();
            uIGoodsDropDirectionElement.Init(PoolElement);

            spawnElementCount++;

            return uIGoodsDropDirectionElement;
        }
        //------------------------------------------------------------------------------------
        private void PoolElement(UIGoodsDropDirectionElement uIGoodsDropDirectionElement)
        {
            m_uIGoodsDropDirectionElements.Enqueue(uIGoodsDropDirectionElement);
        }
        //------------------------------------------------------------------------------------
    }
}