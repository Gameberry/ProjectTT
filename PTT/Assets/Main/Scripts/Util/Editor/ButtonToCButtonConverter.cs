#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ButtonToCButtonConverter : MonoBehaviour
{
    [MenuItem("Tools/Convert Button to CButton")]
    static void ConvertButtonToCButton()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Button oldButton = obj.GetComponent<Button>();
            if (oldButton != null)
            {
                // 기존 설정 복사
                var transition = oldButton.transition;
                var colors = oldButton.colors;
                var navigation = oldButton.navigation;
                var interactable = oldButton.interactable;
                var onClick = oldButton.onClick;
                var targetGraphic = oldButton.targetGraphic;

                // 기존 Button 삭제
                DestroyImmediate(oldButton);

                // 새 CButton 추가
                var newButton = obj.AddComponent<CButton>();

                // 복사한 설정 적용
                newButton.transition = transition;
                newButton.colors = colors;
                newButton.navigation = navigation;
                newButton.interactable = interactable;
                newButton.onClick = onClick;
                newButton.targetGraphic = targetGraphic;

                Debug.Log($"Converted {obj.name} to CButton!");
            }
        }
    }
}
#endif
