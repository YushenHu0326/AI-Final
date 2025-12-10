using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMesh : MonoBehaviour
{
    public int cellX, cellY, cellZ;
    public float cellSize = 1f;

    public struct NavMeshCell
    {
        public int x, y, z;
        public Vector3 position;
        public bool blocked;
        public List<NavMeshCell> neighbors;
    }

    private List<NavMeshCell> cells;

    // Start is called before the first frame update
    void Awake()
    {
        cells = new List<NavMeshCell>();

        for (int x = 0; x < cellX; x++)
        {
            for (int y = 0; y < cellY; y++)
            {
                for (int z = 0; z < cellZ; z++)
                {
                    NavMeshCell cell = new NavMeshCell();
                    cell.x = x;
                    cell.y = y;
                    cell.z = z;

                    Vector3 pos = new Vector3();
                    pos.x = ((float)x + 0.5f) * cellSize + gameObject.transform.position.x;
                    pos.y = ((float)y + 0.5f) * cellSize + gameObject.transform.position.y;
                    pos.z = ((float)z + 0.5f) * cellSize + gameObject.transform.position.z;
                    cell.position = pos;

                    Collider[] hits = Physics.OverlapBox(
                        center: pos,
                        halfExtents: new Vector3(cellSize, cellSize, cellSize) / 2f
                    );
                    cell.blocked = hits.Length > 0;

                    cell.neighbors = new List<NavMeshCell>();

                    cells.Add(cell);
                }
            }
        }

        for (int x = 0; x < cellX; x++)
        {
            for (int y = 0; y < cellY; y++)
            {
                for (int z = 0; z < cellZ; z++)
                {
                    NavMeshCell cell = cells[z + cellZ * y + cellY * cellZ * x];
                    if (z > 0) cell.neighbors.Add(cells[z - 1 + cellZ * y + cellY * cellZ * x]);
                    if (z < cellZ - 1) cell.neighbors.Add(cells[z + 1 + cellZ * y + cellY * cellZ * x]);

                    if (y > 0) cell.neighbors.Add(cells[z + cellZ * (y - 1) + cellY * cellZ * x]);
                    if (y < cellY - 1) cell.neighbors.Add(cells[z + cellZ * (y + 1) + cellY * cellZ * x]);

                    if (x > 0) cell.neighbors.Add(cells[z + cellZ * y + cellY * cellZ * (x - 1)]);
                    if (x < cellX - 1) cell.neighbors.Add(cells[z + cellZ * y + cellY * cellZ * (x + 1)]);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public NavMeshCell GetCell(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= cellX || y >= cellY || z >= cellZ)
            return default;
        return cells[z + cellZ * y + cellY * cellZ * x];
    }

    public bool IsBlocked(int x, int y, int z)
    {
        var c = GetCell(x, y, z);
        return c.blocked;
    }

    public Vector3 GetCellPosition(int x, int y, int z)
    {
        return GetCell(x, y, z).position;
    }

    public Vector3Int GetCellIndex(float x, float y, float z)
    {
        Vector3 local = new Vector3(x, y, z) - transform.position;
        return new Vector3Int((int)Mathf.Floor(local.x / cellSize), (int)Mathf.Floor(local.y / cellSize), (int)Mathf.Floor(local.z / cellSize));
    }

    public List<NavMeshCell> GetCellNeighbors(int x, int y, int z)
    {
        return GetCell(x, y, z).neighbors;
    }
}
