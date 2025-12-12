using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AIAgent : MonoBehaviour
{
    public enum AgentMode
    {
        AStar,
        RTT
    }

    public enum DebugMode
    {
        FullMode,
        SimpleMode,
        NoDebug
    }

    NavMesh navMesh;
    Vector3 pos;
    List<Vector3> path;

    public AgentMode agentMode = AgentMode.AStar;
    public DebugMode debugMode = DebugMode.FullMode;

    public Vector3 initPosition;

    public Transform destTransform;

    RTTController rTTController;

    bool arriveAtStop;
    // Start is called before the first frame update
    void Start()
    {
        navMesh = FindObjectOfType<NavMesh>();
        gameObject.transform.position = initPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (path == null) return;

        if (path.Count > 0)
        {
            pos = path[0];
            if ((pos - gameObject.transform.position).magnitude > 0.1f)
            {
                gameObject.transform.position += (pos - gameObject.transform.position).normalized * 5f * Time.deltaTime;
                arriveAtStop = false;
            }
            else
            {
                if (!arriveAtStop)
                {
                    arriveAtStop = true;
                    path.RemoveAt(0);
                }
            }
        }
    }

    public void PathPlanning()
    {
        Vector3 destPosition = destTransform.position;

        Stopwatch sw = new Stopwatch();

        if (agentMode == AgentMode.AStar)
        {
            sw.Start();
            path = AStarPathPlanning(navMesh.GetCellIndex(initPosition.x, initPosition.y, initPosition.z), navMesh.GetCellIndex(destPosition.x, destPosition.y, destPosition.z));
            sw.Stop();
            UnityEngine.Debug.Log("Runtime (A*): " + sw.ElapsedMilliseconds + " ms");
        }
        else
        {
            rTTController = FindObjectOfType<RTTController>();
            sw.Start();
            path = rTTController.ExpandNode(GetComponent<SphereCollider>().radius, initPosition, destPosition, debugMode);
            sw.Stop();
            UnityEngine.Debug.Log("Runtime (RRT): " + sw.ElapsedMilliseconds + " ms");
        }
    }

    List<Vector3> AStarPathPlanning(Vector3Int init, Vector3Int dest)
    {
        NavMesh.NavMeshCell initCell = navMesh.GetCell(init.x, init.y, init.z);
        NavMesh.NavMeshCell destCell = navMesh.GetCell(dest.x, dest.y, dest.z);

        if (initCell == null || destCell == null || initCell.blocked || destCell.blocked)
        {
            UnityEngine.Debug.Log(initCell.blocked);
            UnityEngine.Debug.Log(destCell.blocked);
            return new List<Vector3>();
        }

        List<NavMesh.NavMeshCell> openSet = new List<NavMesh.NavMeshCell>();
        HashSet<NavMesh.NavMeshCell> closedSet = new HashSet<NavMesh.NavMeshCell>();

        Dictionary<NavMesh.NavMeshCell, NavMesh.NavMeshCell> cameFrom = new Dictionary<NavMesh.NavMeshCell, NavMesh.NavMeshCell>();
        Dictionary<NavMesh.NavMeshCell, float> gScore = new Dictionary<NavMesh.NavMeshCell, float>();
        Dictionary<NavMesh.NavMeshCell, float> fScore = new Dictionary<NavMesh.NavMeshCell, float>();

        openSet.Add(initCell);
        gScore[initCell] = 0f;
        fScore[initCell] = navMesh.GetManhattanDistance(initCell, destCell);

        while (openSet.Count > 0)
        {
            // get node with lowest fScore
            NavMesh.NavMeshCell current = openSet[0];
            float bestF = fScore.ContainsKey(current) ? fScore[current] : Mathf.Infinity;
            for (int i = 1; i < openSet.Count; i++)
            {
                var c = openSet[i];
                float f = fScore.ContainsKey(c) ? fScore[c] : Mathf.Infinity;
                if (f < bestF)
                {
                    bestF = f;
                    current = c;
                }
            }

            if (current == destCell)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in current.neighbors)
            {
                if (neighbor == null) continue;
                if (neighbor.blocked) continue;
                if (closedSet.Contains(neighbor)) continue;

                float tentativeG = gScore[current] + 1f; // cost = 1 per move

                bool inOpen = openSet.Contains(neighbor);
                if (!inOpen || tentativeG < (gScore.ContainsKey(neighbor) ? gScore[neighbor] : Mathf.Infinity))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + navMesh.GetManhattanDistance(neighbor, destCell);

                    if (!inOpen)
                        openSet.Add(neighbor);
                }
            }
        }

        // no path
        UnityEngine.Debug.LogWarning("A* found no path.");
        return new List<Vector3>();
    }

    List<Vector3> ReconstructPath(Dictionary<NavMesh.NavMeshCell, NavMesh.NavMeshCell> cameFrom, NavMesh.NavMeshCell current)
    {
        List<Vector3> totalPath = new List<Vector3>();
        totalPath.Add(current.position);

        while (cameFrom.TryGetValue(current, out var prev))
        {
            current = prev;
            totalPath.Add(current.position);
        }

        totalPath.Reverse();
        return totalPath;
    }
}
