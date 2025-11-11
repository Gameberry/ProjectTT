using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class BattleSceneBase
    {
        public V2Enum_Dungeon MyEnum_ARR_BattleType;

        protected List<CreatureController> _spawnCreature = new List<CreatureController>();
        protected List<CreatureController> _friendCreature = new List<CreatureController>();

        protected ARRRController _myARRRControllers;
        public ARRRController MyARRRControllers { get { return _myARRRControllers; } }

        public Dictionary<V2Enum_Stat, double> _foePassiveBuffs = new Dictionary<V2Enum_Stat, double>();
        public Dictionary<SkillManageInfo, Dictionary<V2Enum_Stat, double>> _foePassiveBuffDatas = new Dictionary<SkillManageInfo, Dictionary<V2Enum_Stat, double>>();

        public Dictionary<V2Enum_Stat, double> _friendPassiveBuffs = new Dictionary<V2Enum_Stat, double>();
        public Dictionary<SkillManageInfo, Dictionary<V2Enum_Stat, double>> _friendPassiveBuffDatas = new Dictionary<SkillManageInfo, Dictionary<V2Enum_Stat, double>>();


        private bool _needRefreshBuff_Foe = false;
        private bool _needRefreshBuff_Friend = false;

        public V2Enum_ARR_BattleSpeed BattleSpeedType { get { return _battleSpeedType; } }
        private V2Enum_ARR_BattleSpeed _battleSpeedType = V2Enum_ARR_BattleSpeed.x1;

        public bool IsPlay { get { return _isPlay; } }
        protected bool _isPlay = false;

        protected int _remainFoeCount = 0;
        public int RemainFoeCount { get { return _remainFoeCount; } }

        protected float _creatureLimitLine = -1;
        public float CreatureLimitLine { get { return _creatureLimitLine; } }

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
        public void ChangeBattleSpeed(V2Enum_ARR_BattleSpeed v2Enum_ARR_BattleSpeed)
        {
            _battleSpeedType = v2Enum_ARR_BattleSpeed;

            if (IsPlay == true)
            {
                Managers.BattleSceneManager.Instance.ChangeTimeScale(_battleSpeedType);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetBattleScene()
        {
            _myARRRControllers = Managers.ARRRManager.Instance.GetCreature();
            _myARRRControllers.gameObject.SetActive(true);
            _myARRRControllers.SetIFFType(IFFType.IFF_Friend);

            ARRRInfo aRRRInfo = new ARRRInfo();

            Managers.ARRRStatManager.Instance.GetPlayerARRRDefaultStat(ref aRRRInfo.DefaultStatValue);

            if (MyEnum_ARR_BattleType != V2Enum_Dungeon.LobbyScene)
            {
                Managers.ARRRSkillManager.Instance.GetARRREquipSkill(ref aRRRInfo.EquipSkillData);

                aRRRInfo.EquipPetInfo = Managers.PetManager.Instance.GetEquipPets();
            }
            else
            { 

            }
            
            
            _myARRRControllers.SetARRRInfo(aRRRInfo);
            _myARRRControllers.ReadyARRR();
            //_myARRRControllers.SetRootTransform(rootTrans);
            //_myARRRControllers.SetRootPos();
            //_myARRRControllers.SetCreatureData(creatureData, level);
            _myARRRControllers.ChangeCharacterLookAtDirection(Enum_ARR_LookDirection.Right);

            _myARRRControllers.SetRootTransform(InGamePositionContainer.Instance.GetArrrStadardPos());
            _myARRRControllers.SetRootPos();

            OnSetBattleScene();
        }
        //------------------------------------------------------------------------------------
        public void RefreshMyARRRControllerStat()
        {
            if (_myARRRControllers != null)
            {
                Managers.ARRRStatManager.Instance.GetPlayerARRRDefaultStat(ref _myARRRControllers.ARRRInfo.DefaultStatValue);

                _myARRRControllers.RefreshARRRStat(_myARRRControllers.ARRRInfo.DefaultStatValue);
            }
        }
        //------------------------------------------------------------------------------------
        public void AddMyARRRBuff(V2Enum_Stat v2Enum_Stat, ObscuredDouble buffValue)
        {
            if (_myARRRControllers != null)
            {
                _myARRRControllers.InCreaseBuffStat(v2Enum_Stat, buffValue);
            }
        }
        //------------------------------------------------------------------------------------
        public void TargetAllDamage(IFFType iFFType, V2SkillAttackData v2SkillAttackData)
        {
            if (iFFType == IFFType.IFF_Friend)
            {
                for (int i = 0; i < _friendCreature.Count; ++i)
                {
                    _friendCreature[i].OnDamage(v2SkillAttackData);
                }
            }
            else
            {
                for (int i = 0; i < _spawnCreature.Count; ++i)
                {
                    if (_spawnCreature[i].IFFType == IFFType.IFF_Friend)
                        continue;

                    _spawnCreature[i].OnDamage(v2SkillAttackData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void TargetAllEffect(IFFType iFFType, V2CCData v2CCData)
        {
            if (iFFType == IFFType.IFF_Friend)
            {
                for (int i = 0; i < _friendCreature.Count; ++i)
                {
                    _friendCreature[i].ApplyCC(v2CCData);
                }
            }
            else
            {
                for (int i = 0; i < _spawnCreature.Count; ++i)
                {
                    if (_spawnCreature[i].IFFType == IFFType.IFF_Friend)
                        continue;

                    _spawnCreature[i].ApplyCC(v2CCData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnSetBattleScene()
        { 

        }
        //------------------------------------------------------------------------------------
        public void PlayBattleScene()
        {
            _isPlay = true;
            Managers.BattleSceneManager.Instance.ChangeTimeScale(_battleSpeedType);
            OnPlayBattleScene();
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnPlayBattleScene()
        {

        }
        //------------------------------------------------------------------------------------
        protected CreatureController SpawnCreature(CreatureData creatureData, 
            int level, Transform rootTrans, IFFType iFFType, Enum_ARR_LookDirection lookDirection
            , Dictionary<V2Enum_Stat, ObscuredDouble> addDefaultStat)
        {
            CreatureController creatureController = Managers.CreatureManager.Instance.GetCreature();
            creatureController.SetIFFType(iFFType);
            creatureController.SetRootTransform(rootTrans);
            creatureController.SetRootPos();
            creatureController.gameObject.SetActive(true);
            creatureController.SetCreatureData(creatureData, level, addDefaultStat);
            creatureController.ChangeCharacterLookAtDirection(lookDirection);

            _spawnCreature.Add(creatureController);

            return creatureController;
        }
        //------------------------------------------------------------------------------------
        protected CreatureController SpawnFoe(CreatureData creatureData,
            Transform rootTrans, Enum_ARR_LookDirection lookDirection
            , List<OperatorOverrideStat> overrideStat)
        {
            CreatureController creatureController = Managers.CreatureManager.Instance.GetCreature();
            creatureController.SetIFFType(IFFType.IFF_Foe);
            creatureController.SetRootTransform(rootTrans);
            creatureController.SetRootPos();
            creatureController.gameObject.SetActive(true);
            creatureController.SetCreatureData(creatureData, 1);
            creatureController.SetOverrideStat(overrideStat);
            creatureController.ChangeCharacterLookAtDirection(lookDirection);

            _spawnCreature.Add(creatureController);

            return creatureController;
        }
        //------------------------------------------------------------------------------------
        public void SetBuff(SkillManageInfo skillManageInfo, IFFType iFFType, bool applyBuff)
        {
            if (skillManageInfo == null)
                return;

            Dictionary<SkillManageInfo, Dictionary<V2Enum_Stat, double>> passiveBuffDatas = iFFType == IFFType.IFF_Friend ? _friendPassiveBuffDatas : _foePassiveBuffDatas;
            Dictionary<V2Enum_Stat, double> applyPassiveValues = iFFType == IFFType.IFF_Friend ? _friendPassiveBuffs : _foePassiveBuffs;

            Dictionary<V2Enum_Stat, double> statValue = null;

            if (passiveBuffDatas.ContainsKey(skillManageInfo) == false)
            {
                statValue = new Dictionary<V2Enum_Stat, double>();
                passiveBuffDatas.Add(skillManageInfo, statValue);
            }
            else
            {
                statValue = passiveBuffDatas[skillManageInfo];

                foreach (var pair in statValue)
                {
                    if (applyPassiveValues.ContainsKey(pair.Key) == false)
                        continue;

                    applyPassiveValues[pair.Key] -= pair.Value;
                }
            }


            if (applyBuff == true)
            {
                int skillLevel = skillManageInfo.GetSkillLevel();

                SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;
                if (skillBaseData.SkillEffect != null && skillBaseData.SkillEffect.Count > 0)
                {
                    for (int effect = 0; effect < skillBaseData.SkillEffect.Count; ++effect)
                    {
                        SkillEffectData skillEffectData = skillBaseData.SkillEffect[effect];

                        V2Enum_Stat v2Enum_Stat = ARRRStatOperator.ConvertCrowdControlTypeToStat(skillEffectData.SkillEffectType);

                        if (v2Enum_Stat != V2Enum_Stat.Max)
                        {
                            if (passiveBuffDatas[skillManageInfo].ContainsKey(v2Enum_Stat) == false)
                                passiveBuffDatas[skillManageInfo].Add(v2Enum_Stat, 0);

                            if (applyPassiveValues.ContainsKey(v2Enum_Stat) == false)
                                applyPassiveValues.Add(v2Enum_Stat, 0);

                            double value = Managers.SkillManager.Instance.GetSkillEffectValue(skillEffectData, skillLevel);

                            passiveBuffDatas[skillManageInfo][v2Enum_Stat] = value;
                            applyPassiveValues[v2Enum_Stat] += value;
                        }
                    }
                }
            }

            if (iFFType == IFFType.IFF_Friend)
                _needRefreshBuff_Friend = true;
            else
                _needRefreshBuff_Foe = true;
        }
        //------------------------------------------------------------------------------------
        public void AddGambleSkill(MainSkillData gambleSkillData, V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType, SkillInfo skillInfo = null)
        {
            if (_myARRRControllers != null)
                _myARRRControllers.AddGambleSkill(gambleSkillData, v2Enum_ARR_SynergyType, skillInfo);
        }
        //------------------------------------------------------------------------------------
        public void AddPet(PetData petData, SkillInfo skillInfo = null)
        {
            if (_myARRRControllers != null)
                _myARRRControllers.AddPet(petData, skillInfo);
        }
        //------------------------------------------------------------------------------------
        public void AddPetAfterSkill(PetData petData, MainSkillData gambleSkillData, SkillInfo skillInfo = null)
        {
            if (_myARRRControllers != null)
                _myARRRControllers.AddPetAfterSkill(petData, gambleSkillData, skillInfo);
        }
        //------------------------------------------------------------------------------------
        public double GetPassiveBuffValue(V2Enum_Stat v2Enum_Stat, IFFType iFFType)
        {
            Dictionary<V2Enum_Stat, double> passiveBuffs = iFFType == IFFType.IFF_Friend ? _friendPassiveBuffs : _foePassiveBuffs;

            if (passiveBuffs.ContainsKey(v2Enum_Stat) == false)
                return 0.0;

            return passiveBuffs[v2Enum_Stat];
        }
        //------------------------------------------------------------------------------------
        public virtual void ReleaseBattleScene()
        {
            _foePassiveBuffs.Clear();
            _foePassiveBuffDatas.Clear();
            _friendPassiveBuffs.Clear();
            _friendPassiveBuffDatas.Clear();

            AllForceReleaseCreature();
            OnReleaseBattleScene();
            _isPlay = false;
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnReleaseBattleScene()
        {
            
        }
        //------------------------------------------------------------------------------------
        protected void ForceReleaseCreature(CreatureController creatureController)
        {
            creatureController.ForceReleaseCreature();
            _spawnCreature.Remove(creatureController);
            if (creatureController.IFFType == IFFType.IFF_Friend)
                _friendCreature.Remove(creatureController);

            Managers.CreatureManager.Instance.PoolCreature(creatureController);
        }
        //------------------------------------------------------------------------------------
        protected void ForceReleaseMyARRRControllers()
        {
            if (_myARRRControllers == null)
                return;

            _myARRRControllers.ForceReleaseCreature();

            Managers.ARRRManager.Instance.PoolCreature(_myARRRControllers);

            _myARRRControllers = null;
        }
        //------------------------------------------------------------------------------------
        protected void AllForceReleaseCreature()
        {
            while (_spawnCreature.Count > 0)
            {
                CreatureController creatureController = _spawnCreature[0];
                ForceReleaseCreature(creatureController);
            }

            _remainFoeCount = 0;

            _friendCreature.Clear();
            _spawnCreature.Clear();

            ForceReleaseMyARRRControllers();
        }
        //------------------------------------------------------------------------------------
        protected void AllForceReleaseFoeCreature()
        {
            int i = 0;
            while (_spawnCreature.Count > i)
            {
                if (_spawnCreature[i].IFFType == IFFType.IFF_Friend)
                {
                    i++;
                    continue;
                }

                ForceReleaseCreature(_spawnCreature[i]);

                
            }

            _remainFoeCount = 0;
        }
        //------------------------------------------------------------------------------------
        public void Updated()
        {
            if (_isPlay == false)
            {
                return;
            }

            //if (_friendCreature.Count > 0)
            //{
            //    float minPos = 0.0f;
            //    float maxPos = 0.0f;

            //    for (int i = 0; i < _friendCreature.Count; ++i)
            //    {
            //        Vector3 pos = _friendCreature[i].transform.position;
            //        if (i == 0)
            //        {
            //            minPos = pos.x;
            //            maxPos = pos.x;

            //            continue;
            //        }

            //        if (minPos > pos.x)
            //        {
            //            minPos = pos.x;
            //        }
            //        else if (maxPos < pos.x)
            //        {
            //            maxPos = pos.x;
            //        }
            //    }

            //    _friendMinPos = minPos;
            //    _friendMaxPos = maxPos;
            //}

            OnUpdated();
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnUpdated()
        {

        }
        //------------------------------------------------------------------------------------
        public void LateUpdated()
        {
            CheckBuffRefresh();
        }
        //------------------------------------------------------------------------------------
        private void CheckBuffRefresh()
        {
            if (_needRefreshBuff_Foe == false
                && _needRefreshBuff_Friend == false)
            {
                return;
            }

            if (_needRefreshBuff_Foe == true
                && _needRefreshBuff_Friend == true)
            {
                for (int i = 0; i < _spawnCreature.Count; ++i)
                {
                    _spawnCreature[i].RefreshBuff();
                }

                if (_myARRRControllers != null)
                    _myARRRControllers.RefreshBuff();
            }
            else if (_needRefreshBuff_Foe == false
                && _needRefreshBuff_Friend == true)
            {
                for (int i = 0; i < _friendCreature.Count; ++i)
                {
                    _friendCreature[i].RefreshBuff();
                }

                if (_myARRRControllers != null)
                    _myARRRControllers.RefreshBuff();
            }
            else if (_needRefreshBuff_Foe == true
                && _needRefreshBuff_Friend == false)
            {
                for (int i = 0; i < _spawnCreature.Count; ++i)
                {
                    if (_spawnCreature[i].IFFType == IFFType.IFF_Foe)
                        _spawnCreature[i].RefreshBuff();
                }
            }

            _needRefreshBuff_Foe = false;
            _needRefreshBuff_Friend = false;
        }
        //------------------------------------------------------------------------------------
        public virtual void CallDeadCreature(CreatureController creatureController)
        {
            if (creatureController.IFFType == IFFType.IFF_Friend)
            { 
                _friendCreature.Remove(creatureController);
            }
            else
                _remainFoeCount--;
        }
        //------------------------------------------------------------------------------------
        public virtual void CallReleaseCreature(CreatureController creatureController)
        {
            _spawnCreature.Remove(creatureController);
            Managers.CreatureManager.Instance.PoolCreature(creatureController);
        }
        //------------------------------------------------------------------------------------
        public virtual void CallDeadARRR(ARRRController creatureController)
        {
        }
        //------------------------------------------------------------------------------------
        public virtual void CallReleaseARRR(ARRRController creatureController)
        {
            if (_myARRRControllers == null)
                return;

            Managers.ARRRManager.Instance.PoolCreature(_myARRRControllers);

            if (_myARRRControllers == creatureController)
            {
                _myARRRControllers = null;
            }
        }
        //------------------------------------------------------------------------------------
    }
}