using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MixedNumber))]
public class MixedNumberPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var propWhole = property.FindPropertyRelative("whole");
        var propNumerator = property.FindPropertyRelative("numerator");
        var propDenominator = property.FindPropertyRelative("denominator");

        //whole
        var rect = new Rect(position.position, new Vector2(50f, position.height));                
        propWhole.intValue = EditorGUI.IntField(rect, propWhole.intValue);

        //fraction
        rect.x += rect.width + 5f;
        rect.width = 12f;
        GUI.Label(rect, "+");

        rect.x += rect.width + 5f;
        rect.width = 8f;
        GUI.Label(rect, "(");

        rect.x += rect.width + 5f;
        rect.width = 50f;
        propNumerator.intValue = EditorGUI.IntField(rect, propNumerator.intValue);

        rect.x += rect.width + 5f;
        rect.width = 10f;
        GUI.Label(rect, "/");

        rect.x += rect.width + 5f;
        rect.width = 50f;
        propDenominator.intValue = EditorGUI.IntField(rect, propDenominator.intValue);

        rect.x += rect.width + 5f;
        rect.width = 8f;
        GUI.Label(rect, ")");

        EditorGUI.EndProperty();
    }
}
