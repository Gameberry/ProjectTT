using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using LitJson;
using Newtonsoft.Json;

namespace GameBerry
{
    public class SynergyRuneData : Gpm.Ui.InfiniteScrollData
    {
        public ObscuredInt Index;

        public ObscuredInt ResourceIndex;

        public Enum_SynergyType SynergyType;

        public V2Enum_Grade Grade;

        public ObscuredInt MainSkillIndex;

        public MainSkillData SynergySkillData;

        public string NameLocalKey;
        public string DescLocalKey;
    }

    public class SynergyRuneOpenconditionData
    {
        public ObscuredInt Index;

        public Enum_SynergyType SynergyType;
        public ObscuredInt SlotNumber;

        public V2Enum_OpenConditionType OpenConditionType;
        public ObscuredInt OpenConditionValue;
    }

    public class SynergyRuneCombineData
    {
        public ObscuredInt Index;

        public V2Enum_Grade Grade;

        public ObscuredInt RequiredCount;
        public ObscuredInt SuccessProb;
    }

    public class SynergyRuneLocalTable : LocalTableBase
    {
        private Dictionary<ObscuredInt, SynergyRuneData> _synergyRuneDatas_Dic = new Dictionary<ObscuredInt, SynergyRuneData>();
        private Dictionary<V2Enum_Grade, List<SynergyRuneData>> _synergyRuneList_Dic = new Dictionary<V2Enum_Grade, List<SynergyRuneData>>();
        private List<SynergyRuneData> _synergyRuneAllData = new List<SynergyRuneData>();

        private Dictionary<ObscuredInt, SynergyRuneOpenconditionData> _synergyRuneOpenconditionDatas_Dic = new Dictionary<ObscuredInt, SynergyRuneOpenconditionData>();
        private Dictionary<V2Enum_Grade, SynergyRuneCombineData> _synergyRuneCombineDatas_Dic = new Dictionary<V2Enum_Grade, SynergyRuneCombineData>();


        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyRune", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SynergyRuneData synergyRuneData = new SynergyRuneData();

                synergyRuneData.Index = rows[i]["Index"].ToString().ToInt();

                synergyRuneData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                synergyRuneData.SynergyType = rows[i]["SynergyType"].ToString().ToInt().IntToEnum32<Enum_SynergyType>();

                synergyRuneData.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                synergyRuneData.MainSkillIndex = rows[i]["MainSkillIndex"].ToString().ToInt();

                synergyRuneData.NameLocalKey = string.Format("synergyrunename/{0}", synergyRuneData.Index);
                synergyRuneData.DescLocalKey = string.Format("synergyrunedesc/{0}", synergyRuneData.Index);


                if (_synergyRuneDatas_Dic.ContainsKey(synergyRuneData.Index) == false)
                    _synergyRuneDatas_Dic.Add(synergyRuneData.Index, synergyRuneData);

                if (_synergyRuneList_Dic.ContainsKey(synergyRuneData.Grade) == false)
                    _synergyRuneList_Dic.Add(synergyRuneData.Grade, new List<SynergyRuneData>());

                _synergyRuneList_Dic[synergyRuneData.Grade].Add(synergyRuneData);

                _synergyRuneAllData.Add(synergyRuneData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyRuneOpencondition", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SynergyRuneOpenconditionData synergyRuneOpenconditionData = new SynergyRuneOpenconditionData();

                synergyRuneOpenconditionData.Index = rows[i]["Index"].ToString().ToInt();

                //synergyRuneOpenconditionData.SynergyType = rows[i]["SynergyType"].ToString().ToInt().IntToEnum32<Enum_SynergyType>();

                synergyRuneOpenconditionData.SlotNumber = rows[i]["SlotNumber"].ToString().ToInt();

                synergyRuneOpenconditionData.OpenConditionType = rows[i]["OpenConditionType"].ToString().ToInt().IntToEnum32<V2Enum_OpenConditionType>();

                synergyRuneOpenconditionData.OpenConditionValue = rows[i]["OpenConditionValue"].ToString().ToInt();

                if (_synergyRuneOpenconditionDatas_Dic.ContainsKey(synergyRuneOpenconditionData.SlotNumber) == false)
                    _synergyRuneOpenconditionDatas_Dic.Add(synergyRuneOpenconditionData.SlotNumber, synergyRuneOpenconditionData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyRuneCombine", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SynergyRuneCombineData synergyRuneCombineData = new SynergyRuneCombineData();

                synergyRuneCombineData.Index = rows[i]["Index"].ToString().ToInt();

                synergyRuneCombineData.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                synergyRuneCombineData.RequiredCount = rows[i]["RequiredCount"].ToString().ToInt();
                synergyRuneCombineData.SuccessProb = rows[i]["SuccessProb"].ToString().ToInt();

                if (_synergyRuneCombineDatas_Dic.ContainsKey(synergyRuneCombineData.Grade) == false)
                    _synergyRuneCombineDatas_Dic.Add(synergyRuneCombineData.Grade, synergyRuneCombineData);
            }
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, SynergyRuneData> GetAllSynergyRuneData_Dic()
        {
            return _synergyRuneDatas_Dic;
        }
        //------------------------------------------------------------------------------------
        public List<SynergyRuneData> GetAllSynergyRuneData()
        {
            return _synergyRuneAllData;
        }
        //------------------------------------------------------------------------------------
        public List<SynergyRuneData> GetAllSynergyRuneData(V2Enum_Grade v2Enum_Grade)
        {
            if (_synergyRuneList_Dic.ContainsKey(v2Enum_Grade) == true)
                return _synergyRuneList_Dic[v2Enum_Grade];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SynergyRuneData GetSynergyRuneData(ObscuredInt index)
        {
            if (_synergyRuneDatas_Dic.ContainsKey(index) == true)
                return _synergyRuneDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SynergyRuneOpenconditionData GetSynergyRuneOpenconditionData(ObscuredInt slotNumber)
        {
            if (_synergyRuneOpenconditionDatas_Dic.ContainsKey(slotNumber) == false)
                return null;

            return _synergyRuneOpenconditionDatas_Dic[slotNumber];
        }
        //------------------------------------------------------------------------------------
        public SynergyRuneCombineData GetSynergyRuneCombineData(V2Enum_Grade grade)
        {
            if (_synergyRuneCombineDatas_Dic.ContainsKey(grade) == true)
                return _synergyRuneCombineDatas_Dic[grade];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}