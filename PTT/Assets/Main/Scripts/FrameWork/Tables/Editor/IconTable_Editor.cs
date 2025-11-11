using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameBerry
{
    [CustomEditor(typeof(IconTableAsset), true)]
    public class IconTable_Editor : UnityEditor.Editor
    {
        BEditorWindow bEditorWindow;

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("AddSprite"))
            {
                bEditorWindow = new BEditorWindow();
                bEditorWindow.EnableLabel();
                bEditorWindow.SetSpriteRecvCallBack_WithName(RecvAddAnimationSprite);
                bEditorWindow.Show();
            }

            if (GUILayout.Button("SortSprite"))
            {
                IconTableAsset myScript = (IconTableAsset)target;
                myScript.SortKey();
            }

            DrawDefaultInspector();
        }

        private void RecvAddAnimationSprite(List<Sprite> textures, string name)
        {
            IconTableAsset myScript = (IconTableAsset)target;
            myScript.AddSprite(textures, name);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(myScript);
#endif

            bEditorWindow.Close();
            bEditorWindow = null;
        }
    }
}
