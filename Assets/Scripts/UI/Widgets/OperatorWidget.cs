using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OperatorWidget : MonoBehaviour {
    [Header("Display")]
    public Text label;

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
