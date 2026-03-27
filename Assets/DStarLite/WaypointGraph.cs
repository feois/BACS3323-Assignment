using static System.Single;
using System.Collections.Generic;
using System.Linq;

namespace DStarLite {
    public abstract class WaypointGraph: Graph<Waypoint> {
        protected readonly ISet<Waypoint> waypoints = new HashSet<Waypoint>();

        public virtual Waypoint AddWaypoint() {
            var waypoint = new Waypoint(this);
            waypoints.Add(waypoint);
            return waypoint;
        }

        public override IEnumerable<Waypoint> Nodes() => waypoints;
        public override float Cost(Waypoint node) => node.cost;
        protected override void SetCost(Waypoint node, float cost) => node.cost = cost;
        public override float Lookahead(Waypoint node) => node.lookahead;
        protected override void SetLookahead(Waypoint node, float lookahead) => node.lookahead = lookahead;
        public override IEnumerable<Waypoint> Predecessors(Waypoint node) => node.predecessors;
        public override IEnumerable<Waypoint> Successors(Waypoint node) => node.successors.Keys;
        public override IEnumerable<(Waypoint, float)> SuccessorsWithCost(Waypoint node) =>
            node.successors.Select(successor => (successor.Key, successor.Value));
        public override float EdgeCost(Waypoint from, Waypoint to) => from.successors.GetValueOrDefault(to, PositiveInfinity);
        public override bool IsObstacle(Waypoint node) => node.isObstacle;
        protected override void SetObstacle(Waypoint node, bool state) => node.isObstacle = state;
    }
}