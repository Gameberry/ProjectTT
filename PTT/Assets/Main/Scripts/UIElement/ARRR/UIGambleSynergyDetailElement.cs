using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UIGambleSynergyDetailElement : MonoBehaviour
    {
        [SerializeField]
        private Image _synergyIcon;

        [SerializeField]
        private TMP_Text _synergyLevel;

        [SerializeField]
        private TMP_Text _synergySkillDesc;

        [SerializeField]
        private List<TMP_Text> _synergyStack;


        [SerializeField]
        private UILobbySynergyEffectElement _synergySkillIcon;

        [SerializeField]
        private UIARRRSkillDescGroup uIARRRSkillDescGroup;


        [SerializeField]
        private List<Transform> _synergyDisableList = new List<Transform>();

        [SerializeField]
        private List<Transform> _synergyEnableList = new List<Transform>();

        [SerializeField]
        private DOTweenAnimation _punchAni;

        [SerializeField]
        private Transform _synergyLock;

        [SerializeField]
        private List<UILobbySynergyUnLockNoticeElement> _UILobbySynergyUnLockNoticeElementList = new List<UILobbySynergyUnLockNoticeElement>();


        //------------------------------------------------------------------------------------
        public void SetGambleSynergyEffectData(SynergyEffectData gambleSynergyEffectData)
        {
            Enum_SynergyType SynergyType = gambleSynergyEffectData.SynergyType;

            int currentStack = Managers.SynergyManager.Instance.GetSynergyStack(SynergyType);
            int targetStack = gambleSynergyEffectData.SynergyCount;

            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(SynergyType);
            if (gambleCardSprite != null)
            {
                if (_synergyIcon != null)
                { 
                    _synergyIcon.sprite = gambleCardSprite.SynergyIcon;
                    _synergyIcon.color = currentStack < targetStack ? Color.gray : Color.white;
                }
            }

            if (_synergyLevel != null)
            { 
                _synergyLevel.SetText("Lv.{0}", gambleSynergyEffectData.SynergyTier);
                _synergyLevel.color = currentStack < targetStack ? Color.gray : Color.white;
            }

            if (_synergySkillDesc != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_synergySkillDesc, string.Format("synergydesc/{0}", gambleSynergyEffectData.Index));

            for (int i = 0; i < _synergyStack.Count; ++i)
            {
                if (_synergyStack[i] != null)
                    _synergyStack[i].SetText("{0}", targetStack);
            }

            for (int i = 0; i < _synergyDisableList.Count; ++i)
            {
                if (_synergyDisableList[i] != null)
                    _synergyDisableList[i].gameObject.SetActive(currentStack < targetStack);
            }

            for (int i = 0; i < _synergyEnableList.Count; ++i)
            {
                if (_synergyEnableList[i] != null)
                    _synergyEnableList[i].gameObject.SetActive(currentStack >= targetStack);
            }

            bool isLock = gambleSynergyEffectData.SynergyTier > Managers.SynergyManager.Instance.GetInGameSynergyUnlockTier(SynergyType);

            SetUnLockDesc(gambleSynergyEffectData);

            if (_synergySkillIcon != null)
                _synergySkillIcon.SetSynergyEffectData(gambleSynergyEffectData);

            SkillInfo skillInfo = Managers.SynergyManager.Instance.GetSynergyEffectSkillInfo(gambleSynergyEffectData);

            if (uIARRRSkillDescGroup != null)
                uIARRRSkillDescGroup.SetSkillData(gambleSynergyEffectData.SynergySkillData, skillInfo.Level);
        }
        //------------------------------------------------------------------------------------
        private void SetUnLockDesc(SynergyEffectData synergyEffectData)
        {
            bool locksynergy = Managers.SynergyManager.Instance.IsLockSynergy(synergyEffectData);

            if (_synergyLock != null)
                _synergyLock.gameObject.SetActive(locksynergy);

            if (locksynergy == true)
            {
                SynergyConditionData synergyConditionData = synergyEffectData.SynergyConditionData;
                if (synergyConditionData == null)
                    return;

                int index = 0;

                if (index >= _UILobbySynergyUnLockNoticeElementList.Count)
                    return;

                if (Managers.SynergyManager.Instance.IsOverLevel(synergyEffectData.SynergyType, 1, synergyConditionData.RequiredTier1Reinforce) == false)
                {
                    SynergyEffectData target = Managers.SynergyManager.Instance.GetEquipSynergyEffect(synergyEffectData.SynergyType, 1);

                    UILobbySynergyUnLockNoticeElement uILobbySynergyUnLockNoticeElement = _UILobbySynergyUnLockNoticeElementList[index];

                    uILobbySynergyUnLockNoticeElement.SetElement(target, synergyConditionData.RequiredTier1Reinforce);
                    uILobbySynergyUnLockNoticeElement.gameObject.SetActive(true);
                    index++;
                }

                if (index >= _UILobbySynergyUnLockNoticeElementList.Count)
                    return;

                if (Managers.SynergyManager.Instance.IsOverLevel(synergyEffectData.SynergyType, 2, synergyConditionData.RequiredTier2Reinforce) == false)
                {
                    SynergyEffectData target = Managers.SynergyManager.Instance.GetEquipSynergyEffect(synergyEffectData.SynergyType, 2);

                    UILobbySynergyUnLockNoticeElement uILobbySynergyUnLockNoticeElement = _UILobbySynergyUnLockNoticeElementList[index];

                    uILobbySynergyUnLockNoticeElement.SetElement(target, synergyConditionData.RequiredTier2Reinforce);
                    uILobbySynergyUnLockNoticeElement.gameObject.SetActive(true);
                    index++;
                }

                if (index >= _UILobbySynergyUnLockNoticeElementList.Count)
                    return;

                if (Managers.SynergyManager.Instance.IsOverLevel(synergyEffectData.SynergyType, 3, synergyConditionData.RequiredTier3Reinforce) == false)
                {
                    SynergyEffectData target = Managers.SynergyManager.Instance.GetEquipSynergyEffect(synergyEffectData.SynergyType, 3);

                    UILobbySynergyUnLockNoticeElement uILobbySynergyUnLockNoticeElement = _UILobbySynergyUnLockNoticeElementList[index];

                    uILobbySynergyUnLockNoticeElement.SetElement(target, synergyConditionData.RequiredTier3Reinforce);
                    uILobbySynergyUnLockNoticeElement.gameObject.SetActive(true);
                    index++;
                }

                if (index >= _UILobbySynergyUnLockNoticeElementList.Count)
                    return;

                if (Managers.SynergyManager.Instance.IsOverLevel(synergyEffectData.SynergyType, 4, synergyConditionData.RequiredTier4Reinforce) == false)
                {
                    SynergyEffectData target = Managers.SynergyManager.Instance.GetEquipSynergyEffect(synergyEffectData.SynergyType, 4);

                    UILobbySynergyUnLockNoticeElement uILobbySynergyUnLockNoticeElement = _UILobbySynergyUnLockNoticeElementList[index];

                    uILobbySynergyUnLockNoticeElement.SetElement(target, synergyConditionData.RequiredTier4Reinforce);
                    uILobbySynergyUnLockNoticeElement.gameObject.SetActive(true);
                    index++;
                }

                if (index >= _UILobbySynergyUnLockNoticeElementList.Count)
                    return;

                for (int i = index; i < _UILobbySynergyUnLockNoticeElementList.Count; ++i)
                {
                    _UILobbySynergyUnLockNoticeElementList[i].gameObject.SetActive(false);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayPunchAni()
        {
            if (_punchAni != null)
            {
                _punchAni.DORewind();
                _punchAni.DORestart();
            }
        }
        //------------------------------------------------------------------------------------
    }
}