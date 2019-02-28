using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class MixedNumberVisualWidget : MonoBehaviour {
    public const int fractionMaxMult = 64;
    public const int fractionMaxRow = 25;

    [Header("Line")]
    public UIGridRenderer gridRenderer;

    [Header("Whole")]
    public GameObject wholeGO;
    public TMPro.TMP_Text wholeAmountText;

    [Header("Fraction")]
    public Image fractionFill;
    public bool fractionFillInverted;
    public RectTransform fractionRoot;

    [Header("Interface")]
    public MixedNumberWidget mixedNumberWidget;
    public TMPro.TMP_Text multText;
    public Button multButtonLeft;
    public Button multButtonRight;

    private int mMultCount;
    
    private int mNumerator;
    private int mDenominator;

    void OnDisable() {
        if(mixedNumberWidget)
            mixedNumberWidget.numberUpdateCallback -= OnNumberUpdate;
    }

    void OnEnable() {
        OnNumberUpdate();

        mixedNumberWidget.numberUpdateCallback += OnNumberUpdate;
    }

    void Awake() {
        multButtonLeft.onClick.AddListener(OnMultPrevClick);
        multButtonRight.onClick.AddListener(OnMultNextClick);
    }

    void OnNumberUpdate() {
        mMultCount = mixedNumberWidget.number.GetGreatestCommonFactor();
        if(mMultCount <= 0)
            mMultCount = 1;

        //whole
        var wholeFromFraction = mixedNumberWidget.number.GetWholeFromFraction();
        var wholeCount = mixedNumberWidget.number.whole + wholeFromFraction;
        if(wholeCount > 0) {
            wholeGO.SetActive(true);
            wholeAmountText.text = wholeCount.ToString();
        }
        else {
            wholeGO.SetActive(false);
        }

        //mult
        multText.text = "x" + mMultCount.ToString();
                
        //fraction
        mNumerator = mixedNumberWidget.number.numerator / mMultCount;
        mDenominator = mixedNumberWidget.number.denominator / mMultCount;

        float fNumerator;
        float fDenominator = mDenominator;

        if(mNumerator > mDenominator)
            fNumerator = mNumerator - wholeFromFraction * mDenominator;
        else
            fNumerator = mNumerator;

        var fVal = fDenominator > 0f && fNumerator != fDenominator ? fNumerator / fDenominator : 0f;

        fractionFill.fillAmount = fractionFillInverted ? 1.0f - fVal : fVal;
        
        UpdateFractionLines();

        multButtonLeft.interactable = mMultCount > 1 && fDenominator > 0;
        multButtonRight.interactable = mMultCount < fractionMaxMult && fDenominator > 0;
    }

    void OnMultPrevClick() {
        if(mMultCount <= 0) //fail-safe
            return;

        mMultCount--;

        UpdateMixedNumber();
    }

    void OnMultNextClick() {
        if(mMultCount <= 0) //fail-safe
            return;

        mMultCount++;

        UpdateMixedNumber();
    }

    private void UpdateMixedNumber() {
        var num = mixedNumberWidget.number;

        num.numerator = mNumerator * mMultCount;
        num.denominator = mDenominator * mMultCount;

        mixedNumberWidget.number = num;
    }
    
    private void UpdateFractionLines() {
        int numerator = mNumerator;
        int denominator = mDenominator;

        if(numerator > denominator)
            numerator -= Mathf.FloorToInt((float)numerator / denominator) * denominator;
        
        if(numerator > 0 && denominator > 0 && numerator != denominator) {
            gridRenderer.GridRows = denominator * mMultCount;
        }
        else {
            gridRenderer.GridRows = 1;
        }
    }
}
