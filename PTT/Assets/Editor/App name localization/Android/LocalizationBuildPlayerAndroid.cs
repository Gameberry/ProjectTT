#if UNITY_ANDROID

using System.Collections.Generic;
using UnityEditor.Android;



namespace UnityEditor.Localization.Platform.Android
{

    class LocalizationBuildPlayerAndroid : IPostGenerateGradleAndroidProject
    {

        public int callbackOrder { get { return 1; } }

        public void OnPostGenerateGradleAndroidProject(string basePath)
        {

            // Set the Product Name, perhaps the name that should represent the app for unsupported localizations
            // At the moment the name shown for unsupported localizations, however, is the name of the executable
            PlayerSettings.productName = "BarBar";


            // Create the list of localizations
            List<AppInfo> list_appinfo = new List<AppInfo>();


            // English
            AppInfo appinfo_en = new AppInfo("en", "BarBar", "");
            list_appinfo.Add(appinfo_en);

            // Korean
            AppInfo appinfo_ko = new AppInfo("ko", "바바굴리기", "");
            list_appinfo.Add(appinfo_ko);


            // Taiwan
            AppInfo appinfo_tw = new AppInfo("zh-TW", "BarBar", "");
            list_appinfo.Add(appinfo_tw);

            //
            AppInfo appinfo_jp = new AppInfo("ja", "BarBar", "");
            list_appinfo.Add(appinfo_jp);        

            // Create the localizations in the Android project
            PlayerAndroid.AddLocalizationToAndroidGradleProject(basePath, list_appinfo);

        }

    }

}

#endif
