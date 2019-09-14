using UnityEngine;
using System.Collections;

// Defines the point light
public class PointLight : MonoBehaviour {

    public Color color;

    public Vector3 GetWorldPosition()
    {
        return this.transform.position;
    }
}
