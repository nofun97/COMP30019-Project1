using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float cameraSpeed = 2.0f;
    private float yawDegree = 0.0f;
    private float pitchDegree = 0.0f;
    public float yawSpeed = 500.0f;
    public float pitchSpeed = 500.0f;

    // Update is called once per frame
    void Update()
    {
        // Move the camera based on its direction and the WASD keys, incuding diagonal movements
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

        // Change the pitch and yaw of the camera based on the mouse movements
        yawDegree += yawSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
        pitchDegree -= pitchSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;
        this.transform.eulerAngles = new Vector3(pitchDegree, yawDegree, 0.0f);
    }
}
