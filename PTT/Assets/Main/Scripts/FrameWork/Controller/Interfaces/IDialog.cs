using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
	public class IDialog : MonoBehaviour
	{
		protected RectTransform _rt;
		protected string _name;
		public GameObject dialogView;

        public bool isEnter { get { return _isEnter; } }

        protected bool _isEnter = false;

		public bool UseBackBtn = false;

		public bool GuideInteractorInView = false;

		private static Event.ShowDialogMsg showDialogMsg = new Event.ShowDialogMsg();
		private static Event.HideDialogMsg hideDialogMsg = new Event.HideDialogMsg();

		public event OnCallBack exitCallBack;
		protected bool ignoreExit = false;

		[SerializeField]
		private List<Button> _exitBtn;

		void Awake()
		{
			if (dialogView == null)
				throw new System.NullReferenceException(string.Format("{0} dialogView Null", this.name));
		}

		public void Load()
		{
			_name = GetType().Name;
			_rt = GetComponent<RectTransform>();

            Message.AddListener<Event.ShowDialogMsg>(_name, Enter);
			Message.AddListener<Event.HideDialogMsg>(_name, Exit);

			int sibling = EnumExtensions.ParseToInt<UISibling>(_name);
			UIManager.Instance.SetSibling(_rt, sibling);

			dialogView.SetActive(false);

			if (GuideInteractorInView == true)
			{
				List<UIGuideInteractor> uIGuideInteractors = dialogView.transform.GetComponentsInAllChildren<UIGuideInteractor>();
				for (int i = 0; i < uIGuideInteractors.Count; ++i)
				{
					if (uIGuideInteractors[i].FocusParent == null)
						uIGuideInteractors[i].FocusParent = dialogView.transform;
				}
			}


			if (_exitBtn != null)
			{
				for (int i = 0; i < _exitBtn.Count; ++i)
				{
					if (_exitBtn[i] != null)
						_exitBtn[i].onClick.AddListener(Exit);
				}
			}

			OnLoad();
		}

        public void Load_Element()
        {
            _name = GetType().Name;
            _rt = GetComponent<RectTransform>();

            Message.AddListener<Event.ShowDialogMsg>(_name, Enter);
            Message.AddListener<Event.HideDialogMsg>(_name, Exit);

            dialogView.SetActive(false);

			if (_exitBtn != null)
			{
				for (int i = 0; i < _exitBtn.Count; ++i)
				{
					if (_exitBtn[i] != null)
						_exitBtn[i].onClick.AddListener(Exit);
				}
			}

			OnLoad();
        }

        protected virtual void OnLoad()
		{
		}

		public void Unload()
		{
			Message.RemoveListener<Event.ShowDialogMsg>(_name, Enter);
			Message.RemoveListener<Event.HideDialogMsg>(_name, Exit);

			OnExit();
			OnUnload();
		}

		protected virtual void OnUnload()
		{
		}

		private void Enter(Event.ShowDialogMsg msg)
		{
			Enter();
		}

		private void Enter()
		{
			if (UIManager.Instance.IgnoreEnterDialog == true)
				return;

			if (dialogView != null)
			{
				if (dialogView.activeSelf)
					return;
				dialogView.SetActive(true);
			}

			_isEnter = true;
			OnEnter();

			if (UseBackBtn == true)
				Managers.AOSBackBtnManager.Instance.EnterBackBtnAction(this);
		}

		public void ElementEnter()
		{
			Enter(null);
		}

		public virtual void BackKeyCall()
		{
			Exit();
		}

		private void Exit(Event.HideDialogMsg msg)
		{
			Exit();
		}

        private void Exit()
        {
			if (ignoreExit == true)
				return;

			if (dialogView != null)
				dialogView.SetActive(false);

			_isEnter = false;
			exitCallBack?.Invoke();
			OnExit();
		}

		public void ElementExit()
		{
			Exit();
		}

		protected virtual void OnDestroy()
        {
            Unload();
        }

		protected virtual void OnEnter()
		{
		}

		protected virtual void OnExit()
		{
		}

        public static void RequestDialogEnter<T>() where T : IDialog
		{
			Message.Send(typeof(T).Name, showDialogMsg);
		}

		public static void RequestDialogExit<T>() where T : IDialog
		{
            Message.Send(typeof(T).Name, hideDialogMsg);
		}
    }
}
