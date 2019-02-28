using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUIController : GameModeController<TestUIController> {
    public MixedNumberOps testOps;
    public MixedNumber[] testDeck;
    public MixedNumberOpsWidget opsWidget;
    public CardDeckWidget deckWidget;

    protected override void OnInstanceInit() {
        base.OnInstanceInit();
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        opsWidget.operation = testOps;

        opsWidget.Show();

        if(deckWidget) {            
            deckWidget.Show();

            while(deckWidget.isBusy)
                yield return null;

            deckWidget.Fill(testDeck);
        }
    }
}
