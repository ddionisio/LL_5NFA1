using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPWidget : MonoBehaviour {
    [Header("Display")]
    public GameObject aliveGO;
    public GameObject deadGO;
    public Image fillImage;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeShow;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeHide;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeUpdate;

    private float mCurHP;
    private float mHPMax;

    public void Init(float hpMax) {
        mHPMax = hpMax;

        if(aliveGO) aliveGO.SetActive(true);
        if(deadGO) deadGO.SetActive(false);

        fillImage.fillAmount = 1.0f;

        if(animator && !string.IsNullOrEmpty(takeShow))
            animator.ResetTake(takeShow);
    }

    public void Show() {
        if(animator && !string.IsNullOrEmpty(takeShow))
            animator.Play(takeShow);
    }

    public IEnumerator ShowWait() {
        if(animator && !string.IsNullOrEmpty(takeShow))
            yield return animator.PlayWait(takeShow);
    }

    public IEnumerator HideWait() {
        if(animator && !string.IsNullOrEmpty(takeHide))
            yield return animator.PlayWait(takeHide);
    }

    public void UpdateValue(float curHP) {
        mCurHP = curHP;

        if(mCurHP <= 0f) {
            if(aliveGO) aliveGO.SetActive(false);
            if(deadGO) deadGO.SetActive(true);
        }

        fillImage.fillAmount = Mathf.Clamp01(mCurHP / mHPMax);

        if(animator && !string.IsNullOrEmpty(takeUpdate))
            animator.Play(takeUpdate);
    }
}
