using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CButton : Button
{
    [Header("-------Btn-------")]
    public Sprite _btn_Sprite;

    public Sprite _btn_Dimmed_Sprite;


    [Header("-------Text-------")]
    public List<TMP_Text> _text;

    public Color _text_Color = Color.white;

    public Color _text_Dimmed_Color = Color.gray;


    protected override void Awake()
    {
        base.Awake();

        ChangeDimmedResource(base.interactable);
    }

    public void SetInteractable(bool interactable)
    {
        base.interactable = interactable;

        ChangeDimmedResource(interactable);
    }

    public void ChangeDimmedResource(bool isDimmed)
    {
        ChangeImageDimmedResource(!isDimmed);
        ChangeTextDimmedResource(!isDimmed);
    }

    private void ChangeImageDimmedResource(bool isDimmed)
    {
        if (image != null)
        {
            if (isDimmed == false)
            {
                if (_btn_Sprite != null)
                    image.sprite = _btn_Sprite;
            }
            else
            {
                if (_btn_Dimmed_Sprite != null)
                    image.sprite = _btn_Dimmed_Sprite;
            }
        }
    }

    private void ChangeTextDimmedResource(bool isDimmed)
    {
        if (_text != null)
        {
            for (int i = 0; i < _text.Count; ++i)
            {
                if (_text[i] != null)
                    _text[i].color = isDimmed == false ? _text_Color : _text_Dimmed_Color;
            }
        }
    }
}
