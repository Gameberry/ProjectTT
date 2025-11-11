using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public static class ClientLocalChartManager
    {
        private static string m_defaultPath = string.Format("{0}/LocalChart", Application.streamingAssetsPath);
        private static string m_defaultPath_V2 = string.Format("{0}/LocalChart/V2", Application.streamingAssetsPath);

        public static string ReadData(string chartname)
        {
            string source = string.Empty;
            string filePathText = string.Format("{0}/{1}.txt", m_defaultPath, chartname);

            //using (StreamReader sr = new StreamReader(filePath))
            //{
            //    source = sr.ReadLine();
            //    sr.Close();
            //}

            using (UnityWebRequest www = UnityWebRequest.Get(filePathText))
            {
                www.SendWebRequest();
                while (!www.isDone) ;

                source = www.downloadHandler.text;
            }

            string re = string.Concat(source.Where(x => !char.IsWhiteSpace(x)));
            re = string.Concat(re.Where(x => x != '﻿'));
            return re;
        }

        public static string ReadJsonData(string chartname)
        {
            string source = string.Empty;
            string filePathText = string.Format("{0}/{1}", m_defaultPath, chartname);

            //using (StreamReader sr = new StreamReader(filePath))
            //{
            //    source = sr.ReadLine();
            //    sr.Close();
            //}


#if UNITY_IOS

            if (File.Exists(filePathText))
            {
                source = File.ReadAllText(filePathText);
            }
#else
            using (UnityWebRequest www = UnityWebRequest.Get(filePathText))
            {
                www.SendWebRequest();
                while (!www.isDone) ;

                source = www.downloadHandler.text;
            }
#endif


            //string re = string.Concat(source.Where(x => !char.IsWhiteSpace(x)));
            //re = string.Concat(re.Where(x => x != '﻿'));
            //return re;

            if (source.Length > 0 && source[0] == '\uFEFF')
            {
                source = source.Remove(0, 1);
            }

            return source;
        }

        public static string ReadJsonData_IOS(string chartname)
        {
            string source = string.Empty;
            string filePathText = string.Format("{0}/{1}", m_defaultPath, chartname);

            if (File.Exists(filePathText))
            {
                source = File.ReadAllText(filePathText);
            }

            //using (UnityWebRequest www = UnityWebRequest.Get(filePathText))
            //{
            //    www.SendWebRequest();
            //    while (!www.isDone) ;

            //    source = www.downloadHandler.text;
            //}

            //string re = string.Concat(source.Where(x => !char.IsWhiteSpace(x)));
            //re = string.Concat(re.Where(x => x != '﻿'));
            //return re;
            return source;
        }

        public static string SaveLocalChartData(string chartname, string path)
        {
            string source = string.Empty;

            using (StreamReader sr = new StreamReader(path))
            {
                source = sr.ReadToEnd();
                sr.Close();
            }

            //using (UnityWebRequest www = UnityWebRequest.Get(path))
            //{
            //    www.SendWebRequest();
            //    while (!www.isDone) ;

            //    source = www.downloadHandler.text;
            //}

            string re = string.Concat(source.Where(x => !char.IsWhiteSpace(x)));
            re = string.Concat(re.Where(x => x != '﻿'));

            SecurityPlayerPrefs.SetString(chartname, source);

            return re;
        }

        public static string GetLocalChartData(string chartname)
        {
            return ReadJsonData(chartname);
            return SecurityPlayerPrefs.GetString(chartname, string.Empty);
        }



        public static string ReadJsonData_V2(string chartname)
        {
            string source = string.Empty;
            //string filePathText = string.Format("{0}/{1}", m_defaultPath_V2, chartname);
            string filePathText = string.Format("{0}/{1}", m_defaultPath_V2, chartname);

            //using (StreamReader sr = new StreamReader(filePath))
            //{
            //    source = sr.ReadLine();
            //    sr.Close();
            //}

#if UNITY_IOS

            if (File.Exists(filePathText))
            {
                source = File.ReadAllText(filePathText);
            }
#else
            using (UnityWebRequest www = UnityWebRequest.Get(filePathText))
            {
                www.SendWebRequest();
                while (!www.isDone) ;

                source = www.downloadHandler.text;
            }
#endif
            //string re = string.Concat(source.Where(x => !char.IsWhiteSpace(x)));
            //re = string.Concat(re.Where(x => x != '﻿'));
            //return re;

            if (source.Length > 0 && source[0] == '\uFEFF')
            {
                source = source.Remove(0, 1);
            }

            return source;
        }


        public static string GetLocalChartData_V2(string chartname)
        {
            return ReadJsonData_V2(chartname);
            
        }

        public static async UniTask<string> GetLocalChartData_Task(string chartname)
        {
            System.UriBuilder defaultUri = new System.UriBuilder();
            string filePathText = string.Format("{0}/{1}", m_defaultPath_V2, chartname);

            string source = string.Empty;
#if UNITY_IOS
            if (File.Exists(filePathText))
            {
                source = File.ReadAllText(filePathText);
            }
#else
            var www = UnityWebRequest.Get(filePathText);
            var operation = www.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            source = www.downloadHandler.text;
#endif

            if (source.Length > 0 && source[0] == '\uFEFF')
            {
                source = source.Remove(0, 1);
            }

            return source;
        }
    }
}