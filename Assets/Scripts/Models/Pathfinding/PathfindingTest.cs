using App.Signals;
using Cysharp.Threading.Tasks;
using Models.Ai;
using Models.Ai.Pathfinding;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;

public class PathfindingTest : MonoBehaviour
{
    [SerializeField, Min(0)] private int sampleCount = 1;
    [SerializeField, Min(1)] private int delayTimeSec = 1;
    [SerializeField] private LineRenderer line;

    [Inject] private NavigationGraph navigationGraph;
    [Inject] private SignalBus signalBus;

    private Node<Vector3> startNode;
    private Node<Vector3> goalNode;

    private DStarLite<Vector3> dStar;

    [ContextMenu("StartTest")]
    public void StartTest()
    {
        _ = CalculateNewRoute();
    }

    [ContextMenu("SpawnSettlers")]
    public void SpawnSettlers()
    {
        for (int i = 0; i < sampleCount; i++)
        {
            signalBus.Fire(new SettlersSignals.SpawnSettler(Vector3.zero, Quaternion.identity));
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!EditorApplication.isPlaying)
            return;

        foreach (var node in navigationGraph.Nodes)
        {
            if (node.NodeType == NodeType.Terrain)
            {
                Handles.color = Color.yellow;
                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), node.Data, Quaternion.identity, .25f, EventType.Repaint);
            }

            else if (node.NodeType == NodeType.Road)
            {
                Handles.color = Color.black;
                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), node.Data, Quaternion.identity, .5f, EventType.Repaint);
            }

            else if (node.NodeType == NodeType.Building)
            {
                Handles.color = Color.red;
                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), node.Data, Quaternion.identity, .25f, EventType.Repaint);
            }

            Handles.color = Color.white;
        }
    }

    private async UniTask CalculateNewRoute()
    {
        var nodesList = navigationGraph.Nodes.ToList();

        for (int i = 0; i < sampleCount; i++)
        {
            do
            {
                startNode = nodesList[Random.Range(0, nodesList.Count)];
                goalNode = nodesList[Random.Range(0, nodesList.Count)];
            }
            while (startNode == goalNode);

            dStar = new DStarLite<Vector3>();
            dStar.Initialize(nodesList, startNode, goalNode);

            var path = dStar.GetPath();

            Debug.LogError("C " + path.Count);

            if (path.Count > 0)
            {
                line.positionCount = path.Count;
                line.SetPositions(path
                    .ConvertAll(p => new Vector3(p.Data.x, 0.1f, p.Data.z))
                    .ToArray());
            }
            else
            {
                Debug.LogWarning($"[D*Lite] Cannot find a path between {startNode.Data} and {goalNode.Data}.");
            }

            await UniTask.WaitForSeconds(delayTimeSec);
        }
    }
}