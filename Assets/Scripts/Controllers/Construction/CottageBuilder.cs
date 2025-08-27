using UnityEngine;

namespace Controllers.Construction
{
    public class CottageBuilder : IConstruction
    {
        public bool RoadConnectionRequired => true;

        private GameObject cottageOverview;
        readonly private float resolution = .2f;

        public CottageBuilder()
        {
            cottageOverview = Resources.Load<GameObject>("Prefabs/cottageOverview");
        }

        public void Tick()
        {

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
    }
}