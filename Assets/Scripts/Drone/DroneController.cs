using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(DroneInputs))]
public class DroneController : BaseRigidBody 
{
    [Header("Control Properties")] 
    [SerializeField] private float _minMaxPitch = 30f;
    [SerializeField] private float _minMaxRoll = 30f;
    [SerializeField] private float _yawPower = 4f;
    [SerializeField] private float _lerpSpeed = 2f;

    [SerializeField] private float _baseThrottle = 0.5f;

    private DroneInputs _input;

    [SerializeField] private DroneEngine flEngine;
    [SerializeField] private DroneEngine frEngine;
    [SerializeField] private DroneEngine blEngine;
    [SerializeField] private DroneEngine brEngine;

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
    }
    
    protected override void HandlePhysics() 
    {
        if (_input == null) return;

        HandleEngines();
    }

    protected virtual void HandleEngines() 
    {
        float throttle = _baseThrottle + _input.Throttle;

        // Calculate adjustments for pitch, roll, and yaw to achieve a balanced maximum angle
        float pitchAdjustment = _input.Cyclic.y * _minMaxPitch;
        float rollAdjustment = _input.Cyclic.x * _minMaxRoll;
        float yawAdjustment = _input.Pedals * _yawPower;

        // Calculate force coefficients for each engine
        float flForce = throttle - pitchAdjustment + rollAdjustment - yawAdjustment;
        float frForce = throttle - pitchAdjustment - rollAdjustment + yawAdjustment;
        float blForce = throttle + pitchAdjustment + rollAdjustment + yawAdjustment;
        float brForce = throttle + pitchAdjustment - rollAdjustment - yawAdjustment;
        
        // Ensure the coefficients are in the valid range [0, 1]
        flForce = _input.Throttle != -1 ? Mathf.Clamp(flForce, _baseThrottle, 1) : 0;
        frForce = _input.Throttle != -1 ? Mathf.Clamp(frForce, _baseThrottle, 1) : 0;
        blForce = _input.Throttle != -1 ? Mathf.Clamp(blForce, _baseThrottle, 1) : 0;
        brForce = _input.Throttle != -1 ? Mathf.Clamp(brForce, _baseThrottle, 1) : 0;

        // Apply forces to each engine
        flEngine.UpdateEngine(_rigidBody, flForce);
        frEngine.UpdateEngine(_rigidBody, frForce);
        blEngine.UpdateEngine(_rigidBody, blForce);
        brEngine.UpdateEngine(_rigidBody, brForce);
    }
}
