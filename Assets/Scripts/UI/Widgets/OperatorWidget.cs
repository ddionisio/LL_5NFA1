using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class OperatorWidget : MonoBehaviour {
    [Header("Display")]
    public TMP_Text label;

    public void SetOperator(OperatorType operatorType) {
        switch(operatorType) {
            case OperatorType.Add:
                label.text = "+";
                break;
            case OperatorType.Subtract:
                label.text = "-";
                break;
        }
    }
}
