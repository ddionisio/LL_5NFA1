using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum VictoryStatFlags {
    None = 0x0,
    Attack = 0x1,
    Defense = 0x2,
    Rounds = 0x4,
    Revive = 0x8
}

[System.Serializable]
public class LevelData {
    public M8.SceneAssetPath scene;    
}

//provide this to victory
public struct VictoryInfo {
    public MixedNumber attackValue;
    public MixedNumber defenseValue;
    public int roundsCount;
    public int reviveCount;
    [M8.EnumMask]
    public VictoryStatFlags flags;
    public M8.SceneAssetPath toScene;
}

[CreateAssetMenu(fileName = "gameData", menuName = "Game/GameData")]
public class GameData : M8.SingletonScriptableObject<GameData> {
    [Header("Level Info")]
    public LevelData[] levels; //scene index is based on progress
    public M8.SceneAssetPath sceneLevelProgress; //scene to show progression
    public M8.SceneAssetPath sceneEnd; //ending

    [Header("Game Data")]
    public float attackDuration = 30f;
    public float defendDuration = 30f;

    [Header("Score Info")]
    public int victoryScore = 1000;
    public float attackMultiplier = 100f;
    public float defenseMultiplier = 100f;
    public float revivePenality = 100f;
    public int roundPar = 5; //expected rounds to finish a fight
    public float roundBonus = 100f; //bonus based on few rounds to finish the fight: roundPar - rounds

    [Header("Victory Info")]
    public string victoryModal = "victory";

    private M8.GenericParams mVictoryParms = new M8.GenericParams();

    public bool IsCurrentSceneLast() {
        var scene = M8.SceneManager.instance.curScene;
        return levels[levels.Length - 1].scene == scene;
    }

    public LevelData GetLevelDataFromProgress() {
        LevelData ret = null;

        if(LoLManager.isInstantiated) {
            var progress = LoLManager.instance.curProgress;
            var levelIndex = progress < levels.Length ? progress : levels.Length - 1;
            ret = levels[levelIndex];
        }

        return ret;
    }

    public LevelData GetLevelDataFromScene() {
        LevelData ret = null;

        if(M8.SceneManager.isInstantiated) {
            var scene = M8.SceneManager.instance.curScene;
            for(int i = 0; i < levels.Length; i++) {
                if(levels[i].scene == scene) {
                    ret = levels[i];
                    break;
                }
            }
        }

        return ret;
    }

    public void OpenVictory(VictoryInfo victoryInfo) {
        M8.ModalManager.main.CloseAll();

        mVictoryParms[ModalVictory.parmVictoryInfo] = victoryInfo;
        M8.ModalManager.main.Open(victoryModal, mVictoryParms);
    }

    /// <summary>
    /// Load level based on current progress
    /// </summary>
    public void ProceedToLevelFromProgress() {
        var curProgress = LoLManager.instance.curProgress;
        if(curProgress >= levels.Length)
            sceneEnd.Load();
        else {
            var levelData = GetLevelDataFromProgress();
            if(levelData != null) {
                levelData.scene.Load();
            }
            else {
                Debug.LogWarning("Unable to get level data.");
            }
        }
    }
}
