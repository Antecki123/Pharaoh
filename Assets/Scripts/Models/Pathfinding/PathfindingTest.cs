using Cysharp.Threading.Tasks;
using Models.Ai;
using Models.Ai.Pathfinding;
using System.Collections.Generic;
using UnityEngine;
using Views.Road;
using Zenject;

public class PathfindingTest : MonoBehaviour
{
    [SerializeField, Min(0)] private int sampleCount = 1;
    [SerializeField, Min(1)] private int delayTimeSec = 1;
    [SerializeField] private LineRenderer line;
    [Inject] private NavigationGraph navigationGraph;

    private Node<RoadNode> startNode;
    private Node<RoadNode> goalNode;

    private DStarLite<RoadNode> dStar;
    private List<Node<RoadNode>> nodes = new List<Node<RoadNode>>();

    [ContextMenu("StartTest")]
    public void StartTest()
    {
        _ = CalculateNewRoute();
    }

    private async UniTask CalculateNewRoute()
    {
        for (int j = 0; j < sampleCount; j++)
        {
            var map = new Dictionary<RoadNode, Node<RoadNode>>();

            nodes.Clear();

            foreach (var roadNode in navigationGraph.Nodes)
            {
                var node = new Node<RoadNode>(
                    roadNode,
                    (a, b) => Vector2.Distance(a.Data.Position, b.Data.Position),
                    (a, b) => Vector2.Distance(a.Data.Position, b.Data.Position));

                nodes.Add(node);
                map[roadNode] = node;
            }

            foreach (var roadNode in navigationGraph.Nodes)
            {
                if (roadNode.Neighbors == null) continue;

                var node = map[roadNode];
                foreach (var neighborRoadNode in roadNode.Neighbors)
                {
                    if (map.TryGetValue(neighborRoadNode, out var neighborNode))
                    {
                        if (!node.Neighbors.Contains(neighborNode))
                            node.Neighbors.Add(neighborNode);
                    }
                }
            }

            do
            {
                startNode = nodes[Random.Range(0, nodes.Count)];
                goalNode = nodes[Random.Range(0, nodes.Count)];
            }
            while (startNode == goalNode);

            dStar = new DStarLite<RoadNode>();
            dStar.Initialize(nodes, startNode, goalNode);

            var path = dStar.GetPath();

            if (path.Count == 0)
                Debug.LogWarning($"[D*Lite] Cannot find a path between {startNode.Data} and {goalNode.Data}.");

            line.positionCount = path.Count;
            line.SetPositions(path
                .ConvertAll(p => new Vector3(p.Data.Position.x, 0.1f, p.Data.Position.z))
                .ToArray());

            await UniTask.WaitForSeconds(delayTimeSec);
        }
    }
}