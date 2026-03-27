using System;
using static System.Single;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DStarLite {
    public class Vector3GridGraph : Graph<Vector3Int> {
        public readonly Vector3Int size;
        public readonly Vector3 origin, step;
        public readonly bool diagonal;
        private readonly float[,,] costs, lookaheads;
        private readonly bool[,,] obstacles;

        private static readonly float SQRT2 = (float) Math.Sqrt(2);
        private static readonly float SQRT3 = (float) Math.Sqrt(3);

        public Vector3GridGraph(Vector3 origin, Vector3 step, Vector3Int size, bool diagonal = false) {
            if (size.x <= 0 || size.y <= 0 || size.z <= 0) throw new ArgumentOutOfRangeException("size must be non-zero positive");

            this.size = size;
            this.origin = origin;
            this.step = step;
            this.diagonal = diagonal;
            costs = new float[size.x, size.y, size.z];
            lookaheads = new float[size.x, size.y, size.z];
            obstacles = new bool[size.x, size.y, size.z];
        }

        public Vector3Int? ToIndex(Vector3 position) {
            position -= origin;

            var x = (int)Math.Round(position.x / step.x);
            var y = (int)Math.Round(position.y / step.y);
            var z = (int)Math.Round(position.z / step.z);
            
            if (x >= 0 && x < size.x && y >= 0 && y < size.y && z >= 0 && z < size.z) return new Vector3Int(x, y, z);

            return null;
        }

        public Vector3 ToPosition(Vector3Int index) {
            var x = index.x * step.x;
            var y = index.y * step.y;
            var z = index.z * step.z;

            return origin + new Vector3(x, y, z);
        }

        private static void FillArray(float[,,] array, float value) =>
            MemoryMarshal.CreateSpan(ref array[0, 0, 0], array.Length).Fill(value);

        public bool Validate(Vector3Int index) => index.x >= 0 && index.x < size.x
                                               && index.y >= 0 && index.y < size.y
                                               && index.z >= 0 && index.z < size.z;

        public virtual IEnumerable<Vector3Int> Neighbors(Vector3Int index) {
            if (diagonal) {
                for (var x = -1; x < 2; x++)
                for (var y = -1; y < 2; y++)
                for (var z = -1; z < 2; z++)
                    if (x != 0 || y != 0 || z != 0)
                        yield return index + new Vector3Int(x, y, z);
            }
            else {
                int x0 = index.x - 1, x1 = index.x, x2 = index.x + 1,
                    y0 = index.y - 1, y1 = index.y, y2 = index.x + 1,
                    z0 = index.z - 1, z1 = index.z, z2 = index.z + 1;
                
                yield return new Vector3Int(x0, y1, z1);
                yield return new Vector3Int(x2, y1, z1);
                yield return new Vector3Int(x1, y0, z1);
                yield return new Vector3Int(x1, y2, z1);
                yield return new Vector3Int(x1, y1, z0);
                yield return new Vector3Int(x1, y1, z2);
            }
        }

        public virtual IEnumerable<Vector3Int> ValidNeighbors(Vector3Int index) => Neighbors(index).Where(Validate);

        private static float VectorLength(Vector3Int n) =>
        ((n.x == -1 ? 1 : n.x) + (n.y == -1 ? 1 : n.y) + (n.z == -1 ? 1 : n.z)) switch {
            0 => 0,
            1 => 1,
            2 => SQRT2,
            3 => SQRT3,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        public void SearchPathPosition(Vector3 start, Vector3 goal) => SearchPath(ToIndex(start)!.Value, ToIndex(goal)!.Value);

        public bool PeekPosition(out Vector3 position) {
            if (Peek(out var index)) {
                position = ToPosition(index);
                return true;
            }

            position = default;
            return false;
        }

        public bool NextPosition(out Vector3 position) {
            if (Next(out var index)) {
                position = ToPosition(index);
                return true;
            }

            position = default;
            return false;
        }
        
        protected override void Reset() {
            FillArray(costs, PositiveInfinity);
            FillArray(lookaheads, PositiveInfinity);
        }

        public override IEnumerable<Vector3Int> Nodes() {
            for (var x = 0; x < size.x; x++)
                for (var y = 0; y < size.y; y++)
                    for (var z = 0; z < size.z; z++)
                        yield return new Vector3Int(x, y, z);
        }
        public override float Cost(Vector3Int node) => costs[node.x, node.y, node.z];
        protected override void SetCost(Vector3Int node, float cost) => costs[node.x, node.y, node.z] = cost;
        public override float Lookahead(Vector3Int node) => lookaheads[node.x, node.y, node.z];
        protected override void SetLookahead(Vector3Int node, float lookahead) => lookaheads[node.x, node.y, node.z] = lookahead;
        public override float Heuristic(Vector3Int start, Vector3Int end) => Vector3Int.Distance(start, end);
        public override IEnumerable<Vector3Int> Predecessors(Vector3Int node) => ValidNeighbors(node);
        public override IEnumerable<Vector3Int> Successors(Vector3Int node) => ValidNeighbors(node);
        public override float EdgeCost(Vector3Int from, Vector3Int to) => VectorLength(to - from);
        public override bool IsObstacle(Vector3Int node) => obstacles[node.x, node.y, node.z];
        protected override void SetObstacle(Vector3Int node, bool state) => obstacles[node.x, node.y, node.z] = state;
    }
}