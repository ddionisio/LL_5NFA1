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

        var scale = 1f / 3f;
        var width = position.width * scale;

        //whole
        var rect = new Rect(position.position, new Vector2(width, position.height));                
        propWhole.intValue = EditorGUI.IntField(rect, propWhole.intValue);

        //fraction
        rect.x += rect.width;
        propNumerator.intValue = EditorGUI.IntField(rect, propNumerator.intValue);

        rect.x += rect.width;
        propDenominator.intValue = EditorGUI.IntField(rect, propDenominator.intValue);

        EditorGUI.EndProperty();
    }
}
