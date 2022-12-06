using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterItemWidget : MonoBehaviour {
    public GameObject fillRootGO;
    
    public void Fill() {
        if(fillRootGO)
            fillRootGO.SetActive(true);
    }

    public void Unfill() {
        if(fillRootGO)
            fillRootGO.SetActive(false);
    }
}
