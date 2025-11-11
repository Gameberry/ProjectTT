using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(CButton))]
public class CButtonEditor : ButtonEditor
{
    SerializedProperty _btn_Sprite;
    SerializedProperty _btn_Dimmed_Sprite;
    SerializedProperty _text;
    SerializedProperty _text_Color;
    SerializedProperty _text_Dimmed_Color;

    private CButton btn;


    protected override void OnEnable()
    {
        base.OnEnable();

        // 여기까지하면 원래 Button이 나옴
        _btn_Sprite = serializedObject.FindProperty(nameof(btn._btn_Sprite));
        _btn_Dimmed_Sprite = serializedObject.FindProperty(nameof(btn._btn_Dimmed_Sprite));
        _text = serializedObject.FindProperty(nameof(btn._text));
        _text_Color = serializedObject.FindProperty(nameof(btn._text_Color));
        _text_Dimmed_Color = serializedObject.FindProperty(nameof(btn._text_Dimmed_Color));
        btn = target as CButton;
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();


        // 여기부터
        EditorGUILayout.PropertyField(_btn_Sprite);
        EditorGUILayout.PropertyField(_btn_Dimmed_Sprite);
        EditorGUILayout.PropertyField(_text);
        EditorGUILayout.PropertyField(_text_Color);
        EditorGUILayout.PropertyField(_text_Dimmed_Color);
        // 여기까지 커스텀

        if (EditorApplication.isPlaying == false)
            btn.ChangeDimmedResource(btn.interactable);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(20);


        base.OnInspectorGUI();

    }
}
