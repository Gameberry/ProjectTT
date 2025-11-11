using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Managers;
using TMPro;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
    public class AdBuffDialog : IDialog
    {
        [SerializeField]
        private TMP_Text _initTime;

        [SerializeField]
        private UIAdBuffPopupElement m_uIAdBuffPopupElement;
        private Dictionary<int, UIAdBuffPopupElement> m_uIAdBuffPopupElement_Dic = new Dictionary<int, UIAdBuffPopupElement>();

        [SerializeField]
        private Transform m_uIAdBuffPopupElement_Root;

        [SerializeField]
        private Transform _remainBuffCountGroup;

        [SerializeField]
        private TMP_Text _remainBuffCount;

        [SerializeField]
        private Transform _adRemoveHooking;

        private AdBuffActiveData _choiceActiveBuff = null;


        [SerializeField]
        private List<ContentSizeFitter> customRefreshSizeFilter = new List<ContentSizeFitter>();

        [SerializeField]
        private List<RectTransform> customRefresh;


        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            Message.AddListener<GameBerry.Event.RefreshAdBuffStateMsg>(RefreshAdBuffState);

            Managers.TimeManager.Instance.RemainInitDailyContent_Text += SetInitInterval;

            foreach (KeyValuePair<int, AdBuffActiveData> pair in AdBuffManager.Instance.GetAllBuffActivaData())
            {

                if (m_uIAdBuffPopupElement != null)
                {
                    /////////////////// BuffPopupElement
                    GameObject clone = Instantiate(m_uIAdBuffPopupElement.gameObject, m_uIAdBuffPopupElement_Root.transform);
                    if (clone != null)
                    {
                        UIAdBuffPopupElement buffbgicon = clone.GetComponent<UIAdBuffPopupElement>();
                        if (buffbgicon != null)
                        {
                            buffbgicon.InitBuffElement(pair.Value, OnClick_BuffActiveBtn);
                            m_uIAdBuffPopupElement_Dic.Add(pair.Value.Index, buffbgicon);
                        }
                    }
                    /////////////////// BuffPopupElement
                }
            }


            VipPackageManager.Instance.changeAdBuffCountInCrease += () =>
            {
                RefreshAdBuffState(null);
            };

            if (_remainBuffCountGroup != null)
                _remainBuffCountGroup.gameObject.SetActive(Define.IsAdBuffAlways == false);

            if (_adRemoveHooking != null)
                _adRemoveHooking.gameObject.SetActive(Define.IsAdBuffAlways == false);

            VipPackageManager.Instance.changeAdBuffAlways += () =>
            {
                if (_remainBuffCountGroup != null)
                    _remainBuffCountGroup.gameObject.SetActive(Define.IsAdBuffAlways == false);

                if (_adRemoveHooking != null)
                    _adRemoveHooking.gameObject.SetActive(Define.IsAdBuffAlways == false);

                RefreshAdBuffState(null);
            };
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshAdBuffStateMsg>(RefreshAdBuffState);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            RefreshAdBuffState(null);

            RefreshSizeFilter().Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask RefreshSizeFilter()
        {
            await UniTask.NextFrame();
            for (int i = 0; i < customRefreshSizeFilter.Count; ++i)
            {
                customRefreshSizeFilter[i].SetLayoutVertical();
            }

            if (customRefresh != null)
            {
                for (int i = 0; i < customRefresh.Count; ++i)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(customRefresh[i]);
                }
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (GuideInteractorManager.isAlive == false)
                return;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.BuffAddView)
            {
                Managers.GuideInteractorManager.Instance.SetPrevGuideInteractor();
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshAdBuffState(GameBerry.Event.RefreshAdBuffStateMsg msg)
        {
            if (_remainBuffCount != null)
                _remainBuffCount.SetText("({0}/{1})", Managers.AdBuffManager.Instance.GetRemainBuffCount()
                    , Managers.AdBuffManager.Instance.GetTotalBuffCount());

            foreach (var pair in m_uIAdBuffPopupElement_Dic)
            {
                pair.Value.SetRefreshBuffState();
            }
        }
        //------------------------------------------------------------------------------------
        private void SetInitInterval(string remaintime)
        {
            if (_initTime != null)
                _initTime.SetText(remaintime);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_BuffActiveBtn(AdBuffActiveData adBuffActiveData)
        {
            if (Managers.AdBuffManager.Instance.GetRemainBuffCount() <= 0)
                return;

            AdBuffActiveData currActive = Managers.AdBuffManager.Instance.GetCurrentActiveBuffData();

            if (currActive == null)
            {
                Managers.AdBuffManager.Instance.OnActiveAdBuff(adBuffActiveData);
            }
            else
            {
                if (currActive == adBuffActiveData)
                    return;

                _choiceActiveBuff = adBuffActiveData;

                Contents.GlobalContent.ShowPopup_OkCancel(
                Managers.LocalStringManager.Instance.GetLocalString("menu/adbuff"),
                Managers.LocalStringManager.Instance.GetLocalString("adbuff/removebuff"),
                () =>
                {
                    Managers.AdBuffManager.Instance.OnActiveAdBuff(_choiceActiveBuff);
                },
                null);
            }
        }
        //------------------------------------------------------------------------------------
    }
}