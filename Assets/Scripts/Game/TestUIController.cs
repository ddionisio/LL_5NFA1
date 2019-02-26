using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUIController : GameModeController<TestUIController> {
    public MixedNumberOps testOps;
    public MixedNumberOpsWidget opsWidget;

    protected override void OnInstanceInit() {
        base.OnInstanceInit();
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        opsWidget.operation = testOps;

        opsWidget.Show();
    }
}
