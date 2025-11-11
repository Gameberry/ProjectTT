#if UNITY_IOS

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.iOS.Xcode;



namespace UnityEditor.Localization.Platform.iOS
{

    public static class PlayeriOS
    {

        const string kInfoFile = "InfoPlist.strings";



        /// <param name="projectDirectory">The root project directory to be updated. This is where the iOS player was built to.</param>
        /// AppInfo is a class that has as parameters: localization ('en', 'it', ...), app_name ('..., ...'), idfa_key ('I use this flag for ...')
        public static void AddLocalizationToXcodeProject(string projectDirectory, List<AppInfo> appinfo_list)
        {

            var pbxPath = PBXProject.GetPBXProjectPath(projectDirectory);
            var project = new PBXProject();
            project.ReadFromFile(pbxPath);
            project.ClearKnownRegions(); // Remove the deprecated regions that get added automatically.

            var plistDocument = new PlistDocument();
            var plistPath = Path.Combine(projectDirectory, "Info.plist");
            plistDocument.ReadFromFile(plistPath);

            var bundleLanguages = plistDocument.root.CreateArray("CFBundleLocalizations");

            foreach (AppInfo appinfo in appinfo_list)
            {
                var code = appinfo.localization;
                project.AddKnownRegion(code);
                bundleLanguages.AddString(code);

                var localeDir = code + ".lproj";
                var dir = Path.Combine(projectDirectory, localeDir);
                Directory.CreateDirectory(dir);

                var filePath = Path.Combine(dir, kInfoFile);
                var relativePath = Path.Combine(localeDir, kInfoFile);

                GenerateLocalizedInfoPlistFile(appinfo, plistDocument, filePath);
                project.AddLocaleVariantFile(kInfoFile, code, relativePath);
            }

            plistDocument.WriteToFile(plistPath);
            project.WriteToFile(pbxPath);

        }


        static void GenerateLocalizedInfoPlistFile(AppInfo appinfo, PlistDocument plistDocument, string filePath)
        {
            using (var stream = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                stream.Write(
                    "/*\n" +
                    $"\t{kInfoFile}\n" +
                    $"\tThis file was auto-generated for localizations\n" +
                    $"*/\n\n");

                //WriteLocalizedValue("CFBundleName", stream, appinfo.app_name, plistDocument);
                WriteLocalizedValue("CFBundleDisplayName", stream, appinfo.app_name, plistDocument);
                //WriteLocalizedValue("NSUserTrackingUsageDescription", stream, appinfo.idfa_key, plistDocument);
            }
        }


        static void WriteLocalizedValue(string valueName, StreamWriter stream, string value, PlistDocument plistDocument)
        {

            stream.WriteLine($"\"{valueName}\" = \"{value}\";");
            plistDocument.root.SetString(valueName, string.Empty);

        }

    }

}

#endif