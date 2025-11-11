using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class GameSettingData
    {
        public GameSettingBtn GameSettingBtnEnum = GameSettingBtn.Max;

        public int State = 0;

        private event System.Action _changeState;
        public event System.Action ChangeState
        {
            add { _changeState += value; }
            remove { _changeState -= value; }
        }

        public void CallListener()
        {
            if (_changeState != null)
                _changeState();
        }
    }

    public enum GameSettingBtn
    {
        BGSound = 0,
        FXSound,
        VisibleDamageFont,
        LowSpecMode,
        PowerSavingMode,
        ChangeLanguage,

        Push,
        PushNight,

        CutSceneSkip,

        Max,
    }

    public class GameSettingManager : MonoSingleton<GameSettingManager>
    {
        private Dictionary<GameSettingBtn, GameSettingData> m_gameSettingValue = new Dictionary<GameSettingBtn, GameSettingData>();

        //------------------------------------------------------------------------------------
        public bool cheat_NoDamage = false;
        public bool cheat_onePunch = false;
        public bool cheat_cutScene = false;
        public bool cheat_damageLog = false;
        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            for (int i = 0; i < (int)GameSettingBtn.Max; ++i)
            {
                GameSettingBtn type = (GameSettingBtn)i;
                GameSettingData gameSettingData = new GameSettingData();
                gameSettingData.GameSettingBtnEnum = type;
                gameSettingData.State = GetPlayerPrefsValue(type);

                m_gameSettingValue.Add(type, gameSettingData);
            }

            ApplyLowSpecMode();
        }
        //------------------------------------------------------------------------------------
        public bool Cheat_CutScene()
        {
#if DEV_DEFINE
            if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop
                || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.QA)
            {
                return cheat_cutScene;
            }
#endif
            return false;
        }
        //------------------------------------------------------------------------------------
        public bool Cheat_NoDamage()
        {
#if DEV_DEFINE
            if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop
                || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.QA)
            {
                return cheat_NoDamage;
            }
#endif
            return false;
        }
        //------------------------------------------------------------------------------------
        public bool Cheat_OnPunch()
        {
#if DEV_DEFINE
            if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop
                || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.QA)
            {
                return cheat_onePunch;
            }
#endif
            return false;
        }
        //------------------------------------------------------------------------------------
        public bool Cheat_DamageLog()
        {
#if DEV_DEFINE
            if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop
                || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.QA)
            {
                return cheat_damageLog;
            }
