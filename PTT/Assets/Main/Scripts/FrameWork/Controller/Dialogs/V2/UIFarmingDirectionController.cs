using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Common;

namespace GameBerry.UI
{
    public class FarmingData
    {
        public Sprite RewardIcon;
        public Color RewardBGColor;
        public string RewardCountText;
    }

    public class UIFarmingDirectionController : MonoBehaviour
    {
        [SerializeField]
        private Transform _farmingListRoot;

        [SerializeField]
        private UIFarmingElement _farmingElement;

        private LinkedList<UIFarmingElement> _farmingElementPool = new LinkedList<UIFarmingElement>();

        private Queue<FarmingData> _farmingDataPool = new Queue<FarmingData>();
        private Queue<FarmingData> _waitFarmingData = new Queue<FarmingData>();

        public bool SetSiblingLast = false;

        //------------------------------------------------------------------------------------
        [SerializeField]
        private float _showTurmTimer = 0.06f;
        private float _lastShowTime = 0.0f;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            for (int i = 0; i < 8; ++i)
            {
                GameObject clone = Instantiate(_farmingElement.gameObject, _farmingListRoot);

                if (clone != null)
                {
                    UIFarmingElement element = clone.GetComponent<UIFarmingElement>();
                    element.Init(PopParmingData);
                    _farmingElementPool.AddLast(element);
                }

                clone.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (_waitFarmingData.Count > 0)
            {
                if (_lastShowTime < Time.time)
                { 
                    var node = _farmingElementPool.First;
                    UIFarmingElement element = node.Value;
                    element.gameObject.SetActive(true);
                    _farmingElementPool.RemoveFirst();

                    element.PlayDirection(_waitFarmingData.Dequeue());
                    _farmingElementPool.AddLast(element);
                    if (SetSiblingLast == true)
                        element.transform.SetAsLastSibling();
                    else
                        element.transform.SetAsFirstSibling();

                    _lastShowTime = Time.time + _showTurmTimer;
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void ShowFarmingReward(Sprite sprite, string content)
        {
            if (_farmingDataPool.Count <= 0)
                _farmingDataPool.Enqueue(new FarmingData());

            FarmingData data = _farmingDataPool.Dequeue();
            data.RewardIcon = sprite;
            data.RewardCountText = content;

            _waitFarmingData.Enqueue(data);
        }
        //------------------------------------------------------------------------------------
        private void PopParmingData(FarmingData data)
        {
            _farmingDataPool.Enqueue(data);
        }
        //------------------------------------------------------------------------------------
        public void ForceRelease()
        {
            var node = _farmingElementPool.First;
            while (node != null)
            {
                node.Value.ForceRelease();
                node = node.Next;
            }
        }
        //------------------------------------------------------------------------------------
    }
}