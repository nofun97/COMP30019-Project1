using UnityEngine;
using System;
using System.Collections.Generic;

public class TerrainGeneratorScript : MonoBehaviour
{
  public PointLight pointLight;
  private Vector3[] vertices;
  private int[] triangles;

  private int vectorCounts;
  public int dimension = 5;
  public float STEP = 0.5f;
  public float HeightVariance = 30;
  private int maxDimension, minDimension, OFFSET;
  Vector3[] vectorArray;
  private System.Random rand;
  // Used for detecting collisions in procedurally-generated mesh
  Mesh mesh;
  MeshFilter meshf;
  MeshCollider meshc;
  
  // Start is called before the first frame update
  void Start()
  {
    this.GenerateTerrainHeights();
    this.GenerateMesh();
    this.generateMeshCollider();
  }

  void Update()
  {
    // Get renderer component (in order to pass params to shader)
    MeshRenderer renderer = this.gameObject.GetComponent<MeshRenderer>();

    // Pass updated light positions to shader
    renderer.material.SetColor("_PointLightColor", this.pointLight.color);
    renderer.material.SetVector("_PointLightPosition", this.pointLight.GetWorldPosition());
  }

  void GenerateMesh()
  {
    Mesh mesh = new Mesh();
    mesh.name = "Terrain";
    this.GenerateVertices();
    mesh.vertices = vertices;
    this.triangles = new int[mesh.vertices.Length];
    for (int i = 0; i < mesh.vertices.Length; i++)
      triangles[i] = i;
    mesh.triangles = this.triangles;
    mesh.normals = this.generateNormals();
    this.gameObject.GetComponent<MeshFilter>().mesh = mesh;
  }

  Vector3[] generateNormals()
  {
    Vector3[] normals = new Vector3[(dimension - 1) * (dimension - 1) * 6];
    Dictionary<Vector3, Vector3> normalMap = new Dictionary<Vector3, Vector3>();
    for (int i = 0; i < vertices.Length; i += 6)
    {
      Vector3 side1, side2, diagonal, cross1, cross2;
      side1 = (vertices[i + 1] - vertices[i]).normalized;
      side2 = (vertices[i + 5] - vertices[i]).normalized;
      diagonal = (vertices[i + 2] - vertices[i]).normalized;
      cross1 = Vector3.Cross(side1, diagonal).normalized;
      cross2 = Vector3.Cross(diagonal, side2).normalized;

      for (int j = 0; j < 6; j++)
      {
        Vector3 cross = j < 3 ? cross1 : cross2;
        if (normalMap.ContainsKey(vertices[i + j]))
          normalMap[vertices[i + j]] += cross;
        else
          normalMap[vertices[i + j]] = cross;
      }
    }

    for (int i = 0; i < vertices.Length; i++)
    {
      Vector3 output;
      normalMap.TryGetValue(vertices[i], out output);
      normals[i] = output;
    }

    return normals;
  }

  void GenerateVertices()
  {
    // List<Vector3> verticesList = new List<Vector3>();
    this.vertices = new Vector3[(dimension - 1) * (dimension - 1) * 6];
    int index = 0;
    for (int y = 0; y < dimension - 1; y++)
    {
      for (int x = 0; x < dimension - 1; x++)
      {
        var pivot = (x, y);
        var top = (x, y + 1);
        var right = (x + 1, y);
        var diagonal = (x + 1, y + 1);
        (int, int)[] order = new (int, int)[] { pivot, top, diagonal, pivot, diagonal, right };
        foreach ((int vx, int vy) o in order)
          this.vertices[index++] = this.getVertice(o.vx, o.vy);
      }
    }
  }

  void GenerateTerrainHeights()
  {
    this.maxDimension = (int)(dimension / 2) + 1;
    this.minDimension = (int)(-dimension / 2);
    this.OFFSET = maxDimension - 1;
    this.vectorCounts = dimension * dimension;
    this.vectorArray = new Vector3[this.vectorCounts];
    this.rand = new System.Random();
    this.CornerGenerator();
    this.DiamondSquare();
  }

  // void PrintHeights()
  // {
  //   StringBuilder sb = new StringBuilder();
  //   for (int i = 0; i < dimension; i++)
  //   {
  //     for (int j = 0; j < dimension; j++)
  //     {
  //       sb.AppendFormat("{0,-10:0.##}", this.vectorArray[j][i].y);
  //     }
  //     sb.AppendLine();
  //   }
  //   Debug.Log(sb.ToString());
  // }

