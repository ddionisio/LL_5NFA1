using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalVictory : M8.ModalController, M8.IModalPush, M8.IModalPop {
    public const string parmVictoryInfo = "victoryInfo";

    [Header("Attack Stats")]
    public GameObject attackRootGO;
    public MixedNumberWidget attackValueDisplay;
    public TMPro.TMP_Text attackScoreText;

    [Header("Defense Stats")]
    public GameObject defenseRootGO;
    public MixedNumberWidget defenseValueDisplay;
    public TMPro.TMP_Text defenseScoreText;

    [Header("Rounds Stats")]
    public GameObject roundsRootGO;
    public TMPro.TMP_Text roundsText;
    public string roundsFormat = "{0}/{1}";
    public TMPro.TMP_Text roundsScoreText;

    [Header("Finish Stats")]
    public TMPro.TMP_Text finishScoreText;

    [Header("Revive Stats")]
    public GameObject reviveRootGO;
    public TMPro.TMP_Text reviveCountText;
    public TMPro.TMP_Text reviveScoreText;

    [Header("Total XP")]
    public TMPro.TMP_Text totalScoreText;

    private int mTotalScore;

    public void Proceed() {
        Close();

        //update LoL info
        int curProgress = LoLManager.instance.curProgress;
        int curScore = mTotalScore;

        LoLManager.instance.ApplyProgress(curProgress + 1, curScore);
        //

        if(GameData.instance.IsCurrentSceneLast())
            GameData.instance.sceneEnd.Load();
        else
            GameData.instance.sceneLevelProgress.Load();
    }

    void M8.IModalPop.Pop() {

    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        var inf = parms.GetValue<VictoryInfo>(parmVictoryInfo);

        int attackScore = Mathf.CeilToInt(inf.attackValue.fValue * GameData.instance.attackMultiplier);
        int defenseScore = Mathf.CeilToInt(inf.defenseValue.fValue * GameData.instance.defenseMultiplier);

        int roundsScore = Mathf.RoundToInt((GameData.instance.roundPar - inf.roundsCount) * GameData.instance.roundBonus);
        if(roundsScore < 0)
            roundsScore = 0;

        int finishScore = GameData.instance.victoryScore;

        int revivePenalty = Mathf.RoundToInt(inf.reviveCount * GameData.instance.revivePenality);

        mTotalScore = (attackScore + defenseScore + roundsScore + finishScore) - revivePenalty;

        //attack
        if((inf.flags & VictoryStatFlags.Attack) != VictoryStatFlags.None) {
            if(attackRootGO) attackRootGO.SetActive(true);

            if(attackValueDisplay) attackValueDisplay.number = inf.attackValue;
            if(attackScoreText) attackScoreText.text = "+" + attackScore.ToString();
        }
        else {
            if(attackRootGO) attackRootGO.SetActive(false);
        }

        //defense
        if((inf.flags & VictoryStatFlags.Defense) != VictoryStatFlags.None) {
            if(defenseRootGO) defenseRootGO.SetActive(true);

            if(defenseValueDisplay) defenseValueDisplay.number = inf.defenseValue;
            if(defenseScoreText) defenseScoreText.text = "+" + defenseScore.ToString();
        }
        else {
            if(defenseRootGO) defenseRootGO.SetActive(false);
        }

        if((inf.flags & VictoryStatFlags.Rounds) != VictoryStatFlags.None) {
            if(roundsRootGO) roundsRootGO.SetActive(true);

            if(roundsText) roundsText.text = string.Format(roundsFormat, inf.roundsCount, GameData.instance.roundPar);
            if(roundsScoreText) roundsScoreText.text = "+" + roundsScore.ToString();
        }
        else {
            if(roundsRootGO) roundsRootGO.SetActive(false);
        }

        //finish
        if(finishScoreText) finishScoreText.text = "+" + finishScore.ToString();

        //revive
        if((inf.flags & VictoryStatFlags.Revive) != VictoryStatFlags.None) {
            if(reviveRootGO) reviveRootGO.SetActive(true);

            if(reviveCountText) reviveCountText.text = inf.reviveCount.ToString();
            if(reviveScoreText) reviveScoreText.text = (-revivePenalty).ToString();
        }
        else {
            if(reviveRootGO) reviveRootGO.SetActive(false);
        }

        if(totalScoreText) totalScoreText.text = mTotalScore.ToString();
    }
}
