using UnityEngine;

namespace DStarLite {
    public class Vector3WaypointGraph: WaypointGraph {
        public override Waypoint AddWaypoint() => null;

        public Vector3Waypoint AddWaypoint(Vector3 position) {
            var waypoint = new Vector3Waypoint(this, position);
            waypoints.Add(waypoint);
            return waypoint;
        }
        
        public override float Heuristic(Waypoint start, Waypoint end) {
            var s = start as Vector3Waypoint;
            var e = end as Vector3Waypoint;
            
            return Vector3.Distance(s!.position, e!.position);
        }
    }
}