using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class LobbyResearchUseAccelDialog : IDialog
    {
        [SerializeField]
        private LobbyResearchDialog _guideCallBack;

        [Header("------------AdAccel------------")]
        [SerializeField]
        private TMP_Text m_remainAd;

        [SerializeField]
        private CButton m_chargeAd;

        [SerializeField]
        private Image m_cannotAd;

        [SerializeField]
        private Image m_adIcon;

        [Header("------------TicketAccel------------")]
        [SerializeField]
        private TMP_Text _useTicketCount;

        [SerializeField]
        private CButton _useTicketBtn;

        [SerializeField]
        private CButton _m1Btn;

        [SerializeField]
        private CButton _p1Btn;

        [SerializeField]
        private CButton _m10Btn;

        [SerializeField]
        private CButton _p10Btn;

        [SerializeField]
        private CButton _minBtn;

        [SerializeField]
        private CButton _maxBtn;


        [Header("------------RemainTime------------")]
        [SerializeField]
        private TMP_Text _remainTime;

        [SerializeField]
        private TMP_Text _accelTIme;



        private ObscuredInt _ticketCount = 0;
        private ObscuredInt _currentSlotIdx = -1;

        protected override void OnLoad()
        {
            if (m_chargeAd != null)
                m_chargeAd.onClick.AddListener(OnClick_AdAccelBtn);

            if (_useTicketBtn != null)
                _useTicketBtn.onClick.AddListener(OnClick_TicketAccelBtn);

            if (_m1Btn != null)
                _m1Btn.onClick.AddListener(OnClick_Minus1Btn);

            if (_p1Btn != null)
                _p1Btn.onClick.AddListener(OnClick_Plus1Btn);

            if (_m10Btn != null)
                _m10Btn.onClick.AddListener(OnClick_Minus10Btn);

            if (_p10Btn != null)
                _p10Btn.onClick.AddListener(OnClick_Plus10Btn);

            if (_minBtn != null)
                _minBtn.onClick.AddListener(OnClick_MinBtn);

            if (_maxBtn != null)
                _maxBtn.onClick.AddListener(OnClick_MaxBtn);

            Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_HalfSec += RefreshRemainTime;
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.ResearchTutorial)
            {
                ignoreExit = true;
            }

            SetAdBtn();

            _ticketCount = 0;
            RefreshRemainTime();

            

            OnClick_MaxBtn();
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {

            if (Managers.RedDotManager.isAlive == false)
                return;
        }
        //------------------------------------------------------------------------------------
        private void SetAdBtn()
        {
            int remainadcount = Managers.ResearchManager.Instance.GetRemainAdResearchAccelCount(_currentSlotIdx);

            if (m_remainAd != null)
                m_remainAd.SetText("({0}/{1})", remainadcount, Define.ResearchADTimeCount);

            if (m_chargeAd != null)
                m_chargeAd.SetInteractable(remainadcount > 0);

            if (m_cannotAd != null)
                m_cannotAd.gameObject.SetActive(remainadcount <= 0);

            if (m_adIcon != null)
                m_adIcon.gameObject.SetActive(remainadcount > 0);
        }
        //------------------------------------------------------------------------------------
        public void RefreshRemainTime()
        {
            if (_isEnter == false)
                return;

            if (_currentSlotIdx == -1)
                return;

            double remaintime = GetRemainResearchTime();

            if (remaintime <= 0)
            {
                if (_isEnter == true)
                {
                    ElementExit();
                    return;
                }
            }

            RefreshChoiceTicketCount();

            if (_remainTime != null)
                _remainTime.SetText(Managers.TimeManager.Instance.GetSecendToDayString_HMS(remaintime.ToInt()));

            int overticket = CheckOverTicketCount();
            if(overticket > 0)
            {
                _ticketCount -= overticket;
                RefreshChoiceTicketCount();
            }

            if (_useTicketBtn != null)
                _useTicketBtn.SetInteractable(_ticketCount > 0);

            double accelTime = GetAccelTime();

            if (_accelTIme != null)
                _accelTIme.SetText(Managers.TimeManager.Instance.GetSecendToDayString_HMS(accelTime.ToInt()));
        }
        //------------------------------------------------------------------------------------
        private int CheckOverTicketCount()
        {
            double remaintime = GetRemainResearchTime();

            double accelTime = GetAccelTime();

            if (remaintime >= accelTime)
                return 0;

            double gab = accelTime - remaintime;

            return Math.Truncate(gab / Define.ResearchAccelTime).ToInt();
        }
        //------------------------------------------------------------------------------------
        private double GetRemainResearchTime()
        {
            if (_currentSlotIdx == -1)
                return 0;

            int ticketcount = Managers.GoodsManager.Instance.GetGoodsAmount(Define.ResearchAccelTicketIndex).ToInt();

            if (_ticketCount > ticketcount)
                _ticketCount = ticketcount;

            ResearchData _currentResearchData = Managers.ResearchManager.Instance.GetSlotResearchData(_currentSlotIdx);
            PlayerResearchInfo playerResearchInfo = Managers.ResearchManager.Instance.GetPlayerResearchInfo(_currentResearchData);
            if (playerResearchInfo == null)
                return 0;

            double remaintime = playerResearchInfo.CompleteTime -
Managers.TimeManager.Instance.Current_TimeStamp;

            if (remaintime < 0)
                remaintime = 0;

            if (Managers.GuideInteractorManager.Instance.PlayResearchTutorial == true)
            {
                remaintime = Managers.ResearchManager.Instance.GetResearchTime(_currentResearchData);
            }

            return remaintime;
        }

        //------------------------------------------------------------------------------------
        private double GetAccelTime()
        { 
            return Define.ResearchAccelTime * _ticketCount;
        }
        //------------------------------------------------------------------------------------
        private void RefreshChoiceTicketCount()
        {
            if (_useTicketCount != null)
                _useTicketCount.SetText("{0}", _ticketCount.GetDecrypted());
        }
        //------------------------------------------------------------------------------------
        public void SetSlotIdx(int slot)
        {
            _currentSlotIdx = slot;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Minus1Btn()
        {
            _ticketCount -= 1;
            if (_ticketCount < 0)
                _ticketCount = 0;

            RefreshRemainTime();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Plus1Btn()
        {
            int ticketcount = Managers.GoodsManager.Instance.GetGoodsAmount(Define.ResearchAccelTicketIndex).ToInt();

            if (ticketcount < _ticketCount + 1)
                return;

            _ticketCount += 1;

            RefreshRemainTime();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Minus10Btn()
        {
            _ticketCount -= 10;
            if (_ticketCount < 0)
                _ticketCount = 0;

            RefreshRemainTime();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Plus10Btn()
        {
            int ticketcount = Managers.GoodsManager.Instance.GetGoodsAmount(Define.ResearchAccelTicketIndex).ToInt();

            if (ticketcount < _ticketCount + 10)
            {
                _ticketCount = ticketcount;
                return;
            }

            _ticketCount += 1;

            RefreshRemainTime();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_MinBtn()
        {
            int ticketcount = Managers.GoodsManager.Instance.GetGoodsAmount(Define.ResearchAccelTicketIndex).ToInt();

            if (ticketcount > 0)
            {
                _ticketCount = 1;
            }
            else
            {
                _ticketCount = 0;
            }
            
            RefreshRemainTime();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_MaxBtn()
        {
            int ticketcount = Managers.GoodsManager.Instance.GetGoodsAmount(Define.ResearchAccelTicketIndex).ToInt();

            double remaintime = GetRemainResearchTime();

            double countdouble = remaintime / Define.ResearchAccelTime;

            int overcount = Math.Truncate(countdouble).ToInt();
            if (countdouble - overcount > 0)
                overcount++;

            if (ticketcount >= overcount)
            {
                _ticketCount = overcount;
            }
            else
            {
                _ticketCount = ticketcount;
            }

            RefreshRemainTime();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_AdAccelBtn()
        {
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.ResearchTutorial)
            {
                return;
            }

            if (Managers.ResearchManager.Instance.DoAdResearchAccel(_currentSlotIdx) == true)
                ElementExit();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_TicketAccelBtn()
        {
            if (Managers.ResearchManager.Instance.DoTicketResearchAccel(_currentSlotIdx, _ticketCount) == true)
            {
                ignoreExit = false;

                if (_guideCallBack != null)
                    _guideCallBack.EndTutorial();
                ElementExit();
            }
        }
        //------------------------------------------------------------------------------------
    }
}