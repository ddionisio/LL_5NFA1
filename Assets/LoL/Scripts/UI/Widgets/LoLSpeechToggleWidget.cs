using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoLSpeechToggleWidget : MonoBehaviour, M8.IModalActive {
    [Header("Display")]
    public Text toggleLabel;
    [M8.Localize]
    public string onStringRef;
    [M8.Localize]
    public string offStringRef;

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
        if(toggleLabel) {
            string txt = LoLManager.instance.isSpeechMute ? M8.Localize.Get(offStringRef) : M8.Localize.Get(onStringRef);
            toggleLabel.text = txt;
        }
    }
}
