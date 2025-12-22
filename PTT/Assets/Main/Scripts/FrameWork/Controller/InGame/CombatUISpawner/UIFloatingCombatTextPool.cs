using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.UI
{
    public sealed class UIFloatingCombatTextPool : MonoBehaviour
    {
        [SerializeField] private UIFloatingCombatText prefab;
        [SerializeField] private RectTransform parent;
        [SerializeField] private int prewarm = 40;

        private readonly Queue<UIFloatingCombatText> _pool = new();

        public void Init()
        {
            if (parent == null) parent = (RectTransform)transform;

            for (int i = 0; i < prewarm; i++)
                Return(CreateNew());
        }

        UIFloatingCombatText CreateNew()
        {
            var inst = Instantiate(prefab, parent);
            inst.gameObject.SetActive(false);
            inst.BindPool(this);
            return inst;
        }

        public UIFloatingCombatText Rent()
        {
            return _pool.Count > 0 ? _pool.Dequeue() : CreateNew();
        }

        public void Return(UIFloatingCombatText item)
        {
            if (item == null) return;
            item.transform.SetParent(parent, false);
            item.gameObject.SetActive(false);
            _pool.Enqueue(item);
        }
    }

}
