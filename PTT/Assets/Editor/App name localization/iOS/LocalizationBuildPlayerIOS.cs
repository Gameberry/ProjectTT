#if UNITY_IOS

using System.IO;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;



namespace UnityEditor.Localization.Platform.iOS
{   
    class LocalizationBuildPlayerIOS : IPostprocessBuildWithReport
    {

        public int callbackOrder => 1;

        public void OnPostprocessBuild(BuildReport report)
        {

            // Create the key for the IDFA
            Create_IDFA_key(report.summary.outputPath, "This identifier will be used to deliver personalized ads to you.");


            // Set the Product Name, the default name that should represent the app for unsupported localizations (?)
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

            // Japanese
            AppInfo appinfo_jp = new AppInfo("ja", "BarBar", "");
            list_appinfo.Add(appinfo_jp);

            string buildPath = report.summary.outputPath;
            // Create the localizations in the iOS project
            PlayeriOS.AddLocalizationToXcodeProject(buildPath, list_appinfo);

            //var projectPath = buildPath+"/Unity-iPhone.xcodeproj/project.pbxproj";


            string projPath = PBXProject.GetPBXProjectPath(buildPath);

            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

            ProjectCapabilityManager manager = new ProjectCapabilityManager(
                projPath,
                "Entitlements.entitlements",
                targetGuid: proj.GetUnityMainTargetGuid()
            );
            manager.AddPushNotifications(false);
            //manager.AddiCloud(true, false, null);

            manager.WriteToFile();

            //PBXProject pbxProject = new PBXProject();
            //pbxProject.ReadFromFile(projectPath);
            ////string targetGuid = pbxProject.TargetGuidByName("Unity-iPhone");
            //string targetGuid = pbxProject.TargetGuidByName(report.summary.guid.ToString());

            //// Add push notifications as a capability on the target
            //pbxProject.AddCapability(targetGuid, PBXCapabilityType.PushNotifications);

            //// Apply settings
            ////File.WriteAllText(projectPath, pbxProject.WriteToString());


            //var target_name = pbxProject.GetUnityMainTargetGuid();
            //var target_guid = pbxProject.TargetGuidByName(target_name);
            //var file_name = "unity.entitlements";
            //var proj_path = PBXProject.GetPBXProjectPath(buildPath);
            ////var proj = new PBXProject();
            ////proj.ReadFromFile(proj_path);

            ////var dst = buildPath + "/" + target_name + "/" + file_name;
            //try
            //{
            //    File.WriteAllText(projectPath, entitlements);
            //    pbxProject.AddFile(target_name + "/" + file_name, file_name);
            //    pbxProject.AddBuildProperty(target_guid, "CODE_SIGN_ENTITLEMENTS", target_name + "/" + file_name);
            //    pbxProject.WriteToFile(proj_path);
            //}
            //catch (IOException e)
            //{
            //    UnityEngine.Debug.LogWarning($"Could not copy entitlements. Probably already exists. ({e})");
            //}
        }
     //   private const string entitlements = @"
     //<?xml version=""1.0"" encoding=""UTF-8\""?>
     //<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
     //<plist version=""1.0"">
     //    <dict>
     //        <key>aps-environment</key>
     //        <string>development</string>
     //    </dict>
     //</plist>";
        
        void Create_IDFA_key(string pathToXcode, string IDFA_description)
        {

            // Get Plist from Xcode project
            string plistPath = pathToXcode + "/Info.plist";

            // Read in Plist
            PlistDocument plistObj = new PlistDocument();
            plistObj.ReadFromString(File.ReadAllText(plistPath));

            // Set values from the root obj
            PlistElementDict plistRoot = plistObj.root;

            // Set value in plist
            plistRoot.SetString("NSUserTrackingUsageDescription", IDFA_description);

            plistRoot.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");
            // Save
            File.WriteAllText(plistPath, plistObj.WriteToString());

        }

    }

}

#endif