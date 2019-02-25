using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISetDirtyOnEnable : MonoBehaviour {
    public Graphic graphic;
    public float delay = 0.1f;

    void OnEnable() {
        if(delay > 0f)
            StartCoroutine(DoApplyDirtyDelay());
        else
            ApplyDirty();
    }

    IEnumerator DoApplyDirtyDelay() {
        yield return new WaitForSeconds(delay);
        ApplyDirty();
    }

    void ApplyDirty() {
        graphic.SetAllDirty();
    }
}
