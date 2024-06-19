using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DroneEngine : MonoBehaviour, IEngine 
{
    [Header("Engine Properties")]
    [SerializeField] private float _motorKv = 2300f;
    [SerializeField] private float _motorKt = 0.00415f;
    [SerializeField] private float _friction = 0.01f;
    [SerializeField] private float _backEMFConstant = 0.150f;
    [SerializeField] private float _batteryVoltage = 11.1f;
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
    [SerializeField] private float _backEMF = 0f;

    public void InitEngine() 
    {
        // Add any necessary engine initialization logic here
    }

    public void UpdateEngine(Rigidbody rigidbody, float throttle) 
    {
        _backEMF = CalculateBackEMF(_angularVelocity);

        // Рассчитываем угловую скорость
        _angularVelocity = CalculateAngularVelocity(_voltageOutput, _backEMF, _motorKt);
        // Calculate the thrust (force lifting the propeller)
        CalculateThrust(throttle);

        // Apply the thrust to the drone
        ApplyThrust(rigidbody);
        HandlePropeller(rigidbody);
    }
    private void CalculateThrust(float throttle)
    {
        float propellerEfficiency = 0.85f;
        //voltage
        float targetVoltageOutput = throttle * _batteryVoltage;
        _voltageOutput = Mathf.Lerp(_voltageOutput, targetVoltageOutput, Time.deltaTime * 10f);
        
        float propellerArea = Mathf.PI * Mathf.Pow(_propellerRadius, 2);
        float current = (_voltageOutput - _backEMF) / 10f;
        _currentRPM = _angularVelocity * 60/(2*Mathf.PI);

        // Correct torque calculation
        _torque = _motorKt * current;
        float currentPower = _voltageOutput * current;

        // Correct air velocity calculation
        _airVelocity = Mathf.Pow((currentPower * propellerEfficiency)/(2f * _airDensity * propellerArea), 1f / 3f);

        // Correct thrust calculation
        _thrust = 2 * _airDensity * propellerArea * Mathf.Pow(_airVelocity, 2) * propellerEfficiency;
    }

    // Метод для расчета обратной ЭДС
    float CalculateBackEMF(float angularVelocity)
    {
        return Mathf.Abs(angularVelocity) * _voltageOutput / _motorKv;
    }

    // Метод для расчета угловой скорости
    float CalculateAngularVelocity(float inputVoltage, float backEMF, float loadTorque)
    {
        // Рассчитываем электрическую скорость мотора (обороты в минуту)
        float electricalSpeed = (inputVoltage - backEMF) / _batteryVoltage * _motorKv;

        // Рассчитываем механическую скорость мотора (радианы в секунду)
        float mechanicalSpeed = electricalSpeed * 2 * Mathf.PI / 60;

        // Рассчитываем ускорение мотора с учетом момента нагрузки и трения
        float acceleration = (_motorKt * (inputVoltage - backEMF) - _friction * _angularVelocity - loadTorque) / 0.1f;

        // Рассчитываем новую угловую скорость мотора
        float newAngularVelocity = _angularVelocity + acceleration * Time.deltaTime;

        return newAngularVelocity;
    }


    private void ApplyThrust(Rigidbody rigidbody)
    {
        // Calculate the thrust force applied to the drone
        Vector3 force = transform.up * _thrust;
        if (float.IsNaN(_thrust))
        {
            force = Vector3.zero;
        }
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
        if (float.IsNaN(_torque))
        {
            _torque = 0;
        }
        rigidbody.AddTorque(rotationDirection * transform.up * _torque, ForceMode.Force);
    }
}
