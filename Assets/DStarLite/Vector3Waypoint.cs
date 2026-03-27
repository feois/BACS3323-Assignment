using System;
using UnityEngine;

namespace DStarLite {
    public class Vector3Waypoint: Waypoint {
        public readonly Vector3 position;
        
        public Vector3Waypoint(Graph<Waypoint> graph, Vector3 position) : base(graph) => this.position = position;

        public override void SetEdge(Waypoint waypoint, float edgeCost) {
            if (waypoint is Vector3Waypoint v) SetEdge(v, Vector3.Distance(position, v.position) + edgeCost);
            else throw new ArgumentException("waypoint must be Vector3Waypoint");
        }

        public void SetEdge(Waypoint waypoint) => SetEdge(waypoint, 0);
    }
}