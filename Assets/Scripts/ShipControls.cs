using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipControls : MonoBehaviour
{
    [SerializeField] private float _rotSpeed = 4f;
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _currentSpeed;
    private float _vertical;
    private float _horizontal;
    [SerializeField] private float _maxRotate;
    [SerializeField] private GameObject _shipModel;

    [Header("Acceleration Settings")]
    [SerializeField] private float acceleration = 4f;  // Rate of speed increase
    [SerializeField] private float deceleration = 4f;  // Rate of speed decrease
    [SerializeField] private float maxSpeed = 4f;       // Maximum speed limit

    // Add this flag to control player input
    [HideInInspector]
    public bool allowPlayerControl = true;
    [SerializeField] private float autoForwardSpeed = 2f; // Speed when in cockpit mode

    void Start()
    {
        _currentSpeed = 1;
    }

    void Update()
    {
        ShipMovement();
    }

    private void ShipMovement()
    {
        if (allowPlayerControl)
        {
            // Use W, A, S, D for movement on X and Y axes only
            float moveVertical = 0f;  // For up/down movement (Y axis)
            float moveHorizontal = 0f; // For left/right movement (X axis)
            if (Input.GetKey(KeyCode.W)) moveVertical = 1f;   // Up (Y+)
            if (Input.GetKey(KeyCode.S)) moveVertical = -1f;  // Down (Y-)
            if (Input.GetKey(KeyCode.D)) moveHorizontal = 1f; // Right (X+)
            if (Input.GetKey(KeyCode.A)) moveHorizontal = -1f; // Left (X-)

            // Use arrow keys for rotation
            _vertical = Input.GetAxis("Vertical");  // Pitch (up/down arrow)
            _horizontal = Input.GetAxis("Horizontal"); // Yaw (left/right arrow)

            // Accelerate with "T" and decelerate with "G" continuously
            if (Input.GetKey(KeyCode.T))
            {
                _currentSpeed += acceleration * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.G))
            {
                _currentSpeed -= deceleration * Time.deltaTime;
            }
            else
            {
                // Gradual deceleration when no input
                _currentSpeed = Mathf.Lerp(_currentSpeed, 0f, deceleration * Time.deltaTime * 0.1f);
            }

            // Clamp speed between 1 and maxSpeed
            _currentSpeed = Mathf.Clamp(_currentSpeed, 1f, maxSpeed);

            // Rotate horizontally (yaw)
            Vector3 rotateH = new Vector3(0, _horizontal, 0);
            transform.Rotate(rotateH * _rotSpeed * Time.deltaTime);

            // Rotate vertically (pitch)
            Vector3 rotateV = new Vector3(_vertical, 0, 0);
            transform.Rotate(rotateV * _rotSpeed * Time.deltaTime);

            // Roll based on horizontal input
            transform.Rotate(new Vector3(0, 0, -_horizontal * 0.2f), Space.Self);

            // Move forward based on current speed (Z+)
            transform.position += transform.forward * _currentSpeed * Time.deltaTime;

            // Move on X (left/right) and Y (up/down) axes only with WASD
            transform.position += new Vector3(moveHorizontal * _moveSpeed * Time.deltaTime, 
                                            moveVertical * _moveSpeed * Time.deltaTime, 
                                            0f);
        }
        else
        {
            // In cockpit mode: move forward automatically at a constant speed
            transform.position += transform.forward * autoForwardSpeed * Time.deltaTime;
        }
    }
}