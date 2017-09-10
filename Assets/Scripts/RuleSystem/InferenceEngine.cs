using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/// <summary>
/// A simple inference engine prototype implemented in Unity.
/// Uses a Ruleset asset to define rules, and has editor
/// mode execution support.
/// </summary>
public class InferenceEngine : MonoBehaviour
{
	/// <summary>
	/// A single fact in the database implemented
	/// as key (identifier)-value pair.
	/// </summary>
	[Serializable]
	public class Fact
	{
		public string identifier;
		public string value;
	}

	/// <summary>
	/// The working memory implementation for inference engine
	/// </summary>
	[Serializable]
	public class WorkingMemory
	{
		/// <summary>
		/// List of all active facts in the working memory
		/// </summary>
		public List<Fact> facts;

		/// <summary>
		/// Reference to the expression parser instance
		/// </summary>
		public B83.ExpressionParser.ExpressionParser parser;

		/// <summary>
		/// Clear this working memory
		/// </summary>
		public void Clear()
		{
			facts.Clear();
			parser = new B83.ExpressionParser.ExpressionParser();
		}

		/// <summary>
		/// Apply one rule effect to the working memory.
		/// </summary>
		/// <param name="effect">Rule effect to apply.</param>
		/// <returns><c>true</c>, if working memory was changed,
		/// <c>false</c> if no changes were made.</returns>
		public bool Apply(Ruleset.RuleEffect effect)
		{
			switch (effect.type)
			{
				case Ruleset.RuleEffectType.SetIdentifier:
					if (TestSetFact(effect.identifier, effect.value))
						return true;
					break;
				case Ruleset.RuleEffectType.RemoveIdentifier:
					if (TestSetFact(effect.identifier, null))
						return true;
					break;
			}
			return false;
		}

		/// <summary>
		/// Internal method to apply fact to working memory.
		/// </summary>
		/// <param name="identifier">Fact identifier.</param>
		/// <param name="value">Value to apply. If this is null,
		/// the fact will be removed from working memory</param>
		/// <returns><c>true</c>, if working memory was changed,
		/// <c>false</c> if no changes were made.</returns>
		private bool TestSetFact(string identifier, string value)
		{
			// Update expression parser
			if (value != null)
			{
				parser.AddConst(identifier, () => Double.Parse(value));
			}
			else
			{
				parser.RemoveConst(identifier);
			}

			// Update working memory
			var oldFact = facts.FirstOrDefault(f => f.identifier == identifier);
			if (oldFact != null)
			{
				if (oldFact.value == value)
				{
					// The fact already exists with given value, no change
					return false;
				}
				else
				{
					// The fact exists but has different value, change it
					oldFact.value = value;
					return true;
				}
			}
			else
			{
				// Add new fact
				facts.Add(new Fact { identifier = identifier, value = value });
			}
			return true;
		}
	}

	public Arbiter arbiter;
	public Ruleset activeRules;
	public WorkingMemory workingMemory = new WorkingMemory();
	public bool finished;

	/// <summary>
	/// List of rules that were matched during the most recent iteration
	/// </summary>
	private List<Ruleset.Rule> matchedRules = new List<Ruleset.Rule>();

	public void Awake()
	{
		Reset();
	}

	/// <summary>
	/// Reset the inference engine. Clears the working memory.
	/// </summary>
	[ContextMenu ("Reset inference engine")]
	public void Reset()
	{
		workingMemory.Clear();
		finished = false;
		Debug.Log("Inference engine has been reset");
	}

	/// <summary>
	/// Runs one iteration of the inference engine.
	/// </summary>
	[ContextMenu ("Run one inference engine iteration")]
	public void RunIteration()
	{
		if (finished)
		{
			Debug.LogError("Inference engine finished, reset to run again");
			return;
		}

		// Clear list of previously matched rules
		matchedRules.Clear();

		// Check which rules match the current working memory
		foreach (var rule in activeRules.rules)
		{
			if (rule.Matches(workingMemory))
				matchedRules.Add(rule);
		}

		if (matchedRules.Count > 0)
		{
			// Use arbiter to pick which of the matched rules will be fired
			Ruleset.Rule rule = arbiter.PickAndApplyRule(matchedRules, workingMemory);

			// If rule was returned, working memory was changed
			if (rule != null)
			{
				Debug.Log("Applied rule " + rule + " to working memory");
			}
			else
			{
				Debug.Log("No changes in working memory, finishing");
				finished = true;
			}
		}
		else
		{
			// No more matching rules, finish
			Debug.Log("No rules matched, finishing");
			finished = true;
		}
	}
}
