using UnityEngine;
using UnityEditor;

public static class RemoveMissingComponents
{
    [MenuItem("GameObject/Remove Missing Components (Recursive)", false, 0)]
    private static void Execute()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogWarning("[RemoveMissingComponents] 선택된 오브젝트가 없습니다.");
            return;
        }

        int total = 0;
        var transforms = go.GetComponentsInChildren<Transform>(true);

        foreach (var t in transforms)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            if (count > 0)
            {
                Undo.RegisterCompleteObjectUndo(t.gameObject, "Remove Missing Components");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                total += count;
            }
        }

        Debug.Log($"[RemoveMissingComponents] '{go.name}' 하위에서 Missing Component {total}개 제거 완료.");
    }

    [MenuItem("GameObject/Remove Missing Components (Recursive)", true)]
    private static bool Validate() => Selection.activeGameObject != null;
}