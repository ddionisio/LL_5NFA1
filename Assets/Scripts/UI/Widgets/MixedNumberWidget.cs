using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MixedNumberWidget : MonoBehaviour {
    [Header("Display")]
    public GameObject wholeRootGO;
    public TMPro.TMP_Text wholeText;

    public TMPro.TMP_Text numeratorText;
    public TMPro.TMP_Text denominatorText;

    public MixedNumber number {
        get { return mNumber; }
        set {
            mNumber = value;

            RefreshDisplay();

            if(numberUpdateCallback != null)
                numberUpdateCallback();
        }
    }

    public event System.Action numberUpdateCallback;

    private MixedNumber mNumber;

    public void RefreshDisplay() {
        if(Mathf.Abs(mNumber.fValue) >= 1.0f) {
            if(mNumber.whole > 0) {
                var wholeVal = mNumber.isNegative ? -mNumber.whole : mNumber.whole;
                wholeText.text = wholeVal.ToString();
            }
            else
                wholeText.text = mNumber.isNegative ? "-" : "";

            if(wholeRootGO) wholeRootGO.SetActive(true);
        }
        else {
            if(mNumber.isNegative) {
                if(wholeRootGO) wholeRootGO.SetActive(true);
                wholeText.text = "-";
            }
            else {
                if(wholeRootGO) wholeRootGO.SetActive(false);
            }
        }

        numeratorText.text = mNumber.numerator.ToString();
        denominatorText.text = mNumber.denominator.ToString();
    }
}
