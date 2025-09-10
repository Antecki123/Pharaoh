using Models.Economy;
using System.Collections.Generic;
using UnityEngine;
using Views.Settler;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class CottageView : BuildingView, IHabitation
    {
        [SerializeField] private Transform entrance;

        [Inject] private HabitationModel habitationModel;

        private List<SettlerView> residents = new List<SettlerView>();
        private int maxResidents = 6;

        public override void PlaceBuilding()
        {
            habitationModel.AddHabitation(this);
        }

        public override void DestroyBuilding()
        {
            habitationModel.RemoveHabitation(this);
        }

        public int GetMaxResidents()
        {
            return maxResidents;
        }

        public IEnumerable<SettlerView> GetResidents()
        {
            return residents;
        }
    }
}