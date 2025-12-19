using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Managers;
using Spine;
using Spine.Unity;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class ComboController
    {
        private ComboDataAsset _comboDataAsset;
        private CharacterControllerBase _owner;

        private Event.RefreshComboUIMsg _showComboUIMsg = null;

        List<Enum_ConditionType> _applyBuffs = new List<Enum_ConditionType>();

        public int _comboCount = 0;
        private float _releaseComboTime = 0.0f;

        //------------------------------------------------------------------------------------
        public void Init(CharacterControllerBase characterControllerBase)
        {
            Managers.UnityUpdateManager.Instance.LateUpdateFunc += LateUpdated;

            _owner = characterControllerBase;
            _comboDataAsset = StaticResource.Instance.GetComboData();
        }
        //------------------------------------------------------------------------------------
        public void Release()
        {
            Managers.UnityUpdateManager.Instance.LateUpdateFunc -= LateUpdated;
        }
        //------------------------------------------------------------------------------------
        public void SetVisibleComboUI()
        {
            _showComboUIMsg = new Event.RefreshComboUIMsg();
        }
        //------------------------------------------------------------------------------------
        public void AddCombo()
        {
            if (_comboCount < 0)
                _comboCount = 0;

            _releaseComboTime = Time.time + StaticResource.Instance.GetBattleModeStaticData().ComboReleaseTime;

            _comboCount++;

            SendRefreshUI();

            if (_owner == null)
                return;

            ComboData comboData = _comboDataAsset.GetData(_comboCount);
            if (comboData != null)
            {
                ConditionData conditionData = StaticResource.Instance.GetConditionData().GetData(comboData.ConditionIndex);
                if (conditionData != null)
                {
                    _applyBuffs.Add(conditionData.Type);
                    _owner.PlayCharacterCondition(conditionData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void ReleaseCombo()
        {
            _comboCount = 0;

            SendRefreshUI();


            if (_owner == null)
            {
                _applyBuffs.Clear();
                return;
            }

            for (int i = 0; i < _applyBuffs.Count; ++i)
            {
                _owner.RemoveConditionsByType(_applyBuffs[i]);
            }

            _applyBuffs.Clear();
        }
        //------------------------------------------------------------------------------------
        private void SendRefreshUI()
        {
            if (_showComboUIMsg == null)
                return;

            _showComboUIMsg.Combo = _comboCount;
            Message.Send(_showComboUIMsg);
        }
        //------------------------------------------------------------------------------------
        private void LateUpdated()
        {
            if (_comboCount <= 0)
                return;

            if (_releaseComboTime < Time.time)
                ReleaseCombo();
            else
                Debug.Log($"comboTimer : {_releaseComboTime - Time.time}");
        }
        //------------------------------------------------------------------------------------
    }
}