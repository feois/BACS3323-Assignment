using System;
using static System.Single;
using System.Collections.Generic;

namespace DStarLite {
	public class Waypoint: IEquatable<Waypoint> {
		private readonly Graph<Waypoint> graph;
		
		public float cost { get; internal set; } = PositiveInfinity;
		public float lookahead { get; internal set; } = PositiveInfinity;
		public bool isObstacle { get; internal set; }
		
		internal readonly ISet<Waypoint> predecessors = new HashSet<Waypoint>();
		internal readonly Dictionary<Waypoint, float> successors = new();
		
		public Waypoint(Graph<Waypoint> graph) => this.graph = graph;

		public bool HasEdge(Waypoint waypoint) => successors.ContainsKey(waypoint);
		public float GetEdgeCost(Waypoint waypoint) => successors[waypoint];
		public bool TryGetEdgeCost(Waypoint waypoint, out float edgeCost) => successors.TryGetValue(waypoint, out edgeCost);
		
		public virtual void SetEdge(Waypoint waypoint, float edgeCost) {
			var oldCost = successors[waypoint];
			successors[waypoint] = edgeCost;
			graph.UpdateEdge(this, waypoint, oldCost, edgeCost);
			waypoint.predecessors.Add(this);
		}
		
		// should be avoided since it's expensive
		// set edge to infinity instead
		public void RemoveEdge(Waypoint waypoint) {
			SetEdge(waypoint, PositiveInfinity);
			graph.RecalculatePath();
			successors.Remove(waypoint);
		}

		public void SetAsObstacle() => graph.SetObstacle(this);
		public void UnsetAsObstacle() => graph.UnsetObstacle(this);
		public void ToggleAsObstacle() => graph.ToggleObstacle(this);
		
		public bool Equals(Waypoint other) => this == other;
	}
}
