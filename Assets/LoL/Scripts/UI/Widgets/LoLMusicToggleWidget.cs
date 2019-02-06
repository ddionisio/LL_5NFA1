using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoLMusicToggleWidget : MonoBehaviour, M8.IModalActive {
    [Header("Display")]
    public Text toggleLabel;
    [M8.Localize]
    public string onStringRef;
    [M8.Localize]
    public string offStringRef;

    [Header("Config")]
    public bool toggleRefreshMusic = true; //turn off music and save path, replay on turn on

    private float mLastMusicVolume;
    private string mLastMusicPlayingPath;
    private bool mLastMusicIsLoop;

    public void ToggleMusic() {
        bool isOn = LoLManager.instance.musicVolume > 0f;

        if(isOn) { //turn off
            mLastMusicVolume = LoLManager.instance.musicVolume;

            //save music path playing
            if(toggleRefreshMusic) {
                mLastMusicPlayingPath = LoLManager.instance.lastSoundBackgroundPath;
                mLastMusicIsLoop = LoLManager.instance.lastSoundBackgroundIsLoop;

                LoLManager.instance.StopCurrentBackgroundSound();
            }

            LoLManager.instance.ApplyVolumes(LoLManager.instance.soundVolume, 0f, true);
        }
        else { //turn on
            if(mLastMusicVolume == 0f) //need to set to default
                mLastMusicVolume = LoLManager.musicVolumeDefault;

            LoLManager.instance.ApplyVolumes(LoLManager.instance.soundVolume, mLastMusicVolume, true);

            //play back last music if there's no music playing
            if(toggleRefreshMusic) {
                var lastSoundBackgroundPath = LoLManager.instance.lastSoundBackgroundPath;
                var lastSoundBackgroundIsLoop = LoLManager.instance.lastSoundBackgroundIsLoop;

                LoLManager.instance.StopCurrentBackgroundSound();

                if(!string.IsNullOrEmpty(lastSoundBackgroundPath)) {
                    LoLManager.instance.PlaySound(lastSoundBackgroundPath, true, lastSoundBackgroundIsLoop);
                }
                else if(!string.IsNullOrEmpty(mLastMusicPlayingPath)) {
                    LoLManager.instance.PlaySound(mLastMusicPlayingPath, true, mLastMusicIsLoop);
                }

                mLastMusicPlayingPath = null;
            }
        }

        UpdateToggleStates();
    }

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            mLastMusicVolume = LoLManager.instance.musicVolume;

            UpdateToggleStates();
        }
    }
    
    private void UpdateToggleStates() {
        if(toggleLabel) {
            string txt = LoLManager.instance.musicVolume > 0f ? M8.Localize.Get(onStringRef) : M8.Localize.Get(offStringRef);
            toggleLabel.text = txt;
        }
    }
}