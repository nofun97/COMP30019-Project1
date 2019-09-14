using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines the rotation of the sun
public class SunRotation : MonoBehaviour
{
  public float spinSpeed;

  // Update is called once per frame
  void Update()
  {
    this.transform.localRotation *= Quaternion.AngleAxis(spinSpeed * Time.deltaTime, Vector3.forward);
  }
}
