using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class LobbyPassDialog : IDialog
    {
        [SerializeField]
        private Transform m_uIPassElement_Root;

        [SerializeField]
        private UIPassElement m_uIPassElement;

        [SerializeField]
        private Dictionary<V2Enum_PassType, UIPassElement> m_uIPassDatas = new Dictionary<V2Enum_PassType, UIPassElement>();

        private UIGuideInteractor m_passGuideElementInteractor;

        private V2Enum_PassType _currentPassType = V2Enum_PassType.Max;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            for (int i = V2Enum_PassType.Wave.Enum32ToInt(); i < V2Enum_PassType.Max.Enum32ToInt(); ++i)
            {
                if (i == V2Enum_PassType.CharacterLevel.Enum32ToInt())
                    continue;

                List<PassData> passDatas = Managers.PassManager.Instance.GetPassDatas(i.IntToEnum32<V2Enum_PassType>());

                if (passDatas.Count <= 0)
                    continue;

                GameObject clone = Instantiate(m_uIPassElement.gameObject, m_uIPassElement_Root);
                if (clone == null)
                    continue;

                UIPassElement uIPassElement = clone.GetComponent<UIPassElement>();
                if (uIPassElement == null)
                    continue;

                V2Enum_PassType v2Enum_PassType = i.IntToEnum32<V2Enum_PassType>();

                uIPassElement.Init(OnClick_PassElementBtn);
                uIPassElement.SetPassElement(v2Enum_PassType);

                m_uIPassDatas.Add(i.IntToEnum32<V2Enum_PassType>(), uIPassElement);
            }

            Message.AddListener<GameBerry.Event.RefreshPassListMsg>(RefreshPassList);
            Message.AddListener<GameBerry.Event.SetGuideInteractorDialogMsg>(SetGuideInteractorDialog);
            Message.AddListener<GameBerry.Event.SetPassDialogStateMsg>(SetPassDialogState);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshPassListMsg>(RefreshPassList);
            Message.RemoveListener<GameBerry.Event.SetGuideInteractorDialogMsg>(SetGuideInteractorDialog);
            Message.RemoveListener<GameBerry.Event.SetPassDialogStateMsg>(SetPassDialogState);
        }
        //------------------------------------------------------------------------------------
        private void RefreshPassList(GameBerry.Event.RefreshPassListMsg msg)
        {
            for (int i = 0; i < msg.passDatas.Count; ++i)
            {
                if (msg.passDatas[i] == null)
                    continue;

                if (m_uIPassDatas.ContainsKey(msg.passDatas[i].PassType) == false)
                    continue;

                m_uIPassDatas[msg.passDatas[i].PassType].SetPassElement(msg.passDatas[i].PassType);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.PassFreeRewardClaim)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(m_passGuideElementInteractor);
            }
            if (_currentPassType == V2Enum_PassType.Max)
                OnClick_PassElementBtn(V2Enum_PassType.Wave);
            else
                Managers.PassManager.Instance.ShowPassDialog(_currentPassType);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.isAlive == false)
                return;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.PassFreeRewardClaim)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(1);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetPassDialogState(GameBerry.Event.SetPassDialogStateMsg msg)
        {
            switch (msg.ContentDetailList)
            {
                case ContentDetailList.PassWave:
                    {
                        OnClick_PassElementBtn(V2Enum_PassType.Wave);
                        break;
                    }
                case ContentDetailList.PassCharacterLevel:
                    {
                        OnClick_PassElementBtn(V2Enum_PassType.CharacterLevel);
                        break;
                    }
                case ContentDetailList.PassSkillLevel:
                    {
                        OnClick_PassElementBtn(V2Enum_PassType.SkillLevel);
                        break;
                    }
                case ContentDetailList.PassDescendLevel:
                    {
                        OnClick_PassElementBtn(V2Enum_PassType.DescendLevel);
                        break;
                    }
                case ContentDetailList.PassMonsterKill:
                    {
                        OnClick_PassElementBtn(V2Enum_PassType.MonsterKill);
                        break;
                    }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PassElementBtn(V2Enum_PassType v2Enum_PassType)
        {
            if (_currentPassType == v2Enum_PassType)
                return;

            Managers.PassManager.Instance.ShowPassDialog(v2Enum_PassType);

            _currentPassType = v2Enum_PassType;

            foreach (var pair in m_uIPassDatas)
            {
                pair.Value.SetSelected(pair.Key == _currentPassType);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetGuideInteractorDialog(GameBerry.Event.SetGuideInteractorDialogMsg msg)
        {
            if (msg.guideQuestData == null)
                return;

            if (msg.guideQuestData.QuestType == V2Enum_EventType.PassFreeRewardClaim)
            {
                if (m_uIPassDatas.ContainsKey(V2Enum_PassType.Wave))
                {
                    UIPassElement uIPassElement = m_uIPassDatas[V2Enum_PassType.Wave];
                    m_passGuideElementInteractor = uIPassElement.SetPassGuideBtn();
                    m_passGuideElementInteractor.FocusParent = dialogView.transform;
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}