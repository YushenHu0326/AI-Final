using UnityEngine;
using System.Collections;

public class DebugController : MonoBehaviour
{
    public CanvasGrid grid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(AddPointCloud());
    }

    IEnumerator AddPointCloud()
    {
        yield return new WaitForSeconds(1f);
        for (int x = 1; x < grid.grid; x++)
            for (int y = 1; y < grid.grid; y++)
                for (int z = 1; z < grid.grid; z++)
                    grid.SetGrid(x, y, z, Mathf.Max(0, Perlin.Noise((float)x / (float)grid.grid * 5f, (float)y / (float)grid.grid * 5f, (float)z / (float)grid.grid * 5f)));
        /*for (int x = 0; x < grid.grid + 1; x++)
            for (int y = 0; y < grid.grid + 1; y++)
                for (int z = 0; z < grid.grid + 1; z++)
                    if (Vector3.Distance(new Vector3((float)x, (float)y, (float)z), new Vector3((float)grid.grid / 2, (float)grid.grid / 2, (float)grid.grid / 2)) < (float)grid.grid / 2)
                        grid.SetGrid(x, y, z, 1f);*/

        grid.UpdateMesh();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
