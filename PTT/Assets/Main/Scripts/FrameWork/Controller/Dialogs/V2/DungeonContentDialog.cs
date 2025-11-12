using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class DungeonContentDialog : IDialog
    {
        [SerializeField]
        private Transform m_elementRoot;

        [SerializeField]
        private UIDungeonContentElement m_uIDungeonContentElement;

        private Dictionary<DungeonData, UIDungeonContentElement> uIDungeonContentElements_Dic = new Dictionary<DungeonData, UIDungeonContentElement>();

        [SerializeField]
        private Transform _comingSoon;

        private GameBerry.Event.SetDungeonSelectMsg m_setDungeonSelectMsg = new GameBerry.Event.SetDungeonSelectMsg();

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            CreateUIDungeonContentElement();

            Message.AddListener<GameBerry.Event.SetDungeonContentDialogStateMsg>(SetDungeonContentDialogState);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetDungeonContentDialogStateMsg>(SetDungeonContentDialogState);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            foreach (var pair in uIDungeonContentElements_Dic)
            {
                pair.Value.SetDungeonMode(pair.Key.DungeonType);
            }
        }
        //------------------------------------------------------------------------------------
        private void CreateUIDungeonContentElement()
        {

            List<DungeonData> dungeonDatas = Managers.DungeonDataManager.Instance.GetDungeonAllData();

            for (int i = 0; i < dungeonDatas.Count; ++i)
            {
                DungeonData dungeonData = dungeonDatas[i];

                GameObject clone = Instantiate(m_uIDungeonContentElement.gameObject, m_elementRoot);
                if (clone == null)
                    break;

                UIDungeonContentElement uIDungeonContentElement = clone.GetComponent<UIDungeonContentElement>();
                uIDungeonContentElement.Init();
                uIDungeonContentElement.SetDungeonContentElement(dungeonData);

                uIDungeonContentElements_Dic.Add(dungeonData, uIDungeonContentElement);
            }

            if (_comingSoon != null)
                _comingSoon.transform.SetAsLastSibling();
        }
        //------------------------------------------------------------------------------------
        private void SetDungeonContentDialogState(GameBerry.Event.SetDungeonContentDialogStateMsg msg)
        {
            //int dungeon = 0;
            //dungeon = msg.ContentDetailList.Enum32ToInt() - ContentDetailList.Dungeon.Enum32ToInt() + 10;

            //Enum_Dungeon EnumDungeon = dungeon.IntToEnum32<Enum_Dungeon>();

            //OnClick_DungeonEnterBtn(EnumDungeon);
        }
        //------------------------------------------------------------------------------------
    }
}