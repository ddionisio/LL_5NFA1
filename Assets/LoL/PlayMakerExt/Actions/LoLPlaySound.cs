using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    public class LoLPlaySound : FsmStateAction {
        public FsmString path;
        public FsmBool isBackground;
        public FsmBool isLoop;

        public override void Reset() {
            path = null;
            isBackground = false;
            isLoop = false;
        }

        public override void OnEnter() {
            if(LoLManager.isInstantiated)
                LoLManager.instance.PlaySound(path.Value, isBackground.Value, isLoop.Value);

            Finish();
        }
    }
}