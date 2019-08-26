using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class TerrainGeneratorScript : MonoBehaviour
{
  public int MAX_HEIGHT = 50, MIN_HEIGHT = 1, dimension = 5;
  public int MAX_ADD_HEIGHT = 2, MAX_SUBTRACT_HEIGHT = -5;
  public float step = 0.5f;
  private int arrayDimension;
  Dictionary<(int, int), float> heightMap = new Dictionary<(int, int), float>();
  private System.Random rand;
  // Start is called before the first frame update
  void Start()
  {
    this.arrayDimension = (int)(dimension / step);
    rand = new System.Random();
    this.CornerGenerator();
    this.DiamondSquare();
    this.PrintHeights();
  }
  void PrintHeights()
  {
    float value;
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < arrayDimension; i++)
    {
      for (int j = 0; j < arrayDimension; j++)
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
    diamondCoordinates.Push((arrayDimension / 2, arrayDimension / 2));
    int reach = arrayDimension;
    while (diamondCoordinates.Count != 0 || squareCoordinates.Count != 0)
    {
      reach /= 2;
      this.DiamondStep(reach, ref diamondCoordinates, ref squareCoordinates);
      this.SquareStep(reach, ref squareCoordinates);
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
    this.assignHeight(0, 0, this.GenerateRandom());
    this.assignHeight(0, arrayDimension - 1, this.GenerateRandom());
    this.assignHeight(arrayDimension - 1, 0, this.GenerateRandom());
    this.assignHeight(arrayDimension - 1, arrayDimension - 1, this.GenerateRandom());
  }

  float GenerateRandom()
  {
    return this.rand.Next(MIN_HEIGHT, MAX_HEIGHT);
  }

  Boolean IsWithinMap(int x, int y)
  {
    return x >= 0 && x < arrayDimension && y >= 0 && y < arrayDimension;
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
