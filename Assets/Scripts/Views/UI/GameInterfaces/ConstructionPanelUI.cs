using App.Signals;
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
        [SerializeField] private Button farmingButton;
        [SerializeField] private Button industryButton;
        [SerializeField] private Button leasureButton;
        [SerializeField] private Button municipalButton;
        [SerializeField] private Button decoratesButton;
        [Space]
        [SerializeField] private RectTransform buildingButtonsContainer;
        [SerializeField] private BuildingButtonUI buildingButtonPrefab;

        [Inject] private readonly SignalBus signalBus;

        private BuildingType? currentOpenPanel = null;
        private List<BuildingButtonUI> activeButtons = new List<BuildingButtonUI>();

        private void OnEnable()
        {
            roadsButton.onClick.AddListener(() =>
            {
                ClearTabs();
                availableBuildingsPanel.gameObject.SetActive(false);
                signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.Road));
            });

            housingButton.onClick.AddListener(() => OpenPanel(BuildingType.Housing));
            farmingButton.onClick.AddListener(() => OpenPanel(BuildingType.Farming));
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
            farmingButton.onClick.RemoveAllListeners();
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
                    case BuildingType.Housing:
                        buildings = new List<BuildingDefinition>()
                        {
                            BuildingDefinition.Cottage,
                            BuildingDefinition.House,
                            BuildingDefinition.Residence
                        };
                        break;

                    case BuildingType.Farming:
                        buildings = new List<BuildingDefinition>()
                        {
                            BuildingDefinition.WheatFarm,
                            BuildingDefinition.LinenFarm,
                            BuildingDefinition.Pasture
                        };
                        break;

                    case BuildingType.Industry:
                        buildings = new List<BuildingDefinition>()
                        {
                            BuildingDefinition.Windmill,
                            BuildingDefinition.Bakery,
                            BuildingDefinition.Brewery,
                            BuildingDefinition.WeavingMill,
                            BuildingDefinition.Granary,
                            BuildingDefinition.Warehouse
                        };
                        break;

                    case BuildingType.Leasure:
                        break;

                    case BuildingType.Municipal:
                        buildings = new List<BuildingDefinition>()
                        {
                            BuildingDefinition.Well,
                            BuildingDefinition.Bazaar,
                            BuildingDefinition.Tavern,
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

            signalBus.Fire(new ConstructionSignals.ConstructionMode(BuildingDefinition.None));
        }

        private void LoadAvailableBuldings(List<BuildingDefinition> buildings)
        {
            ClearTabs();

            foreach (var building in buildings)
            {
                var buildingButton = Instantiate(buildingButtonPrefab);
                buildingButton.transform.SetParent(buildingButtonsContainer);
                buildingButton.InitializeButton(signalBus, building);

                activeButtons.Add(buildingButton);
            }
        }

        private void ClearTabs()
        {
            foreach (var button in activeButtons)
                Destroy(button.gameObject);

            activeButtons.Clear();
        }
    }

    public enum BuildingType
    {
        Housing,
        Farming,
        Industry,
        Leasure,
        Municipal,
        Decorates
    }
}