using System.Collections.Generic;
using System.IO;
using System.Linq;
using BackEnd;
using Castle.Core.Internal;
using LitJson;
using UnityEditor;
using UnityEngine;

public static class TableAutoWriter
{
#if UNITY_EDITOR
    private const string FolderPath = "Assets/Main/Scripts/GameData/TableData";

    private const string ScriptFormat = @"using LitJson;
using BackEnd;

namespace GameBerry.Table
{{
    public class {0} : TableBase
    {{
        public override void SetData(JsonData jsonData)
        {{
            
        }}

        public override Param GetParam()
        {{
            return new Param();
        }}
    }}

}}";

    [MenuItem("PTT/Table Auto Write")]
    public static void CheckCharts()
    {
        EditorUtility.DisplayCancelableProgressBar("테이블 자동 생성", "뒤끝 초기화 중", 0f);

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

        if (EditorUtility.DisplayCancelableProgressBar("테이블 자동 생성", "뒤끝 게스트 로그인 중", 0.2f))
        {
            EditorUtility.ClearProgressBar();
            return;
        }

        var login = Backend.BMember.CustomSignUp("0", "0");
        if (!login.IsSuccess())
        {
            if (login.GetStatusCode() == "409")
            {
                login = Backend.BMember.CustomLogin("0", "0");
                if (!login.IsSuccess())
                {
                    Debug.LogError($"Backend failed to login {login.Message}");
                    EditorUtility.ClearProgressBar();
                    return;
                }
            }
            else
            {
                Debug.LogError($"Backend failed to login {login.Message}");
                EditorUtility.ClearProgressBar();
                return;
            }
        }

        if (EditorUtility.DisplayCancelableProgressBar("테이블 자동 생성", "테이블 리스트 불러오는 중", 0.4f))
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

        if (EditorUtility.DisplayCancelableProgressBar("테이블 자동 생성", "테이블 확인 중", 0.6f))
        {
            EditorUtility.ClearProgressBar();
            return;
        }


        var bro = Backend.PlayerData.GetTableList();

        if (bro.IsSuccess() == false)
        {
            EditorUtility.DisplayDialog("실패", bro.ToString(), "확인");
            Debug.LogError(bro.ToString());
            return;
        }

        LitJson.JsonData tableListJson = bro.GetReturnValuetoJSON()["tables"];

        var unwrittenChartDict = new List<string>();


        for (int i = 0; i < tableListJson.Count; i++)
        {
            string TableName = tableListJson[i]["tableName"].ToString();

            string chartClass = $"GameBerry.Table.{TableName}Table, Assembly-CSharp";

            System.Type type = System.Type.GetType(chartClass);
            if (type == null)
            {
                unwrittenChartDict.Add(TableName);
            }
        }

        if (EditorUtility.DisplayCancelableProgressBar("테이블 자동 생성", "테이블 생성 중", 0.8f))
        {
            EditorUtility.ClearProgressBar();
            return;
        }

        AutoWriteV2(unwrittenChartDict);
    }

    public static void AutoWriteV2(List<string> chartData)
    {
        if (chartData.Count < 1)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("완료", "생성할 테이블 데이터가 없습니다.", "확인");
            return;
        }

        foreach (var name in chartData)
        {
            var path = $"{FolderPath}/{name}Table.cs";
            if (File.Exists(path))
            // var script = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            // if (script != null)
            {
                Debug.Log($"{name} file exist: {path}");
                continue;
            }
            string chartClassName = $"{name}Table";


            var result = string.Format(ScriptFormat, chartClassName);

            File.WriteAllText(path, result);
            AssetDatabase.Refresh();
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("완료", $"자동 생성 완료\n{string.Join(", ", chartData)}", "굿");
    }
#endif
}
