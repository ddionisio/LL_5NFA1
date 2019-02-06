using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoLProgressProxy : MonoBehaviour
{
    public int increment = 1;

    public void Invoke() {
        int curProgress = LoLManager.instance.curProgress;
        LoLManager.instance.ApplyProgress(curProgress + increment);
    }
}
