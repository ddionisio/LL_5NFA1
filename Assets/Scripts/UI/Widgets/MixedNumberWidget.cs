using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MixedNumberWidget : MonoBehaviour {
    [Header("Display")]
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
        wholeText.text = mNumber.whole != 0 ? mNumber.whole.ToString() : "";
        numeratorText.text = mNumber.numerator.ToString();
        denominatorText.text = mNumber.denominator.ToString();
    }
}
