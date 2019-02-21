using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPWidget : MonoBehaviour {
    [Header("Display")]
    public GameObject activeGO;
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

    public bool isBusy { get { return mRout != null; } }
    public bool isHidden { get { return activeGO && !activeGO.activeSelf; } }

    private float mCurHP;
    private float mHPMax;

    private Coroutine mRout;

    public void Init(float hpMax) {
        mCurHP = hpMax;
        mHPMax = hpMax;

        if(aliveGO) aliveGO.SetActive(true);
        if(deadGO) deadGO.SetActive(false);

        fillImage.fillAmount = 1.0f;

        if(activeGO) activeGO.SetActive(false);
    }

    public void Show() {
        Stop();
        mRout = StartCoroutine(DoShow());
    }

    public void Hide() {
        Stop();
        mRout = StartCoroutine(DoHide());
    }
        
    public void UpdateValue(float curHP) {
        mCurHP = curHP;

        if(aliveGO) aliveGO.SetActive(mCurHP > 0f);
        if(deadGO) deadGO.SetActive(mCurHP <= 0f);

        fillImage.fillAmount = Mathf.Clamp01(mCurHP / mHPMax);

        if(animator && !string.IsNullOrEmpty(takeUpdate))
            animator.Play(takeUpdate);
    }

    void OnDisable() {
        mRout = null;
    }

    void Awake() {
        if(activeGO) activeGO.SetActive(false);
    }

    IEnumerator DoShow() {
        if(activeGO) activeGO.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeShow))
            yield return animator.PlayWait(takeShow);

        mRout = null;
    }

    IEnumerator DoHide() {
        if(animator && !string.IsNullOrEmpty(takeHide))
            yield return animator.PlayWait(takeHide);

        if(activeGO) activeGO.SetActive(false);

        mRout = null;
    }

    private void Stop() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }
}
