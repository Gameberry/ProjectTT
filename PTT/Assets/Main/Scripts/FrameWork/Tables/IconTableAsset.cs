using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    [System.Serializable]
    public class IconData
    {
        public string Key;
        public Sprite Icon;
    }

    [CreateAssetMenu(fileName = "IconTable", menuName = "Table/IconTable")]
    public class IconTableAsset : ScriptableObject
    {
        public List<IconData> m_iconDatas = new List<IconData>();
        private Dictionary<string, IconData> m_iconDatas_Dic = new Dictionary<string, IconData>();

        //------------------------------------------------------------------------------------
        void OnEnable()
        {
            for (int i = 0; i < m_iconDatas.Count; ++i)
            {
                m_iconDatas_Dic[m_iconDatas[i].Key] = m_iconDatas[i];
            }
        }
        //------------------------------------------------------------------------------------
        public Sprite GetIcon(string key)
        {
            if (m_iconDatas_Dic.ContainsKey(key) == true)
                return m_iconDatas_Dic[key].Icon;

            return null;
        }
        //------------------------------------------------------------------------------------
        public void AddSprite(List<Sprite> textures, string groupname)
        {
            if (textures == null)
                return;

            if (textures.Count <= 0)
                return;

            List<string> containString = new List<string>();

            for (int i = 0; i < textures.Count; ++i)
            {
                string key = textures[i].name;
                if (string.IsNullOrEmpty(groupname) == true)
                {
                    key = textures[i].name;
                }
                else
                {
                    if (groupname.Contains("{0}"))
                    {
                        key = string.Format(groupname, key);
                    }
                    else
                    {
                        key = string.Format("{0}{1}", groupname, key);
                    }
                }

                IconData iconData = m_iconDatas.Find(x => x.Key == key);

                if (iconData != null)
                {
                    containString.Add(key);
                }
            }

            if (containString.Count > 0)
            {
                Debug.LogError("해당 Key가 이미 있습니다. 아래 로그를 확인해주세요!");

                for (int i = 0; i < containString.Count; ++i)
                {
                    Debug.LogError(containString[i]);
                }

                return;
            }

            for (int i = 0; i < textures.Count; ++i)
            {
                IconData iconData = new IconData();
                iconData.Icon = textures[i];

                string key = textures[i].name;
                if (string.IsNullOrEmpty(groupname) == true)
                {
                    key = textures[i].name;
                }
                else
                {
                    if (groupname.Contains("{0}"))
                    {
                        key = string.Format(groupname, key);
                    }
                    else
                    {
                        key = string.Format("{0}{1}", groupname, key);
                    }
                }

                iconData.Key = key;

                m_iconDatas.Add(iconData);
            }
        }
        //------------------------------------------------------------------------------------
        public void SortKey()
        {
            m_iconDatas.Sort((x, y) =>
            {
                return x.Key.CompareTo(y.Key);
            });
        }
        //------------------------------------------------------------------------------------
    }
}