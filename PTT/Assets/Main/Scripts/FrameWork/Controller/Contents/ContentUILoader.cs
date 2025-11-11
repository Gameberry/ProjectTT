using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GameBerry.UI;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace GameBerry.Contents
{
    public class ContentUILoader : MonoBehaviour
    {
        public List<string> _uiList = new List<string>();

        [HideInInspector]
        public List<IDialog> dialogList = new List<IDialog>();

        private string _name;

		int _loadingCount = 0;
		Action _onLoadComplete;

		string tableLoadLocalString = string.Empty;

		public virtual void Load(Action loadComplete)
		{
			_onLoadComplete = loadComplete;
			_loadingCount = _uiList.Count;
            _name = GetComponent<IContent>().ContentName;

			tableLoadLocalString = Managers.LocalStringManager.Instance.GetLocalString("common/gameLoading");

			//Parallel.For(0, _uiList.Count,
			//	   i => {
			//		   UIManager.Instance.Load(_uiList[i], _name, OnUILoadComplete);
			//	   });

			//for (int i = 0; i < _uiList.Count; i++)
			//    UIManager.Instance.Load(_uiList[i], _name, OnUILoadComplete);

			StartCoroutine(Loading());

			//Loading().Forget();
		}

		public virtual void Unload()
		{
			if (!UIManager.isAlive)
				return;

            dialogList.Clear();

			for (int i = 0; i < _uiList.Count; i++)
			{
				GameObject ui = UIManager.Instance.Get(_uiList[i]);
				if (ui != null)
					ui.GetComponent<IDialog>().Unload();
				
				UIManager.Instance.Unload(_uiList[i]);
			}
		}

		public static GameBerry.Event.SetNoticeMsg m_setNoticeMsg = new GameBerry.Event.SetNoticeMsg();

		void OnUILoadComplete(GameObject ui)
		{
			ui.SetActive(true);

			var dialog = ui.GetComponent<IDialog>();
			dialog.Load();

            dialogList.Add(dialog);

            _loadingCount--;

            m_setNoticeMsg.NoticeStr = string.Format("{0} {1}%", tableLoadLocalString, (int)(((float)(_uiList.Count - _loadingCount) / (float)_uiList.Count) * 100.0f));

            Message.Send(m_setNoticeMsg);

            //Debug.LogFormat("UI load complete) Name: {0}", ui.name);
		}

        //private async UniTask Loading()
        //{
        //	for (int i = 0; i < _uiList.Count; i++)
        //	{
        //		await UIManager.Instance.Load_Async(_uiList[i], _name, OnUILoadComplete);
        //	}

        //	if (_onLoadComplete != null)
        //		_onLoadComplete();
        //}

        IEnumerator Loading()
        {
            for (int i = 0; i < _uiList.Count; i++)
            {
                UIManager.Instance.Load(_uiList[i], _name, OnUILoadComplete);
//#if !UNITY_EDITOR
				if (i % 2 == 0)
				    yield return null;
				//yield return null;
//#endif
            }

            yield return new WaitWhile(() => _loadingCount > 0);

            if (_onLoadComplete != null)
                _onLoadComplete();
        }

        public float CalculateLoadingProgress()
		{
			if (!UIManager.isAlive)
				return 0.0f;
			
			var progress = 0.0f;

			if (_uiList.Count > 0)
			{
				for (int i = 0; i < _uiList.Count; ++i)
				{
					progress += UIManager.Instance.GetProgress(_uiList[i]);
				}

				progress /= (float)_uiList.Count;
			}
			else
			{
				progress = 1.0f;
			}

			return progress;
		}
	}
}
