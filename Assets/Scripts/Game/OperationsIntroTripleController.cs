using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperationsIntroTripleController : MonoBehaviour {
    [Header("Data")]
    public MixedNumberOps ops;

    [Header("Widget")]
    public MixedNumberOpsWidget opsWidget;

    [Header("Signal Listens")]
    public M8.Signal signalOpen;
    public M8.Signal signalClose;
    public SignalBoolean signalAnswerSubmit;

    [Header("Signal Invokes")]
    public M8.Signal signalCorrect;

    void OnDestroy() {
        if(signalOpen) signalOpen.callback -= OnSignalOpen;
        if(signalClose) signalClose.callback -= OnSignalClose;

        if(signalAnswerSubmit) signalAnswerSubmit.callback -= OnSignalAnswerSubmit;
    }

    void Awake() {
        opsWidget.gameObject.SetActive(true);

        signalOpen.callback += OnSignalOpen;
        signalClose.callback += OnSignalClose;

        signalAnswerSubmit.callback += OnSignalAnswerSubmit;
    }

    void OnSignalOpen() {
        opsWidget.operation = ops;

        opsWidget.Show();
    }

    void OnSignalClose() {
        opsWidget.Hide();
    }
    
    void OnSignalAnswerSubmit(bool isCorrect) {
        //show hint if incorrect by # amount

        if(isCorrect) {
            if(signalCorrect)
                signalCorrect.Invoke();
        }
    }
}
