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
  public float step = 1.0f;
  private int maxDimension, minDimension;
  Dictionary<(int, int), float> heightMap = new Dictionary<(int, int), float>();
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
    triangles = new int[mesh.vertices.Length];
    colors = new Color[mesh.vertices.Length];

    for (int i = 0; i < mesh.vertices.Length; i++)
    {
      if (mesh.vertices[i].y > 0.7 * MAX_HEIGHT)
        colors[i] = Color.white;
      else if (mesh.vertices[i].y > 0.4 * MAX_HEIGHT)
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
    for (int y = minDimension; y < maxDimension - 1; y++)
    {
      for (int x = minDimension; x < maxDimension - 1; x++)
      {
        var pivot = (x, y);
        var top = (x, y + 1);
        var right = (x + 1, y);
        var diagonal = (x + 1, y + 1);
        (int, int)[] order = new (int, int)[] { pivot, top, diagonal, pivot, diagonal, right };
        foreach ((int vx, int vy) o in order)
        {
          verticesList.Add(new Vector3((float)o.vx, this.getHeight(o.vx, o.vy), (float)o.vy));
        }
      }
    }

    this.vertices = verticesList.ToArray();
  }

  void GenerateTerrainHeights()
  {
    this.maxDimension = (int)(dimension / (2 * step)) + 1;
    this.minDimension = (int)(-dimension / (2 * step));
    rand = new System.Random();
    this.CornerGenerator();
    this.DiamondSquare();
    // this.PrintHeights();
  }

  void PrintHeights()
  {
    float value;
    StringBuilder sb = new StringBuilder();
    for (int i = minDimension; i < maxDimension - 1; i++)
    {
      for (int j = minDimension; j < maxDimension - 1; j++)
      {
        if (this.heightMap.TryGetValue((i, j), out value))
        {
          sb.AppendFormat("{0,-10:0.##}", value);
        }
        else
        {
          sb.AppendFormat("     ");
        }
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
    Stack<(int, int)> diamondCoordinates = new Stack<(int, int)>();
    Stack<(int, int)> squareCoordinates = new Stack<(int, int)>();
    diamondCoordinates.Push((0, 0));
    int reach = maxDimension - 1;
    while (diamondCoordinates.Count != 0 || squareCoordinates.Count != 0)
    {
      this.DiamondStep(reach, ref diamondCoordinates, ref squareCoordinates);
      this.SquareStep(reach, ref squareCoordinates);
      reach /= 2;
    }
  }

  void DiamondStep(int reach, ref Stack<(int, int)> diamondCoordinates, ref Stack<(int, int)> squareCoordinates)
  {

    Queue<(int, int)> toCalculateNextDiamondCoordinates = new Queue<(int, int)>();

    while (diamondCoordinates.Count != 0)
    {
      float curSum = 0.0f;
      int numOfCorners = 0;
      var coordinate = diamondCoordinates.Pop();
      int x = coordinate.Item1;
      int y = coordinate.Item2;
      if (this.containsCoordinate(x, y)) continue;

      toCalculateNextDiamondCoordinates.Enqueue(coordinate);

      if (this.IsWithinMap(x - reach, y - reach))
      {
        curSum += this.getHeight(x - reach, y - reach);
        numOfCorners++;
      }

      if (this.IsWithinMap(x + reach, y - reach))
      {
        curSum += this.getHeight(x + reach, y - reach);
        numOfCorners++;
      }

      if (this.IsWithinMap(x - reach, y + reach))
      {
        curSum += this.getHeight(x - reach, y + reach);
        numOfCorners++;
      }

      if (this.IsWithinMap(x + reach, y + reach))
      {
        curSum += this.getHeight(x + reach, y + reach);
        numOfCorners++;
      }

      this.assignHeight(x, y, this.generateHeight(curSum / numOfCorners));
      this.addSquareCoordinates(x, y, reach, ref squareCoordinates);
    }

    while (toCalculateNextDiamondCoordinates.Count != 0)
    {
      var coordinate = toCalculateNextDiamondCoordinates.Dequeue();
      this.addDiamondCoordinates(coordinate.Item1, coordinate.Item2, reach, ref diamondCoordinates);
    }
  }
  void SquareStep(int reach, ref Stack<(int, int)> squareCoordinates)
  {
    while (squareCoordinates.Count != 0)
    {
      float curSum = 0.0f;
      int numOfCorners = 0;
      var coordinate = squareCoordinates.Pop();
      int x = coordinate.Item1;
      int y = coordinate.Item2;
      if (this.containsCoordinate(x, y)) continue;

      if (this.IsWithinMap(x - reach, y))
      {
        curSum += this.getHeight(x - reach, y);
        numOfCorners++;
      }

      if (this.IsWithinMap(x + reach, y))
      {
        curSum += this.getHeight(x + reach, y);
        numOfCorners++;
      }

      if (this.IsWithinMap(x, y + reach))
      {
        curSum += this.getHeight(x, y + reach);
        numOfCorners++;
      }

      if (this.IsWithinMap(x, y - reach))
      {
        curSum += this.getHeight(x, y - reach);
        numOfCorners++;
      }

      this.assignHeight(x, y, this.generateHeight(curSum / numOfCorners));
    }

  }

  void addDiamondCoordinates(int x, int y, int currentReach, ref Stack<(int, int)> diamondCoordinates)
  {
    int reach = currentReach / 2;

    if (this.IsWithinMap(x - reach, y - reach) && !this.containsCoordinate(x - reach, y - reach))
    {
      diamondCoordinates.Push((x - reach, y - reach));
    }

    if (this.IsWithinMap(x + reach, y - reach) && !this.containsCoordinate(x + reach, y - reach))
    {
      diamondCoordinates.Push((x + reach, y - reach));
    }

    if (this.IsWithinMap(x - reach, y + reach) && !this.containsCoordinate(x - reach, y + reach))
    {
      diamondCoordinates.Push((x - reach, y + reach));
    }

    if (this.IsWithinMap(x + reach, y + reach) && !this.containsCoordinate(x - reach, y - reach))
    {
      diamondCoordinates.Push((x + reach, y + reach));
    }
  }

  void addSquareCoordinates(int x, int y, int reach, ref Stack<(int, int)> squareCoordinates)
  {
    if (this.IsWithinMap(x - reach, y) && !this.containsCoordinate(x - reach, y))
    {
      squareCoordinates.Push((x - reach, y));
    }

    if (this.IsWithinMap(x + reach, y) && !this.containsCoordinate(x + reach, y))
    {
      squareCoordinates.Push((x + reach, y));
    }

    if (this.IsWithinMap(x, y + reach) && !this.containsCoordinate(x, y + reach))
    {
      squareCoordinates.Push((x, y + reach));
    }

    if (this.IsWithinMap(x, y - reach) && !this.containsCoordinate(x, y - reach))
    {
      squareCoordinates.Push((x, y - reach));
    }
  }
  void CornerGenerator()
  {
    this.assignHeight(minDimension, minDimension, this.GenerateRandom());
    this.assignHeight(minDimension, maxDimension - 1, this.GenerateRandom());
    this.assignHeight(maxDimension - 1, minDimension, this.GenerateRandom());
    this.assignHeight(maxDimension - 1, maxDimension - 1, this.GenerateRandom());
  }

  float GenerateRandom()
  {
    return this.rand.Next(MIN_HEIGHT, MAX_HEIGHT);
  }

  Boolean IsWithinMap(int x, int y)
  {
    return x >= minDimension && x < maxDimension && y >= minDimension && y < maxDimension;
  }


  Boolean assignHeight(int x, int y, float value)
  {
    if (this.containsCoordinate(x, y))
    {
      return false;
    }
    this.heightMap.Add((x, y), value);
    return true;
  }

  float getHeight(int x, int y)
  {
    float output;
    if (!this.heightMap.TryGetValue((x, y), out output))
      throw new System.InvalidOperationException("Accessing a coordinate that has not been assigned a height");
    return output;
  }

  Boolean containsCoordinate(int x, int y)
  {
    return this.heightMap.ContainsKey((x, y));
  }
}
