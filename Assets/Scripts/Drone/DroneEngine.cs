using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DroneEngine : MonoBehaviour, IEngine 
{
    [Header("Engine Properties")]
    [SerializeField] private float _motorKV = 2300f;
    [SerializeField] private float _batteryVoltage = 11.1f;
    [SerializeField] private float _batteryCurrent = 6;
    [SerializeField] private float _propellerRadius = 0.1f; // Propeller radius in meters
    [SerializeField] private bool _isClockwised = true;
    [SerializeField] private Transform _propeller;

    [Header("Aerodynamics")]
    [SerializeField] private float _airDensity = 1.225f; // Air density in kg/m^3

    [SerializeField] private float _thrust; // Engine thrust
    [SerializeField] private float _angularVelocity = 0f;
    [SerializeField] private float _currentRPM;
    [SerializeField] private float _torque = 0;
    [SerializeField] private float _airVelocity = 0f;
    [SerializeField] private float _pitch = 0f;
    [SerializeField] private float _voltageOutput = 0f;

    public void InitEngine() 
    {
        // Add any necessary engine initialization logic here
    }

    public void UpdateEngine(Rigidbody rigidbody, float throttle) 
    {
        // Calculate the thrust (force lifting the propeller)
        CalculateThrust(throttle);

        // Apply the thrust to the drone
        ApplyThrust(rigidbody);
        HandlePropeller(rigidbody);
    }

    private void CalculateThrust(float throttle)
    {
        float voltageInput = _batteryVoltage / 4f;
        float currentInput = _batteryCurrent / 4f;
        
        float propellerEfficiency = 0.85f;
        float motorEfficiency = 0.8f;
        float controllerEfficiency = 0.95f;
        float targetVoltageOutput = throttle * voltageInput * controllerEfficiency;
        _voltageOutput = Mathf.Lerp(_voltageOutput, targetVoltageOutput, Time.deltaTime);
        float currentPower = _voltageOutput * currentInput * controllerEfficiency;
        
        float propellerArea = Mathf.PI * Mathf.Pow(_propellerRadius, 2);
        _currentRPM = _motorKV * _voltageOutput * motorEfficiency;
        
        _angularVelocity = _currentRPM * 2f * Mathf.PI / 60f;

        // Correct torque calculation
        _torque = (currentPower / (_currentRPM / 60f)) * 60f / (2f * Mathf.PI);

        // Correct air velocity calculation
        _airVelocity = Mathf.Pow((currentPower * propellerEfficiency)/(2 * _airDensity * propellerArea), 1f / 3f);

        // Correct thrust calculation
        _thrust = 2 * _airDensity * propellerArea * Mathf.Pow(_airVelocity, 2) * propellerEfficiency;
    }

    private void ApplyThrust(Rigidbody rigidbody)
    {
        // Calculate the thrust force applied to the drone
        Vector3 force = transform.up * _thrust;

        // Apply the thrust force to the center of mass of the drone
        rigidbody.AddForceAtPosition(force, transform.position, ForceMode.Force);
    }

    public void SetClockwiseRotation(bool value)
    {
        _isClockwised = value;
    }

    public void HandlePropeller(Rigidbody rigidbody) 
    {
        if (!_propeller) 
        {
            return;
        }

        // Determine the direction of propeller rotation
        float rotationDirection = _isClockwised ? 1f : -1f; // 1 - clockwise, -1 - counter-clockwise
        _propeller.Rotate(Vector3.up, rotationDirection * _currentRPM * Time.deltaTime);
        rigidbody.AddTorque(rotationDirection * transform.up * _torque, ForceMode.Force);
    }
}
