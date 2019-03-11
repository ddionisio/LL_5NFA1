using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CombatAttackController))]
public class CombatAttackControllerInspector : Editor {
    private int mMixedNumberWhole;
    private int mMixedNumberNumerator;
    private int mMixedNumberDenominator;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(Application.isPlaying) {
            var ctrl = target as CombatAttackController;

            M8.EditorExt.Utility.DrawSeparator();

            GUILayout.BeginHorizontal();

            mMixedNumberWhole = EditorGUILayout.IntField(mMixedNumberWhole);
            mMixedNumberNumerator = EditorGUILayout.IntField(mMixedNumberNumerator);
            mMixedNumberDenominator = EditorGUILayout.IntField(mMixedNumberDenominator);

            if(GUILayout.Button("Submit")) {
                ctrl.ForceSubmit(new MixedNumber() { whole=mMixedNumberWhole, numerator=mMixedNumberNumerator, denominator=mMixedNumberDenominator });
            }

            GUILayout.EndHorizontal();

            if(GUILayout.Button("Time Expire")) {
                ctrl.timerWidget.value = 0f;
            }
        }
    }
}