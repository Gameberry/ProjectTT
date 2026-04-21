using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBerry.Chart;
using GameBerry.Managers;
using UnityEngine;

namespace GameBerry
{
    public class BattleScene_Lobby : BattleSceneBase
    {
        protected override void OnSetBattleScene()
        {
            ResourceLoader.Instance.Load<GameObject>("BattleScene/PlayerController", o =>
            {
                GameObject clone = Object.Instantiate(o, Managers.BattleSceneManager.Instance.transform) as GameObject;
                if (clone == null)
                    return;

                PlayerController = clone.GetComponent<CharacterControllerBase>();
                if (PlayerController == null)
                    return;

                PlayerController.Init();
                PlayBattleScene();
            });
        }
    }
}