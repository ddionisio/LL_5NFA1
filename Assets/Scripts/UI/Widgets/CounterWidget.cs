using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterWidget : MonoBehaviour {
    public GameObject rootGO;

    public CounterItemWidget[] items;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeShow;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeHide;

    public int curCount { get; private set; }
    public int maxCount { get; private set; }
    public bool isBusy { get { return mRout != null; } }

    private Coroutine mRout;

    public void Init(int count) {
        int activeCount = Mathf.Min(count, items.Length);

        //setup active counters
        for(int i = 0; i < items.Length; i++) {
            items[i].gameObject.SetActive(true);
            items[i].Unfill();
        }

        //hide excess counters
        for(int i = count; i < items.Length; i++)
            items[i].gameObject.SetActive(false);

        curCount = 0;
        maxCount = count;

        if(rootGO) rootGO.SetActive(false);
    }

    public void FillIncrement() {
        if(curCount < maxCount) {
            int ind = curCount;
            curCount++;

            items[ind].Fill();
        }
    }

    public void Show() {
        Stop();
        mRout = StartCoroutine(DoShow());
    }

    public void Hide() {
        Stop();
        mRout = StartCoroutine(DoHide());
    }

    void OnDisable() {
        mRout = null;
    }

    void Awake() {
        if(rootGO) rootGO.SetActive(false);
    }

    IEnumerator DoShow() {
        if(rootGO) rootGO.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeShow))
            yield return animator.PlayWait(takeShow);

        mRout = null;
    }

    IEnumerator DoHide() {
        if(animator && !string.IsNullOrEmpty(takeHide))
            yield return animator.PlayWait(takeHide);

        if(rootGO) rootGO.SetActive(false);

        mRout = null;
    }

    private void Stop() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }
}
