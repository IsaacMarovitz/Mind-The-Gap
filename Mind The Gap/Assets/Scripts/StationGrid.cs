using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;
using ProceduralPoints;

public class StationGrid : MonoBehaviour {

    public GameObject prefab;
    public GameObject stationPrefab;
    public GameObject gridParent;
    public int xSize;
    public int zSize;
    public int numberOfStations;
    public float xOffset;
    public float zOffset;
    public Vector3 startPosition;
    public Material stationMaterial;
    public List<Point> pointsList;
    public List<Point> stationList;
    [HideInInspector]
    public GAController gaController;
    public List<GameObject> stationObjects;
    public float seed;

    private Vector3 position;
    private PathCreator pathCreator;
    public PathPlacer pathPlacer;
    private float oldSeed;
    private bool stationsHaveBeenSolved = false;

    public void Start() {
        gaController = GetComponent<GAController>();
        pathCreator = GetComponent<PathCreator>();
        pathPlacer = GetComponent<PathPlacer>();
        pointsList = new List<Point>();
        stationList = new List<Point>();
        gaController.tspSolved += StationsSolved;
        
        DeleteGrid();
        RandomSeed();
        CreateGrid();
        CalculateStations(numberOfStations);
        SolveStations();
    }

    public void DeleteGrid() {
        foreach (Point point in pointsList) {
            GameObject.DestroyImmediate(point.gameObject);
        }
        pointsList.Clear();
        stationList.Clear();
    }

    public void RandomSeed() {
        seed = UnityEngine.Random.Range(0.0f, 1000.0f);
    }

    public void CreateGrid() {
        position.x = startPosition.x;
        for (int x = 0; x < xSize; x++) {
            position.z = startPosition.z;
            for (int z = 0; z < zSize; z++) {
                GameObject newObject = GameObject.Instantiate(prefab, position, Quaternion.identity);
                newObject.transform.parent = gridParent.transform;
                newObject.name = x + ", " + z;
                pointsList.Add(new Point(newObject, PerlinNoise.ProceduralPoints(x, z, seed), x, z));
                position.z += zOffset;
            }
            position.x += xOffset;
        }
    }

    public void CalculateStations(int numberOfStations) {
        for (int i = 0; i < numberOfStations; i++) {
            Point heighestValuePoint = new Point(null, 0.0f, 0, 0);
            foreach (Point point in pointsList) {
                if (heighestValuePoint.randomNum < point.randomNum && !(stationList.Contains(point))){
                    heighestValuePoint = point;
                } 
            }
            heighestValuePoint.gameObject.GetComponent<MeshRenderer>().material = stationMaterial;
            stationList.Add(heighestValuePoint);
        }
        Sort();
    }

    public void SolveStations() {
        gaController.TSPSolve(numberOfStations, stationList);
    }

    public void StationsSolved() {
        stationsHaveBeenSolved = true;
    }

    public void InstantiateStations() {
        stationsHaveBeenSolved = false;
        stationList = gaController.finalPoints;
        stationObjects = new List<GameObject>();
        for (int i = 0; i < stationList.Count; i++) {
            Vector3 tempVector = new Vector3();
            if (i == 0) {
                tempVector = Bisect(stationList[stationList.Count - 1].gameObject.transform.position,
                stationList[i].gameObject.transform.position,
                stationList[i + 1].gameObject.transform.position);
            } else if (i == stationList.Count - 1) {
                tempVector = Bisect(stationList[i - 1].gameObject.transform.position,
                stationList[i].gameObject.transform.position,
                stationList[0].gameObject.transform.position);
            } else {
                tempVector = Bisect(stationList[i - 1].gameObject.transform.position,
                stationList[i].gameObject.transform.position,
                stationList[i + 1].gameObject.transform.position);
            }
            GameObject stationObject = Instantiate(stationPrefab, stationList[i].gameObject.transform.position, Quaternion.identity);
            if (tempVector.x == stationObject.transform.position.x) {
                tempVector = new Vector3(stationObject.transform.position.x+10, stationObject.transform.position.x, stationObject.transform.position.z);
            } else if (tempVector.z == stationObject.transform.position.z) {
                tempVector = new Vector3(stationObject.transform.position.x, stationObject.transform.position.y, stationObject.transform.position.z+10);
            }
            stationObject.transform.LookAt(tempVector); 
            stationObjects.Add(stationObject);
        }
        foreach (Point point in pointsList) {
            Destroy(point.gameObject);
        }
        DrawBezier(stationObjects);
    }

