using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MixedNumberConversionWidget : MonoBehaviour, IPointerClickHandler {
    public enum Mode {
        WholeToFraction,
        FractionToWhole
    }

    public Mode mode;
    public MixedNumberWidget target;
    public M8.UI.Events.HoverGOSetActive highlight;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeProcess;
    public string takeFinish;

    private Coroutine mProcessRout;

    void OnDisable() {
        mProcessRout = null;
    }

    void OnEnable() {
        OnTargetUpdate();
    }

    void OnDestroy() {
        if(target)
            target.numberUpdateCallback -= OnTargetUpdate;
    }

    void Awake() {
        target.numberUpdateCallback += OnTargetUpdate;

        OnTargetUpdate();
    }

    void OnTargetUpdate() {
        if(mProcessRout != null)
            highlight.enabled = IsValid();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if(mProcessRout == null && highlight.enabled) {
            mProcessRout = StartCoroutine(DoProcess());
        }
    }

    IEnumerator DoProcess() {
        highlight.enabled = false;

        if(animator && !string.IsNullOrEmpty(takeProcess))
            yield return animator.PlayWait(takeProcess);

        //apply
        switch(mode) {
            case Mode.FractionToWhole:
                target.number.FractionToWholeSingle();
                break;
            case Mode.WholeToFraction:
                target.number.WholeToFractionSingle();
                break;
        }

        target.RefreshDisplay();

        if(!string.IsNullOrEmpty(takeFinish))
            animator.Play(takeFinish);

        mProcessRout = null;

        highlight.enabled = IsValid();
    }

    bool IsValid() {
        switch(mode) {
            case Mode.FractionToWhole:
                return target.number.isValid && target.number.numerator > target.number.denominator;
            case Mode.WholeToFraction:
                return target.number.isValid && target.number.whole != 0;
            default:
                return false;
        }
    }
}
