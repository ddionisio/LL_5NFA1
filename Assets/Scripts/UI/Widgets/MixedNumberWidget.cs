using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MixedNumberWidget : MonoBehaviour {
    [Header("Display")]
    public GameObject wholeRootGO; //if this is null, whole number is disabled
    public TMPro.TMP_Text wholeText;

    public GameObject fractionRootGO;
    public bool fractionHideIfZero = true; //only if fractionRootGO is valid
    public TMPro.TMP_Text numeratorText;
    public TMPro.TMP_Text denominatorText;

    [Header("Fraction FX")]
    public GameObject fractionFXGO; //root for fraction fx
    public TMPro.TMP_Text fractionFXNumeratorText;
    public TMPro.TMP_Text fractionFXDenominatorText;

    [Header("Number Pulse")]
    public float pulseDelay = 0.3f;
    public float pulseScale = 1f; //set to 1 to have no pulse

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeWholeToFraction;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeFractionToWhole;
    
    public bool isWholeEnabled { get { return wholeRootGO; } }

    public MixedNumber number {
        get { return mNumber; }
        set {
            if(mNumber.isNegative != value.isNegative || mNumber.whole != value.whole || mNumber.numerator != value.numerator || mNumber.denominator != value.denominator) {
                mNumberPrev = mNumber;
                mNumber = value;

                RefreshDisplay();

                if(CanPulse()) {
                    if(mNumber.whole != mNumberPrev.whole && wholeRootGO)
                        StartCoroutine(DoPulse(wholeRootGO.transform));

                    if((mNumber.numerator != mNumberPrev.numerator || mNumber.denominator != mNumberPrev.denominator) && fractionRootGO)
                        StartCoroutine(DoPulse(fractionRootGO.transform));
                }

                if(numberUpdateCallback != null)
                    numberUpdateCallback();
            }
        }
    }

    public event System.Action numberUpdateCallback;

    private MixedNumber mNumber;
    private MixedNumber mNumberPrev;

    private Coroutine mSwapRout;

    /// <summary>
    /// Animate and convert whole of number to fraction
    /// </summary>
    public void WholeToFraction() {
        //no animation if there's no whole to take
        if(mNumber.whole == 0)
            return;

        StopSwapRout();

        if(animator && !string.IsNullOrEmpty(takeWholeToFraction)) {
            mNumberPrev = mNumber;
            mNumber.WholeToFraction();
            StartCoroutine(DoWholeToFractionAnimation());
        }
        else {
            var num = mNumber;
            num.WholeToFraction();
            number = num;
        }
    }

    /// <summary>
    /// Animate and convert fraction of number to whole
    /// </summary>
    public void FractionToWhole() {
        //no animation if there's no whole to take
        if(mNumber.denominator <= 0 || mNumber.numerator < mNumber.denominator)
            return;

        StopSwapRout();

        if(animator && !string.IsNullOrEmpty(takeFractionToWhole)) {
            mNumberPrev = mNumber;
            mNumber.FractionToWhole();
            StartCoroutine(DoFractionToWholeAnimation());
        }
        else {
            var num = mNumber;
            num.FractionToWhole();
            number = num;
        }
    }

    public void RefreshDisplay() {
        ApplyWholeDisplay();
        ApplyFractionDisplay();
    }

    void ApplyWholeDisplay() {
        var num = this.mNumber;

        if(wholeRootGO) {
            if(Mathf.Abs(num.fValue) >= 1.0f) {
                if(num.whole > 0) {
                    var wholeVal = num.isNegative ? -num.whole : num.whole;
                    wholeText.text = wholeVal.ToString();
                }
                else
                    wholeText.text = num.isNegative ? "-" : "";

                wholeRootGO.SetActive(true);
            }
            else {
                if(num.isNegative) {
                    if(wholeRootGO) wholeRootGO.SetActive(true);
                    wholeText.text = "-";
                }
                else {
                    wholeRootGO.SetActive(false);
                }
            }
        }
    }

    void ApplyFractionDisplay() {
        var num = this.mNumber;

        if(!wholeRootGO)
            num.WholeToFraction();

        if(num.denominator > 0) {
            numeratorText.text = num.numerator.ToString();
            denominatorText.text = num.denominator.ToString();
        }
        else {
            numeratorText.text = "-";
            denominatorText.text = "-";
        }

        if(fractionRootGO) {
            if(num.numerator > 0 && num.denominator > 0)
                fractionRootGO.SetActive(true);
            else if(fractionHideIfZero && num.denominator > 0)
                fractionRootGO.SetActive(false);
        }
    }

    void OnDisable() {
        StopSwapRout();
    }

    void OnEnable() {
        if(fractionFXGO) fractionFXGO.SetActive(false);
    }

    void Awake() {
        if(fractionFXGO) fractionFXGO.SetActive(false);
    }

    IEnumerator DoPulse(Transform root) {
        var funcOut = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutSine);
        var funcIn = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.InSine);

        var curTime = 0f;
        var delay = pulseDelay * 0.5f;

        while(curTime < delay) {
            yield return null;

            curTime += Time.deltaTime;

            var t = funcOut(curTime, delay, 0f, 0f);

            var s = Mathf.Lerp(1f, pulseScale, t);

            root.localScale = new Vector3(s, s, 1f);
        }

        curTime = 0f;

        while(curTime < delay) {
            yield return null;

            curTime += Time.deltaTime;

            var t = funcOut(curTime, delay, 0f, 0f);

            var s = Mathf.Lerp(pulseScale, 1f, t);

            root.localScale = new Vector3(s, s, 1f);
        }
    }

    IEnumerator DoWholeToFractionAnimation() {
        animator.Stop();

        bool canPulse = CanPulse();

        ApplyWholeDisplay();

        if(canPulse && wholeRootGO)
            StartCoroutine(DoPulse(wholeRootGO.transform));

        //apply aesthetic numbers
        if(mNumberPrev.denominator > 0)
            ApplyFractionFX(mNumberPrev.whole * mNumberPrev.denominator, mNumberPrev.denominator);
        else
            ApplyFractionFX(mNumberPrev.whole, 1);

        if(fractionFXGO) fractionFXGO.SetActive(true);

        yield return animator.PlayWait(takeWholeToFraction);

        if(fractionFXGO) fractionFXGO.SetActive(false);

        ApplyFractionDisplay();

        if(canPulse && fractionRootGO)
            StartCoroutine(DoPulse(fractionRootGO.transform));

        mSwapRout = null;
    }

    IEnumerator DoFractionToWholeAnimation() {
        animator.Stop();

        bool canPulse = CanPulse();

        ApplyFractionDisplay();
        
        if(canPulse && fractionRootGO)
            StartCoroutine(DoPulse(fractionRootGO.transform));

        //apply aesthetic numbers
        if(mNumberPrev.denominator > 0)
            ApplyFractionFX(mNumberPrev.GetWholeFromFraction() * mNumberPrev.denominator, mNumberPrev.denominator);
        else
            ApplyFractionFX(mNumberPrev.GetWholeFromFraction(), 1);

        if(fractionFXGO) fractionFXGO.SetActive(true);

        yield return animator.PlayWait(takeFractionToWhole);

        if(fractionFXGO) fractionFXGO.SetActive(false);

        ApplyWholeDisplay();

        if(canPulse && wholeRootGO)
            StartCoroutine(DoPulse(wholeRootGO.transform));

        mSwapRout = null;
    }

    private void StopSwapRout() {
        if(mSwapRout != null) {
            //ensure the current display is correct
            ApplyFractionDisplay();
            ApplyWholeDisplay();

            if(fractionFXGO) fractionFXGO.SetActive(false);

            StopCoroutine(mSwapRout);
            mSwapRout = null;
        }
    }

    private void ApplyFractionFX(int numerator, int denominator) {
        if(fractionFXNumeratorText) fractionFXNumeratorText.text = numerator.ToString();
        if(fractionFXDenominatorText) fractionFXDenominatorText.text = denominator.ToString();
    }

    private bool CanPulse() {
        return pulseScale != 1f && pulseDelay > 0f;
    }
}
