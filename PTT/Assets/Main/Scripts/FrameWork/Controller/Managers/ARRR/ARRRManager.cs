using System.Collections.Generic;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using GameBerry.Common;

namespace GameBerry.Managers
{
    public class ARRRManager : MonoSingleton<ARRRManager>
    {
        private ARRRController _aRRRControllerObject;

        private ObjectPool<ARRRController> _aRRRControllerPool = new ObjectPool<ARRRController>();

        //------------------------------------------------------------------------------------
        public void InitCreatureContent()
        {
            ResourceLoader.Instance.Load<GameObject>("BattleScene/ARRRController", o =>
            {
                GameObject obj = o as GameObject;
                _aRRRControllerObject = obj.GetComponent<ARRRController>();
            });
        }
        //------------------------------------------------------------------------------------
        public ARRRController GetCreature()
        {
            ARRRController ARRRController = _aRRRControllerPool.GetObject() ?? CreateCreature();

            return ARRRController;
        }
        //------------------------------------------------------------------------------------
        public void PoolCreature(ARRRController aRRRController)
        {
            aRRRController.gameObject.SetActive(false);
            _aRRRControllerPool.PoolObject(aRRRController);
        }
        //------------------------------------------------------------------------------------
        private ARRRController CreateCreature()
        {
            GameObject clone = Instantiate(_aRRRControllerObject.gameObject, transform);

            ARRRController aRRRController = clone.GetComponent<ARRRController>();
            aRRRController.Init();

            return aRRRController;
        }
        //------------------------------------------------------------------------------------
    }
}