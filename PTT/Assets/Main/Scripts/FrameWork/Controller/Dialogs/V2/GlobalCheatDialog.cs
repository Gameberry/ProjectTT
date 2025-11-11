using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class GlobalCheatDialog : IDialog
    {
        [Header("------------CheatGroup------------")]
        [SerializeField]
        private TMP_Dropdown m_cheatGoodsIDDropdown;

        [SerializeField]
        private TMP_Dropdown m_cheatGoodsIndexDropdown;

        [SerializeField]
        private TMP_InputField m_cheatGoodsAmountInputField;

        [SerializeField]
        private Button m_cheatApplyBtn;



        [SerializeField]
        private TMP_InputField m_maxStageInputField;

        [SerializeField]
        private Button m_maxStageApplyBtn;


        [SerializeField]
        private TMP_InputField m_setStageRewardInputField;

        [SerializeField]
        private Button m_setStageRewardApplyBtn;




        [SerializeField]
        private Toggle m_isAdFreeMode;



        [SerializeField]
        private TMP_InputField m_cheatTimeInputField;

        [SerializeField]
        private Button m_showLogBtn;

        [SerializeField]
        private Button m_showProfileBtn;


        [SerializeField]
        private Button m_deleteAllDBBtn;

        [SerializeField]
        private Button m_deleteRelicBtn;

        [SerializeField]
        private Button m_allHideRedDotBtn;

        [SerializeField]
        private Button m_fullStageIdleRewardBtn;

        [SerializeField]
        private Button m_initDailyContentBtn;

        [SerializeField]
        private Button m_initWeekContentBtn;

        [SerializeField]
        private Button m_initMonthContentBtn;


        [SerializeField]
        private Toggle m_cutScene;

        [SerializeField]
        private Toggle m_noDamage;

        [SerializeField]
        private Toggle m_onePunch;

        [SerializeField]
        private Toggle m_damageLog;

        [SerializeField]
        private Button m_mansurModeBtn;

        [SerializeField]
        private Button m_initMansurModeBtn;

        private V2Enum_Goods m_cheat_V2Enum_Goods = V2Enum_Goods.Max;

        private FrameChecker frameChecker;

#if DEV_DEFINE

        public static int CheatMapStage = -1;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            ////////////////////////Cheat
            if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop
                || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.QA)
            {
                if (m_cheatGoodsIDDropdown != null)
                {
                    m_cheatGoodsIDDropdown.ClearOptions();
                    List<string> optiondatalabel = new List<string>();

                    for (int i = (int)V2Enum_Goods.Gear; i < (int)V2Enum_Goods.Max; ++i)
                    {
                        optiondatalabel.Add(((V2Enum_Goods)i).ToString());
                    }

                    m_cheatGoodsIDDropdown.AddOptions(optiondatalabel);
                    m_cheatGoodsIDDropdown.onValueChanged.AddListener(OnValueChange_CheatGoodsID);

                    OnValueChange_CheatGoodsID(0);
                }

                if (m_cheatGoodsIndexDropdown != null)
                    m_cheatGoodsIndexDropdown.ClearOptions();

                if (m_cheatGoodsAmountInputField != null)
                    m_cheatGoodsAmountInputField.contentType = TMP_InputField.ContentType.IntegerNumber;

                if (m_cheatApplyBtn != null)
                    m_cheatApplyBtn.onClick.AddListener(OnClick_CheatApplyBtn);


                if (m_maxStageInputField != null)
                    m_maxStageInputField.contentType = TMP_InputField.ContentType.IntegerNumber;

                if (m_maxStageApplyBtn != null)
                    m_maxStageApplyBtn.onClick.AddListener(OnClick_SetMaxStage);


                if (m_setStageRewardInputField != null)
                    m_setStageRewardInputField.contentType = TMP_InputField.ContentType.IntegerNumber;

                if (m_setStageRewardApplyBtn != null)
                    m_setStageRewardApplyBtn.onClick.AddListener(OnClick_SetStageReward);


                if (m_isAdFreeMode != null)
                    m_isAdFreeMode.onValueChanged.AddListener(OnValueChanged_AdFreeMode);


                if (m_cheatTimeInputField != null)
                {
                    m_cheatTimeInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                    m_cheatTimeInputField.onValueChanged.AddListener(OnValueChange_CheatTime);
                }

                //if (m_fullBerserkerGaugeBtn != null)
                //    m_fullBerserkerGaugeBtn.onClick.AddListener(Managers.BerserkerManager.Instance.Cheat_FullBerserkerGauge);

                if (m_showLogBtn != null)
                    m_showLogBtn. onClick.AddListener(() =>
                    {
                        Gpm.LogViewer.GpmLogViewer.Instance.Show();
                    });

                if (m_showProfileBtn != null)
                    m_showProfileBtn.onClick.AddListener(() =>
                    {
                        if (frameChecker != null)
                            frameChecker.enabled = !frameChecker.enabled;
                    });


                if (m_deleteAllDBBtn != null)
                    m_deleteAllDBBtn.onClick.AddListener(() =>
                    {
                        TheBackEnd.TheBackEnd_PlayerTable.DeleteAllTable();
                    });

                if (m_deleteRelicBtn != null)
                    m_deleteRelicBtn.onClick.AddListener(() =>
                    {
                        RelicContainer.SynergyInfo.Clear();
                    });

                if (m_allHideRedDotBtn != null)
                    m_allHideRedDotBtn.onClick.AddListener(() =>
                    {
                        foreach (var pair in Managers.RedDotManager.Instance.m_uiRedDotElement)
                        {
                            Managers.RedDotManager.Instance.HideRedDot(pair.Key);
                        }
                    });

                if (m_fullStageIdleRewardBtn != null)
                    m_fullStageIdleRewardBtn.onClick.AddListener(OnClick_GetStageCooltimeRewardBtn);

                if (m_initDailyContentBtn != null)
                    m_initDailyContentBtn.onClick.AddListener(OnClick_InitDailyContentBtn);

                if (m_initWeekContentBtn != null)
                    m_initWeekContentBtn.onClick.AddListener(OnClick_InitWeekContentBtn);

                if (m_initMonthContentBtn != null)
                    m_initMonthContentBtn.onClick.AddListener(OnClick_InitMonthContentBtn);



                if (m_mansurModeBtn != null)
                    m_mansurModeBtn.onClick.AddListener(OnClick_MansurModeBtn);

                if (m_initMansurModeBtn != null)
                    m_initMansurModeBtn.onClick.AddListener(OnClick_InitMansurModeBtn);

                if (m_cutScene != null)
                    m_cutScene.onValueChanged.AddListener(o =>
                    {
                        Managers.GameSettingManager.Instance.cheat_cutScene = o;
                    });

                if (m_noDamage != null)
                    m_noDamage.onValueChanged.AddListener(o =>
                    {
                        Managers.GameSettingManager.Instance.cheat_NoDamage = o;
                    });

                if (m_onePunch != null)
                    m_onePunch.onValueChanged.AddListener(o =>
                    {
                        Managers.GameSettingManager.Instance.cheat_onePunch = o;
                    });

                if (m_damageLog != null)
                    m_damageLog.onValueChanged.AddListener(o =>
                    {
                        Managers.GameSettingManager.Instance.cheat_damageLog = o;
                    });
            }
            ////////////////////////Cheat

            if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop
                || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.QA)
            {
                frameChecker = gameObject.AddComponent<FrameChecker>();
                frameChecker.enabled = false;
            }

            OnValueChange_CheatGoodsID(0);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Product
                || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Stage)
            {
                TheBackEnd.TheBackEndManager.Instance.OnCheatingDetected();
            }

            if (m_isAdFreeMode != null)
                m_isAdFreeMode.isOn = Define.IsAdFree;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SetMaxStage()
        {
            CheatMapStage = m_maxStageInputField.text.ToInt();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SetStageReward()
        {
            int rewardnum = m_setStageRewardInputField.text.ToInt();

            int stage = rewardnum / 100;
            int wave = rewardnum % 100;

            StageInfo stageInfo = null;

            if (MapContainer.StageInfos.ContainsKey(stage) == false)
            {
                stageInfo = new StageInfo();
                stageInfo.StageNumber = stage;
                MapContainer.StageInfos.Add(stageInfo.StageNumber, stageInfo);
            }
            else
                stageInfo = MapContainer.StageInfos[stage];

            if (stageInfo.LastClearWave < wave)
                stageInfo.LastClearWave = wave;
            stageInfo.RecvClearReward = wave;
        }
        //------------------------------------------------------------------------------------
        private void OnValueChange_CheatGoodsID(int value)
        {
            int goodsvalue = m_cheatGoodsIDDropdown.value + (int)V2Enum_Goods.Gear;
            m_cheatGoodsIndexDropdown.ClearOptions();
            m_cheat_V2Enum_Goods = (V2Enum_Goods)goodsvalue;

            List<TMP_Dropdown.OptionData> optiondatalabel = new List<TMP_Dropdown.OptionData>();

            switch (m_cheat_V2Enum_Goods)
            {
                case V2Enum_Goods.Gear:
                    {
                        Dictionary<ObscuredInt, GearData> boxDatas = Managers.GearManager.Instance.GetAllGearData_Dic();

                        foreach (var pair in boxDatas)
                        {
                            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                            optionData.text = pair.Value.Index.ToString();
                            optionData.image = Managers.GoodsManager.Instance.GetGoodsSprite(goodsvalue, pair.Value.Index);
                            optiondatalabel.Add(optionData);
                        }

                        break;
                    }

                case V2Enum_Goods.Point:
                    {
                        for (int i = (int)V2Enum_Point.InGameGold; i < (int)V2Enum_Point.Max; ++i)
                        {
                            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                            optionData.text = ((V2Enum_Point)i).ToString();
                            optionData.image = Managers.GoodsManager.Instance.GetGoodsSprite(goodsvalue, i);
                            optiondatalabel.Add(optionData);
                        }

                        break;
                    }
                case V2Enum_Goods.Ally:
                    {
                        //List<AllyV3Data> allyV2Data = Managers.AllyV3Manager.Instance.GetAllyAllData();

                        //for (int i = 0; i < allyV2Data.Count; ++i)
                        //{
                        //    TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                        //    optionData.text = Managers.LocalStringManager.Instance.GetLocalString(allyV2Data[i].NameLocalStringKey);
                        //    optionData.image = Managers.GoodsManager.Instance.GetGoodsSprite(goodsvalue, allyV2Data[i].Index);
                        //    optiondatalabel.Add(optionData);
                        //}

                        break;
                    }
                case V2Enum_Goods.SummonTicket:
                    {
                        //Dictionary<ObscuredInt, SummonTicketData> summonTicketDatas = Managers.SummonTicketManager.Instance.GetAllBoxData();

                        //foreach (var pair in summonTicketDatas)
                        //{
                        //    TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                        //    optionData.text = Managers.LocalStringManager.Instance.GetLocalString(pair.Value.NameLocalStringKey);
                        //    optionData.image = Managers.GoodsManager.Instance.GetGoodsSprite(goodsvalue, pair.Value.Index);
                        //    optiondatalabel.Add(optionData);
                        //}

                        break;
                    }
                case V2Enum_Goods.Skin:
                    {
                        //List<CharacterSkinData> CharacterSkillDatas = new List<CharacterSkinData>();
                        //CharacterSkillDatas.AddRange(Managers.CharacterSkinManager.Instance.GetCharacterSkinAllData(V2Enum_Skin.SkinWeapon));
                        //CharacterSkillDatas.AddRange(Managers.CharacterSkinManager.Instance.GetCharacterSkinAllData(V2Enum_Skin.SkinBody));

                        //for (int i = 0; i < CharacterSkillDatas.Count; ++i)
                        //{
                        //    TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                        //    optionData.text = Managers.LocalStringManager.Instance.GetLocalString(CharacterSkillDatas[i].NameLocalStringKey);
                        //    optionData.image = Managers.GoodsManager.Instance.GetGoodsSprite(goodsvalue, CharacterSkillDatas[i].Index);
                        //    optiondatalabel.Add(optionData);
                        //}

                        break;
                    }
                case V2Enum_Goods.Box:
                    {
                        //Dictionary<ObscuredInt, BoxData> boxDatas = Managers.BoxManager.Instance.GetAllBoxData();

                        //foreach (var pair in boxDatas)
                        //{
                        //    TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                        //    optionData.text = Managers.LocalStringManager.Instance.GetLocalString(pair.Value.NameLocalStringKey);
                        //    optionData.image = Managers.GoodsManager.Instance.GetGoodsSprite(goodsvalue, pair.Value.Index);
                        //    optiondatalabel.Add(optionData);
                        //}

                        break;
                    }
                case V2Enum_Goods.Synergy:
                    {
                        Dictionary<ObscuredInt, SynergyEffectData> boxDatas = Managers.SynergyManager.Instance.GetAllSynergyEffectDatas();

                        foreach (var pair in boxDatas)
                        {
                            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                            optionData.text = pair.Value.Index.ToString();
                            optionData.image = Managers.GoodsManager.Instance.GetGoodsSprite(goodsvalue, pair.Value.Index);
                            optiondatalabel.Add(optionData);
                        }

                        break;
                    }
                case V2Enum_Goods.Descend:
                    {
                        Dictionary<ObscuredInt, DescendData> boxDatas = Managers.DescendManager.Instance.GetAllSynergyEffectDatas();

                        foreach (var pair in boxDatas)
                        {
                            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                            optionData.text = pair.Value.Index.ToString();
                            optionData.image = Managers.GoodsManager.Instance.GetGoodsSprite(goodsvalue, pair.Value.Index);
                            optiondatalabel.Add(optionData);
                        }

                        break;
                    }
                case V2Enum_Goods.Relic:
                    {
                        Dictionary<ObscuredInt, RelicData> boxDatas = Managers.RelicManager.Instance.GetAllRelicData();

                        foreach (var pair in boxDatas)
                        {
                            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                            optionData.text = pair.Value.Index.ToString();
                            optionData.image = Managers.GoodsManager.Instance.GetGoodsSprite(goodsvalue, pair.Value.Index);
                            optiondatalabel.Add(optionData);
                        }

                        break;
                    }
                case V2Enum_Goods.VipPackage:
                    {
                        Dictionary<ObscuredInt, VipPackageData> boxDatas = Managers.VipPackageManager.Instance.GetVipPackageDatas();

                        foreach (var pair in boxDatas)
                        {
                            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                            optionData.text = pair.Value.Index.ToString();
                            optionData.image = Managers.GoodsManager.Instance.GetGoodsSprite(goodsvalue, pair.Value.Index);
                            optiondatalabel.Add(optionData);
                        }

                        break;
                    }
                case V2Enum_Goods.SynergyBreak:
                    {
                        Dictionary<ObscuredInt, SynergyBreakthroughData> boxDatas = Managers.SynergyManager.Instance.GetAllSynergyBreakthroughData();

                        foreach (var pair in boxDatas)
                        {
                            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                            optionData.text = pair.Value.Index.ToString();
                            optionData.image = Managers.GoodsManager.Instance.GetGoodsSprite(goodsvalue, pair.Value.Index);
                            optiondatalabel.Add(optionData);
                        }

                        break;
                    }
                case V2Enum_Goods.SynergyRune:
                    {
                        Dictionary<ObscuredInt, SynergyRuneData> boxDatas = Managers.SynergyRuneManager.Instance.GetAllSynergyRuneData_Dic();

                        foreach (var pair in boxDatas)
                        {
                            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                            optionData.text = pair.Value.Index.ToString();
                            optionData.image = Managers.GoodsManager.Instance.GetGoodsSprite(goodsvalue, pair.Value.Index);
                            optiondatalabel.Add(optionData);
                        }

                        break;
                    }
            }

            m_cheatGoodsIndexDropdown.AddOptions(optiondatalabel);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_CheatApplyBtn()
        {
            if (m_cheatGoodsIDDropdown == null
                || m_cheatGoodsIndexDropdown == null
                || m_cheatGoodsAmountInputField == null)
                return;

            int goodsvalue = m_cheatGoodsIDDropdown.value + (int)V2Enum_Goods.Gear;

            int itemid = m_cheatGoodsIndexDropdown.value;

            switch (m_cheat_V2Enum_Goods)
            {
                
                case V2Enum_Goods.Point:
                    {
                        itemid += (int)V2Enum_Point.InGameGold;
                        break;
                    }
                case V2Enum_Goods.Ally:
                    {
                        //List<AllyV3Data> CharacterSkillDatas = Managers.AllyV3Manager.Instance.GetAllyAllData();
                        //if (CharacterSkillDatas.Count > itemid)
                        //    itemid = CharacterSkillDatas[itemid].Index;

                        break;
                    }

                case V2Enum_Goods.SummonTicket:
                    {
                        itemid = itemid + 172010001;
                        break;
                    }
                case V2Enum_Goods.Skin:
                    {
                        //List<CharacterSkinData> CharacterSkillDatas = new List<CharacterSkinData>();
                        //CharacterSkillDatas.AddRange(Managers.CharacterSkinManager.Instance.GetCharacterSkinAllData(V2Enum_Skin.SkinWeapon));
                        //CharacterSkillDatas.AddRange(Managers.CharacterSkinManager.Instance.GetCharacterSkinAllData(V2Enum_Skin.SkinBody));

                        //if (CharacterSkillDatas.Count > itemid)
                        //    itemid = CharacterSkillDatas[itemid].Index;

                        break;
                    }
                case V2Enum_Goods.Box:
                    {
                        itemid = itemid + 174010001;

                        break;
                    }
                case V2Enum_Goods.Gear:
                case V2Enum_Goods.Synergy:
                case V2Enum_Goods.Descend:
                case V2Enum_Goods.Relic:
                case V2Enum_Goods.VipPackage:
                case V2Enum_Goods.SynergyBreak:
                case V2Enum_Goods.SynergyRune:
                    {
                        int indexsss = m_cheatGoodsIndexDropdown.value;
                        itemid = m_cheatGoodsIndexDropdown.options[indexsss].text.ToInt();

                        break;
                    }
            }

            double itemamount = m_cheatGoodsAmountInputField.text.ToDouble();

            Managers.GoodsManager.Instance.SetGoodsAmount(goodsvalue, itemid, itemamount);
        }
        //------------------------------------------------------------------------------------
        private void OnValueChanged_AdFreeMode(bool value)
        {
            if (value == true)
                Managers.PlayerDataManager.Instance.ChangeAdFree();

            Define.IsAdFree = value;
        }
        //------------------------------------------------------------------------------------
        private void OnValueChange_CheatTime(string str)
        {
            Managers.TimeManager.Instance.CheatTime = str.ToDouble();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_GetStageCooltimeRewardBtn()
        {
            Managers.TimeManager.Instance.SetCheat_FullStageCoolTimeReward();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_InitDailyContentBtn()
        {
            Managers.TimeManager.Instance.DailyInit_TimeStamp = Managers.TimeManager.Instance.Current_TimeStamp + 1;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_InitWeekContentBtn()
        {
            Managers.TimeManager.Instance.WeekInit_TimeStamp = Managers.TimeManager.Instance.Current_TimeStamp + 1;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_InitMonthContentBtn()
        {
            Managers.TimeManager.Instance.MonthInit_TimeStamp = Managers.TimeManager.Instance.Current_TimeStamp + 1;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_MansurModeBtn()
        {
            for (int goods = (int)V2Enum_Goods.Gear; goods < (int)V2Enum_Goods.Max; ++goods)
            {
                V2Enum_Goods v2Enum_Goods = (V2Enum_Goods)goods;

                switch (v2Enum_Goods)
                {
                    case V2Enum_Goods.Gear:
                        {
                            Dictionary<ObscuredInt, GearData> boxDatas = Managers.GearManager.Instance.GetAllGearData_Dic();

                            foreach (var pair in boxDatas)
                            {
                                Managers.GearManager.Instance.SetSynergyAmount(pair.Value.Index, 1);
                            }

                            break;
                        }
                    case V2Enum_Goods.Point:
                        {
                            for (int i = (int)V2Enum_Point.InGameGold; i < (int)V2Enum_Point.Max; ++i)
                            {
                                Managers.GoodsManager.Instance.SetGoodsAmount(goods, i, int.MaxValue / 2);
                            }

                            break;
                        }
                    case V2Enum_Goods.Ally:
                        {
                            //List<AllyV3Data> allyV2Data = Managers.AllyV3Manager.Instance.GetAllyAllData();

                            //for (int i = 0; i < allyV2Data.Count; ++i)
                            //{
                            //    Managers.GoodsManager.Instance.AddGoodsAmount(goods, allyV2Data[i].Index, 1);
                            //}

                            break;
                        }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_InitMansurModeBtn()
        {
            for (int goods = (int)V2Enum_Goods.Gear; goods < (int)V2Enum_Goods.Max; ++goods)
            {
                V2Enum_Goods v2Enum_Goods = (V2Enum_Goods)goods;

                switch (v2Enum_Goods)
                {
                    case V2Enum_Goods.Gear:
                        {
                            Dictionary<ObscuredInt, GearData> boxDatas = Managers.GearManager.Instance.GetAllGearData_Dic();

                            foreach (var pair in boxDatas)
                            {
                                Managers.GearManager.Instance.UseSynergyAmount(pair.Value.Index, 1);
                            }

                            break;
                        }
                    case V2Enum_Goods.Point:
                        {
                            for (int i = (int)V2Enum_Point.InGameGold; i < (int)V2Enum_Point.Max; ++i)
                            {
                                Managers.GoodsManager.Instance.SetGoodsAmount(goods, i, 0.0);
                            }

                            break;
                        }
                    case V2Enum_Goods.Ally:
                        {
                            //Managers.AllyV3Manager.Instance.CheatResetAllyData();

                            break;
                        }
                }
            }
        }
        //------------------------------------------------------------------------------------
#endif

    }
}