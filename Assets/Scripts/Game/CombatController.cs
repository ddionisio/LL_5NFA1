using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : GameModeController<CombatController> {
    [Header("Data")]
    public float playerHP;
    public float enemyHP;
    public float reviveEndDelay = 0.5f;
    public M8.SceneAssetPath nextScene;

    [Header("Victory Flags")]
    public bool victoryRoundsEnabled = true;
    public bool victoryReviveEnabled = true;

    [Header("Attack")]
    public CombatAttackController attackControl;

    [Header("Defense")]
    public CombatDefenseController defenseControl;
    
    [Header("Characters")]
    public CombatCharacterController playerControl;
    public CombatCharacterController enemyControl;

    [Header("Signal Listens")]
    public M8.Signal signalListenBegin; //if this is not null, wait for this signal before beginning fight

    [Header("Signal Invokes")]
    public M8.Signal signalReady; //called once contestants are in place

    private MixedNumber mAttackDamage = new MixedNumber(); //accumulation of damage
    private MixedNumber mDefenseAmount = new MixedNumber(); //accumulation of defense
    private int mRoundCount = 0;
    private int mReviveCount = 0;

    private bool mIsBeginWait;

    protected override void OnInstanceDeinit() {
        if(signalListenBegin) signalListenBegin.callback -= OnSignalBegin;

        base.OnInstanceDeinit();
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        playerControl.Init(playerHP);
        enemyControl.Init(enemyHP);

        if(signalListenBegin) {
            mIsBeginWait = true;
            signalListenBegin.callback += OnSignalBegin;
        }
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        //enter both contestant
        playerControl.action = CombatCharacterController.Action.Enter;
        enemyControl.action = CombatCharacterController.Action.Enter;

        while(playerControl.isBusy || enemyControl.isBusy)
            yield return null;

        if(signalReady) signalReady.Invoke();
        //

        //wait for signal
        while(mIsBeginWait)
            yield return null;

        //show vs. animation

        //some dialog/tutorial depending on level

        var waitReviveEndDelay = new WaitForSeconds(reviveEndDelay);

        //combat loop
        while(true) {
            mRoundCount++;

            /////////////////////////////////////
            //attack state
            if(attackControl) {
                attackControl.Init(playerControl, enemyControl);
                attackControl.Play();

                yield return null;

                while(attackControl.isPlaying)
                    yield return null;

                mAttackDamage += attackControl.attackTotalNumber;

                //check if enemy is dead
                if(enemyControl.hpCurrent <= 0f) {
                    break;
                }
            }
            /////////////////////////////////////

            /////////////////////////////////////
            //defend state
            if(defenseControl) {
                defenseControl.Init(enemyControl, playerControl);
                defenseControl.Play();

                yield return null;

                while(defenseControl.isPlaying)
                    yield return null;

                mDefenseAmount += defenseControl.defenseTotalNumber;

                //check if player is dead
                if(playerControl.hpCurrent <= 0f) {
                    //revive
                    mReviveCount++;

                    playerControl.action = CombatCharacterController.Action.Revive;
                    while(playerControl.isBusy)
                        yield return null;

                    //animation of hp going back up
                    playerControl.hpWidget.Show();
                    while(playerControl.hpWidget.isBusy)
                        yield return null;

                    playerControl.hpCurrent = playerControl.hpMax;

                    yield return waitReviveEndDelay;

                    playerControl.hpWidget.Hide();
                }

                if(!attackControl) //only one round if no attack control
                    break;
            }
            /////////////////////////////////////

            yield return null;
        }

        //player victory
        playerControl.action = CombatCharacterController.Action.Victory;

        while(playerControl.isBusy)
            yield return null;
        
        var victoryInfo = new VictoryInfo();

        victoryInfo.toScene = nextScene;

        if(attackControl && mAttackDamage.fValue > 0f) {
            victoryInfo.attackValue = mAttackDamage;
            victoryInfo.flags |= VictoryStatFlags.Attack;
        }

        if(defenseControl && mDefenseAmount.fValue > 0f) {
            victoryInfo.defenseValue = mDefenseAmount;
            victoryInfo.flags |= VictoryStatFlags.Defense;
        }

        if(victoryRoundsEnabled) {
            victoryInfo.roundsCount = mRoundCount;
            victoryInfo.flags |= VictoryStatFlags.Rounds;
        }

        if(victoryReviveEnabled && mReviveCount > 0) {
            victoryInfo.reviveCount = mReviveCount;
            victoryInfo.flags |= VictoryStatFlags.Revive;
        }

        GameData.instance.OpenVictory(victoryInfo);
    }

    void OnSignalBegin() {
        mIsBeginWait = false;
    }
}
