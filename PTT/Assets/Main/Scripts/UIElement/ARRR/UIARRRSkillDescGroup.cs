using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIARRRSkillDescGroup : MonoBehaviour
    {
        [SerializeField]
        private UISkillIconElement _uISkillDetailElement;

        [SerializeField]
        private TMP_Text _uISkillDetail_Name;

        [SerializeField]
        private TMP_Text _uISkillDetail_CoolTime;

        [SerializeField]
        private TMP_Text _uISkillDetail_Desc;

        [SerializeField]
        private Transform _uISkillDetail_EffectRoot;

        [SerializeField]
        private UIARRRSkillEffectDescElement _uIARRRSkillEffectDescElement;

        [SerializeField]
        private TMP_Text _level;

        private LinkedList<UIARRRSkillEffectDescElement> _uIARRRSkillEffectDescElement_List = new LinkedList<UIARRRSkillEffectDescElement>();

        private Queue<UIARRRSkillEffectDescElement> _uIARRRSkillEffectDescElementPool = new Queue<UIARRRSkillEffectDescElement>();

        public void SetSkillData(MainSkillData gambleSkillData, int level = 0)
        {
            if (gambleSkillData == null)
                return;

            SkillBaseData skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(gambleSkillData.MainSkillTypeParam1);

            if (skillBaseData == null)
                return;

            if (_uISkillDetailElement != null)
            { 
                _uISkillDetailElement.SetSkillElement(gambleSkillData);
                _uISkillDetailElement.VisibleGrade(true);
            }

            if (_level != null)
                _level.SetText("Lv.{0}", level);

            var node = _uIARRRSkillEffectDescElement_List.First;

            while (node != null)
            {
                _uIARRRSkillEffectDescElementPool.Enqueue(node.Value);
                node.Value.gameObject.SetActive(false);
                node = node.Next;
            }

            _uIARRRSkillEffectDescElement_List.Clear();

            if (_uISkillDetail_Name != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_uISkillDetail_Name, gambleSkillData.NameLocalKey);

            if (_uISkillDetail_CoolTime != null)
            {
                if (skillBaseData.CoolTimeType == Enum_CoolTimeType.Default
                    || skillBaseData.CoolTimeType == Enum_CoolTimeType.GamebleCoolTime)
                {
                    if (skillBaseData.CoolTimeValue == -1 || skillBaseData.CoolTimeValue == 0)
                        _uISkillDetail_CoolTime.SetText("-");
                    else if (skillBaseData.CoolTimeValue >= 9999)
                        _uISkillDetail_CoolTime.SetText("-");
                    else
                        _uISkillDetail_CoolTime.SetText("{0}s", skillBaseData.CoolTimeValue);
                }
                else
                    _uISkillDetail_CoolTime.SetText("-");
            }

            if (_uISkillDetail_Desc != null)
            {
                double factor = 0;
                double addfactor = 0;

                SkillDamageData skillDamageData = skillBaseData.SkillDamageIndex;
                if (skillDamageData != null)
                {
                    addfactor = (skillDamageData.DamageFactorPerLevel * level);

                    factor = skillDamageData.DamageFactorBase + addfactor;
                }

                addfactor *= Define.PerStatPrintRecoverValueTemp;
                factor *= Define.PerStatPrintRecoverValueTemp;

                string desc = string.Format(Managers.LocalStringManager.Instance.GetLocalString(gambleSkillData.DescLocalKey), factor, addfactor);

                _uISkillDetail_Desc.SetText(desc);
            }

            if (skillBaseData.SkillEffect != null)
            {
                if (skillBaseData.SkillEffect.Count <= 0)
                {
                    if (_uISkillDetail_EffectRoot != null)
                        _uISkillDetail_EffectRoot.gameObject.SetActive(false);
                }
                else
                {
                    if (_uISkillDetail_EffectRoot != null)
                        _uISkillDetail_EffectRoot.gameObject.SetActive(true);

                    for (int i = 0; i < skillBaseData.SkillEffect.Count; ++i)
                    {
                        SkillEffectData skillEffectData = skillBaseData.SkillEffect[i];
                        AddEffectDesc(skillEffectData, level);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetSkillData(DescendData gambleSkillData, int level = 0)
        {
            if (gambleSkillData == null)
                return;

            if (_uISkillDetailElement != null)
            {
                _uISkillDetailElement.SetSkillElement(gambleSkillData);
                _uISkillDetailElement.VisibleGrade(true);
            }

            if (_uISkillDetail_Name != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_uISkillDetail_Name, gambleSkillData.NameLocalKey);


            if (_level != null)
                _level.SetText("Lv.{0}", level);

            var node = _uIARRRSkillEffectDescElement_List.First;

            while (node != null)
            {
                _uIARRRSkillEffectDescElementPool.Enqueue(node.Value);
                node.Value.gameObject.SetActive(false);
                node = node.Next;
            }

            _uIARRRSkillEffectDescElement_List.Clear();


            SkillBaseData skillBaseData = null;

            if (gambleSkillData.DescendType == Enum_DescendType.DescendSkill)
            {
                if (gambleSkillData.SynergySkillData != null)
                    skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(gambleSkillData.SynergySkillData.MainSkillTypeParam1);
            }
            else
            {
                if (gambleSkillData.PetData != null)
                {
                    if (gambleSkillData.PetData.PassiveDatas.Count > 0)
                        skillBaseData = gambleSkillData.PetData.PassiveDatas[0];
                }
            }

            if (skillBaseData == null)
                return;

            if (_uISkillDetail_Desc != null)
            {
                double factor = 0;
                double addfactor = 0;

                SkillDamageData skillDamageData = skillBaseData.SkillDamageIndex;
                if (skillDamageData != null)
                {
                    addfactor = (skillDamageData.DamageFactorPerLevel * level);

                    factor = skillDamageData.DamageFactorBase + addfactor;
                }

                addfactor *= Define.PerStatPrintRecoverValueTemp;
                factor *= Define.PerStatPrintRecoverValueTemp;

                string desc = string.Format(Managers.LocalStringManager.Instance.GetLocalString(gambleSkillData.DescLocalKey), factor, addfactor);

                _uISkillDetail_Desc.SetText(desc);
            }

            if (_uISkillDetail_CoolTime != null)
            {
                if (skillBaseData.CoolTimeType == Enum_CoolTimeType.Default
                    || skillBaseData.CoolTimeType == Enum_CoolTimeType.GamebleCoolTime)
                {
                    if (skillBaseData.CoolTimeValue == -1 || skillBaseData.CoolTimeValue == 0)
                        _uISkillDetail_CoolTime.SetText("-");
                    else if (skillBaseData.CoolTimeValue >= 9999)
                        _uISkillDetail_CoolTime.SetText("-");
                    else
                        _uISkillDetail_CoolTime.SetText("{0}s", skillBaseData.CoolTimeValue);
                }
                else
                    _uISkillDetail_CoolTime.SetText("-");
            }

            if (skillBaseData.SkillEffect != null)
            {
                if (skillBaseData.SkillEffect.Count <= 0)
                {
                    if (_uISkillDetail_EffectRoot != null)
                        _uISkillDetail_EffectRoot.gameObject.SetActive(false);
                }
                else
                {
                    if (_uISkillDetail_EffectRoot != null)
                        _uISkillDetail_EffectRoot.gameObject.SetActive(true);

                    for (int i = 0; i < skillBaseData.SkillEffect.Count; ++i)
                    {
                        SkillEffectData skillEffectData = skillBaseData.SkillEffect[i];
                        AddEffectDesc(skillEffectData, level);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetSkillData(RelicData gambleSkillData, int level = 0)
        {
            if (gambleSkillData == null)
                return;

            if (_uISkillDetailElement != null)
            {
                _uISkillDetailElement.SetSkillElement(gambleSkillData);
                _uISkillDetailElement.VisibleGrade(true);
            }

            if (_uISkillDetail_Name != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_uISkillDetail_Name, gambleSkillData.NameLocalKey);




            var node = _uIARRRSkillEffectDescElement_List.First;

            while (node != null)
            {
                _uIARRRSkillEffectDescElementPool.Enqueue(node.Value);
                node.Value.gameObject.SetActive(false);
                node = node.Next;
            }

            _uIARRRSkillEffectDescElement_List.Clear();

            if (gambleSkillData.SynergySkillData == null)
                return;

            SkillBaseData skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(gambleSkillData.SynergySkillData.MainSkillTypeParam1);

            if (skillBaseData == null)
                return;

            if (_uISkillDetail_CoolTime != null)
            {
                if (skillBaseData.CoolTimeType == Enum_CoolTimeType.Default
                    || skillBaseData.CoolTimeType == Enum_CoolTimeType.GamebleCoolTime)
                {
                    if (skillBaseData.CoolTimeValue == -1 || skillBaseData.CoolTimeValue == 0)
                        _uISkillDetail_CoolTime.SetText("-");
                    else if (skillBaseData.CoolTimeValue >= 9999)
                        _uISkillDetail_CoolTime.SetText("-");
                    else
                        _uISkillDetail_CoolTime.SetText("{0}s", skillBaseData.CoolTimeValue);
                }
                else
                    _uISkillDetail_CoolTime.SetText("-");
            }

            if (_uISkillDetail_Desc != null)
            {
                double factor = 0;
                double addfactor = 0;

                SkillDamageData skillDamageData = skillBaseData.SkillDamageIndex;
                if (skillDamageData != null)
                {
                    addfactor = (skillDamageData.DamageFactorPerLevel * level);

                    factor = skillDamageData.DamageFactorBase + addfactor;
                }

                addfactor *= Define.PerStatPrintRecoverValueTemp;
                factor *= Define.PerStatPrintRecoverValueTemp;

                string desc = string.Format(Managers.LocalStringManager.Instance.GetLocalString(gambleSkillData.DescLocalKey), factor, addfactor);

                _uISkillDetail_Desc.SetText(desc);
            }

            if (skillBaseData.SkillEffect != null)
            {
                if (skillBaseData.SkillEffect.Count <= 0)
                {
                    if (_uISkillDetail_EffectRoot != null)
                        _uISkillDetail_EffectRoot.gameObject.SetActive(false);
                }
                else
                {
                    if (_uISkillDetail_EffectRoot != null)
                        _uISkillDetail_EffectRoot.gameObject.SetActive(true);

                    for (int i = 0; i < skillBaseData.SkillEffect.Count; ++i)
                    {
                        SkillEffectData skillEffectData = skillBaseData.SkillEffect[i];
                        AddEffectDesc(skillEffectData, level);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetJobStat(int tier, int level = 0)
        {
            if (_level != null)
                _level.SetText("Lv.{0}", level);


            if (_uISkillDetail_Desc != null)
            {
                CharacterBaseStatData statdata = null;

                JobLevelUpStatData jobLevelUpStatData = Managers.JobManager.Instance.GetJobLevelUpStatData(tier);
                if (jobLevelUpStatData == null)
                    return;

                statdata = Managers.ARRRStatManager.Instance.GetCharacterBaseStatData(jobLevelUpStatData.LevelUpStatType);

                string statname = Managers.LocalStringManager.Instance.GetLocalString(statdata.NameLocalStringKey);
                double value = Managers.JobManager.Instance.GetStatValue(tier, level);

                if (statdata.VisibleType == V2Enum_PrintType.Percent)
                {
                    _uISkillDetail_Desc.text = string.Format("{0} +{1:0.0}%", statname, value * Define.PerStatPrintRecoverValueTemp);
                }
                else
                {
                    _uISkillDetail_Desc.text = string.Format("{0} +{1}", statname, System.Math.Round(value).ToString("N0"));
                }
            }
        }
        //------------------------------------------------------------------------------------
        private UIARRRSkillEffectDescElement GetUIARRRSkillCoolTimeElement()
        {
            if (_uIARRRSkillEffectDescElementPool.Count > 0)
                return _uIARRRSkillEffectDescElementPool.Dequeue();

            GameObject clone = Instantiate(_uIARRRSkillEffectDescElement.gameObject, _uISkillDetail_EffectRoot);

            UIARRRSkillEffectDescElement uIARRRSkillCoolTimeElement = clone.GetComponent<UIARRRSkillEffectDescElement>();

            return uIARRRSkillCoolTimeElement;
        }
        //------------------------------------------------------------------------------------
        private void AddEffectDesc(SkillEffectData skillEffectData, int level = 0)
        {
            UIARRRSkillEffectDescElement uIARRRSkillCoolTimeElement = GetUIARRRSkillCoolTimeElement();
            if (uIARRRSkillCoolTimeElement == null)
                return;

            _uIARRRSkillEffectDescElement_List.AddLast(uIARRRSkillCoolTimeElement);

            uIARRRSkillCoolTimeElement.gameObject.SetActive(true);
            uIARRRSkillCoolTimeElement.SetEffectDesc(skillEffectData, level);
        }
        //------------------------------------------------------------------------------------
    }
}