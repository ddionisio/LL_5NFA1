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
        Victory,
        Revive
    }

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
                if(mHPCurrent < 0f)
                    mHPCurrent = 0f;

                hpWidget.UpdateValue(mHPCurrent);
            }
        }
    }

    public bool isBusy { get { return mCurRout != null; } }

    public float hpMax { get; private set; }

    public Action action {
        get { return mAction; }
        set {
            if(mAction != value) {
                Stop();

                var prevAction = mAction;
                mAction = value;

                switch(prevAction) {
                    case Action.Defend:
                        if(defendActiveGO) defendActiveGO.SetActive(false);
                        break;
                }

                switch(mAction) {
                    case Action.Enter:
                        mCurRout = StartCoroutine(DoEnter());
                        break;
                    case Action.Idle:
                        if(prevAction == Action.Attack)
                            mCurRout = StartCoroutine(DoAttackToIdle());
                        else {
                            if(animator && !string.IsNullOrEmpty(takeIdle))
                                animator.Play(takeIdle);
                        }
                        break;
                    case Action.Attack:
                        mCurRout = StartCoroutine(DoAttack());
                        break;
                    case Action.Defend:
                        if(animator && !string.IsNullOrEmpty(takeDefend))
                            animator.Play(takeDefend);

                        if(defendActiveGO) defendActiveGO.SetActive(true);
                        break;
                    case Action.Hurt:
                        if(animator && !string.IsNullOrEmpty(takeHurt))
                            animator.Play(takeHurt);
                        break;
                    case Action.Death:
                        if(animator && !string.IsNullOrEmpty(takeDeath))
                            animator.Play(takeDeath);
                        break;
                    case Action.Victory:
                        if(animator && !string.IsNullOrEmpty(takeVictory))
                            animator.Play(takeVictory);
                        break;
                    case Action.Revive:
                        mCurRout = StartCoroutine(DoRevive());
                        break;                    
                }
            }
        }
    }

    private float mHPCurrent;

    private Coroutine mCurRout;

    private Action mAction;

    public void Init(float aHPMax) {
        hpMax = aHPMax;
        mHPCurrent = aHPMax;

        hpWidget.Init(hpMax);

        mAction = Action.None;

        if(animatorMove && !string.IsNullOrEmpty(takeMoveEnter))
            animatorMove.ResetTake(takeMoveEnter);
    }

    public void Stop() {
        if(mCurRout != null) {
            StopCoroutine(mCurRout);
            mCurRout = null;
        }
    }

    IEnumerator DoEnter() {
        if(animatorMove && !string.IsNullOrEmpty(takeMoveEnter))
            yield return animatorMove.PlayWait(takeMoveEnter);

        mCurRout = null;
    }

    IEnumerator DoAttackToIdle() {
        if(animatorMove && !string.IsNullOrEmpty(takeMoveAttackExit))
            yield return animatorMove.PlayWait(takeMoveAttackExit);

        if(animator && !string.IsNullOrEmpty(takeIdle))
            animator.Play(takeIdle);

        mCurRout = null;
    }

    IEnumerator DoAttack() {
        if(animatorMove && !string.IsNullOrEmpty(takeMoveAttackEnter))
            yield return animatorMove.PlayWait(takeMoveAttackEnter);

        if(animator && !string.IsNullOrEmpty(takeAttack))
            yield return animator.PlayWait(takeAttack);

        mCurRout = null;
    }

    IEnumerator DoRevive() {
        if(animator && !string.IsNullOrEmpty(takeRevive))
            yield return animator.PlayWait(takeRevive);

        mCurRout = null;

        hpCurrent = hpMax;

        action = Action.Idle;
    }
}
