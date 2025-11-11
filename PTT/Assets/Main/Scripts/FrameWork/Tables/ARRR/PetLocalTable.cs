using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using CodeStage.AntiCheat.ObscuredTypes;
using Gpm.Ui;

namespace GameBerry
{
    public class PetData : InfiniteScrollData
    {
        public ObscuredInt Index;

        public ObscuredInt ResourceIndex; // 모델링 인덱스
        public ObscuredInt ResourceSkin; // 모델링 기본 스킨 번호

        public ObscuredInt AnimNumber; // 애니 번호

        public ObscuredFloat Scale = 1.0f;
        public V2Enum_Grade Grade;

        public ObscuredInt ActiveSkill;
        public SkillBaseData ActiveSkillData;

        public List<SkillBaseData> PassiveDatas;

        public ObscuredInt PassiveSkill1;

        public ObscuredInt PassiveSkill2;

        public ObscuredInt PassiveSkill3;

        public string NameLocalStringKey;
        public string DescLocalStringKey;

        public DescendData descendData = null;
    }

    public class LevelUpCostData
    {
        public ObscuredInt Index;

        public V2Enum_Grade Grade;
        public ObscuredInt MaximumLevel;

        public ObscuredInt LevelUpCostCount;
    }

    public class PetLocalTable : LocalTableBase
    {
        private List<PetData> _creatureDatas = new List<PetData>();
        private Dictionary<ObscuredInt, PetData> _creatureDatas_Dic = new Dictionary<ObscuredInt, PetData>();
        private Dictionary<V2Enum_Grade, LevelUpCostData> _petLevelUpCostDatas_Dic = new Dictionary<V2Enum_Grade, LevelUpCostData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Pet", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);


            for (int i = 0; i < rows.Count; ++i)
            {
                PetData petData = new PetData();

                petData.Index = rows[i]["Index"].ToString().ToInt();

                petData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();
                petData.ResourceSkin = rows[i]["ResourceSkin"].ToString().ToInt();
                petData.AnimNumber = rows[i]["AnimNumber"].ToString().ToInt();

                petData.Scale = rows[i]["Scale"].ToString().ToFloat();

                petData.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                petData.ActiveSkill = rows[i]["ActiveSkill"].ToString().ToInt();

                petData.PassiveSkill1 = rows[i]["PassiveSkill1"].ToString().ToInt();
                petData.PassiveSkill2 = rows[i]["PassiveSkill2"].ToString().ToInt();
                petData.PassiveSkill3 = rows[i]["PassiveSkill3"].ToString().ToInt();

                petData.NameLocalStringKey = string.Format("petname/{0}", petData.Index);
                petData.DescLocalStringKey = string.Format("petdescription/{0}", petData.Index);


                _creatureDatas.Add(petData);

                if (_creatureDatas_Dic.ContainsKey(petData.Index) == false)
                    _creatureDatas_Dic.Add(petData.Index, petData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("PetLevelUpCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);


            for (int i = 0; i < rows.Count; ++i)
            {
                LevelUpCostData petLevelUpCostData = new LevelUpCostData();

                petLevelUpCostData.Index = rows[i]["Index"].ToString().ToInt();

                petLevelUpCostData.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                petLevelUpCostData.MaximumLevel = rows[i]["MaximumLevel"].ToString().ToInt();

                petLevelUpCostData.LevelUpCostCount = rows[i]["PetLevelUpCostCount"].ToString().ToInt();

                if (_petLevelUpCostDatas_Dic.ContainsKey(petLevelUpCostData.Grade) == false)
                    _petLevelUpCostDatas_Dic.Add(petLevelUpCostData.Grade, petLevelUpCostData);
            }
        }
        //------------------------------------------------------------------------------------
        public List<PetData> GetPetAllDatas()
        {
            return _creatureDatas;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, PetData> GetPetAllData()
        {
            return _creatureDatas_Dic;
        }
        //------------------------------------------------------------------------------------
        public PetData GetPetData(ObscuredInt index)
        {
            if (_creatureDatas_Dic.ContainsKey(index) == true)
                return _creatureDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public LevelUpCostData GetPetLevelUpCostData(V2Enum_Grade index)
        {
            if (_petLevelUpCostDatas_Dic.ContainsKey(index) == true)
                return _petLevelUpCostDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}