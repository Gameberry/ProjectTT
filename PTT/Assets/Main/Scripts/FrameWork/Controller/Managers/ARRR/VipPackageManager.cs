using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class VipPackageManager : MonoSingleton<VipPackageManager>
    {
        private List<string> _changeInfoUpdate = new List<string>();

        private List<string> _changeWithPostInfoUpdate = new List<string>();

        public event System.Action changeAdFreeMode;
        public event System.Action changeSpeedUpMode;
        public event System.Action changeAdBuffCountInCrease;
        public event System.Action changeOpenSlotMode;

        public event System.Action changeAdBuffAlways;
        public event System.Action changeSweepUnlimited;

        private Dictionary<int, Sprite> _skillIcons = new Dictionary<int, Sprite>();

        private Dictionary<string, Sprite> _packageIcons = new Dictionary<string, Sprite>();

        private Event.RefreshShopVipPackageMsg _refreshShopVipPackageMsg = new Event.RefreshShopVipPackageMsg();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            VipPackageOperator.Init();

            _changeInfoUpdate.Add(Define.PlayerVipPackageInfoTable);

            _changeWithPostInfoUpdate.Add(Define.PlayerVipPackageInfoTable);
            _changeWithPostInfoUpdate.Add(Define.PlayerShopInfoTable);

            UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += RefreshVipState;
        }
        //------------------------------------------------------------------------------------
        public void InitVipPackageContent()
        {
            TimeManager.Instance.OnInitDailyContent += OnInitDayContent;

            OnInitDayContent(TimeManager.Instance.DailyInit_TimeStamp);

            Define.IsAdFree = IsIgnoreADMode();
            Define.IsSpeedUpMode = IsSpeedUpMode();
            Define.InCreaseAdBuffCount = GetIncreaseAdBuffCount();
            Define.OpenResearchSlot = IsOpenResearchSlotMode();

            Define.IsAdBuffAlways = IsAdBuffAlwaysMode();
            Define.IsSweepUnlimited = IsSweepUnlimitedMode();
        }
        //------------------------------------------------------------------------------------
        private void OnInitDayContent(double nextinittimestamp)
        {
            foreach (KeyValuePair<ObscuredInt, VipPackageData> pair in GetVipPackageDatas())
            {
                VipPackageData vipPackageData = pair.Value;
                if (vipPackageData.ReceiveDiaEveryday <= 0)
                    continue;

                CheckAndSendDia(vipPackageData);
            }
        }
        //------------------------------------------------------------------------------------
        private void CheckAndSendDia(VipPackageData vipPackageData)
        {
            if (vipPackageData.ReceiveDiaEveryday <= 0)
                return;

            if (IsActivePackage(vipPackageData) == false)
                return;

            VipPackageInfo vipPackageInfo = GetVipPackageInfo(vipPackageData);

            if (vipPackageInfo.NextSendDiaTime > Managers.TimeManager.Instance.Current_TimeStamp)
                return;

            Managers.ShopPostManager.Instance.AddVipPost(vipPackageData);
            vipPackageInfo.NextSendDiaTime = Managers.TimeManager.Instance.DailyInit_TimeStamp;

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(_changeWithPostInfoUpdate);
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, VipPackageData> GetVipPackageDatas()
        {
            return VipPackageOperator.GetVipPackageDatas();
        }
        //------------------------------------------------------------------------------------
        public VipPackageData GetVipPackageData(int index)
        {
            return VipPackageOperator.GetVipPackageData(index);
        }
        //------------------------------------------------------------------------------------
        public List<VipPackageShopData> GetVipPackageShopDatas()
        {
            return VipPackageOperator.GetVipPackageShopDatas();
        }
        //------------------------------------------------------------------------------------
        public VipPackageShopData GetVipPackageShopData(int index)
        {
            return GetVipPackageShopDatas().Find(x => x.Index == index);
        }
        //------------------------------------------------------------------------------------
        private void CheckVipMode()
        {
            if (IsIgnoreADMode() != Define.IsAdFree)
            {
                Define.IsAdFree = IsIgnoreADMode();
                changeAdFreeMode?.Invoke();
            }

            if (IsSpeedUpMode() != Define.IsSpeedUpMode)
            {
                Define.IsSpeedUpMode = IsSpeedUpMode();
                changeSpeedUpMode?.Invoke();
            }

            if (GetIncreaseAdBuffCount() != Define.InCreaseAdBuffCount)
            {
                Define.InCreaseAdBuffCount = GetIncreaseAdBuffCount();
                changeAdBuffCountInCrease?.Invoke();
            }

            if (IsOpenResearchSlotMode() != Define.OpenResearchSlot)
            {
                Define.OpenResearchSlot = IsOpenResearchSlotMode();
                changeAdBuffCountInCrease?.Invoke();
            }

            if (IsAdBuffAlwaysMode() != Define.IsAdBuffAlways)
            {
                Define.IsAdBuffAlways = IsAdBuffAlwaysMode();
                changeAdBuffAlways?.Invoke();
            }

            if (IsSweepUnlimitedMode() != Define.IsSweepUnlimited)
            {
                Define.IsSweepUnlimited = IsSweepUnlimitedMode();
                changeSweepUnlimited?.Invoke();
            }
        }
        //------------------------------------------------------------------------------------
        private bool IsIgnoreADMode()
        {
            foreach (KeyValuePair<ObscuredInt, VipPackageData> pair in GetVipPackageDatas())
            {
                VipPackageData vipPackageData = pair.Value;
                if (vipPackageData.IgnoreAd > 0)
                {
                    if (IsActivePackage(vipPackageData) == true)
                        return true;
                }
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        private bool IsSpeedUpMode()
        {
            foreach (KeyValuePair<ObscuredInt, VipPackageData> pair in GetVipPackageDatas())
            {
                VipPackageData vipPackageData = pair.Value;
                if (vipPackageData.IsSpeedUp > 0)
                {
                    if (IsActivePackage(vipPackageData) == true)
                        return true;
                }
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        private int GetIncreaseAdBuffCount()
        {
            int count = 0;

            foreach (KeyValuePair<ObscuredInt, VipPackageData> pair in GetVipPackageDatas())
            {
                VipPackageData vipPackageData = pair.Value;
                if (vipPackageData.AdBuffCountIncrease > 0)
                {
                    if (IsActivePackage(vipPackageData) == true)
                        count += vipPackageData.AdBuffCountIncrease;
                }
            }

            return count;
        }
        //------------------------------------------------------------------------------------
        private bool IsOpenResearchSlotMode()
        {
            foreach (KeyValuePair<ObscuredInt, VipPackageData> pair in GetVipPackageDatas())
            {
                VipPackageData vipPackageData = pair.Value;
                if (vipPackageData.IsOpenResearchSlot > 0)
                {
                    if (IsActivePackage(vipPackageData) == true)
                        return true;
                }
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        private bool IsAdBuffAlwaysMode()
        {
            foreach (KeyValuePair<ObscuredInt, VipPackageData> pair in GetVipPackageDatas())
            {
                VipPackageData vipPackageData = pair.Value;
                if (vipPackageData.AdBuffAlways > 0)
                {
                    if (IsActivePackage(vipPackageData) == true)
                        return true;
                }
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        private bool IsSweepUnlimitedMode()
        {
            foreach (KeyValuePair<ObscuredInt, VipPackageData> pair in GetVipPackageDatas())
            {
                VipPackageData vipPackageData = pair.Value;
                if (vipPackageData.SweepUnlimited > 0)
                {
                    if (IsActivePackage(vipPackageData) == true)
                        return true;
                }
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public bool IsActivePackage(VipPackageData vipPackageData)
        {
            VipPackageInfo vipPackageInfo = GetVipPackageInfo(vipPackageData);
            if (vipPackageInfo == null)
                return false;

            if (vipPackageData.DurationType == V2Enum_IntervalType.Account)
                return true;

            return vipPackageInfo.PackageEndTime >= Managers.TimeManager.Instance.Current_TimeStamp;
        }
        //------------------------------------------------------------------------------------
        public double GetPackageEndTime(VipPackageData vipPackageData)
        {
            VipPackageInfo vipPackageInfo = GetVipPackageInfo(vipPackageData);
            if (vipPackageInfo == null)
                return 0;

            return vipPackageInfo.PackageEndTime;
        }
        //------------------------------------------------------------------------------------
        private VipPackageInfo GetVipPackageInfo(VipPackageData vipPackageData)
        {
            if (vipPackageData == null)
                return null;

            if (VipPackageContainer.VipPackageInfo.ContainsKey(vipPackageData.Index) == true)
                return VipPackageContainer.VipPackageInfo[vipPackageData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public VipPackageInfo AddNewVipPackageInfo(VipPackageData synergyEffectData)
        {
            if (synergyEffectData == null)
                return null;

            VipPackageInfo vipPackageInfo = new VipPackageInfo();
            vipPackageInfo.Index = synergyEffectData.Index;
            VipPackageContainer.VipPackageInfo.Add(vipPackageInfo.Index, vipPackageInfo);

            return vipPackageInfo;
        }
        //------------------------------------------------------------------------------------
        public VipPackageShopInfo GetVipPackageShopInfo(VipPackageShopData vipPackageShopData)
        {
            if (vipPackageShopData == null)
                return null;

            if (VipPackageContainer.VipPackageShopInfo.ContainsKey(vipPackageShopData.Index) == true)
                return VipPackageContainer.VipPackageShopInfo[vipPackageShopData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public VipPackageShopInfo AddNewVipPackageShopInfo(VipPackageShopData vipPackageShopData)
        {
            if (vipPackageShopData == null)
                return null;

            VipPackageShopInfo vipPackageInfo = new VipPackageShopInfo();
            vipPackageInfo.Index = vipPackageShopData.Index;
            VipPackageContainer.VipPackageShopInfo.Add(vipPackageInfo.Index, vipPackageInfo);

            return vipPackageInfo;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetPackageIcon(string path)
        {
            Sprite sp = null;

            if (_packageIcons.ContainsKey(path) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(path, o =>
                {
                    sp = o as Sprite;
                    _packageIcons.Add(path, sp);
                });
            }
            else
                sp = _packageIcons[path];

            return sp;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetRelicIcon(VipPackageData skillBaseData)
        {
            if (skillBaseData == null)
                return null;

            return GetIcon(skillBaseData.Index);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetRelicIcon(int index)
        {
            return GetRelicIcon(GetVipPackageData(index));
        }
        //------------------------------------------------------------------------------------
        private Sprite GetIcon(int iconIndex)
        {
            Sprite sp = null;

            if (_skillIcons.ContainsKey(iconIndex) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(Define.VipPackageIconPath, iconIndex), o =>
                {
                    sp = o as Sprite;
                    _skillIcons.Add(iconIndex, sp);
                });
            }
            else
                sp = _skillIcons[iconIndex];

            return sp;
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyAmount(int index, int amount)
        {
            SetSynergyAmount(GetVipPackageData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyAmount(VipPackageData synergyEffectData, int amount)
        {
            if (synergyEffectData == null)
                return;

            VipPackageInfo playerVipPackageInfo = GetVipPackageInfo(synergyEffectData);
            if (playerVipPackageInfo == null)
                playerVipPackageInfo = AddNewVipPackageInfo(synergyEffectData);

            double addTime = Managers.TimeManager.Instance.GetInitAddTime(synergyEffectData.DurationType, synergyEffectData.DurationParam) * amount;

            if (playerVipPackageInfo.PackageEndTime < TimeManager.Instance.Current_TimeStamp)
            { 
                playerVipPackageInfo.PackageEndTime = TimeManager.Instance.Current_TimeStamp;
                CheckAndSendDia(synergyEffectData);
            }

            playerVipPackageInfo.PackageEndTime += addTime;

            CheckVipMode();
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAmount(int index)
        {
            return GetSynergyAmount(GetVipPackageData(index));
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyAmount(VipPackageData synergyEffectData)
        {
            if (synergyEffectData == null)
                return 0;

            return IsActivePackage(synergyEffectData) == true ? 1 : 0;
        }
        //------------------------------------------------------------------------------------
        public int AddSynergyAmount(int index, int amount)
        {
            return AddSynergyAmount(GetVipPackageData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int AddSynergyAmount(VipPackageData synergyEffectData, int amount)
        {
            if (synergyEffectData == null)
                return 0;

            VipPackageInfo playerVipPackageInfo = GetVipPackageInfo(synergyEffectData);
            if (playerVipPackageInfo == null)
                playerVipPackageInfo = AddNewVipPackageInfo(synergyEffectData);

            double addTime = Managers.TimeManager.Instance.GetInitAddTime(synergyEffectData.DurationType, synergyEffectData.DurationParam) * amount;

            if (playerVipPackageInfo.PackageEndTime < TimeManager.Instance.Current_TimeStamp)
            {
                playerVipPackageInfo.PackageEndTime = TimeManager.Instance.Current_TimeStamp;
                CheckAndSendDia(synergyEffectData);
            }

            playerVipPackageInfo.PackageEndTime += addTime;

            CheckVipMode();

            return 1;
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyAmount(int index, int amount)
        {
            return UseSynergyAmount(GetVipPackageData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int UseSynergyAmount(VipPackageData synergyEffectData, int amount)
        {
            return 0;
        }
        //------------------------------------------------------------------------------------
        public string GetSynergyLocalKey(int index)
        {
            VipPackageData synergyEffectData = GetVipPackageData(index);
            if (synergyEffectData != null)
            {
                return synergyEffectData.NameLocalKey;
            }

            return string.Empty;
        }
        //------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------
        private void RefreshVipState()
        {
            if (BattleSceneManager.Instance.BattleType != V2Enum_Dungeon.LobbyScene)
                return;

            CheckVipMode();

            if (TimeContainer.AccumLoginTime > 86400.0)
            {
                Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.NoAdBuff);
            }
        }
        //------------------------------------------------------------------------------------
        public bool CanBuy(VipPackageShopData synergyEffectData)
        {
            VipPackageShopInfo vipPackageShopInfo = GetVipPackageShopInfo(synergyEffectData);
            if (vipPackageShopInfo == null)
                return true;

            if (synergyEffectData.IntervalType == V2Enum_IntervalType.Account)
                return false;

            return vipPackageShopInfo.NextBuyTime < Managers.TimeManager.Instance.Current_TimeStamp;
        }
        //------------------------------------------------------------------------------------
        public void Buy(VipPackageShopData passData)
        {
            if (CanBuy(passData) == false)
                return;

            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            ShopManager.Instance.ProcessBilling(passData, IAPComplete);
        }
        //------------------------------------------------------------------------------------
        public void IAPComplete(ShopDataBase shopDataBase)
        {
            if (shopDataBase == null)
                return;

            VipPackageShopData passData = shopDataBase as VipPackageShopData;

            VipPackageShopInfo playerShopInfo = GetVipPackageShopInfo(passData) ?? AddNewVipPackageShopInfo(passData);

            if (passData.IntervalType == V2Enum_IntervalType.Account)
                playerShopInfo.NextBuyTime = TimeManager.Instance.Current_TimeStamp + 999999999999999999999.0;
            else
                playerShopInfo.NextBuyTime = TimeManager.Instance.Current_TimeStamp + TimeManager.Instance.GetInitAddTime(passData.IntervalType, passData.IntervalParam);

            ShopPostManager.Instance.AddShopPost(passData);

            Contents.GlobalContent.ShowPopup_Ok(
                LocalStringManager.Instance.GetLocalString("common/popUp/title"),
                string.Format("{0}\n{1}", LocalStringManager.Instance.GetLocalString(passData.TitleLocalStringKey),
                LocalStringManager.Instance.GetLocalString("common/ui/purchaseSuccess")));

            //Contents.GlobalContent.ShowGlobalNotice(LocalStringManager.Instance.GetLocalString("common/ui/purchaseSuccess"));

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(_changeWithPostInfoUpdate, null);

            Message.Send(_refreshShopVipPackageMsg);
        }
        //------------------------------------------------------------------------------------
    }
}