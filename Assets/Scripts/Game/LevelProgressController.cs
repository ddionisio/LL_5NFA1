using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgressController : GameModeController<LevelProgressController> {

    protected override void OnInstanceInit() {
        base.OnInstanceInit();


    }

    protected override IEnumerator Start() {
        yield return base.Start();
    }
}
