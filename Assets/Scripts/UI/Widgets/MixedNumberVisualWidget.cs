using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MixedNumberVisualWidget : MonoBehaviour {
    public const int linePoolStartCapacity = 32;
    public const int linePoolMaxCapacity = 256;
    public const int fractionMaxMult = 32;

    [Header("Line")]
    public GameObject lineTemplate;
    public string linePoolGroup;
        
    [Header("Whole")]
    public GameObject wholeGO;
    public TMPro.TMP_Text wholeAmountText;

    [Header("Fraction")]
    public Image fractionFill;
    public RectTransform fractionRoot;

    [Header("Interface")]
    public MixedNumberWidget mixedNumberWidget;
    public TMPro.TMP_Text multText;
    public Button multButtonLeft;
    public Button multButtonRight;

    private int mMultCount;

    private M8.PoolController mPool;

    private List<RectTransform> mLines = new List<RectTransform>();

    private int mNumerator;
    private int mDenominator;

    void OnDisable() {
        if(mixedNumberWidget)
            mixedNumberWidget.numberUpdateCallback -= OnNumberUpdate;

        ClearLines();
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
        var wholeCount = mixedNumberWidget.number.whole + mixedNumberWidget.number.GetWholeFromFraction();
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

        var fVal = mDenominator > 0 && mNumerator < mDenominator ? (float)mNumerator / mDenominator : 0f;

        fractionFill.fillAmount = fVal;
        
        UpdateFractionLines();

        multButtonLeft.interactable = mMultCount > 1 && mDenominator > 0;
        multButtonRight.interactable = mMultCount < fractionMaxMult && mDenominator > 0;
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
        if(!mPool) {
            mPool = M8.PoolController.CreatePool(linePoolGroup);
            mPool.AddType(lineTemplate, linePoolStartCapacity, linePoolMaxCapacity);
        }

        int count = mNumerator > 0 && mNumerator < mDenominator ? mDenominator * mMultCount - 1 : 0;

        if(mLines.Count != count) {
            ClearLines();

            if(count > 0) {
                var scale = 1.0f / (mDenominator * mMultCount);

                ClearLines();

                for(int i = 0; i < count; i++) {
                    var newLine = mPool.Spawn<RectTransform>(lineTemplate.name, i.ToString(), fractionRoot, null);

                    newLine.anchorMin = Vector2.zero;
                    newLine.anchorMax = new Vector2(1f, 0f);
                    newLine.pivot = new Vector2(0, 0.5f);
                    newLine.anchoredPosition = new Vector2(0f, fractionRoot.rect.height * scale * (i + 1));
                    newLine.sizeDelta = new Vector2(0f, newLine.sizeDelta.y);

                    mLines.Add(newLine);
                }
            }
        }
    }

    private void ClearLines() {
        if(mPool) {
            for(int i = 0; i < mLines.Count; i++)
                mPool.Release(mLines[i]);
        }

        mLines.Clear();
    }
}
