using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalVictory : M8.ModalController, M8.IModalPush, M8.IModalPop, M8.IModalActive {
    public const string parmVictoryInfo = "victoryInfo";

    [Header("Data")]
    public float activateDelay;

    [Header("Attack Stats")]
    public GameObject attackRootGO;
    public GameObject attackDisplayGO;
    public MixedNumberWidget attackValueDisplay;
    public Text attackScoreText;

    [Header("Defense Stats")]
    public GameObject defenseRootGO;
    public GameObject defenseDisplayGO;
    public MixedNumberWidget defenseValueDisplay;
    public Text defenseScoreText;

    [Header("Rounds Stats")]
    public GameObject roundsRootGO;
    public GameObject roundsDisplayGO;
    public Text roundsText;
    public string roundsFormat = "{0}/{1}";
    public Text roundsScoreText;

    [Header("Finish Stats")]
    public GameObject finishDisplayGO;
    public Text finishScoreText;

    [Header("Revive Stats")]
    public GameObject reviveRootGO;
    public GameObject reviveDisplayGO;
    public Text reviveCountText;
    public Text reviveScoreText;

    [Header("Total XP")]
    public GameObject totalDisplayGO;
    public M8.UI.Texts.TextCounter totalScoreCounter;

    private int mTotalScore;

    private M8.CacheList<GameObject> mActivateGOs = new M8.CacheList<GameObject>(6);

    private bool mIsActivateShown;

    public void Proceed() {
        Close();

        //update LoL info
        int curProgress = LoLManager.instance.curProgress;
        int curScore = LoLManager.instance.curScore;

        LoLManager.instance.ApplyProgress(curProgress + 1, curScore + mTotalScore);
        //

        if(GameData.instance.IsCurrentSceneLast())
            GameData.instance.sceneEnd.Load();
        else
            GameData.instance.sceneLevelProgress.Load();
    }

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            if(!mIsActivateShown) {
                mIsActivateShown = true;
                StartCoroutine(DoActivate());
            }
        }
    }

    void M8.IModalPop.Pop() {
        mActivateGOs.Clear();
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        mActivateGOs.Clear();

        var inf = parms.GetValue<VictoryInfo>(parmVictoryInfo);

        int attackScore = Mathf.CeilToInt(inf.attackValue.fValue * GameData.instance.attackMultiplier);
        int defenseScore = Mathf.CeilToInt(inf.defenseValue.fValue * GameData.instance.defenseMultiplier);

        int roundsScore = Mathf.RoundToInt((GameData.instance.roundPar - inf.roundsCount) * GameData.instance.roundBonus);
        if(roundsScore < 0)
            roundsScore = 0;

        int finishScore = GameData.instance.victoryScore;

        int revivePenalty = Mathf.RoundToInt(inf.reviveCount * GameData.instance.revivePenality);

        mTotalScore = (attackScore + defenseScore + roundsScore + finishScore) - revivePenalty;
        if(mTotalScore < 0)
            mTotalScore = 0;

        //attack        
        if((inf.flags & VictoryStatFlags.Attack) != VictoryStatFlags.None) {
            if(attackRootGO) attackRootGO.SetActive(true);

            if(attackDisplayGO)
                mActivateGOs.Add(attackDisplayGO);

            if(attackValueDisplay) attackValueDisplay.number = inf.attackValue.simplified;
            if(attackScoreText) attackScoreText.text = "+" + attackScore.ToString();
        }
        else {
            if(attackRootGO) attackRootGO.SetActive(false);
        }

        //defense        
        if((inf.flags & VictoryStatFlags.Defense) != VictoryStatFlags.None) {
            if(defenseRootGO) defenseRootGO.SetActive(true);

            if(defenseDisplayGO)
                mActivateGOs.Add(defenseDisplayGO);

            if(defenseValueDisplay) defenseValueDisplay.number = inf.defenseValue.simplified;
            if(defenseScoreText) defenseScoreText.text = "+" + defenseScore.ToString();
        }
        else {
            if(defenseRootGO) defenseRootGO.SetActive(false);
        }

        //rounds        
        if((inf.flags & VictoryStatFlags.Rounds) != VictoryStatFlags.None) {
            if(roundsRootGO) roundsRootGO.SetActive(true);

            if(roundsDisplayGO)
                mActivateGOs.Add(roundsDisplayGO);

            if(roundsText) roundsText.text = string.Format(roundsFormat, inf.roundsCount, GameData.instance.roundPar);
            if(roundsScoreText) roundsScoreText.text = "+" + roundsScore.ToString();
        }
        else {
            if(roundsRootGO) roundsRootGO.SetActive(false);
        }

        //finish
        if(finishDisplayGO)
            mActivateGOs.Add(finishDisplayGO);

        if(finishScoreText) finishScoreText.text = "+" + finishScore.ToString();

        //revive        
        if((inf.flags & VictoryStatFlags.Revive) != VictoryStatFlags.None) {
            if(reviveRootGO) reviveRootGO.SetActive(true);

            if(reviveDisplayGO)
                mActivateGOs.Add(reviveDisplayGO);

            if(reviveCountText) reviveCountText.text = inf.reviveCount.ToString();
            if(reviveScoreText) reviveScoreText.text = (-revivePenalty).ToString();
        }
        else {
            if(reviveRootGO) reviveRootGO.SetActive(false);
        }

        //total
        if(totalDisplayGO)
            mActivateGOs.Add(totalDisplayGO);

        for(int i = 0; i < mActivateGOs.Count; i++)
            mActivateGOs[i].SetActive(false);

        totalScoreCounter.SetCountImmediate(0);

        mIsActivateShown = false;
    }

    IEnumerator DoActivate() {
        var wait = new WaitForSeconds(activateDelay);
        for(int i = 0; i < mActivateGOs.Count; i++) {
            mActivateGOs[i].SetActive(true);

            yield return wait;
        }

        mActivateGOs.Clear();

        totalScoreCounter.count = mTotalScore;
    }
}
