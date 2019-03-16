using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperationsIntroWholeNumberController : MonoBehaviour {
    [Header("Data")]
    public MixedNumberOps ops;

    [Header("Widget")]
    public MixedNumberOpsWidget opsWidget;
    public DragToGuideWidget dragGuideWidget;
    
    [Header("Signal Listens")]
    public M8.Signal signalOpen;
    public M8.Signal signalClose;
    public M8.Signal signalHelpShow;
    public M8.Signal signalHelpHide;
    public SignalBoolean signalAnswerSubmit;

    [Header("Signal Invokes")]
    public M8.Signal signalCorrect;

    private Coroutine mHelpShowRout;

    void OnDestroy() {
        if(signalOpen) signalOpen.callback -= OnSignalOpen;
        if(signalClose) signalClose.callback -= OnSignalClose;

        if(signalHelpShow) signalHelpShow.callback -= OnSignalHelpShow;
        if(signalHelpHide) signalHelpHide.callback -= OnSignalHelpHide;

        if(signalAnswerSubmit) signalAnswerSubmit.callback -= OnSignalAnswerSubmit;
    }

    void Awake() {
        opsWidget.gameObject.SetActive(true);

        signalOpen.callback += OnSignalOpen;
        signalClose.callback += OnSignalClose;

        signalHelpShow.callback += OnSignalHelpShow;
        signalHelpHide.callback += OnSignalHelpHide;

        signalAnswerSubmit.callback += OnSignalAnswerSubmit;
    }

    void OnSignalOpen() {
        opsWidget.operation = ops;

        opsWidget.Show();
    }

    void OnSignalClose() {
        opsWidget.Hide();
    }

    void OnSignalHelpShow() {
        if(mHelpShowRout != null)
            StopCoroutine(mHelpShowRout);

        mHelpShowRout = StartCoroutine(DoHelpDisplay());
    }

    void OnSignalHelpHide() {
        if(mHelpShowRout != null) {
            StopCoroutine(mHelpShowRout);
            mHelpShowRout = null;
        }

        dragGuideWidget.Hide();
    }

    void OnSignalAnswerSubmit(bool isCorrect) {
        //show hint if incorrect by # amount

        if(isCorrect) {
            if(signalCorrect)
                signalCorrect.Invoke();
        }
    }

    IEnumerator DoHelpDisplay() {
        //find the first operand card
        CardWidget card = null;

        while(true) {
            if(opsWidget.operation != null && opsWidget.operation.operands.Length > 0)
                card = opsWidget.operandSlots.slots[0].card;

            if(card)
                break;

            yield return null;
        }

        Vector2 wholePos = card.numberWidget.wholeRootGO.transform.position;
        Vector2 fractionPos = card.numberWidget.fractionRootGO.transform.position;

        var dragDelay = dragGuideWidget.cursorFadeDelay * 2f + dragGuideWidget.cursorIdleDelay * 5f + dragGuideWidget.cursorMoveDelay;

        var waitDrag = new WaitForSeconds(dragDelay);

        while(true) {
            dragGuideWidget.Show(false, wholePos, fractionPos);

            yield return waitDrag;

            dragGuideWidget.Show(false, fractionPos, wholePos);

            yield return waitDrag;
        }
    }
}
