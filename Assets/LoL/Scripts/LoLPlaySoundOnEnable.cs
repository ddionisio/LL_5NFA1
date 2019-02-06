using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoLPlaySoundOnEnable : MonoBehaviour {
    public string soundPath;
    public bool isBackground;
    public bool isLoop;

    void OnEnable() {
        if(LoLManager.isInstantiated)
            LoLManager.instance.PlaySound(soundPath, isBackground, isLoop);
    }
}
