using System.Collections.Generic;
using UnityEngine;

namespace Views.Construction
{
    [SelectionBase]
    public class FarmersHutView : BuildingView
    {
        private List<GameObject> crops = new List<GameObject>();
        private float range = 25f;

        public override void PlaceBuilding()
        {

        }
    }
}