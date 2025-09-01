using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DroneEngine : MonoBehaviour, IEngine
{
    [Header("ESC")]
    [Range(0.5f, 1f)] public float escVoltageEff = 0.98f; // ������
    public float slewPerSec = 50f;                       // ����������� duty*Vbat

    [Header("Motor")]
    public BLDCMotorPhys motor = new BLDCMotorPhys();

    [Header("Prop")]
    public PropAero prop = new PropAero();
    public bool clockwise = true; // CW ��� ������������� yaw �� ������

    [Header("Visual")]
    public Transform propeller;

    // debug
    [SerializeField] float Vin_eff, Va, Pin, Pout, eta;

    public bool Clockwise => clockwise;
    public void SetClockwiseRotation(bool cw) => clockwise = cw;

    public float StepEngine(Rigidbody rb, float duty01, float vbat, float rho)
    {
        float dt = Time.fixedDeltaTime;

        // ESC вход
        float targetVin = Mathf.Clamp01(duty01) * Mathf.Max(0f, vbat) * Mathf.Clamp01(escVoltageEff);
        Vin_eff = Mathf.Lerp(Vin_eff, targetVin, 1f - Mathf.Exp(-Mathf.Max(1f, slewPerSec) * dt));

        // скорость набегающего потока
        Va = -Vector3.Dot(rb.linearVelocity, transform.up); // ВАЖНО: velocity, не linearVelocity

        // аэронагрузка на текущей ω
        prop.Compute(rho, motor.omega, Va, useInducedForTorque: false);
        float Qload = Mathf.Max(0f, prop.TorqueNm);

        // шаг мотора
        motor.Step(dt, Vin_eff, Qload, out var tau, out var E, out Pin, out Pout, out eta);

        // пересчёт T,Q после обновлённой ω
        prop.Compute(rho, motor.omega, Va, useInducedForTorque: false);
        float T = prop.ThrustN;
        float Q = prop.TorqueNm;

        // санитарные клампы
        const float T_MAX = 300f;   // под себя
        const float Q_MAX = 10f;
        if (float.IsNaN(T) || float.IsInfinity(T)) T = 0f;
        if (float.IsNaN(Q) || float.IsInfinity(Q)) Q = 0f;
        T = Mathf.Clamp(T, 0f, T_MAX);
        Q = Mathf.Clamp(Q, 0f, Q_MAX);

        // силы/моменты
        rb.AddForceAtPosition(transform.up * T, transform.position, ForceMode.Force);
        float yawSign = clockwise ? -1f : 1f;
        rb.AddTorque(yawSign * Q * transform.up, ForceMode.Force);

        // визуал
        if (propeller)
            propeller.Rotate(Vector3.up, (clockwise ? 1f : -1f) * motor.omega * Mathf.Rad2Deg * dt);

        // ток от батареи
        float Ibat = (vbat > 1e-3f) ? (Pin / vbat) : 0f;
        return Mathf.Max(0f, Ibat);
    }
}
