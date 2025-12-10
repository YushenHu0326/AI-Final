using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgent : MonoBehaviour
{
    NavMesh navMesh;
    Vector3 pos;
    Vector3Int current, next;

    public Vector3Int initPosition;

    public Vector3 destination;

    bool arriveAtStop;
    // Start is called before the first frame update
    void Start()
    {
        navMesh = FindObjectOfType<NavMesh>();
        gameObject.transform.position = navMesh.GetCellPosition(initPosition.x, initPosition.y, initPosition.z);
        pos = gameObject.transform.position;

        next = initPosition;

        QueryNextPosition(destination);
    }

    // Update is called once per frame
    void Update()
    {
        pos = navMesh.GetCellPosition(next.x, next.y, next.z);
        if ((pos - gameObject.transform.position).magnitude > 0.01f)
        {
            gameObject.transform.position += (pos - gameObject.transform.position).normalized * 1f * Time.deltaTime;
            arriveAtStop = false;
        }
        else
        {
            if (!arriveAtStop)
            {
                arriveAtStop = true;
                QueryNextPosition(destination);
            }
        }

        current = navMesh.GetCellIndex(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
    }

    public void QueryNextPosition(Vector3 dest)
    {
        NavMesh.NavMeshCell currentCell = navMesh.GetCell(current.x, current.y, current.z);
        foreach (NavMesh.NavMeshCell cell in currentCell.neighbors)
        {
            
        }
    }
}
