using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gpm.Ui;


namespace GameBerry.UI
{
    public class UILobbySynergyRuneCombineGroupElement : MonoBehaviour
    {
        [SerializeField]
        private Transform _skillSlotElementRoot;

        [SerializeField]
        private UIARRRSkillSlotElement _uISkillSlotElement;

        [SerializeField]
        private UILobbySynergyRuneElement _resultElement;

        [SerializeField]
        private Transform _percent_Group;

        [SerializeField]
        private TMP_Text _percent;

        [SerializeField]
        private Transform _resultSuccess;

        [SerializeField]
        private Transform _resultFail;

        private Dictionary<ObscuredInt, UIARRRSkillSlotElement> _spawnSkillSlotElement_Dic = new Dictionary<ObscuredInt, UIARRRSkillSlotElement>();

        private ObscuredInt _groupID = -1;

        private List<SynergyRuneData> _materials = new List<SynergyRuneData>();

        private SynergyRuneCombineData _runeCombineData = null;

        private System.Action<SynergyRuneData> _callBack;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            for(int i = 0; i < 3; ++i)
            {
                UIARRRSkillSlotElement element = CreateSlot();
                element.SetSlotID(i);

                SynergyRuneData synergyRuneData = null;

                element.SetSkill(synergyRuneData);
                _spawnSkillSlotElement_Dic.Add(i, element);
            }
        }
        //------------------------------------------------------------------------------------
        private UIARRRSkillSlotElement CreateSlot()
        {
            GameObject clone = Instantiate(_uISkillSlotElement.gameObject, _skillSlotElementRoot);
            UIARRRSkillSlotElement slot = clone.GetComponent<UIARRRSkillSlotElement>();
            slot.Init(OnClick_SlotBtn,
                null, null, null);

            return slot;
        }
        //------------------------------------------------------------------------------------
        public void SetGroup(System.Action<SynergyRuneData> action, int id)
        {
            _callBack = action;
            _groupID = id;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SlotBtn(int slotid)
        {
            //Managers.CharacterSkillSlotManager.Instance.UnEquipSkill(slotid);
            Debug.Log(string.Format("OnClick : {0}", slotid));

            if (_spawnSkillSlotElement_Dic.ContainsKey(slotid) == false)
                return;

            SynergyRuneData synergyRuneData = _spawnSkillSlotElement_Dic[slotid].GetSynergyRuneData();
            if (synergyRuneData == null)
                return;

            _materials.Remove(synergyRuneData);
            Managers.SynergyRuneManager.Instance.RemoveMaterial(synergyRuneData);

            _callBack?.Invoke(synergyRuneData);

            synergyRuneData = null;

            _spawnSkillSlotElement_Dic[slotid].SetSkill(synergyRuneData);

            if (_materials.Count == 0)
            {
                ReleaseMaterial();
            }
        }
        //------------------------------------------------------------------------------------
        public void ReleaseMaterial()
        {
            for (int i = 0; i < _materials.Count; ++i)
            {
                Managers.SynergyRuneManager.Instance.RemoveMaterial(_materials[i]);
            }

            _materials.Clear();

            foreach (var pair in _spawnSkillSlotElement_Dic)
            {
                SynergyRuneData synergyRuneData = null;
                pair.Value.SetSkill(synergyRuneData);
                pair.Value.gameObject.SetActive(true);
            }

            if (_resultElement != null)
                _resultElement.gameObject.SetActive(false);

            if (_resultSuccess != null)
                _resultSuccess.gameObject.SetActive(false);

            if (_resultFail != null)
                _resultFail.gameObject.SetActive(false);

            if (_percent_Group != null)
                _percent_Group.gameObject.SetActive(true);

            if (_percent != null)
                _percent.SetText("-");

            _runeCombineData = null;
        }
        //------------------------------------------------------------------------------------
        public bool AddMaterial(SynergyRuneData synergyRuneData)
        {
            if (_runeCombineData == null)
            {
                ReleaseMaterial();

                SynergyRuneCombineData synergyRuneCombineData = Managers.SynergyRuneManager.Instance.GetSynergyRuneCombineData(synergyRuneData.Grade);
                _runeCombineData = synergyRuneCombineData;

                if (_runeCombineData == null)
                    return false;

                for (int i = _runeCombineData.RequiredCount; i < _spawnSkillSlotElement_Dic.Count; ++i)
                {
                    _spawnSkillSlotElement_Dic[i].gameObject.SetActive(false);
                }

                if (_percent != null)
                    _percent.SetText("{0}%", _runeCombineData.SuccessProb);
            }

            if (_runeCombineData == null)
                return false;

            if (_materials.Count >= _runeCombineData.RequiredCount)
                return false;

            if (_runeCombineData.Grade != synergyRuneData.Grade)
                return false;

            bool returnfunc = true;

            foreach (var pair in _spawnSkillSlotElement_Dic)
            {
                if (pair.Value.GetSynergyRuneData() == null)
                {
                    pair.Value.SetSkill(synergyRuneData);
                    returnfunc = false;
                    break;
                }
            }

            if (returnfunc == true)
                return false;

            _materials.Add(synergyRuneData);
            Managers.SynergyRuneManager.Instance.AddMaterial(synergyRuneData);

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool DoCombine()
        {
            if (_runeCombineData == null)
            {
                ReleaseMaterial();
                return false;
            }

            SynergyRuneData synergyRuneData = Managers.SynergyRuneManager.Instance.DoRuneCombine(_runeCombineData, _materials);

            

            if (synergyRuneData != null)
            {
                if (_resultElement != null)
                { 
                    _resultElement.gameObject.SetActive(true);
                    _resultElement.SetSynergyEffectData(synergyRuneData);
                    
                }

                if (_percent_Group != null)
                    _percent_Group.gameObject.SetActive(false);

                if (synergyRuneData.Grade > _runeCombineData.Grade)
                {
                    if (_resultSuccess != null)
                        _resultSuccess.gameObject.SetActive(true);
                }
                else
                {
                    if (_resultFail != null)
                        _resultFail.gameObject.SetActive(true);
                }

                if (_materials != null)
                    _materials.Clear();

                return true;
            }
            else
                ReleaseMaterial();

            return false;
        }
        //------------------------------------------------------------------------------------
    }
}