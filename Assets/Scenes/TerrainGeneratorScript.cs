using UnityEngine;
using System;
using System.Collections.Generic;

public class TerrainGeneratorScript : MonoBehaviour
{
  // To store light data
  public PointLight pointLight;

  // dimension of the plane
  public int dimension = 5;

  // the horizontal distance of each points
  public float STEP = 1.0f;

  // the variance of height that is added to the average height
  public float HeightVariance = 30;

  // store vertices for mesh
  private Vector3[] vertices;

  // store the arrays of vectors for the plane
  private Vector3[] vectorArray;

  // offset for each point so that plane is generated in the middle
  private int OFFSET;

  // for each grid, there are 6 points needed to be defined
  private const int POINTS_PER_GRID = 6;

  // random number generator
  private System.Random rand;

  // Start is called before the first frame update
  void Start()
  {
    this.generateTerrainHeights();
    this.generateMesh();
    this.generateMeshCollider();
  }

  // Update is called for every frame
  void Update()
  {
    // Get renderer component (in order to pass params to shader)
    MeshRenderer renderer = this.gameObject.GetComponent<MeshRenderer>();

    // Pass updated light positions to shader
    renderer.material.SetColor("_PointLightColor", this.pointLight.color);
    renderer.material.SetVector("_PointLightPosition", this.pointLight.GetWorldPosition());
  }

  /**
   * generateMesh generates mesh info and assigns it to the game object mesh
   */
  void generateMesh()
  {
    Mesh mesh = new Mesh();
    mesh.name = "Terrain";

    // assigning vertices
    this.generateVertices();
    mesh.vertices = vertices;

    // assigning triangles
    var triangles = new int[mesh.vertices.Length];
    for (int i = 0; i < mesh.vertices.Length; i++)
      triangles[i] = i;
    mesh.triangles = triangles;

    // assigning normals
    mesh.normals = this.generateNormals(mesh.vertices.Length);

    // attaching the mesh to the object
    this.gameObject.GetComponent<MeshFilter>().mesh = mesh;
  }

  /**
   * generate normals based on the generated vertices and returns a list
   * of normals for the mesh
   */
  Vector3[] generateNormals(int length)
  {
    // record the normals for each vertice
    Vector3[] normals = new Vector3[length];

    // record the resultant force of each vertice's normal
    Dictionary<Vector3, Vector3> normalMap = new Dictionary<Vector3, Vector3>();

    // iterating through each grid
    for (int i = 0; i < vertices.Length; i += POINTS_PER_GRID)
    {
      Vector3 side1, side2, diagonal, cross1, cross2;

      // the vertical unit vector from top down view ((x, y) and (x, y + STEP))
      side1 = (vertices[i + 1] - vertices[i]).normalized;

      // the horizontal unit vector from top down view ((x, y) and (x + STEP, y))
      side2 = (vertices[i + 5] - vertices[i]).normalized;

      // the diagonal unit vector from top down view ((x, y) and (x + STEP, y + STEP))
      diagonal = (vertices[i + 2] - vertices[i]).normalized;

      // normal for two polygons (two polygons in each grid)
      cross1 = Vector3.Cross(side1, diagonal).normalized;
      cross2 = Vector3.Cross(diagonal, side2).normalized;

      // Sum the normals for each vertice in a grid
      for (int j = 0; j < POINTS_PER_GRID; j++)
      {
        Vector3 cross = j < 3 ? cross1 : cross2;
        if (normalMap.ContainsKey(vertices[i + j]))
          normalMap[vertices[i + j]] += cross;
        else
          normalMap[vertices[i + j]] = cross;
      }
    }

    // assign each normal to each vertice accordingly
    for (int i = 0; i < vertices.Length; i++)
    {
      Vector3 output;
      normalMap.TryGetValue(vertices[i], out output);
      normals[i] = output;
    }

    return normals;
  }

  /**
   * generateVertices assign the vertices in the appropriate location
   */
  void generateVertices()
  {
    // number of vertices is calculated by calculating number of grids and multiplied by POINTS_PER_GRID
    this.vertices = new Vector3[(dimension - 1) * (dimension - 1) * POINTS_PER_GRID];

    // iterate through each point
    int index = 0;
    for (int y = 0; y < dimension - 1; y++)
    {
      for (int x = 0; x < dimension - 1; x++)
      {
        // adding the points in certain order which is pivot, top, diagonal, pivot, diagonal, right
        this.vertices[index++] = this.getVertice(x, y);
        this.vertices[index++] = this.getVertice(x, y + 1);
        this.vertices[index++] = this.getVertice(x + 1, y + 1);
        this.vertices[index++] = this.getVertice(x, y);
        this.vertices[index++] = this.getVertice(x + 1, y + 1);
        this.vertices[index++] = this.getVertice(x + 1, y);
      }
    }
  }

  /**
   * generateTerrainHeights is the function to initialize value before running
   * the diamond square algorithm
   */
  void generateTerrainHeights()
  {
    this.OFFSET = (int)(dimension / 2);
    this.vectorArray = new Vector3[dimension * dimension];
    this.rand = new System.Random();
    this.cornerGenerator();
    this.diamondSquare();
  }

