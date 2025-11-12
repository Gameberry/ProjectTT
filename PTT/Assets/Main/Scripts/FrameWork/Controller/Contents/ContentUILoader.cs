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

			for (int i = 0; i < _uiList.Count; i++)
			{
                IDialog ui = UIManager.Instance.Get(_uiList[i]);
				if (ui != null)
					ui.Unload();
				
				UIManager.Instance.Unload(_uiList[i]);
			}
		}

		public static GameBerry.Event.SetNoticeMsg m_setNoticeMsg = new GameBerry.Event.SetNoticeMsg();

		void OnUILoadComplete(IDialog dialog)
		{
            _loadingCount--;

            m_setNoticeMsg.NoticeStr = string.Format("{0} {1}%", tableLoadLocalString, (int)(((float)(_uiList.Count - _loadingCount) / (float)_uiList.Count) * 100.0f));

            Message.Send(m_setNoticeMsg);
		}

        IEnumerator Loading()
        {
            for (int i = 0; i < _uiList.Count; i++)
            {
                UIManager.Instance.Load(_uiList[i], OnUILoadComplete);
//#if !UNITY_EDITOR
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