#endif
            return false;
        }
        //------------------------------------------------------------------------------------
        public void AddListener(GameSettingBtn gameSettingBtn, System.Action action)
        {
            if (m_gameSettingValue.ContainsKey(gameSettingBtn) == false)
                return;

            m_gameSettingValue[gameSettingBtn].ChangeState += action;
        }
        //------------------------------------------------------------------------------------
        public void RemoveListener(GameSettingBtn gameSettingBtn, System.Action action)
        {
            if (m_gameSettingValue.ContainsKey(gameSettingBtn) == false)
                return;

            m_gameSettingValue[gameSettingBtn].ChangeState -= action;
        }
        //------------------------------------------------------------------------------------
        public void ApplyLowSpecMode()
        {
            if (IsOn(GameSettingBtn.LowSpecMode) == true)
            {
                Application.targetFrameRate = 30;
            }
            else
                Application.targetFrameRate = 60;

        }
        //------------------------------------------------------------------------------------
        private int GetPlayerPrefsValue(GameSettingBtn gameSettingBtn)
        {
            if (gameSettingBtn == GameSettingBtn.BGSound
                || gameSettingBtn == GameSettingBtn.FXSound
                || gameSettingBtn == GameSettingBtn.VisibleDamageFont
                || gameSettingBtn == GameSettingBtn.Push
                || gameSettingBtn == GameSettingBtn.PushNight)
                return PlayerPrefs.GetInt(gameSettingBtn.ToString(), 1);
            else
                return PlayerPrefs.GetInt(gameSettingBtn.ToString(), 0);
        }
        //------------------------------------------------------------------------------------
        public void ChangeGameOption(GameSettingBtn gameSettingBtn)
        { // 토글형태의 옵션들만 들어온다,
            if (m_gameSettingValue.ContainsKey(gameSettingBtn) == false)
                return;

            bool ison = IsOn(gameSettingBtn);

            if (gameSettingBtn == GameSettingBtn.Push
                || gameSettingBtn == GameSettingBtn.PushNight)
            {
                if (gameSettingBtn == GameSettingBtn.Push)
                {
                    bool seton = ison == false;

                    TheBackEnd.TheBackEnd_Login.SetPush(seton);

                    if (seton == false)
                    {
                        if (IsOn(GameSettingBtn.PushNight) == true)
                        {
                            TheBackEnd.TheBackEnd_Login.SetPushNight(seton);
                        }
                    }
                }
                else if (gameSettingBtn == GameSettingBtn.PushNight)
                {
                    bool seton = ison == false;

                    TheBackEnd.TheBackEnd_Login.SetPushNight(seton);

                    if (seton == true)
                    {
                        if (IsOn(GameSettingBtn.Push) == false)
                        {
                            TheBackEnd.TheBackEnd_Login.SetPush(seton);
                        }
                    }
                }
            }
            else
            {
                if (ison == true)
                    SetPlayerPrefsValue(gameSettingBtn, 0);
                else
                    SetPlayerPrefsValue(gameSettingBtn, 1);

                if (gameSettingBtn == GameSettingBtn.LowSpecMode)
                    ApplyLowSpecMode();
            }
        }
        //------------------------------------------------------------------------------------
        public void SetPushState(GameSettingBtn gameSettingBtn, bool IsOn)
        {
            if (m_gameSettingValue.ContainsKey(gameSettingBtn) == true)
            {
                m_gameSettingValue[gameSettingBtn].State = IsOn == true ? 1 : 0;
                m_gameSettingValue[gameSettingBtn].CallListener();
            }
        }
        //------------------------------------------------------------------------------------
        public bool IsOn(GameSettingBtn gameSettingBtn)
        {
            if (m_gameSettingValue.ContainsKey(gameSettingBtn) == false)
                return true;

            return m_gameSettingValue[gameSettingBtn].State == 0 ? false : true;
        }
        //------------------------------------------------------------------------------------
        private void SetPlayerPrefsValue(GameSettingBtn gameSettingBtn, int value)
        {
            if (m_gameSettingValue.ContainsKey(gameSettingBtn) == true)
            { 
                m_gameSettingValue[gameSettingBtn].State = value;
                m_gameSettingValue[gameSettingBtn].CallListener();
                PlayerPrefs.SetInt(gameSettingBtn.ToString(), value);
                PlayerPrefs.Save();
            }
        }
        //------------------------------------------------------------------------------------
        //private string prevName = string.Empty;
        //private bool benNickNameChange = false;
        public void ChangeNickName(string nickname)
        {
            //if (benNickNameChange == true)
            //    return;

            //prevName = Managers.PlayerDataManager.Instance.GetPlayerName();

            //if (nickname == prevName)
            //    return;

            //benNickNameChange = true;

            //DKServerManager.Instance.API_Player_Paid_Set_NameRequest(nickname, API_Player_Paid_Set_NameResponse);
        }
        //------------------------------------------------------------------------------------
        //private void API_Player_Paid_Set_NameResponse(PlayerPaidSetNameResponse playerPaidSetNameResponse)
        //{
            //benNickNameChange = false;

            //if (playerPaidSetNameResponse.error_code != 0)
            //{
            //    m_showNickNameChangeErrorMsg.IsSuccess = false;
            //    m_showNickNameChangeErrorMsg.ShowNoticeMsg = LocalStringManager.Instance.GetLocalString("PlayerNick_UI_Overlap");
            //    Message.Send(m_showNickNameChangeErrorMsg);
            //    Debug.LogError(playerPaidSetNameResponse.error_message);
            //    return;
            //}

            //m_showNickNameChangeErrorMsg.IsSuccess = true;
            //Message.Send(m_showNickNameChangeErrorMsg);

            //double former_quan, used_quan, keep_quan;

            //ItemData data = ItemManager.Instance.GetItemData(ItemSortTypeName.Gem);

            //former_quan = PlayerDataManager.Instance.GetGoodsAmount(data.ItemID);

            //PlayerDataManager.Instance.SetCurrencyDatas(playerPaidSetNameResponse.currency_list);
            //PlayerDataManager.Instance.SetPlayerName(playerPaidSetNameResponse.player.player_name);

            //keep_quan = PlayerDataManager.Instance.GetGoodsAmount(data.ItemID);

            //used_quan = former_quan - keep_quan;

            //ThirdPartyLog.Instance.SendLog_NickeEvent(prevName, playerPaidSetNameResponse.player.player_name,
            //    data.ItemID.ToString(), former_quan.ToString(), used_quan.ToString(), keep_quan.ToString());
        //}
        //------------------------------------------------------------------------------------
    }
}