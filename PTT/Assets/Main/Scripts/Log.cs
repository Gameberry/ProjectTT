using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
using GameBerry;

public class Log : MonoSingleton<Log>
{
    string _logPath = "";

    static FileStream _fileLog;
    static StreamWriter _writer;
    const string _logFilename = "Log";


    //-----------------------------------------------------------------------------------------------
    public void Setup()
    {
#if (UNITY_EDITOR || UNITY_EDITOR_64)
        _logPath = Application.dataPath + "/Log";

        if (!System.IO.Directory.Exists(_logPath))
            System.IO.Directory.CreateDirectory(_logPath);

        string fullpath = MakeFullpath(_logFilename, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        _fileLog = new FileStream(fullpath, FileMode.Append);
        _writer = new StreamWriter(_fileLog);
#endif
    }
    //-----------------------------------------------------------------------------------------------
    void OnDestroy()
    {
        if (_writer != null)
            _writer.Close();

        if (_fileLog != null)
            _fileLog.Close();
    }
    //-----------------------------------------------------------------------------------------------
    public void log(string logmsg)
    {
#if (UNITY_EDITOR || UNITY_EDITOR_64)
        if (_writer != null)
            _writer.WriteLine(logmsg);
#endif
    }
    //-----------------------------------------------------------------------------------------------
    string MakeFullpath(string fileName, string dateTimeString)
    {
        return string.Format("{0}/{1}_{2}.txt", _logPath, fileName, dateTimeString);
    }
    //-----------------------------------------------------------------------------------------------
    public static void ClientError(string errorlog)
    {
        Debug.LogError(string.Format("ClientError   Message : {0}", errorlog));
    }
    //-----------------------------------------------------------------------------------------------
}
