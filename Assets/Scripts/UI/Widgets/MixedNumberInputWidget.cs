﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MixedNumberInputWidget : MonoBehaviour {
    public enum SelectType {
        None,
        Whole,
        Numerator,
        Denominator
    }

    [Header("Data")]
    public string modalNumpad = "numpad";

    [Header("Whole")]
    public GameObject wholeRootGO;
    public GameObject wholeActiveGO;
    public TMPro.TMP_Text wholeText;

    [Header("Numerator")]
    public GameObject numeratorActiveGO;
    public TMPro.TMP_Text numeratorText;

    [Header("Denominator")]
    public GameObject denominatorActiveGO;
    public TMPro.TMP_Text denominatorText;

    [Header("Input")]
    public M8.InputAction inputHorizontal;
    public M8.InputAction inputVertical;

    [Header("Signal Invokes")]
    public SignalFloat signalChangeValue;

    [Header("Signal Listens")]
    public SignalFloat signalValueChanged; //from numpad
        
    public event System.Action submitCallback;

    public MixedNumber number { get; private set; }

    private SelectType mCurSelect;
    private bool mIsWholeEnabled;

    private M8.GenericParams mNumpadParms = new M8.GenericParams();

    public void CloseNumpad() {
        if(M8.ModalManager.main && M8.ModalManager.main.IsInStack(modalNumpad))
            M8.ModalManager.main.CloseUpTo(modalNumpad, true);
    }

    public void ClickWhole() {
        if(!mIsWholeEnabled)
            return;

        if(mCurSelect != SelectType.Whole) {
            SetSelect(SelectType.Whole);
            ActivateInputValue();
        }
    }

    public void ClickNumerator() {
        if(mCurSelect != SelectType.Numerator) {
            SetSelect(SelectType.Numerator);
            ActivateInputValue();
        }
    }

    public void ClickDenominator() {
        if(mCurSelect != SelectType.Denominator) {
            SetSelect(SelectType.Denominator);
            ActivateInputValue();
        }
    }

    public void ClickSubmit() {
        CloseNumpad();

        if(submitCallback != null)
            submitCallback();
    }

    public void Init(bool isWholeEnabled) {
        number = new MixedNumber();

        mCurSelect = SelectType.None;

        mIsWholeEnabled = isWholeEnabled;

        if(wholeRootGO) wholeRootGO.SetActive(mIsWholeEnabled);

        if(wholeActiveGO) wholeActiveGO.SetActive(false);
        if(wholeText) wholeText.text = "";

        if(numeratorActiveGO) numeratorActiveGO.SetActive(false);
        if(numeratorText) numeratorText.text = "";

        if(denominatorActiveGO) denominatorActiveGO.SetActive(false);
        if(denominatorText) denominatorText.text = "";
    }

    void OnDisable() {
        CloseNumpad();
    }

    void OnDestroy() {
        if(signalValueChanged)
            signalValueChanged.callback -= OnSignalValueChanged;
    }

    void Awake() {
        if(signalValueChanged)
            signalValueChanged.callback += OnSignalValueChanged;
    }

    void Update() {
        //deselect if numpad is closed
        if(mCurSelect != SelectType.None) {
            if(!M8.ModalManager.main.IsInStack(modalNumpad))
                SetSelect(SelectType.None);
        }

        //input
        switch(mCurSelect) {
            case SelectType.Whole:
                if(inputHorizontal.IsPressed())
                    ClickNumerator();
                break;
            case SelectType.Numerator:
                if(inputHorizontal.IsPressed()) {
                    if(mIsWholeEnabled)
                        ClickWhole();
                    else
                        ClickDenominator();
                }
                else if(inputVertical)
                    ClickDenominator();
                break;
            case SelectType.Denominator:
                if(inputHorizontal.IsPressed()) {
                    if(mIsWholeEnabled)
                        ClickWhole();
                    else
                        ClickNumerator();
                }
                else if(inputVertical)
                    ClickNumerator();
                break;
        }
    }

    void OnSignalValueChanged(float val) {
        var _num = number;
        var iVal = Mathf.FloorToInt(val);

        switch(mCurSelect) {
            case SelectType.Whole:
                _num.whole = iVal;
                break;
            case SelectType.Numerator:
                _num.numerator = iVal;
                break;
            case SelectType.Denominator:
                _num.denominator = iVal;
                break;
        }

        number = _num;
    }

    private void SetSelect(SelectType select) {
        var prevSelect = mCurSelect;
        mCurSelect = select;

        switch(prevSelect) {
            case SelectType.Whole:
                if(wholeActiveGO) wholeActiveGO.SetActive(false);
                break;
            case SelectType.Numerator:
                if(numeratorActiveGO) numeratorActiveGO.SetActive(false);
                break;
            case SelectType.Denominator:
                if(denominatorActiveGO) denominatorActiveGO.SetActive(false);
                break;
        }

        switch(mCurSelect) {
            case SelectType.Whole:
                if(wholeActiveGO) wholeActiveGO.SetActive(true);
                break;
            case SelectType.Numerator:
                if(numeratorActiveGO) numeratorActiveGO.SetActive(true);
                break;
            case SelectType.Denominator:
                if(denominatorActiveGO) denominatorActiveGO.SetActive(true);
                break;
        }
    }

    private void ActivateInputValue() {
        if(M8.ModalManager.main.IsInStack(modalNumpad)) {
            switch(mCurSelect) {
                case SelectType.Whole:
                    signalChangeValue.Invoke(number.whole);
                    break;
                case SelectType.Numerator:
                    signalChangeValue.Invoke(number.numerator);
                    break;
                case SelectType.Denominator:
                    signalChangeValue.Invoke(number.denominator);
                    break;
            }
        }
        else {
            switch(mCurSelect) {
                case SelectType.Whole:
                    mNumpadParms[ModalCalculator.parmInitValue] = number.whole;
                    break;
                case SelectType.Numerator:
                    mNumpadParms[ModalCalculator.parmInitValue] = number.numerator;
                    break;
                case SelectType.Denominator:
                    mNumpadParms[ModalCalculator.parmInitValue] = number.denominator;
                    break;
            }

            M8.ModalManager.main.Open(modalNumpad, mNumpadParms);
        }
    }
}
