#if UNITY_ANDROID

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;



namespace UnityEditor.Localization.Platform.Android
{


    public static class PlayerAndroid
    {

        const string k_InfoFile = "strings.xml";



        /// <param name="projectDirectory">The root project directory to be updated. This is where the Android player was built to.</param>
        public static void AddLocalizationToAndroidGradleProject(string projectDirectory, List<AppInfo> appinfo_list)
        {

            var project = new GradleProjectSettings();

            foreach (AppInfo appinfo in appinfo_list)
            {
                var localeIdentifier = appinfo.localization;
                var IsSpecialLocaleIdentifier = localeIdentifier.Contains("Hans") || localeIdentifier.Contains("Hant") || localeIdentifier.Contains("Latn") || localeIdentifier.Contains("Cyrl") || localeIdentifier.Contains("Arab") || localeIdentifier.Contains("valencia");
                localeIdentifier = localeIdentifier.Contains("-") ? IsSpecialLocaleIdentifier ? localeIdentifier.Replace("-", "+") : localeIdentifier.Replace("-", "-r") : localeIdentifier;
                GenerateLocalizedXmlFile(Path.Combine(Directory.CreateDirectory(Path.Combine(project.GetResFolderPath(projectDirectory), "values-b+" + localeIdentifier)).FullName, k_InfoFile), appinfo);
            }

            var androidManifest = new AndroidManifest(project.GetManifestPath(projectDirectory));
            androidManifest.SetAtrribute("label", project.LabelName);

            androidManifest.SaveIfModified();
        
        }


        static void GenerateLocalizedXmlFile(string filePath, AppInfo appinfo)
        {

            // We are adding a back slash when the entry value contains an single quote, to prevent android build failures and show the display name with apostrophe ex: " J'adore ";
            // (?<!\\) - Negative Lookbehind to ignore any that already start with \\
            // (?<replace>') - match colon and place it into the replace variable
            var localizedValue = Regex.Replace(appinfo.app_name, @"(?<!\\)(?<replace>')", @"\'");


            using (var stream = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                stream.WriteLine(
                    $@"<?xml version=""1.0"" encoding=""utf-8""?>" +
                    "<!--" +
                    "\n" +
                    $"\t{k_InfoFile}\n" +
                    $"\tThis file was auto-generated for localizations\n" +
                    $"-->" +
                    "\n" +
                    $@"<resources>
                       <string name=""app_name""> {localizedValue} </string>
                       </resources>");
            }

        }

    }

}

#endif
