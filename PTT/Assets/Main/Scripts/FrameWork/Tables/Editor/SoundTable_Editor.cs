using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace GameBerry
{
    [CustomEditor(typeof(SoundTableAsset), true)]

    public class SoundTable_Editor : UnityEditor.Editor
    {
        BEditorWindow bEditorWindow;

        int selectIdx = -1;

        List<SoundData> m_soundDatas = new List<SoundData>();

        public override void OnInspectorGUI()
        {
            SoundTableAsset myScript = (SoundTableAsset)target;

            selectIdx = EditorGUILayout.IntField("SelectIdx", selectIdx);

            if (GUILayout.Button("InsertNewData"))
            {
                myScript.InsertIndex(selectIdx);
            }

            if (GUILayout.Button("DeleteData"))
            {
                myScript.DeleteIndex(selectIdx);
            }

            if (GUILayout.Button("SetAllData"))
            {
                m_soundDatas.Clear();
                var path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
                SetSoundDatas("Assets/AssetBundle/TableResources/Sound");
                myScript.SetAllData(m_soundDatas);
                m_soundDatas.Clear();
            }

            if (GUILayout.Button("AddAudioClip"))
            {
                bEditorWindow = new BEditorWindow();
                bEditorWindow.SetAudioClipRecvCallBack(RecvAddAnimationSprite);
                bEditorWindow.Show();
            }

            DrawDefaultInspector();
        }

        private void RecvAddAnimationSprite(List<AudioClip> textures)
        {
            SoundTableAsset myScript = (SoundTableAsset)target;
            myScript.AddAudioClip(textures);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(myScript);
#endif

            bEditorWindow.Close();
            bEditorWindow = null;
        }

        public void SetSoundDatas(string path)
        {
            System.IO.DirectoryInfo Info = new System.IO.DirectoryInfo(path);

            if (Info.Exists)
            {
                System.IO.DirectoryInfo[] CInfo = Info.GetDirectories("*", System.IO.SearchOption.AllDirectories);
                if (CInfo.Length == 0)
                {
                    foreach (System.IO.FileInfo File in Info.GetFiles())
                    {
                        if (File.Name.Contains("meta"))
                            continue;

                        if (File.Name.Contains(".mp3") == false && File.Name.Contains(".wav") == false)
                            continue;

                        string FullFileName = File.FullName;
                        FullFileName = FullFileName.Replace("\\", "/");

                        string FileNameOnly = Path.GetFileNameWithoutExtension(FullFileName);
                        int pathIndex = FullFileName.IndexOf("Assets");
                        FullFileName = FullFileName.Substring(pathIndex, FullFileName.Length - pathIndex);

                        FullFileName = FullFileName.Replace("Assets/AssetBundle/", "");
                        FullFileName = FullFileName.Replace(File.Name, "");

                        SoundData data = new SoundData();
                        data.FilePath = FullFileName;
                        data.FileName = FileNameOnly;
                        if (data.FilePath.Contains("BG") == true)
                            data.Loop = true;
                        else
                            data.Loop = false;

                        m_soundDatas.Add(data);
                    }
                }
                else
                {
                    foreach (System.IO.DirectoryInfo info in CInfo)
                    {
                        string temp = info.FullName;
                        temp = temp.Replace("\\", "/");
                        SetSoundDatas(temp);
                    }
                }
            }
        }
    }
}
