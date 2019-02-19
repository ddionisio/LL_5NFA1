using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCharacterController : MonoBehaviour {
    public enum Action {
        None,

        Enter,
        Idle,
        Attack,
        Defend,
        Hurt,
        Death,
        Revive,
        Victory
    }

    [Header("Data")]
    public float hpMax;

    [Header("Displays")]
    public HPWidget hpWidget;
    public GameObject defendActiveGO;

    [Header("Animation Move")]
    public M8.Animator.Animate animatorMove;
    [M8.Animator.TakeSelector(animatorField = "animatorMove")]
    public string takeMoveEnter;
    [M8.Animator.TakeSelector(animatorField = "animatorMove")]
    public string takeMoveAttackEnter;
    [M8.Animator.TakeSelector(animatorField = "animatorMove")]
    public string takeMoveAttackExit;

    [Header("Animation")]
    public M8.Animator.Animate animator;    
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeIdle;    
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeAttack;    
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeDefend;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeHurt;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeDeath;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeRevive;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeVictory;

    public float hpCurrent {
        get { return mHPCurrent; }
        set {
            if(mHPCurrent != value) {
                mHPCurrent = value;

                hpWidget.UpdateValue(mHPCurrent);
            }
        }
    }

    public bool isBusy { get { return mCurRout != null; } }

    public Action action {
        get { return mAction; }
        set {
            if(mAction != value) {
                var prevAction = mAction;
                mAction = value;

            }
        }
    }

    private float mHPCurrent;

    private Coroutine mCurRout;

    private Action mAction;

    public void Init() {
        mHPCurrent = hpMax;

        hpWidget.Init(hpMax);

        mAction = Action.None;
    }

    public void Stop() {
        if(mCurRout != null) {
            StopCoroutine(mCurRout);
            mCurRout = null;
        }
    }
}
