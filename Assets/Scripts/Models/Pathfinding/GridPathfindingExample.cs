using Models.Ai.Pathfinding;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GridPathfindingExample : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private List<NodeGO> nodesGo;
    [Space]
    private NodeGO startNodeGO;
    private NodeGO goalNodeGO;

    private DStarLite<Vector2> dStar;
    private List<Node<Vector2>> nodes = new List<Node<Vector2>>();

    public void Start()
    {
        _ = CalculateNewRoute();
    }

    private async Task CalculateNewRoute()
    {
        for (int j = 0; j < 0; j++)
        {
            foreach (var node in nodesGo)
                node.GetComponent<MeshRenderer>().material.color = Color.white;

            do
            {
                startNodeGO = nodesGo[Random.Range(0, nodesGo.Count)];
                goalNodeGO = nodesGo[Random.Range(0, nodesGo.Count)];
            }
            while (startNodeGO == goalNodeGO);

            startNodeGO.GetComponent<MeshRenderer>().material.color = Color.green;
            goalNodeGO.GetComponent<MeshRenderer>().material.color = Color.red;

            nodes.Clear();

            foreach (var nGo in nodesGo)
            {
                var pos = new Vector2(nGo.transform.position.x, nGo.transform.position.z);
                var node = new Node<Vector2>(
                    pos,
                    (a, b) => Vector2.Distance(a.Data, b.Data),
                    (a, b) => Vector2.Distance(a.Data, b.Data)
                );

                nodes.Add(node);
                nGo.Node = node;
            }

            for (int i = 0; i < nodesGo.Count; i++)
            {
                var currentNode = nodes[i];
                currentNode.Neighbors = nodesGo[i]
                    .GetNeighbours()
                    .Select(nGo => nGo.Node)
                    .ToList();
            }

            var startNode = startNodeGO.Node;
            var goalNode = goalNodeGO.Node;

            //dStar = new DStarLite<Vector2>(startNode, goalNode, nodes);
            //dStar.Initialize();

            var path = dStar.GetPath(startNode, goalNode);
            path.Add(goalNode);

            line.positionCount = path.Count;
            line.SetPositions(path
                .ConvertAll(p => new Vector3(p.Data.x, 0.1f, p.Data.y))
                .ToArray());

            await Task.Delay(2000);
        }
    }
}
