using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StationGrid))]
public class StationGridInspector : Editor {
    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        StationGrid stationGrid = (StationGrid)target;
        if (GUILayout.Button("Create Grid")) {
            stationGrid.DeleteGrid();
            stationGrid.RandomSeed();
            stationGrid.CreateGrid();
            stationGrid.CalculateStations(stationGrid.numberOfStations);
        }
        /*if (GUILayout.Button("Random Seed")) {
            stationGrid.RandomSeed();
        }*/
        if (GUILayout.Button("Instantiate Stations")) {
            stationGrid.SolveStations();
        }
        if (GUILayout.Button("Delete Grid")) {
            stationGrid.DeleteGrid();
        }
        /*if (GUILayout.Button("Draw Lines")) {
            stationGrid.DrawLines();
        }*/
    }
}