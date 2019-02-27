using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractionVisualToggle : MonoBehaviour {
    public const string stateVar = "fractionVisual";
    public const bool isFixedVisual = true;

    [Header("Display")]
    public GameObject visibleActiveGO;
    public GameObject visibleInactiveGO;

    [Header("Signals")]
    public M8.Signal signalVisibleUpdate;

    public static bool isVisible {
        get { return isFixedVisual || M8.SceneState.instance.local.GetValue(stateVar) != 0; }
        set { M8.SceneState.instance.local.SetValue(stateVar, value ? 1 : 0, false); }
    }

    public void Toggle() {
        isVisible = !isVisible;

        VisibleUpdate();

        if(signalVisibleUpdate)
            signalVisibleUpdate.Invoke();
    }

    void OnEnable() {
        VisibleUpdate();
    }

    private void VisibleUpdate() {
        var _isVisible = isVisible;

        if(visibleActiveGO) visibleActiveGO.SetActive(_isVisible);
        if(visibleInactiveGO) visibleInactiveGO.SetActive(!_isVisible);
    }
}
