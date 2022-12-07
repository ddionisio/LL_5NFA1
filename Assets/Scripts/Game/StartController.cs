using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartController : GameModeController<StartController> {
    [Header("Signals")]
    public M8.Signal signalNew;
    public M8.Signal signalContinue;

    protected override void OnInstanceDeinit() {
        if(signalNew)
            signalNew.callback -= OnSignalNew;

        if(signalContinue)
            signalContinue.callback -= OnSignalProceed;

        base.OnInstanceDeinit();
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        if(signalNew)
            signalNew.callback += OnSignalNew;

        if(signalContinue)
            signalContinue.callback += OnSignalProceed;
    }

    void OnSignalNew() {
        LoLManager.instance.ApplyProgress(0, 0);
        GameData.instance.ProceedToLevelFromProgress();
    }

    void OnSignalProceed() {
        GameData.instance.ProceedToLevelFromProgress();
    }
}
