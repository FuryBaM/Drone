using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(DroneInputs))]
public class DroneController : BaseRigidBody 
{
    [Header("Control Properties")] 
    [SerializeField] private float _minMaxPitch = 30f;
    [SerializeField] private float _minMaxRoll = 30f;

    [SerializeField] private float _baseThrottle = 0.5f;

    private DroneInputs _input;

    [SerializeField] private DroneEngine flEngine;
    [SerializeField] private DroneEngine frEngine;
    [SerializeField] private DroneEngine blEngine;
    [SerializeField] private DroneEngine brEngine;

    private PIDController _pitchPID;
    private PIDController _rollPID;
    private PIDController _yawPID;

    private void Start() 
    {
        _input = GetComponent<DroneInputs>();
        if (_input == null)
        {
            Debug.LogError("DroneInputs component not found!");
            return;
        }

        flEngine.SetClockwiseRotation(true);
        frEngine.SetClockwiseRotation(false);
        blEngine.SetClockwiseRotation(false);
        brEngine.SetClockwiseRotation(true);

        // Initialize PID controllers for pitch, roll, and yaw with tuned parameters
        _pitchPID = new PIDController(1.3f, 0f, 1.3f);
        _rollPID = new PIDController(1.3f, 0f, 1.3f);
        _yawPID = new PIDController(1.0f, 0.1f, 0.5f); // Parameters for yaw PID can be adjusted as needed
    }
    
    protected override void HandlePhysics() 
    {
        if (_input == null) return;

        HandleEngines();
    }

    protected virtual void HandleEngines() 
    {
        float throttle = Mathf.Clamp(_baseThrottle + _input.Throttle, 0, 1);

        // Get the current angles from the drone's gyroscope
        Vector3 currentAngles = _rigidBody.rotation.eulerAngles;
        Vector3 angularVelocity = _rigidBody.angularVelocity;

        float currentPitch = (currentAngles.x > 180f ? currentAngles.x - 360f : currentAngles.x) / 360f;
        float currentRoll = (currentAngles.z > 180f ? currentAngles.z - 360f : currentAngles.z) / 360f;
        float currentYawRate = angularVelocity.y; // Use angular velocity for yaw stabilization

        // Calculate desired angles based on input
        float targetPitch = _input.Cyclic.y;
        float targetRoll = _input.Cyclic.x;
        float targetYawRate = _input.Pedals;

        // Calculate adjustments using PID controllers
        float pitchAdjustment = _pitchPID.Update(targetPitch * (_minMaxPitch / 360f) - currentPitch, Time.deltaTime);
        float rollAdjustment = _rollPID.Update(targetRoll * (_minMaxRoll / 360f) + currentRoll, Time.deltaTime);
        float yawAdjustment = _yawPID.Update(targetYawRate - currentYawRate, Time.deltaTime);

        // Clamp adjustments to prevent excessive force
        pitchAdjustment = Mathf.Clamp(pitchAdjustment, -1f, 1f);
        rollAdjustment = Mathf.Clamp(rollAdjustment, -1f, 1f);
        yawAdjustment = Mathf.Clamp(yawAdjustment, -1f, 1f);

        // Calculate force coefficients for each engine
        float flForce = throttle - pitchAdjustment + rollAdjustment + yawAdjustment;
        float frForce = throttle - pitchAdjustment - rollAdjustment - yawAdjustment;
        float blForce = throttle + pitchAdjustment + rollAdjustment - yawAdjustment;
        float brForce = throttle + pitchAdjustment - rollAdjustment + yawAdjustment;
        
        // Ensure the coefficients are in the valid range [0, 1]
        flForce = Mathf.Clamp(flForce, 0, 1);
        frForce = Mathf.Clamp(frForce, 0, 1);
        blForce = Mathf.Clamp(blForce, 0, 1);
        brForce = Mathf.Clamp(brForce, 0, 1);

        // Apply forces to each engine
        flEngine.UpdateEngine(_rigidBody, flForce);
        frEngine.UpdateEngine(_rigidBody, frForce);
        blEngine.UpdateEngine(_rigidBody, blForce);
        brEngine.UpdateEngine(_rigidBody, brForce);
    }
}

// Simple PID Controller class
public class PIDController
{
    private float _kp;
    private float _ki;
    private float _kd;
    
    private float _integral;
    private float _previousError;

    public PIDController(float kp, float ki, float kd)
    {
        _kp = kp;
        _ki = ki;
        _kd = kd;
    }

    public float Update(float error, float deltaTime)
    {
        _integral += error * deltaTime;
        float derivative = (error - _previousError) / deltaTime;
        _previousError = error;

        return _kp * error + _ki * _integral + _kd * derivative;
    }
}
