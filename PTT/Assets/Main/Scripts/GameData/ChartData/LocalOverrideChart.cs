namespace GameBerry.Chart
{
    public struct LocalOverrideInfo
    {
        public int Index;
        public string TextKey;
        public string TextKR;
        public string TextEN;
        public string TextJP;
        public string TextTW;
        public string TextPT;
        public string TextSP;
    }

    public class LocalOverrideChart : ChartBase
    {
        public LocalOverrideInfo this[int index] => rows[index];
        public LocalOverrideInfo[] rows;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            StringLocalChart stringLocalChart = Managers.LocalTableManager.Instance.GetTableClass<StringLocalChart>();

            if (stringLocalChart == null)
                return;

            for (int i = 0; i < rows.Length; ++i)
            {
                try
                {
                    string id = rows[i].TextKey;

                    StringLocalData stringlocaldata = stringLocalChart.GetLocalString(id);
                    if (stringlocaldata == null)
                    {
                        stringlocaldata = new StringLocalData();
                        stringlocaldata.LocalStringID = id;
                        stringLocalChart.AddLocalString(stringlocaldata);
                    }

                    if (stringlocaldata.LocalizeString.ContainsKey(LocalizeType.Korean) == false)
                        stringlocaldata.LocalizeString.Add(LocalizeType.Korean, string.Empty);

                    if (stringlocaldata.LocalizeString.ContainsKey(LocalizeType.English) == false)
                        stringlocaldata.LocalizeString.Add(LocalizeType.English, string.Empty);

                    if (stringlocaldata.LocalizeString.ContainsKey(LocalizeType.Japanese) == false)
                        stringlocaldata.LocalizeString.Add(LocalizeType.Japanese, string.Empty);

                    if (stringlocaldata.LocalizeString.ContainsKey(LocalizeType.ChineseTraditional) == false)
                        stringlocaldata.LocalizeString.Add(LocalizeType.ChineseTraditional, string.Empty);

                    if (stringlocaldata.LocalizeString.ContainsKey(LocalizeType.Portuguesa) == false)
                        stringlocaldata.LocalizeString.Add(LocalizeType.Portuguesa, string.Empty);

                    if (stringlocaldata.LocalizeString.ContainsKey(LocalizeType.Spanish) == false)
                        stringlocaldata.LocalizeString.Add(LocalizeType.Spanish, string.Empty);

                    stringlocaldata.LocalizeString[LocalizeType.Korean] = rows[i].TextKR;
                    stringlocaldata.LocalizeString[LocalizeType.English] = rows[i].TextEN;
                    stringlocaldata.LocalizeString[LocalizeType.Japanese] = rows[i].TextJP;
                    stringlocaldata.LocalizeString[LocalizeType.ChineseTraditional] = rows[i].TextTW;
                    stringlocaldata.LocalizeString[LocalizeType.Portuguesa] = rows[i].TextPT;
                    stringlocaldata.LocalizeString[LocalizeType.Spanish] = rows[i].TextSP;
                }
                catch
                {
                    UnityEngine.Debug.LogError(string.Format("LocalOverrideChart 터짐 범인 : {0} 번째", i));
                }
            }
        }
    }

}