using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class GoodsDropDirectionManager : MonoSingleton<GoodsDropDirectionManager>
    {
        private Dictionary<V2Enum_Goods, Transform> m_goodsPosition = new Dictionary<V2Enum_Goods, Transform>();
        private Dictionary<V2Enum_Point, Transform> m_pointPosition = new Dictionary<V2Enum_Point, Transform>();

        private Dictionary<string, Transform> m_customPosition = new Dictionary<string, Transform>();

        private Event.ShowGoodsDropMsg m_showGoodsDropMsg = new Event.ShowGoodsDropMsg();

        private Camera m_characterCamera;
        private Camera m_uiCamera;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            // 원본코드 잠시 주석 2024.09.30
            //m_characterCamera = PlayerManager.Instance.GetInGameCameraController().GetComponent<Camera>();
            m_characterCamera = BattleSceneCamera.Instance.BattleCamera;
            m_uiCamera = UI.UIManager.Instance.screenCanvasCamera;
        }
        //------------------------------------------------------------------------------------
        public void AddGoodsPosition(V2Enum_Goods v2Enum_Goods, Transform pos)
        {
            if (m_goodsPosition.ContainsKey(v2Enum_Goods) == false)
                m_goodsPosition.Add(v2Enum_Goods, pos);
        }
        //------------------------------------------------------------------------------------
        public void AddPointPosition(V2Enum_Point v2Enum_Point, Transform pos)
        {
            if (m_pointPosition.ContainsKey(v2Enum_Point) == false)
                m_pointPosition.Add(v2Enum_Point, pos);
        }
        //------------------------------------------------------------------------------------
        public void AddCustomPosition(string custom, Transform pos)
        {
            if (m_customPosition.ContainsKey(custom) == false)
                m_customPosition.Add(custom, pos);
        }
        //------------------------------------------------------------------------------------
        public void ShowDropIn_World(V2Enum_Goods v2Enum_Goods, int index, Vector3 from, int amount = -1)
        {
            Vector3 fromScreenPoin = m_characterCamera.WorldToScreenPoint(from);
            Vector3 fromPos = m_uiCamera.ScreenToWorldPoint(fromScreenPoin);


            ShowDropIn(v2Enum_Goods, index, fromPos, amount);
        }
        //------------------------------------------------------------------------------------
        public void ShowDropIn_World(V2Enum_Goods v2Enum_Goods, int index, Vector3 from, Vector3 to, int amount = -1)
        {
            Vector3 fromScreenPoin = m_characterCamera.WorldToScreenPoint(from);
            Vector3 fromPos = m_uiCamera.ScreenToWorldPoint(fromScreenPoin);

            Vector3 toScreenPoin = m_characterCamera.WorldToScreenPoint(to);
            Vector3 toPos = m_uiCamera.ScreenToWorldPoint(toScreenPoin);

            ShowDropIn(v2Enum_Goods, index, fromPos, toPos, amount);
        }
        //------------------------------------------------------------------------------------
        public void ShowDropIn_FromWorld_ToScreen(V2Enum_Goods v2Enum_Goods, int index, Vector3 from, Vector3 to, int amount = -1)
        {
            Vector3 fromScreenPoin = m_characterCamera.WorldToScreenPoint(from);
            Vector3 fromPos = m_uiCamera.ScreenToWorldPoint(fromScreenPoin);

            ShowDropIn(v2Enum_Goods, index, fromPos, to, amount);
        }
        //------------------------------------------------------------------------------------
        public void ShowDropIn(V2Enum_Goods v2Enum_Goods, int index, Vector3 from, int amount = -1)
        {
            Sprite goodsSprite = Managers.GoodsManager.Instance.GetGoodsSprite(v2Enum_Goods.Enum32ToInt(), index);
            Vector2 to = GetToPosition(v2Enum_Goods, index);

            PlayGoodsDrop(goodsSprite, from, to, amount);
        }
        //------------------------------------------------------------------------------------
        public void ShowDropIn(V2Enum_Goods v2Enum_Goods, int index, Vector3 from, Vector3 to, int amount = -1)
        {
            Sprite goodsSprite = Managers.GoodsManager.Instance.GetGoodsSprite(v2Enum_Goods.Enum32ToInt(), index);

            PlayGoodsDrop(goodsSprite, from, to, amount);
        }
        //------------------------------------------------------------------------------------
        private Vector3 GetToPosition(V2Enum_Goods v2Enum_Goods, int index)
        {
            if (v2Enum_Goods == V2Enum_Goods.Point
                || v2Enum_Goods == V2Enum_Goods.TimePoint)
            {
                V2Enum_Point v2Enum_Point = index.IntToEnum32<V2Enum_Point>();

                if (m_pointPosition.ContainsKey(v2Enum_Point) == true)
                    return m_pointPosition[v2Enum_Point].position;
            }
            else
            {
                if (m_goodsPosition.ContainsKey(v2Enum_Goods) == true)
                    return m_goodsPosition[v2Enum_Goods].position;
            }

            return Vector2.zero;
        }
        //------------------------------------------------------------------------------------
        public Vector3 GetCustomToPosition(string key)
        {
            if (m_customPosition.ContainsKey(key) == true)
                return m_customPosition[key].position;

            return Vector2.zero;
        }
        //------------------------------------------------------------------------------------
        private void PlayGoodsDrop(Sprite sprite, Vector3 from, Vector3 to, int amount = -1)
        {
            m_showGoodsDropMsg.sprite = sprite;
            m_showGoodsDropMsg.from = from;
            m_showGoodsDropMsg.to = to;
            m_showGoodsDropMsg.amount = amount;

            Message.Send(m_showGoodsDropMsg);
        }
        //------------------------------------------------------------------------------------

    }
}