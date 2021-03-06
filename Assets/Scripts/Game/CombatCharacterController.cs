﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCharacterController : MonoBehaviour {
    public enum Action {
        None,

        Enter,
        Idle,
        AttackEnter,
        Attack,        
        Defend,
        Hurt,
        Death,        
        Victory,
        Revive,
        Startle
    }

    [Header("Displays")]
    public HPWidget hpWidget;
    public GameObject defendActiveGO;
    public GameObject attackActiveGO;
    public GameObject deadActiveGO;
    public float attackDelay;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeAttackEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeAttackExit;
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
            if(value < 0f)
                value = 0f;

            if(mHPCurrent != value) {
                mHPCurrent = value;

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
                if(!Application.isPlaying)
                    return;

                Stop();

                var prevAction = mAction;
                mAction = value;

                switch(prevAction) {
                    case Action.Defend:
                        if(defendActiveGO) defendActiveGO.SetActive(false);
                        break;
                    case Action.Attack:
                        if(attackActiveGO) attackActiveGO.SetActive(false);
                        break;
                    case Action.Death:
                        if(deadActiveGO) deadActiveGO.SetActive(false);
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
                    case Action.AttackEnter:
                        mCurRout = StartCoroutine(DoAttackEnter());
                        break;
                    case Action.Attack:
                        if(prevAction == Action.AttackEnter)
                            mCurRout = StartCoroutine(DoAttack());
                        else
                            mCurRout = StartCoroutine(DoIdleToAttack());
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
                        if(deadActiveGO) deadActiveGO.SetActive(true);

                        if(animator && !string.IsNullOrEmpty(takeDeath))
                            animator.Play(takeDeath);
                        break;
                    case Action.Victory:
                        mCurRout = StartCoroutine(DoVictory());
                        break;
                    case Action.Revive:
                        mCurRout = StartCoroutine(DoRevive());
                        break;
                    case Action.Startle:
                        mCurRout = StartCoroutine(DoStartle());
                        break;
                }
            }
        }
    }

    private float mHPCurrent;

    private Coroutine mCurRout;

    private Action mAction;

    //use for proxy
    public void SetAction(Action toAction) {
        action = toAction;
    }

    public void Init(float aHPMax) {
        if(defendActiveGO) defendActiveGO.SetActive(false);
        if(attackActiveGO) attackActiveGO.SetActive(false);
        if(deadActiveGO) deadActiveGO.SetActive(false);

        hpMax = aHPMax;
        mHPCurrent = aHPMax;

        hpWidget.Init(hpMax);

        mAction = Action.None;

        if(animator && !string.IsNullOrEmpty(takeEnter))
            animator.ResetTake(takeEnter);
    }

    public void Stop() {
        if(mCurRout != null) {
            StopCoroutine(mCurRout);
            mCurRout = null;
        }
    }

    IEnumerator DoEnter() {
        if(animator && !string.IsNullOrEmpty(takeEnter))
            yield return animator.PlayWait(takeEnter);
        else
            yield return null;

        mCurRout = null;

        action = Action.Idle;
    }

    IEnumerator DoAttackEnter() {
        if(animator && !string.IsNullOrEmpty(takeAttackEnter))
            yield return animator.PlayWait(takeAttackEnter);

        mCurRout = null;
    }

    IEnumerator DoAttack() {
        if(animator && !string.IsNullOrEmpty(takeAttack))
            animator.Play(takeAttack);

        if(attackActiveGO)
            attackActiveGO.SetActive(true);

        yield return new WaitForSeconds(attackDelay);

        if(attackActiveGO)
            attackActiveGO.SetActive(false);

        mCurRout = null;
    }

    IEnumerator DoIdleToAttack() {
        if(animator && !string.IsNullOrEmpty(takeAttackEnter))
            yield return animator.PlayWait(takeAttackEnter);

        mCurRout = StartCoroutine(DoAttack());
    }

    IEnumerator DoAttackToIdle() {
        if(animator && !string.IsNullOrEmpty(takeAttackExit))
            yield return animator.PlayWait(takeAttackExit);

        if(animator && !string.IsNullOrEmpty(takeIdle))
            animator.Play(takeIdle);

        mCurRout = null;
    }
        
    IEnumerator DoRevive() {
        if(animator && !string.IsNullOrEmpty(takeRevive))
            yield return animator.PlayWait(takeRevive);
        else
            yield return null;

        mCurRout = null;

        //hpCurrent = hpMax;

        action = Action.Idle;
    }

    IEnumerator DoVictory() {
        if(animator && !string.IsNullOrEmpty(takeVictory))
            yield return animator.PlayWait(takeVictory);

        mCurRout = null;
    }

    IEnumerator DoStartle() {
        if(animator && !string.IsNullOrEmpty(takeHurt))
            yield return animator.PlayWait(takeHurt);
        else
            yield return null;

        mCurRout = null;

        action = Action.Idle;
    }
}
