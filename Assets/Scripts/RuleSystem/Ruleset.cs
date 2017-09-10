using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// The Ruleset ScriptableObject defines an unity asset, which is
/// used to configure rule sets for the inference engine.
/// </summary>
[CreateAssetMenu(fileName = "New Ruleset", menuName = "RuleSystem/Ruleset", order = 1)]
public class Ruleset : ScriptableObject
{
	/// <summary>
	/// Rule effect type, each effect can set or clear
	/// a given fact in the working memory.
	/// </summary>
	[Serializable]
	public enum RuleEffectType
	{
		SetIdentifier,
		RemoveIdentifier
	}

	/// <summary>
	/// Rule effect. Contains type, fact identifier and fact value.
	/// </summary>
	[Serializable]
	public class RuleEffect
	{
		public RuleEffectType type;
		public string identifier;
		public string value;

		public override string ToString ()
		{
			return "[" + type + "]" + identifier + "=" + value;
		}
	}

	/// <summary>
	/// One individual rule in the rule set. Contains list of
	/// conditions which must be satisfied for the rule to fire,
	/// and list of rule effects which will be applied to the
	/// working memory if the conditions were met.
	/// </summary>
	[Serializable]
	public class Rule
	{
		/// <summary>
		/// Conditions of the rule. Each condition is an expression
		/// that can be resolved into boolean value of either
		/// 0 or 1. For example:
		/// 
		///     a==1
		///     b!=0
		/// 
		/// Any previously set identifiers in working memory can
		/// be used in the expressions.
		/// </summary>
		public string[] conditions;

		/// <summary>
		/// Effects of the rule
		/// </summary>
		public RuleEffect[] effects;

		/// <summary>
		/// Matches the rule conditions against the working memory.
		/// </summary>
		/// <param name="memory">The working memory instance.</param>
		/// <returns><c>true</c>, if all conditions match the
		/// working memory, <c>false</c> otherwise.</returns>
		public bool Matches(InferenceEngine.WorkingMemory memory)
		{
			foreach (var condition in conditions)
			{
				if (memory.parser.Evaluate(condition) < 0.5f)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Apply effects to the working memory
		/// </summary>
		/// <returns><c>true</c>, if the working memory was changed,
		/// <c>false</c> otherwise.</returns>
		/// <param name="memory">The working memory instance.</param>
		public bool ApplyTo(InferenceEngine.WorkingMemory memory)
		{
			bool changed = false;
			foreach (var effect in effects)
			{
				changed |= memory.Apply(effect);
			}
			return changed;
		}

		public override string ToString ()
		{
			return "[Rule: Conditions:(" +
				String.Join(",", conditions) + "), Effects:(" + 
				String.Join(",", effects.Select(c => c.ToString()).ToArray()) + ")]";
		}
	}

	/// <summary>
	/// Root rule container of the ruleset
	/// </summary>
	public Rule[] rules;
}
