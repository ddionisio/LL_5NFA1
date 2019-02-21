using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : GameModeController<CombatController> {
    [Header("Data")]
    public float playerHP;
    public float enemyHP;

    [Header("Controls")]
    public CombatAttackController attackControl;
    public CombatDefenseController defenseControl;

    [Header("Characters")]
    public CombatCharacterController playerControl;
    public CombatCharacterController enemyControl;

    private MixedNumber mAttackDamage = new MixedNumber(); //accumulation of damage
    private MixedNumber mDefenseAmount = new MixedNumber(); //accumulation of defense
    private int mRoundCount = 0;
    private int mReviveCount = 0;

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        playerControl.Init(playerHP);
        enemyControl.Init(enemyHP);
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        //enter both contestant
        playerControl.action = CombatCharacterController.Action.Enter;
        enemyControl.action = CombatCharacterController.Action.Enter;

        while(playerControl.isBusy || enemyControl.isBusy)
            yield return null;
        //

        //show vs. animation

        //some dialog/tutorial depending on level

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

            }
            /////////////////////////////////////

            yield return null;
        }

        //player victory
        playerControl.action = CombatCharacterController.Action.Victory;

        //open victory modal
    }
}
