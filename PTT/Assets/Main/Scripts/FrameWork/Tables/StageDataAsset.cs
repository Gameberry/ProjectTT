using UnityEngine;
using System.Collections.Generic;

namespace GameBerry
{
    [System.Serializable]
    public class StageData
    {
        public int StageNumber = 0;
        public int MapIndex = 0;
        public List<int> MonsterSpineIndex = new List<int>();
        public List<StatViewer> MonsterStat = new List<StatViewer>();
    }

    [CreateAssetMenu(fileName = "StageData", menuName = "Table/StageData", order = 1)]
    public class StageDataAsset : ScriptableObject
    {
        [ArrayElementTitle("StageNumber")]
        public List<StageData> StageDatas = new List<StageData>();

        //------------------------------------------------------------------------------------
        void OnEnable()
        {
            if (StageDatas.Count <= 0)
            {
                for (int chapter = 1; chapter <= 5; ++chapter)
                {
                    for (int stage = 1; stage <= 5; ++stage)
                    {
                        StageData stageData = new StageData();

                        stageData.StageNumber = stage;
                        stageData.StageNumber += chapter * 100;

                        stageData.MapIndex = (chapter - 1) % 4;

                        int selectmonsterspine = Random.Range(1, 4);

                        if (selectmonsterspine == 1)
                            stageData.MonsterSpineIndex.Add(selectmonsterspine);
                        else if (selectmonsterspine == 2)
                            stageData.MonsterSpineIndex.Add(selectmonsterspine);
                        else
                        {
                            stageData.MonsterSpineIndex.Add(1);
                            stageData.MonsterSpineIndex.Add(2);
                        }

                        double attackvalue = 2 * stage + (6 * (chapter - 1));
                        double hpvalue = 40 * stage + (200 * (chapter - 1));
                        double movespeed = 0.75;
                        double attackspeed = 2;

                        stageData.MonsterStat.Add(new StatViewer { v2Enum_Stat = V2Enum_Stat.Attack, value = attackvalue });
                        stageData.MonsterStat.Add(new StatViewer { v2Enum_Stat = V2Enum_Stat.HP, value = hpvalue });
                        stageData.MonsterStat.Add(new StatViewer { v2Enum_Stat = V2Enum_Stat.MoveSpeed, value = movespeed });
                        stageData.MonsterStat.Add(new StatViewer { v2Enum_Stat = V2Enum_Stat.AttackSpeed, value = attackspeed });

                        StageDatas.Add(stageData);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public StageData GetStageData(int stagenumber)
            => StageDatas.Find(x => x.StageNumber == stagenumber);
        //------------------------------------------------------------------------------------
    }
}