using System;
using static System.Single;
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DStarLite {
	public abstract class Graph<Node> where Node: IEquatable<Node> {
		private readonly struct NodeKey: IComparable<NodeKey> {
			private readonly float total, cost;
			
			public NodeKey(float cost, float lookahead, float heuristic, float offset) {
				this.cost = Min(cost, lookahead);
				total = this.cost + heuristic + offset;
			}
			
			public int CompareTo(NodeKey other) => (total, cost).CompareTo((other.total, other.cost));
			
			public static bool operator <(NodeKey l, NodeKey r) => l.CompareTo(r) < 0;
			public static bool operator >(NodeKey l, NodeKey r) => l.CompareTo(r) > 0;
			public static bool operator <=(NodeKey l, NodeKey r) => l.CompareTo(r) <= 0;
			public static bool operator >=(NodeKey l, NodeKey r) => l.CompareTo(r) >= 0;
		}
		
		private readonly RemovablePriorityQueue<Node, NodeKey> openList = new();
		public bool obstacleEnabled { get; private set; } = true;
		
		internal float offset;
		private bool updated;

		public bool searching { get; private set; }
		public Node startNode { get; private set; }
		public Node currentNode { get; private set;  }
		public Node goalNode { get; private set; }
		private Node offsetNode;
		
		// can be overriden for optimization, must set all nodes' cost and lookahead to infinity 
		protected virtual void Reset() {
			foreach (var node in Nodes()) {
				SetCost(node, PositiveInfinity);
				SetLookahead(node, PositiveInfinity);
			}
		}
		
		// must be implemented
		public abstract IEnumerable<Node> Nodes(); // may not be necessary if Reset is overriden
		public abstract float Cost(Node node);
		protected abstract void SetCost(Node node, float cost);
		public abstract float Lookahead(Node node);
		protected abstract void SetLookahead(Node node, float lookahead);
		public abstract float Heuristic(Node start, Node end);
		public abstract IEnumerable<Node> Predecessors(Node node);
		public abstract IEnumerable<Node> Successors(Node node);
		public abstract float EdgeCost(Node from, Node to);
		public abstract bool IsObstacle(Node node);
		protected abstract void SetObstacle(Node node, bool state);
		
		// can be overriden for optimization
		public virtual IEnumerable<(Node, float)> PredecessorsWithCost(Node node) =>
			Predecessors(node).Select(predecessor => (predecessor, EdgeCost(predecessor, node)));
		public virtual IEnumerable<(Node, float)> SuccessorsWithCost(Node node) =>
			Successors(node).Select(successor => (successor, EdgeCost(node, successor)));

		public IEnumerable<(Node, float)> PredecessorsWithCostAndObstacle(Node node) {
			var obstacle = obstacleEnabled && IsObstacle(node);
			
			foreach (var (predecessor, cost) in PredecessorsWithCost(node))
				yield return (predecessor, obstacleEnabled && (obstacle || IsObstacle(predecessor)) ? PositiveInfinity : cost);
		}
		
		public IEnumerable<(Node, float)> SuccessorsWithCostAndObstacle(Node node) {
			var obstacle = obstacleEnabled && IsObstacle(node);

			foreach (var (successor, cost) in SuccessorsWithCost(node))
				yield return (successor, obstacleEnabled && (obstacle || IsObstacle(successor)) ? PositiveInfinity : cost);
		}

		public float EdgeCostWithObstacle(Node from, Node to) =>
			obstacleEnabled && (IsObstacle(from) || IsObstacle(to)) ? PositiveInfinity : EdgeCost(from, to);
		
		private NodeKey Key(Node node) => new(Cost(node), Lookahead(node), Heuristic(currentNode, node), offset);
		
		private (Node, float) MinSuccessorWithLookahead(Node node) {
			Node successor = default;
			var lookahead = PositiveInfinity;

			foreach (var (s, c) in SuccessorsWithCostAndObstacle(node)) {
				var l = c + Cost(s);

				if (l >= lookahead) continue;
				
				successor = s;
				lookahead = l;
			}

			return (successor, lookahead);
		}
		
		private void UpdateNode(Node node) {
			if (Cost(node).Equals(Lookahead(node))) openList.Remove(node);
			else openList.Enqueue(node, Key(node));
		}

		private void UpdateLookahead(Node node) {
			if (node.Equals(goalNode)) return;
			var (_, lookahead) = MinSuccessorWithLookahead(node);
			SetLookahead(node, lookahead);
		}

		private void UpdateLookahead(Node node, float lookahead) {
			if (node.Equals(goalNode)) return;
			if (lookahead < Lookahead(node))
				SetLookahead(node, lookahead);
		}

		// MUST BE CALLED WHEN EDGE COST CHANGES
		// DOES NOT CHANGE EDGE COST, EDGE COST MUST BE UPDATED BEFORE CALLING THIS
		public void UpdateEdge(Node from, Node to, float oldCost, float newCost) {
			if (!searching) return;

			if (!updated) {
				updated = true;
				offset += Heuristic(offsetNode, currentNode);
				offsetNode = currentNode;
			}

			if (oldCost > newCost) UpdateLookahead(from, newCost + Cost(to));
			else if (Mathf.Approximately(Lookahead(from), oldCost + Cost(to))) UpdateLookahead(from);
			
			UpdateNode(from);
		}
		
		private void CalculatePath() {
			while (openList.TryPeek(out var node, out var key) && (key < Key(currentNode) || !Lookahead(currentNode).Equals(Cost(currentNode)))) {
				if (key < Key(node)) openList.Enqueue(node, Key(node)); // stale key
				else if (Cost(node) > Lookahead(node)) {
					SetCost(node, Lookahead(node));
					openList.Dequeue();
					foreach (var (predecessor, cost) in PredecessorsWithCostAndObstacle(node)) {
						UpdateLookahead(predecessor, cost + Cost(node));
						UpdateNode(predecessor);
					}
				}
				else {
					var oldCost = Cost(node);
					SetCost(node, PositiveInfinity);
					foreach (var (predecessor, cost) in PredecessorsWithCostAndObstacle(node).Append((node, 0))) {
						if (Mathf.Approximately(Lookahead(predecessor), cost + oldCost))
							UpdateLookahead(predecessor);
						UpdateNode(predecessor);
					}
				}
			}
		}

		public void RecalculatePath() => CalculatePath();
		
		public void SearchPath(Node start, Node goal) {
			searching = true;
			updated = false;
			startNode = currentNode = offsetNode = start;
			goalNode = goal;
			
			Reset();
			SetLookahead(goal, 0);
			
			offset = 0;
			openList.Clear();
			openList.Enqueue(goal, Key(goal));
			CalculatePath();
		}

		public bool Peek(out Node node) {
			if (searching) {
				if (updated) {
					RecalculatePath();
					updated = false;
				}

				if (!(currentNode.Equals(goalNode) || IsPositiveInfinity(Cost(currentNode)))) {
					(node, _) = MinSuccessorWithLookahead(currentNode);
					return true;
				}
			}

			node = default;
			return false;
		}

		public bool Next() => Next(out _);

		public bool Next(out Node node) {
			if (!Peek(out node)) return false;
			currentNode = node;
			return true;
		}

		// is not necessary but can improve performance by skipping unnecessary operations
		public void EndSearch() => searching = false;

		public void EnableObstacle() {
			if (obstacleEnabled) return;
			
			obstacleEnabled = true;
				
			if (searching) SearchPath(startNode, goalNode);
		}

		public void DisableObstacle() {
			if (!obstacleEnabled) return;

			obstacleEnabled = false;
			
			if (searching) SearchPath(startNode, goalNode);
		}

		public void SetObstacle(Node node) {
			if (IsObstacle(node)) return;
			
			SetObstacle(node, true);

			if (!searching) return;

			foreach (var (predecessor, cost) in PredecessorsWithCost(node)) 
				UpdateEdge(predecessor, node, cost, PositiveInfinity);
			
			foreach (var (successor, cost) in SuccessorsWithCost(node))
				UpdateEdge(node, successor, cost, PositiveInfinity);
		}
		
		public void UnsetObstacle(Node node) {
			if (!IsObstacle(node)) return;
			
			SetObstacle(node, false);

			if (!searching) return;
			
			foreach (var (predecessor, cost) in PredecessorsWithCost(node)) 
				UpdateEdge(predecessor, node, PositiveInfinity, cost);
			
			foreach (var (successor, cost) in SuccessorsWithCost(node))
				UpdateEdge(node, successor, PositiveInfinity, cost);
		}

		public void ToggleObstacle(Node node) {
			if (IsObstacle(node)) UnsetObstacle(node);
			else SetObstacle(node);
		}
	}
}
