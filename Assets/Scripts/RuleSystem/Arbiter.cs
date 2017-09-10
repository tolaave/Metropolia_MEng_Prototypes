using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// A simple arbiter component of the inference engine
/// </summary>
public class Arbiter : MonoBehaviour
{
	/// <summary>
	/// Arbiter type used for rule matching
	/// </summary>
	public enum ArbiterType 
	{
		FirstMatch,
		Random
	}

	public ArbiterType type;

	/// <summary>
	/// Superclass of the rule matching methods. Should be
	/// overridden to provide different rule-matching types.
	/// </summary>
	abstract class ArbiterMethod
	{
		protected InferenceEngine.WorkingMemory workingMemory;

		public ArbiterMethod(InferenceEngine.WorkingMemory workingMemory)
		{
			this.workingMemory = workingMemory;
		}

		public abstract Ruleset.Rule PickAndApplyRule(ICollection<Ruleset.Rule> rules);
	}

	/// <summary>
	/// First-match rule arbiter matching method. Iterates though
	/// the given rules until one of them has effect on working memory.
	/// </summary>
	class ArbiterMethodFirstMatch : ArbiterMethod
	{
		public ArbiterMethodFirstMatch(InferenceEngine.WorkingMemory workingMemory) : base(workingMemory) {}

		public override Ruleset.Rule PickAndApplyRule(ICollection<Ruleset.Rule> rules)
		{
			foreach (var rule in rules)
			{
				if (rule.ApplyTo(workingMemory))
					return rule;
			}
			return null;
		}
	}

	/// <summary>
	/// Random-match rule arbiter mathod. Iterates the given rules in
	/// random order until one of them has effect on working memory.
	/// </summary>
	class ArbiterMethodRandom : ArbiterMethod
	{
		public ArbiterMethodRandom(InferenceEngine.WorkingMemory workingMemory) : base(workingMemory) {}

		public override Ruleset.Rule PickAndApplyRule(ICollection<Ruleset.Rule> rules)
		{
			// Randomize the rule collection using LINQ OrderBy and Random
			var rnd = new System.Random();
			var randomizedList = rules.ToList().OrderBy(i => rnd.Next());
			foreach (var rule in randomizedList)
			{
				if (rule.ApplyTo(workingMemory))
					return rule;
			}
			return null;
		}
	}

	/// <summary>
	/// Creates an instance of the currently active arbiter
	/// matching method type, and uses that to pick and fire
	/// a rule from the rule list.
	/// </summary>
	/// <returns>The rule that was applied. May be null if
	/// no changes were made to the working memory.</returns>
	/// <param name="rules">List of available rules to apply.</param>
	/// <param name="workingMemory">The working memory instance.</param>
	public Ruleset.Rule PickAndApplyRule(ICollection<Ruleset.Rule> rules, InferenceEngine.WorkingMemory workingMemory)
	{
		if (rules.Count == 0)
			return null;
		
		switch (type)
		{
			case ArbiterType.FirstMatch:
				return new ArbiterMethodFirstMatch(workingMemory).PickAndApplyRule(rules);
			case ArbiterType.Random:
				return new ArbiterMethodRandom(workingMemory).PickAndApplyRule(rules);
		}
		return null;
	}
}
