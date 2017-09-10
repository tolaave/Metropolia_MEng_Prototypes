using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A very simple priority queue implementation. Uses
/// reversed priority values, where smaller values of
/// priority mean higher priority.
/// </summary>
public class PriorityQueue<T>
{
	/// <summary>
	/// Internal node for priority queue. Basically and single-linked
	/// list with priority(float) and value (V) pair.
	/// </summary>
	private class PQNode<V> where V : T
	{
		public float priority;
		public V value;
		public PQNode<T> next;

		public PQNode(float priority, V value)
		{
			this.priority = priority;
			this.value = value;
		}
	}

	// Root of the linked list
	private PQNode<T> first;

	public bool Empty { get { return first == null; } }

	public void Clear()
	{
		first = null;
	}

	/// <summary>
	/// Insert a value into the priority queue with given priority.
	/// Internally, it iterates the linked list until it finds
	/// the correct nodes between which it should insert it. 
	/// </summary>
	/// <param name="priority">Priority of the inserted item
	/// (smaller values indicate higher priority)</param>
	/// <param name="value">Actual item being inserted</param>
	public void Insert(float priority, T value)
	{
		PQNode<T> node = new PQNode<T>(priority, value);

		// List empty? Add as the only node
		if (Empty)
		{
			first = node;
			return;
		}
		else
		{
			var v = first;

			// First item priority lower (greater value) than
			// this node? Add in front and exit.
			if (v.priority >= priority)
			{
				node.next = first;
				first = node;
				return;
			}

			// Loop until proper priority placement found
			for (;;)
			{
				// If ran out of nodes, add as last node
				if (v.next == null)
				{
					v.next = node;
					return;
				}
				// If next node priority is lower,
				// add before it
				else if (v.next.priority >= priority)
				{
					// Insert after current node
					node.next = v.next;
					v.next = node;
					return;
				}
				v = v.next;
			}
		}
	}

	/// <summary>
	/// Validate integrity of the priority queue, checks
	/// that no items in queue are out-of-order.
	/// </summary>
	public void Validate()
	{
		var node = first;
		var prevPriority = first.priority;
		while (node.next != null)
		{
			if (node.priority < prevPriority)
				throw new System.FormatException("Priority queue out of order");
			prevPriority = node.priority;
			node = node.next;
		}
	}

	/// <summary>
	/// Removes and returns the highest-priority value from the queue.
	/// If queue is empty, returns the default value.
	/// </summary>
	public T Dequeue()
	{
		if (Empty)
			return default(T);

		var v = first;
		first = v.next;
		return v.value;
	}
}
