using System.Collections.Generic;
using System.IO;
using System.Linq;
using BackEnd;
using Castle.Core.Internal;
using LitJson;
using UnityEditor;
using UnityEngine;

// 애쉬앤베일에서 슥 가져옴
public static class ChartAutoWriter
{
    public class ChartAutoWriteInfo
    {
        public bool isUpload;
        public string name;
        public string explain;
        public int selectedFileId;
        public string old;

        public ChartAutoWriteInfo()
        {

        }
        public ChartAutoWriteInfo(JsonData json)
        {
            name = json["chartName"].ToString();
            explain = json["chartExplain"].ToString();
            int outNum = 0;

            if (System.Int32.TryParse(json["selectedChartFileId"].ToString(), out outNum))
            {
                isUpload = true;
                selectedFileId = outNum;
            }
            else
            {
                isUpload = false;
                selectedFileId = 0;
            }

            old = json["old"].ToString();
        }

        public override string ToString()
        {
            return $"chartName: {name}\n" +
            $"chartExplain: {explain}\n" +
            $"isChartUpload: {isUpload}\n" +
            $"selectedChartFileId: {selectedFileId}\n" +
            $"old: {old}\n";
        }
    }

#if UNITY_EDITOR
    private const string VariableFormat = "        public {0} {1};";

    private const string FolderPath = "Assets/Main/Scripts/GameData/ChartData";

    private const string ScriptFormat = @"namespace GameBerry.Chart
{{
    public struct {0}Info
    {{
{2}
    }}

    public class {1} : ChartBase
    {{
        public {0}Info this[int index] => rows[index];
        public {0}Info[] rows;

        public override bool IsLoaded()
        {{
            return rows != null;
        }}
    }}

}}";

    [MenuItem("PTT/Chart Auto Write")]
    public static void CheckCharts()
    {
        EditorUtility.DisplayCancelableProgressBar("차트 자동 생성", "뒤끝 초기화 중", 0f);

        var settings = new BackendCustomSetting();
        settings.clientAppID = "d2a3d980-d3ff-11f0-b82d-791b975f922710704";
        settings.signatureKey = "d2a3d981-d3ff-11f0-b82d-791b975f922710704";
        settings.functionAuthKey = "";
        //settings.isAllPlatform = true;
        //settings.sendLogReport = false;
        settings.timeOutSec = 100;
        settings.useAsyncPoll = false;

        var init = Backend.Initialize(settings);
        if (!init.IsSuccess())
        {
            Debug.LogError("Backend failed to initialize");
            EditorUtility.ClearProgressBar();
            return;
        }

        if (EditorUtility.DisplayCancelableProgressBar("차트 자동 생성", "뒤끝 게스트 로그인 중", 0.2f))
        {
            EditorUtility.ClearProgressBar();
            return;
        }

        var login = Backend.BMember.GuestLogin();
        if (!login.IsSuccess())
        {
            Debug.LogError("Backend failed to login");
            EditorUtility.ClearProgressBar();
            return;
        }

        if (EditorUtility.DisplayCancelableProgressBar("차트 자동 생성", "차트 리스트 불러오는 중", 0.4f))
        {
            EditorUtility.ClearProgressBar();
            return;
        }

        var chartList = Backend.Chart.GetChartList();
        if (!chartList.IsSuccess())
        {
            Debug.LogError("Backend failed to get chart list");
            EditorUtility.ClearProgressBar();
            return;
        }

        if (EditorUtility.DisplayCancelableProgressBar("차트 자동 생성", "차트 확인 중", 0.6f))
        {
            EditorUtility.ClearProgressBar();
            return;
        }

        var unwrittenChartDict = new Dictionary<string, string>();

        var json = chartList.FlattenRows();
        for (var i = 0; i < json.Count; i++)
        {
            var info = new ChartAutoWriteInfo(json[i]);
            var local = Backend.Chart.GetLocalChartData(info.selectedFileId.ToString());

            string chartClass = $"GameBerry.Chart.{info.name}Chart, Assembly-CSharp";

            if (local.IsNullOrEmpty())
            {
                var server = Backend.Chart.GetOneChartAndSave(info.selectedFileId.ToString());
                if (!server.IsSuccess())
                {
                    Debug.LogError($"Backend failed to get and save chart: {info.name}");
                }


                System.Type type = System.Type.GetType(chartClass);
                if (type == null)
                {
                    unwrittenChartDict.Add(info.name, local);
                }
            }
            else
            {
                System.Type type = System.Type.GetType(chartClass);
                if (type == null)
                {
                    unwrittenChartDict.Add(info.name, local);
                }
            }
        }

        if (EditorUtility.DisplayCancelableProgressBar("차트 자동 생성", "차트 생성 중", 0.8f))
        {
            EditorUtility.ClearProgressBar();
            return;
        }

        AutoWriteV2(unwrittenChartDict);
    }

    public static void AutoWriteV2(Dictionary<string, string> chartData)
    {
        if (chartData.Count < 1)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("완료", "생성할 차트 데이터가 없습니다.", "확인");
            return;
        }

        foreach (var (name, str) in chartData)
        {
            var path = $"{FolderPath}/{name}Chart.cs";
            if (File.Exists(path))
            // var script = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            // if (script != null)
            {
                Debug.Log($"{name} file exist: {path}");
                continue;
            }
            string chartClassName = $"{name}Chart";


            var json = JsonMapper.ToObject(str);
            if (!json.ContainsKey("rows"))
            {
                continue;
            }
            var row = json["rows"][0];
            var variables = "";
            for (var j = 0; j < row.Count; j++)
            {
                var key = row.Keys.ElementAt(j);
                variables += $"{string.Format(VariableFormat, GetTypeName(row[key]["S"].ToString()), key)}\n";
            }

            var result = string.Format(ScriptFormat, name, chartClassName, variables[..^1]);

            File.WriteAllText(path, result);
            AssetDatabase.Refresh();
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("완료", $"자동 생성 완료\n{string.Join(", ", chartData.Keys)}", "굿");
    }

    private static string GetTypeName(string value)
    {
        if (value.Contains(';'))
        {
            // 배열
            var split = value.Split(';');
            if (int.TryParse(split[0], out _))
            {
                return "int[]";
            }

            if (double.TryParse(split[0], out _))
            {
                return "double[]";
            }

            if (bool.TryParse(split[0], out _))
            {
                return "bool[]";
            }

            return "string[]";
        }

        if (int.TryParse(value, out _))
        {
            return "int";
        }

        if (double.TryParse(value, out _))
        {
            return "double";
        }

        if (bool.TryParse(value, out _))
        {
            return "bool";
        }

        return "string";
    }
#endif
}
