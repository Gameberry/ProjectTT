using System.Collections.Generic;
using UnityEngine;
using GameBerry;

namespace GameBerry.Managers
{
    public class PlayerDataManager : MonoSingleton<PlayerDataManager>
    {
        private Event.RefreshNickNameMsg m_refreshNickNameMsg = new Event.RefreshNickNameMsg();

        private Event.RefreshProfileMsg m_refreshProfileMsg = new Event.RefreshProfileMsg();

        public event System.Action onAdFreeMode;
        public event System.Action onBuffFree;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
        }
        //------------------------------------------------------------------------------------
        protected override void Release()
        {

        }
        //------------------------------------------------------------------------------------
        public CharacterProfileData GetCurrentProfile()
        {
            if (PlayerDataContainer.characterProfileDatas_Dic.ContainsKey(PlayerDataContainer.Profile) == true)
                return PlayerDataContainer.characterProfileDatas_Dic[PlayerDataContainer.Profile];

            return null;
        }
        //------------------------------------------------------------------------------------
        public CharacterProfileData GetProfile(int index)
        {
            if (PlayerDataContainer.characterProfileDatas_Dic.ContainsKey(index) == true)
                return PlayerDataContainer.characterProfileDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public void SetProfileIndex(int index)
        {
            PlayerDataContainer.Profile = index;

            CharacterProfileData characterProfileData = GetProfile(index);

            if(characterProfileData != null)
            {
                m_refreshProfileMsg.characterProfileData = characterProfileData;
                Message.Send(m_refreshProfileMsg);
            }

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerInfoTable);
        }
        //------------------------------------------------------------------------------------
        public void ChangeAdFree()
        {
            if (Define.IsAdFree == true)
                return;

            Define.IsAdFree = true;

            if (onAdFreeMode != null)
                onAdFreeMode();

            onAdFreeMode = null;

            Message.Send(new Event.ChangeAdFreeStateUIMsg());

            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerInfoTable();
        }
        //------------------------------------------------------------------------------------
        public void RecvPreregistGift()
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            PlayerDataContainer.PlayerRecvPreReward = 1;

            RewardData rewardData = Managers.RewardManager.Instance.GetRewardData();
            rewardData.Amount = 5000;
            rewardData.V2Enum_Goods = V2Enum_Goods.Point;
            rewardData.Index = V2Enum_Point.LobbyGold.Enum32ToInt();

            GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerInfoTable();
            TheBackEnd.TheBackEndManager.Instance.SendUpdateWaitData(true);


            GameBerry.Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new GameBerry.Event.SetInGameRewardPopupMsg();
            m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
            Message.Send(m_setInGameRewardPopupMsg);

            UI.IDialog.RequestDialogEnter<UI.InGameRewardPopupDialog>();
        }
        //------------------------------------------------------------------------------------
        public void RecvSigninGift()
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            PlayerDataContainer.PlayerRecvLaunchReward = 1;

            RewardData rewardData = Managers.RewardManager.Instance.GetRewardData();
            rewardData.Amount = 5000;
            rewardData.V2Enum_Goods = V2Enum_Goods.Point;
            rewardData.Index = V2Enum_Point.LobbyGold.Enum32ToInt();

            GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerInfoTable();
            TheBackEnd.TheBackEndManager.Instance.SendUpdateWaitData(true);

            GameBerry.Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new GameBerry.Event.SetInGameRewardPopupMsg();
            m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
            Message.Send(m_setInGameRewardPopupMsg);

            UI.IDialog.RequestDialogEnter<UI.InGameRewardPopupDialog>();
        }
        //------------------------------------------------------------------------------------
        public string GetPlayerName()
        {
            return PlayerDataContainer.PlayerName;
        }
        //------------------------------------------------------------------------------------
    }
}