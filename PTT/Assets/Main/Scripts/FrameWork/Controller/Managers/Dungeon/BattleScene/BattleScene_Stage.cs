using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBerry.Common;

namespace GameBerry
{
    public class BattleScene_Stage : BattleSceneBase
    {
        //------------------------------------------------------------------------------------
        protected override void OnSetBattleScene()
        {
            ResourceLoader.Instance.Load<GameObject>("BattleScene/PlayerController", o =>
            {
                GameObject clone = Object.Instantiate(o, Managers.BattleSceneManager.Instance.transform) as GameObject;
                if (clone != null)
                {
                    PlayerController = clone.GetComponent<CharacterControllerBase>();
                    PlayerController.Init();
                }

            });
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlayBattleScene()
        {

        }
        //------------------------------------------------------------------------------------
        private void SpawnMonster()
        {
            //StaticResource.Instance.GetBattleModeStaticData()
            //GameObject clone = Object.Instantiate(o, Managers.BattleSceneManager.Instance.transform) as GameObject;
            //if (clone != null)
            //{
            //    PlayerController = clone.GetComponent<CharacterControllerBase>();
            //    PlayerController.Init();
            //}
        }
        //------------------------------------------------------------------------------------
        protected override void OnReleaseBattleScene()
        {

        }
        //------------------------------------------------------------------------------------
    }
}