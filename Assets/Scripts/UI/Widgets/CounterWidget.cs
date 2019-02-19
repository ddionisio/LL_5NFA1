using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterWidget : MonoBehaviour {
    
    public CounterItemWidget[] items;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeShow;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeHide;

    public int curCount { get; private set; }
    public int maxCount { get; private set; }

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

        if(animator && !string.IsNullOrEmpty(takeShow))
            animator.ResetTake(takeShow);
    }

    public void FillIncrement() {
        if(curCount < maxCount) {
            int ind = curCount;
            curCount++;

            items[ind].Fill();
        }
    }

    public void Show() {
        if(animator && !string.IsNullOrEmpty(takeShow))
            animator.Play(takeShow);
    }

    public void Hide() {
        if(animator && !string.IsNullOrEmpty(takeHide))
            animator.Play(takeHide);
    }

    void Awake() {
        if(animator && !string.IsNullOrEmpty(takeShow))
            animator.ResetTake(takeShow);
    }
}
