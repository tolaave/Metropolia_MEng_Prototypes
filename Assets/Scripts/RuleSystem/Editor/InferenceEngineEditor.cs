using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Custom editor for the inference engine. Reformats
/// the working memory to a more readable format.
/// </summary>
[CustomEditor(typeof(InferenceEngine))]
public class InferenceEngineEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		InferenceEngine myTarget = (InferenceEngine)target;
		EditorGUI.BeginChangeCheck ();

		// Basic Unity editor fields for arbiter and ruleset
		myTarget.arbiter = EditorGUILayout.ObjectField("Arbiter", myTarget.arbiter, typeof(Arbiter), true) as Arbiter;
		myTarget.activeRules = EditorGUILayout.ObjectField("Ruleset", myTarget.activeRules, typeof(Ruleset), true) as Ruleset;

		// Show finished flag as a label
		EditorGUILayout.LabelField("Finished", myTarget.finished ? "Yes" : "No");

		// Display the working memory. 
		EditorGUILayout.LabelField("Working memory", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		var facts = myTarget.workingMemory.facts;
		if (facts.Count == 0)
		{
			// Working memory empty
			EditorGUILayout.LabelField("(empty)");
		}
		else
		{
			// Show each fact as an indented label with identifier-value
			// pair as the label field label and value.
			foreach (var fact in facts)
			{
				EditorGUILayout.LabelField(fact.identifier, fact.value);
			}
		}
		EditorGUI.indentLevel--;

		// Save changes if needed
		if (EditorGUI.EndChangeCheck ())
		{
			serializedObject.ApplyModifiedProperties();
		}
	}
}