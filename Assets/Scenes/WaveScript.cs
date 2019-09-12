using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveScript : MonoBehaviour
{
  public PointLight pointLight;
  // Start is called before the first frame update
  void Start()
  {
    // generateNormals();
    // Debug.Log(this.gameObject.GetComponent<MeshFilter>().mesh.vertices.Length);
    // generateNormals();
  }

  // Update is called once per frame
  void Update()
  {
    // generateNormals();
    // Get renderer component (in order to pass params to shader)
    MeshRenderer renderer = this.gameObject.GetComponent<MeshRenderer>();

    // Pass updated light positions to shader
    renderer.material.SetColor("_PointLightColor", this.pointLight.color);
    renderer.material.SetVector("_PointLightPosition", this.pointLight.GetWorldPosition());
  }

  // void generateNormals()
  // {
  //   Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
  //   Vector3[] normals = new Vector3[mesh.vertices.Length];
  //   Dictionary<Vector3, Vector3> normalMap = new Dictionary<Vector3, Vector3>();
  //   for (int i = 0; i < mesh.vertices.Length; i += 6)
  //   {
  //     // normals[i] = Vector3.up;
  //     Vector3 side1, side2, diagonal, cross1, cross2;
  //     Debug.Log(i);
  //     side1 = (mesh.vertices[i + 1] - mesh.vertices[i]).normalized;
  //     side2 = (mesh.vertices[i + 5] - mesh.vertices[i]).normalized;
  //     diagonal = (mesh.vertices[i + 2] - mesh.vertices[i]).normalized;
  //     cross1 = Vector3.Cross(side1, diagonal).normalized;
  //     cross2 = Vector3.Cross(diagonal, side2).normalized;

  //     for (int j = 0; j < 6; j++)
  //     {
  //       Vector3 cross = j < 3 ? cross1 : cross2;
  //       if (normalMap.ContainsKey(mesh.vertices[i + j]))
  //         normalMap[mesh.vertices[i + j]] += cross;
  //       else
  //         normalMap[mesh.vertices[i + j]] = cross;
  //     }
  //   }

  //   for (int i = 0; i < mesh.vertices.Length; i++)
  //   {
  //     Vector3 output;
  //     normalMap.TryGetValue(mesh.vertices[i], out output);
  //     normals[i] = output;
  //   }

  //   mesh.normals = normals;
  // }
}
