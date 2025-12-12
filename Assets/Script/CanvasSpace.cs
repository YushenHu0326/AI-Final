using UnityEngine;
using System.Collections;

public class CanvasSpace : MonoBehaviour
{
    public int xNum, yNum, zNum;

    public float gridSize;
    public int gridDivisions;

    public float surfaceValue;

    public bool useGPU;

    public Material mat;

    public ComputeShader marchingTetrahedraShader;

    CanvasGrid[] canvasGrids;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGrids = new CanvasGrid[xNum * yNum * zNum];
        for (int i = 0; i < xNum; i++)
        {
            for (int j = 0; j < yNum; j++)
            {
                for (int k = 0; k < zNum; k++)
                {
                    GameObject gridObject = new GameObject();
                    CanvasGrid grid = gridObject.AddComponent<CanvasGrid>();

                    grid.xIndex = i;
                    grid.yIndex = j;
                    grid.zIndex = k;

                    grid.canvasSize = gridSize;
                    grid.grid = gridDivisions;
                    grid.surfaceVal = surfaceValue;
                    grid.totalGridNum = xNum * yNum * zNum;
                    grid.useGPU = useGPU;
                    grid.marchingTetrahedraShader = marchingTetrahedraShader;

                    gridObject.transform.position = gameObject.transform.position + new Vector3(i * gridSize, j * gridSize, k * gridSize);

                    canvasGrids[k + j * zNum + i * yNum * zNum] = grid;
                }
            }
        }

        StartCoroutine(GenerateNoise());
    }

    public float GetGrid(int x, int y, int z, int i, int j, int k)
    {
        return canvasGrids[z + y * zNum + x * yNum * zNum].GetGrid(i, j, k);
    }

    public Vector3 GetGridPosition(int x, int y, int z, int i, int j, int k)
    {
        Vector3 gridPosition = canvasGrids[z + y * zNum + x * yNum * zNum].gameObject.transform.position;
        return gridPosition + new Vector3((float)i * gridSize / (float)gridDivisions, (float)j * gridSize / (float)gridDivisions, (float)k * gridSize / (float)gridDivisions);
    }

    IEnumerator GenerateNoise()
    {
        yield return new WaitForSeconds(0.1f);

        for (int x = 0; x < xNum; x++)
        {
            for (int y = 0; y < yNum; y++)
            {
                for (int z = 0; z < zNum; z++)
                {
                    for (int i = 0; i < gridDivisions + 1; i++)
                    {
                        for (int j = 0; j < gridDivisions + 1; j++)
                        {
                            for (int k = 0; k < gridDivisions + 1; k++)
                            {
                                canvasGrids[z + y * zNum + x * zNum * yNum].SetGrid(i, j, k, Mathf.Max(0f, Perlin.Noise((float)(i + x * gridDivisions) / 5f, (float)(j + y * gridDivisions) / 5f, (float)(k + z * gridDivisions) / 5f)));
                            }
                        }
                    }
                }
            }
        }

        for (int x = 0; x < xNum; x++)
        {
            for (int y = 0; y < yNum; y++)
            {
                for (int z = 0; z < zNum; z++)
                    canvasGrids[z + y * zNum + x * zNum * yNum].UpdateMesh();
            }
        }

        NavMesh navMesh = FindObjectOfType<NavMesh>();
        if (navMesh)
        {
            for (int i = 0; i < gridDivisions + 1; i++)
            {
                for (int j = 0; j < gridDivisions + 1; j++)
                {
                    for (int k = 0; k < gridDivisions + 1; k++)
                    {
                        navMesh.GetCell(i, j, k).blocked = canvasGrids[0].GetGrid(i, j, k) > surfaceValue;
                    }
                }
            }
        }
    }
}
