using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;

namespace GameBerry.Managers
{
    public class MonsterManager : MonoSingleton<MonsterManager>
    {
        private MonsterController _MonsterControllerObject;

        private ObjectPool<MonsterController> _MonsterControllerPool = new ObjectPool<MonsterController>();


        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            ResourceLoader.Instance.Load<GameObject>("BattleScene/MonsterController", o =>
            {
                GameObject obj = o as GameObject;
                _MonsterControllerObject = obj.GetComponent<MonsterController>();
            });

            for (int i = 0; i < 10; ++i)
            {
                CreateMonster();
            }
        }
        //------------------------------------------------------------------------------------
        public MonsterController GetMonster()
        {
            MonsterController MonsterController = _MonsterControllerPool.GetObject() ?? CreateMonster();

            return MonsterController;
        }
        //------------------------------------------------------------------------------------
        public void PoolMonster(MonsterController MonsterController)
        {
            MonsterController.gameObject.SetActive(false);
            _MonsterControllerPool.PoolObject(MonsterController);
        }
        //------------------------------------------------------------------------------------
        private MonsterController CreateMonster()
        {
            GameObject clone = Instantiate(_MonsterControllerObject.gameObject, transform);

            MonsterController MonsterController = clone.GetComponent<MonsterController>();
            MonsterController.Init();
            MonsterController.gameObject.SetActive(false);

            return MonsterController;
        }
        //------------------------------------------------------------------------------------
    }
}