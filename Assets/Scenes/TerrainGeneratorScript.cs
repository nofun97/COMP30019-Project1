using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class TerrainGeneratorScript : MonoBehaviour
{
  public static int MAX_HEIGHT = 50, MIN_HEIGHT = 1, step = 1, dimension = 5;
  public static int arrayDimension = dimension / step;
  double[][] heightMap = new double[arrayDimension][];

  // Start is called before the first frame update
  void Start()
  {
    for (int i = 0; i < arrayDimension; i++)
    {
      heightMap[i] = new double[arrayDimension];
    }
    this.InitHeights();
    this.CornerGenerator();
    // this.PrintHeights();
    this.DiamondSquare(arrayDimension / 2);
    this.PrintHeights();
  }

  void InitHeights()
  {
    for (int i = 0; i < arrayDimension; i++)
    {
      for (int j = 0; j < arrayDimension; j++)
      {
        this.heightMap[i][j] = 0;
      }
    }
  }
  void PrintHeights()
  {
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < arrayDimension; i++)
    {
      for (int j = 0; j < arrayDimension; j++)
      {
        sb.AppendFormat("{0:n2} ", this.heightMap[i][j]);
      }
      sb.AppendLine();
    }
    Debug.Log(sb.ToString());
  }

  // Update is called once per frame
  void DiamondSquare(int half)
  {
    int arrayStep = half / 2;
    if (arrayStep < 1) return;

    for (int x = arrayStep; x < arrayDimension; x += half)
      for (int y = arrayStep; y < arrayDimension; y += half)
        this.DiamondStep(x, y, arrayStep);

    int column = 0;
    for (int x = 0; x < arrayDimension; x += arrayStep)
    {
      column++;
      if (column % 2 == 0)
        for (int y = 0; y < arrayDimension; y += arrayStep)
          this.SquareStep(x, y, arrayStep);

      for (int y = arrayStep; y < arrayDimension; y += arrayStep)
        this.SquareStep(x, y, arrayStep);
    }
    // this.PrintHeights();
    this.DiamondSquare(half / 2);
  }

  void SquareStep(int x, int y, int reach)
  {
    double curSum = 0.0f;
    int numOfCorners = 0;

    if (this.IsWithinMap(x - reach, y))
    {
      curSum += this.heightMap[x - reach][y];
      numOfCorners++;
    }

    if (this.IsWithinMap(x + reach, y))
    {
      curSum += this.heightMap[x + reach][y];
      numOfCorners++;
    }

    if (this.IsWithinMap(x, y + reach))
    {
      curSum += this.heightMap[x][y + reach];
      numOfCorners++;
    }

    if (this.IsWithinMap(x, y + reach))
    {
      curSum += this.heightMap[x][y + reach];
      numOfCorners++;
    }

    this.heightMap[x][y] = curSum / numOfCorners; // + this.GenerateRandom();
    // this.PrintHeights();
  }

  void DiamondStep(int x, int y, int reach)
  {
    double curSum = 0.0f;
    int numOfCorners = 0;

    if (this.IsWithinMap(x - reach, y - reach))
    {
      curSum += this.heightMap[x - reach][y - reach];
      numOfCorners++;
    }

    if (this.IsWithinMap(x + reach, y - reach))
    {
      curSum += this.heightMap[x + reach][y - reach];
      numOfCorners++;
    }

    if (this.IsWithinMap(x - reach, y + reach))
    {
      curSum += this.heightMap[x - reach][y + reach];
      numOfCorners++;
    }

    if (this.IsWithinMap(x + reach, y + reach))
    {
      curSum += this.heightMap[x + reach][y + reach];
      numOfCorners++;
    }

    this.heightMap[x][y] = curSum / numOfCorners; // + this.GenerateRandom();
  }
  void CornerGenerator()
  {
    heightMap[0][0] = this.GenerateRandom();
    heightMap[0][arrayDimension - 1] = this.GenerateRandom();
    heightMap[arrayDimension - 1][0] = this.GenerateRandom();
    heightMap[arrayDimension - 1][arrayDimension - 1] = this.GenerateRandom();
  }

  double GenerateRandom()
  {
    System.Random rand = new System.Random();
    return rand.Next(MIN_HEIGHT, MAX_HEIGHT);
  }

  Boolean IsWithinMap(int x, int y)
  {
    return x >= 0 && x < arrayDimension && y >= 0 && y < arrayDimension;
  }
}
