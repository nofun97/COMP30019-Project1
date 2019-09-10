using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float cameraSpeed = 2.5f;
    public float yawSpeed = 200.0f;
    public float pitchSpeed = 200.0f;
    private int safeguard = 1;
    private float startingPitch = 40.0f;
    private float startingYaw = 4.0f;
    private float positionX = 7.0f;
    private float positionY = 50.0f;
    private float positionZ = -28.0f;
    private float pitchDegree = 60.0f;
    private float yawDegree = -10.0f;
    private float maxHeight = 75.0f;
    private float boundarySize;
    //Rigidbody cameraRigidbody;
    //GameObject camera;
    GameObject plane;
    TerrainGeneratorScript terrainScript;
    CursorLockMode cursorMode;

    // Start is called before the first frame update
    void Start()
    {
        // retrieve the dimensions of the plane to dynamically bound the camera later on
        plane = GameObject.Find("Plane");
        terrainScript = plane.GetComponent<TerrainGeneratorScript>();
        boundarySize = terrainScript.dimension/2;
        
        // Set a suitable starting rotation and position for the camera to view the scene
        this.transform.eulerAngles = new Vector3(startingPitch, startingYaw, 0.0f);
        this.transform.position = new Vector3(positionX, positionY, positionZ);
        
        /* Set the cursor to be invisible while playing. Press "ESC" to show the cursor again, and click the screen 
        to make it disappear again */
        cursorMode = CursorLockMode.Locked;
        Cursor.lockState = cursorMode;

        // Add a rigidbody (without gravity) to simulate collision with the landscape
        //camera = GameObject.Find("Main Camera");
        //cameraRigidbody = camera.AddComponent<Rigidbody>();
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
                BoundCamera(this.transform.position, "Right");
            }
            if (Input.GetAxisRaw("Horizontal") <= 0) {
                this.transform.position = transform.position - Camera.main.transform.right * cameraSpeed * Time.deltaTime;
                BoundCamera(this.transform.position, "Left");
            }
            if (Input.GetAxisRaw("Vertical") >= 0) {
                this.transform.position = transform.position + Camera.main.transform.forward * cameraSpeed * Time.deltaTime;
                BoundCamera(this.transform.position, "Forward");
            }
            if (Input.GetAxisRaw("Vertical") <= 0) {
                this.transform.position = transform.position - Camera.main.transform.forward * cameraSpeed * Time.deltaTime;
                BoundCamera(this.transform.position, "Backward");
            }

            // Change the pitch and yaw of the camera based on the mouse movements
            pitchDegree -= pitchSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;
            yawDegree += yawSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
            this.transform.eulerAngles = new Vector3(pitchDegree, yawDegree, 0.0f);
        }
    }

    // Bound the camera within the plane dimensions, through using "a postierori" collision detection
    void BoundCamera(Vector3 position, String input)
    {
        // If the camera exceeds the plane dimensions or goes above the max height, revert its position
        if (this.transform.position.x >= boundarySize || this.transform.position.x <= -boundarySize || 
        this.transform.position.z >= boundarySize || this.transform.position.z <= -boundarySize || 
        this.transform.position.y >= maxHeight || this.transform.position.y <= -maxHeight) {
            if (input == "Right") {
                this.transform.position = transform.position - Camera.main.transform.right * cameraSpeed * Time.deltaTime;
            }
            if (input == "Left") {
                this.transform.position = transform.position + Camera.main.transform.right * cameraSpeed * Time.deltaTime;
            }
            if (input == "Forward") {
                this.transform.position = transform.position - Camera.main.transform.forward * cameraSpeed * Time.deltaTime;
            }
            if (input == "Backward") {
                this.transform.position = transform.position + Camera.main.transform.forward * cameraSpeed * Time.deltaTime;
            }
        }
    }
}