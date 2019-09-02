using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class TerrainGeneratorScript : MonoBehaviour
{
  private Vector3[] vertices;
  private int[] triangles;

  private Color[] colors;
  public int MAX_HEIGHT = 50, MIN_HEIGHT = -30, dimension = 5;
  public int MAX_ADD_HEIGHT = 2, MAX_SUBTRACT_HEIGHT = -5;
  private int maxDimension, minDimension, OFFSET;
  private float highestPeak;
  Vector3[][] heightMap;
  private System.Random rand;
  // Start is called before the first frame update
  void Start()
  {
    this.GenerateTerrainHeights();
    this.GenerateMesh();
  }

  void GenerateMesh()
  {
    Mesh mesh = GetComponent<MeshFilter>().mesh;
    this.GenerateVertices();
    mesh.vertices = vertices;
    // mesh.uv = newUV;
    this.triangles = new int[mesh.vertices.Length];
    this.colors = new Color[mesh.vertices.Length];

    for (int i = 0; i < mesh.vertices.Length; i++)
    {
      if (mesh.vertices[i].y > 0.5 * highestPeak)
        colors[i] = Color.white;
      else if (mesh.vertices[i].y > 0.2 * highestPeak)
        colors[i] = Color.green;
      else
        colors[i] = Color.blue;
    }
    mesh.colors = this.colors;
    for (int i = 0; i < mesh.vertices.Length; i++)
      triangles[i] = i;
    mesh.triangles = this.triangles;
  }

  void GenerateVertices()
  {
    List<Vector3> verticesList = new List<Vector3>();
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
        {
          verticesList.Add(this.getVertice(o.vx, o.vy));
        }
      }
    }

    this.vertices = verticesList.ToArray();
  }

  void GenerateTerrainHeights()
  {
    this.maxDimension = (int)(dimension / 2) + 1;
    this.minDimension = (int)(-dimension / 2);
    this.OFFSET = maxDimension - 1;
    this.highestPeak = MIN_HEIGHT;
    this.heightMap = new Vector3[dimension][];
    for (int i = 0; i < dimension; i++)
      this.heightMap[i] = new Vector3[dimension];
    rand = new System.Random();
    this.CornerGenerator();
    // this.PrintHeights();
    this.DiamondSquare();
    // this.PrintHeights();
  }

  void PrintHeights()
  {
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < dimension; i++)
    {
      for (int j = 0; j < dimension; j++)
      {
        sb.AppendFormat("{0,-10:0.##}", this.heightMap[j][i].y);
      }
      sb.AppendLine();
    }
    Debug.Log(sb.ToString());
  }

  float generateHeight(float baseHeight)
  {
    float height = baseHeight + this.rand.Next(MAX_SUBTRACT_HEIGHT, MAX_ADD_HEIGHT);
    if (height > MAX_HEIGHT)
    {
      height = MAX_HEIGHT;
    }
    else if (height < MIN_HEIGHT)
    {
      height = MIN_HEIGHT;
    }
    return height;
  }
  void DiamondSquare()
  {
    int reach = dimension - 1;
    for (int size = dimension - 1; size > 1; size /= 2)
    {
      reach /= 2;
      this.diamondStep(reach, size);
      this.squareStep(reach, size);
    }
  }

  void diamondStep(int reach, int size)
  {
    for (int y = reach; y < dimension; y += size)
    {
      for (int x = reach; x < dimension; x += size)
      {
        float curSum = 0.0f;
        int numOfCorners = 0;
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
        int numOfCorners = 0;
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
    this.assignHeight(0, 0, this.GenerateRandom());
    this.assignHeight(0, dimension - 1, this.GenerateRandom());
    this.assignHeight(dimension - 1, 0, this.GenerateRandom());
    this.assignHeight(dimension - 1, dimension - 1, this.GenerateRandom());
  }

  float GenerateRandom()
  {
    return this.rand.Next(MIN_HEIGHT, MAX_HEIGHT);
  }

  Boolean IsWithinMap(int x, int y)
  {
    return x >= 0 && x < dimension && y >= 0 && y < dimension;
  }


  void assignHeight(int x, int y, float value)
  {
    this.heightMap[y][x] = new Vector3(x - this.OFFSET, value, y - this.OFFSET);
    if (value > this.highestPeak)
    {
      this.highestPeak = value;
    }
  }

  Vector3 getVertice(int x, int y)
  {
    if (!this.containsCoordinate(x, y))
      throw new System.InvalidOperationException("Accessing a coordinate that has not been assigned a height");
    return this.heightMap[y][x];
  }

  Boolean containsCoordinate(int x, int y)
  {
    return this.heightMap[y][x] != null;
  }
}
