using System.Collections.Generic;
using UnityEngine;

namespace Views.Construction
{
    public class FarmPreviewView : MonoBehaviour
    {
        private List<Vector3> verticles = new List<Vector3>();
        private LineRenderer line;

        private Material material;

        private void Awake()
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = Color.yellow };

            line = gameObject.AddComponent<LineRenderer>();
            line.material = material;
            line.loop = true;
        }

        public void AddVerticleView(Vector3 position)
        {
            verticles.Add(position);

            var farmVerticle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            farmVerticle.transform.position = position;
            farmVerticle.transform.localScale = new Vector3(.25f, .001f, .25f);
            farmVerticle.transform.SetParent(transform);
            farmVerticle.name = "FarmVerticle";
            farmVerticle.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        public void Clear()
        {
            verticles.Clear();
        }

        private void Update()
        {
            line.positionCount = verticles.Count;
            line.SetPositions(verticles.ToArray());
        }
    }
}