using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ColorGroupButtonWidget : Selectable {
    public M8.UI.Graphics.ColorGroup colorGroup;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.white;
    public Color pressedColor = Color.white;
    public Color disabledColor = Color.white;

    private bool mIsPointerInside;
    private bool mIsPointerDown;
    private bool mIsDisabled;

    void Update() {
        bool isDisabled = !interactable;
        if(mIsDisabled != isDisabled) {
            mIsDisabled = isDisabled;
            ApplyColor();
        }
    }

    protected override void OnEnable() {
        base.OnEnable();

        mIsDisabled = !interactable;

        ApplyColor();
    }

    protected override void OnDisable() {
        mIsPointerInside = false;
        mIsPointerDown = false;
        mIsDisabled = false;

        base.OnDisable();
    }
        
    public override void OnPointerDown(PointerEventData eventData) {
        mIsPointerDown = true;
        ApplyColor();
    }

    public override void OnPointerUp(PointerEventData eventData) {
        mIsPointerDown = false;
        ApplyColor();
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        mIsPointerInside = true;
        ApplyColor();
    }

    public override void OnPointerExit(PointerEventData eventData) {
        mIsPointerInside = false;
        ApplyColor();
    }

    void ApplyColor() {
        if(!colorGroup) return;

        if(mIsDisabled)
            colorGroup.ApplyColor(disabledColor);
        else if(mIsPointerDown)
            colorGroup.ApplyColor(pressedColor);
        else if(mIsPointerInside)
            colorGroup.ApplyColor(highlightColor);
        else
            colorGroup.ApplyColor(normalColor);
    }
}
