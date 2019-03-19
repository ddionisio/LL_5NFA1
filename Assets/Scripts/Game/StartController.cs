using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartController : GameModeController<StartController> {
    [Header("Signals")]
    public M8.Signal signalProceed;

    protected override void OnInstanceDeinit() {
        signalProceed.callback -= OnSignalProceed;

        base.OnInstanceDeinit();
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        signalProceed.callback += OnSignalProceed;
    }

    void OnSignalProceed() {
        GameData.instance.ProceedToLevelFromProgress();
    }
}
