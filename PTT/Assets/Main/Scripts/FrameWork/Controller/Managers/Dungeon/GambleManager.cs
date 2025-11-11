using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class GambleOperationData
    {
        public OperationProbData OperationProbData;

        public ObscuredDouble GambleProb;
    }

    public class GambleManager : MonoSingleton<GambleManager>
    {
        private ObscuredDouble _tempLuckStat = 1000;

        private ARR_CardGambleData[] _aRR_CardGambleData = new ARR_CardGambleData[Define.DisplayGambleSkillCount];

        private List<GambleOperationData> _gambleCardDatas = new List<GambleOperationData>();
        private WeightedRandomPicker<GambleOperationData> _gambleCardPicker = new WeightedRandomPicker<GambleOperationData>();
        private WeightedRandomPicker<GambleOperationData> _gambleCardPicker_Helper = new WeightedRandomPicker<GambleOperationData>();


        private List<GambleOperationData> _gambleGradeDatas = new List<GambleOperationData>();
        private WeightedRandomPicker<GambleOperationData> _gambleGradePicker = new WeightedRandomPicker<GambleOperationData>();

        private List<GambleOperationData> _gambleSlotStatDatas = new List<GambleOperationData>();
        private WeightedRandomPicker<GambleOperationData> _gambleSlotStatPicker = new WeightedRandomPicker<GambleOperationData>();

        private List<GambleOperationData> _gambleSlotValueDatas = new List<GambleOperationData>();
        private WeightedRandomPicker<GambleOperationData> _gambleSlotValuePicker = new WeightedRandomPicker<GambleOperationData>();


        private Dictionary<V2Enum_ARR_GambleType, int> _gambleActionCount = new Dictionary<V2Enum_ARR_GambleType, int>();

        private System.Random _random = new System.Random();

        private Event.PlayGambleCardMsg _playGambleCardMsg = new Event.PlayGambleCardMsg();
        private Event.PlayGasSynergyMsg _playGasSynergyMsg = new Event.PlayGasSynergyMsg();
        private Event.PlayMinorJokerSynergyMsg _playMinorJokerSynergyMsg = new Event.PlayMinorJokerSynergyMsg();
        private Event.RefreshGambleAutoTriggerMsg _refreshGambleAutoTriggerMsg = new Event.RefreshGambleAutoTriggerMsg();

        public List<V2Enum_ARR_SynergyType> AutoGambleOrder = new List<V2Enum_ARR_SynergyType>();

        private bool _synergyGambleAuto = false;
        public bool SynergyGambleAuto { get { return _synergyGambleAuto; } }

        public float AutoSelectDelay = 0.5f;
        public bool SetStopAllMax = false;
        public bool PlayedAutoGamble = false;
        public float nextPlayTime = 0.0f;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            string autoGambleKey = PlayerPrefs.GetString(Define.AutoGambleKey);
            if (string.IsNullOrEmpty(autoGambleKey) == false)
            {
                string[] arr = autoGambleKey.Split(',');

                for (int i = 0; i < arr.Length; ++i)
                {
                    V2Enum_ARR_SynergyType index = arr[i].ToInt().IntToEnum32<V2Enum_ARR_SynergyType>();
                    AutoGambleOrder.Add(index);
                }
            }
            else
            {
                AutoGambleOrder.Add(V2Enum_ARR_SynergyType.Red);
                AutoGambleOrder.Add(V2Enum_ARR_SynergyType.Blue);
                AutoGambleOrder.Add(V2Enum_ARR_SynergyType.White);
                AutoGambleOrder.Add(V2Enum_ARR_SynergyType.Yellow);
            }

            int autoMaxStopKey = PlayerPrefs.GetInt(Define.AutoMaxStopKey, -1);
            if (autoMaxStopKey != -1)
            {
                SetStopAllMax = autoMaxStopKey == 1;
            }
            else
            {
                SetStopAllMax = true;
            }

            GambleOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitGambleContent()
        {
            for (int i = V2Enum_ARR_GambleType.Card.Enum32ToInt(); i < V2Enum_ARR_GambleType.Max.Enum32ToInt(); ++i)
            {
                V2Enum_ARR_GambleType v2Enum_ARR_GambleType = i.IntToEnum32<V2Enum_ARR_GambleType>();

                if (_gambleActionCount.ContainsKey(v2Enum_ARR_GambleType) == false)
                    _gambleActionCount.Add(v2Enum_ARR_GambleType, 0);
            }

            ObscuredDouble luckStat = GetLuckStat();


            {
                List<GambleCardData> gambleProbDatas = GambleOperator.GetGambleCardProbDatas();

                for (int i = 0; i < gambleProbDatas.Count; ++i)
                {
                    GambleCardData gambleProbData = gambleProbDatas[i];

                    GambleOperationData gambleOperationData = new GambleOperationData();

                    gambleOperationData.OperationProbData = gambleProbData;
                    gambleOperationData.GambleProb = gambleProbData.GambleProb + (gambleProbData.GambleProbWeight * luckStat);

                    _gambleCardDatas.Add(gambleOperationData);
                    _gambleCardPicker.Add(gambleOperationData, gambleOperationData.GambleProb);
                }
            }


            Dictionary<V2Enum_ARR_GambleSlotType, List<GambleSlotProbData>> gambleSlotProbData_Dic = GambleOperator.GetGambleSlotProbData_Dic();

            foreach (var pair in gambleSlotProbData_Dic)
            {
                List<GambleSlotProbData> gambleProbDatas = pair.Value;

                List<GambleOperationData> gambleSlotDatas = null;
                WeightedRandomPicker<GambleOperationData> gambleSlotPicker = null;

                if (pair.Key == V2Enum_ARR_GambleSlotType.SlotStat)
                {
                    gambleSlotDatas = _gambleSlotStatDatas;
                    gambleSlotPicker = _gambleSlotStatPicker;
                }
                else
                {
                    gambleSlotDatas = _gambleSlotValueDatas;
                    gambleSlotPicker = _gambleSlotValuePicker;
                }

                for (int i = 0; i < gambleProbDatas.Count; ++i)
                {
                    GambleSlotProbData gambleProbData = gambleProbDatas[i];

                    GambleOperationData gambleOperationData = new GambleOperationData();

                    gambleOperationData.OperationProbData = gambleProbData;
                    gambleOperationData.GambleProb = gambleProbData.GambleProb + (gambleProbData.GambleProbWeight * luckStat);

                    gambleSlotDatas.Add(gambleOperationData);
                    gambleSlotPicker.Add(gambleOperationData, gambleOperationData.GambleProb);
                }
            }
        }
        //------------------------------------------------------------------------------------

        public int _cheat_GambleGradeLevel = -1;

        public void SetInGameData()
        {
            return;

            ObscuredDouble luckStat = GetLuckStat();

            _gambleGradeDatas.Clear();
            _gambleGradePicker.Clear();

            {
                int gambleLevel = Managers.SynergyManager.Instance.GambleGradeLevel;

                List<GambleGradeProbData> gambleProbDatas = GambleOperator.GetGambleProbDatas(SynergyManager.Instance.GambleGradeLevel);

                if (_cheat_GambleGradeLevel != -1)
                {
                    gambleProbDatas = GambleOperator.GetGambleProbDatas(_cheat_GambleGradeLevel);
                }

                for (int i = 0; i < gambleProbDatas.Count; ++i)
                {
                    GambleGradeProbData gambleProbData = gambleProbDatas[i];

                    GambleOperationData gambleOperationData = new GambleOperationData();

                    gambleOperationData.OperationProbData = gambleProbData;
                    gambleOperationData.GambleProb = gambleProbData.GambleProb + (gambleProbData.GambleProbWeight * luckStat);

                    _gambleGradeDatas.Add(gambleOperationData);
                    _gambleGradePicker.Add(gambleOperationData, gambleOperationData.GambleProb);
                }
            }


        }
        public void ResetGambleState()
        {
            for (int i = V2Enum_ARR_GambleType.Card.Enum32ToInt(); i < V2Enum_ARR_GambleType.Max.Enum32ToInt(); ++i)
            {
                V2Enum_ARR_GambleType v2Enum_ARR_GambleType = i.IntToEnum32<V2Enum_ARR_GambleType>();

                if (_gambleActionCount.ContainsKey(v2Enum_ARR_GambleType) == true)
                    _gambleActionCount[v2Enum_ARR_GambleType] = 0;
            }
        }
        //------------------------------------------------------------------------------------
        #region Luck
        //------------------------------------------------------------------------------------
        public ObscuredDouble GetLuckStat()
        {
            return _tempLuckStat;
        }
        //------------------------------------------------------------------------------------
        public void RefreshLuckyStat()
        {
            RefreshPicker(_gambleCardDatas, _gambleCardPicker);
            RefreshPicker(_gambleGradeDatas, _gambleGradePicker);
            RefreshPicker(_gambleSlotStatDatas, _gambleSlotStatPicker);
            RefreshPicker(_gambleSlotValueDatas, _gambleSlotValuePicker);
        }
        //------------------------------------------------------------------------------------
        public void RefreshPicker(List<GambleOperationData> dataList, WeightedRandomPicker<GambleOperationData> dataPicker)
        {
            ObscuredDouble luckStat = GetLuckStat();

            for (int i = 0; i < dataList.Count; ++i)
            {
                GambleOperationData gambleOperationData = dataList[i];
                OperationProbData gambleProbData = gambleOperationData.OperationProbData;

                gambleOperationData.GambleProb = gambleProbData.GambleProb + (gambleProbData.GambleProbWeight * luckStat);
                if (gambleOperationData.GambleProb < 0)
                    gambleOperationData.GambleProb = 0;
            }

            dataPicker.RefreshTotalWeight();
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region CardGamble

        //------------------------------------------------------------------------------------
        public void SetAutoGamble(bool auto)
        {
            _synergyGambleAuto = auto;
            Message.Send(_refreshGambleAutoTriggerMsg);
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (_synergyGambleAuto == true)
            {
                if (nextPlayTime < Time.time)
                    PlayAutoGamble();
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayAutoGamble()
        {
            if (Managers.BattleSceneManager.Instance.CurrentBattleScene.IsPlay == false)
                return;

            if (Managers.BattleSceneManager.Instance.BattleType != V2Enum_Dungeon.StageScene)
                return;

            if (_synergyGambleAuto == false)
                return;

            if (PlayedAutoGamble == true)
                return;


            double currentGold = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGold.Enum32ToInt());

            double needcost = GetCost(V2Enum_ARR_GambleType.Card);

            if (needcost > currentGold)
            {
                
                return;
            }

            PlayedAutoGamble = true;

            nextPlayTime = Time.time + AutoSelectDelay;

            PlayCardGamble();
        }
        //------------------------------------------------------------------------------------
        public void FinishAutoGamble()
        {
            if (SetStopAllMax == true)
            {
                bool allMax = true;
                for (int i = 0; i < AutoGambleOrder.Count; ++i)
                {
                    V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType = AutoGambleOrder[i];

                    if (v2Enum_ARR_SynergyType == V2Enum_ARR_SynergyType.Yellow)
                    {
                        if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockGoldSynergy) == false)
                        {
                            continue;
                        }
                    }
                    else if (v2Enum_ARR_SynergyType == V2Enum_ARR_SynergyType.White)
                    {
                        if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockThunderSynergy) == false)
                        {
                            continue;
                        }
                    }

                    if (Managers.SynergyManager.Instance.GetInGameSynergyUnlockTier(v2Enum_ARR_SynergyType) > Managers.SynergyManager.Instance.GetSynergyLevel(v2Enum_ARR_SynergyType))
                    {
                        allMax = false;
                    }
                }

                if (allMax == true)
                {
                    SetAutoGamble(false);
                }
            }

            PlayedAutoGamble = false;
            //PlayAutoGamble();
        }
        //------------------------------------------------------------------------------------
        public void PlayCardGamble()
        {
            if (Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial == true)
                return;

            _playGambleCardMsg.aRR_GambleResultData = GetGambleResult();
            Message.Send(_playGambleCardMsg);
        }
        //------------------------------------------------------------------------------------
        public bool ShowGambleResultLog = true;
        //------------------------------------------------------------------------------------
        public V2Enum_ARR_SynergyType _cheat_GambleCard = V2Enum_ARR_SynergyType.Max;
        public V2Enum_Grade _cheat_GambleGrade = V2Enum_Grade.Max;

        public bool _cheat_ShowJoker = false;
        //------------------------------------------------------------------------------------
        public ARR_CardGambleResultData GetGambleResult()
        {
            ARR_CardGambleResultData aRR_CardGambleResultData = new ARR_CardGambleResultData();

            List<MainSkillData> finalSkillData = new List<MainSkillData>();

            string GambleResultLog = string.Empty;

            if (ShowGambleResultLog)
            {
                GambleResultLog += string.Format("\nfinalSkillData : ");
            }

            _gambleCardPicker_Helper.Clear();

            for (int i = 0; i < _gambleCardDatas.Count; ++i)
            {
                GambleOperationData gambleOperationData = _gambleCardDatas[i];
                GambleCardData gambleCardProbData = gambleOperationData.OperationProbData as GambleCardData;
                if (gambleCardProbData.SynergyType == V2Enum_ARR_SynergyType.Yellow)
                {
                    if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockGoldSynergy) == false)
                        continue;
                }
                else if (gambleCardProbData.SynergyType == V2Enum_ARR_SynergyType.White)
                {
                    if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockThunderSynergy) == false)
                        continue;
                }

                _gambleCardPicker_Helper.Add(_gambleCardDatas[i], _gambleCardDatas[i].GambleProb);
            }

            int addGrade = Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.GetIncreaseSynergyCount();

            bool toturedshow = false;

            for (int i = 0; i < Define.DisplayGambleSkillCount; ++i)
            {
                GambleOperationData gambleOperationData = null;

                V2Enum_ARR_SynergyType selectSynergy = V2Enum_ARR_SynergyType.Max;

                ObscuredDouble totalWeight = 1000000;
                double select = _random.NextDouble() * totalWeight;


                if (SynergyManager.Instance.UnLockJoker == false)
                {
                    gambleOperationData = _gambleCardPicker_Helper.Pick();
                    GambleCardData gambleCardProbData = gambleOperationData.OperationProbData as GambleCardData;
                    selectSynergy = gambleCardProbData.SynergyType;

                    _gambleCardPicker_Helper.Remove(gambleOperationData, gambleOperationData.GambleProb);
                    _gambleCardPicker_Helper.RefreshTotalWeight();
                }
                else
                {
                    if (select > Define.JokerCardProb + Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.GetIncreaseJokerProb())
                    {
                        gambleOperationData = _gambleCardPicker_Helper.Pick();
                        GambleCardData gambleCardProbData = gambleOperationData.OperationProbData as GambleCardData;
                        selectSynergy = gambleCardProbData.SynergyType;

                        _gambleCardPicker_Helper.Remove(gambleOperationData, gambleOperationData.GambleProb);
                        _gambleCardPicker_Helper.RefreshTotalWeight();
                    }
                }

                V2Enum_Grade resultGrade = V2Enum_Grade.Max;

                if (selectSynergy != V2Enum_ARR_SynergyType.Max)
                {
                    WeightedRandomPicker<GambleGradeProbData> weightedRandomPicker = GambleOperator.GetGambleGradePicker(Managers.ResearchManager.Instance.GetSynergyGambleLevel(selectSynergy));
                    GambleGradeProbData pick = weightedRandomPicker.Pick();
                    resultGrade = pick.CardGambleGrade + addGrade;
                }
                
                if (_cheat_GambleGrade != V2Enum_Grade.Max)
                    resultGrade = _cheat_GambleGrade;

                if (_cheat_ShowJoker == true)
                    selectSynergy = V2Enum_ARR_SynergyType.Max;

                if (Managers.MapManager.Instance.NeedTutotial1())
                {
                    if (_gambleActionCount[V2Enum_ARR_GambleType.Card] == 0)
                    {
                        if (i == 0)
                        {
                            resultGrade = V2Enum_Grade.D;
                            selectSynergy = V2Enum_ARR_SynergyType.Red;
                        }
                        else if (i == 1)
                        {
                            resultGrade = V2Enum_Grade.D;
                            selectSynergy = V2Enum_ARR_SynergyType.Blue;
                        }

                        Managers.GuideInteractorManager.Instance.PlayCardTutorial = true;

                        ThirdPartyLog.Instance.SendLog_log_tutorial(0);

                    }
                    else if (_gambleActionCount[V2Enum_ARR_GambleType.Card] == 1)
                    {
                        if (i == 0)
                        {
                            resultGrade = V2Enum_Grade.D;
                            selectSynergy = V2Enum_ARR_SynergyType.Red;
                        }
                        else if (i == 1)
                        {
                            resultGrade = V2Enum_Grade.D;
                            selectSynergy = V2Enum_ARR_SynergyType.Blue;
                        }

                        //Managers.GuideInteractorManager.Instance.PlayCardTutorial = true;
                    }
                    //else if (_gambleActionCount[V2Enum_ARR_GambleType.Card] == 2)
                    //{
                    //    Managers.GuideInteractorManager.Instance.PlayJokerTutorial = true;

                    //    selectSynergy = V2Enum_ARR_SynergyType.Max;
                    //}
                }
                else if(selectSynergy == V2Enum_ARR_SynergyType.Max)
                {
                    ThirdPartyLog.Instance.SendLog_log_dungeon_joker(0);
                }


                if (_cheat_GambleCard != V2Enum_ARR_SynergyType.Max)
                    selectSynergy = _cheat_GambleCard;


                if (_aRR_CardGambleData[i] == null)
                    _aRR_CardGambleData[i] = new ARR_CardGambleData();

                if (MapContainer.PlayingStage == 1 && MapContainer.MapMaxClear == 0)
                {
                    if (_gambleActionCount[V2Enum_ARR_GambleType.Card] <= 2)
                    {
                        if (UnityEngine.Random.Range(0, 2) == 1)
                        {
                            resultGrade = V2Enum_Grade.C;
                        }
                    }
                }
                else if (MapContainer.PlayingStage == 2 && MapContainer.MapMaxClear == 1)
                {
                    if (_gambleActionCount[V2Enum_ARR_GambleType.Card] <= 2)
                    {
                        if (selectSynergy == V2Enum_ARR_SynergyType.Red)
                        {
                            if (UnityEngine.Random.Range(0, 2) == 1)
                            {
                                resultGrade = V2Enum_Grade.C;
                            }
                        }
                    }
                }

                _aRR_CardGambleData[i].SynergyGrade = resultGrade;
                _aRR_CardGambleData[i].SynergyStack = resultGrade.Enum32ToInt() - 10;
                _aRR_CardGambleData[i].SynergyType = selectSynergy;

                
            }

            aRR_CardGambleResultData.FinalSkillData = _aRR_CardGambleData;

            double needcost = GetCost(V2Enum_ARR_GambleType.Card);

            Managers.GoodsManager.Instance.UseGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGold.Enum32ToInt(), needcost);

            if (ShowGambleResultLog)
            {
                Debug.Log(GambleResultLog);
            }

            _gambleActionCount[V2Enum_ARR_GambleType.Card]++;

            ThirdPartyLog.Instance.SendLog_Card(MapContainer.MapLastEnter);

            Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.RefreshUseGoldAmount(needcost);
            Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.RefreshInterestAmount(Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt()));

            return aRR_CardGambleResultData;
        }
        //------------------------------------------------------------------------------------
        public bool CanUpGradeCardGambleResult(V2Enum_Grade v2Enum_ARR_GambleSlotGrade)
        {
            double chance = GetCardGambleProbChance(v2Enum_ARR_GambleSlotGrade);

            ObscuredDouble totalWeight = 10000;
            double select = _random.NextDouble() * totalWeight;

            if (select < chance)
                return true;

            return false;
        }
        //------------------------------------------------------------------------------------
        public double GetCardGambleProbChance(V2Enum_Grade v2Enum_ARR_GambleSlotGrade)
        {
            GambleProbChanceData gambleProbChanceData = GambleOperator.GetGambleProbChanceData();

            Dictionary<V2Enum_Grade, ObscuredDouble> gradeChance = gambleProbChanceData.CardGradeChance;

            if (gradeChance.ContainsKey(v2Enum_ARR_GambleSlotGrade) == false)
                return 0.0;

            int count = _gambleActionCount[V2Enum_ARR_GambleType.Reinforcement];

            double chance = gradeChance[v2Enum_ARR_GambleSlotGrade];

            return chance + (chance * count);
        }
        //------------------------------------------------------------------------------------
        public void AddGambleSkill(ARR_CardGambleData gambleSkillData, ref int descendEnhance)
        {
            if (gambleSkillData == null)
                return;

            //Managers.BattleSceneManager.Instance.AddGambleSkill(gambleSkillData);
            SynergyManager.Instance.AddGambleSynergy(gambleSkillData.SynergyType, gambleSkillData.SynergyStack, ref descendEnhance);
            Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.SkillActiveController.AddGambleCardRewardCount(1, gambleSkillData.SynergyType);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region SlotGamble
        //------------------------------------------------------------------------------------
        public GambleSlotIncreaseValueData PlayGambleSlot()
        {
            GambleOperationData stat = _gambleSlotStatPicker.Pick();
            GambleOperationData value = _gambleSlotValuePicker.Pick();

            if (stat == null || value == null)
                return null;

            GambleSlotProbData statData = stat.OperationProbData as GambleSlotProbData;
            GambleSlotProbData valueData = value.OperationProbData as GambleSlotProbData;


            List<GambleSlotIncreaseValueData> resultList = GambleOperator.GetGambleSlotIncreaseValueDatas(statData.Index);

            V2Enum_ARR_GambleSlotGrade fakeGrade = valueData.GambleSlotGrade;
            V2Enum_ARR_GambleSlotGrade resultGrade = fakeGrade;

            if (Managers.MapManager.Instance.NeedTutotial1() == true && _gambleActionCount[V2Enum_ARR_GambleType.Slot] == 0)
            {
                resultList = GambleOperator.GetGambleSlotIncreaseValueDatas(106060001);

                fakeGrade = V2Enum_ARR_GambleSlotGrade.Six;
                resultGrade = V2Enum_ARR_GambleSlotGrade.Six;
            }

            if (Managers.MapManager.Instance.Stage1TryCount > 3)
            {
                fakeGrade = UnityEngine.Random.Range((int)V2Enum_ARR_GambleSlotGrade.Four, (int)V2Enum_ARR_GambleSlotGrade.Max).IntToEnum32<V2Enum_ARR_GambleSlotGrade>();
                resultGrade = fakeGrade;
            }

            bool gradeUp = CanUpGradeSlotGambleResult(valueData.GambleSlotGrade);
            if (gradeUp == true)
            {
                resultGrade = (fakeGrade.Enum32ToInt() + 1).IntToEnum32<V2Enum_ARR_GambleSlotGrade>();
            }

            if (resultGrade == V2Enum_ARR_GambleSlotGrade.Max)
                resultGrade = V2Enum_ARR_GambleSlotGrade.Six;

            GambleSlotIncreaseValueData result = resultList.Find(x => x.GambleSlotGrade == resultGrade);
            if(result == null)
            {
                Debug.Log("sdfsdf");
            }
            result.FakeData = resultList.Find(x => x.GambleSlotGrade == fakeGrade);

            if (result != null)
            {
                Managers.BattleSceneManager.Instance.AddMyARRRBuff(result.BaseStat, result.BaseStatIncreaseValue);
            }

            //double needcost = Managers.GambleManager.Instance.GetCost(V2Enum_ARR_GambleType.Card);

            //Managers.GoodsManager.Instance.UseGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.Gold.Enum32ToInt(), needcost);

            _gambleActionCount[V2Enum_ARR_GambleType.Slot]++;

            ThirdPartyLog.Instance.SendLog_Slot(MapContainer.MapLastEnter);

            return result;
        }
        //------------------------------------------------------------------------------------
        public bool CanUpGradeSlotGambleResult(V2Enum_ARR_GambleSlotGrade v2Enum_ARR_GambleSlotGrade)
        {
            double chance = GetSlotGambleProbChance(v2Enum_ARR_GambleSlotGrade);

            ObscuredDouble totalWeight = 10000;
            double select = _random.NextDouble() * totalWeight;

            if (select < chance)
                return true;

            return false;
        }
        //------------------------------------------------------------------------------------
        public double GetSlotGambleProbChance(V2Enum_ARR_GambleSlotGrade v2Enum_ARR_GambleSlotGrade)
        {
            GambleProbChanceData gambleProbChanceData = GambleOperator.GetGambleProbChanceData();

            Dictionary<V2Enum_ARR_GambleSlotGrade, ObscuredDouble> gradeChance = gambleProbChanceData.SlotGradeChance;

            if (gradeChance.ContainsKey(v2Enum_ARR_GambleSlotGrade) == false)
                return 0.0;

            int count = _gambleActionCount[V2Enum_ARR_GambleType.Reinforcement];

            double chance = gradeChance[v2Enum_ARR_GambleSlotGrade];

            return chance + (chance * count);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region ReinforcementGamble
        //------------------------------------------------------------------------------------
        public bool DoReinforcementLevelUp()
        {
            double needcost = Managers.GambleManager.Instance.GetCost(V2Enum_ARR_GambleType.Reinforcement);

            Managers.GoodsManager.Instance.UseGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGold.Enum32ToInt(), needcost);

            _gambleActionCount[V2Enum_ARR_GambleType.Reinforcement]++;

            ThirdPartyLog.Instance.SendLog_LuckyUp(MapContainer.MapLastEnter);

            return false;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //-----------------------------------------------------------------------------------
        public bool IsMaxCount(V2Enum_ARR_GambleType v2Enum_ARR_GambleType)
        {
            if (_gambleActionCount.ContainsKey(v2Enum_ARR_GambleType) == false)
                return true;

            int count = _gambleActionCount[v2Enum_ARR_GambleType];
            GambleCostData gambleCostData = GambleOperator.GetGambleCostData(v2Enum_ARR_GambleType);
            if (gambleCostData.MaxLevel == -1)
                return false;

            return count >= gambleCostData.MaxLevel;
        }
        //-----------------------------------------------------------------------------------
        public int GetGambleActionCount(V2Enum_ARR_GambleType v2Enum_ARR_GambleType)
        {
            if (_gambleActionCount.ContainsKey(v2Enum_ARR_GambleType) == false)
                return 0;

            return _gambleActionCount[v2Enum_ARR_GambleType];
        }
        //-----------------------------------------------------------------------------------
        public double GetCost(V2Enum_ARR_GambleType v2Enum_ARR_GambleType)
        {
            int count = _gambleActionCount[v2Enum_ARR_GambleType];
            GambleCostData gambleCostData = GambleOperator.GetGambleCostData(v2Enum_ARR_GambleType);

            double cost = gambleCostData.BaseCost + (gambleCostData.CostIncrease * count);

            if (v2Enum_ARR_GambleType == V2Enum_ARR_GambleType.Card)
            {
                if (Managers.BattleSceneManager.isAlive == true && Managers.BattleSceneManager.Instance.CurrentBattleScene != null && Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers != null)
                {
                    cost -= cost * Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.GetDecreaseGamblePrice();
                    cost = Math.Ceiling(cost);
                }
            }

            return cost;
        }
        //------------------------------------------------------------------------------------
        public List<V2Enum_Stat> GetDisplayStatList()
        {
            return GambleOperator.GetDisplayStatList();
        }
        //------------------------------------------------------------------------------------
        #region Gas
        //------------------------------------------------------------------------------------
        public double GetGasSynergyPercent()
        {
            return Define.GasSynergyProb + Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.GetIncreaseGasGambleProb() + (Managers.ResearchManager.Instance.GetResearchValues(V2Enum_ResearchType.GasGambleIncrease) * 100);
        }
        //------------------------------------------------------------------------------------
        public bool CanPlayGasSynergy()
        {
            double prob = GetGasSynergyPercent();

            if (prob >= 100)
                return true;

            double chance = prob;

            ObscuredDouble totalWeight = 100;
            double select = _random.NextDouble() * totalWeight;

            if (select < chance)
                return true;

            return false;
        }
        //------------------------------------------------------------------------------------
        public double GetGasHpRecoveryRatio()
        {
            return Define.GasHpRecovery + Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.GetIncreaseGasHpHeal() + (Managers.ResearchManager.Instance.GetResearchValues(V2Enum_ResearchType.GasHpIncrease) * 100);
        }
        //------------------------------------------------------------------------------------
        public void PlayGasHp()
        {
            //List<int> used_type = new List<int>();
            //List<double> former_quan = new List<double>();
            //List<double> used_quan = new List<double>();
            //List<double> keep_quan = new List<double>();

            //used_type.Add(V2Enum_Point.InGameGas.Enum32ToInt());
            //former_quan.Add(GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGas.Enum32ToInt()));
            //used_quan.Add(1);

            Managers.GoodsManager.Instance.UseGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGas.Enum32ToInt(), 1);

            //keep_quan.Add(GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGas.Enum32ToInt()));

            Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.HPRecoverPer(GetGasHpRecoveryRatio());

            ThirdPartyLog.Instance.SendLog_log_dungeon_pearl(1);
        }
        //------------------------------------------------------------------------------------
        public void PlayGasSynergy()
        {
            Message.Send(_playGasSynergyMsg);

            //List<int> used_type = new List<int>();
            //List<double> former_quan = new List<double>();
            //List<double> used_quan = new List<double>();
            //List<double> keep_quan = new List<double>();

            double current = GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGas.Enum32ToInt());

            //used_type.Add(V2Enum_Point.InGameGas.Enum32ToInt());
            //former_quan.Add(current + 1);
            //used_quan.Add(1);
            //keep_quan.Add(current + 1);

            ThirdPartyLog.Instance.SendLog_log_dungeon_pearl(0);

            ThirdPartyLog.Instance.SendLog_log_dungeon_joker(1);
        }
        //------------------------------------------------------------------------------------
        #endregion
        #region MinorJoker
        //------------------------------------------------------------------------------------
        public void PlayMinorJokerSynergy(ObscuredInt synergyStack)
        {
            _playMinorJokerSynergyMsg.SynergyStack = synergyStack;
            Message.Send(_playMinorJokerSynergyMsg);
        }
        //------------------------------------------------------------------------------------
        #endregion
        
    }
}