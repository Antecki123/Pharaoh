using App.Configs;
using Models.Ai;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Views.Road
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoadView : MonoBehaviour
    {
        [Inject] private NavigationGraph navigationGraph;
        [Inject] private ConstructionConfig constructionConfig;

        private Vector3 startPos;
        private Vector3 endPos;

        public void Init(Vector3 startPos, Vector3 endPos)
        {
            this.startPos = startPos;
            this.endPos = endPos;

            GenerateMesh(startPos, endPos);

            var roadIntersectionGenerator = new RoadIntersectionGenerator(navigationGraph, constructionConfig);
            roadIntersectionGenerator.GenerateIntersection(startPos);
            roadIntersectionGenerator.GenerateIntersection(endPos);
        }

        private void OnDrawGizmos()
        {
            Handles.matrix = transform.localToWorldMatrix;
            Handles.SphereHandleCap(0, startPos, Quaternion.identity, .1f, EventType.Repaint);
            Handles.SphereHandleCap(1, endPos, Quaternion.identity, .1f, EventType.Repaint);
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.IsPlaying(gameObject))
                return;

            var startNode = navigationGraph.GetNode(startPos);
            var endNode = navigationGraph.GetNode(endPos);

            foreach (var nb in startNode.Neighbors)
            {
                if (nb == endNode)
                    continue;

                var dir = (nb.Data - startNode.Data).normalized;
                Handles.ArrowHandleCap(GUIUtility.GetControlID(FocusType.Passive), startNode.Data, Quaternion.LookRotation(dir, Vector3.up), 1f, EventType.Repaint);
            }

            foreach (var nb in endNode.Neighbors)
            {
                if (nb == startNode)
                    continue;

                var dir = (nb.Data - endNode.Data).normalized;
                Handles.ArrowHandleCap(GUIUtility.GetControlID(FocusType.Passive), endNode.Data, Quaternion.LookRotation(dir, Vector3.up), 1f, EventType.Repaint);
            }
        }

        private void GenerateMesh(Vector3 startPos, Vector3 endPos)
        {
            var mesh = new Mesh();
            var direction = (endPos - startPos).normalized;
            var flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;
            var right = Vector3.Cross(Vector3.up, flatDirection).normalized * (constructionConfig.RoadWidth / 2f);

            var v0 = startPos - right;
            var v1 = startPos + right;
            var v2 = endPos - right;
            var v3 = endPos + right;

            v0 = transform.InverseTransformPoint(v0);
            v1 = transform.InverseTransformPoint(v1);
            v2 = transform.InverseTransformPoint(v2);
            v3 = transform.InverseTransformPoint(v3);

            mesh.vertices = new Vector3[] { v0, v1, v2, v3 };

            mesh.triangles = new int[]
            {
                0, 2, 1,
                2, 3, 1
            };

            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            mesh.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = Color.gray
            };
        }
    }
}