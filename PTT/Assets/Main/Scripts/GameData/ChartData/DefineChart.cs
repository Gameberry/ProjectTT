using System.Collections.Generic;

namespace GameBerry.Chart
{
    public struct DefineInfo
    {
        public string key;
        public string value;
    }

    public class DefineChart : ChartBase
    {
        public DefineInfo this[int index] => rows[index];
        public DefineInfo[] rows;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            for (int i = 0; i < rows.Length; ++i)
            {
                switch (rows[i].key)
                {
                    case "StarforceRestoration1_Key":
                        {
                            Define.StarforceRestoration1_Key = PackUtil.UnpackValue<int>(rows[i].value);
                            break;
                        }
                    case "StarforceRestoration1_Price":
                        {
                            Define.StarforceRestoration1_Price = PackUtil.UnpackValue<long>(rows[i].value);
                            break;
                        }
                    case "StarforceRestoration2_Key":
                        {
                            Define.StarforceRestoration2_Key = PackUtil.UnpackValue<int>(rows[i].value);
                            break;
                        }
                    case "StarforceRestoration2_Price":
                        {
                            Define.StarforceRestoration2_Price = PackUtil.UnpackValue<long>(rows[i].value);
                            break;
                        }
                }
            }
        }
    }
}