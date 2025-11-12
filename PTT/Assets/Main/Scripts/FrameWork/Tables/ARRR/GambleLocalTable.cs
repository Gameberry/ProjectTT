using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using LitJson;

namespace GameBerry
{
    public class GambleCardData : OperationProbData
    {
        public ObscuredInt Index;

        public ObscuredInt ResourceIndex;

        public Enum_SynergyType SynergyType;
    }

    public class OperationProbData
    {
        public ObscuredDouble GambleProb;
        public ObscuredDouble GambleProbWeight;
    }

    public class GambleGradeProbData : OperationProbData
    {
        public ObscuredInt Index;

        public V2Enum_Grade CardGambleGrade;
    }

    public class GambleSkillProbData
    {
        public ObscuredInt Index;

        public Enum_SynergyType SynergyType;

        public V2Enum_Grade GambleGrade;

        public ObscuredInt GambleResultSkill;

        public ObscuredDouble GamblResultProb;
    }

    public class GambleSlotData
    {
        public ObscuredInt Index;

        public ObscuredInt ResourceIndex;

        public Enum_GambleSlotType SlotType;
    }

    public class GambleSlotProbData : OperationProbData
    {
        public ObscuredInt Index;

        public ObscuredInt ResourceIndex;

        public Enum_GambleSlotType SlotType;
        public Enum_GambleSlotGrade GambleSlotGrade;
    }


    public class GambleSlotIncreaseValueData
    {
        public ObscuredInt Index;

        public ObscuredInt StatSlotIndex;

        public Enum_GambleSlotGrade GambleSlotGrade;
        public GambleSlotIncreaseValueData FakeData; // 연출용 데이터

        public V2Enum_Stat BaseStat;

        public ObscuredDouble BaseStatIncreaseValue;
    }

    public class GambleProbChanceData
    {
        public Dictionary<V2Enum_Grade, ObscuredDouble> CardGradeChance = new Dictionary<V2Enum_Grade, ObscuredDouble>();
        public Dictionary<Enum_GambleSlotGrade, ObscuredDouble> SlotGradeChance = new Dictionary<Enum_GambleSlotGrade, ObscuredDouble>();
    }

    public class GambleCostData
    {
        public ObscuredInt Index;

        public Enum_GambleType GambleType;

        public ObscuredDouble BaseCost;
        public ObscuredDouble CostIncrease;

        public ObscuredInt MaxLevel;
    }






    public class GambleLocalTable : LocalTableBase
    {
        private List<GambleCardData> _gambleCardDatas = new List<GambleCardData>();
        private Dictionary<ObscuredInt, List<GambleGradeProbData>> _gambleProbDatas_Dic = new Dictionary<ObscuredInt, List<GambleGradeProbData>>();
        private List<GambleGradeProbData> _gambleProbDatas = new List<GambleGradeProbData>();
        private Dictionary<V2Enum_Grade, List<GambleSkillProbData>> _gambleSkillProbData_Dic = new Dictionary<V2Enum_Grade, List<GambleSkillProbData>>();

        private Dictionary<ObscuredInt, WeightedRandomPicker<GambleGradeProbData>> _gambleGradePicker_Dic = new Dictionary<ObscuredInt, WeightedRandomPicker<GambleGradeProbData>>();

        private Dictionary<Enum_SynergyType, Dictionary<V2Enum_Grade, List<GambleSkillProbData>>> _gambleSkillCardGradeProbData_Dic = new Dictionary<Enum_SynergyType, Dictionary<V2Enum_Grade, List<GambleSkillProbData>>>();



        private Dictionary<Enum_GambleSlotType, List<GambleSlotProbData>> _gambleSlotProbData_Dic = new Dictionary<Enum_GambleSlotType, List<GambleSlotProbData>>();

        private Dictionary<ObscuredInt, List<GambleSlotIncreaseValueData>> _gambleSlotIncreaseValueData_Dic = new Dictionary<ObscuredInt, List<GambleSlotIncreaseValueData>>();

