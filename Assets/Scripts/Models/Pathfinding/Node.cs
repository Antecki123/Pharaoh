using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models.Ai.Pathfinding
{
    public static class NumberExtensions
    {
        public static bool Approx(this float f1, float f2)
        {
            return Mathf.Approximately(f1, f2);
        }
    }

    public class Node<T>
    {
        public T Data { get; set; }

        public Func<Node<T>, Node<T>, float> Cost { get; set; }

        public Func<Node<T>, Node<T>, float> Heuristic { get; }

        public float G { get; set; }
        public float RHS { get; set; }
        public bool GEqualRHS => G.Approx(RHS);

        public List<Node<T>> Neighbors { get; set; } = new();

        public Node(T data, Func<Node<T>, Node<T>, float> cost, Func<Node<T>, Node<T>, float> heuristic)
        {
            Data = data;
            Cost = cost;
            Heuristic = heuristic;

            G = float.MaxValue;
            RHS = float.MaxValue;
        }
    }

    public readonly struct Key
    {
        readonly float k1;
        readonly float k2;

        public Key(float k1, float k2)
        {
            this.k1 = k1;
            this.k2 = k2;
        }

        public static bool operator <(Key a, Key b) => a.k1 < b.k1 || a.k1.Approx(b.k1) && a.k2 < b.k2;
        public static bool operator >(Key a, Key b) => a.k1 > b.k1 || a.k1.Approx(b.k1) && a.k2 > b.k2;
        public static bool operator ==(Key a, Key b) => a.k1.Approx(b.k1) && a.k2.Approx(b.k2);
        public static bool operator !=(Key a, Key b) => !(a == b);

        public override bool Equals(object obj) => obj is Key key && this == key;
        public override int GetHashCode() => HashCode.Combine(k1, k2);
        public override string ToString() => $"({k1}, {k2})";
    }
}