using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Linq;

namespace GameBerry.UI
{
    public class CheatDialog : IDialog
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

        [Header("------------SkillGroup------------")]
        [SerializeField]
        private TMP_Dropdown m_cheatSkillIndexDropdown;

        [SerializeField]
        private Button m_cheatOpenSkillBtn;


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

        private Enum_ItemType m_cheat_V2Enum_Goods = Enum_ItemType.Max;

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

                    for (int i = (int)Enum_ItemType.Potion; i < (int)Enum_ItemType.Max; ++i)
                    {
                        optiondatalabel.Add(((Enum_ItemType)i).ToString());
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


                if (m_cheatSkillIndexDropdown != null)
                {
                    m_cheatSkillIndexDropdown.ClearOptions();

                    Chart.SkillChart itemChart = Chart.GameChart.Get<Chart.SkillChart>();

                    List<TMP_Dropdown.OptionData> optiondatalabel = new List<TMP_Dropdown.OptionData>();

                    for (int i = 0; i < itemChart.rows.Length; ++i)
                    {
                        TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                        optionData.text = itemChart.rows[i].SkillId.ToString();
                        optionData.image = SkillManager.Instance.GetIcon(itemChart.rows[i].SkillId);
                        optiondatalabel.Add(optionData);
                    }

                    m_cheatSkillIndexDropdown.AddOptions(optiondatalabel);
                }

                if (m_cheatOpenSkillBtn != null)
                    m_cheatOpenSkillBtn.onClick.AddListener(OnClick_CheatOpenSkillBtn);

                if (m_maxStageInputField != null)
                    m_maxStageInputField.contentType = TMP_InputField.ContentType.IntegerNumber;

                if (m_maxStageApplyBtn != null)
                    m_maxStageApplyBtn.onClick.AddListener(OnClick_SetMaxStage);


                if (m_setStageRewardInputField != null)
                    m_setStageRewardInputField.contentType = TMP_InputField.ContentType.IntegerNumber;


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
                    m_showLogBtn.onClick.AddListener(() =>
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
                        Table.UserTable.DeleteAllTable();
                    });

                if (m_initDailyContentBtn != null)
                    m_initDailyContentBtn.onClick.AddListener(OnClick_InitDailyContentBtn);

                if (m_initWeekContentBtn != null)
                    m_initWeekContentBtn.onClick.AddListener(OnClick_InitWeekContentBtn);

                if (m_initMonthContentBtn != null)
                    m_initMonthContentBtn.onClick.AddListener(OnClick_InitMonthContentBtn);


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
        private void OnValueChange_CheatGoodsID(int value)
        {
            int goodsvalue = m_cheatGoodsIDDropdown.value + (int)Enum_ItemType.Potion;
            m_cheatGoodsIndexDropdown.ClearOptions();
            m_cheat_V2Enum_Goods = (Enum_ItemType)goodsvalue;

            List<TMP_Dropdown.OptionData> optiondatalabel = new List<TMP_Dropdown.OptionData>();

            Chart.ItemChart itemChart = Chart.GameChart.Get<Chart.ItemChart>();

            var query = from data in itemChart.rows
                        where data.ItemType == m_cheat_V2Enum_Goods
                        orderby data.ItemId
                        select data;

            foreach (var pair in query)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = pair.ItemId.ToString();
                optionData.image = ItemManager.Instance.GetIcon(pair.ItemId);
                optiondatalabel.Add(optionData);
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

            int goodsvalue = m_cheatGoodsIDDropdown.value + (int)Enum_ItemType.Potion;

            int itemid = m_cheatGoodsIndexDropdown.value;

            Chart.ItemChart itemChart = Chart.GameChart.Get<Chart.ItemChart>();

            var query = from data in itemChart.rows
                        where data.ItemType == m_cheat_V2Enum_Goods
                        orderby data.ItemId
                        select data;

            Chart.ItemInfo itemInfo = query.ToList()[itemid];

            long itemamount = m_cheatGoodsAmountInputField.text.ToLong();

            ItemManager.Instance.AddItem(itemInfo.ItemId, itemamount);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_CheatOpenSkillBtn()
        {
            if (m_cheatSkillIndexDropdown == null)
                return;

            int idx = m_cheatSkillIndexDropdown.value;

            Chart.SkillChart skillChart = Chart.GameChart.Get<Chart.SkillChart>();
            Chart.SkillInfo itemInfo = skillChart.rows[idx];

            SkillManager.Instance.UnlockSkill(itemInfo.SkillId, 99, 999);
        }
        //------------------------------------------------------------------------------------
        private void OnValueChanged_AdFreeMode(bool value)
        {
            Define.IsAdFree = value;
        }
        //------------------------------------------------------------------------------------
        private void OnValueChange_CheatTime(string str)
        {
            Managers.TimeManager.Instance.CheatTime = str.ToDouble();
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
#endif

    }
}