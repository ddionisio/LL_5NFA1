using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CombatDefenseController))]
public class CombatDefenseControllerInspector : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(Application.isPlaying) {
            var ctrl = target as CombatDefenseController;

            M8.EditorExt.Utility.DrawSeparator();

            if(GUILayout.Button("Time Expire")) {
                ctrl.timerWidget.value = 0f;
            }
        }
    }
}