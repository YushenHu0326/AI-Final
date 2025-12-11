using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTTController : MonoBehaviour
{
    NavMesh navMesh;
    Vector3 start, end;

    public class RTTNode
    {
        public Vector3 position;
        public RTTNode previous;
    }

    public float nodeResolution = 1f;
    // Start is called before the first frame update
    void Awake()
    {
        navMesh = FindObjectOfType<NavMesh>();
        start = navMesh.gameObject.transform.position;
        end = start + new Vector3(navMesh.cellSize * (float)navMesh.cellX, 
                                  navMesh.cellSize * (float)navMesh.cellY,
                                  navMesh.cellSize * (float)navMesh.cellZ);
    }

    public List<Vector3> ExpandNode(float radius, Vector3 init, Vector3 dest)
    {
        List<RTTNode> nodes = new List<RTTNode>();

        Vector3 current = init;
        RTTNode startNode = new RTTNode();
        startNode.position = current;
        nodes.Add(startNode);

        RTTNode node = startNode;
        
        RaycastHit hit;

        int safecheck1 = 0;

        while (safecheck1 < 20000 && Physics.SphereCast(current, radius, (dest - current).normalized, out hit, (dest - current).magnitude))
        {
            safecheck1++;

            int safecheck2 = 0;
            Vector3 sample = new Vector3(Random.Range(start.x, end.x),
                                         Random.Range(start.y, end.y),
                                         Random.Range(start.z, end.z));

            while (safecheck2 < 1000 && Physics.SphereCast(current, radius, (sample - current).normalized, out hit, (sample - current).magnitude))
            {
                sample = new Vector3(Random.Range(start.x, end.x),
                                     Random.Range(start.y, end.y),
                                     Random.Range(start.z, end.z));
                safecheck2++;
            }

            int nearestIdx = 0;
            for (int i = 1; i < nodes.Count; i++)
            {
                if ((nodes[i].position - sample).magnitude < (nodes[nearestIdx].position - sample).magnitude)
                {
                    nearestIdx = i;
                }
            }

            Vector3 next = nodes[nearestIdx].position + nodeResolution * (sample - nodes[nearestIdx].position).normalized;

            if (Physics.CheckSphere(next, radius))
                continue;
            RTTNode nextNode = new RTTNode();
            nextNode.position = next;
            nextNode.previous = nodes[nearestIdx];
            nodes.Add(nextNode);

            current = next;
            node = nextNode;

            Debug.DrawLine(nextNode.position, nextNode.previous.position, Color.red, 20f);
        }

        List<Vector3> path = new List<Vector3>();
        path.Add(dest);
        while (node.previous != null)
        {
            path.Add(node.position);
            node = node.previous;
        }

        path.Reverse();
        return path;
    }
}
