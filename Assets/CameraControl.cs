using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float cameraSpeed = 1.0f;

    // Update is called once per frame
    void Update()
    {
        // Move the camera based on its direction and the WASD keys, including diagonal inputs
        if (Input.GetAxisRaw("Horizontal") >= 0) {
            this.transform.position = transform.position + Camera.main.transform.right * cameraSpeed * Time.deltaTime;
        }
        if (Input.GetAxisRaw("Horizontal") <= 0) {
            this.transform.position = transform.position - Camera.main.transform.right * cameraSpeed * Time.deltaTime;
        }
        if (Input.GetAxisRaw("Vertical") >= 0) {
            this.transform.position = transform.position + Camera.main.transform.forward * cameraSpeed * Time.deltaTime;
        }
        if (Input.GetAxisRaw("Vertical") <= 0) {
            this.transform.position = transform.position - Camera.main.transform.forward * cameraSpeed * Time.deltaTime;
        }
    }
}
