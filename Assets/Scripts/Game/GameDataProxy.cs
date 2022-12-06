using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataProxy : MonoBehaviour {
    public void LoadLevelProgress() {
        GameData.instance.sceneLevelProgress.Load();
    }

    public void ProceedToLevelFromProgress() {
        GameData.instance.ProceedToLevelFromProgress();
    }
}
