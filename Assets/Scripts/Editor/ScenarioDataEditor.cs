using Controllers.Construction;
using Models.Gameplay;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScenarioData))]
public class ScenarioDataEditor : Editor
{
    private Type[] objectiveTypes;

    private void OnEnable()
    {
        objectiveTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(Objective)) && !t.IsAbstract)
            .ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var scenario = (ScenarioData)target;

        DrawPropertiesExcluding(serializedObject, "AvailableBuildings", "Objectives");

        // =======================
        //  AVAILABLE BUILDINGS
        // =======================

        var allBuildings = Enum.GetValues(typeof(BuildingDefinition))
                               .Cast<BuildingDefinition>();

        if (scenario.AvailableBuildings == null)
            scenario.AvailableBuildings = new System.Collections.Generic.List<BuildingAvailibility>();

        foreach (var building in allBuildings)
        {
            if (building == BuildingDefinition.None)
                continue;

            if (!scenario.AvailableBuildings.Any(x => x.buildingDefinition == building))
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

        // =======================
        //  OBJECTIVES
        // =======================

        var objectivesProp = serializedObject.FindProperty("Objectives");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Objectives", EditorStyles.boldLabel);

        for (int i = 0; i < objectivesProp.arraySize; i++)
        {
            var element = objectivesProp.GetArrayElementAtIndex(i);

            var label = element.managedReferenceValue is Objective objective ? objective.Name : $"Element {i}";
            var guiContent = new GUIContent(label);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.PropertyField(element, guiContent, true);

            if (GUILayout.Button("Remove"))
            {
                objectivesProp.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Objective"))
        {
            var menu = new GenericMenu();

            foreach (var type in objectiveTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    serializedObject.Update();

                    objectivesProp.arraySize++;
                    var element = objectivesProp.GetArrayElementAtIndex(objectivesProp.arraySize - 1);
                    element.managedReferenceValue = Activator.CreateInstance(type);

                    serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(target);
    }
}