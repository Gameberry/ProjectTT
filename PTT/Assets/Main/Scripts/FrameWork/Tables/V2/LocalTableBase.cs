using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class LocalTableBase
    {
        protected List<string> TableList = new List<string>();

        public virtual async UniTask InitData_Async()
        {
            UnityEngine.Debug.LogWarning("이거 호출");
            await UniTask.Yield();
            await UniTask.NextFrame();
        }
    }
}