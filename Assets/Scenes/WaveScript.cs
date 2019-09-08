using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveScript : MonoBehaviour
{

    private int dimension = 65;
    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.vertices = new Vector3[]{
            new Vector3(-31.0f, 0.0f, -31.0f),
            new Vector3(-31.0f, 0.0f, 31.0f),
            new Vector3(31.0f, 0.0f, 31.0f),
            new Vector3(-31.0f, 0.0f, -31.0f),
            new Vector3(31.0f, 0.0f, 31.0f),
            new Vector3(31.0f, 0.0f, -31.0f),
        };
        mesh.triangles = new int[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
            mesh.triangles[i] = i;
    }

}
