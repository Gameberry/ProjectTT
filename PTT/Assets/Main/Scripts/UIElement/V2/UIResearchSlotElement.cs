using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class UIResearchSlotElement : MonoBehaviour
    {
        [SerializeField]
        private List<Transform> _lockHideElements = new List<Transform>();

        [SerializeField]
        private UIResearchElement _uIResearchElement;

        [SerializeField]
        private TMP_Text _researchName;

        [SerializeField]
        private TMP_Text _researchTime;

        [SerializeField]
        private Transform _selectEffect;

        [SerializeField]
        private Transform _reserachDoing;

        [Header("------------TicketAccel------------")]
        [SerializeField]
        private CButton _uIResearchTicketAccelBtn;

        [SerializeField]
        private Transform _uIAccelDimmed;

        [Header("------------SlotLockImage------------")]
        [SerializeField]
        private Transform _uISlotLockGroup;


        [Header("------------UnLockShop------------")]
        [SerializeField]
        private UIShopElement _uIShopElement;


        private ResearchData _currentResearchData = null;

        private ObscuredInt _currentSlotIdx = -1;

        private System.Action<int> _callback;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_uIResearchTicketAccelBtn != null)
                _uIResearchTicketAccelBtn.onClick.AddListener(OnClick_AccelBtn);
        }
        //------------------------------------------------------------------------------------
        public void SetSlotIdx(int idx, System.Action<int> callback)
        {
            _currentSlotIdx = idx;
            _callback = callback;
            RefreshSlotData();

            if (idx == 2)
            {
                if (_uIShopElement != null)
                {
                    ShopDataBase shopPackageData = Managers.ShopManager.Instance.GetShopData(Define.Research2SlotOpenCost);
                    _uIShopElement.Init();
                    _uIShopElement.SetShopElement(shopPackageData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshSlotData()
        {
            bool islock = Managers.ResearchManager.Instance.IsLockSlot(_currentSlotIdx);

            for (int i = 0; i < _lockHideElements.Count; ++i)
            {
                _lockHideElements[i].gameObject.SetActive(!islock);
            }

            if (_uISlotLockGroup != null)
                _uISlotLockGroup.gameObject.SetActive(islock);
        }
        //------------------------------------------------------------------------------------
        public void SetResearchData(ResearchData researchData)
        {
            _currentResearchData = researchData;

            if (_currentResearchData == null)
            {
                if (_uIResearchElement != null)
                    _uIResearchElement.gameObject.SetActive(false);

                if (_researchName != null)
                    Managers.LocalStringManager.Instance.SetLocalizeText(_researchName, "-");

                if (_researchTime != null)
                    _researchTime.SetText("-");

                if (_uIAccelDimmed != null)
                    _uIAccelDimmed.gameObject.SetActive(true);

                if (_reserachDoing != null)
                    _reserachDoing.gameObject.SetActive(false);
            }
            else
            {
                if (_uIResearchElement != null)
                { 
                    _uIResearchElement.gameObject.SetActive(true);
                    _uIResearchElement.SetResearchElement(researchData);
                }

                if (_researchName != null)
                    Managers.LocalStringManager.Instance.SetLocalizeText(_researchName, Managers.ResearchManager.Instance.GetResearchName(_currentResearchData));

                if (_uIAccelDimmed != null)
                    _uIAccelDimmed.gameObject.SetActive(false);

                if (_reserachDoing != null)
                    _reserachDoing.gameObject.SetActive(true);

                RefreshResearchData();
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshResearchData()
        {
            if (_currentResearchData == null)
                return;

            PlayerResearchInfo playerResearchInfo = Managers.ResearchManager.Instance.GetPlayerResearchInfo(_currentResearchData);

            if (playerResearchInfo == null)
            {
                if (_researchTime != null)
                    _researchTime.SetText("-");
            }
            else
            {
                double remaintime = playerResearchInfo.CompleteTime -
                    Managers.TimeManager.Instance.Current_TimeStamp;

                if (remaintime < 0)
                    remaintime = 0;

                if (_researchTime != null)
                    _researchTime.SetText(Managers.TimeManager.Instance.GetSecendToDayString_HMS(remaintime.ToInt()));
            }
        }
        //------------------------------------------------------------------------------------
        public void EnableSelectEffect(bool enable)
        {
            if (_selectEffect != null)
                _selectEffect.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_AccelBtn()
        {
            _callback?.Invoke(_currentSlotIdx);
        }
        //------------------------------------------------------------------------------------
    }
}