  /**
   * generateHeight adds the random number to the average height and returns
   * the height to assign at a point
   */
  float generateHeight(float baseHeight)
  {
    // the height variance is multiplied by value between [-1, 1] for randomization
    return baseHeight + HeightVariance * (float)(rand.NextDouble() * 2.0f - 1.0f);
  }

  /**
   * diamondSquare runs the diamond square algorithm
   */
  void diamondSquare()
  {
    // the distance of points to reach from a point
    int reach = dimension - 1;
    // iterate through each segment of diamond square
    for (int size = dimension - 1; size > 1; size /= 2)
    {
      // run the diamond step and square step while halving the reach
      reach /= 2;
      this.diamondStep(reach, size);
      this.squareStep(reach, size);

      // reducing height variance to lessen the randomization each iteration
      HeightVariance *= 0.5f;
    }
  }

  /**
   * diamondStep runs the diamond step of the diamond square algorithm
   */
  void diamondStep(int reach, int size)
  {
    // iterate through each diamond points for each segment
    for (int y = reach; y < dimension; y += size)
    {
      for (int x = reach; x < dimension; x += size)
      {
        float curSum = 0.0f;
        float numOfCorners = 0.0f;

        // check values diagonally
        if (this.isWithinArray(x - reach, y - reach)){
          curSum += this.getVertice(x - reach, y - reach).y;
          numOfCorners++;
        }
        if (this.isWithinArray(x + reach, y - reach)){
          curSum += this.getVertice(x + reach, y - reach).y;
          numOfCorners++;
        }
        if (this.isWithinArray(x - reach, y + reach)){
          curSum += this.getVertice(x - reach, y + reach).y;
          numOfCorners++;
        }
        if (this.isWithinArray(x + reach, y + reach)){
          curSum += this.getVertice(x + reach, y + reach).y;
          numOfCorners++;
        }

        // assigns the vector to the vertice
        this.assignVertice(x, y, this.generateHeight(curSum / numOfCorners));
      }
    }
  }

  /**
   * squareStep runs the square step of the diamond square algorithm
   */
  void squareStep(int reach, int size)
  {
    // iterate through each square points for each segment
    Boolean even = true;
    for (int y = 0; y < dimension; y += reach)
    {
      for (int x = (even ? reach : 0); x < dimension; x += size)
      {
        float curSum = 0.0f;
        float numOfCorners = 0.0f;

        // calculate the points on top, bottom, left, and right from each square point
        if (this.isWithinArray(x - reach, y)){
          curSum += this.getVertice(x - reach, y).y;
          numOfCorners++;
        }
        if (this.isWithinArray(x + reach, y)){
          curSum += this.getVertice(x + reach, y).y;
          numOfCorners++;
        }
        if (this.isWithinArray(x, y + reach)){
          curSum += this.getVertice(x, y + reach).y;
          numOfCorners++;
        }
        if (this.isWithinArray(x, y - reach)){
          curSum += this.getVertice(x, y - reach).y;
          numOfCorners++;
        }

        // assigns vertices
        this.assignVertice(x, y, this.generateHeight(curSum / numOfCorners));
      }
      even = !even;
    }
  }

  /**
   * cornerGenerator generates random values between 0 to 1 for each corner
   */
  void cornerGenerator()
  {
    /**
     * values are from 0 to 1 so that there is no too high or too low height in the corner
     * this is also to avoid generating just a slope in the plane
     */
    this.assignVertice(0, 0, UnityEngine.Random.value);
    this.assignVertice(0, dimension - 1, UnityEngine.Random.value);
    this.assignVertice(dimension - 1, 0, UnityEngine.Random.value);
    this.assignVertice(dimension - 1, dimension - 1, UnityEngine.Random.value);
  }

  /**
   * isWithinArray checks if a index is within array
   * returns true if value is within array and false otherwise
   */
  Boolean isWithinArray(int x, int y)
  {
    return (y * dimension + x) >= 0 && (y * dimension + x) < dimension * dimension;
  }

  /**
   * assignVertice adds the vector to the corresponding index in the vectorArray
   */
  void assignVertice(int x, int y, float value)
  {
    /**
     * vectorArray is a 1-dimensional array and it requires index calculation
     * 1-dimensional array is chosen for better performance
     */
    this.vectorArray[(y * dimension + x)] = new Vector3((x - this.OFFSET) * STEP, value, (y - this.OFFSET) * STEP);
  }

  /**
   * getVertice gets the vector in an index and returns the vector in that index
   */
  Vector3 getVertice(int x, int y)
  {
    return this.vectorArray[(y * dimension + x)];
  }

  /**
   * Create the MeshCollider based on the generated Mesh
   */
  void generateMeshCollider()
  {
    MeshFilter meshf;
    MeshCollider meshc;
    meshf = this.gameObject.GetComponent<MeshFilter>();
    meshc = this.gameObject.AddComponent<MeshCollider>();
    meshc.convex = false;
    meshc.sharedMesh = meshf.mesh;
  }
}
