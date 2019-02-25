using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUIController : GameModeController<TestUIController> {
    protected override void OnInstanceInit() {
        base.OnInstanceInit();
    }

    protected override IEnumerator Start() {
        yield return base.Start();


    }
}
