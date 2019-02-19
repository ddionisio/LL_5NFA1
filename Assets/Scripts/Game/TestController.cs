using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : GameModeController<TestController> {
    [Header("Data")]
    public MixedNumberOps testOps;
    public MixedNumber[] testDeck;

    [Header("Widgets")]
    public MixedNumberOpsWidget opsWidget;
    public CardDeckWidget deckWidget;
    
    [Header("Signals")]
    public SignalBoolean signalSubmit;

    protected override void OnInstanceDeinit() {
        signalSubmit.callback -= OnOpsProceed;

        base.OnInstanceDeinit();
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        signalSubmit.callback += OnOpsProceed;
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        ModalDialog.Open("", "test", ModalDialog.CloseGeneric);
        
        opsWidget.operation = testOps;
        opsWidget.Show();

        deckWidget.Fill(testDeck);
    }

    void OnOpsProceed(bool correct) {
        if(correct) {
            Debug.Log("CORRECT");

            opsWidget.MoveAnswerToOperand(0);

            deckWidget.Fill(testDeck);
        }
        else
            Debug.Log("WRONG");
    }
}
