using Controllers.Construction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views.Ui.GameInterfaces
{
    public class ConstructionPanelUI : MonoBehaviour
    {
        [SerializeField] private RectTransform availableBuildingsPanel;
        [Space]
        [SerializeField] private Button roadsButton;
        [SerializeField] private Button housingButton;
        [SerializeField] private Button industryButton;
        [SerializeField] private Button leasureButton;
        [SerializeField] private Button municipalButton;
        [SerializeField] private Button decoratesButton;
        [Space]
        [SerializeField] private RectTransform buildingButtonsContainer;
        [SerializeField] private BuildingButtonUI buildingButtonPrefab;

        [Inject] private SignalBus signalBus;

        private BuildingType? currentOpenPanel = null;
        private List<BuildingButtonUI> activeBUttons = new List<BuildingButtonUI>();

        private void OnEnable()
        {
            roadsButton.onClick.AddListener(() => OpenPanel(BuildingType.Roads));
            housingButton.onClick.AddListener(() => OpenPanel(BuildingType.Housing));
            industryButton.onClick.AddListener(() => OpenPanel(BuildingType.Industry));
            leasureButton.onClick.AddListener(() => OpenPanel(BuildingType.Leasure));
            municipalButton.onClick.AddListener(() => OpenPanel(BuildingType.Municipal));
            decoratesButton.onClick.AddListener(() => OpenPanel(BuildingType.Decorates));

            availableBuildingsPanel.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            roadsButton.onClick.RemoveAllListeners();
            housingButton.onClick.RemoveAllListeners();
            industryButton.onClick.RemoveAllListeners();
            leasureButton.onClick.RemoveAllListeners();
            municipalButton.onClick.RemoveAllListeners();
            decoratesButton.onClick.RemoveAllListeners();
        }

        private void OpenPanel(BuildingType buildingType)
        {
            if (currentOpenPanel == null || currentOpenPanel != buildingType)
            {
                currentOpenPanel = buildingType;

                var buildings = new List<BuildingDefinition>();
                switch (buildingType)
                {
                    case BuildingType.Roads:
                        buildings = new List<BuildingDefinition>()
                        {
                            BuildingDefinition.Road
                        };
                        break;
                    case BuildingType.Housing:
                        buildings = new List<BuildingDefinition>()
                        {
                            BuildingDefinition.Cottage,
                            BuildingDefinition.House
                        };
                        break;
                    case BuildingType.Industry:
                        buildings = new List<BuildingDefinition>()
                        {
                            BuildingDefinition.Windmill,
                            BuildingDefinition.Bakery,
                            BuildingDefinition.Granary,
                            BuildingDefinition.Warehouse,
                            BuildingDefinition.WheatFarm,
                            BuildingDefinition.LinenFarm,
                            BuildingDefinition.Pasture
                        };
                        break;
                    case BuildingType.Leasure:
                        break;
                    case BuildingType.Municipal:
                        buildings = new List<BuildingDefinition>()
                        {
                            BuildingDefinition.Bazaar
                        };
                        break;
                    case BuildingType.Decorates:
                        break;
                    default:
                        break;
                }

                LoadAvailableBuldings(buildings);
                availableBuildingsPanel.gameObject.SetActive(true);
            }
            else if (currentOpenPanel == null || currentOpenPanel == buildingType)
            {
                currentOpenPanel = null;
                availableBuildingsPanel.gameObject.SetActive(false);
            }
        }

        private void LoadAvailableBuldings(List<BuildingDefinition> buildings)
        {
            foreach (var button in activeBUttons)
                Destroy(button.gameObject);
            activeBUttons.Clear();

            foreach (var building in buildings)
            {
                var buildingButton = Instantiate(buildingButtonPrefab);
                buildingButton.transform.SetParent(buildingButtonsContainer);
                buildingButton.InitializeButton(signalBus, building);

                activeBUttons.Add(buildingButton);
            }
        }
    }

    public enum BuildingType
    {
        Roads,
        Housing,
        Industry,
        Leasure,
        Municipal,
        Decorates
    }
}