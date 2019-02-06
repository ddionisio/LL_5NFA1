using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    public class LoLStopSound : FsmStateAction {
        public FsmString path;

        public override void Reset() {
            path = null;
        }

        public override void OnEnter() {
            if(LoLManager.isInstantiated)
                LoLManager.instance.StopSound(path.Value);

            Finish();
        }
    }
}