        private List<V2Enum_Stat> _slotStatList = new List<V2Enum_Stat>();

        private GambleProbChanceData _gambleProbChanceData;

        private Dictionary<Enum_GambleType, GambleCostData> _gambleCost_Dic = new Dictionary<Enum_GambleType, GambleCostData>();

        public override async UniTask InitData_Async()
        {
            await LoadCardData();
        }
        //------------------------------------------------------------------------------------
        
        private async UniTask LoadCardData()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GambleCard", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                int index = rows[i]["Index"].ToString().ToInt();

                if (index == -1)
                    continue;

                GambleCardData gambleCardData = new GambleCardData();

                gambleCardData.Index = index;

                gambleCardData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                gambleCardData.SynergyType = rows[i]["SynergyType"].ToString().ToInt().IntToEnum32<Enum_SynergyType>();

                gambleCardData.GambleProb = rows[i]["CardGambleProb"].ToString().ToDouble();
                gambleCardData.GambleProbWeight = rows[i]["CardGambleWeight"].ToString().ToDouble();

                _gambleCardDatas.Add(gambleCardData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GambleGradeProb", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GambleGradeProbData gambleProbData = new GambleGradeProbData();

                gambleProbData.Index = rows[i]["Index"].ToString().ToInt();

                gambleProbData.CardGambleGrade = rows[i]["CardGambleGrade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                gambleProbData.GambleProb = rows[i]["GambleProb"].ToString().ToDouble();
                gambleProbData.GambleProbWeight = rows[i]["GambleProbWeight"].ToString().ToDouble();

                int level = rows[i]["CardLevel"].ToString().ToInt();

                if (_gambleProbDatas_Dic.ContainsKey(level) == false)
                    _gambleProbDatas_Dic.Add(level, new List<GambleGradeProbData>());

                _gambleProbDatas_Dic[level].Add(gambleProbData);

                _gambleProbDatas.Add(gambleProbData);

                if (_gambleGradePicker_Dic.ContainsKey(level) == false)
                    _gambleGradePicker_Dic.Add(level, new WeightedRandomPicker<GambleGradeProbData>());

                _gambleGradePicker_Dic[level].Add(gambleProbData, gambleProbData.GambleProb);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GambleSkillProb", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GambleSkillProbData gambleSkillProbData = new GambleSkillProbData();

                gambleSkillProbData.Index = rows[i]["Index"].ToString().ToInt();

                gambleSkillProbData.SynergyType = rows[i]["SynergyType"].ToString().ToInt().IntToEnum32<Enum_SynergyType>();

                gambleSkillProbData.GambleGrade = rows[i]["GambleGrade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                gambleSkillProbData.GambleResultSkill = rows[i]["GambleResultSkill"].ToString().ToInt();

                gambleSkillProbData.GamblResultProb = rows[i]["GamblResultProb"].ToString().ToDouble();


                List<GambleSkillProbData> weightedRandomPicker = null;

                if (_gambleSkillProbData_Dic.ContainsKey(gambleSkillProbData.GambleGrade) == true)
                    weightedRandomPicker = _gambleSkillProbData_Dic[gambleSkillProbData.GambleGrade];
                else
                {
                    weightedRandomPicker = new List<GambleSkillProbData>();
                    _gambleSkillProbData_Dic.Add(gambleSkillProbData.GambleGrade, weightedRandomPicker);
                }

                if (_gambleSkillCardGradeProbData_Dic.ContainsKey(gambleSkillProbData.SynergyType) == false)
                    _gambleSkillCardGradeProbData_Dic.Add(gambleSkillProbData.SynergyType, new Dictionary<V2Enum_Grade, List<GambleSkillProbData>>());

                if (_gambleSkillCardGradeProbData_Dic[gambleSkillProbData.SynergyType].ContainsKey(gambleSkillProbData.GambleGrade) == false)
                    _gambleSkillCardGradeProbData_Dic[gambleSkillProbData.SynergyType].Add(gambleSkillProbData.GambleGrade, new List<GambleSkillProbData>());

                _gambleSkillCardGradeProbData_Dic[gambleSkillProbData.SynergyType][gambleSkillProbData.GambleGrade].Add(gambleSkillProbData);





                weightedRandomPicker.Add(gambleSkillProbData);
            }


            




            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GambleSlotProb", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GambleSlotProbData gambleSlotProbData = new GambleSlotProbData();

                gambleSlotProbData.Index = rows[i]["Index"].ToString().ToInt();
                gambleSlotProbData.SlotType = rows[i]["SlotType"].ToString().ToInt().IntToEnum32<Enum_GambleSlotType>();
                gambleSlotProbData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();
                gambleSlotProbData.GambleSlotGrade = rows[i]["GambleSlotGrade"].ToString().ToInt().IntToEnum32<Enum_GambleSlotGrade>();

                gambleSlotProbData.GambleProb = rows[i]["GambleProb"].ToString().ToDouble();
                gambleSlotProbData.GambleProbWeight = rows[i]["GambleProbWeight"].ToString().ToDouble();

                if (_gambleSlotProbData_Dic.ContainsKey(gambleSlotProbData.SlotType) == false)
                    _gambleSlotProbData_Dic.Add(gambleSlotProbData.SlotType, new List<GambleSlotProbData>());

                _gambleSlotProbData_Dic[gambleSlotProbData.SlotType].Add(gambleSlotProbData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GambleSlotIncreaseValue", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GambleSlotIncreaseValueData gambleSlotIncreaseValueData = new GambleSlotIncreaseValueData();

                gambleSlotIncreaseValueData.Index = rows[i]["Index"].ToString().ToInt();
                gambleSlotIncreaseValueData.StatSlotIndex = rows[i]["StatSlotIndex"].ToString().ToInt();
                gambleSlotIncreaseValueData.GambleSlotGrade = rows[i]["GambleSlotGrade"].ToString().ToInt().IntToEnum32<Enum_GambleSlotGrade>();

                gambleSlotIncreaseValueData.BaseStat = rows[i]["BaseStat"].ToString().ToInt().IntToEnum32<V2Enum_Stat>();

                gambleSlotIncreaseValueData.BaseStatIncreaseValue = rows[i]["BaseStatIncreaseValue"].ToString().ToDouble() * Define.PerSkillEffectRecoverValue;

                if (_gambleSlotIncreaseValueData_Dic.ContainsKey(gambleSlotIncreaseValueData.StatSlotIndex) == false)
                {
                    _gambleSlotIncreaseValueData_Dic.Add(gambleSlotIncreaseValueData.StatSlotIndex, new List<GambleSlotIncreaseValueData>());
                }

                _gambleSlotIncreaseValueData_Dic[gambleSlotIncreaseValueData.StatSlotIndex].Add(gambleSlotIncreaseValueData);

                if (_slotStatList.Contains(gambleSlotIncreaseValueData.BaseStat) == false)
                    _slotStatList.Add(gambleSlotIncreaseValueData.BaseStat);
            }





            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GambleProbChance", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                _gambleProbChanceData = new GambleProbChanceData();

                for (int j = 11; j <= 14; ++j)
                {
                    V2Enum_Grade V2Enum_Grade = j.IntToEnum32<V2Enum_Grade>();

                    if (_gambleProbChanceData.CardGradeChance.ContainsKey(V2Enum_Grade) == true)
                        continue;

                    try
                    {
                        ObscuredDouble goodstype = rows[i][string.Format("Card{0}GradeChance", j)].ToString().ToDouble();

                        _gambleProbChanceData.CardGradeChance.Add(V2Enum_Grade, goodstype);
                    }
                    catch
                    {

                    }
                }

                for (int j = 11; j <= 15; ++j)
                {
                    Enum_GambleSlotGrade Enum_GambleSlotGrade = j.IntToEnum32<Enum_GambleSlotGrade>();

                    if (_gambleProbChanceData.SlotGradeChance.ContainsKey(Enum_GambleSlotGrade) == true)
                        continue;

                    try
                    {
                        ObscuredDouble goodstype = rows[i][string.Format("Slot{0}GradeChance", j)].ToString().ToDouble();

                        _gambleProbChanceData.SlotGradeChance.Add(Enum_GambleSlotGrade, goodstype);
                    }
                    catch
                    {

                    }
                }

                break;
            }




            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GambleCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GambleCostData gambleSlotIncreaseValueData = new GambleCostData();

                gambleSlotIncreaseValueData.Index = rows[i]["Index"].ToString().ToInt();
                gambleSlotIncreaseValueData.GambleType = rows[i]["GambleType"].ToString().ToInt().IntToEnum32<Enum_GambleType>();

                gambleSlotIncreaseValueData.BaseCost = rows[i]["BaseCost"].ToString().ToDouble();
                gambleSlotIncreaseValueData.CostIncrease = rows[i]["CostIncrease"].ToString().ToDouble();

                gambleSlotIncreaseValueData.MaxLevel = rows[i]["MaxLevel"].ToString().ToInt();

                if (_gambleCost_Dic.ContainsKey(gambleSlotIncreaseValueData.GambleType) == false)
                {
                    _gambleCost_Dic.Add(gambleSlotIncreaseValueData.GambleType, gambleSlotIncreaseValueData);
                }
            }

        }
        //------------------------------------------------------------------------------------
        public List<GambleCardData> GetGambleCardProbDatas()
        {
            return _gambleCardDatas;
        }
        //------------------------------------------------------------------------------------
        public List<GambleGradeProbData> GetGambleProbDatas()
        {
            return _gambleProbDatas;
        }
        //------------------------------------------------------------------------------------
        public List<GambleGradeProbData> GetGambleProbDatas(ObscuredInt level)
        {
            if (_gambleProbDatas_Dic.ContainsKey(level) == false)
                return _gambleProbDatas;

            return _gambleProbDatas_Dic[level];
        }
        //------------------------------------------------------------------------------------
        public WeightedRandomPicker<GambleGradeProbData> GetGambleGradePicker(ObscuredInt level)
        {
            if (_gambleGradePicker_Dic.ContainsKey(level) == true)
                return _gambleGradePicker_Dic[level];

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<GambleSkillProbData> GetGambleSkillProbs(V2Enum_Grade V2Enum_Grade)
        {
            if (_gambleSkillProbData_Dic.ContainsKey(V2Enum_Grade) == true)
                return _gambleSkillProbData_Dic[V2Enum_Grade];

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<GambleSkillProbData> GetGambleSkillProbs(Enum_SynergyType Enum_Card, V2Enum_Grade V2Enum_Grade)
        {
            if (_gambleSkillCardGradeProbData_Dic.ContainsKey(Enum_Card) == false)
                return null;

            if (_gambleSkillCardGradeProbData_Dic[Enum_Card].ContainsKey(V2Enum_Grade) == false)
                return null;

            return _gambleSkillCardGradeProbData_Dic[Enum_Card][V2Enum_Grade];
        }
        //------------------------------------------------------------------------------------
        public Dictionary<Enum_GambleSlotType, List<GambleSlotProbData>> GetGambleSlotProbData_Dic()
        {
            return _gambleSlotProbData_Dic;
        }
        //------------------------------------------------------------------------------------
        public List<GambleSlotIncreaseValueData> GetGambleSlotIncreaseValueDatas(ObscuredInt index)
        {
            if (_gambleSlotIncreaseValueData_Dic.ContainsKey(index) == true)
                return _gambleSlotIncreaseValueData_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<V2Enum_Stat> GetDisplayStatList()
        {
            return _slotStatList;
        }
        //------------------------------------------------------------------------------------
        public GambleProbChanceData GetGambleProbChanceData()
        {
            return _gambleProbChanceData;
        }
        //------------------------------------------------------------------------------------
        public GambleCostData GetGambleCostData(Enum_GambleType Enum_GambleType)
        {
            if (_gambleCost_Dic.ContainsKey(Enum_GambleType) == true)
                return _gambleCost_Dic[Enum_GambleType];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}