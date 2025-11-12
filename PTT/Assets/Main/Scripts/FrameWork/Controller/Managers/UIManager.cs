using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
	public class UIManager : MonoSingleton<UIManager>
	{
		public Canvas damageCanvas;
		public Transform DamageCanvasContent;

		public Canvas screenCanvas;
        public CanvasScaler screenCanvasScaler;
        public Camera screenCanvasCamera;
        public Transform screenCanvasContent;
        public Transform screenCanvasGlobalContent;
		public RectTransform screenCanvasRect;

		public Transform ProjectLoadingContent;

		public Transform screenCanvasPowerSavingContent;

        private Dictionary<string, IDialog> _uis;

		public delegate void OnComplete(IDialog ui);

        private Dictionary<string, System.Delegate> _onCompleteEvents;

		public bool IgnoreEnterDialog = false;

        private const string ASSET_PATH = "ContentResources";

        protected override void Init()
		{
			_uis = new Dictionary<string, IDialog>();
			_onCompleteEvents = new Dictionary<string, System.Delegate>();
		}

		protected override void Release()
		{
			UnloadAll();
		}

		public IDialog Get(string uiName)
		{
            IDialog ui;
			_uis.TryGetValue(uiName, out ui);
			return ui;
		}

		public async UniTask Load_Async(string uiName, OnComplete onComplete)
		{
			AddEvent(uiName, onComplete);

            IDialog ui = Get(uiName);
			if (ui == null)
			{
                var bundleName = string.Format("{0}/Dialogs/{1}", ASSET_PATH, uiName);
				await ResourceLoader.Instance.LoadAsync<GameObject>(bundleName, OnPostLoadProcess);
			}
			else
			{
				ui.gameObject.SetActive(true);

				AttachToCanvas(ui);
				RaiseEvent(ui);
			}
		}

        public static void DialogEnter<T>() where T : IDialog
        {
            Instance.DialogEnter(typeof(T).Name);
        }

        public void DialogEnter(string uiName)
        {
            IDialog ui = Get(uiName);
            if (ui == null)
            {
				Load(uiName, o =>
                {
                    o.Enter();
                });
			}
            else
            {
                ui.Enter();
			}
        }

        public static void DialogExit<T>() where T : IDialog
        {
            Instance.DialogExit(typeof(T).Name);
        }

		public void DialogExit(string uiName)
        {
            Get(uiName)?.Exit();
        }

		public void Load(string uiName, OnComplete onComplete)
		{
			AddEvent(uiName, onComplete);

            IDialog ui = Get(uiName);
			if (ui == null)
			{
                var bundleName = string.Format("{0}/Dialogs/{1}", ASSET_PATH, uiName);
                ResourceLoader.Instance.Load<GameObject>(bundleName, OnPostLoadProcess);
			}
			else
			{
				ui.gameObject.SetActive(true);

				AttachToCanvas(ui);
				RaiseEvent(ui);
			}
		}

        private void OnPostLoadProcess(Object o)
		{
			var ui = Instantiate(o) as GameObject;

			ui.SetActive(true);
			ui.name = o.name;

            Common.ShaderHelper.SetupShader(ui);

            IDialog dialog = ui.GetComponent<IDialog>();

			AttachToCanvas(dialog);

			_uis.Add(ui.name, dialog);

            dialog.Load();

			RaiseEvent(dialog);
		}

        private void AttachToCanvas(IDialog ui)
		{
			int sibling = EnumExtensions.ParseToInt<UISibling>(ui.name);

			var obj = GetGameObjectBySiblingIndex(sibling);
			ui.transform.SetParent(obj.transform, false);
			ui.gameObject.SetLayerInChildren(obj.layer);

			//ui.transform.localPosition = Vector3.zero;
			ui.transform.localScale = Vector3.one;
		}

		public void BackgroundLoad(string uiName)
		{
			// TODO: Background Load 기능 추가 구현
		}

        // 현재 사용하지 않음
		public float GetProgress(string uiName)
		{
            IDialog ui;
			_uis.TryGetValue(uiName, out ui);
			if (ui != null)
				return 1.0f;

			float progress = 0.0f;
			var fullpaths = $"{ASSET_PATH}{uiName}";
			progress = ResourceLoader.Instance.GetProgress(fullpaths);

			return progress * 0.9f; // 리소스 로딩 90%, OnPostLoadProcess()에서 후처리 10%
		}

		public void Unload(string uiName)
		{
            IDialog ui = Get(uiName);
			if (ui != null)
			{				
				Destroy(ui);
				_uis.Remove(uiName);

				var fullpath = string.Format("{0}{1}", ASSET_PATH, uiName);
				if (ResourceLoader.isAlive)
					ResourceLoader.Instance.Unload(fullpath);
			}
		}

		public void UnloadAll()
		{
			foreach (var ui in _uis)
			{
				Destroy(ui.Value);
			}
			_uis.Clear();
		}

        private void AddEvent(string uiName, OnComplete onComplete)
		{
			if (onComplete == null)
				return;

			System.Delegate events;
			if (_onCompleteEvents.TryGetValue(uiName, out events))
			{
				_onCompleteEvents[uiName] = (OnComplete)_onCompleteEvents[uiName] + onComplete;
			}
			else
			{
				_onCompleteEvents.Add(uiName, onComplete);
			}
		}

        private void RaiseEvent(IDialog ui)
		{
			System.Delegate events;
			_onCompleteEvents.TryGetValue(ui.name, out events);
			if (events == null)
				return;

			var onComplete = (OnComplete)events;
			onComplete(ui);
             
			_onCompleteEvents.Remove(ui.name);
		}

		public void LastSibling(string uiName)
		{
            IDialog ui;
			_uis.TryGetValue(uiName, out ui);

			if (ui != null)
				ui.transform.SetSiblingIndex(_uis.Count);
		}

		public void SetSibling(RectTransform source, int sourceSibling)
		{
//			Debug.LogFormat("This Target SIBLING {0}-[{1}]", sourceSibling, source.name);
			bool addChild = false;

			var siblingObj = GetGameObjectBySiblingIndex(sourceSibling);
			source.gameObject.layer = siblingObj.gameObject.layer;
			Transform ctf = siblingObj.transform;

			for (int i = 0; i < ctf.childCount; ++i)
			{					
				var dest = ctf.GetChild(i);
				if (dest == source)
					continue;

				int destSibling = EnumExtensions.ParseToInt<UISibling>(dest.name);
				if (destSibling > sourceSibling)
				{
//					Debug.LogFormat("Sort SIBLING {0}-{1}-{2}", destSibling, dest.GetSiblingIndex(), dest.name);
					source.SetSiblingIndex(dest.GetSiblingIndex());			
					addChild = true;
					break;
				}
				else
				{
//					Debug.LogFormat("PASS SIBLING {0}-{1}-{2}", destSibling, dest.GetSiblingIndex(), dest.name);
					continue;
				}
			}

			if (!addChild)
			{
				source.SetAsLastSibling();
			}
		}

        private GameObject GetGameObjectBySiblingIndex(int index)
		{
			GameObject obj = null;
            if (1 <= index && index < 500)
                obj = screenCanvasContent.gameObject;
            else if (501 <= index && index < 800)
                obj = screenCanvasGlobalContent.gameObject;
			else if (801 <= index && index < 1000)
				obj = screenCanvasPowerSavingContent.gameObject;
			return obj;
		}
	}
}
