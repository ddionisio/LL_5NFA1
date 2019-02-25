﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoLSoundToggleWidget : MonoBehaviour, M8.IModalActive {
    [Header("Display")]
    public GameObject onActiveGO;
    public GameObject offActiveGO;

    private float mLastSoundVolume;

    public void ToggleSound() {
        bool isOn = LoLManager.instance.soundVolume > 0f;

        if(isOn) { //turn off
            mLastSoundVolume = LoLManager.instance.soundVolume;

            LoLManager.instance.ApplyVolumes(0f, LoLManager.instance.musicVolume, true);
        }
        else { //turn on
            if(mLastSoundVolume == 0f) //need to set to default
                mLastSoundVolume = LoLManager.soundVolumeDefault;

            LoLManager.instance.ApplyVolumes(mLastSoundVolume, LoLManager.instance.musicVolume, true);
        }

        UpdateToggleStates();
    }

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            mLastSoundVolume = LoLManager.instance.soundVolume;

            UpdateToggleStates();
        }
    }

    private void UpdateToggleStates() {
        bool isSoundOn = LoLManager.instance.soundVolume > 0f;
        
        if(onActiveGO) onActiveGO.SetActive(isSoundOn);
        if(offActiveGO) offActiveGO.SetActive(!isSoundOn);
    }
}
