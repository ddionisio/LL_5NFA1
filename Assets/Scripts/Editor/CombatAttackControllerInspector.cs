using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CombatAttackController))]
public class CombatAttackControllerInspector : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(Application.isPlaying) {
            var ctrl = target as CombatAttackController;

            M8.EditorExt.Utility.DrawSeparator();

            if(GUILayout.Button("Time Expire")) {
                ctrl.timerWidget.value = 0f;
            }
        }
    }
}