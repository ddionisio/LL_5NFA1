using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoLPlaySoundRandom : MonoBehaviour {
    public string[] paths;
    public bool isBackground;
    public bool isLoop;

    public void Play() {
        if(LoLManager.isInstantiated) {
            int ind = Random.Range(0, paths.Length);
            LoLManager.instance.PlaySound(paths[ind], isBackground, isLoop);
        }
    }
}
