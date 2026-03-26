using System.Collections.Generic;
using LitJson;
using BackEnd;

namespace GameBerry.Table
{
    public class DungeonProgressData : IPackable
    {
        public Enum_Dungeon dungeonType;
        public int currentChapter = 1;
        public int currentStage = 1;
        public int maxChapter = 1;
        public int maxStage = 1;
        public int clearedStage = 0;

        public string Pack()
            => $"{PackUtil.PackValue(dungeonType.Enum32ToInt())},{PackUtil.PackValue(currentChapter)},{PackUtil.PackValue(currentStage)},{PackUtil.PackValue(maxChapter)},{PackUtil.PackValue(maxStage)},{PackUtil.PackValue(clearedStage)}";

        public void Unpack(string str)
        {
            dungeonType = Enum_Dungeon.None;
            currentChapter = 1;
            currentStage = 1;
            maxChapter = 1;
            maxStage = 1;
            clearedStage = 0;

            if (string.IsNullOrEmpty(str))
                return;

            string[] sp = str.Split(',');
            if (sp.Length > 0)
                dungeonType = PackUtil.UnpackValue<int>(sp[0]).IntToEnum32<Enum_Dungeon>();
            if (sp.Length > 1)
                currentChapter = PackUtil.UnpackValue<int>(sp[1]);
            if (sp.Length > 2)
                currentStage = PackUtil.UnpackValue<int>(sp[2]);
            if (sp.Length > 3)
                maxChapter = PackUtil.UnpackValue<int>(sp[3]);
            if (sp.Length > 4)
                maxStage = PackUtil.UnpackValue<int>(sp[4]);
            if (sp.Length > 5)
                clearedStage = PackUtil.UnpackValue<int>(sp[5]);
        }
    }

    public class DungeonProgressTable : TableBase
    {
        private const string dungeonProgressKey = "DungeonProgress";
        private List<DungeonProgressData> _progressList = new List<DungeonProgressData>();

        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0)
                return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate")
                        SetInData(data[i][key].ToString());
                    else if (key == dungeonProgressKey)
                        _progressList = PackUtil.UnpackList<DungeonProgressData>(data[i][key].ToString());
                }
            }
        }

        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(dungeonProgressKey, PackUtil.PackList(_progressList));
            return p;
        }

        public DungeonProgressData GetOrCreate(Enum_Dungeon dungeonType)
        {
            DungeonProgressData data = _progressList.Find(x => x.dungeonType == dungeonType);
            if (data != null)
                return data;

            data = new DungeonProgressData
            {
                dungeonType = dungeonType,
                currentChapter = 1,
                currentStage = 1,
                maxChapter = 1,
                maxStage = 1,
                clearedStage = 0
            };
            _progressList.Add(data);
            return data;
        }

        public bool TryGet(Enum_Dungeon dungeonType, out DungeonProgressData data)
        {
            data = _progressList.Find(x => x.dungeonType == dungeonType);
            return data != null;
        }

        public void SetCurrent(Enum_Dungeon dungeonType, int chapter, int stage)
        {
            DungeonProgressData data = GetOrCreate(dungeonType);
            data.currentChapter = chapter < 1 ? 1 : chapter;
            data.currentStage = stage < 1 ? 1 : stage;
        }

        public void SetMax(Enum_Dungeon dungeonType, int chapter, int stage)
        {
            DungeonProgressData data = GetOrCreate(dungeonType);
            data.maxChapter = chapter < 1 ? 1 : chapter;
            data.maxStage = stage < 1 ? 1 : stage;
        }

        public void SetClearedStage(Enum_Dungeon dungeonType, int stage)
        {
            DungeonProgressData data = GetOrCreate(dungeonType);
            data.clearedStage = stage < 0 ? 0 : stage;
        }
    }
}