    public void DrawBezier(List<GameObject> stationObjects) {
        List<Vector3> bezierPoints = new List<Vector3>();
        for (int i = 0; i < stationObjects.Count; i++) {
            float distanceToEntrance;
            float distanceToExit;
            Vector3 entrancePosition = stationObjects[i].transform.Find("Entrance").transform.position;
            Vector3 exitPosition = stationObjects[i].transform.Find("Exit").transform.position;
            if (i > 0) {
                distanceToEntrance = Vector3.Distance(stationObjects[i - 1].transform.position, entrancePosition);
                distanceToExit = Vector3.Distance(stationObjects[i - 1].transform.position, exitPosition);
            } else {
                distanceToEntrance = Vector3.Distance(stationObjects[stationObjects.Count - 1].transform.position, entrancePosition);
                distanceToExit = Vector3.Distance(stationObjects[stationObjects.Count - 1].transform.position, exitPosition);
            }
            if (distanceToEntrance > distanceToExit) {
                bezierPoints.Add(exitPosition);
                bezierPoints.Add((exitPosition + entrancePosition) / 2);
                bezierPoints.Add(entrancePosition);
                int index = bezierPoints.IndexOf(exitPosition);
                bezierPoints[index+1].Set(exitPosition.x, exitPosition.y, exitPosition.z);
                index = bezierPoints.IndexOf((entrancePosition + exitPosition) / 2);
                bezierPoints[index-1].Set(((entrancePosition + exitPosition) / 2).x, ((entrancePosition + exitPosition) / 2).y, ((entrancePosition + exitPosition) / 2).z);
                bezierPoints[index+1].Set(((entrancePosition + exitPosition) / 2).x, ((entrancePosition + exitPosition) / 2).y, ((entrancePosition + exitPosition) / 2).z);
                index = bezierPoints.IndexOf(entrancePosition);
                bezierPoints[index-1].Set(entrancePosition.x, entrancePosition.y, entrancePosition.z);
            } else {
                bezierPoints.Add(entrancePosition);
                bezierPoints.Add((entrancePosition + exitPosition) / 2);
                bezierPoints.Add(exitPosition);
                int index = bezierPoints.IndexOf(entrancePosition);
                bezierPoints[index+1].Set(entrancePosition.x, entrancePosition.y, entrancePosition.z);
                index = bezierPoints.IndexOf((entrancePosition + exitPosition) / 2);
                bezierPoints[index-1].Set(((entrancePosition + exitPosition) / 2).x, ((entrancePosition + exitPosition) / 2).y, ((entrancePosition + exitPosition) / 2).z);
                bezierPoints[index+1].Set(((entrancePosition + exitPosition) / 2).x, ((entrancePosition + exitPosition) / 2).y, ((entrancePosition + exitPosition) / 2).z);
                index = bezierPoints.IndexOf(exitPosition);
                bezierPoints[index-1].Set(exitPosition.x, exitPosition.y, exitPosition.z);
            }
        }
        for (int i = 0; i < bezierPoints.Count; i++) {
           // bezierPoints[i]
        }
        CreateBezier(bezierPoints);
    }

    public void CreateBezier(List<Vector3> points) {
        if (points != null) {
            pathCreator = GetComponent<PathCreator>();
            pathCreator.bezierPath = new BezierPath(points, true, PathSpace.xz);
            pathCreator.bezierPath.controlMode = BezierPath.ControlMode.Free;
        }
        pathPlacer.Generate();
    }

    public Vector3 Bisect(Vector3 trA, Vector3 trB, Vector3 trC) {
        var v3BA = trA - trB;
        var v3BC = trC - trB;

        var axis = Vector3.Cross(v3BA, v3BC);
        var angle = Vector3.Angle(v3BA, v3BC);
        var v3 = Quaternion.AngleAxis(angle / 2.0f, axis) * v3BA;

        Debug.DrawRay(trB, v3, Color.magenta, 100f);
        return (trB + v3);
    }

    public void Sort() {
        List<Point> sorted = stationList.OrderBy(x => x.xCoord).ThenBy(x => x.zCoord).ToList();
        stationList = sorted;
    }

    public void Update() {
        if (stationsHaveBeenSolved) {
            InstantiateStations();
        }
    }
}

[System.Serializable]
public class Point {
    public GameObject gameObject;
    public float randomNum;
    public int xCoord;
    public int zCoord;
    public Point(GameObject gameObject, float randomNum, int xCoord, int zCoord) {
        this.gameObject = gameObject;
        this.randomNum = randomNum;
        this.xCoord = xCoord;
        this.zCoord = zCoord;
    }
}

