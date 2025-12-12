using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CanvasGrid : MonoBehaviour
{
    // the class that stores information for each vertex on a grid
    public class GridInfo
    {
        public int x;
        public int y;
        public int z;
        public Vector3 position;
        public float val;
        public List<int> xTetras, yTetras, zTetras, wTetras;
    }

    // the cube that will be used to compute the actual mesh, consists of 8 vertices from the grid
    public struct Cube
    {
        public GridInfo x1y1z1;
        public GridInfo x2y1z1;
        public GridInfo x1y2z1;
        public GridInfo x2y2z1;
        public GridInfo x1y1z2;
        public GridInfo x2y1z2;
        public GridInfo x1y2z2;
        public GridInfo x2y2z2;
    }

    // the struct of cube but for GPU computing
    public struct Tetrahedra
    {
        public Vector3 x;
        public Vector3 y;
        public Vector3 z;
        public Vector3 w;

        public float xVal;
        public float yVal;
        public float zVal;
        public float wVal;

        public Vector3 t1a;
        public Vector3 t1b;
        public Vector3 t1c;
        public Vector3 t1n;

        public Vector3 t2a;
        public Vector3 t2b;
        public Vector3 t2c;
        public Vector3 t2n;
    }

    public class Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
        public Vector3 n;
    }

    public int xIndex, yIndex, zIndex;

    // the size of the grid canvas
    public float canvasSize = 100f;

    // the number of division
    public int grid = 10;

    // the maximum value stored on each vertex
    public float surfaceVal = 0.5f;

    // grid/cell size computed on start
    float gridSize;

    public bool useGPU;

    public GridInfo[] gridInfos;

    Cube[] cubes;
    Tetrahedra[] tetras;

    MeshFilter filter;
    MeshRenderer meshRenderer;
    Mesh mesh;

    bool reconstructing;

    public int totalGridNum = 1;

    public Material mat;

    public ComputeShader marchingTetrahedraShader;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitGrid();

        InitCube();

        filter = gameObject.GetComponent<MeshFilter>();
        if (filter == null) filter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.materials[0] = mat;

        Vector3[] newVertices = new Vector3[0];
        Vector2[] newUVs = new Vector2[0];
        int[] newTriangles = new int[0];

        mesh = new Mesh
        {
            vertices = newVertices,
            uv = newUVs,
            triangles = newTriangles
        };

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        filter.mesh = mesh;
    }

    void Update()
    {
        
    }

    void InitGrid()
    {
        gridSize = canvasSize / (float)grid;
        gridInfos = new GridInfo[(grid + 1) * (grid + 1) * (grid + 1)];

        for (int x = 0; x <= grid; x++)
        {
            for (int y = 0; y <= grid; y++)
            {
                for (int z = 0; z <= grid; z++)
                {
                    GridInfo info = new GridInfo();
                    info.x = x;
                    info.y = y;
                    info.z = z;
                    info.position = new Vector3(gridSize * (float)x, gridSize * (float)y, gridSize * (float)z);
                    info.val = 0f;
                    info.xTetras = new List<int>();
                    info.yTetras = new List<int>();
                    info.zTetras = new List<int>();
                    info.wTetras = new List<int>();

                    gridInfos[z + y * (grid + 1) + x * (grid + 1) * (grid + 1)] = info;
                }
            }
        }

        /*foreach (GridInfo info in gridInfos)
        {
            if (info.x > 2 && info.x < 8)
                if (info.y > 2 && info.y < 8)
                    if (info.z > 2 && info.z < 8)
                        info.val = 1f;
        }*/
        //SetGrid(1, 2, 1, 1f);
    }

    void InitCube()
    {
        cubes = new Cube[grid * grid * grid];
        tetras = new Tetrahedra[grid * grid * grid * 6];

        for (int x = 0; x < grid; x++)
        {
            for (int y = 0; y < grid; y++)
            {
                for (int z = 0; z < grid; z++)
                {
                    Cube cube = new Cube();
                    cube.x1y1z1 = FindGrid(x, y, z);
                    cube.x2y1z1 = FindGrid(x + 1, y, z);
                    cube.x1y2z1 = FindGrid(x, y + 1, z);
                    cube.x2y2z1 = FindGrid(x + 1, y + 1, z);
                    cube.x1y1z2 = FindGrid(x, y, z + 1);
                    cube.x2y1z2 = FindGrid(x + 1, y, z + 1);
                    cube.x1y2z2 = FindGrid(x, y + 1, z + 1);
                    cube.x2y2z2 = FindGrid(x + 1, y + 1, z + 1);

                    cubes[z + grid * y + grid * grid * x] = cube;

                    Tetrahedra tetra1 = new Tetrahedra();
                    tetra1.x = cube.x1y1z1.position;
                    tetra1.y = cube.x2y1z2.position;
                    tetra1.z = cube.x2y1z1.position;
                    tetra1.w = cube.x2y2z2.position;
                    tetra1.xVal = cube.x1y1z1.val;
                    tetra1.yVal = cube.x2y1z2.val;
                    tetra1.zVal = cube.x2y1z1.val;
                    tetra1.wVal = cube.x2y2z2.val;
                    tetras[(z + grid * y + grid * grid * x) * 6] = tetra1;
                    cube.x1y1z1.xTetras.Add((z + grid * y + grid * grid * x) * 6);
                    cube.x2y1z2.yTetras.Add((z + grid * y + grid * grid * x) * 6);
                    cube.x2y1z1.zTetras.Add((z + grid * y + grid * grid * x) * 6);
                    cube.x2y2z2.wTetras.Add((z + grid * y + grid * grid * x) * 6);

                    Tetrahedra tetra2 = new Tetrahedra();
                    tetra2.x = cube.x1y1z1.position;
                    tetra2.y = cube.x2y1z1.position;
                    tetra2.z = cube.x2y2z1.position;
                    tetra2.w = cube.x2y2z2.position;
                    tetra2.xVal = cube.x1y1z1.val;
                    tetra2.yVal = cube.x2y1z1.val;
                    tetra2.zVal = cube.x2y2z1.val;
                    tetra2.wVal = cube.x2y2z2.val;
                    tetras[(z + grid * y + grid * grid * x) * 6 + 1] = tetra2;
                    cube.x1y1z1.xTetras.Add((z + grid * y + grid * grid * x) * 6 + 1);
                    cube.x2y1z1.yTetras.Add((z + grid * y + grid * grid * x) * 6 + 1);
                    cube.x2y2z1.zTetras.Add((z + grid * y + grid * grid * x) * 6 + 1);
                    cube.x2y2z2.wTetras.Add((z + grid * y + grid * grid * x) * 6 + 1);

                    Tetrahedra tetra3 = new Tetrahedra();
                    tetra3.x = cube.x1y1z1.position;
                    tetra3.y = cube.x2y2z1.position;
                    tetra3.z = cube.x1y2z1.position;
                    tetra3.w = cube.x2y2z2.position;
                    tetra3.xVal = cube.x1y1z1.val;
                    tetra3.yVal = cube.x2y2z1.val;
                    tetra3.zVal = cube.x1y2z1.val;
                    tetra3.wVal = cube.x2y2z2.val;
                    tetras[(z + grid * y + grid * grid * x) * 6 + 2] = tetra3;
                    cube.x1y1z1.xTetras.Add((z + grid * y + grid * grid * x) * 6 + 2);
                    cube.x2y2z1.yTetras.Add((z + grid * y + grid * grid * x) * 6 + 2);
                    cube.x1y2z1.zTetras.Add((z + grid * y + grid * grid * x) * 6 + 2);
                    cube.x2y2z2.wTetras.Add((z + grid * y + grid * grid * x) * 6 + 2);

                    Tetrahedra tetra4 = new Tetrahedra();
                    tetra4.x = cube.x1y1z1.position;
                    tetra4.y = cube.x1y2z1.position;
                    tetra4.z = cube.x1y2z2.position;
                    tetra4.w = cube.x2y2z2.position;
                    tetra4.xVal = cube.x1y1z1.val;
                    tetra4.yVal = cube.x1y2z1.val;
                    tetra4.zVal = cube.x1y2z2.val;
                    tetra4.wVal = cube.x2y2z2.val;
                    tetras[(z + grid * y + grid * grid * x) * 6 + 3] = tetra4;
                    cube.x1y1z1.xTetras.Add((z + grid * y + grid * grid * x) * 6 + 3);
                    cube.x1y2z1.yTetras.Add((z + grid * y + grid * grid * x) * 6 + 3);
                    cube.x1y2z2.zTetras.Add((z + grid * y + grid * grid * x) * 6 + 3);
                    cube.x2y2z2.wTetras.Add((z + grid * y + grid * grid * x) * 6 + 3);

                    Tetrahedra tetra5 = new Tetrahedra();
                    tetra5.x = cube.x1y1z1.position;
                    tetra5.y = cube.x1y2z2.position;
                    tetra5.z = cube.x1y1z2.position;
                    tetra5.w = cube.x2y2z2.position;
                    tetra5.xVal = cube.x1y1z1.val;
                    tetra5.yVal = cube.x1y2z2.val;
                    tetra5.zVal = cube.x1y1z2.val;
                    tetra5.wVal = cube.x2y2z2.val;
                    tetras[(z + grid * y + grid * grid * x) * 6 + 4] = tetra5;
                    cube.x1y1z1.xTetras.Add((z + grid * y + grid * grid * x) * 6 + 4);
                    cube.x1y2z2.yTetras.Add((z + grid * y + grid * grid * x) * 6 + 4);
                    cube.x1y1z2.zTetras.Add((z + grid * y + grid * grid * x) * 6 + 4);
                    cube.x2y2z2.wTetras.Add((z + grid * y + grid * grid * x) * 6 + 4);

                    Tetrahedra tetra6 = new Tetrahedra();
                    tetra6.x = cube.x1y1z1.position;
                    tetra6.y = cube.x1y1z2.position;
                    tetra6.z = cube.x2y1z2.position;
                    tetra6.w = cube.x2y2z2.position;
                    tetra6.xVal = cube.x1y1z1.val;
                    tetra6.yVal = cube.x1y1z2.val;
                    tetra6.zVal = cube.x2y1z2.val;
                    tetra6.wVal = cube.x2y2z2.val;
                    tetras[(z + grid * y + grid * grid * x) * 6 + 5] = tetra6;
                    cube.x1y1z1.xTetras.Add((z + grid * y + grid * grid * x) * 6 + 5);
                    cube.x1y1z2.yTetras.Add((z + grid * y + grid * grid * x) * 6 + 5);
                    cube.x2y1z2.zTetras.Add((z + grid * y + grid * grid * x) * 6 + 5);
                    cube.x2y2z2.wTetras.Add((z + grid * y + grid * grid * x) * 6 + 5);
                }
            }
        }
    }

    public void UpdateMesh()
    {
        if (mesh == null) return;

        if (cubes == null) return;

        if (reconstructing) return;

        if (useGPU)
        {
            mesh.Clear(false);

            int verticesCount = 0;

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            ComputeBuffer tetraBuffer = new ComputeBuffer(tetras.Length, sizeof(float) * 40);
            tetraBuffer.SetData(tetras);
            marchingTetrahedraShader.SetBuffer(0, "tetras", tetraBuffer);
            marchingTetrahedraShader.SetFloat("surfaceVal", surfaceVal);
            marchingTetrahedraShader.SetFloat("gridSize", gridSize);
            marchingTetrahedraShader.Dispatch(0, tetras.Length / 50, 1, 1);

            tetraBuffer.GetData(tetras);
            tetraBuffer.Dispose();

            for (int i = 0; i < cubes.Length; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (tetras[i * 6 + j].t1n.magnitude > 0f)
                    {
                        vertices.Add(tetras[i * 6 + j].t1a);
                        vertices.Add(tetras[i * 6 + j].t1b);
                        vertices.Add(tetras[i * 6 + j].t1c);

                        Vector3 n = Vector3.Cross(tetras[i * 6 + j].t1b - tetras[i * 6 + j].t1a, tetras[i * 6 + j].t1c - tetras[i * 6 + j].t1a);
                        if (Vector3.Dot(n, tetras[i * 6 + j].t1n) > 0f)
                        {
                            triangles.Add(verticesCount);
                            triangles.Add(verticesCount + 1);
                            triangles.Add(verticesCount + 2);
                        }
                        else
                        {
                            triangles.Add(verticesCount);
                            triangles.Add(verticesCount + 2);
                            triangles.Add(verticesCount + 1);
                        }

                        verticesCount += 3;
                    }

                    if (tetras[i * 6 + j].t2n.magnitude > 0f)
                    {
                        vertices.Add(tetras[i * 6 + j].t2a);
                        vertices.Add(tetras[i * 6 + j].t2b);
                        vertices.Add(tetras[i * 6 + j].t2c);

                        Vector3 n = Vector3.Cross(tetras[i * 6 + j].t2b - tetras[i * 6 + j].t2a, tetras[i * 6 + j].t2c - tetras[i * 6 + j].t2a);
                        if (Vector3.Dot(n, tetras[i * 6 + j].t2n) > 0f)
                        {
                            triangles.Add(verticesCount);
                            triangles.Add(verticesCount + 1);
                            triangles.Add(verticesCount + 2);
                        }
                        else
                        {
                            triangles.Add(verticesCount);
                            triangles.Add(verticesCount + 2);
                            triangles.Add(verticesCount + 1);
                        }

                        verticesCount += 3;
                    }
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateNormals();

            if (!GetComponent<MeshCollider>())
                gameObject.AddComponent<MeshCollider>();
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
        else
        {
            StartCoroutine(ReconstructMeshCPU());
        }
    }

    IEnumerator ReconstructMeshCPU()
    {
        mesh.Clear(false);

        reconstructing = true;

        int verticesCount = 0;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int i = 0; i < cubes.Length; i++)
        {
            Cube cube = cubes[i];
            List<Triangle> t1 = MarchingTetrahedra(cube.x1y1z1, cube.x2y1z2, cube.x2y1z1, cube.x2y2z2);
            List<Triangle> t2 = MarchingTetrahedra(cube.x1y1z1, cube.x2y1z1, cube.x2y2z1, cube.x2y2z2);
            List<Triangle> t3 = MarchingTetrahedra(cube.x1y1z1, cube.x2y2z1, cube.x1y2z1, cube.x2y2z2);
            List<Triangle> t4 = MarchingTetrahedra(cube.x1y1z1, cube.x1y2z1, cube.x1y2z2, cube.x2y2z2);
            List<Triangle> t5 = MarchingTetrahedra(cube.x1y1z1, cube.x1y2z2, cube.x1y1z2, cube.x2y2z2);
            List<Triangle> t6 = MarchingTetrahedra(cube.x1y1z1, cube.x1y1z2, cube.x2y1z2, cube.x2y2z2);

            t1.AddRange(t2);
            t1.AddRange(t3);
            t1.AddRange(t4);
            t1.AddRange(t5);
            t1.AddRange(t6);

            if (t1.Count > 0)
            {
                foreach (Triangle t in t1)
                {
                    vertices.Add(t.a);
                    vertices.Add(t.b);
                    vertices.Add(t.c);

                    Vector3 n = Vector3.Cross(t.b - t.a, t.c - t.a);
                    if (Vector3.Dot(n, t.n) > 0f)
                    {
                        triangles.Add(verticesCount);
                        triangles.Add(verticesCount + 1);
                        triangles.Add(verticesCount + 2);
                    }
                    else
                    {
                        triangles.Add(verticesCount);
                        triangles.Add(verticesCount + 2);
                        triangles.Add(verticesCount + 1);
                    }

                    verticesCount += 3;
                }
            }
            if (i % (int)Mathf.Ceil(100f / (float)totalGridNum) == 0)
            {
                yield return new WaitForFixedUpdate();

                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();

                mesh.RecalculateNormals();
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();

        reconstructing = false;
    }

    List<Triangle> MarchingTetrahedra(GridInfo x, GridInfo y, GridInfo z, GridInfo w)
    {
        List<Triangle> triangles = new List<Triangle>();

        if (x.val < surfaceVal && y.val > surfaceVal && z.val > surfaceVal && w.val > surfaceVal)
        {
            Triangle triangle = new Triangle();
            triangle.a = y.position + (x.position - y.position) * ((surfaceVal - y.val) / (x.val - y.val));
            triangle.b = z.position + (x.position - z.position) * ((surfaceVal - z.val) / (x.val - z.val));
            triangle.c = w.position + (x.position - w.position) * ((surfaceVal - w.val) / (x.val - w.val));
            triangle.n = x.position - y.position;
            triangles.Add(triangle);
        }

        if (x.val > surfaceVal && y.val < surfaceVal && z.val < surfaceVal && w.val < surfaceVal)
        {
            Triangle triangle = new Triangle();
            triangle.a = x.position + (y.position - x.position) * ((surfaceVal - x.val) / (y.val - x.val));
            triangle.b = x.position + (z.position - x.position) * ((surfaceVal - x.val) / (z.val - x.val));
            triangle.c = x.position + (w.position - x.position) * ((surfaceVal - x.val) / (w.val - x.val));
            triangle.n = y.position - x.position;
            triangles.Add(triangle);
        }

        if (y.val < surfaceVal && x.val > surfaceVal && z.val > surfaceVal && w.val > surfaceVal)
        {
            Triangle triangle = new Triangle();
            triangle.a = x.position + (y.position - x.position) * ((surfaceVal - x.val) / (y.val - x.val));
            triangle.b = z.position + (y.position - z.position) * ((surfaceVal - z.val) / (y.val - z.val));
            triangle.c = w.position + (y.position - w.position) * ((surfaceVal - w.val) / (y.val - w.val));
            triangle.n = y.position - x.position;
            triangles.Add(triangle);
        }

        if (y.val > surfaceVal && x.val < surfaceVal && z.val < surfaceVal && w.val < surfaceVal)
        {
            Triangle triangle = new Triangle();
            triangle.a = y.position + (x.position - y.position) * ((surfaceVal - y.val) / (x.val - y.val));
            triangle.b = y.position + (z.position - y.position) * ((surfaceVal - y.val) / (z.val - y.val));
            triangle.c = y.position + (w.position - y.position) * ((surfaceVal - y.val) / (w.val - y.val));
            triangle.n = x.position - y.position;
            triangles.Add(triangle);
        }

        if (z.val < surfaceVal && y.val > surfaceVal && x.val > surfaceVal && w.val > surfaceVal)
        {
            Triangle triangle = new Triangle();
            triangle.a = x.position + (z.position - x.position) * ((surfaceVal - x.val) / (z.val - x.val));
            triangle.b = y.position + (z.position - y.position) * ((surfaceVal - y.val) / (z.val - y.val));
            triangle.c = w.position + (z.position - w.position) * ((surfaceVal - w.val) / (z.val - w.val));
            triangle.n = z.position - y.position;
            triangles.Add(triangle);
        }

        if (z.val > surfaceVal && y.val < surfaceVal && x.val < surfaceVal && w.val < surfaceVal)
        {
            Triangle triangle = new Triangle();
            triangle.a = z.position + (x.position - z.position) * ((surfaceVal - z.val) / (x.val - z.val));
            triangle.b = z.position + (y.position - z.position) * ((surfaceVal - z.val) / (y.val - z.val));
            triangle.c = z.position + (w.position - z.position) * ((surfaceVal - z.val) / (w.val - z.val));
            triangle.n = y.position - z.position;
            triangles.Add(triangle);
        }

        if (w.val < surfaceVal && y.val > surfaceVal && x.val > surfaceVal && z.val > surfaceVal)
        {
            Triangle triangle = new Triangle();
            triangle.a = x.position + (w.position - x.position) * ((surfaceVal - x.val) / (w.val - x.val));
            triangle.b = y.position + (w.position - y.position) * ((surfaceVal - y.val) / (w.val - y.val));
            triangle.c = z.position + (w.position - z.position) * ((surfaceVal - z.val) / (w.val - z.val));
            triangle.n = w.position - y.position;
            triangles.Add(triangle);
        }

        if (w.val > surfaceVal && y.val < surfaceVal && x.val < surfaceVal && z.val < surfaceVal)
        {
            Triangle triangle = new Triangle();
            triangle.a = w.position + (x.position - w.position) * ((surfaceVal - w.val) / (x.val - w.val));
            triangle.b = w.position + (y.position - w.position) * ((surfaceVal - w.val) / (y.val - w.val));
            triangle.c = w.position + (z.position - w.position) * ((surfaceVal - w.val) / (z.val - w.val));
            triangle.n = y.position - w.position;
            triangles.Add(triangle);
        }

        if (x.val < surfaceVal && y.val < surfaceVal && z.val > surfaceVal && w.val > surfaceVal)
        {
            Triangle triangle1 = new Triangle();
            Triangle triangle2 = new Triangle();

            triangle1.a = z.position + (x.position - z.position) * ((surfaceVal - z.val) / (x.val - z.val));
            triangle1.b = w.position + (x.position - w.position) * ((surfaceVal - w.val) / (x.val - w.val));
            triangle1.c = w.position + (y.position - w.position) * ((surfaceVal - w.val) / (y.val - w.val));
            triangle1.n = x.position - z.position;

            triangle2.a = z.position + (y.position - z.position) * ((surfaceVal - z.val) / (y.val - z.val));
            triangle2.b = w.position + (y.position - w.position) * ((surfaceVal - w.val) / (y.val - w.val));
            triangle2.c = z.position + (x.position - z.position) * ((surfaceVal - z.val) / (x.val - z.val));
            triangle2.n = x.position - z.position;

            triangles.Add(triangle1);
            triangles.Add(triangle2);
        }

        if (x.val > surfaceVal && y.val > surfaceVal && z.val < surfaceVal && w.val < surfaceVal)
        {
            Triangle triangle1 = new Triangle();
            Triangle triangle2 = new Triangle();

            triangle1.a = x.position + (z.position - x.position) * ((surfaceVal - x.val) / (z.val - x.val));
            triangle1.b = x.position + (w.position - x.position) * ((surfaceVal - x.val) / (w.val - x.val));
            triangle1.c = y.position + (w.position - y.position) * ((surfaceVal - y.val) / (w.val - y.val));
            triangle1.n = z.position - x.position;

            triangle2.a = y.position + (z.position - y.position) * ((surfaceVal - y.val) / (z.val - y.val));
            triangle2.b = y.position + (w.position - y.position) * ((surfaceVal - y.val) / (w.val - y.val));
            triangle2.c = x.position + (z.position - x.position) * ((surfaceVal - x.val) / (z.val - x.val));
            triangle2.n = z.position - x.position;

            triangles.Add(triangle1);
            triangles.Add(triangle2);
        }

        if (x.val < surfaceVal && z.val <surfaceVal && y.val > surfaceVal && w.val > surfaceVal)
        {
            Triangle triangle1 = new Triangle();
            Triangle triangle2 = new Triangle();

            triangle1.a = y.position + (x.position - y.position) * ((surfaceVal - y.val) / (x.val - y.val));
            triangle1.b = w.position + (x.position - w.position) * ((surfaceVal - w.val) / (x.val - w.val));
            triangle1.c = w.position + (z.position - w.position) * ((surfaceVal - w.val) / (z.val - w.val));
            triangle1.n = x.position - y.position;

            triangle2.a = y.position + (z.position - y.position) * ((surfaceVal - y.val) / (z.val - y.val));
            triangle2.b = w.position + (z.position - w.position) * ((surfaceVal - w.val) / (z.val - w.val));
            triangle2.c = y.position + (x.position - y.position) * ((surfaceVal - y.val) / (x.val - y.val));
            triangle2.n = x.position - y.position;

            triangles.Add(triangle1);
            triangles.Add(triangle2);
        }

        if (x.val > surfaceVal && z.val > surfaceVal && y.val < surfaceVal && w.val <surfaceVal)
        {
            Triangle triangle1 = new Triangle();
            Triangle triangle2 = new Triangle();

            triangle1.a = x.position + (y.position - x.position) * ((surfaceVal - x.val) / (y.val - x.val));
            triangle1.b = x.position + (w.position - x.position) * ((surfaceVal - x.val) / (w.val - x.val));
            triangle1.c = z.position + (w.position - z.position) * ((surfaceVal - z.val) / (w.val - z.val));
            triangle1.n = y.position - x.position;

            triangle2.a = z.position + (y.position - z.position) * ((surfaceVal - z.val) / (y.val - z.val));
            triangle2.b = z.position + (w.position - z.position) * ((surfaceVal - z.val) / (w.val - z.val));
            triangle2.c = x.position + (y.position - x.position) * ((surfaceVal - x.val) / (y.val - x.val));
            triangle2.n = y.position - x.position;

            triangles.Add(triangle1);
            triangles.Add(triangle2);
        }

        if (x.val < surfaceVal && w.val < surfaceVal && z.val > surfaceVal && y.val > surfaceVal)
        {
            Triangle triangle1 = new Triangle();
            Triangle triangle2 = new Triangle();

            triangle1.a = z.position + (x.position - z.position) * ((surfaceVal - z.val) / (x.val - z.val));
            triangle1.b = y.position + (x.position - y.position) * ((surfaceVal - y.val) / (x.val - y.val));
            triangle1.c = y.position + (w.position - y.position) * ((surfaceVal - y.val) / (w.val - y.val));
            triangle1.n = x.position - z.position;

            triangle2.a = z.position + (w.position - z.position) * ((surfaceVal - z.val) / (w.val - z.val));
            triangle2.b = y.position + (w.position - y.position) * ((surfaceVal - y.val) / (w.val - y.val));
            triangle2.c = z.position + (x.position - z.position) * ((surfaceVal - z.val) / (x.val - z.val));
            triangle2.n = x.position - z.position;

            triangles.Add(triangle1);
            triangles.Add(triangle2);
        }

        if (x.val > surfaceVal && w.val > surfaceVal && z.val < surfaceVal && y.val < surfaceVal)
        {
            Triangle triangle1 = new Triangle();
            Triangle triangle2 = new Triangle();

            triangle1.a = x.position + (z.position - x.position) * ((surfaceVal - x.val) / (z.val - x.val));
            triangle1.b = x.position + (y.position - x.position) * ((surfaceVal - x.val) / (y.val - x.val));
            triangle1.c = w.position + (z.position - w.position) * ((surfaceVal - w.val) / (z.val - w.val));
            triangle1.n = z.position - x.position;

            triangle2.a = w.position + (z.position - w.position) * ((surfaceVal - w.val) / (z.val - w.val));
            triangle2.b = w.position + (y.position - w.position) * ((surfaceVal - w.val) / (y.val - w.val));
            triangle2.c = x.position + (y.position - x.position) * ((surfaceVal - x.val) / (y.val - x.val));
            triangle2.n = z.position - x.position;

            triangles.Add(triangle1);
            triangles.Add(triangle2);
        }

        return triangles;
    }

    public void SetGrid(int x, int y, int z, float val)
    {
        GridInfo gridInfo = FindGrid(x, y, z);
        gridInfo.val = val;

        foreach (int i in gridInfo.xTetras)
            tetras[i].xVal = val;
        foreach (int i in gridInfo.yTetras)
            tetras[i].yVal = val;
        foreach (int i in gridInfo.zTetras)
            tetras[i].zVal = val;
        foreach (int i in gridInfo.wTetras)
            tetras[i].wVal = val;
    }

    public float GetGrid(int x, int y, int z)
    {
        GridInfo gridInfo = FindGrid(x, y, z);
        return gridInfo.val;
    }

    // Helper function to locate a grid vertex
    public GridInfo FindGrid(int x, int y, int z)
    {
        return gridInfos[z + (grid + 1) * y + (grid + 1) * (grid + 1) * x];
    }
}
