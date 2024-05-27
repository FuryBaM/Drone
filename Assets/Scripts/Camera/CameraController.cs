using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _target; // Target to follow, typically the drone
    [SerializeField] private float _distance = 5.0f; // Distance from the target
    [SerializeField] private float _xSpeed = 120.0f; // Speed of rotation around the x-axis
    [SerializeField] private float _ySpeed = 120.0f; // Speed of rotation around the y-axis
    [SerializeField] private float _yMinLimit = -20f; // Minimum y-axis angle
    [SerializeField] private float _yMaxLimit = 80f;  // Maximum y-axis angle

    private float x = 0.0f;
    private float y = 0.0f;

    private void Start()
    {
        // Initialize the angles based on the current rotation of the camera
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }
    }

    private void LateUpdate()
    {
        if (_target)
        {
            // Get input from Input System
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * Time.deltaTime;
            x += mouseDelta.x * _xSpeed;
            y -= mouseDelta.y * _ySpeed;

            // Clamp the y value within the min and max limits
            y = Mathf.Clamp(y, _yMinLimit, _yMaxLimit);

            // Create rotation quaternion based on x and y values
            Quaternion rotation = Quaternion.Euler(y, x, 0);

            // Calculate position based on the rotation and the distance from the target
            Vector3 position = rotation * new Vector3(0.0f, 0.0f, -_distance) + _target.position;

            // Set the rotation and position of the camera
            transform.rotation = rotation;
            transform.position = position;
        }
    }
}
