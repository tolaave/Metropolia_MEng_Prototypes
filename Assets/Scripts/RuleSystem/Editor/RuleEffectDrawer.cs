using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor drawer for the rule effects.
/// Formats the effect field to a bit more
/// readable format.
/// </summary>
[CustomPropertyDrawer(typeof(Ruleset.RuleEffect))]
public class RuleEffectDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);
		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

		var indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// Position the three properties horizontally
		var typeRect = new Rect(position.x, position.y, 100, position.height);
		var identifierRect = new Rect(position.x + 105, position.y, 50, position.height);
		var valueRect = new Rect(
			                position.x + 160,
			                position.y,
			                position.width - 160,
			                position.height);

		// Show editor fields for all three properties.
		var typeProperty = property.FindPropertyRelative("type");
		EditorGUI.PropertyField(typeRect, typeProperty, GUIContent.none);
		EditorGUI.PropertyField(
			identifierRect,
			property.FindPropertyRelative("identifier"),
			GUIContent.none);
		
		// If effect type is RemoveIdentifier, the "value" field is not needed
		// and can be hidden
		if (typeProperty.enumValueIndex != (int)Ruleset.RuleEffectType.RemoveIdentifier)
		{
			EditorGUI.PropertyField(
				valueRect,
				property.FindPropertyRelative("value"),
				GUIContent.none);
		}

		EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty();
	}
}