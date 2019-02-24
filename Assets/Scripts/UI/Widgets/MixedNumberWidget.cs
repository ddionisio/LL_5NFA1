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
    public TMPro.TMP_Text numeratorText;
    public TMPro.TMP_Text denominatorText;

    public bool isWholeEnabled { get { return wholeRootGO; } }

    public MixedNumber number {
        get { return mNumber; }
        set {
            if(mNumber.isNegative != value.isNegative || mNumber.whole != value.whole || mNumber.numerator != value.numerator || mNumber.denominator != value.denominator) {
                mNumber = value;

                RefreshDisplay();

                if(numberUpdateCallback != null)
                    numberUpdateCallback();
            }
        }
    }

    public event System.Action numberUpdateCallback;

    private MixedNumber mNumber;

    public void RefreshDisplay() {
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
        else {
            num.WholeToFraction();
        }

        if(fractionRootGO) {
            if(num.numerator > 0 && num.denominator > 0) {
                fractionRootGO.SetActive(true);
                numeratorText.text = num.numerator.ToString();
                denominatorText.text = num.denominator.ToString();
            }
            else
                fractionRootGO.SetActive(false);
        }
        else {
            if(num.denominator > 0) {
                numeratorText.text = num.numerator.ToString();
                denominatorText.text = num.denominator.ToString();
            }
            else {
                numeratorText.text = "-";
                denominatorText.text = "-";
            }
        }
    }
}
