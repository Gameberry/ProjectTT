using System.Collections.Generic;
using UnityEngine;
using GameBerry;

namespace GameBerry.Managers
{
    public class PlayerDataManager : MonoSingleton<PlayerDataManager>
    {
        private Event.RefreshNickNameMsg m_refreshNickNameMsg = new Event.RefreshNickNameMsg();

        private Event.RefreshProfileMsg m_refreshProfileMsg = new Event.RefreshProfileMsg();

        private Event.ReadyLuckyRouletteMsg m_readyLuckyRouletteMsg = new Event.ReadyLuckyRouletteMsg();
        

        public event System.Action onAdFreeMode;
        public event System.Action onBuffFree;

        private LuckyRouletteLocalTable luckyRouletteLocalTable = null;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
        }
        //------------------------------------------------------------------------------------
        protected override void Release()
        {

        }
        //------------------------------------------------------------------------------------
        public void InitPlayerDataContent()
        {
            luckyRouletteLocalTable = TableManager.Instance.GetTableClass<LuckyRouletteLocalTable>();

            double currentTime = TimeManager.Instance.Current_TimeStamp;

            if (PlayerDataContainer.LastRouletteInitTimeStemp < currentTime)
            {
                PlayerDataContainer.LastRouletteActionTime = 0.0;
                PlayerDataContainer.TodayRouletteActionCount = 0;
                PlayerDataContainer.LastRouletteInitTimeStemp = TimeManager.Instance.DailyInit_TimeStamp;
            }

            TimeManager.Instance.OnInitDailyContent += OnInitContent;

            if (CanRoulette() == false)
                TimeManager.Instance.PlayCheckLuckyRoulette();
        }
        //------------------------------------------------------------------------------------
        public void OnInitContent(double nextinittimestamp)
        {
            PlayerDataContainer.LastRouletteActionTime = 0.0;
            PlayerDataContainer.TodayRouletteActionCount = 0;
            PlayerDataContainer.LastRouletteInitTimeStemp = TimeManager.Instance.DailyInit_TimeStamp;

            Message.Send(new Event.ReadyLuckyRouletteMsg());
        }
        //------------------------------------------------------------------------------------
        public List<LuckyRouletteData> GetLuckytRouletteAllData()
        {
            return luckyRouletteLocalTable.luckytRouletteDatas;
        }
        //------------------------------------------------------------------------------------
        public double GetLuckytRouletteTotalWeightData()
        {
            return luckyRouletteLocalTable.luckytRouletteDataPicker.GetTotalWeight();
        }
        //------------------------------------------------------------------------------------
        public int RemainRouletteCount()
        {
            return Define.LuckyRouletteDailySpinMaxCount.GetDecrypted() - PlayerDataContainer.TodayRouletteActionCount.GetDecrypted();
        }
        //------------------------------------------------------------------------------------
        public bool IsAdRoulette()
        {
            return Define.LuckyRouletteDailySpinMaxCount.GetDecrypted() > RemainRouletteCount();
        }
        //------------------------------------------------------------------------------------
        public bool CanRoulette()
        {
            double currentTime = TimeManager.Instance.Current_TimeStamp;
            return PlayerDataContainer.LastRouletteActionTime.GetDecrypted() + Define.LuckyRouletteSpinInterval.GetDecrypted() <= currentTime;
        }
        //------------------------------------------------------------------------------------
        public LuckyRouletteData PlayLuckyRoulette()
        {
            LuckyRouletteData luckyRouletteData = luckyRouletteLocalTable.luckytRouletteDataPicker.Pick();

            PlayerDataContainer.LastRouletteActionTime = TimeManager.Instance.Current_TimeStamp;
            PlayerDataContainer.TodayRouletteActionCount += 1;

            TimeManager.Instance.PlayCheckLuckyRoulette();

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();


            RewardData rewardData = luckyRouletteData.RewardData;

            reward_type.Add(rewardData.Index);
            before_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

            reward_quan.Add((int)rewardData.Amount);

            Managers.GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

            after_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerInfoTable);
            TheBackEnd.TheBackEndManager.Instance.SendUpdateWaitData(true);

            SendReadyLuckyRouletteMsg();

            ThirdPartyLog.Instance.SendLog_LuckyRouletteEvent(PlayerDataContainer.TodayRouletteActionCount.GetDecrypted()
                , reward_type, before_quan, reward_quan, after_quan);

            return luckyRouletteData;
        }
        //------------------------------------------------------------------------------------
        public void SendReadyLuckyRouletteMsg()
        {
            Message.Send(m_readyLuckyRouletteMsg);
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
        public void SetProfileData(List<Sprite> characterProfileSprites)
        {
            List<CharacterSkinData> characterSkinDatas = CharacterSkinManager.Instance.GetCharacterSkinAllData(V2Enum_Skin.SkinBody).FindAll(x => x.SkinIndex != -1);

            for (int i = 0; i < characterProfileSprites.Count; ++i)
            {
                CharacterProfileData characterProfileData = new CharacterProfileData();
                CharacterSkinData myskindata = characterSkinDatas.Find(x => x.SkinIndex == i);

                if (myskindata != null)
                {
                    characterProfileData.isEnable = CharacterSkinManager.Instance.GetPlayerSkinInfo(myskindata.Index) != null;
                }
                else
                    characterProfileData.isEnable = true;

                //if (i == 4 || i == 5)
                //{
                //    if (i == 4)
                //        characterProfileData.isEnable = CharacterSkinManager.Instance.GetPlayerSkinInfo(141010008) != null;
                //    if (i == 5)
                //        characterProfileData.isEnable = CharacterSkinManager.Instance.GetPlayerSkinInfo(141010009) != null;
                //}
                //else
                //    characterProfileData.isEnable = true;

                characterProfileData.sprite = characterProfileSprites[i];
                characterProfileData.profileIndex = i;
                characterProfileData.isAlly = false;
                PlayerDataContainer.characterProfileDatas.Add(characterProfileData);
                PlayerDataContainer.characterProfileDatas_Dic.Add(characterProfileData.profileIndex, characterProfileData);
            }
        }
        //------------------------------------------------------------------------------------
        public void AddNewProfileData_Skin(CharacterSkinData skinindex)
        {
            if (skinindex == null)
                return;

            //int index = -1;

            //if (skinindex == 141010008)
            //    index = 4;
            //else if (skinindex == 141010009)
            //    index = 5;
            //else if (skinindex == 141010014)
            //    index = 6;
            //else if (skinindex == 141010017)
            //    index = 7;

            if (PlayerDataContainer.characterProfileDatas_Dic.ContainsKey(skinindex.SkinIndex) == true)
            {
                CharacterProfileData characterProfileData = PlayerDataContainer.characterProfileDatas_Dic[skinindex.SkinIndex];
                characterProfileData.isEnable = true;
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.CharacterProfile);
            }
        }
        //------------------------------------------------------------------------------------
        public void AddNewProfileData_Ally(int index)
        {
            if (PlayerDataContainer.characterProfileDatas_Dic.ContainsKey(index) == true)
            {
                CharacterProfileData characterProfileData = PlayerDataContainer.characterProfileDatas_Dic[index];
                characterProfileData.isEnable = true;
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.CharacterProfile);
            }
        }
        //------------------------------------------------------------------------------------
        public List<CharacterProfileData> GetCharacterProfileDatas()
        {
            return PlayerDataContainer.characterProfileDatas;
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