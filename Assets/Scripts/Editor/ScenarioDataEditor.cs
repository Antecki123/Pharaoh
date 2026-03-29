using Controllers.Construction;
using Models.Gameplay;
using System;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(ScenarioData))]
public class ScenarioDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var scenario = (ScenarioData)target;

        DrawDefaultInspector();

        var allBuildings = Enum.GetValues(typeof(BuildingDefinition))
                               .Cast<BuildingDefinition>();

        foreach (var building in allBuildings)
        {
            if(building == BuildingDefinition.None)
                continue;

            if (!scenario.AvailableBuildings.Any(x => x.buildingDefinition.Equals(building)))
            {
                scenario.AvailableBuildings.Add(new BuildingAvailibility
                {
                    buildingDefinition = building,
                    isAvailable = false
                });
            }
        }

        scenario.AvailableBuildings.RemoveAll(x =>
            !allBuildings.Contains(x.buildingDefinition));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Available Buildings", EditorStyles.boldLabel);

        foreach (var entry in scenario.AvailableBuildings)
        {
            entry.isAvailable = EditorGUILayout.Toggle(
                entry.buildingDefinition.ToString(),
                entry.isAvailable
            );
        }

        EditorUtility.SetDirty(scenario);
    }
}