  float generateHeight(float baseHeight)
  {
    return baseHeight + HeightVariance * (float)(rand.NextDouble() * 2.0f - 1.0f);
  }
  void DiamondSquare()
  {
    System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
    int reach = dimension - 1;
    for (int size = dimension - 1; size > 1; size /= 2)
    {
      reach /= 2;
      this.diamondStep(reach, size);
      Debug.Log(watch.ElapsedMilliseconds);
      this.squareStep(reach, size);
      Debug.Log(watch.ElapsedMilliseconds);
      HeightVariance /= 2.0f;
    }
  }

  void diamondStep(int reach, int size)
  {
    for (int y = reach; y < dimension; y += size)
    {
      for (int x = reach; x < dimension; x += size)
      {
        float curSum = 0.0f;
        float numOfCorners = 0.0f;
        (int, int)[] coordinates = this.getDiamondPattern(x, y, reach);
        foreach ((int diamondX, int diamondY) c in coordinates)
        {
          if (this.IsWithinMap(c.diamondX, c.diamondY))
          {
            curSum += this.getVertice(c.diamondX, c.diamondY).y;
            numOfCorners++;
          }
        }
        // Debug.Log(String.Format("Assigning ({0}, {1}), size: {2}, reach: {3}", x, y, size, reach));
        this.assignHeight(x, y, this.generateHeight(curSum / numOfCorners));
      }
    }
  }

  void squareStep(int reach, int size)
  {
    Boolean even = true;
    for (int y = 0; y < dimension; y += reach)
    {
      for (int x = (even ? reach : 0); x < dimension; x += size)
      {
        float curSum = 0.0f;
        float numOfCorners = 0.0f;
        (int, int)[] coordinates = this.getSquarePattern(x, y, reach);
        foreach ((int squareX, int squareY) c in coordinates)
        {
          if (this.IsWithinMap(c.squareX, c.squareY))
          {

            curSum += this.getVertice(c.squareX, c.squareY).y;
            numOfCorners++;
          }
        }
        // Debug.Log(String.Format("Assigning ({0}, {1}), size: {2}, reach: {3}", x, y, size, reach));
        this.assignHeight(x, y, this.generateHeight(curSum / numOfCorners));
      }
      even = !even;
    }
  }

  (int, int)[] getDiamondPattern(int x, int y, int reach)
  {
    return new (int, int)[] { (x - reach, y - reach), (x + reach, y - reach), (x - reach, y + reach), (x + reach, y + reach) };
  }

  (int, int)[] getSquarePattern(int x, int y, int reach)
  {
    return new (int, int)[] { (x - reach, y), (x + reach, y), (x, y + reach), (x, y - reach) };
  }

  void CornerGenerator()
  {
    // this.assignHeight(0, 0, (float) rand.Next(15, 30));
    this.assignHeight(0, 0, this.GenerateRandom());
    this.assignHeight(0, dimension - 1, this.GenerateRandom());
    this.assignHeight(dimension - 1, 0, this.GenerateRandom());
    this.assignHeight(dimension - 1, dimension - 1, this.GenerateRandom());
    // this.assignHeight(dimension - 1, dimension - 1, (float)rand.Next(-30, -15));
  }

  float GenerateRandom()
  {
    // return (this.rand.Next(-10, 0) + (float)this.rand.NextDouble());
    // return rand.Next() % 5 < 1 ? this.rand.Next(MIN_HEIGHT, MAX_HEIGHT) + UnityEngine.Random.value : UnityEngine.Random.value;
    return UnityEngine.Random.value;
  }

  Boolean IsWithinMap(int x, int y)
  {
    return x >= 0 && x < dimension && y >= 0 && y < dimension;
  }


  void assignHeight(int x, int y, float value)
  {
    // this.vectorArray[y][x] = new Vector3((x - this.OFFSET) * STEP, value, (y - this.OFFSET) * STEP);
    this.vectorArray[y * dimension + x] = new Vector3((x - this.OFFSET) * STEP, value, (y - this.OFFSET) * STEP);
  }
  
  // Create the MeshCollider based on the generated Mesh
  void generateMeshCollider () {
    meshf = this.gameObject.GetComponent<MeshFilter>();
    meshc = this.gameObject.AddComponent<MeshCollider>() as MeshCollider;

    // Mesh Collider needs to be convex to allow for collision detection
    meshc.convex = true;
    meshc.sharedMesh = meshf.mesh;
  }

  Vector3 getVertice(int x, int y)
  {
    return this.vectorArray[y * dimension + x];
  }
}
