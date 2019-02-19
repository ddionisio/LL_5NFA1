using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCharacterController : MonoBehaviour {
    [Header("Data")]
    public float hpMax;

    [Header("Displays")]
    public HPWidget hpWidget;

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

    private float mHPCurrent;

    private Coroutine mCurRout;

    public void Init() {
        mHPCurrent = hpMax;

        hpWidget.Init(hpMax);
    }
}
