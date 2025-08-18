using App.Signals;
using UnityEngine;
using Zenject;

namespace Views.Road
{
    public class RoadView : MonoBehaviour
    {
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] private GameObject roadNodePrefab;
        [SerializeField] private float nodeSpacing = 2f;

        [Inject] private SignalBus signalBus;

        bool buildingModeOn;
        readonly float resolution = .2f;

        Vector3? startPosition;
        Vector3? endPosition;

        private void Start()
        {
            lineRenderer.positionCount = 0;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                buildingModeOn = !buildingModeOn;
                signalBus.Fire(new ConstructionSignals.ConstructionMode(buildingModeOn));
            }

            if (Input.GetMouseButtonDown(0) && buildingModeOn)
            {
                if (TryGetMouseWorldPosition(out Vector3 worldPos))
                {
                    if (startPosition == null)
                    {
                        startPosition = worldPos;
                        lineRenderer.positionCount = 2;
                        lineRenderer.SetPosition(0, worldPos);
                        lineRenderer.SetPosition(1, worldPos);
                    }
                    else if (endPosition == null)
                    {
                        endPosition = worldPos;
                        lineRenderer.SetPosition(1, worldPos);

                        GenerateNodes((Vector3)startPosition, (Vector3)endPosition);
                    }
                    else
                    {
                        startPosition = worldPos;
                        endPosition = null;
                        lineRenderer.positionCount = 2;
                        lineRenderer.SetPosition(0, worldPos);
                        lineRenderer.SetPosition(1, worldPos);
                    }
                }
            }

            if (startPosition != null && endPosition == null)
            {
                if (TryGetMouseWorldPosition(out Vector3 previewPos))
                {
                    lineRenderer.SetPosition(1, previewPos);
                }
            }
        }

        private bool TryGetMouseWorldPosition(out Vector3 worldPos)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var posX = Mathf.Round(hit.point.x / resolution) * resolution;
                var posY = .1f;
                var posZ = Mathf.Round(hit.point.z / resolution) * resolution;
                worldPos = new Vector3(posX, posY, posZ);

                return true;
            }

            worldPos = Vector3.zero;
            return false;
        }

        private void GenerateNodes(Vector3 start, Vector3 end)
        {
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            int nodeCount = Mathf.FloorToInt(distance / nodeSpacing);

            Vector3 previousPos = start;
            GameObject previousNode = null;

            for (int i = 0; i <= nodeCount; i++)
            {
                Vector3 nodePos = start + direction * (i * nodeSpacing);
                if (i == nodeCount) nodePos = end;

                var nodeObj = Instantiate(roadNodePrefab, nodePos, Quaternion.identity);
                nodeObj.transform.SetParent(transform, false);
                nodeObj.SetActive(true);
                var node = nodeObj.GetComponent<RoadNode>();

                if (previousNode != null)
                {
                    var prevNode = previousNode.GetComponent<RoadNode>();
                    prevNode.Neighbors.Add(node);
                    node.Neighbors.Add(prevNode);
                }

                previousNode = nodeObj;
            }
        }
    }
}