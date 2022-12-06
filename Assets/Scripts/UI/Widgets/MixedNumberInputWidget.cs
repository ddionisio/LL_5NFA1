using System.Collections;
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

    [Header("Lock")]
    public GameObject lockGO;

    [Header("Whole")]
    public GameObject negativeSignGO;
    public GameObject wholeRootGO;
    public GameObject wholeActiveGO;
    public Text wholeText;

    [Header("Numerator")]
    public GameObject numeratorActiveGO;
    public Text numeratorText;

    [Header("Denominator")]
    public GameObject denominatorActiveGO;
    public Text denominatorText;

    [Header("Submit")]
    public Selectable submitSelectable;

    [Header("Input")]
    public M8.InputAction inputCycleHorizontal;
    public M8.InputAction inputCycleVertical;
    public M8.InputAction inputSubmit;

    [Header("Signal Invokes")]
    public SignalFloat signalChangeValue;

    [Header("Signal Listens")]
    public SignalFloat signalValueChanged; //from numpad
    public M8.Signal signalSubmit;
    public M8.Signal signalCyclePrev;
    public M8.Signal signalCycleNext;

    public event System.Action submitCallback;

    public bool isLocked {
        get { return mIsLocked; }
        set {
            if(mIsLocked != value) {
                mIsLocked = value;

                ApplyLocked();

                if(mIsLocked) {
                    SetSelect(SelectType.None);
                    CloseNumpad();
                }
            }
        }
    }

    public MixedNumber number { get; private set; }
    public bool numberIsNegative { get; private set; }

    private SelectType mCurSelect;
    private bool mIsWholeEnabled;

    private M8.GenericParams mNumpadParms = new M8.GenericParams();

    private bool mIsLocked;

    public void CloseNumpad() {
        if(M8.ModalManager.main && M8.ModalManager.main.IsInStack(modalNumpad))
            M8.ModalManager.main.CloseUpTo(modalNumpad, true);
    }

    public void ClickWhole() {
        if(!mIsWholeEnabled)
            return;

        if(mCurSelect != SelectType.Whole) {
            SetSelect(SelectType.Whole);
            OnSignalValueChanged(number.whole);
            ActivateInputValue();
        }
        else { //close
            SetSelect(SelectType.None);
            CloseNumpad();
        }
    }

    public void ClickNumerator() {
        if(mCurSelect != SelectType.Numerator) {
            SetSelect(SelectType.Numerator);
            OnSignalValueChanged(number.numerator);
            ActivateInputValue();
        }
        else { //close
            SetSelect(SelectType.None);
            CloseNumpad();
        }
    }

    public void ClickDenominator() {
        if(mCurSelect != SelectType.Denominator) {
            SetSelect(SelectType.Denominator);
            OnSignalValueChanged(number.denominator);
            ActivateInputValue();
        }
        else { //close
            SetSelect(SelectType.None);
            CloseNumpad();
        }
    }

    public void ClickSubmit() {
        //CloseNumpad();

        if(submitCallback != null)
            submitCallback();
    }

    public void Init(bool isWholeEnabled, bool isNegative) {
        number = new MixedNumber();

        mCurSelect = SelectType.None;

        mIsWholeEnabled = isWholeEnabled;
        numberIsNegative = isNegative;

        mIsLocked = false;
        ApplyLocked();

        if(mIsWholeEnabled) {
            if(wholeRootGO) wholeRootGO.SetActive(mIsWholeEnabled);
            if(negativeSignGO) negativeSignGO.SetActive(false);
        }
        else {
            if(wholeRootGO) wholeRootGO.SetActive(false);
            if(negativeSignGO) negativeSignGO.SetActive(numberIsNegative);
        }
        
        if(wholeActiveGO) wholeActiveGO.SetActive(false);
        if(wholeText) wholeText.text = mIsWholeEnabled && numberIsNegative ? "-" : "";

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

        if(signalSubmit)
            signalSubmit.callback -= OnSignalSubmit;
        if(signalCyclePrev)
            signalCyclePrev.callback -= OnSignalCyclePrev;
        if(signalCycleNext)
            signalCycleNext.callback -= OnSignalCycleNext;
    }

    void Awake() {
        if(signalValueChanged)
            signalValueChanged.callback += OnSignalValueChanged;

        if(signalSubmit)
            signalSubmit.callback += OnSignalSubmit;
        if(signalCyclePrev)
            signalCyclePrev.callback += OnSignalCyclePrev;
        if(signalCycleNext)
            signalCycleNext.callback += OnSignalCycleNext;
    }

    void Update() {
        //deselect if numpad is closed
        if(mCurSelect != SelectType.None) {
            if(!M8.ModalManager.main.IsInStack(modalNumpad))
                SetSelect(SelectType.None);
        }

        if(mIsLocked)
            return;

        //input
        if(inputCycleHorizontal.IsPressed()) {
            var axis = inputCycleHorizontal.GetAxis();
            if(axis > 0f)
                OnSignalCycleNext();
            else if(axis < 0f)
                OnSignalCyclePrev();
        }
        else if(inputCycleVertical.IsPressed()) {
            var axis = inputCycleVertical.GetAxis();
            if(axis > 0f)
                OnSignalCycleNext();
            else if(axis < 0f)
                OnSignalCyclePrev();
        }

        if(inputSubmit.IsPressed())
            OnSignalSubmit();
    }

    void OnSignalValueChanged(float val) {
        //NOTE: only input positives

        var _num = number;
        var iVal = Mathf.FloorToInt(Mathf.Abs(val));

        switch(mCurSelect) {
            case SelectType.Whole:
                _num.whole = iVal;
                if(numberIsNegative) {
                    if(iVal > 0)
                        wholeText.text = (-iVal).ToString();
                    else
                        wholeText.text = "-";
                }
                else
                    wholeText.text = iVal.ToString();
                break;
            case SelectType.Numerator:
                _num.numerator = iVal;
                numeratorText.text = iVal.ToString();
                break;
            case SelectType.Denominator:
                _num.denominator = iVal;
                denominatorText.text = iVal.ToString();
                break;
        }

        number = _num;
    }

    void OnSignalSubmit() {
        if(mIsLocked)
            return;

        if(mCurSelect == SelectType.None) {
            if(mIsWholeEnabled)
                ClickWhole();
            else
                ClickNumerator();
        }
        else
            ClickSubmit();
    }

    void OnSignalCycleNext() {
        if(mIsLocked)
            return;

        switch(mCurSelect) {
            case SelectType.Whole:
                ClickNumerator();
                break;
            case SelectType.Numerator:
                ClickDenominator();
                break;
            case SelectType.Denominator:
            case SelectType.None:
                if(mIsWholeEnabled)
                    ClickWhole();
                else
                    ClickNumerator();
                break;
        }
    }

    void OnSignalCyclePrev() {
        if(mIsLocked)
            return;

        switch(mCurSelect) {
            case SelectType.Whole:
                ClickDenominator();
                break;
            case SelectType.Denominator:
                ClickNumerator();
                break;
            case SelectType.Numerator:
            case SelectType.None:
                if(mIsWholeEnabled)
                    ClickWhole();
                else
                    ClickDenominator();
                break;
        }
    }

    private void ApplyLocked() {
        if(lockGO) lockGO.SetActive(mIsLocked);
        if(submitSelectable) submitSelectable.interactable = !mIsLocked;
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
