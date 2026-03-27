using UnityEngine;

namespace DStarLite {
    public class Vector2GridGraph: Vector3GridGraph {
        public Vector2GridGraph(Vector3 origin, Vector2 step, Vector2Int size, bool diagonal = false)
            : base(origin, new Vector3(step.x, 1, step.y), new Vector3Int(size.x, 1, size.y), diagonal) {}
        
        public bool IsObstacle(Vector2Int index) => IsObstacle(new Vector3Int(index.x, 0, index.y));
        public void SetObstacle(Vector2Int index) => SetObstacle(new Vector3Int(index.x, 0, index.y));
        public void UnsetObstacle(Vector2Int index) => UnsetObstacle(new Vector3Int(index.x, 0, index.y));
        public void ToggleObstacle(Vector2Int index) => ToggleObstacle(new Vector3Int(index.x, 0, index.y));
        
        public float Cost(Vector2Int index) => Cost(new Vector3Int(index.x, 0, index.y));
        public float Lookahead(Vector2Int index) => Lookahead(new Vector3Int(index.x, 0, index.y));
    }
}