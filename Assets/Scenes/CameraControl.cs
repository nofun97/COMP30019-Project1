using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float cameraSpeed = 5.0f;
    public float yawSpeed = 200.0f;
    public float pitchSpeed = 200.0f;
    private int safeguard = 1;
    private float startingPitch = 40.0f;
    private float startingYaw = 4.0f;
    private float positionX = -4.0f;
    private float positionY = 75.0f;
    private float positionZ = -67.0f;
    private float pitchDegree = 40.0f;
    private float yawDegree = 4.0f;
    CursorLockMode cursorMode;

    // Start is called before the first frame update
    void Start()
    {
        // Set a suitable starting rotation and position for the camera to view the scene
        this.transform.eulerAngles = new Vector3(startingPitch, startingYaw, 0.0f);
        this.transform.position = new Vector3(positionX, positionY, positionZ);
        
        /* Set the cursor to be invisible while playing. Press "ESC" to show the cursor again, and click the screen 
        to make it disappear again */
        cursorMode = CursorLockMode.Locked;
        Cursor.lockState = cursorMode;
    }
    
    // Update is called once per frame
    void Update()
    {
        /* Allow camera movement only after the scene has been fully rendered plus one second (safeguard) to prevent 
        accidentally displacing the initial camera position while the scene is still loading */
        if (Time.time > safeguard) {

            // Move the camera based on its direction using the WASD keys, including diagonal movements
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
            pitchDegree -= pitchSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;
            yawDegree += yawSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
            this.transform.eulerAngles = new Vector3(pitchDegree, yawDegree, 0.0f);
        }
    }
}