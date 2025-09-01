using UnityEngine;

[System.Serializable]
public sealed class PropAero
{
    [Header("Geometry")]
    public float diameter_m = 0.2032f; // 8"
    public float pitch_m = 0.127f;     // 5" (необяз., информативно)

    [Header("Coefficients: Ct(J)=Ct0+Ct1*J, Cq(J)=Cq0+Cq1*J")]
    public float Ct0 = 0.10f, Ct1 = -0.05f;
    public float Cq0 = 0.040f, Cq1 = -0.020f;

    // Выходы
    public float ThrustN { get; private set; }
    public float TorqueNm { get; private set; }

    public void Compute(float rho, float omega, float Va, bool useInducedForTorque = false)
    {
        float D = Mathf.Max(0.05f, diameter_m);
        float n = omega / (2f * Mathf.PI); // об/с
        float J = (n > 1e-4f) ? Va / (n * D) : 0f;

        float Ct = Mathf.Max(0f, Ct0 + Ct1 * J);
        float Cq = Mathf.Max(0f, Cq0 + Cq1 * J);

        float n2 = n * n;
        float D4 = D * D * D * D;
        float D5 = D4 * D;

        ThrustN = Ct * rho * n2 * D4;
        float Q = Cq * rho * n2 * D5;

        if (useInducedForTorque)
        {
            // Уточнение через импульсную теорию
            float A = Mathf.PI * 0.25f * D * D;
            float disc = Va * Va + 2f * ThrustN / (rho * A);
            float vi = 0.5f * (Mathf.Sqrt(Mathf.Max(disc, 0f)) - Va);
            float P = ThrustN * Mathf.Max(0f, Va + vi);
            Q = (omega > 1e-3f) ? P / omega : 0f;
        }

        TorqueNm = Mathf.Max(0f, Q);
    }
}
