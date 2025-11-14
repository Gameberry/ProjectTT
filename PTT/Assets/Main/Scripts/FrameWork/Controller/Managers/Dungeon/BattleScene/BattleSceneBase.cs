using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class BattleSceneBase
    {
        public Enum_Dungeon MyEnum_BattleType;

        public bool IsPlay = false;

        public CharacterControllerBase PlayerController;

        //------------------------------------------------------------------------------------
        public virtual void Init()
        { 

        }
        //------------------------------------------------------------------------------------
        public virtual bool IsReadyBattle()
        {
            return true;
        }
        //------------------------------------------------------------------------------------
        
        public void SetBattleScene()
        {
            
            OnSetBattleScene();
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnSetBattleScene()
        { 

        }
        //------------------------------------------------------------------------------------
        public void PlayBattleScene()
        {
            IsPlay = true;
            OnPlayBattleScene();
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnPlayBattleScene()
        {

        }
        //------------------------------------------------------------------------------------
        public void ReleaseBattleScene()
        {
            IsPlay = false;
            OnReleaseBattleScene();
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnReleaseBattleScene()
        {
            
        }
        //------------------------------------------------------------------------------------
        public void Updated()
        {
            
            OnUpdated();
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnUpdated()
        {

        }
        //------------------------------------------------------------------------------------
        public void LateUpdated()
        {
        }
        //------------------------------------------------------------------------------------
        public void DeadPlayer(PlayerController playerController)
        { 

        }
        //------------------------------------------------------------------------------------
        public void DeadMonster(MonsterController monsterController)
        { 

        }
        //------------------------------------------------------------------------------------
    }
}