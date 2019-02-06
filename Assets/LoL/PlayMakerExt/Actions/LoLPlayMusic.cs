using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    [Tooltip("Plays sound as background. This replaces the last background sound.")]
    public class LoLPlayMusic : FsmStateAction {
        public FsmString path;
        public FsmBool isLoop;

        public override void Reset() {
            path = null;
            isLoop = true;
        }

        public override void OnEnter() {
            if(LoLManager.isInstantiated)
                LoLManager.instance.PlaySound(path.Value, true, isLoop.Value);

            Finish();
        }
    }
}