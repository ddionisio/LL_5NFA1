using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperationsIntroController : MonoBehaviour {
    [Header("Data")]
    public MixedNumberOps ops;

    [Header("Widget")]
    public MixedNumberOpsWidget opsWidget;

    [Header("Signal Listens")]
    public M8.Signal signalOpen;
    public M8.Signal signalClose;
    public SignalBoolean signalAnswerSubmit;

    [Header("Signal Invokes")]
    public M8.Signal signalDenominatorEqual;
    public M8.Signal signalCorrect;

    private bool mCheckDenominatorEqual;
    
    void OnDestroy() {
        if(signalOpen) signalOpen.callback -= OnSignalOpen;
        if(signalClose) signalClose.callback -= OnSignalClose;
        if(signalAnswerSubmit) signalAnswerSubmit.callback -= OnSignalAnswerSubmit;
    }

    void Awake() {
        opsWidget.operation = ops;
        opsWidget.answerLocked = true; //unlock once denominator is equal
        
        signalOpen.callback += OnSignalOpen;
        signalClose.callback += OnSignalClose;
        signalAnswerSubmit.callback += OnSignalAnswerSubmit;
    }

    void Update() {
        if(mCheckDenominatorEqual)
            CheckDenominator();
    }

    void CheckDenominator() {
        if(!opsWidget.isShown)
            return;

        if(opsWidget.operandSlots.slots.Length <= 1) //fail-safe
            return;

        if(opsWidget.operandSlots.slots[0].card == null) //fail-safe
            return;

        int firstDenominator = opsWidget.operandSlots.slots[0].card.number.denominator;

        for(int i = 1; i < opsWidget.operandSlots.slots.Length; i++) {
            var slot = opsWidget.operandSlots.slots[i];
            if(slot.card == null)
                return;

            if(slot.card.number.denominator != firstDenominator)
                return;
        }

        mCheckDenominatorEqual = false;

        if(signalDenominatorEqual)
            signalDenominatorEqual.Invoke();
    }

    void OnSignalOpen() {
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
