using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoLSpeechToggleWidget : MonoBehaviour, M8.IModalActive {
    [Header("Display")]
    public GameObject onActiveGO;
    public GameObject offActiveGO;

    public void ToggleSound() {
        bool isOn = LoLManager.instance.isSpeechMute;

        if(isOn) { //turn off
            LoLManager.instance.ApplySpeechMute(false, true);
        }
        else { //turn on
            LoLManager.instance.ApplySpeechMute(true, true);
        }

        UpdateToggleStates();
    }

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            UpdateToggleStates();
        }
    }

    private void UpdateToggleStates() {
        bool isMute = LoLManager.instance.isSpeechMute;
        
        if(onActiveGO) onActiveGO.SetActive(!isMute);
        if(offActiveGO) offActiveGO.SetActive(isMute);
    }
}
