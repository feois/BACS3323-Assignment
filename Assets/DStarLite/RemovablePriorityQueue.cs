using System;
using System.Collections.Generic;
using System.Linq;

namespace DStarLite {
	public class RemovablePriorityQueue<T, P> where P: IComparable<P> where T: IEquatable<T> {
		private class RemovableList<U> where U: IEquatable<U> {
			private readonly HashSet<U> list = new();
			private bool peeked;
			private U peekValue;

			public bool isEmpty => list.Count == 0;
			public void Add(U u) => list.Add(u);
			public U Peek() {
				if (peeked) return peekValue;
				peeked = true;
				return peekValue = list.First();
			}
			public void Remove(U u) {
				if (peeked && u.Equals(peekValue)) peeked = false;
				list.Remove(u);
			}
		}
		
		private readonly SortedDictionary<P, RemovableList<T>> queue = new();
		private readonly Dictionary<T, P> dict = new();
		
		public int count => dict.Count;
		
		public void Clear() {
			queue.Clear();
			dict.Clear();
		}
		
		public bool Has(T t) => dict.ContainsKey(t);
		public P Get(T t) => dict[t];
		
		public void Enqueue(T t, P p) {
			Remove(t);
			dict.Add(t, p);
			queue.TryAdd(p, new RemovableList<T>());
			queue[p].Add(t);
		}
		
		public (T, P) Peek() {
			var (p, t) = queue.First();
			
			return (t.Peek(), p);
		}
		
		public bool TryPeek(out T t, out P p) {
			if (count > 0) {
				(t, p) = Peek();
				return true;
			}
			t = default;
			p = default;
			return false;
		}
		
		public (T, P) Dequeue() {
			var (t, p) = Peek();
			
			Remove(t);
			
			return (t, p);
		}
		
		public bool TryDequeue(out T t, out P p) {
			if (count > 0) {
				(t, p) = Dequeue();
				return true;
			}
			t = default;
			p = default;
			return false;
		}
		
		public void Remove(T t) {
			if (!dict.Remove(t, out var p)) return;
			queue[p].Remove(t);
			if (queue[p].isEmpty) queue.Remove(p);
		}
	}
}