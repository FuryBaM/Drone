using UnityEngine;

[RequireComponent(typeof(DroneInputs), typeof(Rigidbody))]
public class DroneController : BaseRigidBody
{
    [Header("Environment")]
    public float airDensity = 1.225f;

    [Header("Battery")]
    public BatteryPack battery = new BatteryPack();

    [Header("Attitude limits (deg)")]
    public float maxPitchDeg = 30f;
    public float maxRollDeg = 30f;
    public float maxYawRateDeg = 120f;

    [Header("Hover throttle guess")]
    [Range(0f, 1f)] public float baseThrottle = 0.5f;

    [Header("PIDs")]
    public PID pitchAnglePID = new PID { kp = 4.0f, ki = 0.0f, kd = 0.8f };
    public PID rollAnglePID = new PID { kp = 4.0f, ki = 0.0f, kd = 0.8f };
    public PID pitchRatePID = new PID { kp = 0.2f, ki = 0.0f, kd = 0.01f };
    public PID rollRatePID = new PID { kp = 0.2f, ki = 0.0f, kd = 0.01f };
    public PID yawRatePID = new PID { kp = 0.15f, ki = 0.02f, kd = 0.01f };

    [Header("Mix weights")]
    public float kPR = 0.25f;
    public float kY = 0.15f;

    [Header("Engines (X)")]
    public DroneEngine fl; // Front-Left  (x-, z+)
    public DroneEngine fr; // Front-Right (x+, z+)
    public DroneEngine bl; // Back-Left   (x-, z-)
    public DroneEngine br; // Back-Right  (x+, z-)

    DroneInputs _in;

    protected override void Awake()
    {
        base.Awake();
        _in = GetComponent<DroneInputs>();
        if (!fl || !fr || !bl || !br) Debug.LogError("Assign 4 engines.");
    }

    protected override void HandlePhysics()
    {
        float dt = Time.fixedDeltaTime;

        // текущие углы и локальные угл. скорости (град/с)
        var eul = rb.rotation.eulerAngles;
        float rollDeg = Mathf.DeltaAngle(0f, eul.z);
        float pitchDeg = Mathf.DeltaAngle(0f, eul.x);
        Vector3 wLoc = transform.InverseTransformDirection(rb.angularVelocity) * Mathf.Rad2Deg;
        float rollRate = wLoc.z;
        float pitchRate = wLoc.x;
        float yawRate = wLoc.y;

        // команды
        float cmdPitchDeg = Mathf.Clamp(_in.Cyclic.y * maxPitchDeg, -maxPitchDeg, maxPitchDeg);
        float cmdRollDeg = Mathf.Clamp(-_in.Cyclic.x * maxRollDeg, -maxRollDeg, maxRollDeg);
        float cmdYawRate = Mathf.Clamp(_in.Pedals * maxYawRateDeg, -maxYawRateDeg, maxYawRateDeg);
        float throttle = Mathf.Clamp01(baseThrottle + Mathf.Clamp(_in.Throttle, -1f, 1f));

        // угол -> скорость
        float pitchRateCmd = pitchAnglePID.Update(cmdPitchDeg - pitchDeg, dt);
        float rollRateCmd = rollAnglePID.Update(cmdRollDeg - rollDeg, dt);

        // скорость -> момент (нормированные управляющие)
        float uPitch = Mathf.Clamp(pitchRatePID.Update(pitchRateCmd - pitchRate, dt), -1f, 1f);
        float uRoll = Mathf.Clamp(rollRatePID.Update(rollRateCmd - rollRate, dt), -1f, 1f);
        float uYaw = Mathf.Clamp(yawRatePID.Update(cmdYawRate - yawRate, dt), -1f, 1f);

        // миксер
        float flCmd = throttle - kPR * uPitch - kPR * uRoll + kY * (fl.Clockwise ? -uYaw : +uYaw);
        float frCmd = throttle - kPR * uPitch + kPR * uRoll + kY * (fr.Clockwise ? -uYaw : +uYaw);
        float blCmd = throttle + kPR * uPitch - kPR * uRoll + kY * (bl.Clockwise ? -uYaw : +uYaw);
        float brCmd = throttle + kPR * uPitch + kPR * uRoll + kY * (br.Clockwise ? -uYaw : +uYaw);

        // нормализация по максимуму, затем кламп 0..1
        float maxCmd = Mathf.Max(flCmd, frCmd, blCmd, brCmd, 1f);
        if (maxCmd > 1f) { float inv = 1f / maxCmd; flCmd *= inv; frCmd *= inv; blCmd *= inv; brCmd *= inv; }
        flCmd = Mathf.Clamp01(flCmd); frCmd = Mathf.Clamp01(frCmd); blCmd = Mathf.Clamp01(blCmd); brCmd = Mathf.Clamp01(brCmd);

        // первый проход: считаем токи моторов при текущем Vbat
        float Vbat_now = Mathf.Max(0.1f, battery.Vbat == 0f ? battery.ocvPerCell.Evaluate(battery.SOC) * battery.cells : battery.Vbat);
        float I1 = fl.StepEngine(rb, flCmd, Vbat_now, airDensity);
        float I2 = fr.StepEngine(rb, frCmd, Vbat_now, airDensity);
        float I3 = bl.StepEngine(rb, blCmd, Vbat_now, airDensity);
        float I4 = br.StepEngine(rb, brCmd, Vbat_now, airDensity);

        // обновить батарею (просадка и SOC)
        float Itot = I1 + I2 + I3 + I4;
        battery.Step(Itot, dt);
    }
}
