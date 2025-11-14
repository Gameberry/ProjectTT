using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameBerry;
using GameBerry.Scene;
using System.Runtime.InteropServices;
using System;
using System.IO;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace GameBerry.Managers
{
    public class SceneManager : MonoSingleton<SceneManager>
    {
        public Constants.SceneName startScene;
        public Constants.SceneName nowScene;

        // TODO: 한번에 여러 씬을 로딩하는 경우를 위해 구현한 부분인데 이제는 그럴일이 없다. 수정하자.
        List<GameObject> _inSettingsScenes;
        int _inSettingsScenesCount;

        Dictionary<Constants.SceneName, GameObject> _scenes;
        LinkedList<Constants.SceneName> _showList;

        [SerializeField]
        private BuildEnvironmentSelectAsset _buildEnvironmentSelectAsset;
        public BuildEnvironmentEnum BuildElement = BuildEnvironmentEnum.Develop;

        Transform _root;

        public Transform DevTool;

        bool bPaused = false;  // 어플리케이션이 내려진 상태인지 아닌지의 스테이트를 저장하기 위한 변수

        //App초기화가 되었는지
        bool completeAppInit = false;

        private bool isCheatingUser = false;

        public string SpacialCharRegex = string.Empty;
        public string FranceEmptyStr = string.Empty;

        public bool AuthOverride = false;
        public string AuthOverrideID = string.Empty;

        [HideInInspector]
        public string IOSDeviceToken = string.Empty;

        public bool UseLocalChart = false;

        #region Initialize

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            Debug.LogWarning(string.Format("SystemLanguage : {0}", Application.systemLanguage));

            System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("en-US");

            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Application.targetFrameRate = 60;

            Input.multiTouchEnabled = false;
            _inSettingsScenes = new List<GameObject>();
            _scenes = new Dictionary<Constants.SceneName, GameObject>();
            _showList = new LinkedList<Constants.SceneName>();

            _root = GetComponent<Transform>();

            SoundManager.Instance.Setup();

            BuildElement = _buildEnvironmentSelectAsset.BuildElement;

#if DEV_DEFINE && !UNITY_EDITOR
            if (BuildElement == BuildEnvironmentEnum.Develop
                || BuildElement == BuildEnvironmentEnum.QA)
            {
                if (DevTool != null)
                    DevTool.gameObject.SetActive(true);
            }
#endif

#if UNITY_IOS
// 푸쉬 때문에 함수 추가. 토큰 받아오면 좋은거고 안되면 말고...
            StartCoroutine(RequestAuthorization());
#endif
            TableManager.Instance.LoadDeviceLocalString(() =>
            {
                LoadStartScene();
            }).Forget();
        }
        //------------------------------------------------------------------------------------
        void LoadStartScene()
        {
            if (startScene == Constants.SceneName.None)
                return;

            nowScene = startScene;
            var scene = GetRoot(nowScene);
            if (scene == null)
            {
                var fullpath = string.Format("Scenes/{0}", nowScene);
                StartCoroutine(ResourceLoader.Instance.LoadAsync<GameObject>(fullpath, o => OnPostLoadProcess(o)));
            }
        }
        //------------------------------------------------------------------------------------
        protected override void Release()
        {
            UnloadAll();
        }
        //------------------------------------------------------------------------------------
#if UNITY_IOS
        IEnumerator RequestAuthorization()
        {
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
            using (var req = new AuthorizationRequest(authorizationOption, true))
            {
            Debug.Log("Start Get Token");

                while (!req.IsFinished)
                {

                Debug.Log("!req.IsFinished");
                    yield return null;
                };

                string res = "\n RequestAuthorization:";
                res += "\n finished: " + req.IsFinished;
                res += "\n granted :  " + req.Granted;
                res += "\n error:  " + req.Error;
                res += "\n deviceToken:  " + req.DeviceToken;

                IOSDeviceToken = req.DeviceToken;
                Debug.Log(res);
            }
        }
#endif

        //------------------------------------------------------------------------------------
#endregion
        //------------------------------------------------------------------------------------
        // 혹시라도 yield return StartCoroutine 사용해서 싱글톤 초기화를 할 수 있기 때문에 IEnumerator로 냅둠
        //IEnumerator InitializeApp()
        //{
        //    bool completeString = false;

        //    // 디바이스에 있는 스트링만 로드하자
        //    TableManager.Instance.LoadDeviceLocalString(() =>
        //    {
        //        completeString = true;
        //    }).Forget();

        //    while (completeString == false)
        //        yield return null;

        //    completeAppInit = true;

        //    LoadStartScene();
        //}
        //------------------------------------------------------------------------------------
        GameObject GetRoot(Constants.SceneName sceneName)
        {
            GameObject scene;
            _scenes.TryGetValue(sceneName, out scene);
            return scene;
        }
        //------------------------------------------------------------------------------------
        public T GetScript<T>(Constants.SceneName sceneName)
        {
            var scene = GetRoot(sceneName);
            if (scene != null)
                return scene.GetComponent<T>();

            return default(T);
        }
        //------------------------------------------------------------------------------------
        public T GetScript<T>()
        {
            var scene = GetRoot(EnumExtensions.Parse<Constants.SceneName>(typeof(T).Name.Replace("Scene", "")));
            if (scene != null)
                return scene.GetComponent<T>();

            return default(T);
        }
        //------------------------------------------------------------------------------------
#region Load/Unload
        //------------------------------------------------------------------------------------
        public bool Load(Constants.SceneName sceneName, bool unload = true)
        {
            if (sceneName == nowScene)
                return false;

            LoadRoot(sceneName, unload);
            return true;
        }
        //------------------------------------------------------------------------------------
        void LoadRoot(Constants.SceneName sceneName, bool unload)
        {
            if (unload)
                UnloadAll();

            nowScene = sceneName;
            var scene = GetRoot(sceneName);
            if (scene == null)
            {
                var fullpath = string.Format("Scenes/{0}", sceneName);
                StartCoroutine(ResourceLoader.Instance.LoadAsync<GameObject>(fullpath, o => OnPostLoadProcess(o)));
            }
            else
            {
                var sceneScript = scene.GetComponent<IScene>();
                SetupScene(scene, sceneScript.contentsList);
            }
        }
        //------------------------------------------------------------------------------------
        void OnPostLoadProcess(UnityEngine.Object o)
        {
            var scene = Instantiate(o) as GameObject;

            var sceneScript = scene.GetComponent<IScene>();
            scene.name = sceneScript.sceneName.ToString();
            scene.transform.SetParent(_root);

            _scenes.Add(sceneScript.sceneName, scene);
            SetupScene(scene, sceneScript.contentsList);
        }
        //------------------------------------------------------------------------------------
        void SetupScene(GameObject scene, List<string> enterContentList)
        {
            _inSettingsScenes.Add(scene);
            _inSettingsScenesCount++;

            scene.SetActive(true);

            var sceneScript = scene.GetComponent<IScene>();
            sceneScript.LoadAssets(enterContentList,
                () =>
                {
                    BringToTop(sceneScript.sceneName);
                    _inSettingsScenes.Remove(scene);
                });
        }
        //------------------------------------------------------------------------------------
        public void Unload(Constants.SceneName sceneName)
        {
            var scene = GetRoot(sceneName);
            if (scene != null)
            {
                scene.GetComponent<IScene>().Unload();

                _scenes.Remove(sceneName);
                _showList.Remove(sceneName);

                var fullpath = string.Format("Scenes/{0}", scene.name);
                if (ResourceLoader.isAlive)
                    ResourceLoader.Instance.Unload(fullpath);
            }
        }
        //------------------------------------------------------------------------------------
        public void UnloadAll()
        {
            LinkedListNode<Constants.SceneName> node;

            while (true)
            {
                node = _showList.First;
                if (node == null)
                    break;

                Unload(node.Value);
            }
        }
        //------------------------------------------------------------------------------------
#endregion
        //------------------------------------------------------------------------------------
        public void Show(Constants.SceneName sceneName)
        {
            BringToTop(sceneName);
        }
        //------------------------------------------------------------------------------------
        void BringToTop(Constants.SceneName sceneName)
        {
            _showList.Remove(sceneName);
            _showList.AddFirst(sceneName);

            var scene = GetRoot(sceneName);
            scene.transform.SetAsFirstSibling();
        }

        void OnApplicationPause(bool pause)
        {
            if (pause)
            {


                bPaused = true;

                // todo : 어플리케이션을 내리는 순간에 처리할 행동들 /

                //if (QuestManager.isAlive == true)
                //    QuestManager.Instance.ForceSavePlayTime();

#if !UNITY_IOS
                if (GameBerry.TheBackEnd.TheBackEndManager.isAlive == true)
                    GameBerry.TheBackEnd.TheBackEndManager.Instance.SetApplicationPause(true);

                Managers.LocalNoticeManager.Instance.SetNotice();
#endif
            }
            else
            {
                if (bPaused)
                {
                    bPaused = false;

                    //todo : 내려놓은 어플리케이션을 다시 올리는 순간에 처리할 행동들 

                    //if (GameChatManager.isAlive == true)
                    //    GameChatManager.Instance.CheckAdminChat();

                    if (GameBerry.TheBackEnd.TheBackEndManager.isAlive == true)
                        GameBerry.TheBackEnd.TheBackEndManager.Instance.SetApplicationPause(false);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void OnApplicationQuit()
        {
            //if (QuestManager.isAlive == true)
            //    QuestManager.Instance.ForceSavePlayTime();
            Managers.LocalNoticeManager.Instance.SetNotice();
            if (GameBerry.TheBackEnd.TheBackEndManager.isAlive == true)
                GameBerry.TheBackEnd.TheBackEndManager.Instance.SetApplicationQuit();

            Application.Quit();
        }
        //------------------------------------------------------------------------------------
        public void OnApplicationRestart()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                AndroidJavaObject pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                AndroidJavaObject intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);
                intent.Call<AndroidJavaObject>("setFlags", 0x20000000);//Intent.FLAG_ACTIVITY_SINGLE_TOP

                AndroidJavaClass pendingIntent = new AndroidJavaClass("android.app.PendingIntent");
                AndroidJavaObject contentIntent = pendingIntent.CallStatic<AndroidJavaObject>("getActivity", currentActivity, 0, intent, 0x8000000); //PendingIntent.FLAG_UPDATE_CURRENT = 134217728 [0x8000000]
                AndroidJavaObject alarmManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "alarm");
                AndroidJavaClass system = new AndroidJavaClass("java.lang.System");
                long currentTime = system.CallStatic<long>("currentTimeMillis");
                alarmManager.Call("set", 1, currentTime + 1000, contentIntent); // android.app.AlarmManager.RTC = 1 [0x1]

                Debug.LogError("alarm_manager set time " + currentTime + 1000);
                currentActivity.Call("finish");

                AndroidJavaClass process = new AndroidJavaClass("android.os.Process");
                int pid = process.CallStatic<int>("myPid");
                process.CallStatic("killProcess", pid);
            }
        }
        //------------------------------------------------------------------------------------
        public void GoStore()
        {
#if UNITY_ANDROID
            //Application.OpenURL(string.Format("market://details?id={0}", Application.identifier));
            Application.OpenURL(string.Format("https://play.google.com/store/apps/details?id=studio.arr.barbar"));
            
#elif UNITY_IPHONE
            Application.OpenURL("https://itunes.apple.com/kr/app/apple-store/id6468973299");
#else
            Application.OpenURL(string.Format("http://play.google.com/store/apps/details?id={0}", Application.identifier));
#endif
        }
        //------------------------------------------------------------------------------------
        public void GoDiscode()
        {
            Application.OpenURL(string.Format("https://discord.gg/trFMWHh3f6"));
        }
        //------------------------------------------------------------------------------------
        public void GoTerms()
        {
            LocalizeType localizeType = Managers.LocalStringManager.Instance.GetLocalizeType();

            //if (localizeType == LocalizeType.Korean)
            //{
            //    Application.OpenURL("https://gameberrystudio.oqupie.com/portals/2456/customer-news/3315");
            //}
            //else
            //    Application.OpenURL("https://gameberrystudio.oqupie.com/portals/2457/articles/52928");

            Application.OpenURL("https://monophobia.xyz/privacy_policy.html");
        }
        //------------------------------------------------------------------------------------
        public void GoPrivacy()
        {
            LocalizeType localizeType = Managers.LocalStringManager.Instance.GetLocalizeType();

            //if (localizeType == LocalizeType.Korean)
            //{
            //    Application.OpenURL("https://gameberry.studio/policy.html");
            //}
            //else
            //    Application.OpenURL("https://gameberry.studio/privacy_policy_en.html");

            Application.OpenURL("https://monophobia.xyz/privacy_policy.html");
        }
        //------------------------------------------------------------------------------------
        public void GoOqupie()
        {
            LocalizeType localizeType = Managers.LocalStringManager.Instance.GetLocalizeType();

            if (localizeType == LocalizeType.Korean)
            {
                Application.OpenURL("https://gameberrystudio.oqupie.com/portals/2456");
            }
            else
                Application.OpenURL("https://gameberrystudio.oqupie.com/portals/2457");
        }
        //------------------------------------------------------------------------------------
        public void GoCommunity()
        {
            LocalizeType localizeType = Managers.LocalStringManager.Instance.GetLocalizeType();

            if (localizeType == LocalizeType.Korean)
            {
                GoLounge();
            }
            else
                GoDiscode();
        }
        //------------------------------------------------------------------------------------
        public void GoLounge()
        {
            Application.OpenURL("https://game.naver.com/lounge/Dark_Clan/board/10");
        }
        //------------------------------------------------------------------------------------
        public void GoPlayGooglePlayServiceUpdate()
        {
            Application.OpenURL("https://play.google.com/store/apps/details?id=com.google.android.gms");
        }
        //------------------------------------------------------------------------------------
    }
}
