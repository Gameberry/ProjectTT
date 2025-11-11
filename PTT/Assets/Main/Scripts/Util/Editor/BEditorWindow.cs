using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

#if UNITY_EDITOR



public class BEditorWindow : EditorWindow
{
    System.Action<List<Sprite>> m_callBack_Sprite = null;
    System.Action<List<Sprite>, string> m_callBack_Sprite_WithString = null;

    System.Action<List<ParticleSystem>> m_callBack_ParticleSystem = null;
    
    System.Action<List<AudioClip>> m_callBack_AudioClip = null;

    List<Sprite> outputSprite = new List<Sprite>();
    List<ParticleSystem> outputParticle = new List<ParticleSystem>();

    List<AudioClip> outputAudioClip = new List<AudioClip>();

    [MenuItem("Tools/DragNDropWindow")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(BEditorWindow)).Show();
    }
    void OnEnable()
    {
        wantsMouseMove = true;
    }
    protected void OnDisable()
    {
        outputSprite.Clear();
        outputParticle.Clear();
    }

    private bool ShowLabel = false;

    private string myTextFieldReturn = "Input Name";

    public void EnableLabel()
    {
        ShowLabel = true;
    }

    public void SetSpriteRecvCallBack_WithName(System.Action<List<Sprite>, string> callback)
    {
        m_callBack_Sprite_WithString = callback;

    }

    public void SetSpriteRecvCallBack(System.Action<List<Sprite>>  callback)
    {
        m_callBack_Sprite = callback;
    }

    public void SetParticleRecvCallBack(System.Action<List<ParticleSystem>> callback)
    {
        m_callBack_ParticleSystem = callback;
    }

    public void SetAudioClipRecvCallBack(System.Action<List<AudioClip>> callback)
    {
        m_callBack_AudioClip = callback;
    }
    

    public void ShowOverMouse()
    {
        Vector2 pos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        Vector2 size = Vector2.zero;
        size.x = 500;
        size.y = 500;

        //position = Rect(pos, size);

    }


    private void OnGUI()
    {
        Focus();

        bool isClick = false;

        var ButtonArea = GUILayoutUtility.GetRect(100, 100, 30, 30);
        isClick = GUI.Button(ButtonArea, "OnComplete");

        if (ShowLabel == true)
        {
            var inputStringArea = GUILayoutUtility.GetRect(100, 100, 30, 30);
            myTextFieldReturn = GUI.TextField(inputStringArea, myTextFieldReturn);
        }
        else
            myTextFieldReturn = string.Empty;

        var dropArea = GUILayoutUtility.GetRect(1000, 1000, 500, 500);
        GUI.Box(dropArea, "Drag and Drop");


        var evt = Event.current;      //현재 이벤트 얻어오기.
        switch (evt.type)
        {
            case EventType.DragUpdated:     // 드래그 하는 동안 업데이트 되는 이벤트 타입.
            case EventType.DragPerform:    // 드래그 후 마우스 버튼 업인 상태일때.
                {
                    if (!dropArea.Contains(evt.mousePosition))    // 마우스 포지션이 박스안에 영역인지 확인.
                        break;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)   // // 드래그 후 마우스 버튼 업인 상태일때.
                    {
                        DragAndDrop.AcceptDrag();   // 드래그앤 드랍을 허용함.

                        foreach (var draggedObj in DragAndDrop.paths)    // objectReferences: 드래그한 오브젝트들의 레퍼런스
                        {
                            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(draggedObj);
                            if (sp != null)
                                outputSprite.Add(sp);

                            ParticleSystem ps = AssetDatabase.LoadAssetAtPath<ParticleSystem>(draggedObj);
                            if (ps != null)
                                outputParticle.Add(ps);

                            AudioClip ac = AssetDatabase.LoadAssetAtPath<AudioClip>(draggedObj);
                            if (ac != null)
                                outputAudioClip.Add(ac);
                        }
                    }
                }
                Event.current.Use();  // 이벤트 사용한 후, 이벤트의 타입을 변경해준다. (EventType.Used)
                break;
        }


        if (isClick == true)
        {
            if (outputSprite.Count > 0)
            {
                if (ShowLabel == true)
                {
                    if (m_callBack_Sprite_WithString != null)
                        m_callBack_Sprite_WithString(outputSprite, myTextFieldReturn);

                    m_callBack_Sprite_WithString = null;
                }
                else
                {
                    if (m_callBack_Sprite != null)
                        m_callBack_Sprite(outputSprite);

                    m_callBack_Sprite = null;
                }

            }

            if (outputParticle.Count > 0)
            {
                if (m_callBack_ParticleSystem != null)
                    m_callBack_ParticleSystem(outputParticle);

                m_callBack_ParticleSystem = null;
            }

            if (outputAudioClip.Count > 0)
            {
                if (m_callBack_AudioClip != null)
                    m_callBack_AudioClip(outputAudioClip);

                m_callBack_AudioClip = null;
            }
        }
        else
        {
            string showname = "";

            for (int i = 0; i < outputSprite.Count; ++i)
            {
                string assetname = outputSprite[i].name;
                if (myTextFieldReturn.Contains("{0}"))
                {
                    assetname = string.Format(myTextFieldReturn, assetname);
                }
                else
                {
                    assetname = string.Format("{0}{1}", myTextFieldReturn, assetname);
                }

                showname += string.Format("{0}\n", assetname);
            }

            for (int i = 0; i < outputParticle.Count; ++i)
            {
                string assetname = outputParticle[i].name;
                if (myTextFieldReturn.Contains("{0}"))
                {
                    assetname = string.Format(myTextFieldReturn, assetname);
                }
                else
                {
                    assetname = string.Format("{0}{1}", myTextFieldReturn, assetname);
                }

                showname += string.Format("{0}\n", assetname);
            }

            for (int i = 0; i < outputAudioClip.Count; ++i)
            {
                string assetname = outputAudioClip[i].name;
                if (myTextFieldReturn.Contains("{0}"))
                {
                    assetname = string.Format(myTextFieldReturn, assetname);
                }
                else
                {
                    assetname = string.Format("{0}{1}", myTextFieldReturn, assetname);
                }

                showname += string.Format("{0}\n", assetname);
            }

            GUI.Label(dropArea, showname);
        }
    }
}

#endif